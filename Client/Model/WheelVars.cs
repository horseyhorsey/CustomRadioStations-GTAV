using System.Collections.Generic;
using SelectorWheel;

namespace CustomRadioStations
{
    public static class WheelVars
    {
        public static List<Wheel> RadioWheels = new List<Wheel>();

        public static Wheel CurrentRadioWheel;

        public static Wheel NextQueuedWheel;
    }
}
