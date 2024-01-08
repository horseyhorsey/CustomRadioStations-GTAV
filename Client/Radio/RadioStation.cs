using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SelectorWheel;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using Logging;

namespace CustomRadioStations
{
    class RadioStation
    {
        public static RadioStation CurrentPlaying;
        public static RadioStation NextQueuedStation;
        public static Random random = new Random();

        bool allSoundsPlayedOnce;
        WheelCategory corrWheelCat;
        public static ISoundFile CurrentSoundFile;
        bool hasPlayedOnce;
        int lastCommIndex = 0;
        int lastPlayedSoundIndex;
        DateTime lastPlayedTime;
        List<SoundFileTimePair> SoundFileTimePairs;
        uint stoppedPositionSound;
        uint stoppedPositionStation;
        DateTime trackUpdateTimer = DateTime.Now;

        /// <summary> Goes through every file and will either add a commercial or a SoundFileTimePair <para/>
        /// files can be shortcut .lnk and files can be prefixed with [Commercial]</summary>
        /// <param name="correspondingWheelCategory"></param>
        /// <param name="songFilesPaths">file paths</param>
        public RadioStation(WheelCategory correspondingWheelCategory, IEnumerable<string> songFilesPaths, string baseurl = null)
        {
            corrWheelCat = correspondingWheelCategory;
            Name = corrWheelCat.Name;
            SoundFileTimePairs = new List<SoundFileTimePair>();
            List<Tuple<string, string>> commercials = new List<Tuple<string, string>>();

            foreach (var path in songFilesPaths)
            {
                try
                {
                    //Logger.Log(path.Substring(path.LastIndexOf('\\') + 1));
                    // If file is a shortcut, get the real path first
                    //Shortcut files unused
                    //if (Path.GetExtension(path) == ".lnk")
                    //{
                    //    string str = GeneralHelper.GetShortcutTargetFile(path);

                    //    if (str == string.Empty) continue;

                    //    if (Path.GetFileNameWithoutExtension(str).Contains("[Commercial]")
                    //        || Path.GetFileNameWithoutExtension(path).Contains("[Commercial]"))
                    //    {
                    //        commercials.Add(Tuple.Create(str, path));
                    //    }
                    //    else
                    //    {
                    //        SoundFileTimePairs.Add(new SoundFileTimePair(new SoundFile(str, path), 0));
                    //    }
                    //}

                    var fullPath = path != null ? baseurl + path : path;                    

                    if (Path.GetFileNameWithoutExtension(fullPath).Contains("[Commercial]"))
                    {
                        commercials.Add(Tuple.Create(path, string.Empty));
                    }
                    else
                    {
                        Logger.Log("adding time pair:" + fullPath);
                        SoundFileTimePairs.Add(new SoundFileTimePair(new SoundFile(fullPath), 0));
                        Logger.Log("time pair added");
                    }

                    Config.LoadTickAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log("ERROR : " + path.Substring(path.LastIndexOf('\\') + 1) + " : " + ex.Message);
                    BaseScript.Delay(500);
                }
            }

            ShuffleList(); // Do this based on an ini setting? Yes. TODO

            InsertCommercials(commercials);

            // Calculate lengths and stuff for the station
            // Replaced by UpdateRadioLengthWithCurrentSound()
            //foreach (var s in SoundFileTimePairs)
            //{
            //    s.StartTime = TotalLength;
            //    TotalLength += s.SoundFile.Length;
            //}
        }

        public bool CurrentSoundIsPaused
        {
            get
            {
                return CurrentSoundFile.IsPaused;
            }
            set
            {
                CurrentSoundFile.IsPaused = value;
            }
        }

        public bool IsPlaying
        {
            get { return CurrentSoundFile != null && CurrentSoundFile.IsPlaying(); }
        }

        public string Name { get; set; }

        /// <summary>
        /// In milliseconds
        /// </summary>
        public uint TotalLength { get; private set; } = 0;

        public static void ManageStations()
        {
            if (CurrentPlaying == null) return;

            CurrentPlaying.Update();
        }

        public void Play()
        {
            if (!hasPlayedOnce)
            {
                try
                {
                    if (CurrentSoundFile?.Sound != null || SoundFileTimePairs != null)
                    {
                        CurrentSoundFile = SoundFileTimePairs[0].SoundFile;
                        CurrentSoundFile.PlaySound(false, false, true);
                        hasPlayedOnce = true;

                        //TODO: this needs to be done after sound is loaded
                        //UpdateRadioLengthWithCurrentSound();       
                        
                        UpdateWheelInfo();
                    }
                    else
                    {
                        Logger.Log("WARNING: No sound set");
                    }                    
                }
                catch (Exception ex)
                {
                    Logger.Log("ERROR:" + ex.ToString());
                }
            }
            else
            {
                if (!allSoundsPlayedOnce &&
                    lastPlayedSoundIndex == SoundFileTimePairs.Count - 1)
                {
                    allSoundsPlayedOnce = true;
                }

                ResumeContinuity();
            }

            Function.Call(Hash.SET_AUDIO_FLAG, "DisableFlightMusic", true);
            Function.Call(Hash.SET_AUDIO_FLAG, "DisableWantedMusic", true);
        }

        /// <summary>Plays next song, tracklist or standard</summary>
        public void PlayNextSong()
        {
            if (CurrentSoundFile != null)
            {
                // If CurrentSound has a tracklist but isn't at the last song, skip to the next song in the tracklist.
                if (CurrentSoundFile.HasTrackList && CurrentSoundFile.GetCurrentTrackIndex() < CurrentSoundFile.Tracklist.Count - 1)
                {
                    Logger.Log(nameof(PlayNextSong) + ": skip to next track");
                    CurrentSoundFile.SkipToNextTrack();

                    UpdateWheelInfo();
                    UpdateTrackUpdateTimer();
                }
                // Else, skip to the next SoundFile.
                else
                {
                    PlayNextSound();
                }
            }
        }

        public void Stop()
        {
            if (CurrentSoundFile == null || CurrentSoundFile.Sound == null) return;

            Logger.Log("stopping radio. play pos: " + CurrentSoundFile.Sound.PlayPosition);

            // Get stopped position
            var stPair = SoundFileTimePairs.Find(s => s.SoundFile == CurrentSoundFile);
            stoppedPositionStation = stPair.StartTime + CurrentSoundFile.Sound.PlayPosition;

            lastPlayedTime = DateTime.Now;
            lastPlayedSoundIndex = SoundFileTimePairs.IndexOf(stPair);
            stoppedPositionSound = CurrentSoundFile.Sound.PlayPosition;

            // Set name in wheel to just the station name
            WheelCategoryItem radioWheelItem = corrWheelCat.ItemList[0];
            radioWheelItem.Name = Name;

            CurrentSoundFile.StopSound();
            CurrentSoundIsPaused = true;
            CurrentSoundFile = null;

            Function.Call(Hash.SET_AUDIO_FLAG, "DisableFlightMusic", false);
            Function.Call(Hash.SET_AUDIO_FLAG, "DisableWantedMusic", false);
        }

        public void Update()
        {
            if (CurrentSoundFile == null || CurrentSoundFile.Sound == null) return;

            //UI.ShowSubtitle((lastPlayedSoundIndex + 1) + " / " + SoundFileTimePairs.Count);

            if (CurrentSoundFile.HasTrackList && trackUpdateTimer < DateTime.Now)
            {
                UpdateWheelInfo();
                UpdateTrackUpdateTimer();
            }

            if (CurrentSoundFile.IsFinishedPlaying())
            {
                PlayNextSound();
            }
        }

        /// <summary>Updates the radio scaleform form with name artist title. <para/>
        /// See <see cref="RadioNativeFunctions.UpdateRadioScaleform(string, string, string)"/> </summary>
        public void UpdateDashboardInfo()
        {
            if (CurrentSoundFile?.Sound == null) return;

            if (Game.Player.Character.IsInVehicle() && !string.IsNullOrWhiteSpace(CurrentSoundFile.DisplayName))
            {
                string[] info = CurrentSoundFile.DisplayName.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                RadioNativeFunctions.UpdateRadioScaleform(Name, info[0], info[1]);
            }
        }

        internal void RescanSoundsTracklists()
        {
            foreach (var pair in SoundFileTimePairs)
            {
                var soundFile = pair.SoundFile;
                
                soundFile.HasTrackList = soundFile.TracklistExists(soundFile.FilePath);
            }
            UpdateWheelInfo();
            UpdateTrackUpdateTimer();
        }

        internal void SetPosition(int mediaPos)
        {
            CurrentSoundFile.SetPlayPosition(mediaPos);
        }

        private int GetNewRandom(int input, int max)
        {
            int rnd = random.Next(0, max);
            if (input == rnd && max != 1)
            {
                return GetNewRandom(input, max);
            }
            else
            {
                return rnd;
            }

        }

        private uint GetTimeFromPrevious(uint previous, uint duration, uint elapsed)
        {            
            Logger.Log(nameof(GetTimeFromPrevious) + $": pr:{previous}, dur: {duration}, elaps: {elapsed}");
            if (duration == 0) return 0;

            uint adjElapsed = elapsed - (duration - previous);
            uint time = (previous + elapsed) > duration ? adjElapsed % duration : previous + elapsed;
            return time == duration ? 0 : time;
        }

        /// <summary> Inserts into the SoundFileTimePairs</summary>
        /// <param name="commercials"></param>
        private void InsertCommercials(List<Tuple<string, string>> commercials)
        {
            if (commercials.Count > 0)
            {
                int numToInsert = SoundFileTimePairs.Count / 3;
                int lastIndex = -1;
                SoundFileTimePairs.Capacity += numToInsert;

                for (int i = 0; i < numToInsert; i++)
                {
                    lastIndex += 4;

                    lastCommIndex = GetNewRandom(lastCommIndex, commercials.Count);
                    var commercial = commercials[lastCommIndex];
                    var pair = new SoundFileTimePair(
                            string.IsNullOrEmpty(commercial.Item2) ? new SoundFile(commercial.Item1) :
                            new SoundFile(commercial.Item1, commercial.Item2), 0);

                    if (lastIndex >= SoundFileTimePairs.Count)
                    {
                        SoundFileTimePairs.Add(pair);
                    }
                    else
                    {
                        SoundFileTimePairs.Insert(lastIndex, pair);
                    }
                }
            }
        }

        private void PlayNextSound()
        {
            int currentSoundIndex = 0;
            if (CurrentSoundFile != null)
            {
                currentSoundIndex = SoundFileTimePairs.IndexOf(SoundFileTimePairs.Find(s => s.SoundFile == CurrentSoundFile));
                CurrentSoundFile.StopSound();
                Logger.Log("sound stopped:" + CurrentSoundFile.FilePath);
            }            

            // Set next in list
            currentSoundIndex = currentSoundIndex < SoundFileTimePairs.Count - 1 ? currentSoundIndex + 1 : 0;

            CurrentSoundFile = SoundFileTimePairs[currentSoundIndex].SoundFile;

            Logger.Log(nameof(PlayNextSound) + ": file: " + CurrentSoundFile.FilePath);

            CurrentSoundFile.PlaySound(true);

            UpdateRadioLengthWithCurrentSound();
            UpdateWheelInfo();
            UpdateTrackUpdateTimer();
        }

        private void ResumeContinuity()
        {
            uint elapsed = (uint)(DateTime.Now - lastPlayedTime).TotalMilliseconds;

            var lastPlayedSound = SoundFileTimePairs[lastPlayedSoundIndex].SoundFile;

            Logger.Log(nameof(ResumeContinuity));
            //UI.ShowSubtitle("allSoundsPlayed: " + allSoundsPlayedOnce +
            //    "\nelapsed ms: " + elapsed +
            //    "\nRemaining playtime: " + (lastPlayedSound.Length - stoppedPositionSound));

            if (allSoundsPlayedOnce)
            {
                Logger.Log("all sounds have been played once..");

                uint newPlayPos = GetTimeFromPrevious(stoppedPositionStation, TotalLength, elapsed);
                var stPair = SoundFileTimePairs.LastOrDefault(s => newPlayPos >= s.StartTime);
                CurrentSoundFile = stPair != default(SoundFileTimePair) ? stPair.SoundFile : SoundFileTimePairs[0].SoundFile;

                CurrentSoundFile.Sound.PlayPosition = Math.Max(0, newPlayPos - stPair.StartTime);
                CurrentSoundIsPaused = false;

                CurrentSoundFile.PlaySound(true, false, true);

                //UpdateRadioLengthWithCurrentSound();                
            }
            else if (elapsed < lastPlayedSound.Length - stoppedPositionSound)
            {
                CurrentSoundFile = SoundFileTimePairs[lastPlayedSoundIndex].SoundFile;

                if(CurrentSoundFile.Sound != null)
                {
                    CurrentSoundFile.Sound.PlayPosition = Math.Min(CurrentSoundFile.Length - 1, stoppedPositionSound + elapsed);                    
                }                

                CurrentSoundFile.PlaySound(true, false, true);
                
                CurrentSoundIsPaused = false;
                UpdateRadioLengthWithCurrentSound();
            }
            else
            {
                CurrentSoundFile = lastPlayedSoundIndex != SoundFileTimePairs.Count - 1 ?
                    SoundFileTimePairs[lastPlayedSoundIndex + 1].SoundFile : SoundFileTimePairs[0].SoundFile;
                CurrentSoundFile.PlaySound(true);
                UpdateRadioLengthWithCurrentSound();
            }

            UpdateWheelInfo();
            if (CurrentSoundFile.HasTrackList)
                UpdateTrackUpdateTimer();
        }

        private void ShuffleList()
        {
            /*int n = SoundFileTimePairs.Count;

            for (int i = SoundFileTimePairs.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                SoundFileTimePair value = SoundFileTimePairs[rnd];
                SoundFileTimePairs[rnd] = SoundFileTimePairs[i];
                SoundFileTimePairs[i] = value;
            }*/

            var count = SoundFileTimePairs.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                var tmp = SoundFileTimePairs[i];
                SoundFileTimePairs[i] = SoundFileTimePairs[r];
                SoundFileTimePairs[r] = tmp;
            }
        }

        /// <summary>
        /// Must be called after CurrentSound.PlaySound();
        /// </summary>
        private void UpdateRadioLengthWithCurrentSound()
        {
            var s = SoundFileTimePairs.Find(x => x.SoundFile == CurrentSoundFile);

            Logger.Log(nameof(UpdateRadioLengthWithCurrentSound) + ": sound file length = " + s.SoundFile.Length);

            if (!CurrentSoundFile?.LengthAdded ?? false)
            {
                s.StartTime = TotalLength;
                TotalLength += CurrentSoundFile.Length;
                CurrentSoundFile.LengthAdded = true;
            }
            else
            {
                Logger.Log("sound already has duration of " + CurrentSoundFile.Length + " ,total length:" + TotalLength);
            }
        }

        private void UpdateTrackUpdateTimer()
        {
            trackUpdateTimer = DateTime.Now.AddMilliseconds(CurrentSoundFile == null || CurrentSoundFile.Sound == null ? 5000 : CurrentSoundFile.TimeUntilNextTrack());
        }

        /// <summary>
        /// Can be called on tick
        /// </summary>
        private void UpdateWheelInfo()
        {
            if (CurrentSoundFile == null || CurrentSoundFile.Sound == null) return;

            WheelCategoryItem radioWheelItem = corrWheelCat.ItemList[0];

            radioWheelItem.Name = Name + "\n" + CurrentSoundFile.DisplayName;
        }

        internal void SetLength(uint secs)
        {
            Logger.Log("setting sound play length:" + secs);
            CurrentSoundFile.Sound.PlayLength = secs;
            CurrentSoundFile.Length = secs;
            UpdateRadioLengthWithCurrentSound();
        }
    }
}
