using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CustomRadioStations.Extensions;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using Logging;

namespace CustomRadioStations
{
    public class NativeWheelOrganizerScript : BaseScript
    {
        Control ControlNextWheel;
        Control ControlPrevWheel;
        NativeWheel currentWheel;

        bool Event_JUST_OPENED_OnNextOpen = true;
        bool loaded;
        int maxStationCount;
        List<string> validStationNames;

        public NativeWheelOrganizerScript() { }

        [EventHandler(nameof(onResourceStopped))]
        void onResourceStopped(string resource)
        {
            if (resource == API.GetCurrentResourceName())
                OnAbort();
        }

        void ControlWheelChange()
        {
            if (!WheelListIsPopulated()) return;

            if (Game.IsControlJustPressed(2, ControlNextWheel))
            {
                currentWheel = NativeWheel.WheelList.GetNext(currentWheel);
                UpdateWheelThisFrame();
            }
            else if (Game.IsControlJustPressed(2, ControlPrevWheel))
            {
                currentWheel = NativeWheel.WheelList.GetPrevious(currentWheel);
                UpdateWheelThisFrame();
            }
        }

        void DisableNativeScrollRadioControls()
        {
            Game.DisableControlThisFrame(2, Control.VehicleNextRadio);
            Game.DisableControlThisFrame(2, Control.VehiclePrevRadio);
        }

        /// <summary> Reads the config (NativeWheels.cfg) from the <see cref="Constants.NLOG_CFG_PATH"/> to create Native Radio wheels. <para/>
        /// configuration will include [Full] or [Favs], (untested creating more - horse), <para/>
        /// fivem uses LoadResourceFile instead of using System.IO </summary>
        void GetOrganizationLists()
        {            
            //load the text resource NativeWheels.cfg
            var res = API.LoadResourceFile(API.GetCurrentResourceName(), Constants.NLOG_CFG_PATH);
            if(string.IsNullOrEmpty(res)) return;

            using (StringReader sr = new StringReader(res))
            {
                bool lastLineWasWheelName = false;
                string line;
                while((line = sr.ReadLine()) != null)
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
            }           

            if (WheelListIsPopulated())
                currentWheel = NativeWheel.WheelList[0];
        }

        void LogAllStations()
        {
            //Logger.Init(Constants.NLOG_PATH);

            Logger.Log("Game version: " + Game.Version.ToString());
            if ((int)Game.Version < (int)GameVersion.v1_0_463_1_Steam)
                Logger.Log("WARNING: Can't use the native radio wheel organizer on this game version. " +
                    "Please update to 1.0.1493.0 or higher.");

            Logger.Log("Checking all native and add-on radios...");

            maxStationCount = RadioNativeFunctions._MAX_RADIO_STATION_INDEX();

            validStationNames = new List<string>();

            for (int i = 0; i < maxStationCount; i++)
            {
                string stationName = RadioNativeFunctions.GET_RADIO_STATION_NAME(i);
                validStationNames.Add(stationName);
                string s = "Name: " + stationName + " || Proper name: " + RadioNativeFunctions.GetRadioStationProperName(i);
                Logger.Log(s);
            }

            Logger.Log("Please use the 'Name' name for your wheel organization lists (NativeWheels.cfg)! 'Proper name' is only for display purposes.");
        }

        private void OnAbort() => UnhideAllStations();

        void OnJustClosed()
        {
            //UI.ShowSubtitle("Just Closed");
        }

        void OnJustOpened()
        {
            //UI.ShowSubtitle("Just Opened");
            UpdateWheelThisFrame();
        }

        [Tick]
        async Task OnTickAsync()
        {
            if (Game.WasCheatStringJustEntered("radio_reload"))
            {
                UnhideAllStations();
                NativeWheel.WheelList = null;
                currentWheel = null;
                GetOrganizationLists();
                loaded = true;
                await Delay(150);
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
            ControlNextWheel = ControlHelper.UsingGamepad() ? Control.VehicleAccelerate : Control.WeaponWheelPrev;
            ControlPrevWheel = ControlHelper.UsingGamepad() ? Control.VehicleBrake : Control.WeaponWheelNext;

            if (!Config.DisplayHelpText) return;

            string nativeWheelText = (int)Game.Version < (int)GameVersion.v1_0_463_1_Steam ?
                "" :
                (WheelListIsPopulated() ?
                "\n" +
                ControlHelper.InputString(ControlNextWheel) + " " +
                ControlHelper.InputString(ControlPrevWheel) +
                " : Next / Prev Wheel\n" +
                "Wheel: " + currentWheel.Name
                : "");

            Screen.DisplayHelpTextThisFrame(
                ControlHelper.InputString(Config.KB_Toggle, (int)Config.GP_Toggle) +
                " : Switch to Custom Wheels" + nativeWheelText);
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
