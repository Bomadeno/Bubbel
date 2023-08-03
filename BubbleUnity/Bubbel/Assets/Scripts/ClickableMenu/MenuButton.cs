namespace ClickableMenu
{
    
    /// <summary>
    /// MenuButton is a class similar to GameButton but
    /// does not use DrawableGameComponent - update and 
    /// draw methods must be called manually.
    /// </summary>
    public class MenuButton
    {
        public string descriptor;
        public UnityEngine.Sprite buttonTexture;
        public UnityEngine.Rect buttonArea;
        public ClickableMenuAction buttonClickAction;

        private int buttonActivationLevel;

        public MenuButton(string descriptor, UnityEngine.Sprite buttonTexture, UnityEngine.Rect buttonArea, ClickableMenuAction action)
        {
            this.descriptor = descriptor;
            this.buttonTexture = buttonTexture;
            this.buttonArea = buttonArea;
            this.buttonClickAction = action;
            this.buttonActivationLevel = 0;
        }
        

        
        public void Update()
        {
        }

        //todo replace with canvas stuff
        /*
        private void CheckForClicksAndMouseOvers()
        {
            //check for clicks in rectangle
            mouseState = Mouse.GetState();

            //Waiting for someone to mouse over button
            if (buttonActivationLevel == 0)
            {
                if (MouseIsOverButton() && mouseState.LeftButton == ButtonState.Released)
                {
                    buttonActivationLevel = 1;
                }
            }
            else if (buttonActivationLevel == 1)
            {
                if (!MouseIsOverButton())
                {
                    buttonActivationLevel = 0;
                }
                else if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    buttonActivationLevel = 2;
                }
            }
            else if (buttonActivationLevel == 2)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    if (MouseIsOverButton())
                    {
                        buttonClickAction();
                    }

                    buttonActivationLevel = 0;
                }
                //if mouse is still down no change occurs to the activationlevel
            }
        }

        //draws the button. draws buttn slightly differently is mouse is over button.
        public void Draw(SpriteBatch spriteBatch)
        {
            if (MouseIsOverButton())
            {
                spriteBatch.Draw(buttonTexture, buttonArea, Color.LightGray);
            }
            else
            {
                spriteBatch.Draw(buttonTexture, buttonArea, Color.White);
            }
        }


        /// <summary>
        /// Tests if the mouse is in the button area
        /// </summary>
        /// <returns>true is the mouse is within the button area and
        /// false otherwise</returns>
        private bool MouseIsOverButton()
        {
            if (mouseState.X < buttonArea.Left || mouseState.X > buttonArea.Right || mouseState.Y < buttonArea.Top || mouseState.Y > buttonArea.Bottom)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        */
    }
    
}