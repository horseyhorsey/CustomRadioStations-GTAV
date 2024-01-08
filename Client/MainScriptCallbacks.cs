using CitizenFX.Core;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomRadioStations
{
    public partial class MainScript : BaseScript
    {
        [EventHandler("__cfx_nui:setCurPos")]
        void setCurPos(IDictionary<string, object> args, CallbackDelegate cb)
        {
            if (args?.Any() ?? false && args.ContainsKey("pos"))
            {
                int.TryParse(args["pos"].ToString(), out var mediaPos);
                RadioStation.CurrentPlaying.SetPosition(mediaPos * 1000);
            }

            cb(true);
        }

        [EventHandler("__cfx_nui:setSongLoaded")]
        void setSongLoaded(IDictionary<string, object> args, CallbackDelegate cb)
        {
            Logger.Log("__cfx_nui:setSongLoaded");

            if (args?.Any() ?? false)
            {
                if (args.ContainsKey("duration"))
                {
                    var du = args["duration"];
                    int.TryParse(du.ToString(), out int secs);

                    Logger.Log("setting duration:" + secs * 1000);
                    RadioStation.CurrentPlaying.SetLength((uint)secs * 1000);

                    RadioStation.CurrentSoundFile.Sound.PlayPosition =
                        RadioStation.CurrentSoundFile.GetRandomPlayPosition();

                    RadioStation.CurrentSoundFile.Sound.Paused = false;
                }
            }

            cb(true);
        }
    }
}
