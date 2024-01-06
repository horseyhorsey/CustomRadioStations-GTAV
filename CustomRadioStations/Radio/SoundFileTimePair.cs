namespace CustomRadioStations
{
    class SoundFileTimePair
    {
        public SoundFile SoundFile;
        public uint StartTime;

        public SoundFileTimePair(SoundFile sFile, uint time)
        {
            SoundFile = sFile;
            StartTime = time;
        }
    }
}
