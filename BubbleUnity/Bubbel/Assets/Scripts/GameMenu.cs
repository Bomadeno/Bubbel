using System.Collections.Generic;
using Color = UnityEngine.Color;
using Rect = UnityEngine.Rect;


namespace MattsMenuLibrary
{
    public class GameMenu : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] private UnityEngine.UI.Image background;
        
        //internal variables for getting stuff done
        private List<MenuItem> menuItems;
        //-1 indicates nothing is selected
        private int currentlySelected;
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
    }
}
