using GTA;
using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CustomRadioStations.Extensions;

namespace CustomRadioStations
{
    public class NativeWheelOrganizerScript : Script
    {
        GTA.Control ControlNextWheel;
        GTA.Control ControlPrevWheel;
        NativeWheel currentWheel;

        bool Event_JUST_OPENED_OnNextOpen = true;
        bool loaded;
        int maxStationCount;
        List<string> validStationNames;

        public NativeWheelOrganizerScript()
        {
            Tick += OnTick;
            Aborted += OnAbort;
            Interval = 10;
        }

        void ControlWheelChange()
        {
            if (!WheelListIsPopulated()) return;

            if (Game.IsControlJustPressed(ControlNextWheel))
            {
                currentWheel = NativeWheel.WheelList.GetNext(currentWheel);
                UpdateWheelThisFrame();
            }
            else if (Game.IsControlJustPressed(ControlPrevWheel))
            {
                currentWheel = NativeWheel.WheelList.GetPrevious(currentWheel);
                UpdateWheelThisFrame();
            }
        }

        void DisableNativeScrollRadioControls()
        {
            Game.DisableControlThisFrame(GTA.Control.VehicleNextRadio);
            Game.DisableControlThisFrame(GTA.Control.VehiclePrevRadio);
        }

        /// <summary> Reads the config (.cfg) from the <see cref="Constants.NLOG_CFG_PATH"/> to create Native Radio wheels. <para/>
        /// configuration will include [Full] or [Favs], (untested creating more - horse)</summary>
        void GetOrganizationLists()
        {
            if (!File.Exists(Constants.NLOG_CFG_PATH)) return;

            string[] lines = File.ReadAllLines(Constants.NLOG_CFG_PATH);

            bool lastLineWasWheelName = false;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string l = line.Trim();

                if (l.Contains("[") && l.Contains("]"))
                {
                    if (lastLineWasWheelName)
                    {
                        NativeWheel.WheelList.Remove(NativeWheel.WheelList.Last());
                    }

                    var wheel = new NativeWheel(l.Substring(1, l.Length - 2));
                    NativeWheel.WheelList.Add(wheel);
                    lastLineWasWheelName = true;
                    continue;
                }

                if (WheelListIsPopulated() && validStationNames.Any(s => s.Equals(l)))
                {
                    NativeWheel.WheelList.Last().stationList.Add(l);
                    lastLineWasWheelName = false;
                }
            }

            if (WheelListIsPopulated())
                currentWheel = NativeWheel.WheelList[0];
        }

        void LogAllStations()
        {
            Logger.Init(Constants.NLOG_PATH);

            Logger.Log("Game version: " + Game.Version.ToString(), Constants.NLOG_PATH);
            if ((int)Game.Version < (int)GameVersion.v1_0_1493_1_Steam)
                Logger.Log("WARNING: Can't use the native radio wheel organizer on this game version. " +
                    "Please update to 1.0.1493.0 or higher.");

            Logger.Log("Checking all native and add-on radios...", Constants.NLOG_PATH);

            maxStationCount = RadioNativeFunctions._MAX_RADIO_STATION_INDEX();

            validStationNames = new List<string>();

            for (int i = 0; i < maxStationCount; i++)
            {
                string stationName = RadioNativeFunctions.GET_RADIO_STATION_NAME(i);
                validStationNames.Add(stationName);
                string s = "Name: " + stationName + " || Proper name: " + RadioNativeFunctions.GetRadioStationProperName(i);
                Logger.Log(s, Constants.NLOG_PATH);
            }

            Logger.Log("Please use the 'Name' name for your wheel organization lists (NativeWheels.cfg)! 'Proper name' is only for display purposes.", Constants.NLOG_PATH);
        }

        private void OnAbort(object sender, EventArgs e)
        {
            UnhideAllStations();
        }

        void OnJustClosed()
        {
            //UI.ShowSubtitle("Just Closed");
        }

        void OnJustOpened()
        {
            //UI.ShowSubtitle("Just Opened");
            UpdateWheelThisFrame();
        }

        void OnTick(object sender, EventArgs e)
        {
            if (GTA.Game.WasCheatStringJustEntered("radio_reload"))
            {
                UnhideAllStations();
                NativeWheel.WheelList = null;
                currentWheel = null;
                GetOrganizationLists();
                loaded = true;
                Wait(150);
            }

            if (RadioNativeFunctions.IsRadioHudComponentVisible())
            {
                if (!loaded && Game.Player.CanControlCharacter)
                {
                    LogAllStations();
                    GetOrganizationLists();
                    loaded = true;
                }

                ShowHelpTexts();

                ControlWheelChange();

                if (Event_JUST_OPENED_OnNextOpen)
                {
                    OnJustOpened();
                }

                DisableNativeScrollRadioControls();

                Event_JUST_OPENED_OnNextOpen = false;
            }
            else
            {
                if (!loaded) return;

                if (!Event_JUST_OPENED_OnNextOpen)
                {
                    OnJustClosed();
                    Event_JUST_OPENED_OnNextOpen = true;
                }
            }
        }

        void ShowHelpTexts()
        {
            ControlNextWheel = ControlHelper.UsingGamepad() ? GTA.Control.VehicleAccelerate : GTA.Control.WeaponWheelPrev;
            ControlPrevWheel = ControlHelper.UsingGamepad() ? GTA.Control.VehicleBrake : GTA.Control.WeaponWheelNext;

            if (!Config.DisplayHelpText) return;

            string nativeWheelText = (int)Game.Version < (int)GameVersion.v1_0_1493_1_Steam ?
                "" :
                (WheelListIsPopulated() ?
                "\n" +
                ControlHelper.InputString(ControlNextWheel) + " " +
                ControlHelper.InputString(ControlPrevWheel) +
                " : Next / Prev Wheel\n" +
                "Wheel: " + currentWheel.Name
                : "");

            GTA.UI.Screen.ShowHelpTextThisFrame(
                ControlHelper.InputString(Config.KB_Toggle, Config.GP_Toggle) +
                " : Switch to Custom Wheels" + nativeWheelText,
                false);
        }

        void UnhideAllStations()
        {
            for (int i = 0; i < maxStationCount; i++)
            {
                RadioNativeFunctions._LOCK_RADIO_STATION(RadioNativeFunctions.GET_RADIO_STATION_NAME(i), false);
            }
        }

        void UpdateWheelThisFrame()
        {
            if (!WheelListIsPopulated()) return;

            // Unhide all listed radios
            foreach (var station in currentWheel.stationList)
            {
                RadioNativeFunctions._LOCK_RADIO_STATION(station, false);
            }

            // Hide any valid station name that isn't in the current wheel station list
            foreach (var station in validStationNames)
            {
                if (!currentWheel.stationList.Any(s => s.Equals(station)))
                {
                    RadioNativeFunctions._LOCK_RADIO_STATION(station, true);
                }
            }
        }

        bool WheelListIsPopulated()
        {
            return NativeWheel.WheelList.Count > 0;
        }
    }
}
