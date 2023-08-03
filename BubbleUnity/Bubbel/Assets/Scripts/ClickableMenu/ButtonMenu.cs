using System;
using System.Collections.Generic;
using MattsMenuLibrary;
using Color = UnityEngine.Color;
using KeyCode = UnityEngine.KeyCode;

namespace ClickableMenu
{
    public delegate void ClickableMenuAction();

    public class ButtonMenu : UnityEngine.MonoBehaviour
    {
        private List<MenuButton> menuItems;

        //default background pixel for simple coloured backgrounds.
        private UnityEngine.Sprite backgroundPixel;

        private Background background;

        private bool isEnabled = true;

        KeyCode menuHotkey;
        private int ignoreMenuHotkey;

        ClickableMenuAction pauseAction;
        ClickableMenuAction resumeAction;
        bool hasResumeMethod;
        bool hasPauseMethod;

        private void Awake()
        {
            menuItems = new List<MenuButton>();
            isEnabled = false;
        }



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

        public void SetBackground(UnityEngine.Sprite image, byte transparency, Color color)
        {
            if (image == null)
            {
                background = new Background(backgroundPixel, transparency, color);
            }
            else
            {
                background = new Background(image, transparency, color);
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

        public void SetMenuHotkey(KeyCode newMenuHotkey)
        {
            this.menuHotkey = newMenuHotkey;
        }


        protected void LoadContent()
        {
            background = new Background(backgroundPixel, 128, Color.white);
        }


        public void Update()
        {
            if (isEnabled)
            {
                foreach (MenuButton button in menuItems)
                {
                    button.Update();
                }
            }
            ProcessMenuHotkey();
        }



        /// <summary>
        /// Checks for menu hotkey (set by user) What is done when the hotkey is detected is up to the
        /// user. It is the user's responsibility to show the menu as well! (for flexibility, a game 
        /// may not want to show a menu under all conditions)
        /// 
        /// When checking for the menu hotkey, the menu ignores a held key, but does accept
        /// a rapid double press.
        /// 
        /// Todo (bomadeno) - Add a more general hotkey addeer, allow a keysignature/action/ Rename to Processhotkeys.
        /// Todo (bomadeno) Iterate over hotkeys.
        /// 
        /// Default behaviour: Escape is hotkey, pressing it shows/hides menu
        /// </summary>
        private void ProcessMenuHotkey()
        {
            if (menuHotkey != KeyCode.None && UnityEngine.Input.GetKey(menuHotkey) && ignoreMenuHotkey < 1)
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
            else if (!UnityEngine.Input.GetKey(menuHotkey))
            {
                ignoreMenuHotkey = 0;
            }
            else if (ignoreMenuHotkey > -10)
            {
                ignoreMenuHotkey--;
            }
        }

        //Big todo, redo as canvas
/*
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
    */
    }
}
