using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MattsMenuLibrary;

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