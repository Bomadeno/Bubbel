using System;
using System.Collections.Generic;
using Bubbel_Shot;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;
using Rect = UnityEngine.Rect;

//using MattsMouseHandlerLibrary;

namespace MattsMenuLibrary
{
    public class GameMenu : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] private UnityEngine.UI.Image background;
        
        //private MouseHandler mouseHandler;

        //internal variables for getting stuff done
        
        private List<MenuItem> menuItems;
        //-1 indicates nothing is selected
        private int currentlySelected;
        private Position titlePosition;
        private Position menuItemsPosition;
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
        public string menuTitle;
        public string MenuTitle
        {
            get { return menuTitle; }
            set { menuTitle = value; }
        }
        //defines whether the menu has a loop round behaviour
        public bool menuLoopsRound;
        Color inactiveColor;
        Color activecolor;
        Color titleColor;
        const int padding = 40;
        const int lineSpacing = 0;
        Rect menuArea;
        UnityEngine.KeyCode menuHotkey;

        private void Awake()
        {
            menuItems = new List<MenuItem>();
            menuIsShown = false;
            currentlySelected = -1;
            menuLoopsRound = true;
            titlePosition = Position.Centre;
            menuItemsPosition = Position.UnderTitle;
            activecolor = Color.red;
            inactiveColor = Color.black;
            titleColor = inactiveColor;
        }

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
                throw new BubbelException("Error in calling code... title cannot be positioned under title.");
            }
            else
            {
                titlePosition = position;
            }
        }

        public void SetBackground(UnityEngine.Sprite image, Color colour)
        {
            background.sprite = image;
            background.color = colour;
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

        public void SetMenuHotkey(UnityEngine.KeyCode  newMenuHotkey)
        {
            this.menuHotkey = newMenuHotkey;
        }

        //todo: click(x,y) (activate item under click)


        public void Update()
        {
            if (menuIsShown)
            {
                ProcessKeyboard();
            }
            ProcessMenuHotkey();
        }

        private void ProcessMenuHotkey()
        {
            if (menuHotkey != UnityEngine.KeyCode.None && UnityEngine.Input.GetKey(menuHotkey) && ignoreMenuHotkey < 1)
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
            else if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.Escape))
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
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.UpArrow) && ignoreUpArrow < 1)
            {
                MoveSelectionUp();
                ignoreUpArrow = menuLag;
            }
            else if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.UpArrow))
            {
                ignoreUpArrow = 0;
            }
            else if (ignoreUpArrow > -10)
            {
                ignoreUpArrow--;
            }

            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.DownArrow) && ignoreDownArrow < 1)
            {
                MoveSelectionDown();
                ignoreDownArrow = menuLag;
            }
            else if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.DownArrow))
            {
                ignoreDownArrow = 0;
            }
            else if (ignoreDownArrow > -10)
            {
                ignoreDownArrow--;
            }

            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.KeypadEnter) && ignoreEnter < 1)
            {
                ActivateCurrentMenuItem();
                ignoreEnter = menuLag;
            }
            else if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.KeypadEnter))
            {
                ignoreEnter = 0;
            }
            else if (ignoreEnter > -10)
            {
                ignoreEnter--;
            }
        }
        
        //big todo: turn this code into corresponding canvas layout
        
        /*

        public void CreateMenuRectangle()
        {
            menuArea = new Rect(0, 0, 0, 0);
            if (menuItems.Count > 0)
            {
                menuArea.height = (int)activeFont.MeasureString(menuItems[0].menuItemName).Y;
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


        public override void Draw(GameTime gameTime)
        {
            if (menuIsShown)
            {
                DrawMenu();
            }
        }


        public void DrawMenu()
        {
            
            int backBufferWidth = UnityEngine.Screen.width;
            int backBufferHeight = UnityEngine.Screen.height;
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
            int backBufferWidth = UnityEngine.Screen.width;
            int backBufferHeight = UnityEngine.Screen.height;
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
    */
    }
}
