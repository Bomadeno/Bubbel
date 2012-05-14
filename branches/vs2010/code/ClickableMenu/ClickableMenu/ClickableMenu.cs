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
using MattsButtonLibrary;

namespace ClickableMenu
{
    

    public delegate void ClickableMenuAction();


    /// <summary>
    /// Represents a background image/color/transparency. All parts are optional.
    /// </summary>
    public struct CBackground
    {
        public Texture2D Image;
        public Color color;

        public CBackground(Texture2D image, byte transparency, Color color)
        {
            this.Image = image;
            this.color = color;
            this.color.A = transparency;
        }
    }



    /// <summary>
    /// MenuButton is a class similar to GameButton but
    /// does not use DrawableGameComponent - update and 
    /// draw methods must be called manually.
    /// </summary>
    public class MenuButton
    {
        public string descriptor;
        public Texture2D buttonTexture;
        public Rectangle buttonArea;
        public ClickableMenuAction buttonClickAction;

        private MouseState mouseState;
        private int buttonActivationLevel;

        public MenuButton(string descriptor, Texture2D buttonTexture, Rectangle buttonArea, ClickableMenuAction action)
        {
            this.descriptor = descriptor;
            this.buttonTexture = buttonTexture;
            this.buttonArea = buttonArea;
            this.buttonClickAction = action;
            this.buttonActivationLevel = 0;
        }

        #region Update

        public void Update()
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

        #endregion



        #region Draw

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

        #endregion



        #region Internal methods

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

        #endregion
    }




    public class ButtonMenu : DrawableGameComponent
    {
        #region Variables

        private List<MenuButton> menuItems;

        //default background pixel for simple coloured backgrounds.
        private Texture2D backgroundPixel;
        private PresentationParameters presentationParameters;
        private SpriteBatch spriteBatch;

        private CBackground background;

        private bool isEnabled = true;

        Keys menuHotkey;
        private int ignoreMenuHotkey;

        ClickableMenuAction pauseAction;
        ClickableMenuAction resumeAction;
        bool hasResumeMethod;
        bool hasPauseMethod;

        #endregion



        #region Constructor

        public ButtonMenu(Game game)
            : base(game)
        {
            menuItems = new List<MenuButton>();
            isEnabled = false;
        }

        #endregion



        #region Exposed Methods

        public void AddMenuItem(MenuButton item)
        {
            menuItems.Add(item);
        }

        public void RemoveMenuItem(string descriptor)
        {
            for (int i = menuItems.Count - 1; i >= 0; i--)
            {
                if (menuItems[i].descriptor.Equals(descriptor))
                {
                    menuItems.RemoveAt(i);
                }
            }
        }

        public void ShowMenu()
        {
            isEnabled = true;
            ignoreMenuHotkey = 20;
        }

        public void HideMenu()
        {
            isEnabled = false;
            ignoreMenuHotkey = 20;
        }

        public void SetBackground(Texture2D image, byte transparency, Color color)
        {
            if (image == null)
            {
                background = new CBackground(backgroundPixel, transparency, color);
            }
            else
            {
                background = new CBackground(image, transparency, color);
            }
        }

        public void SetResumeMethod(ClickableMenuAction resume)
        {
            if (resume != null)
            {
                this.resumeAction = resume;
                hasResumeMethod = true;
            }
            else
            {
                hasResumeMethod = false;
            }
        }

        public void SetPauseMethod(ClickableMenuAction pause)
        {
            if (pause != null)
            {
                this.pauseAction = pause;
                hasPauseMethod = true;
            }
            else
            {
                hasPauseMethod = false;
            }
        }

        public void SetMenuHotkey(Keys newMenuHotkey)
        {
            this.menuHotkey = newMenuHotkey;
        }

        #endregion



        #region Initialisation

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion



        #region Load

        protected override void LoadContent()
        {
            this.presentationParameters = GraphicsDevice.PresentationParameters;
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundPixel = Game.Content.Load<Texture2D>("backgroundPixel");
            background = new CBackground(backgroundPixel, 128, Color.White);

            base.LoadContent();
        }

        #endregion



        #region Update

        public override void Update(GameTime gameTime)
        {
            if (isEnabled)
            {
                foreach (MenuButton button in menuItems)
                {
                    button.Update();
                }
            }
            ProcessMenuHotkey();

            base.Update(gameTime);
        }


        /// 
        /// Todo (bomadeno) - Add a more general hotkey addeer, allow a keysignature/action/ Rename to Processhotkeys.
        /// Iterate over hotkeys.

        /// <summary>
        /// Checks for menu hotkey (set by user) What is done when the hotkey is detected is up to the
        /// user. It is the uders responsibility to show the menu as well! (for flexibility, a game 
        /// may not want to show a menu under all conditions)
        /// 
        /// When checking for the menu hotkey, the menu ignores a held key, but does accept
        /// a rapid double press.
        /// 
        /// Default behaviour: Escape is hotkey, pressing it shows/hides menu
        /// </summary>
        private void ProcessMenuHotkey()
        {
            KeyboardState kbState = Keyboard.GetState();
            if (menuHotkey != Keys.None && kbState.IsKeyDown(menuHotkey) && ignoreMenuHotkey < 1)
            {
                if (isEnabled)
                {
                    if (hasResumeMethod)
                    {
                        resumeAction();
                    }
                    else
                    {
                        HideMenu();
                    }
                }
                else
                {
                    if (hasPauseMethod)
                    {
                        pauseAction();
                    }
                    else
                    {
                        ShowMenu();
                    }
                }
            }
            else if (kbState.IsKeyUp(menuHotkey))
            {
                ignoreMenuHotkey = 0;
            }
            else if (ignoreMenuHotkey > -10)
            {
                ignoreMenuHotkey--;
            }
        }

        #endregion



        #region Draw

        public override void Draw(GameTime gameTime)
        {
            if (isEnabled)
            {
                spriteBatch.Begin();
                
                DrawBackground();

                //draw buttons 
                foreach (MenuButton button in menuItems)
                {
                    button.Draw(spriteBatch);
                }

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);

            spriteBatch.Draw(background.Image, screenRectangle, background.color);
        }

        #endregion
    }
}
