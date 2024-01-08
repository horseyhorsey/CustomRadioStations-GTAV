using CustomRadioStations.Interface;
using System.Collections.Generic;

namespace CustomRadioStations
{
    internal interface ISoundFile
    {
        ISound Sound { get; }

        /// <summary> If a tracklist is avaliable this sets
        /// the display name to the current track with artist \n title like the games radio </summary>
        string DisplayName { get; }

        string FilePath { get; }

        bool HasTrackList { get; set; }

        bool IsPaused { get; set; }

        /// <summary> Returns sound length in milliseconds</summary>
        uint Length { get; set; }

        /// <summary> If Length has been added to this sound's corresponding station</summary>
        bool LengthAdded { get; set; }

        /// <summary> Songs by start time, artist, title </summary>
        List<Track> Tracklist { get; }

        void Dispose();

        /// <summary> Gets the current track if a <see cref="Tracklist"/> is available </summary>
        Track GetCurrentTrack();

        int GetCurrentTrackIndex();

        Track GetNextTrack();

        uint GetRandomPlayPosition(float percentMinBound = 0.2F, float percentMaxBound = 0.7F);

        /// <returns>Sound.Finished</returns>
        bool IsFinishedPlaying();

        bool IsPlaying();

        /// <summary>Returns -1 if null, not playing, etc. Else returns position in milliseconds.</summary>
        /// <returns></returns>
        void SetPlayPosition(int mediaPos);

        void PlaySound(bool resume, bool playLooped = false, bool playPaused = false, bool allowMultipleInstances = false, bool allowSoundEffects = false);

        /// <summary>Sets the position of playing track to the next tracks start time if tracklist available</summary>
        void SkipToNextTrack();

        /// <summary>Sound.Stop if not already stopped</summary>
        void StopSound();

        /// <returns>If a tracklist is available otherwise 0</returns>
        uint TimeUntilNextTrack();

        bool TracklistExists(string filepath);
    }
}