namespace CustomRadioStations
{
    class SoundFileTimePair
    {
        public ISoundFile SoundFile;
        public uint StartTime;

        public SoundFileTimePair(ISoundFile sFile, uint time)
        {
            SoundFile = sFile;
            StartTime = time;
        }
    }
}
