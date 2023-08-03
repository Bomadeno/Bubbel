using System.Collections.Generic;
using UnityEngine;

namespace Bubbel_Shot
{
    public struct FallingBubbels
    {
        public List<Point> points;
        public List<Vector2> positions;
        public Vector2 downSpeed;
        public bool accelerating;
        public const float maxSpeed = 30;
        public List<Color> color;
    }
}