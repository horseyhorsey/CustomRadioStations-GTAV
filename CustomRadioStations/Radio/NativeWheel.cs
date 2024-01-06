using System.Collections.Generic;

namespace CustomRadioStations
{
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
