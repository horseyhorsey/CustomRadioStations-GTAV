namespace CustomRadioStations.Model
{
    public class UserWheelCats
    {
        public WheelCat[] WheelCats { get; set; }
    }

    public class WheelCat
    {
        public string name { get; set; }
        public string desc { get; set; }

        public string displayname { get; set; }
        public Station[] stations { get; set; }
    }

    public class Station
    {
        public string name { get; set; }
        public string displayname { get; set; }
        public string type { get; set; }
        public string desc { get; set; }
        public string url { get; set; }
        public string[] formats { get; set; }
        public string[] files { get; set; }
    }
}
