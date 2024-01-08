using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace CustomRadioStations.Radio
{
    public static class NuiRadioService
    {
        public static void PlayFile(string file, uint pos) =>
            API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "play_file", file = file, pos = pos }));

        /// <summary> Send "seek" and time to seek to </summary>
        /// <param name="secs"></param>
        public static void Seek(uint secs) => API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "seek", time = secs }));

        public static void Stop() => API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "stop" }));

        internal static void SetVolume(float soundVolume) => API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "vol", vol = soundVolume }));
    }
}
