using UnityEngine;

namespace MattsMenuLibrary
{
    public struct Background
    {
        public Sprite Image;
        public Color color;

        public Background(Sprite image, byte transparency, Color color)
        {
            this.Image = image;
            this.color = color;
            this.color.a = transparency;
        }
    }
}