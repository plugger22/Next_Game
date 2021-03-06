﻿using RLNET;

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
            if (Game.gameAct == Act.One)
            {
                //change menu structure depending on mode (NOTE: make sure each menu structure has a means of returning to the main menu)
                switch (menuMode)
                {
                    //Player as Usurper
                    case MenuMode.Main:
                        menuColorFore = RLColor.White;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Main MENU ---";
                        menuArrayCategories[1] = "Info ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main menu commands
                        menuArrayText[0, 0] = "[C] Crow";
                        menuArrayText[0, 1] = "[ENTER] End Turn";
                        menuArrayText[0, 2] = "[I] Toggle Info";
                        menuArrayText[0, 3] = "[U] Toggle Disguise";
                        //Info category commands
                        menuArrayText[1, 0] = "[P] Show Player Actors";
                        menuArrayText[1, 1] = "[M] Show Messages";
                        menuArrayText[1, 2] = "[H] Show House";
                        menuArrayText[1, 3] = "[G] Show Generator Stats";
                        menuArrayText[1, 4] = "[A] Show Actor";
                        menuArrayText[1, 5] = "[E] Show Enemies";
                        //Switch Menu commands
                        menuArrayText[3, 0] = "[1..9] Character MENU";
                        menuArrayText[3, 1] = "[D] Debug MENU";
                        menuArrayText[3, 2] = "[O] God MENU";
                        menuArrayText[3, 3] = "[R] Reference MENU";
                        menuArrayText[3, 4] = "[B] Balance MENU";
                        menuArrayText[3, 5] = "[K] King MENU";
                        //menuArrayText[3, 4] = "[L] Lore MENU";
                        menuArrayText[3, 6] = "[X] Quit";
                        break;
                    case MenuMode.Actor_Control:
                        menuColorFore = RLColor.Blue;
                        menuColorBack = RLColor.LightGray;
                        //input categories
                        menuArrayCategories[0] = "Character MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Character menu commands
                        menuArrayText[0, 0] = "[M] Map";
                        menuArrayText[0, 1] = "[P] Move Player Actor";
                        //switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Reference:
                        menuColorFore = RLColor.LightMagenta;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Records MENU ---";
                        menuArrayCategories[1] = "Rumours MENU ---";
                        menuArrayCategories[2] = "Lore MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Record menu commands
                        menuArrayText[0, 0] = "[A] All";
                        menuArrayText[0, 1] = "[C] Custom";
                        menuArrayText[0, 2] = "[D] Dead Actors";
                        menuArrayText[0, 3] = "[G] Marriages";
                        menuArrayText[0, 4] = "[K] Kingdom Events";
                        menuArrayText[0, 5] = "[H] Horses";
                        //Rumours menu commands
                        menuArrayText[1, 0] = "[R] All";
                        menuArrayText[1, 1] = "[S] Show Rumour";
                        menuArrayText[1, 2] = "[E] Show Enemies";
                        //Lore menu commands
                        menuArrayText[2, 0] = "[U] Uprising";
                        menuArrayText[2, 1] = "[F] Fate of Royals";
                        //switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.God:
                        menuColorFore = Color._godMode;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "God MENU ---";
                        menuArrayCategories[1] = "Player Menu ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main God Menu
                        menuArrayText[0, 0] = "[A] Toggle Act";
                        //Player God Menu
                        menuArrayText[1, 0] = "[P] Move Player";
                        menuArrayText[1, 1] = "[K] Known Status";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.King:
                        menuColorFore = RLColor.LightCyan;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "King MENU ---";
                        //menuArrayCategories[1] = "Player Menu ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main King Menu
                        menuArrayText[0, 0] = "[R] Relationships";
                        menuArrayText[0, 1] = "[F] Finances";
                        menuArrayText[0, 2] = "[C} Council";
                        menuArrayText[0, 3] = "[P] Policies";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Balance:
                        menuColorFore = RLColor.LightBlue;
                        menuColorBack = RLColor.LightGray;
                        //input categories
                        menuArrayCategories[0] = "Food MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Food menu commands
                        menuArrayText[0, 0] = "[A] Surpluses";
                        menuArrayText[0, 1] = "[B] Deficits";
                        menuArrayText[0, 2] = "[C} Houses";
                        menuArrayText[0, 3] = "[D] Branches";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Debug:
                        menuColorFore = RLColor.LightRed;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Debug Map ---";
                        menuArrayCategories[1] = "Info ---";
                        menuArrayCategories[2] = "Spy ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Debug Map commands
                        menuArrayText[0, 0] = "[A] Draw Route";
                        menuArrayText[0, 1] = "[D] Route Debug";
                        menuArrayText[0, 2] = "[R] Routes";
                        menuArrayText[0, 3] = "[M] Map Toggle";
                        menuArrayText[0, 4] = "[I] Info Toggle";
                        menuArrayText[0, 5] = "[G] Show GameVars";
                        menuArrayText[0, 6] = "[C] Show GameStats";
                        //Debug Info commands
                        menuArrayText[1, 0] = "[S] Show Secrets";
                        menuArrayText[1, 1] = "[L] Show Items";
                        menuArrayText[1, 2] = "[E] Show Errors";
                        menuArrayText[1, 3] = "[T] Show Timers";
                        menuArrayText[1, 4] = "[P] Show Dup's";
                        menuArrayText[1, 5] = "[K] Show Old K's Hse";
                        menuArrayText[1, 6] = "[H] Show Hse Rels";
                        menuArrayText[1, 7] = "[Q] Show Enemies";
                        //Debug Spy commands
                        menuArrayText[2, 0] = "[U] Show All";
                        menuArrayText[2, 1] = "[V] Show Actor";
                        menuArrayText[2, 2] = "[Y] Show Active";
                        menuArrayText[2, 3] = "[Z] Show Enemies";
                        //Debug Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                }
            }
            else if (Game.gameAct == Act.Two)
            {
                //Player as King
                switch (menuMode)
                {
                    case MenuMode.Main:
                        menuColorFore = RLColor.White;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Main MENU ---";
                        menuArrayCategories[1] = "Info ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main menu commands
                        menuArrayText[0, 0] = "[C] Crow";
                        menuArrayText[0, 1] = "[ENTER] End Turn";
                        menuArrayText[0, 2] = "[I] Toggle Info";
                        //Info category commands
                        menuArrayText[1, 0] = "[P] Show Inquisitors";
                        menuArrayText[1, 1] = "[M] Show Messages";
                        menuArrayText[1, 2] = "[H] Show House";
                        menuArrayText[1, 3] = "[G] Show Generator Stats";
                        menuArrayText[1, 4] = "[A] Show Actor";
                        menuArrayText[1, 5] = "[E] Show Enemies";
                        //Switch Menu commands
                        menuArrayText[3, 0] = "[1..9] Character MENU";
                        menuArrayText[3, 1] = "[D] Debug MENU";
                        menuArrayText[3, 2] = "[O] God MENU";
                        menuArrayText[3, 3] = "[R] Reference MENU";
                        menuArrayText[3, 4] = "[B] Balance MENU";
                        menuArrayText[3, 5] = "[K] King MENU";
                        //menuArrayText[3, 4] = "[L] Lore MENU";
                        menuArrayText[3, 6] = "[X] Quit";
                        break;
                    case MenuMode.Actor_Control:
                        menuColorFore = RLColor.Blue;
                        menuColorBack = RLColor.LightGray;
                        //input categories
                        menuArrayCategories[0] = "Character MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Character menu commands
                        menuArrayText[0, 0] = "[M] Map";
                        menuArrayText[0, 1] = "[P] Move Player Actor";
                        //switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Reference:
                        menuColorFore = RLColor.LightMagenta;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Records MENU ---";
                        menuArrayCategories[1] = "Rumours MENU ---";
                        menuArrayCategories[2] = "Lore MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Record menu commands
                        menuArrayText[0, 0] = "[A] All";
                        menuArrayText[0, 1] = "[C] Custom";
                        menuArrayText[0, 2] = "[D] Dead Actors";
                        menuArrayText[0, 3] = "[G] Marriages";
                        menuArrayText[0, 4] = "[K] Kingdom Events";
                        menuArrayText[0, 5] = "[H] Horses";
                        //Rumours menu commands
                        menuArrayText[1, 0] = "[R] All";
                        menuArrayText[1, 1] = "[S] Show Rumour";
                        menuArrayText[1, 2] = "[E] Show Enemies";
                        //Lore menu commands
                        menuArrayText[2, 0] = "[U] Uprising";
                        menuArrayText[2, 1] = "[F] Fate of Royals";
                        //switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.God:
                        menuColorFore = Color._godMode;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "God MENU ---";
                        menuArrayCategories[1] = "Player Menu ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main God Menu
                        menuArrayText[0, 0] = "[A] Toggle Act";
                        //Player God Menu
                        menuArrayText[1, 0] = "[P] Move Player";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.King:
                        menuColorFore = RLColor.LightCyan;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "King MENU ---";
                        //menuArrayCategories[1] = "Player Menu ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Main King Menu
                        menuArrayText[0, 0] = "[R] Relationships";
                        menuArrayText[0, 1] = "[F] Finances";
                        menuArrayText[0, 2] = "[C} Council";
                        menuArrayText[0, 3] = "[P] Policies";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Balance:
                        menuColorFore = RLColor.LightBlue;
                        menuColorBack = RLColor.LightGray;
                        //input categories
                        menuArrayCategories[0] = "Food MENU ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Food menu commands
                        menuArrayText[0, 0] = "[A] Surpluses";
                        menuArrayText[0, 1] = "[B] Deficits";
                        menuArrayText[0, 2] = "[C} Houses";
                        menuArrayText[0, 3] = "[D] Branches";
                        //Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                    case MenuMode.Debug:
                        menuColorFore = RLColor.LightRed;
                        menuColorBack = RLColor.Gray;
                        //input categories
                        menuArrayCategories[0] = "Debug Map ---";
                        menuArrayCategories[1] = "Info ---";
                        menuArrayCategories[2] = "Spy ---";
                        menuArrayCategories[3] = "Switch ---";
                        //Debug Map commands
                        menuArrayText[0, 0] = "[G] Draw Route";
                        menuArrayText[0, 1] = "[D] Route Debug";
                        menuArrayText[0, 2] = "[R] Routes";
                        menuArrayText[0, 3] = "[M] Map Toggle";
                        menuArrayText[0, 4] = "[I] Info Toggle";
                        menuArrayText[0, 5] = "[A] Show GameVars";
                        menuArrayText[0, 6] = "[C] Show GameStats";
                        //Debug Info commands
                        menuArrayText[1, 0] = "[S] Show Secrets";
                        menuArrayText[1, 1] = "[L] Show Items";
                        menuArrayText[1, 2] = "[E] Show Errors";
                        menuArrayText[1, 3] = "[T] Show Timers";
                        menuArrayText[1, 4] = "[P] Show Dup's";
                        menuArrayText[1, 5] = "[K] Show Old K's Hse";
                        menuArrayText[1, 6] = "[H] Show Hse Rels";
                        menuArrayText[1, 7] = "[Q] Show Enemies";
                        //Debug Spy commands
                        menuArrayText[2, 0] = "[U] Show All";
                        menuArrayText[2, 1] = "[V] Show Actor";
                        menuArrayText[2, 2] = "[Y] Show Active";
                        menuArrayText[2, 3] = "[Z] Show Enemies";
                        //Debug Switch commands
                        menuArrayText[3, 0] = "[ESC] Main Menu";
                        menuArrayText[3, 1] = "[ENTER] End Turn";
                        break;
                }
            }
            else { Game.SetError(new Error(322, $"Invalid Game.gameAct \"{Game.gameAct}\"")); }
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