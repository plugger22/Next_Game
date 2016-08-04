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

    public static class Game
    {
        // The screen height and width are in number of tiles

        private static int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
        //DEBUG: insert seed here to test a particular map
        //private static int seed = 34190;
        

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
        public static int gameTurn = 0; //each turn represents a day
        public static int gameStart = 1200; //starting year for 1st generation
        public static int gameYear = 1200; //current game year
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
        private static bool multiInput = false; //flag to allow multiple key inputs
        private static int multiCaller = 0; //each instance that calls multi key input has a unique ID which is > 0
        private static string multiData = null; //multi key input is stored here
        private static int inputState = 0; //used to differentiate suquential levels of input for individual commands
        private static MenuMode _menuMode = MenuMode.Main; //menu mode in operation (corresponds to enum above)

        //other
        private static RLKeyPress keyLast = null; //last known keypress
        private static Position posSelect1; //used for input of map positions
        private static Position posSelect2;
        private static int charIDSelected; //selected player character

        public static void Main(string[] args)
        {
            Console.SetWindowSize(100, 80);
            Console.WriteLine("Seed: {0}", seed);
            //initialise game objects
            messageLog = new MessageLog();
            map = new Map(mapSize, seed);
            map.InitialiseMap(4, 2);
            network = new Network(seed);
            network.InitialiseNetwork();
            history = new History(seed);
            history.InitialiseHistory(network.GetNumUniqueHouses());
            history.CreatePlayerCharacters(6);
            world = new World();
            world.InitiatePlayerActors(history.GetPlayerCharacters(), 1);
            network.UpdateHouses(history.GetGreatHouses());
            world.InitialiseHouses();
            //---interim history.cs step needed to update History of houses
            infoChannel = new InfoChannel();
            world.ShowGeneratorStatsRL();
            messageLog.Add(new Snippet($"Game world created with seed {seed}"), gameTurn);
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


        /// <summary>
        /// Event handler for RLNET's Update event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            //game loop
            RLKeyPress keyPress =_rootConsole.Keyboard.GetKeyPress();
            //last used keypress
            if (keyPress != null)
            { keyLast = keyPress; }
            RLMouse mouse = _rootConsole.Mouse;
            bool mouseLeft = _rootConsole.Mouse.GetLeftClick();
            bool mouseRight = _rootConsole.Mouse.GetRightClick();
            //
            // Multi Key input ---
            //
            if (multiInput == true && keyPress != null)
            {
                bool complete = MultiKeyInput(keyPress);
                renderRequired = true;
                if (complete == true)
                {
                    switch(multiCaller)
                    {
                        case 1:
                            //Show Actor (input actorID)
                            infoChannel.SetInfoList(world.ShowActorRL(Convert.ToInt32(multiData)), ConsoleDisplay.Multi);
                            break;

                    }
                    //reset
                    multiCaller = 0;
                    multiData = null;
                }
            }
            //normal mouse and keyboard input
            else
            {
                //
                // MOUSE input ---
                //
                if (mouseLeft == true || mouseRight == true)
                {
                    //Mouse specific input OFF - generic location and party info
                    if (mouseOn == false)
                    {
                        int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
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
                                        if (inputState == 1)
                                        {
                                            //valid location?
                                            int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                            if (locID > 0)
                                            {
                                                string locName = world.GetLocationName(locID);
                                                infoChannel.AppendInfoList(new Snippet(locName), ConsoleDisplay.Input);
                                                posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                infoChannel.AppendInfoList(new Snippet("Select DESTINATION Location by Mouse (press ESC to Exit)"), ConsoleDisplay.Input);
                                                inputState = 2;
                                            }
                                        }
                                        //Destination location
                                        else if (inputState == 2)
                                        {
                                            //valid location?
                                            int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                            if (locID > 0)
                                            {
                                                //process two positions to show on map.
                                                posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                //check that the two coords aren't identical
                                                if ((posSelect1 != null && posSelect2 != null) && (posSelect1.PosX != posSelect2.PosX || posSelect1.PosY != posSelect2.PosY))
                                                {
                                                    List<Route> listOfRoutes = network.GetRouteAnywhere(posSelect1, posSelect2);
                                                    map.DrawRouteDebug(listOfRoutes);
                                                    infoChannel.AppendInfoList(new Snippet(network.ShowRouteDetails(listOfRoutes)), ConsoleDisplay.Input);
                                                }
                                                inputState = 0;
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
                                        if (inputState == 1)
                                        {
                                            //valid location?
                                            int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                            if (locID > 0)
                                            {
                                                string locName = world.GetLocationName(locID);
                                                infoChannel.AppendInfoList(new Snippet(locName), ConsoleDisplay.Input);
                                                posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                infoChannel.AppendInfoList(new Snippet("Select DESTINATION Location by Mouse (press ESC to Exit)"), ConsoleDisplay.Input);
                                                inputState = 2;
                                            }
                                        }
                                        //Destination location
                                        else if (inputState == 2)
                                        {
                                            //valid location?
                                            int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                            if (locID > 0)
                                            {
                                                //process two positions to show on map.
                                                posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                if ((posSelect1 != null && posSelect2 != null) && (posSelect1.PosX != posSelect2.PosX || posSelect1.PosY != posSelect2.PosY))
                                                {
                                                    List<Route> listOfRoutes = network.GetRouteAnywhere(posSelect1, posSelect2);
                                                    map.DrawRouteRL(listOfRoutes);
                                                    infoChannel.AppendInfoList(new Snippet(network.ShowRouteDetails(listOfRoutes)), ConsoleDisplay.Input);
                                                }
                                                inputState = 0;
                                                mouseOn = false;
                                            }
                                        }
                                        break;
                                }
                                break;
                            case RLKey.H:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        renderRequired = true;
                                        //Show House Details
                                        if (inputState == 1)
                                        {
                                            //valid location?
                                            int houseID = map.GetMapInfo(MapLayer.Houses, mouse.X, mouse.Y, true);
                                            if (houseID > 0)
                                            { infoChannel.SetInfoList(world.ShowHouseRL(houseID), ConsoleDisplay.Multi); }
                                        }
                                        mouseOn = false;
                                        break;
                                }
                                break;
                            case RLKey.P:
                                switch (_menuMode)
                                {
                                    case MenuMode.Character:
                                        //move Player character from A to B
                                        renderRequired = true;
                                        //valid location?
                                        if (inputState == 1)
                                        {
                                            int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                            if (locID > 0)
                                            {
                                                //process two positions to show on map.
                                                posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                if (posSelect2 != null)
                                                {
                                                    string infoString = string.Format("Journey from {0} to {1}? [Left Click] to confirm, [Right Click] to Cancel.",
                                                        network.GetLocationName(posSelect1), network.GetLocationName(posSelect2));
                                                    infoChannel.AppendInfoList(new Snippet(infoString), ConsoleDisplay.Input);
                                                    inputState = 2;
                                                }
                                            }
                                            else
                                            {
                                                //cancel journey
                                                if (mouseRight == true)
                                                { infoChannel.AppendInfoList(new Snippet("Journey Cancelled!"), ConsoleDisplay.Input); inputState = 0; mouseOn = false; }
                                            }
                                        }
                                        else if (inputState == 2)
                                        {
                                            if (mouseLeft == true)
                                            {
                                                List<Position> pathToTravel = network.GetPathAnywhere(posSelect1, posSelect2);
                                                string infoText = world.InitiateMoveCharacters(charIDSelected, posSelect1, posSelect2, pathToTravel);
                                                messageLog.Add(new Snippet(infoText), gameTurn);
                                                infoChannel.AppendInfoList(new Snippet(infoText), ConsoleDisplay.Input);
                                                //show route
                                                map.UpdateMap();
                                                map.DrawRoutePath(pathToTravel);
                                            }
                                            else if (mouseRight == true)
                                            { infoChannel.AppendInfoList(new Snippet("Journey Cancelled!"), ConsoleDisplay.Input); }
                                            inputState = 0;
                                            mouseOn = false;
                                            //autoswitch back to Main menu
                                            _menuMode = menu.SwitchMenuMode(MenuMode.Main);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
                //
                // KEYBOARD Input ---
                //
                else if (keyPress != null)
                {
                    //turn off mouse specific input whenever a key is pressed
                    renderRequired = true;
                    mouseOn = false;
                    //which key pressed?
                    switch (keyPress.Key)
                    {
                        case RLKey.A:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    //show Actor
                                    infoChannel.SetInfoList(new List<Snippet>(), ConsoleDisplay.Input);
                                    infoChannel.AppendInfoList(new Snippet("---Input Actor ID ", RLColor.Magenta, RLColor.Black), ConsoleDisplay.Input);
                                    infoChannel.AppendInfoList(new Snippet("Press ENTER when done, ESC to exit"), ConsoleDisplay.Input);
                                    multiInput = true;
                                    multiCaller = 1;
                                    break;
                            }
                            break;
                        case RLKey.E:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    //Show Full message log
                                    infoChannel.SetInfoList(messageLog.GetMessageList(), ConsoleDisplay.Multi);
                                    infoChannel.InsertHeader(new Snippet("--- Message Log ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                    break;
                            }
                            break;
                        case RLKey.C:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    break;
                            }
                            break;
                        case RLKey.D:
                            //List<Route> listOfRoutes_2 = network.RouteInput("D"); map.DrawRouteDebug(listOfRoutes_2);
                            renderRequired = true;
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    //switch to Debug menu
                                    _menuMode = menu.SwitchMenuMode(MenuMode.Debug);
                                    break;
                                case MenuMode.Debug:
                                    //show debug route
                                    List<Snippet> inputList = new List<Snippet>();
                                    inputList.Add(new Snippet("--- Show the Route between two Locations", RLColor.Magenta, RLColor.Black));
                                    inputList.Add(new Snippet("Select ORIGIN Location by Mouse (press ESC to Exit)"));
                                    infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                    mouseOn = true;
                                    inputState = 1;
                                    break;
                            }
                            break;
                        case RLKey.G:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    world.ShowGeneratorStatsRL();
                                    break;
                                case MenuMode.Debug:
                                    List<Snippet> inputList = new List<Snippet>();
                                    inputList.Add(new Snippet("--- Show the Route between two Locations", RLColor.Magenta, RLColor.Black));
                                    inputList.Add(new Snippet("Select ORIGIN Location by Mouse (press ESC to Exit)"));
                                    infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                    inputState = 1;
                                    mouseOn = true;
                                    break;
                            }
                            break;
                        case RLKey.H:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    //Show House Details
                                    List<Snippet> inputList = new List<Snippet>();
                                    inputList.Add(new Snippet("--- Show House Details", RLColor.Magenta, RLColor.Black));
                                    inputList.Add(new Snippet("Select Location by Mouse (press ESC to Exit)"));
                                    infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                    inputState = 1;
                                    mouseOn = true;
                                    break;
                            }
                            break;
                        case RLKey.M:
                            //Draw Map: applies to all menu modes
                            map.UpdateMap(false, true);
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
                                    mouseOn = true;
                                    break;
                            }
                            break;
                        case RLKey.P:
                            switch (_menuMode)
                            {
                                case MenuMode.Main:
                                    //Show Player Characters
                                    infoChannel.SetInfoList(world.ShowPlayerActorsRL(), ConsoleDisplay.Multi);
                                    break;
                                case MenuMode.Character:
                                    //move Player characters around map
                                    List<Snippet> charList = new List<Snippet>();
                                    charList.Add(world.GetCharacterRL(charIDSelected));
                                    posSelect1 = world.GetActiveActorLocationByPos(charIDSelected);
                                    if (posSelect1 != null)
                                    { charList.Add(new Snippet("Click on the Destination location or press [Right Click] to cancel")); mouseOn = true; }
                                    else
                                    { charList.Add(new Snippet("The character is not currently at your disposal")); mouseOn = false; }
                                    infoChannel.SetInfoList(charList, ConsoleDisplay.Input);
                                    inputState = 1;
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
                                    List<Snippet> infoList = new List<Snippet>();
                                    infoList.Add(world.ShowSelectedCharacter(charIDSelected));
                                    infoChannel.SetInfoList(infoList, ConsoleDisplay.Input);
                                    break;
                            }
                            break;
                        case RLKey.Enter:
                            map.UpdateMap();
                            map.UpdatePlayers(world.MoveCharacters());
                            infoChannel.SetInfoList(new List<Snippet>(), ConsoleDisplay.Input);
                            infoChannel.AppendInfoList(new Snippet(ShowDate(), RLColor.Yellow, RLColor.Black), ConsoleDisplay.Input);
                            gameTurn++;
                            break;
                        case RLKey.X:
                            //exit application from Main Menu
                            if (_menuMode == MenuMode.Main)
                            {
                                _rootConsole.Close();
                                Environment.Exit(1);
                            }
                            break;
                        case RLKey.Escape:
                            //clear input console
                            infoChannel.SetInfoList(new List<Snippet>(), ConsoleDisplay.Input);
                            //exit mouse input 
                            if (mouseOn == true)
                            { mouseOn = false; }
                            //revert back to main menu
                            else
                            {
                                if (_menuMode != MenuMode.Main)
                                {
                                    //return to main menu from sub menus
                                    _menuMode = menu.SwitchMenuMode(MenuMode.Main);
                                }
                            }
                            break;
                    }
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
                infoChannel.SetInfoList(world.ShowPlayerActorsRL(), ConsoleDisplay.Status);

                //draw to consoles
                map.DrawMapRL(_mapConsole);
                messageLog.DrawMessageQueue(_messageConsole);
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

        /// <summary>
        /// allows multi key input. input stored in a string 'dataInput', process ended once enter pressed. Handles alpha and numeric input.
        /// </summary>
        /// <param name="keyPress"></param>
        /// <returns>false if input ongoing, true if completed (ENTER pressed)</returns>
        private static bool MultiKeyInput(RLKeyPress keyPress, bool numInput = true, bool alphaInput = false)
            {
            char data = '?';
            bool inputComplete = false;
            int inputType = 0; //1 - numeric, 2 - alphabetic
            switch(keyPress.Key)
            {
                case RLKey.Number0:
                    data = '0';
                    inputType = 1;
                    break;
                case RLKey.Number1:
                    data = '1';
                    inputType = 1;
                    break;
                case RLKey.Number2:
                    data = '2';
                    inputType = 1;
                    break;
                case RLKey.Number3:
                    data = '3';
                    inputType = 1;
                    break;
                case RLKey.Number4:
                    data = '4';
                    inputType = 1;
                    break;
                case RLKey.Number5:
                    data = '5';
                    inputType = 1;
                    break;
                case RLKey.Number6:
                    data = '6';
                    inputType = 1;
                    break;
                case RLKey.Number7:
                    data = '7';
                    inputType = 1;
                    break;
                case RLKey.Number8:
                    data = '8';
                    inputType = 1;
                    break;
                case RLKey.Number9:
                    data = '9';
                    inputType = 1;
                    break;
                case RLKey.A:
                    data = 'A';
                    inputType = 2;
                    break;
                case RLKey.B:
                    data = 'B';
                    inputType = 2;
                    break;
                case RLKey.C:
                    data = 'C';
                    inputType = 2;
                    break;
                case RLKey.D:
                    data = 'D';
                    inputType = 2;
                    break;
                case RLKey.E:
                    data = 'E';
                    inputType = 2;
                    break;
                case RLKey.F:
                    data = 'F';
                    inputType = 2;
                    break;
                case RLKey.G:
                    data = 'G';
                    inputType = 2;
                    break;
                case RLKey.H:
                    data = 'H';
                    inputType = 2;
                    break;
                case RLKey.I:
                    data = 'I';
                    inputType = 2;
                    break;
                case RLKey.J:
                    data = 'J';
                    inputType = 2;
                    break;
                case RLKey.K:
                    data = 'K';
                    inputType = 2;
                    break;
                case RLKey.L:
                    data = 'L';
                    inputType = 2;
                    break;
                case RLKey.M:
                    data = 'M';
                    inputType = 2;
                    break;
                case RLKey.N:
                    data = 'N';
                    inputType = 2;
                    break;
                case RLKey.O:
                    data = 'O';
                    inputType = 2;
                    break;
                case RLKey.P:
                    data = 'P';
                    inputType = 2;
                    break;
                case RLKey.Q:
                    data = 'Q';
                    inputType = 2;
                    break;
                case RLKey.R:
                    data = 'R';
                    inputType = 2;
                    break;
                case RLKey.S:
                    data = 'S';
                    inputType = 2;
                    break;
                case RLKey.T:
                    data = 'T';
                    inputType = 2;
                    break;
                case RLKey.U:
                    data = 'U';
                    inputType = 2;
                    break;
                case RLKey.V:
                    data = 'V';
                    inputType = 2;
                    break;
                case RLKey.W:
                    data = 'W';
                    inputType = 2;
                    break;
                case RLKey.X:
                    data = 'X';
                    inputType = 2;
                    break;
                case RLKey.Y:
                    data = 'Y';
                    inputType = 2;
                    break;
                case RLKey.Z:
                    data = 'Z';
                    inputType = 2;
                    break;
                case RLKey.Enter:
                    //exit multi key input
                    multiInput = false;
                    multiData = multiData.Replace("?", "");
                    Console.WriteLine("{0} input", multiData);
                    inputComplete = true;
                    break;
                case RLKey.Escape:
                    //exit data input, exit calling routine
                    multiInput = false;
                    inputComplete = true;
                    multiCaller = 0;
                    break;         
            }
            //add to global character string (exclude the final 'Enter')
            if (inputComplete == false)
            {
                //only accept valid input types
                if ((numInput == true && inputType == 1) || (alphaInput == true && inputType == 2))
                { multiData += data; }
                else
                { multiData += '?'; }
            }
            //clear input console before displaying input
            infoChannel.SetInfoList(new List<Snippet>(), ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet(string.Format("{0} input", multiData), RLColor.LightMagenta, RLColor.Black), ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet(string.Format("Press ENTER when done or ESC to exit", multiData)), ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet("Any '?' will be automatically removed"), ConsoleDisplay.Input);
            return inputComplete;
        }

        public static string ShowDate()
        {
            string dateReturn = "Unknown";
            int moonDay = (gameTurn % 30) + 1;
            int moonCycle = (gameTurn / 30) + 1;
            string moonSuffix = "th";
            if (moonCycle == 1)
            { moonSuffix = "st"; }
            else if (moonCycle == 2)
            { moonSuffix = "nd"; }
            else if (moonCycle == 3)
            { moonSuffix = "rd"; }
            dateReturn = string.Format("Day {0} of the {1}{2} Moon in the Year of our Gods {3}  (Turn {4})", moonDay, moonCycle, moonSuffix, gameYear, gameTurn + 1);
            return dateReturn;
        }
    }
}
