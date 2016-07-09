using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using Next_Game.Cartographic;

namespace Next_Game
{
    public enum MenuMode {Main, Character, Debug} //distinct menu sets (Menu.cs)
    public enum ConsoleDisplay {Status, Input, Multi} //different console windows (Menu window handled independently by Menu.cs)

    public static class Program
    {
        // The screen height and width are in number of tiles
        /*private static readonly int _screenWidth = 60;
        private static readonly int _screenHeight = 70;
        private static int playerX = 20;
        private static int playerY = 20;*/
        private static int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;

        private static readonly int _rootWidth = 230;
        private static readonly int _rootHeight = 140;
        private static RLRootConsole _rootConsole; //main console

        private static readonly int _mapWidth = 100;
        private static readonly int _mapHeight = 100;
        private static RLConsole _mapConsole; //map

        private static readonly int _menuWidth = 100;
        private static readonly int _menuHeight = 20;
        private static RLConsole _menuConsole; //top left

        private static readonly int _statusWidth = 100;
        private static readonly int _statusHeight = 20;
        private static RLConsole _statusConsole; //bottom left

        private static readonly int _inputWidth = 130;
        private static readonly int _inputHeight = 20;
        private static RLConsole _inputConsole; //top right

        private static readonly int _multiWidth = 130;
        private static readonly int _multiHeight = 100;
        private static RLConsole _multiConsole; //middle right

        private static readonly int _messageWidth = 130;
        private static readonly int _messageHeight = 20;
        private static RLConsole _messageConsole; //bottom right

        private static int mapSize = 30;
        public static Game game;
        public static Menu menu;
        public static MessageLog messageLog;
        public static Map map;
        public static Network network;
        public static History history;
        public static World world;
        public static InfoChannel infoChannel;

        //flags
        private static bool renderRequired = true; //redraw Console?
        private static bool mouseOn = false; //receive mouse input?
        private static bool secondInput = false; //used for double input situations, eg. origin and destination
        private static MenuMode _menuMode = MenuMode.Main; //menu mode in operation (corresponds to enum above)

        //other
        private static RLKeyPress keyLast = null; //last known keypress
        private static Position posSelect1; //used for input of map positions
        private static Position posSelect2;
        private static int charIDSelected; //selected player character

        public static void Main(string[] args)
        {
            //initialise game objects
            messageLog = new MessageLog();
            game = new Game();
            map = new Map(mapSize, seed);
            map.InitialiseMap(4, 2);
            network = new Network(seed);
            network.InitialiseNetwork();
            History history = new History();
            world = new World();
            history.CreatePlayerCharacters(6);
            world.InitiatePlayerCharacters(history.GetPlayerCharacters(), 1);
            infoChannel = new InfoChannel();
            //set up menu
            menu = new Menu(4, 6);
            _menuMode = menu.SwitchMenuMode(MenuMode.Main);
            // This must be the exact name of the bitmap font file we are using or it will error.
            string fontFileName = "terminal8x8.png";
            // The title will appear at the top of the console window
            string consoleTitle = "Next Game Prototype v1.0";
            // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
            _mapConsole = new RLRootConsole(fontFileName, _mapWidth, _mapHeight, 8, 8, 1f, consoleTitle);
            _menuConsole = new RLRootConsole(fontFileName, _menuWidth, _menuHeight, 8, 8, 1f, consoleTitle);
            _statusConsole = new RLRootConsole(fontFileName, _statusWidth, _statusHeight, 8, 8, 1f, consoleTitle);
            _inputConsole = new RLRootConsole(fontFileName, _inputWidth, _inputHeight, 8, 8, 1f, consoleTitle);
            _multiConsole = new RLRootConsole(fontFileName, _multiWidth, _multiHeight, 8, 8, 1f, consoleTitle);
            _messageConsole = new RLRootConsole(fontFileName, _messageWidth, _messageHeight, 8, 8, 1f, consoleTitle);
            _rootConsole = new RLRootConsole(fontFileName, _rootWidth, _rootHeight, 8, 8, 1f, consoleTitle);

            //debug to set up consoles
            /*_mapConsole.SetBackColor(0, 0, _mapWidth, _mapHeight, RLColor.Gray);
            _menuConsole.SetBackColor(0, 0, _menuWidth, _menuHeight, RLColor.LightGray);
            _statusConsole.SetBackColor(0, 0, _menuWidth, _menuHeight, RLColor.LightGray);
            _inputConsole.SetBackColor(0, 0, _inputWidth, _inputHeight, RLColor.Gray);
            _multiConsole.SetBackColor(0, 0, _multiWidth, _multiHeight, RLColor.LightGray);
            _messageConsole.SetBackColor(0, 0, _messageWidth, _messageHeight, RLColor.Gray);*/

            // Set up handlers for RLNET's Update & Render events
           _rootConsole.Update += OnRootConsoleUpdate;
           _rootConsole.Render += OnRootConsoleRender;
            // Begin RLNET's game loop
           _rootConsole.Run();
            renderRequired = true;
        }


        // Event handler for RLNET's Update event
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            //game loop
            
            RLKeyPress keyPress =_rootConsole.Keyboard.GetKeyPress();
            //last used keypress
            if (keyPress != null)
            { keyLast = keyPress; }
            RLMouse mouse = _rootConsole.Mouse;
            bool mouseLeft = _rootConsole.Mouse.GetLeftClick();
            //mouse input
            if(mouseLeft == true)
            {
                //Mouse specific input OFF - generic location and party info
                if (mouseOn == false)
                {
                    int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                    infoChannel.SetInfoList(world.ShowLocationRL(locID), ConsoleDisplay.Multi);
                    renderRequired = true;
                }
                //Mouse specific input ON
                else if (mouseOn == true)
                {
                    //last pressed key indicates context of mouse press
                    switch (keyLast?.Key)
                    {
                        case RLKey.D:
                            switch (_menuMode)
                            {
                                case MenuMode.Debug:
                                    //debug route between two points
                                    renderRequired = true;
                                    //Origin location
                                    if (secondInput == false)
                                    {
                                        //valid location?
                                        int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                                        if (locID > 0)
                                        {
                                            string locName = world.GetLocationName(locID);
                                            infoChannel.AppendInfoList(locName, ConsoleDisplay.Input);
                                            posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                            infoChannel.AppendInfoList("Select DESTINATION Location by Mouse (press ESC to Exit)", ConsoleDisplay.Input);
                                            secondInput = true;
                                        }
                                    }
                                    //Destination location
                                    else if (secondInput == true)
                                    {
                                        //valid location?
                                        int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                                        if (locID > 0)
                                        {
                                            //process two positions to show on map.
                                            posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                            //check that the two coords aren't identical
                                            if ((posSelect1 != null && posSelect2 != null) && (posSelect1.PosX != posSelect2.PosX || posSelect1.PosY != posSelect2.PosY))
                                            {
                                                List<Route> listOfRoutes = network.GetRouteAnywhere(posSelect1, posSelect2);
                                                map.DrawRouteDebug(listOfRoutes);
                                            }
                                            secondInput = false;
                                            mouseOn = false;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case RLKey.G:
                            switch (_menuMode)
                            {
                                case MenuMode.Debug:
                                    renderRequired = true;
                                    //Origin location
                                    if (secondInput == false)
                                    {
                                        //valid location?
                                        int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                                        if (locID > 0)
                                        {
                                            string locName = world.GetLocationName(locID);
                                            infoChannel.AppendInfoList(locName, ConsoleDisplay.Input);
                                            posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                            infoChannel.AppendInfoList("Select DESTINATION Location by Mouse (press ESC to Exit)", ConsoleDisplay.Input);
                                            secondInput = true;
                                        }
                                    }
                                    //Destination location
                                    else if (secondInput == true)
                                    {
                                        //valid location?
                                        int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                                        if (locID > 0)
                                        {
                                            //process two positions to show on map.
                                            posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                            if ((posSelect1 != null && posSelect2 != null) && (posSelect1.PosX != posSelect2.PosX || posSelect1.PosY != posSelect2.PosY))
                                            {
                                                List<Route> listOfRoutes = network.GetRouteAnywhere(posSelect1, posSelect2);
                                                map.DrawRouteRL(listOfRoutes);
                                            }
                                            secondInput = false;
                                            mouseOn = false;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case RLKey.P:
                            switch (_menuMode)
                            {
                                case MenuMode.Character:
                                    //move Player character from A to B
                                    //valid location?
                                    int locID = map.GetLocationID(mouse.X, mouse.Y, true);
                                    if (locID > 0)
                                    {
                                        //process two positions to show on map.
                                        posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                        if (posSelect2 != null)
                                        {
                                            List<Position> pathToTravel = network.GetPathAnywhere(posSelect1, posSelect2);
                                            string infoText = world.InitiateMoveCharacters(charIDSelected, posSelect1, posSelect2, pathToTravel);
                                            messageLog.Add(infoText, world.GetGameTurn());
                                            infoChannel.AppendInfoList(infoText, ConsoleDisplay.Input);
                                            mouseOn = false;
                                            renderRequired = true;
                                            //autoswitch back to Main menu
                                            _menuMode = menu.SwitchMenuMode(MenuMode.Main);
                                        }
                                        else { infoChannel.AppendInfoList("ERROR: Not a valid Location", ConsoleDisplay.Input); }
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
            else if (keyPress != null)
            {
                //turn off mouse specific input whenever a key is pressed

                //which key pressed?
                switch (keyPress.Key)
                {
                    case RLKey.M:
                        //Draw Map: applies to all menu modes
                        map.UpdateMap(false, true);
                        renderRequired = true;
                        mouseOn = false;
                        break;
                    case RLKey.E:
                        //world.ShowPastEvents();
                        switch (_menuMode)
                        {
                            case MenuMode.Main:
                                renderRequired = true;
                                mouseOn = false;
                                break;
                        }
                        break;
                    case RLKey.C:
                        switch (_menuMode)
                        {
                            case MenuMode.Main:
                                //Show Player Characters
                                infoChannel.SetInfoList(world.ShowPlayerCharactersRL(), ConsoleDisplay.Multi);
                                renderRequired = true;
                                mouseOn = false;
                                break;
                        }
                        break;
                    case RLKey.D:
                        //List<Route> listOfRoutes_2 = network.RouteInput("D"); map.DrawRouteDebug(listOfRoutes_2);
                        renderRequired = true;
                        switch(_menuMode)
                            {
                            case MenuMode.Main:
                                //switch to Debug menu
                                _menuMode = menu.SwitchMenuMode(MenuMode.Debug);
                                
                                break;
                            case MenuMode.Debug:
                                //show debug route
                                List<string> inputList = new List<string>();
                                inputList.Add("--- Show the Route between two Locations");
                                inputList.Add("Select ORIGIN Location by Mouse (press ESC to Exit)");
                                infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                mouseOn = true;
                                break;
                            }
                        break;
                    case RLKey.G:
                        switch (_menuMode)
                        {
                            case MenuMode.Debug:
                                List<string> inputList = new List<string>();
                                inputList.Add("--- Show the Route between two Locations");
                                inputList.Add("Select ORIGIN Location by Mouse (press ESC to Exit)");
                                infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                renderRequired = true;
                                mouseOn = true;
                                break;
                        }
                        break;
                    case RLKey.R:
                        //Show all routes on the map in red
                        switch (_menuMode)
                        {
                            case MenuMode.Main:
                                break;
                            case MenuMode.Debug:
                                //show debug route
                                map.UpdateMap(true, false);
                                renderRequired = true;
                                mouseOn = true;
                                break;
                        }
                        break;
                    case RLKey.P:
                        switch (_menuMode)
                        {
                            case MenuMode.Character:
                                //move Player characters around map
                                List<string> charList = new List<string>();
                                charList.Add(world.GetCharacterRL(charIDSelected));
                                posSelect1 = world.GetCharacterLocationByPos(charIDSelected);
                                if (posSelect1 != null)
                                { charList.Add("Click on the Destination location or press [ESC] to cancel"); mouseOn = true; }
                                else
                                { charList.Add("The character is not currently at your disposal"); mouseOn = false; }
                                infoChannel.SetInfoList(charList, ConsoleDisplay.Input);
                                renderRequired = true;
                                break;
                        }
                        break;
                    //Player controlled character selected
                    case RLKey.Number1:
                    case RLKey.Number2:
                    case RLKey.Number3:
                    case RLKey.Number4:
                    case RLKey.Number5:
                    case RLKey.Number6:
                        switch (_menuMode)
                        {
                            case MenuMode.Main:
                                _menuMode = menu.SwitchMenuMode(MenuMode.Character);
                                charIDSelected = (int)keyPress.Key - 109; //based on a system where '1' is '110'
                                List<string> infoList = new List<string>();
                                infoList.Add(world.ShowSelectedCharacter(charIDSelected));
                                infoChannel.SetInfoList(infoList, ConsoleDisplay.Input);
                                renderRequired = true;
                                break;
                        }
                        break;
                    case RLKey.Enter:
                        world.IncrementGameTurn();
                        map.UpdatePlayers(world.MoveCharacters());
                        renderRequired = true;
                        mouseOn = false;
                        break;
                    case RLKey.Escape:
                        //exit mouse input 
                        if(mouseOn)
                        { mouseOn = false; }
                        //exit app
                        else
                        switch(_menuMode)
                            {
                                case MenuMode.Main:
                                    //exit application
                                    _rootConsole.Close();
                                    Environment.Exit(1);
                                    break;
                                default:
                                    //return to main menu from sub menus
                                    _menuMode = menu.SwitchMenuMode(MenuMode.Main);
                                    renderRequired = true;
                                    break;
                            }
                        break;
                }
            }
        }


        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            // Tell RLNET to draw the console that we set
            if (renderRequired == true)
            {
                //update status console
                infoChannel.SetInfoList(world.ShowPlayerCharactersRL(), ConsoleDisplay.Status);

                //draw to consoles
                map.DrawMapRL(_mapConsole);
                messageLog.DrawMessageLog(_messageConsole);
                menu.DrawMenuRL(_menuConsole);
                infoChannel.DrawInfoConsole(_inputConsole, ConsoleDisplay.Input);
                infoChannel.DrawInfoConsole(_multiConsole, ConsoleDisplay.Multi);
                infoChannel.DrawInfoConsole(_statusConsole, ConsoleDisplay.Status);

                //Blit to root Console
                RLConsole.Blit(_menuConsole, 0, 0, _menuWidth, _menuHeight,_rootConsole, 0, 0);
                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight,_rootConsole, 0, 20);
                RLConsole.Blit(_statusConsole, 0, 0, _statusWidth, _statusHeight, _rootConsole, 0, 120);
                RLConsole.Blit(_inputConsole, 0, 0, _inputWidth, _inputHeight,_rootConsole, 100, 0);
                RLConsole.Blit(_multiConsole, 0, 0, _multiWidth, _multiHeight,_rootConsole, 100, 20);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight,_rootConsole, 100, 120);
               _rootConsole.Draw();
                renderRequired = false;
            }
        }
    }
}
