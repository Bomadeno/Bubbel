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
//using MattsMouseHandlerLibrary;

namespace MattsMenuLibrary
{
    public delegate void MenuAction();
    
    public class MenuItem
    {
        #region MenuItem Variables

        public string menuItemName;
        public MenuAction Activate;

        #endregion

        public MenuItem(string menuItemName, MenuAction menuItemAction)
        {
            this.menuItemName = menuItemName;
            this.Activate = menuItemAction;
        }
    }



    public enum Alignment
    {
        Left,
        Right,
        Centre
    }



    public struct Background
    {
        public Texture2D Image;
        public Color color;

        public Background(Texture2D image, byte transparency, Color color)
        {
            this.Image = image;
            this.color = color;
            this.color.A = transparency;
        }
    }



    public enum Position
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Centre,
        UnderTitle
    }



    public class GameMenu : DrawableGameComponent
    {
        #region Menu Variables

        //private MouseHandler mouseHandler;

        ///internal variables for getting stuff done
        private SpriteBatch spriteBatch;
        private List<MenuItem> menuItems;
        private PresentationParameters presentationParameters;
        ContentManager manager;
        Texture2D backgroundFallback;
        //-1 indicates nothing is selected
        private int currentlySelected;
        private Position titlePosition;
        private Position menuItemsPosition;
        private Background background;
        //prevent menu scrolling too fast
        private int ignoreDownArrow;
        private int ignoreUpArrow;
        private int ignoreEnter;
        private int ignoreMenuHotkey;
        const int menuLag = 20;

        private bool hasResumeMethod = false;
        private bool hasPauseMethod = false;
        private MenuAction resume;
        private MenuAction pause;

        //is menu being updated/drawn?
        private bool menuIsShown;
        public bool MenuIsShown
        {
            get { return menuIsShown; }
        }

        //the title for the menu
        private string menuTitle;
        public string MenuTitle
        {
            get { return menuTitle; }
            set { menuTitle = value; }
        }
        //defines whether the menu has a loop round behaviour
        public bool menuLoopsRound;
        SpriteFont inactiveFont;
        SpriteFont activeFont;
        SpriteFont titleFont;
        Color inactiveColor;
        Color activecolor;
        Color titleColor;
        const int padding = 40;
        const int lineSpacing = 0;
        Rectangle menuArea;
        Keys menuHotkey;
        

        #endregion

        #region Initialisation

        public GameMenu(Game game, string menuTitle)
            : base(game)
        {
            menuItems = new List<MenuItem>();
            menuIsShown = false;
            this.menuTitle = menuTitle;
            currentlySelected = -1;
            menuLoopsRound = true;
            titlePosition = Position.Centre;
            menuItemsPosition = Position.UnderTitle;
            background = new Background(null, 255, Color.White);
            manager = new ResourceContentManager(game.Services, Resources.ResourceManager);
            activecolor = Color.Red;
            inactiveColor = Color.Black;
            titleColor = inactiveColor;
        }

        public override void Initialize()
        {
            this.presentationParameters = GraphicsDevice.PresentationParameters;

            base.Initialize();
        }

        #endregion

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            backgroundFallback = manager.Load<Texture2D>("backgroundPixel");
            activeFont = manager.Load<SpriteFont>("defaultMenuFont");
            inactiveFont = manager.Load<SpriteFont>("defaultMenuFont");
            titleFont = manager.Load<SpriteFont>("defaultMenuTitle");
        }




        #region Exposed methods

        public void AddMenuItem(string menuItemName, MenuAction menuAction)
        {
            menuItems.Add(new MenuItem(menuItemName, menuAction));
            //if this is the first added item, make it the selected item
            if (menuItems.Count == 1)
            {
                currentlySelected = 0;
            }
        }

        public void RemoveMenuItem(string menuItemToBeRemoved)
        {
            MenuItem toBeRemoved;
            foreach (MenuItem mu in menuItems)
            {
                if (mu.menuItemName.Equals(menuItemToBeRemoved))
                {
                    toBeRemoved = mu;
                    menuItems.Remove(toBeRemoved);
                }
            }
            //deselects from the menu
            currentlySelected = -1;
        }

        public void SetMenuItemsPosition(Position position)
        {
            menuItemsPosition = position;
        }

        public void SetTitlePosition(Position position)
        {
            if (position == Position.UnderTitle)
            {
                throw new ApplicationException("Error in calling code... title cannot be positioned under title.");
            }
            else
            {
                titlePosition = position;
            }
        }

        public void SetBackground(Texture2D image, byte transparency, Color colour)
        {
            background = new Background(image, transparency, colour);
        }

        public void MoveSelectionUp()
        {
            //move selection down
            currentlySelected--;

            if (currentlySelected < 0)
            {
                if (menuItems.Count > 0)
                {
                    if (menuLoopsRound)
                    {
                        //selects last entry in list (for if nothing is selected, or 'roll round' from top)
                        currentlySelected = menuItems.Count - 1;
                    }
                    else
                    {
                        //do not change selection/select list top
                        currentlySelected = 0;
                    }
                }
            }
            
        }

        public void MoveSelectionDown()
        {
            //if nothing is selected and there are menu items
            if (currentlySelected < 0 && menuItems.Count > 0)
            {
                //select first entry in list (for if nothing is selected)
                currentlySelected = 0;
                return;
            }

            //move selection up
            currentlySelected++;

            //if selected item is above list range
            if (currentlySelected > menuItems.Count - 1)
            {
                if (menuLoopsRound)
                {
                    //show loop round behaviour
                    currentlySelected = 0;
                }
                else
                {
                    //do not change selection
                    currentlySelected = menuItems.Count - 1;
                }
            }          
        }

        public void ActivateCurrentMenuItem()
        {
            menuItems[currentlySelected].Activate();
        }

        public void SetResumeMethod(MenuAction resume)
        {
            if (resume != null)
            {
                this.resume = resume;
                hasResumeMethod = true;
            }
            else
            {
                hasResumeMethod = false;
            }
        }

        public void SetPauseMethod(MenuAction pause)
        {
            if (pause != null)
            {
                this.pause = pause;
                hasPauseMethod = true;
            }
            else
            {
                hasPauseMethod = false;
            }
        }

        public void ShowMenu()
        {
            this.menuIsShown = true;
            ignoreMenuHotkey = menuLag;
        }

        public void HideMenu()
        {
            this.menuIsShown = false;
            ignoreMenuHotkey = menuLag;
        }

        public void SetMenuHotkey(Keys newMenuHotkey)
        {
            this.menuHotkey = newMenuHotkey;
        }

        //todo: click(x,y) (activate item under click)

        #endregion



        #region Update method

        public override void Update(GameTime gameTime)
        {
            if (menuIsShown)
            {
                ProcessKeyboard();
                CreateMenuRectangle();
            }
            ProcessMenuHotkey();
            base.Update(gameTime);
        }

        private void ProcessMenuHotkey()
        {
            KeyboardState kbState = Keyboard.GetState();
            if (menuHotkey != Keys.None && kbState.IsKeyDown(menuHotkey) && ignoreMenuHotkey < 1)
            {
                if (menuIsShown)
                {
                    if (hasResumeMethod)
                    {
                        resume();
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
                        pause();
                    }
                    else
                    {
                        ShowMenu();
                    }
                }
            }
            else if (kbState.IsKeyUp(Keys.Escape))
            {
                ignoreMenuHotkey = 0;
            }
            else if (ignoreMenuHotkey > -10)
            {
                ignoreMenuHotkey--;
            }
        }

        private void ProcessKeyboard()
        {
            KeyboardState kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.Up) && ignoreUpArrow < 1)
            {
                MoveSelectionUp();
                ignoreUpArrow = menuLag;
            }
            else if (kbState.IsKeyUp(Keys.Up))
            {
                ignoreUpArrow = 0;
            }
            else if (ignoreUpArrow > -10)
            {
                ignoreUpArrow--;
            }

            if (kbState.IsKeyDown(Keys.Down) && ignoreDownArrow < 1)
            {
                MoveSelectionDown();
                ignoreDownArrow = menuLag;
            }
            else if (kbState.IsKeyUp(Keys.Down))
            {
                ignoreDownArrow = 0;
            }
            else if (ignoreDownArrow > -10)
            {
                ignoreDownArrow--;
            }

            if (kbState.IsKeyDown(Keys.Enter) && ignoreEnter < 1)
            {
                ActivateCurrentMenuItem();
                ignoreEnter = menuLag;
            }
            else if (kbState.IsKeyUp(Keys.Enter))
            {
                ignoreEnter = 0;
            }
            else if (ignoreEnter > -10)
            {
                ignoreEnter--;
            }
        }

        public void CreateMenuRectangle()
        {
            menuArea = new Rectangle(0, 0, 0, 0);
            if (menuItems.Count > 0)
            {
                menuArea.Height = (int)activeFont.MeasureString(menuItems[0].menuItemName).Y;
                foreach (MenuItem mu in menuItems)
                {
                    if (activeFont.MeasureString(mu.menuItemName).X > menuArea.Width)
                    {
                        //gets the widest menu item
                        menuArea.Width = (int)activeFont.MeasureString(mu.menuItemName).X;
                    }
                }
            }
            menuArea.Height = (menuArea.Height + lineSpacing) * menuItems.Count;
        }

        #endregion



        #region Draw method

        public override void Draw(GameTime gameTime)
        {
            if (menuIsShown)
            {
                DrawBackground();
                DrawMenu();
            }
        }

        private void DrawBackground()
        {

            Rectangle screenRectangle = new Rectangle(0, 0, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            spriteBatch.Begin();
            if (background.Image == null)
            {
                spriteBatch.Draw(backgroundFallback, screenRectangle, background.color);
            }
            else
            {
                spriteBatch.Draw(background.Image, screenRectangle, background.color);
            }
            spriteBatch.End();
        }

        public void DrawMenu()
        {
            
            int backBufferWidth = presentationParameters.BackBufferWidth;
            int backBufferHeight = presentationParameters.BackBufferHeight;
            Vector2 titleSize = titleFont.MeasureString(menuTitle);

            



            spriteBatch.Begin();
            switch (titlePosition)
            {
                case Position.BottomLeft:
                    if (menuItemsPosition == Position.UnderTitle)
                    {
                        //draw menu title then draw menu items
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(padding, backBufferHeight - 40 - titleSize.Y- menuArea.Height), titleColor);
                        //left align
                        DrawMenuItems(new Vector2(padding, backBufferHeight - 40 - menuArea.Height), Alignment.Left);
                    }
                    else
                    {
                        //draw menu title bottom left
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(40,backBufferHeight-40-titleSize.Y), titleColor);
                        //then draw menu items wherever
                        DrawMenuItems();
                    }
                    break;

                case Position.BottomRight:
                    if (menuItemsPosition == Position.UnderTitle)
                    {
                        //draw menu title then draw menu items
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth - padding - titleSize.X, backBufferHeight - 40 - titleSize.Y - menuArea.Height), titleColor);
                        //right align
                        DrawMenuItems(new Vector2(backBufferWidth - padding-menuArea.Width, backBufferHeight - 40 - menuArea.Height), Alignment.Right);
                    }
                    else
                    {
                        //draw menu title in bottom right
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth - padding - titleSize.X, backBufferHeight - 40 - titleSize.Y), titleColor);
                        //then menu items wherever
                        DrawMenuItems();
                    }
                    break;

                case Position.Centre:
                    if (menuItemsPosition == Position.UnderTitle)
                    {
                        //draw menu title
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth / 2 - titleSize.X / 2, backBufferHeight / 2 - menuArea.Height / 2 - titleSize.Y / 2), titleColor);
                        //then menu items underneath, centre align
                        DrawMenuItems(new Vector2(backBufferWidth / 2 - (menuArea.Width / 2), (backBufferHeight / 2) - (menuArea.Height / 2) + titleSize.Y / 2), Alignment.Centre);
                    }
                    else
                    {
                        //draw menu title in centre
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth / 2 - titleSize.X / 2, backBufferHeight / 2 - titleSize.Y / 2), titleColor);
                        //then menu items wherever
                        DrawMenuItems();
                    }
                    break;

                case Position.TopLeft:
                    if (menuItemsPosition == Position.UnderTitle)
                    {
                        //draw menu items and then title above them
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(padding, padding), titleColor);
                        //centre left
                        DrawMenuItems(new Vector2(padding, padding+titleSize.Y), Alignment.Left);
                    }
                    else
                    {
                        //draw menu title in top left
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(padding,padding), titleColor);
                        //then menu items wherever
                        DrawMenuItems();
                    }
                    break;

                case Position.TopRight:
                    if (menuItemsPosition == Position.UnderTitle)
                    {
                        //draw menu items and then title above them
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth - padding - titleSize.X, padding), titleColor);
                        //right align
                        DrawMenuItems(new Vector2(backBufferWidth - menuArea.Width - padding, padding + titleSize.Y), Alignment.Right);
                    }
                    else
                    {
                        //draw menu title in top right
                        spriteBatch.DrawString(titleFont, menuTitle, new Vector2(backBufferWidth - padding - titleSize.X, padding), titleColor);
                        //then menu items wherever
                        DrawMenuItems();
                    }
                    break;
                default:
                    break;
            }
            spriteBatch.End();
        }

        //draw menu items wherever(not under title)
        public void DrawMenuItems()
        {
            int backBufferWidth = presentationParameters.BackBufferWidth;
            int backBufferHeight = presentationParameters.BackBufferHeight;
            Vector2 itemSize = titleFont.MeasureString(menuItems[0].menuItemName);
            switch (menuItemsPosition)
            {
                case Position.BottomLeft:
                    DrawMenuItems(new Vector2(padding, backBufferHeight - padding - menuArea.Height), Alignment.Left);
                    break;
                case Position.BottomRight:
                    DrawMenuItems(new Vector2(backBufferWidth - padding - menuArea.Width, backBufferHeight - 40 - menuArea.Height), Alignment.Right);
                    break;
                case Position.Centre:
                    DrawMenuItems(new Vector2(backBufferWidth/2-(menuArea.Width/2),(backBufferHeight/2)-(menuArea.Height/2)), Alignment.Centre);
                    break;
                case Position.TopLeft:
                    DrawMenuItems(new Vector2(padding,padding), Alignment.Left);
                    break;
                case Position.TopRight:
                    DrawMenuItems(new Vector2(backBufferWidth - padding - menuArea.Width, padding), Alignment.Right);
                    break;
            }
        }


        /// <summary>
        /// Draws the menu items from the start point
        /// with corresponding alignment
        /// </summary>
        /// <param name="startPoint">The top left corner of the menuArea</param>
        /// <param name="align">the alignment. left =text against left side of the menuarea
        /// right = text agains the right side of the menu area
        /// centre = in the middle of the menu area</param>
        private void DrawMenuItems(Vector2 startPoint, Alignment align)
        {
            Vector2 nextItemPosition = startPoint;
            switch(align){
                case Alignment.Left:
                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        if (i == currentlySelected)
                        {
                            spriteBatch.DrawString(activeFont, menuItems[i].menuItemName, nextItemPosition, activecolor);
                        }
                        else
                        {
                            spriteBatch.DrawString(inactiveFont, menuItems[i].menuItemName, nextItemPosition, inactiveColor);
                        }
                        nextItemPosition.Y = nextItemPosition.Y + lineSpacing+inactiveFont.MeasureString(menuItems[i].menuItemName).Y;
                    }
                    break;
                case Alignment.Right:
                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        nextItemPosition.X = nextItemPosition.X + (menuArea.Width-inactiveFont.MeasureString(menuItems[i].menuItemName).X);
                        if (i == currentlySelected)
                        {
                            spriteBatch.DrawString(activeFont, menuItems[i].menuItemName, nextItemPosition, activecolor);
                        }
                        else
                        {
                            spriteBatch.DrawString(inactiveFont, menuItems[i].menuItemName, nextItemPosition, inactiveColor);
                        }
                        nextItemPosition.Y = nextItemPosition.Y + lineSpacing + inactiveFont.MeasureString(menuItems[i].menuItemName).Y;
                        nextItemPosition.X = startPoint.X;
                    }
                    break;
                case Alignment.Centre:
                    //throw new NotImplementedException();
                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        nextItemPosition.X = nextItemPosition.X + ((menuArea.Width - inactiveFont.MeasureString(menuItems[i].menuItemName).X) / 2);
                        if (i == currentlySelected)
                        {
                            spriteBatch.DrawString(activeFont, menuItems[i].menuItemName, nextItemPosition, activecolor);
                        }
                        else
                        {
                            spriteBatch.DrawString(inactiveFont, menuItems[i].menuItemName, nextItemPosition, inactiveColor);
                        }
                        nextItemPosition.Y = nextItemPosition.Y + lineSpacing + inactiveFont.MeasureString(menuItems[i].menuItemName).Y;
                        nextItemPosition.X = startPoint.X;
                    }
                    break;
            }
        }

        #endregion
    }
}
