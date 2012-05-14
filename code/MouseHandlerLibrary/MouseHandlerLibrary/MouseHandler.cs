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

namespace MouseHandlerLibrary
{
    public delegate void MouseClickAction(Vector2 location);

    public enum CursorMode
    {
        Pointer,
        Crosshair
    }

    /// <summary>
    /// This class provides mouse functionality.
    /// 
    /// Set the active area, whether the mouse movement is enabled
    /// Whether the mouse clicks are enabled
    /// Pass delegates to be executed when a mouse click is detected
    /// the cursor
    /// </summary>
    public class MouseHandler : DrawableGameComponent
    {
        #region Variables

        //general variables
        private SpriteBatch spriteBatch;

        //the current cursor mode
        private CursorMode cursorMode;

        //Cursor textures and hotspots
        private Texture2D defaultCursor;
        private Point defaultCursorHotSpot;
        private Texture2D crosshairCursor;
        private Vector2 crossHairHotSpot;

        private MouseState lastMouseState;
        private MouseState mouseState;

        //the actions performed when certain actions are detected
        private MouseClickAction leftClick;
        private MouseClickAction rightClick;
        private MouseClickAction middleClick;
        private MouseClickAction mouseMoved;

        private bool leftClickDetectionEnabled;
        private bool rightClickDetectionEnabled;
        private bool middleClickDetectionEnabled;
        private bool mouseMovedDetectionEnabled;
        PresentationParameters presentationParams;

        private bool mouseEnabled;
        private Rectangle activeRegion;

        #endregion

        #region Constructor

        public MouseHandler(Game game)
            : base(game)
        {
            leftClickDetectionEnabled = false;
            mouseMovedDetectionEnabled = false;
            rightClickDetectionEnabled = false;
            middleClickDetectionEnabled = false;
        }

        #endregion

        #region Initialisation

        public override void Initialize()
        {
            presentationParams = Game.GraphicsDevice.PresentationParameters;

            base.Initialize();
        }

        #endregion

        #region Load content

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load cursors
            crosshairCursor = Game.Content.Load<Texture2D>("CrosshairCursor");
            crossHairHotSpot = new Vector2(crosshairCursor.Width/2, crosshairCursor.Height/2);
            defaultCursor = Game.Content.Load<Texture2D>("DefaultCursor");
            
            base.LoadContent();
        }

        #endregion

        #region Exposed Methods

        /// <summary>
        /// Sets the active region for this cursor, where the
        /// cursor is displayed and responds to clicks
        /// </summary>
        /// <param name="activeRegion"></param>
        public void SetActiveRegion(Rectangle activeRegion)
        {
            this.activeRegion = activeRegion;
        }

        public Vector2 CurrentMouseLocation()
        {
            return new Vector2(mouseState.X, mouseState.Y);
        }

        public void SetCursorMode(CursorMode cursorMode)
        {
            this.cursorMode = cursorMode;
        }

        public void AddLeftClickAction(MouseClickAction action)
        {
            //add action
            leftClick = action;
            //enable left click detection
            leftClickDetectionEnabled = true;
        }

        public void RemoveLeftClickAction()
        {
            //remove action
            leftClick = null;
            //disable left click detection
            leftClickDetectionEnabled = false;
        }

        public void AddRightClickAction(MouseClickAction action)
        {
            //add action
            rightClick = action;
            //enable right click detection
            rightClickDetectionEnabled = true;
        }

        public void RemoveRightClickAction()
        {
            //remove action
            rightClick = null;
            //disable right click detection
            rightClickDetectionEnabled = false;
        }

        public void AddMiddleClickAction(MouseClickAction action)
        {
            //add action
            middleClick = action;
            //enable right click detection
            middleClickDetectionEnabled = true;
        }

        public void RemoveMiddleClickAction()
        {
            //remove action
            middleClick = null;
            //disable right click detection
            middleClickDetectionEnabled = false;
        }

        public void AddMouseMovedAction(MouseClickAction action)
        {
            //ad action
            mouseMoved = action;
            //enable mousemoved action
            mouseMovedDetectionEnabled = true;
        }

        public void Enable()
        {
            mouseEnabled = true;
        }

        public void Disable()
        {
            mouseEnabled = false;
        }

        #endregion

        #region Update methods

        public override void Update(GameTime gameTime)
        {
            lastMouseState = mouseState;
            mouseState = Mouse.GetState();

            if (mouseEnabled && IsInsideActiveRegion())
            {

                if (leftClickDetectionEnabled && mouseState.LeftButton == ButtonState.Pressed)
                {
                    leftClick(CurrentMouseLocation());
                }
                if (rightClickDetectionEnabled && mouseState.RightButton == ButtonState.Pressed)
                {
                    rightClick(CurrentMouseLocation());
                }
                if (middleClickDetectionEnabled && mouseState.MiddleButton == ButtonState.Pressed)
                {
                    middleClick(CurrentMouseLocation());
                }
                if (mouseMovedDetectionEnabled && MouseMoved())
                {
                    mouseMoved(CurrentMouseLocation());
                }
            }
            base.Update(gameTime);
        }

        #endregion

        #region Draw Methods

        public override void Draw(GameTime gameTime)
        {
            if (mouseEnabled && IsInsideActiveRegion())
            {
                spriteBatch.Begin();
                DrawCursor();
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        private void DrawCursor()
        {
            if (cursorMode == CursorMode.Pointer)
            {
                spriteBatch.Draw(defaultCursor, CurrentMouseLocation(), Color.White);
            }
            else if (cursorMode == CursorMode.Crosshair)
            {
                spriteBatch.Draw(crosshairCursor, CurrentMouseLocation(), null, Color.White, 0, crossHairHotSpot, 1, SpriteEffects.None, 0);
            }
        }

        #endregion


        #region Internal Helpers

        private bool IsInsideActiveRegion()
        {
            if (CurrentMouseLocation().X < activeRegion.Left)
                return false;
            if (CurrentMouseLocation().X > activeRegion.Right)
                return false;
            if (CurrentMouseLocation().Y < activeRegion.Top)
                return false;
            if (CurrentMouseLocation().Y > activeRegion.Bottom)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the mouse has moved
        /// </summary>
        /// <returns></returns>
        private bool MouseMoved()
        {
            if (lastMouseState.X != mouseState.X)
                return true;
            if (lastMouseState.Y != mouseState.Y)
                return true;

            return false;
        }

        #endregion
    }
}
