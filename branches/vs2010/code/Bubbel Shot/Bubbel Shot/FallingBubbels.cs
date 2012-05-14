using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

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