namespace Bubbel_Shot
{
    class DifficultySettings
    {
        private int initialRowCount;

        public int InitialRowCount
        {
            get { return initialRowCount; }
            set { initialRowCount = value; }
        }

        private int numberOfDifferentBallColours;

        public int NumberOfDifferentBallColours
        {
            get { return numberOfDifferentBallColours; }
            set { numberOfDifferentBallColours = value; }
        }

        private int missedShotsAllowed;

        public int MissedShotsAllowed
        {
            get { return missedShotsAllowed; }
            set { missedShotsAllowed = value; }
        }

        public DifficultySettings()
        {
            //set to default
            initialRowCount = 5;
            missedShotsAllowed = 3;
            numberOfDifferentBallColours = 5;
        }
    }
}
