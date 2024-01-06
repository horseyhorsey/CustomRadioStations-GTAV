using System.Collections.Generic;

namespace CustomRadioStations
{
    /// <summary> Native to the game radio station wheel which has child NativeWheels </summary>
    class NativeWheel
    {
        public string Name;
        public List<string> stationList = new List<string>();

        public NativeWheel(string name)
        {
            Name = name;
        }
        
        public static List<NativeWheel> WheelList = new List<NativeWheel>();
    }
}
