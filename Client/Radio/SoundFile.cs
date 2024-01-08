using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomRadioStations.Radio;
using CustomRadioStations.Interface;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using Logging;

namespace CustomRadioStations
{
    /// <summary> Soundfile utilizing the IrrKlang library. <para/>
    /// The sound file contains a list of tracks which are generated from user .ini files in station directories</summary>
    class SoundFile : ISoundFile
    {
        private static Random random = new Random();

        /// <summary>https://www.ambiera.com/irrklang/docu/index.html#soundSources</summary>
        //public static ISoundEngine SoundEngine = new ISoundEngine();

        public ISound Sound { get; private set; }

        public ISoundSource Source;

        //public ISoundEffectControl SoundEffect;

        private string _displayName;

        /// <summary> Adds the klang sound source from a file path. The sound file is not automatically loaded into memory, only when play() is called. <para/>
        /// Larger files will make game stall (above 200mb espeically noticable) <para/>
        /// If file is an ini file a tracklist will be created from the contents
        /// </summary>
        /// <param name="filepath"></param>
        public SoundFile(string filepath)
        {
            FilePath = filepath;

            //TODO: add file source
            //Source = SoundEngine.AddSoundSourceFromFile(filepath, StreamMode.AutoDetect, false);
            //if (Source == null)
            //{
            //    Source = SoundEngine.GetSoundSource(filepath);
            //}

            //FileName = Path.GetFileNameWithoutExtension(filepath);

            //_displayName = DisplayNameFromFilename();
            HasTrackList = TracklistExists(filepath);
        }

        //public float MaximumDistance = 20f;
        //public float MinimumDistance = 1f;

        /// <summary>loading from shortcut paths, call base constructor and overrides FilePath</summary>
        /// <param name="filepath"></param>
        /// <param name="shortcutPath"></param>
        public SoundFile(string filepath, string shortcutPath) : this(filepath)
        {
            FilePath = shortcutPath;
            HasTrackList = TracklistExists(shortcutPath);
        }

        public string DisplayName
        {
            get
            {
                if (HasTrackList)
                {
                    Track t = GetCurrentTrack();
                    if (t == null) return "";
                    return t.Artist + "\n" + t.Title;
                }
                else
                {
                    return _displayName;
                }
            }
        }

        public string FileName { get; }

        public string FilePath { get; }

        public bool HasTrackList { get; set; }

        public bool IsPaused
        {
            get
            {
                if (Sound == null) return false;
                return Sound.Paused;
            }
            set
            {
                if (Sound == null || Sound.Finished) return;
                Sound.Paused = value;
            }
        }
        
        public uint Length { get; set; } = 0;
        
        public bool LengthAdded { get; set; }

        public List<Track> Tracklist { get; private set; }
        public static float SoundVolume { get; set; }

        /// <summary> cleans up sound engine and any playing sounds </summary>
        public static void DisposeSoundEngine()
        {
            //SoundEngine.StopAllSounds();
            //SoundEngine.Dispose();

            NuiRadioService.Stop();
        }

        /// <summary> Updates the SoundEngine called every tick</summary>
        public static void ManageSoundEngine()
        {
            //SoundEngine.SetListenerPosition(new Vector3D(0, 0, 0), new Vector3D(0, 0, 1), new Vector3D(0, 0, 0), new Vector3D(0, 1, 0));
            //SoundEngine.Update(); //don't need to call this, update tick on irrklang
        }

        public static void StepVolume(float step, int decimals)
        {
            float temp = (float)Math.Round(SoundVolume + step, decimals);
            SoundVolume = temp.LimitToRange(0f, 1f);
            NuiRadioService.SetVolume(SoundVolume);
        }
        
        public Track GetCurrentTrack()
        {
            if (!HasTrackList) return null;

            //Track trk = Tracklist.FirstOrDefault(t => t.StartTime <= PlayPosition());
            Track trk = Tracklist.LastOrDefault(t => Sound.PlayPosition >= t.StartTime);

            if (trk == default(Track))
            {
                return null;
            }
            else
            {
                return trk;
            }
        }

        public int GetCurrentTrackIndex()
        {
            if (!HasTrackList) return 0;

            return Tracklist.IndexOf(GetCurrentTrack());
        }

        public Track GetNextTrack()
        {
            if (!HasTrackList) return null;

            Track t = GetCurrentTrack();

            if (Tracklist.Last() == t)
            {
                return Tracklist.First();
            }
            else
            {
                return Tracklist[Tracklist.IndexOf(t) + 1];
            }
        }

        public uint GetRandomPlayPosition(float percentMinBound = 0.2f, float percentMaxBound = 0.7f)
        {
            if (Sound == null) return 0;
            uint min = (uint)(percentMinBound * Sound.PlayLength);
            uint max = (uint)(percentMaxBound * Sound.PlayLength);

            // Get random uint within bounds
            var buffer = new byte[sizeof(uint)];
            new Random().NextBytes(buffer);
            uint result = BitConverter.ToUInt32(buffer, 0);

            result = (result % (max - min)) + min;
            return result;
        }
        
        /// <summary> TODO: set this flag to finished</summary>
        /// <returns></returns>
        public bool IsFinishedPlaying() => Sound.Finished;

        public bool IsPlaying() => Sound != null && !Sound.Finished;

        public void PlaySound(/*Vector3 sourcePosition,*/bool resume, bool playLooped = false, bool playPaused = false, bool allowMultipleInstances = false, bool allowSoundEffects = false)
        {
            Debug.WriteLine($"PlaySound: " + FilePath);

            if (allowMultipleInstances || (!allowMultipleInstances && (Sound == null || Sound != null && (Sound?.Finished ?? false || IsPaused))))
            {
                // Vector3D sourcePos = SoundHelperIK.Vector3ToVector3D(GameplayCamera.GetOffsetFromWorldCoords(sourcePosition));

                if (resume && IsPaused)
                {
                    IsPaused = false;
                    return;
                }

                // Sound = SoundEngine.Play3D(Source, sourcePos.X, sourcePos.Y, sourcePos.Z, playLooped, false, false);
                //Sound = SoundEngine.Play2D(Source, playLooped, true, allowSoundEffects);
                //if (Sound == null) return;

                //TODO: populate this sound with necessary properties like length
                Sound = new Sound();
                Sound.Volume = 0f;

                NuiRadioService.PlayFile(FilePath, 0);

                //TODO:               
                if(Sound != null)
                {
                    if (!playPaused)
                        Sound.Paused = false;

                    //TODO;
                    //Sound.Volume = Source.DefaultVolume;

                    if (Length == 0)
                    {
                        //Length = Source.PlayLength;
                        //TODO: length of track after loaded
                        Length = Sound.PlayLength;
                    }
                }                                

                //TODO sound effects
                //if (allowSoundEffects)
                //{
                //    SoundEffect = Sound.SoundEffectControl;
                //}
            }
            else if(Sound != null)
            {                
                if(FilePath.StartsWith("http"))
                    NuiRadioService.PlayFile(FilePath, 0);
                else
                    NuiRadioService.PlayFile(FilePath, Sound.PlayPosition);
            }
            else
            {
                Debug.WriteLine($"not playing sound : " + FilePath);
            }
        }

        public void SkipToNextTrack()
        {
            if (!HasTrackList) return;

            Sound.PlayPosition = GetNextTrack().StartTime; //irrklang

            Logger.Log("seek tracklist play position: " + Sound.PlayPosition);

            //NuiRadioService.PlayFile(FilePath, Sound.PlayPosition);
            NuiRadioService.Seek(Sound.PlayPosition);
        }

        public void StopSound()
        {
            Sound?.Stop();
            //if (Sound?.Finished ?? false) return;            
        }

        public uint TimeUntilNextTrack()
        {
            if (Sound == null || Sound.PlayPosition  == 0) return 0;
            if (!HasTrackList) return Length - Sound.PlayPosition;

            Track t = GetNextTrack();
            uint pPos = Sound.PlayPosition;

            return t.StartTime > pPos ? t.StartTime - pPos + 1 : Length - pPos;
        }

        /// <summary> Loads cfx resource ini for a stations tracklist </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool TracklistExists(string filepath)
        {
            //remove the extension without sys.io.PATH
            int dot = filepath.LastIndexOf('.');            
            if (dot > 0 && dot < filepath.Length - 1)
            {
                var ext = filepath.Substring(dot + 1);
                var iniFile = filepath.Replace(ext, "ini");
                Logger.Log("looking for tracklist.." + iniFile);

                var res = API.LoadResourceFile(API.GetCurrentResourceName(), iniFile);
                if (string.IsNullOrEmpty(res)) return false;

                Tracklist = new List<Track>();

                using (var sr = new StringReader(res))
                {
                    var line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                            CreateTracklist(line);
                    }
                }

                Logger.Log("tracklist created");
                return true;
            }

            return false;
        }

        /// <summary> Creates a tracklist from the contents of a station.ini <para/>
        /// Lines must start with time 00:00:00 with artist and title delimited with ||</summary>
        /// <param name="inputFromINI"></param>
        private void CreateTracklist(string inputFromINI)
        {
            if (uint.TryParse(inputFromINI.Substring(0, 2), out uint h)
                && uint.TryParse(inputFromINI.Substring(3, 2), out uint m)
                && uint.TryParse(inputFromINI.Substring(6, 2), out uint s))
            {
                // Convert hours:minutes:seconds to milliseconds
                uint startTime = (h * 60 * 60 * 1000)
                    + (m * 60 * 1000)
                    + (s * 1000);

                // Skip all entries that have a timestamp past the length of the entire file
                //if (startTime > Length) return; // Gonna let this slide for now, working on getting length only when sound is loaded...

                if (inputFromINI.Length == 8)
                {
                    Tracklist.Add(new Track(startTime, "", ""));
                    return;
                }

                // Get remainder of string
                string artistTitle = inputFromINI.Substring(8);

                if (artistTitle.Contains("||"))
                {
                    // Separate it by the string ||
                    string[] splitTexts = artistTitle.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                    // Set Artist and Title with beginning and ending whitespaces removed
                    var artist = splitTexts[0].Trim();
                    var title = splitTexts[1].Trim();

                    Tracklist.Add(new Track(startTime, artist, title));
                }
                else
                {
                    Tracklist.Add(new Track(startTime, "", artistTitle));
                }
            }
        }

        private string DisplayNameFromFilename()
        {
            string[] sections = FileName.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            string str = "";
            foreach (var s in sections)
            {
                str += s.Trim() + "\n";
            }
            return str;
        }

        public void Dispose() { }

        public void SetPlayPosition(int mediaPos)
        {
            this.Sound.PlayPosition = (uint)mediaPos;
        }

        // 3D Sound stuff only
        /*public void ProcessSound(Vector3 sourcePosition)
        {
            if (Sound != null && !Sound.Finished)
            {
                Sound.MaxDistance = MaximumDistance;
                Sound.MinDistance = MinimumDistance;
                Vector3D sourcePos = SoundHelperIK.Vector3ToVector3D(GameplayCamera.GetOffsetFromWorldCoords(sourcePosition));
                Sound.Position = sourcePos;
            }
        }

        public void SetDistances(float max, float min)
        {
            MaximumDistance = max;
            MinimumDistance = min;
        }*/
    }
}
