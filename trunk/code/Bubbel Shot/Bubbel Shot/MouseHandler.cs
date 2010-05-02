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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Bubbel_Shot
{
    public delegate void VectorAction(Vector2 clickSpot);

    class MouseHandlerOld
    {
        private MouseState mouseState;
        private VectorAction leftClickAction;
        private VectorAction updateTarget;
        public bool mouseEnabled = true;
        private Rectangle activeArea;

        public MouseHandlerOld(Rectangle activeArea)
        {
            this.activeArea = activeArea;
        }

        public void SetTargetUpdater(VectorAction updateTarget)
        {
            this.updateTarget = updateTarget;
        }

        public void SetLeftClickAction(VectorAction leftClickAction)
        {
            this.leftClickAction = leftClickAction;
        }

        public Vector2 GetMouseLocation()
        {
            return new Vector2(mouseState.X, mouseState.Y);
        }

        public void Update()
        {
            mouseState = Mouse.GetState();

            if (mouseEnabled && IsInActiveArea())
            {
                updateTarget(GetMouseLocation());
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseEnabled = true;
                    leftClickAction(GetMouseLocation());
                }
            }
        }

        private bool IsInActiveArea()
        {
            if (mouseState.X > activeArea.Left && mouseState.X < activeArea.Right
                && mouseState.Y > activeArea.Top && mouseState.Y < activeArea.Bottom)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
