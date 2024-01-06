namespace CustomRadioStations
{
    /// <summary>Represents a song track with artist title </summary>
    class Track
    {
        public uint StartTime { get; }
        public string Artist { get; }
        public string Title { get; }

        public Track(uint startTime, string artist, string title)
        {
            StartTime = startTime;
            Artist = artist;
            Title = title;
        }

        /// <summary> artist - title - starttime </summary>
        /// <returns></returns>
        public override string ToString() => $"Artist: {Artist}\nTitle: {Title}\nMs: {StartTime}";
    }
}
