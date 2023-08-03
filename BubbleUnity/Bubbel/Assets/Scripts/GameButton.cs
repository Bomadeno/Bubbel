

namespace MattsButtonLibrary
{
    public delegate void ButtonClickAction();


    public class GameButton: UnityEngine.MonoBehaviour
    {
        private UnityEngine.Rect buttonArea;
        private UnityEngine.Sprite buttonTexture;
        private ButtonClickAction buttonClickAction;
        private bool mouseIsOverButton;
        //button mode (pressed down, indented, etc)
        //checkbox button?
        //togglebutton?
        //TODO add 'mouse down mouse up' behaviour so buttons can be unclicked
        //add mouseover events (and other events)


        /// <summary>      
        /// No image is set, the button will be half transparent grey until
        /// an image is set.
        /// Set both the button action and the button area
        /// </summary>
        /// <param name="game">The instance of game the button
        /// is associated with</param>
        /// <param name="action">The method called when the button is clicked</param>
        /// <param name="buttonArea">The active area of the button</param>
        public void InitializeGameButton(ButtonClickAction action, UnityEngine.Rect buttonArea)
        {
            this.buttonClickAction = action;
            this.buttonArea = buttonArea;
        }


        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public void SetButtonArea(UnityEngine.Rect buttonArea)
        {
            this.buttonArea = buttonArea;
        }

        public void SetButtonImage(UnityEngine.Sprite buttonTexture)
        {
            this.buttonTexture = buttonTexture;
        }

        //big todo all the below need to be turned into TMP buttons
        
        /*
        public void Update()
        {
            if (isEnabled)
            {
                CheckForMouseOver();
                CheckForMousePresses();
            }
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
            if (mState.LeftButton == UnityEngine.Input.GetMouseButtonDown(0) && mouseIsOverButton)
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
        */
    }
}
