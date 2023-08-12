using System.Collections.Generic;
using UnityEngine;

namespace Bubbel_Shot
{
    public struct CannonData
    {
        public float AngleInRadians;
        public Color currentBallColor;
        public float currentBallRotation;
        public List<Color> nextFiveShots;
        public Vector2[] nextFiveShotLocation;
    }
}