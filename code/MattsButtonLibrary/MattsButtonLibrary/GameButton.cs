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

namespace MattsButtonLibrary
{
    public delegate void ButtonClickAction();


    public class GameButton:DrawableGameComponent
    {
        #region Variables

        private SpriteBatch spriteBatch;
        private bool isEnabled = true;
        private Rectangle buttonArea;
        private Texture2D buttonTexture;
        private ButtonClickAction buttonClickAction;
        private bool mouseIsOverButton;
        //button mode (pressed down, indented, etc)
        //checkbox button?
        //togglebutton?
        //TODO add 'mouse down mouse up' behaviour so buttons can be unclicked
        //add mouseover events (and other events)

        #endregion



        
        #region Constructor
        
        /// <summary>      
        /// No image is set, the button will be half transparent grey until
        /// an image is set.
        /// Set both the button action and the button area
        /// </summary>
        /// <param name="game">The instance of game the button
        /// is associated with</param>
        /// <param name="action">The method called when the button is clicked</param>
        /// <param name="buttonArea">The active area of the button</param>
        public GameButton(Game game, ButtonClickAction action, Rectangle buttonArea)
            : base(game)
        {
            this.buttonClickAction = action;
            this.buttonArea = buttonArea;
        }

        #endregion




        #region Exposed Methods

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        public void SetButtonArea(Rectangle buttonArea)
        {
            this.buttonArea = buttonArea;
        }

        public void SetButtonImage(Texture2D buttonTexture)
        {
            this.buttonTexture = buttonTexture;
        }

        #endregion



        #region Initialisation

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion



        #region Load content

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        #endregion



        #region Update

        public override void Update(GameTime gameTime)
        {
            if (isEnabled)
            {
                CheckForMouseOver();
                CheckForMousePresses();
            }

            base.Update(gameTime);
        }

        private void CheckForMouseOver()
        {
            if (MouseInButtonArea())
            {
                mouseIsOverButton = true;
            }
            else
            {
                mouseIsOverButton = false;
            }
        }

        private void CheckForMousePresses()
        {
            //check for a click
            MouseState mState = Mouse.GetState();
            //check the click is in teh area
            if (mState.LeftButton == ButtonState.Pressed && mouseIsOverButton)
            {
                //do the action associated with the button
                buttonClickAction();
            }
        }

        private bool MouseInButtonArea()
        {
            MouseState mState = Mouse.GetState();
            if (mState.X < buttonArea.Left)
                return false;
            if (mState.X > buttonArea.Right)
                return false;
            if (mState.Y < buttonArea.Top)
                return false;
            if (mState.Y > buttonArea.Bottom)
                return false;

            //else
            return true;
        }

        #endregion



        #region Draw

        public override void Draw(GameTime gameTime)
        {
            if (buttonTexture == null)
            {
                buttonTexture = Game.Content.Load<Texture2D>("TransparentBackgroundPixel");
            }

            spriteBatch.Begin();

            if (mouseIsOverButton)
            {
                spriteBatch.Draw(buttonTexture, buttonArea, Color.LightGray);
            }
            else
            {
                spriteBatch.Draw(buttonTexture, buttonArea, Color.White);
            }

            base.Draw(gameTime);

            spriteBatch.End();
        }

        #endregion

    }
}
