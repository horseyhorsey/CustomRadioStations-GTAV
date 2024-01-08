using CustomRadioStations.Radio;
using Logging;

namespace CustomRadioStations.Interface
{
    interface ISound
    {
        bool Finished { get; set; }

        uint PlayLength { get; set; }

        /// <summary>in milliseconds, returns -1 if not playing</summary>
        uint PlayPosition { get; set; }

        bool Paused { get; set; }

        void Stop();

        /// <summary>0.0 - 1.0</summary>
        float Volume { get; set; }
    }

    public class Sound : ISound
    {
        public bool Finished { get; set; }

        public uint PlayLength { get; set; }

        public uint PlayPosition { get; set; }
        public bool Paused { get; set; }
        public float Volume { get; set; }

        public void Stop()
        {
            Logger.Log("Sound stop");
            NuiRadioService.Stop();
        }
    }
}
