using RLNET;

namespace Next_Game
{

    //MenuMode used (defined in Game.cs) for unique menu set-ups - Main, Character, Debug, etc.

    

    public class Menu
    {
        int menuCatNum; //number of categories, each one a seperate vertical list on screen
        int menuSize; //max number of entries per category
        string[,] menuArrayText; // eg. '[M] Move'
        int[,] menuArrayStatus; //0 off, 1 on, or active
        string[] menuArrayCategories; //Category header texts, eg. 'Main Map ---'
        //display menu parameters
        int categories; //number of distinct groups (max 6 per group, each group with a header defined in menuArrayCategories
        int size; //menu depth (number of entries per group, or category.
        int margin; // margin offset from top and left of console
        int menuWidth; //width (in chars) between characters
        int menuSpacing; //lines between each menu item (vertically), 1 means no spacing
        RLColor menuColorFore; //menu catergory text colors
        RLColor menuColorBack;


        public Menu(int categories, int size)
        {
            this.categories = categories;
            this.size = size;
            margin = 2;
            menuWidth = 24;
            menuSpacing = 2;
            menuCatNum = categories;
            menuSize = size;
            menuColorFore = RLColor.White;
            menuColorBack = RLColor.Black;
            //duplicate sized arrays
            menuArrayText = new string[categories, size];
            menuArrayStatus = new int[categories, size];
            //category array
            menuArrayCategories = new string[categories];
            //menu status set to active (except Character Menu)
            for(int row = 0; row < categories; row++)
            {
                for(int column = 0; column < size; column++)
                { menuArrayStatus[row, column] = 1; }
            }
        }

        /// <summary>
        /// sets up a new menu structure depending on selected MenuMode
        /// </summary>
        /// <param name="menuMode"></param>
        public MenuMode SwitchMenuMode(MenuMode menuMode)
        {
            //menu status set to active
            for (int row = 0; row < categories; row++)
            {
                //blank all categories
                menuArrayCategories[row] = null;
                for (int column = 0; column < size; column++)
                {
                    //blank all texts
                    menuArrayText[row, column] = null;
                    //all options set to active
                    menuArrayStatus[row, column] = 1;
                }
            }
            //change menu structure depending on mode (NOTE: make sure each menu structure has a means of returning to the main menu)
            switch (menuMode)
            {
                case MenuMode.Main:
                    menuColorFore = RLColor.Black;
                    menuColorBack = RLColor.LightGray;
                    //input categories
                    menuArrayCategories[0] = "Main MENU ---";
                    menuArrayCategories[1] = "Info ---";
                    menuArrayCategories[3] = "Switch ---";
                    //Main menu commands
                    menuArrayText[0, 0] = "[M] Map";
                    menuArrayText[0, 1] = "[C] Crow";
                    menuArrayText[0, 2] = "[ENTER] End Turn";
                    //Info category commands
                    menuArrayText[1, 0] = "[P] Show Player Actors";
                    menuArrayText[1, 1] = "[E] Show Messages";
                    menuArrayText[1, 2] = "[H] Show House";
                    menuArrayText[1, 3] = "[G] Show Generator Stats";
                    menuArrayText[1, 4] = "[A] Show Actor";
                    menuArrayText[1, 5] = "[I] Show Followers on Map";
                    //sub Menu commands
                    menuArrayText[3, 0] = "[1..9] Character MENU";
                    menuArrayText[3, 1] = "[D] Debug MENU";
                    menuArrayText[3, 2] = "[R] Records MENU";
                    menuArrayText[3, 3] = "[L] Lore MENU";
                    menuArrayText[3, 4] = "[X] Quit";
                    break;
                case MenuMode.Actor_Active:
                    menuColorFore = RLColor.Blue;
                    menuColorBack = RLColor.LightGray;
                    //input categories
                    menuArrayCategories[0] = "Character MENU ---";
                    menuArrayCategories[3] = "Switch ---";
                    //Character menu commands
                    menuArrayText[0, 0] = "[M] Map";
                    menuArrayText[0, 1] = "[P] Move Player Character";

                    menuArrayText[3, 0] = "[ESC] Main Menu";
                    menuArrayText[3, 1] = "[ENTER] End Turn";
                    break;
                case MenuMode.Record:
                    menuColorFore = RLColor.Brown;
                    menuColorBack = RLColor.LightGray;
                    //input categories
                    menuArrayCategories[0] = "Records MENU ---";
                    menuArrayCategories[3] = "Switch ---";
                    //Record menu commands
                    menuArrayText[0, 0] = "[A] All";
                    menuArrayText[0, 1] = "[C] Custom";
                    menuArrayText[0, 2] = "[D] Dead Actors";
                    menuArrayText[0, 3] = "[G] Marriages";
                    menuArrayText[0, 4] = "[K] Kingdom Events";

                    menuArrayText[3, 0] = "[ESC] Main Menu";
                    menuArrayText[3, 1] = "[ENTER] End Turn";
                    break;
                case MenuMode.Lore:
                    menuColorFore = RLColor.Brown;
                    menuColorBack = RLColor.LightGray;
                    //input categories
                    menuArrayCategories[0] = "Lore MENU ---";
                    menuArrayCategories[3] = "Switch ---";
                    //Lore menu commands
                    menuArrayText[0, 0] = "[U] Uprising";
                    menuArrayText[0, 1] = "[F] Fate of Royals";

                    menuArrayText[3, 0] = "[ESC] Main Menu";
                    menuArrayText[3, 1] = "[ENTER] End Turn";
                    break;
                case MenuMode.Debug:
                    menuColorFore = RLColor.Red;
                    menuColorBack = RLColor.LightGray;
                    //input categories
                    menuArrayCategories[0] = "Debug Map ---";
                    menuArrayCategories[1] = "Info ---";
                    menuArrayCategories[3] = "Switch ---";
                    //Debug menu commands
                    menuArrayText[0, 0] = "[G] Draw Route";
                    menuArrayText[0, 1] = "[D] Route Debug";
                    menuArrayText[0, 2] = "[R] Routes";
                    menuArrayText[0, 3] = "[M] Map";

                    menuArrayText[1, 0] = "[S] Show Secrets";
                    menuArrayText[1, 1] = "[E] Show Errors";
                    menuArrayText[1, 2] = "[T] Show Timers";
                    menuArrayText[1, 3] = "[P] Show Duplicates";
                    menuArrayText[1, 4] = "[K] Show Old King's House";
                    menuArrayText[1, 5] = "[H] Show House Relations";

                    menuArrayText[3, 0] = "[ESC] Main Menu";
                    menuArrayText[3, 1] = "[ENTER] End Turn";
                    break;
            }
            return menuMode;
        }

        public void DrawMenuRL(RLConsole menuConsole)
        {
            menuConsole.Clear();
            string cmdOption;
            RLColor optionColor;
            //outer loop (categories are a group of related commands)
            for(int cat = 0; cat < menuCatNum; cat++)
            {
                //only print category if present
                if (menuArrayCategories[cat] != null)
                { menuConsole.Print(cat * menuWidth + margin, margin, menuArrayCategories[cat], menuColorFore, menuColorBack); }
                //innner loop (command for this category)
                for (int cmd = 0; cmd < menuSize; cmd++)
                {
                    optionColor = RLColor.White;
                    //only print if text present
                    cmdOption = menuArrayText[cat, cmd];
                    if (cmdOption != null)
                    {
                        //active (white), inactive (gray)
                        if( menuArrayStatus[cat, cmd] == 0)
                        { optionColor = RLColor.Gray; }
                        menuConsole.Print(cat * menuWidth + margin, cmd * menuSpacing + menuSpacing + margin, cmdOption, optionColor, RLColor.Black);
                    }
                }
            }
        }


    }

    
}