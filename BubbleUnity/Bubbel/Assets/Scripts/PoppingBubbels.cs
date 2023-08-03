using System.Collections.Generic;
using UnityEngine;

namespace Bubbel_Shot
{
    public struct PoppingBubbels
    {
        public List<Point> points;
        public List<Color> colours;
        public Point currentlyPopping;
        public Color currentlyPoppingColor;
        public int currentlyPoppingProgress;
    }
}