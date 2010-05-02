using System;
using System.Collections.Generic;
using System.Linq;

namespace Bubbel_Shot
{
    public class Score
    {
        public const int baseScorePerPop = 10;
        public const int baseScorePerDrop = 12;
        public const int missedShotsAllowed = 3;

        public long currentScore;
        public int multiplier; //increases based on shots fired and 
        public int comboChain; //number of pops in a row
        public int numberOfShotsFired;
        public int numberPopped;
        public int totalPopped;
        public int numberDropped;
        public int totalDropped;

        public int shotsUntilFieldDrops;

        public Score()
        {
            shotsUntilFieldDrops = 4;
            comboChain = 1;
        }
    }
}