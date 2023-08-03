namespace Bubbel_Shot
{
    public class Score
    {
        public const int BASE_SCORE_PER_POP = 10;
        public const int BASE_SCORE_PER_DROP = 12;

        public long currentBoardScore;
        public long currentTotalScore;
        public int multiplier; //increases based on shots fired 
        public int numberOfShotsFired;
        public int numberPoppedInARow;
        public int numberDropped;

        public int shotsMissed;

        public Score()
        {
            shotsMissed = 0;
        }

        public void Miss()
        {
            multiplier = 1;
            numberPoppedInARow = 0;
        }

        public void Hit()
        {
            multiplier++;
        }

        public int Pop()
        {
            numberPoppedInARow++;
            int scoreForThisBubbel = Score.BASE_SCORE_PER_POP * numberPoppedInARow * multiplier;
            currentTotalScore += scoreForThisBubbel;

            return scoreForThisBubbel;
        }
    }
}