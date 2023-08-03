namespace MattsMenuLibrary
{
    public delegate void MenuAction();
    
    public class MenuItem
    {
        public string menuItemName;
        public MenuAction Activate;

        public MenuItem(string menuItemName, MenuAction menuItemAction)
        {
            this.menuItemName = menuItemName;
            this.Activate = menuItemAction;
        }
    }
}