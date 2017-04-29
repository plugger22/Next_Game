using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using Next_Game.Cartographic;
using Next_Game.Event_System;
using System.Globalization;
using System.IO;

namespace Next_Game
{
    public enum MenuMode {Main, Actor_Active, Actor_Passive, Debug, Record, Special, Lore} //distinct menu sets (Menu.cs)
    public enum ConsoleDisplay {Status, Input, Multi, Message, Event, Conflict} //different console windows (Menu window handled independently by Menu.cs) -> Event & Conflict are within Multi
    public enum InputMode {Normal, MultiKey, Scrolling} //special input modes
    public enum SpecialMode {None, FollowerEvent, PlayerEvent, Conflict, Outcome, Notification, Confirm} //if MenuMode.Special then -> type of special (Notification -> msg, Confirm -> Y/N)
    public enum ConflictMode { None, Intro, Strategy, Cards, Outcome, Confirm, AutoResolve, ErrorStrategy, Popup, RestoreCards} //submodes within SpecialMode.Conflict, determines GUI
    public enum InfoMode { None, Followers, Enemies}; //which characters to highlight on the map

    public static class Game
    {
        // The screen height and width are in number of tiles

        //private static int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
        //DEBUG: insert seed here to test a particular map
        private static int seed = 24117;

        static Random rnd;
        
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

        public static readonly int _multiWidth = 130;
        public static readonly int _multiHeight = 100;
        private static RLConsole _multiConsole; //middle right

        private static readonly int _messageWidth = 130;
        private static readonly int _messageHeight = 20;
        private static RLConsole _messageConsole; //bottom right

        private static int _updateTimer = 0; //controls redraws

        public static int gameTurn, gameGeneration, gameStart, gameRevolt, gameExile, gameYear, mapSize;

        //core objects
        public static Menu menu;
        //public static MessageLog messageLog;
        public static Map map;
        public static Network network;
        public static History history;
        public static World world;
        public static Lore lore;
        public static Director director;
        public static InfoChannel infoChannel;
        public static Constant constant;
        public static Variable variable;
        public static FileImport file;
        public static Utility utility;
        public static Layout layout;
        public static Conflict conflict;
        public static Logger logStart;
        public static Logger logError;
        public static Logger logTurn;
        
        //flags & vars
        private static bool _renderRequired = true; //redraw Console?
        private static bool _mouseOn = false; //receive mouse input?
        private static int _multiCaller = 0; //each instance that calls multi key input has a unique ID which is > 0
        private static string _multiData = null; //multi key input is stored here
        private static int _inputState = 0; //used to differentiate suquential levels of input for individual commands
        private static int _actorID = 0; //used for special MenuMode.Actor_Passive to flip between actors using <- & ->
        public static MenuMode _menuMode = MenuMode.Main; //menu mode in operation (corresponds to enum above)
        public static InputMode _inputMode = InputMode.Normal; //non-standard input modes, default none
        public static SpecialMode _specialMode = SpecialMode.None; //special, multiConsole display modes
        public static ConflictMode _conflictMode = ConflictMode.None; //conflict secondary display modes
        public static InfoMode _infoMode = InfoMode.None; //toggled on/off to display character information on map (followers, inquisitors, etc)
        public static int _eventID = 0; //ID of current event being dealt with
        public static bool _fullConsole = false; //set to true by InfoChannel.DrawInfoConsole if multiConsole is maxxed out
        public static int _scrollIndex = 0; //used by infoChannel.DrawConsole to handle scrolling up and down
        public static int _multiConsoleLength = 48; //max length of data in multi Console (infochannel.drawInfoConsole) - Scrolling beyond this
        //errors
        private static string _errorLast = ""; //text of the last generated error (game.SetError)
        private static int _errorCounter = 0; //counts instances of identical errors (game.SetError)
        private static int _errorLimit = 5; //max. # of identical errors that will be shown before 'repeat...' msg
        //other
        private static RLKeyPress _keyLast = null; //last known keypress
        private static Position _posSelect1; //used for input of map positions
        private static Position _posSelect2;
        private static int _charIDSelected; //selected player character
        private static long _totalTime; //Stopwatch total time elasped for all timed sections
        private static bool _endGame = false; //game over if true
        private static bool _endFlag = false; //preliminary end game flag that, if confirmed, triggers _endGame
        private static int _startMode = 0; //controls start turn processes: Early -> Mid -> Late
        //logs
        private static Dictionary<int, Error> dictErrors = new Dictionary<int, Error>(); //all errors (key is errorID)
        private static List<string> listOfTimers = new List<string>(); //all timer tests

        /// <summary>
        /// Main Game Loop
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            rnd = new Random(seed);
            //debug -> write seed to file
            DateTime date1 = DateTime.Now;
            string seedInfo = string.Format("Seed {0} -> {1}", seed, date1.ToString("f", CultureInfo.CreateSpecificCulture("en-AU"))) + Environment.NewLine;
            File.AppendAllText("c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/Seed.txt", seedInfo);
            Console.SetWindowSize(180, 90); //debug console
            //initialise game objects
            Stopwatch timer_1 = new Stopwatch();
            constant = new Constant();
            variable = new Variable(seed);
            utility = new Utility();
            logStart = new Logger("c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/LogStart.txt", true);
            logError = new Logger("c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/LogError.txt");
            try
            {
                //game initialisation -> logStart
                file = new FileImport("c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/");
                file.GetConstants("Constants.txt");
                InitialiseGameVariables();
                timer_1.Start();
                map = new Map(mapSize, seed);
                map.InitialiseMap(constant.GetValue(Global.MAP_FREQUENCY), constant.GetValue(Global.MAP_SPACING));
                StopTimer(timer_1, "Map Initialisation");
                timer_1.Start();
                network = new Network(seed);
                network.InitialiseNetwork();
                StopTimer(timer_1, "Network Initialisation");
                lore = new Lore(seed);
                timer_1.Start();
                history = new History(seed);
                history.InitialiseHistory(network.GetNumUniqueHouses());
                //history.CreatePlayerActors(6);
                StopTimer(timer_1, "History Initialisation");
                world = new World(seed);
                world.InitialiseWorld();
                infoChannel = new InfoChannel();
                timer_1.Start();
                director = new Director(seed);
                director.InitialiseDirector();
                StopTimer(timer_1, "Director Initialisation");
                world.ShowGeneratorStatsRL();
                Message message = new Message($"Game world created with seed {seed}", MessageType.System);
                world.SetMessage(message);
            }
            catch(Exception e)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("ERROR_0   (Initialise Game)");
                builder.AppendLine();
                builder.Append("--- Error Message");
                builder.AppendLine();
                builder.Append(e.Message);
                builder.AppendLine();
                builder.Append("--- Source");
                builder.AppendLine();
                builder.Append(e.Source);
                builder.AppendLine();
                builder.Append("--- Stack Trace");
                builder.AppendLine();
                builder.Append(e.StackTrace);
                builder.AppendLine();
                builder.Append("--- TargetSite");
                builder.AppendLine();
                builder.Append(e.TargetSite);
                string descriptionError = builder.ToString();
                logStart?.Write(descriptionError); logError?.Write(descriptionError);
            }
            finally
            {
                //tidy up before crash
                logStart?.Dispose();
                logStart = null;
                logTurn = new Logger("c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/LogTurn.txt");
            }
            world.ProcessStartGame();
            //layout & conflict
            layout = new Layout(seed, 130, 100, 2, 3, RLColor.Black, RLColor.Yellow);
            layout.Initialise();
            conflict = new Conflict(seed);

            //set up menu
            menu = new Menu(4, 8);
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
            _renderRequired = true;
           
        }


        /// <summary>
        /// Event handler for RLNET's Update event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                //game loop
                RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();
                //last used keypressx
                if (keyPress != null)
                { _keyLast = keyPress; }
                RLMouse mouse = _rootConsole.Mouse;
                bool mouseLeft = _rootConsole.Mouse.GetLeftClick();
                bool mouseRight = _rootConsole.Mouse.GetRightClick();
                bool complete = false;

                //end Game?
                if (_endGame == true)
                {
                    logTurn?.Dispose();
                    logError?.Dispose();
                    _rootConsole.Close();
                    //Environment.Exit(1); - not needed and causes OpenTK error
                }

                //start turn Mode?
                if (_startMode > 0)
                {
                    world.ProcessStartTurnLate();
                    _startMode = 0;
                }

                //special display mode (Events and Conflicts)
                if (_specialMode > 0)
                {
                    if (keyPress != null)
                    {
                        SpecialModeInput(keyPress, _specialMode);
                        keyPress = null;
                    }
                    _renderRequired = true;
                }

                //activate scrolling mode
                if (_fullConsole == true && keyPress != null)
                {
                    _inputMode = InputMode.Scrolling;
                    _renderRequired = true;
                }

                // Multi Key input ---
                if (_inputMode == InputMode.MultiKey && keyPress != null)
                {
                    complete = MultiKeyInput(keyPress);
                    _renderRequired = true;
                    if (complete == true && !String.IsNullOrEmpty(_multiData))
                    {
                        switch (_multiCaller)
                        {
                            case 1:
                                //Show Actor (input actorID)
                                _actorID = Convert.ToInt32(_multiData);
                                infoChannel.SetInfoList(world.ShowActorRL(_actorID), ConsoleDisplay.Multi);
                                infoChannel.ClearConsole(ConsoleDisplay.Input);
                                infoChannel.AppendInfoList(new Snippet("Press LEFT or RIGHT ARROWS to change Actors, ENTER or ESC to exit", RLColor.Magenta, RLColor.Black), ConsoleDisplay.Input);
                                _menuMode = MenuMode.Actor_Passive;
                                //_mouseOn = false;
                                break;
                            case 2:
                                //Select Loyal Follower as a Crow destination
                                int playerID = Convert.ToInt32(_multiData);
                                infoChannel.SetInfoList(world.SendCrow(playerID), ConsoleDisplay.Multi);
                                infoChannel.ClearConsole(ConsoleDisplay.Input);
                                break;
                            case 3:
                                //Spy -> Show specific Actor
                                int actorID = Convert.ToInt32(_multiData);
                                infoChannel.SetInfoList(world.ShowSpyRL(actorID), ConsoleDisplay.Multi);
                                infoChannel.ClearConsole(ConsoleDisplay.Input);
                                break;
                        }
                        //reset
                        keyPress = null; //to prevent Enter keypress from causing the date to tick up
                        _multiCaller = 0;
                        _multiData = null;
                    }
                }

                // Scrolling mode in Multi Console ---
                //scrolling mode - hand off input to scrolling method
                else if (_inputMode == InputMode.Scrolling && keyPress != null)
                {
                    complete = ScrollingKeyInput(keyPress);
                    //return to normal input mode?
                    if (complete == true)
                    {
                        _inputMode = InputMode.Normal;
                        _fullConsole = false;
                        _scrollIndex = 0;
                        infoChannel.ClearConsole(ConsoleDisplay.Multi);
                        keyPress = null;
                    }
                }
                //
                //normal mouse and keyboard input ---
                //
                if (_inputMode == InputMode.Normal)
                {
                    //
                    // MOUSE input ---
                    //
                    if (mouseLeft == true || mouseRight == true)
                    {
                        //Mouse specific input OFF - generic location and party info
                        if (_mouseOn == false)
                        {
                            if (_menuMode != MenuMode.Actor_Passive)
                            {
                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                infoChannel.SetInfoList(world.ShowLocationRL(locID, mouse.X, mouse.Y), ConsoleDisplay.Multi);
                                _renderRequired = true;
                            }
                        }
                        //Mouse specific input ON
                        else if (_mouseOn == true)
                        {
                            //last pressed key indicates context of mouse press
                            switch (_keyLast?.Key)
                            {
                                case RLKey.D:
                                    switch (_menuMode)
                                    {
                                        case MenuMode.Debug:
                                            //debug route between two points
                                            _renderRequired = true;
                                            //Origin location
                                            if (_inputState == 1)
                                            {
                                                //valid location?
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (locID > 0)
                                                {
                                                    string locName = world.GetLocationName(locID);
                                                    infoChannel.AppendInfoList(new Snippet(locName), ConsoleDisplay.Input);
                                                    _posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                    infoChannel.AppendInfoList(new Snippet("Select DESTINATION Location by Mouse (press ESC to Exit)"), ConsoleDisplay.Input);
                                                    _inputState = 2;
                                                }
                                            }
                                            //Destination location
                                            else if (_inputState == 2)
                                            {
                                                //valid location?
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (locID > 0)
                                                {
                                                    //process two positions to show on map.
                                                    _posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                    //check that the two coords aren't identical
                                                    if ((_posSelect1 != null && _posSelect2 != null) && (_posSelect1.PosX != _posSelect2.PosX || _posSelect1.PosY != _posSelect2.PosY))
                                                    {
                                                        List<Route> listOfRoutes = network.GetRouteAnywhere(_posSelect1, _posSelect2);
                                                        map.DrawRouteDebug(listOfRoutes);
                                                        infoChannel.AppendInfoList(new Snippet(network.ShowRouteDetails(listOfRoutes)), ConsoleDisplay.Input);
                                                    }
                                                    _inputState = 0;
                                                    _mouseOn = false;
                                                }
                                            }
                                            break;
                                    }
                                    break;
                                case RLKey.G:
                                    switch (_menuMode)
                                    {
                                        case MenuMode.Debug:
                                            _renderRequired = true;
                                            //Origin location
                                            if (_inputState == 1)
                                            {
                                                //valid location?
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (locID > 0)
                                                {
                                                    string locName = world.GetLocationName(locID);
                                                    infoChannel.AppendInfoList(new Snippet(locName), ConsoleDisplay.Input);
                                                    _posSelect1 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                    infoChannel.AppendInfoList(new Snippet("Select DESTINATION Location by Mouse (press ESC to Exit)"), ConsoleDisplay.Input);
                                                    _inputState = 2;
                                                }
                                            }
                                            //Destination location
                                            else if (_inputState == 2)
                                            {
                                                //valid location?
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (locID > 0)
                                                {
                                                    //process two positions to show on map.
                                                    _posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                    if ((_posSelect1 != null && _posSelect2 != null) && (_posSelect1.PosX != _posSelect2.PosX || _posSelect1.PosY != _posSelect2.PosY))
                                                    {
                                                        List<Route> listOfRoutes = network.GetRouteAnywhere(_posSelect1, _posSelect2);
                                                        map.DrawRouteRL(listOfRoutes);
                                                        infoChannel.AppendInfoList(new Snippet(network.ShowRouteDetails(listOfRoutes)), ConsoleDisplay.Input);
                                                    }
                                                    _inputState = 0;
                                                    _mouseOn = false;
                                                }
                                            }
                                            break;
                                    }
                                    break;
                                case RLKey.H:
                                    switch (_menuMode)
                                    {
                                        case MenuMode.Main:
                                            _renderRequired = true;
                                            //Show House Details
                                            if (_inputState == 1)
                                            {
                                                //valid location?
                                                int houseID = map.GetMapInfo(MapLayer.HouseID, mouse.X, mouse.Y, true);
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (houseID > 0)
                                                { infoChannel.SetInfoList(world.ShowHouseRL(houseID), ConsoleDisplay.Multi); }
                                                else if (locID == 1)
                                                { infoChannel.SetInfoList(world.ShowCapitalRL(), ConsoleDisplay.Multi); }
                                            }
                                            _mouseOn = false;
                                            break;
                                    }
                                    break;
                                case RLKey.P:
                                    switch (_menuMode)
                                    {
                                        case MenuMode.Actor_Active:
                                            //move Player character from A to B
                                            _renderRequired = true;
                                            //valid location?
                                            if (_inputState == 1)
                                            {
                                                int locID = map.GetMapInfo(MapLayer.LocID, mouse.X, mouse.Y, true);
                                                if (locID > 0)
                                                {
                                                    //process two positions to show on map.
                                                    _posSelect2 = new Position(map.ConvertMouseCoords(mouse.X, mouse.Y));
                                                    if (_posSelect2 != null)
                                                    {
                                                        string infoString = string.Format("Journey from {0} to {1}? [Left Click] to confirm, [Right Click] to Cancel.",
                                                            network.GetLocationName(_posSelect1), network.GetLocationName(_posSelect2));
                                                        infoChannel.AppendInfoList(new Snippet(infoString), ConsoleDisplay.Input);
                                                        _inputState = 2;
                                                    }
                                                }
                                                else
                                                {
                                                    //cancel journey
                                                    if (mouseRight == true)
                                                    {
                                                        infoChannel.AppendInfoList(new Snippet("Journey Cancelled!", RLColor.Red, RLColor.Black), ConsoleDisplay.Input);
                                                        _inputState = 0; _mouseOn = false;
                                                    }
                                                }
                                            }
                                            else if (_inputState == 2)
                                            {
                                                if (mouseLeft == true)
                                                {
                                                    if ((_posSelect1 != null && _posSelect2 != null) && (_posSelect1.PosX != _posSelect2.PosX || _posSelect1.PosY != _posSelect2.PosY))
                                                    {
                                                        //int locID = map.GetMapInfo(MapLayer.LocID, _posSelect2.PosX, _posSelect2.PosY);
                                                        //int refID = world.GetRefID(locID);
                                                        //List<Position> pathToTravel = network.GetPathAnywhere(_posSelect1, _posSelect2);
                                                        /*string infoText = */
                                                        world.InitiateMoveActor(_charIDSelected, _posSelect1, _posSelect2/*, pathToTravel*/);
                                                        /*Message message = new Message(infoText, _charIDSelected, locID, MessageType.Move);
                                                        world.SetMessage(message);
                                                        if (_charIDSelected == 1)
                                                        { Game.world.SetPlayerRecord(new Record(infoText, _charIDSelected, locID, refID, CurrentActorIncident.Travel)); }
                                                        else if (_charIDSelected > 1)
                                                        { Game.world.SetCurrentRecord(new Record(infoText, _charIDSelected, locID, refID, CurrentActorIncident.Travel)); }
                                                        infoChannel.AppendInfoList(new Snippet(infoText), ConsoleDisplay.Input);*/
                                                        /*//show route
                                                        map.UpdateMap();
                                                        map.DrawRoutePath(pathToTravel);*/
                                                    }
                                                    else
                                                    { infoChannel.AppendInfoList(new Snippet("Destination the same as Origin. Journey Cancelled!", RLColor.Red, RLColor.Black), ConsoleDisplay.Input); }
                                                }
                                                else if (mouseRight == true)
                                                { infoChannel.AppendInfoList(new Snippet("Journey Cancelled!", RLColor.Red, RLColor.Black), ConsoleDisplay.Input); }
                                                _inputState = 0;
                                                _mouseOn = false;
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
                        _renderRequired = true;
                        _mouseOn = false;
                        //which key pressed?
                        switch (keyPress.Key)
                        {
                            case RLKey.A:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                    case MenuMode.Actor_Passive:
                                        //show Actor
                                        infoChannel.ClearConsole(ConsoleDisplay.Input);
                                        infoChannel.AppendInfoList(new Snippet("---Input Actor ID ", RLColor.Magenta, RLColor.Black), ConsoleDisplay.Input);
                                        infoChannel.AppendInfoList(new Snippet("Press ENTER when done, BACKSPACE to change, ESC to exit"), ConsoleDisplay.Input);
                                        _inputMode = InputMode.MultiKey;
                                        _multiCaller = 1;
                                        break;
                                    case MenuMode.Record:
                                        //All Actors
                                        infoChannel.SetInfoList(world.GetHistoricalRecordSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- ALL RECORDS", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;

                                }
                                break;
                            case RLKey.C:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        List<Snippet> inputList = new List<Snippet>();
                                        int numCrows = world.GetCrowsAvailable();
                                        if (numCrows > 0)
                                        {
                                            inputList.Add(new Snippet("--- Send a Crow to a Loyal Follower", RLColor.Magenta, RLColor.Black));
                                            inputList.Add(new Snippet(string.Format("You have {0} {1} remaining", numCrows, numCrows == 1 ? "Crow" : "Crows")));
                                            inputList.Add(new Snippet("Select Follower (input #)"));
                                            _inputMode = InputMode.MultiKey;
                                            _multiCaller = 2;
                                        }
                                        else
                                        { inputList.Add(new Snippet("You have run out of crows!", RLColor.Red, RLColor.Black)); }
                                        infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                        break;
                                    case MenuMode.Record:
                                        //Custom report (debugging)
                                        infoChannel.SetInfoList(world.GetHistoricalRecordSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- CUSTOM", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.D:
                                //List<Route> listOfRoutes_2 = network.RouteInput("D"); map.DrawRouteDebug(listOfRoutes_2);
                                _renderRequired = true;
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //switch to Debug menu
                                        _menuMode = menu.SwitchMenuMode(MenuMode.Debug);
                                        break;
                                    case MenuMode.Record:
                                        //Dead Actors
                                        infoChannel.SetInfoList(world.GetHistoricalRecordSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- all DEATHS", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        //show debug route
                                        List<Snippet> inputList = new List<Snippet>();
                                        inputList.Add(new Snippet("--- Show the Route between two Locations", RLColor.Magenta, RLColor.Black));
                                        inputList.Add(new Snippet("Select ORIGIN Location by Mouse (press ESC to Exit)"));
                                        infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                        _mouseOn = true;
                                        _inputState = 1;
                                        break;
                                }
                                break;
                            case RLKey.E:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //Show Enemies (what player knows)
                                        infoChannel.SetInfoList(world.ShowEnemiesRL(), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        //Show ALL Errors
                                        infoChannel.SetInfoList(ShowErrorsRL(), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Errors ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.F:
                                switch (_menuMode)
                                {
                                    case MenuMode.Lore:
                                        //Fate of Royal Family Lore
                                        infoChannel.SetInfoList(world.GetLoreSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Fate of Royal Family LORE", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.G:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        world.ShowGeneratorStatsRL();
                                        break;
                                    case MenuMode.Record:
                                        //Marriages
                                        infoChannel.SetInfoList(world.GetHistoricalRecordSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- all MARRIAGES", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        List<Snippet> inputList = new List<Snippet>();
                                        inputList.Add(new Snippet("--- Show the Route between two Locations", RLColor.Magenta, RLColor.Black));
                                        inputList.Add(new Snippet("Select ORIGIN Location by Mouse (press ESC to Exit)"));
                                        infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
                                        _inputState = 1;
                                        _mouseOn = true;
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
                                        _inputState = 1;
                                        _mouseOn = true;
                                        break;
                                    case MenuMode.Debug:
                                        //Show House Relations
                                        List<string> tempList = history.GetHouseMasterRels();
                                        if (tempList != null && tempList.Count > 0)
                                        {
                                            List<Snippet> tempSnippets = new List<Snippet>();
                                            foreach (string text in tempList)
                                            { tempSnippets.Add(new Snippet(text)); }
                                            infoChannel.SetInfoList(tempSnippets, ConsoleDisplay.Multi);
                                            infoChannel.InsertHeader(new Snippet("--- House Relationships ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        }
                                        break;
                                }
                                break;
                            case RLKey.I:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                    case MenuMode.Debug:
                                        //Toggle display of key characters; Off -> Followers -> Enemies -> Off (handled directly by Map.DrawMapRL)
                                        //If in Debug mode All enemies are shown in current position, otherwise only those known or recently known (marker is how many days old info is)
                                        switch (_infoMode)
                                        {
                                            case InfoMode.None:
                                                _infoMode = InfoMode.Followers;
                                                break;
                                            case InfoMode.Followers:
                                                _infoMode = InfoMode.Enemies;
                                                break;
                                            case InfoMode.Enemies:
                                                _infoMode = InfoMode.None;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case RLKey.K:
                                switch (_menuMode)
                                {
                                    case MenuMode.Record:
                                        //Kingdom Events
                                        infoChannel.SetInfoList(world.GetHistoricalRecordSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- all KINGDOM EVENTS", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        infoChannel.SetInfoList(world.ShowHouseRL(0, lore.OldKing.RefID), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.L:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //witch to Lore Menu
                                        _menuMode = menu.SwitchMenuMode(MenuMode.Lore);
                                        break;
                                    case MenuMode.Debug:
                                        //Show All Items log
                                        infoChannel.SetInfoList(world.ShowPossessionsRL(PossessionType.Item), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Items ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.M:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //Show Full message log
                                        infoChannel.SetInfoList(world.ShowMessagesRL(), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Message Log ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        //Draw Map: applies to all menu modes
                                        map.UpdateMap(false, true);
                                        map.ShowHouses();
                                        break;
                                }
                                break;
                            case RLKey.P:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //Show Player Characters
                                        infoChannel.SetInfoList(world.ShowActiveActorsRL(), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Actor_Active:
                                        //move Active characters around map (must be AtLocation in order to move)
                                        List<Snippet> charList = new List<Snippet>();
                                        charList.Add(world.GetActorStatusRL(_charIDSelected));
                                        if (world.CheckActorStatus(_charIDSelected, ActorStatus.AtLocation) == true)
                                        {
                                            _posSelect1 = world.GetActiveActorLocationByPos(_charIDSelected);
                                            if (_posSelect1 != null)
                                            { charList.Add(new Snippet("Click on the Destination location or press [Right Click] to cancel")); _mouseOn = true; }
                                            else
                                            { charList.Add(new Snippet("The character is not currently at your disposal", RLColor.Red, RLColor.Black)); _mouseOn = false; }
                                            _inputState = 1;
                                        }
                                        infoChannel.SetInfoList(charList, ConsoleDisplay.Input);
                                        break;
                                    case MenuMode.Debug:
                                        //Show Duplicates (imported files)
                                        infoChannel.SetInfoList(world.ShowDuplicatesRL(), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Duplicates (Imported Files)", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.Q:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        break;
                                    case MenuMode.Debug:
                                        //Show Enemies (full info)
                                        infoChannel.SetInfoList(world.ShowEnemiesRL(true), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.R:
                                //Show all routes on the map in red
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        //infoChannel.SetInfoList(world.ShowRecordsRL(), ConsoleDisplay.Multi);
                                        //switch to Debug menu
                                        _menuMode = menu.SwitchMenuMode(MenuMode.Record);
                                        break;
                                    case MenuMode.Debug:
                                        //show debug route
                                        map.UpdateMap(true, false);
                                        _mouseOn = true;
                                        break;
                                }
                                break;
                            case RLKey.S:
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        /*
                                        _menuMode = MenuMode.Special;
                                        _specialMode = SpecialMode.Conflict;
                                        _conflictMode = ConflictMode.Intro;
                                        //debug
                                        if (rnd.Next(100) < 55)
                                        {
                                            Noble newKing = lore.NewKing;
                                            //NOTE: Stick to this sequence
                                            conflict.Conflict_Type = ConflictType.Combat;
                                            conflict.Combat_Type = (ConflictCombat)rnd.Next(1, 4);
                                            //conflict.Combat_Type = ConflictCombat.Battle;
                                            conflict.SetOpponent(newKing.ActID, Convert.ToBoolean(rnd.Next(2)));
                                            conflict.SetSpecialSituation((ConflictSpecial)rnd.Next(1, 5), rnd.Next(1, 5));
                                            conflict.SetGameSituation(ConflictState.Relative_Army_Size);
                                        }
                                        else
                                        {
                                            Noble newQueen = lore.NewQueen;
                                            conflict.Conflict_Type = ConflictType.Social;
                                            conflict.Social_Type = (ConflictSocial)rnd.Next(1, 4);
                                            conflict.SetOpponent(newQueen.ActID, Convert.ToBoolean(rnd.Next(0, 2)));
                                            ConflictState debugState = (ConflictState)rnd.Next(2, 6);
                                            //conflict.SetGameSituation(debugState, string.Format("Your {0}", debugState));
                                            conflict.SetGameSituation(debugState);
                                        }
                                        if (conflict.InitialiseConflict() == false)
                                        {
                                            //invalid conflict setup, revert to normal
                                            _menuMode = MenuMode.Main;
                                            _specialMode = SpecialMode.None;
                                            _conflictMode = ConflictMode.None;
                                        }*/
                                        //debug
                                        List<Snippet> tempList = new List<Snippet>();
                                        tempList.Add(new Snippet("Test Notification"));
                                        _specialMode = SpecialMode.Notification;
                                        world.SetNotification(tempList);
                                        break;
                                    case MenuMode.Debug:
                                        //Show All Secrets log
                                        infoChannel.SetInfoList(world.ShowPossessionsRL(PossessionType.Secret), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Secrets ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.T:
                                switch (_menuMode)
                                {
                                    case MenuMode.Debug:
                                        //Show Timer log
                                        infoChannel.SetInfoList(ShowTimersRL(), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Timers ALL", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.U:
                                switch (_menuMode)
                                {
                                    case MenuMode.Lore:
                                        //Uprising Lore
                                        infoChannel.SetInfoList(world.GetLoreSet(keyPress), ConsoleDisplay.Multi);
                                        infoChannel.InsertHeader(new Snippet("--- Uprising LORE", RLColor.Yellow, RLColor.Black), ConsoleDisplay.Multi);
                                        break;
                                    case MenuMode.Debug:
                                        //Spy -> Show All Bloodhound data, grouped by turns
                                        infoChannel.SetInfoList(world.ShowSpyAllRL(true, true), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.V:
                                switch (_menuMode)
                                {
                                    case MenuMode.Debug:
                                        //Spy -> Bloodhound -> show specific Actor
                                        infoChannel.ClearConsole(ConsoleDisplay.Input);
                                        infoChannel.AppendInfoList(new Snippet("---Input Actor ID ", RLColor.Magenta, RLColor.Black), ConsoleDisplay.Input);
                                        infoChannel.AppendInfoList(new Snippet("Press ENTER when done, BACKSPACE to change, ESC to exit"), ConsoleDisplay.Input);
                                        _inputMode = InputMode.MultiKey;
                                        _multiCaller = 3;
                                        break;
                                }
                                break;
                            case RLKey.X:
                                //exit application from Main Menu
                                switch (_menuMode)
                                {
                                    case MenuMode.Main:
                                        SetEndGame("Player chose to exit (pressed 'X')");
                                        break;
                                }
                                break;
                            case RLKey.Y:
                                switch (_menuMode)
                                {
                                    case MenuMode.Debug:
                                        //Spy -> Show Active Actors Bloodhound data, grouped by turns
                                        infoChannel.SetInfoList(world.ShowSpyAllRL(true, false), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.Z:
                                switch (_menuMode)
                                {
                                    case MenuMode.Debug:
                                        //Spy -> Show Enemy Actors Bloodhound data, grouped by turns
                                        infoChannel.SetInfoList(world.ShowSpyAllRL(false, true), ConsoleDisplay.Multi);
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
                            case RLKey.Number7:
                            case RLKey.Number8:
                            case RLKey.Number9:
                                if (_inputMode == InputMode.Normal)
                                {
                                    switch (_menuMode)
                                    {
                                        case MenuMode.Main:
                                            _menuMode = menu.SwitchMenuMode(MenuMode.Actor_Active);
                                            _charIDSelected = (int)keyPress.Key - 109; //based on a system where '1' is '110'
                                            List<Snippet> infoList = new List<Snippet>();
                                            infoList.Add(world.ShowSelectedActor(_charIDSelected));
                                            infoChannel.SetInfoList(infoList, ConsoleDisplay.Input);
                                            break;
                                    }
                                }
                                break;
                            case RLKey.Left:
                                switch (_menuMode)
                                {
                                    case MenuMode.Actor_Passive:
                                        //Left Arrow -> show Actor with ID--
                                        _actorID--;
                                        _actorID = Math.Max(_actorID, 1);
                                        infoChannel.SetInfoList(world.ShowActorRL(_actorID), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.Right:
                                switch (_menuMode)
                                {
                                    case MenuMode.Actor_Passive:
                                        //Right Arrow -> show Actor with ID--
                                        _actorID++;
                                        infoChannel.SetInfoList(world.ShowActorRL(_actorID), ConsoleDisplay.Multi);
                                        break;
                                }
                                break;
                            case RLKey.Enter:
                                world.ProcessEndTurn();
                                logTurn?.Close(); logTurn?.Open(); //retain previous turn's output only
                                if (world.ProcessStartTurnEarly() == false)
                                { _startMode = 1; }
                                infoChannel.ClearConsole(ConsoleDisplay.Input);
                                infoChannel.ClearConsole(ConsoleDisplay.Multi);
                                //infoChannel.AppendInfoList(new Snippet(Game.utility.ShowDate(), RLColor.Yellow, RLColor.Black), ConsoleDisplay.Input);
                                world.ShowGameStateRL();
                                if (_menuMode == MenuMode.Actor_Passive)
                                { _menuMode = MenuMode.Main; }
                                break;
                            case RLKey.Escape:
                                //clear input & multi consoles
                                infoChannel.ClearConsole(ConsoleDisplay.Input);
                                infoChannel.ClearConsole(ConsoleDisplay.Multi);
                                //exit special display mode
                                _specialMode = SpecialMode.None;
                                //exit mouse input 
                                if (_mouseOn == true)
                                { _mouseOn = false; }
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
            catch (Exception ex)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("ERROR_00   (Main Game Loop)");
                builder.AppendLine();
                builder.Append("--- Error Message");
                builder.AppendLine();
                builder.Append(ex.Message);
                builder.AppendLine();
                builder.Append("--- Source");
                builder.AppendLine();
                builder.Append(ex.Source);
                builder.AppendLine();
                builder.Append("--- Stack Trace");
                builder.AppendLine();
                builder.Append(ex.StackTrace);
                builder.AppendLine();
                builder.Append("--- TargetSite");
                builder.AppendLine();
                builder.Append(ex.TargetSite);
                string descriptionError = builder.ToString();
                if (logTurn != null)
                {
                    logTurn?.Write(descriptionError); logError?.Write(descriptionError);
                    //tidy up before crash
                    logError?.Dispose(); logError = null;
                    logTurn?.Dispose(); logTurn = null;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(descriptionError);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

    
        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            if (_renderRequired == false)
            {
                //redraw screen every so often regardless of whether update is required in order to get consistently flashing symbols onscreen
                _updateTimer++;
                if (_updateTimer > 100)
                { _renderRequired = true;  _updateTimer = 0; }
            }
            // Tell RLNET to draw the console that we set
            if (_renderRequired == true)
            {
                //update status console
                infoChannel.SetInfoList(world.ShowActiveActorsRL(), ConsoleDisplay.Status);
                infoChannel.SetInfoList(world.ShowRecentMessagesRL(), ConsoleDisplay.Message);
                //draw to consoles
                map.DrawMapRL(_mapConsole);
                menu.DrawMenuRL(_menuConsole);
                infoChannel.DrawInfoConsole(_inputConsole, ConsoleDisplay.Input);
                infoChannel.DrawInfoConsole(_messageConsole, ConsoleDisplay.Message);
                infoChannel.DrawInfoConsole(_multiConsole, ConsoleDisplay.Multi, _specialMode);
                infoChannel.DrawInfoConsole(_statusConsole, ConsoleDisplay.Status);
                //Blit to root Console
                RLConsole.Blit(_menuConsole, 0, 0, _menuWidth, _menuHeight,_rootConsole, 0, 0);
                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight,_rootConsole, 0, 20);
                RLConsole.Blit(_statusConsole, 0, 0, _statusWidth, _statusHeight, _rootConsole, 0, 120);
                RLConsole.Blit(_inputConsole, 0, 0, _inputWidth, _inputHeight,_rootConsole, 100, 0);
                RLConsole.Blit(_multiConsole, 0, 0, _multiWidth, _multiHeight,_rootConsole, 100, 20);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight,_rootConsole, 100, 120);
               _rootConsole.Draw();
                _renderRequired = false;
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
            int inputType = 0; //1 - numeric, 2 - alphabetic, 3 - Backspace
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
                case RLKey.BackSpace:
                    inputType = 3;
                    break;
                case RLKey.Enter:
                    if (_multiData != null)
                    {   //exit multi key input
                        _inputMode = InputMode.Normal;
                        _multiData = _multiData.Replace("?", "");
                        logTurn?.Write("--- MultiKeyInput (Game.cs)");
                        logTurn?.Write($"{_multiData} input");
                        inputComplete = true;
                    }
                    break;
                case RLKey.Escape:
                    //exit data input, exit calling routine
                    _inputMode = InputMode.Normal;
                    inputComplete = true;
                    _multiCaller = 0;
                    break;         
            }
            //add to global character string (exclude the final 'Enter')
            if (inputComplete == false)
            {
                //only accept valid input types
                if ((numInput == true && inputType == 1) || (alphaInput == true && inputType == 2))
                { _multiData += data; }
                else if (inputType == 3)
                {
                    //backspace - delete last character
                    if (_multiData.Length > 0)
                    {
                        try
                        { _multiData = _multiData.Remove(_multiData.Length - 1); }
                        catch (Exception e)
                        { SetError(new Error(61, e.Message)); }
                    }
                }
                else
                { _multiData += '?'; }
            }
            //clear input console before displaying input
            infoChannel.ClearConsole(ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet(string.Format("{0} input", _multiData), RLColor.LightMagenta, RLColor.Black), ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet("Press ENTER when done, BACKSPACE to change, or ESC to exit"), ConsoleDisplay.Input);
            infoChannel.AppendInfoList(new Snippet("Any '?' will be automatically removed"), ConsoleDisplay.Input);
            return inputComplete;
        }

        /// <summary>
        /// Takes over keyboard input in the event of the multi console going into scrolling mode
        /// </summary>
        /// <param name="keyPress"></param>
        /// <returns></returns>
        private static bool ScrollingKeyInput(RLKeyPress keyPress)
        {
            bool inputComplete = false;
            switch (keyPress.Key)
            {
                case RLKey.PageUp:
                    _scrollIndex -= _multiConsoleLength;
                    _scrollIndex = Math.Max(_scrollIndex, 0);
                    _renderRequired = true;
                    break;
                case RLKey.PageDown:
                    _scrollIndex += _multiConsoleLength;
                    _renderRequired = true;
                    break;
                case RLKey.Escape:
                    inputComplete = true;
                    _renderRequired = true;
                break;
            }
                return inputComplete;
        }

        /// <summary>
        /// Handles input for Special Display modes (based around function key inputs)
        /// </summary>
        /// <param name="keyPress"></param>
        /// <param name="mode"></param>
        private static void SpecialModeInput(RLKeyPress keyPress, SpecialMode mode)
        {
            bool exitFlag = false;
            int returnCode = 0;
            switch (keyPress.Key)
            {
                case RLKey.F1:
                    if (mode == SpecialMode.PlayerEvent)
                    {
                       returnCode = director.ResolveOptionOutcome(_eventID, 1);
                       if (returnCode == 1) { _specialMode = SpecialMode.Outcome; }
                       else if (returnCode == 2)
                        {
                            //_menuMode = MenuMode.Special;
                            _specialMode = SpecialMode.Conflict;
                            _conflictMode = ConflictMode.Intro;
                            if (conflict.InitialiseConflict() == false)
                            {
                                //invalid conflict setup, revert to normal
                                _specialMode = SpecialMode.Outcome;
                                _conflictMode = ConflictMode.None;
                            }
                        }
                    }
                    else if (mode == SpecialMode.Conflict)
                    {
                        switch (_conflictMode)
                        {
                            case ConflictMode.Strategy:
                                _conflictMode = ConflictMode.Confirm;
                                layout.Strategy_Player = 0;
                                break;
                            case ConflictMode.Cards:
                                //only a valid keypress if there is influence remaining
                                if (layout.InfluenceRemaining > 0)
                                {
                                    //repeat until hand is empty
                                    if (layout.CheckHandStatus() == false)
                                    { _conflictMode = ConflictMode.Outcome; }
                                    layout.ResolveCard(true);
                                }
                                break;
                        }
                    }
                    break;
                case RLKey.F2:
                    if (mode == SpecialMode.PlayerEvent)
                    {
                        returnCode = director.ResolveOptionOutcome(_eventID, 2);
                        if (returnCode == 1) { _specialMode = SpecialMode.Outcome; }
                        else if (returnCode == 2)
                        {
                            //_menuMode = MenuMode.Special;
                            _specialMode = SpecialMode.Conflict;
                            _conflictMode = ConflictMode.Intro;
                            if (conflict.InitialiseConflict() == false)
                            {
                                //invalid conflict setup, revert to normal
                                _specialMode = SpecialMode.Outcome;
                                _conflictMode = ConflictMode.None;
                            }
                        }
                    }
                    else if (mode == SpecialMode.Conflict)
                    {
                        switch (_conflictMode)
                        {
                            case ConflictMode.Strategy:
                                _conflictMode = ConflictMode.Confirm;
                                layout.Strategy_Player = 1;
                                break;
                            case ConflictMode.Cards:
                                _conflictMode = ConflictMode.Popup;
                                break;
                            case ConflictMode.Popup:
                                _conflictMode = ConflictMode.RestoreCards;
                                break;
                        }
                    }
                    break;
                case RLKey.F3:
                    if (mode == SpecialMode.PlayerEvent)
                    {
                        returnCode = director.ResolveOptionOutcome(_eventID, 3);
                        if (returnCode == 1) { _specialMode = SpecialMode.Outcome; }
                        else if (returnCode == 2)
                        {
                            //_menuMode = MenuMode.Special;
                            _specialMode = SpecialMode.Conflict;
                            _conflictMode = ConflictMode.Intro;
                            if (conflict.InitialiseConflict() == false)
                            {
                                //invalid conflict setup, revert to normal
                                _specialMode = SpecialMode.Outcome;
                                _conflictMode = ConflictMode.None;
                            }
                        }
                    }
                    else if (mode == SpecialMode.Conflict)
                    {
                        switch (_conflictMode)
                        {
                            case ConflictMode.Strategy:
                                _conflictMode = ConflictMode.Confirm;
                                layout.Strategy_Player = 2;
                                break;
                        }
                    }
                    break;
                case RLKey.F4:
                case RLKey.F5:
                case RLKey.F6:
                case RLKey.F7:
                case RLKey.F8:
                case RLKey.F9:
                case RLKey.F10:
                case RLKey.F11:
                case RLKey.F12:
                    if (mode == SpecialMode.PlayerEvent)
                    {
                        int codeNum = 4;
                        switch (keyPress.Key)
                        {
                            case RLKey.F5: codeNum = 5; break;
                            case RLKey.F6: codeNum = 6; break;
                            case RLKey.F7: codeNum = 7; break;
                            case RLKey.F8: codeNum = 8; break;
                            case RLKey.F9: codeNum = 9; break;
                            case RLKey.F10: codeNum = 10; break;
                            case RLKey.F11: codeNum = 11; break;
                            case RLKey.F12: codeNum = 12; break;
                        }
                        returnCode = director.ResolveOptionOutcome(_eventID, codeNum);
                        if (returnCode == 1) { _specialMode = SpecialMode.Outcome; }
                        else if (returnCode == 2)
                        {
                            //_menuMode = MenuMode.Special;
                            _specialMode = SpecialMode.Conflict;
                            _conflictMode = ConflictMode.Intro;
                            if (conflict.InitialiseConflict() == false)
                            {
                                //invalid conflict setup, revert to normal
                                _specialMode = SpecialMode.Outcome;
                                _conflictMode = ConflictMode.None;
                            }
                        }
                    }
                    break;
                case RLKey.Y:
                    if (mode == SpecialMode.Confirm)
                    { _endFlag = true; exitFlag = true; }
                    break;
                case RLKey.N:
                case RLKey.Space:
                case RLKey.Enter:
                case RLKey.Escape:
                    //any Follower events that need dealing with?
                    if (mode == SpecialMode.FollowerEvent)
                    {
                        
                        if (director.ResolveFollowerEvents())
                        { }
                        else
                        { exitFlag = true; }
                    }
                    else if (mode == SpecialMode.Notification)
                    { exitFlag = true; _startMode = 1; }
                    else if (mode == SpecialMode.Confirm)
                    { exitFlag = true; _endFlag = false; }
                    //Player Events
                    else if (mode == SpecialMode.PlayerEvent)
                    {
                        //default option (first option) if player ignores the event
                        returnCode = director.ResolveOptionOutcome(_eventID, 1);
                        if (returnCode == 1) { _specialMode = SpecialMode.Outcome; }
                        else if (returnCode == 2)
                        {
                            //_menuMode = MenuMode.Special;
                            _specialMode = SpecialMode.Conflict;
                            _conflictMode = ConflictMode.Intro;
                            if (conflict.InitialiseConflict() == false)
                            {
                                //invalid conflict setup, revert to normal
                                _specialMode = SpecialMode.Outcome;
                                _conflictMode = ConflictMode.None;
                            }
                        }

                        else if (director.ResolvePlayerEvents())
                        { }
                        else
                        {
                            //any follower events? (they come after Player events)
                            if (director.CheckRemainingFollowerEvents())
                            {
                                _specialMode = SpecialMode.FollowerEvent;
                                director.ResolveFollowerEvents();
                            }
                            else
                            { exitFlag = true; }
                        }
                    }
                    //Outcome - needs to be AFTER Player Events
                    else if (mode == SpecialMode.Outcome)
                    {
                        _eventID = 0;
                        infoChannel.ClearConsole(ConsoleDisplay.Input);
                        infoChannel.ClearConsole(ConsoleDisplay.Multi);
                        if (director.CheckRemainingPlayerEvents())
                        {
                            //auto chained events (one leads onto the next)
                            _specialMode = SpecialMode.PlayerEvent;
                            director.ResolvePlayerEvents();
                        }
                        else if (director.CheckRemainingFollowerEvents())
                        {
                            _specialMode = SpecialMode.FollowerEvent;
                            director.ResolveFollowerEvents();
                        }
                        else
                        { exitFlag = true; }

                    }
                    else if (mode == SpecialMode.Conflict)
                    {
                        if (keyPress.Key == RLKey.Escape && (_conflictMode == ConflictMode.Strategy || _conflictMode == ConflictMode.Cards))
                        {
                            //AutoResolve Mode
                            layout.HandAutoResolve();
                            _conflictMode = ConflictMode.AutoResolve;
                        }
                        else
                        {
                            //Manual Resolution
                            switch (_conflictMode)
                            {
                                case ConflictMode.Intro:
                                    _conflictMode = ConflictMode.Strategy;
                                    break;
                                case ConflictMode.Strategy:
                                    _conflictMode = ConflictMode.ErrorStrategy;
                                    break;
                                case ConflictMode.ErrorStrategy:
                                    _conflictMode = ConflictMode.Strategy;
                                    break;
                                case ConflictMode.Cards:
                                    //repeat until hand is empty
                                    if (layout.CheckHandStatus() == false) { _conflictMode = ConflictMode.Outcome; layout.UpdateOutcome(); }
                                    layout.ResolveCard();
                                    break;
                                case ConflictMode.Confirm:
                                    _conflictMode = ConflictMode.Cards;
                                    //layout.ResolveCard();
                                    break;
                                case ConflictMode.AutoResolve:
                                    _conflictMode = ConflictMode.Outcome;
                                    layout.UpdateOutcome();
                                    break;
                                case ConflictMode.Outcome:
                                    layout.ResetLayout();
                                    //any follower events? (they come after Player events)
                                    if (director.CheckRemainingFollowerEvents())
                                    {
                                        _specialMode = SpecialMode.FollowerEvent;
                                        director.ResolveFollowerEvents();
                                    }
                                    else
                                    { exitFlag = true; }
                                    break;
                                case ConflictMode.Popup:
                                    _conflictMode = ConflictMode.RestoreCards;
                                    break;
                                /*case ConflictMode.RestoreCards:
                                    _conflictMode = ConflictMode.Cards;
                                    break;*/
                            }
                        }
                    }
                    break;
            }

            //exit
            if (exitFlag == true)
            {
                _eventID = 0;
                infoChannel.ClearConsole(ConsoleDisplay.Input);
                infoChannel.ClearConsole(ConsoleDisplay.Multi);
                //Exit out of Special Display mode
                _specialMode = SpecialMode.None;
                //exit mouse input 
                if (_mouseOn == true)
                { _mouseOn = false; }
                //revert back to main menu
                else
                {
                    //return to main menu from sub menus
                    if (_menuMode != MenuMode.Main)
                    { _menuMode = menu.SwitchMenuMode(MenuMode.Main); }
                }
                //end of game
                if (_endFlag == true) { _endGame = true; }
            }
            
        }
        

        /// <summary>
        /// Generate a list of ALL Errors
        /// </summary>
        /// <returns></returns>
        internal static List<Snippet> ShowErrorsRL()
        {
            List<string> tempList = new List<string>();
            IEnumerable<string> errorList =
                from error in dictErrors
                orderby error.Value.errorID
                select Convert.ToString("E_" + error.Value.Code + " " + error.Value.Text + " (M:" + error.Value.Method + " L:" + error.Value.Line + ")");
            tempList = errorList.ToList();
            //snippet list
            List<Snippet> listData = new List<Snippet>();
            foreach (string data in tempList)
            { listData.Add(new Snippet(data)); }
            return listData;
        }

        /// <summary>
        /// Adds error to dictionary and spits it out to console as a back up
        /// </summary>
        /// <param name="error"></param>
        internal static void SetError(Error error)
        {
            try
            {
                dictErrors?.Add(error.errorID, error);
                //check if identical to previous error
                if (error.Text.Equals(_errorLast) == true)
                { _errorCounter++; }
                else
                { _errorCounter = 0; _errorLast = error.Text; }
                //flag to avoid multiple writes to console for the same error
                bool console = true;
                //print first 5 occurences of error
                if (_errorCounter < _errorLimit + 1)
                {
                    string descriptor = string.Format("Method: {0},  Line: {1},  Object: {2},  Turn: {3},  Local Time: {4} UTC {5}", error.Method, error.Line, error.Object, error.Turn, 
                        error.Time, error.TimeZone);
                    //write to log files
                    logError?.Write(string.Format("ERROR_{0} \"{1}\"", error.Code, error.Text), true, ConsoleColor.Yellow); logError?.Write(descriptor, true, ConsoleColor.Yellow); console = false;
                    if (logStart != null)
                    { logStart.Write(string.Format("ERROR_{0} \"{1}\"", error.Code, error.Text), console, ConsoleColor.Yellow);
                        logStart?.Write(descriptor, console, ConsoleColor.Yellow); console = false; }
                    else if (logTurn != null)
                    { logTurn.Write(string.Format("ERROR_{0} \"{1}\"", error.Code, error.Text), console, ConsoleColor.Yellow); logTurn?.Write(descriptor, console, ConsoleColor.Yellow); }
                }
                //print message regarding ongoing repeats and then ignore the rest
                else if (_errorCounter == _errorLimit)
                {
                    //Console.WriteLine("Multiple repeats of same error...");
                    logError?.Write("Multiple repeats of same error...", true, ConsoleColor.Red); console = false;
                    logStart?.Write("Multiple repeats of same error...", console, ConsoleColor.Red); console = false;
                    logTurn?.Write(string.Format("Multiple repeats of same error..."), console, ConsoleColor.Red);
                }
                
            }
            catch(ArgumentNullException)
            {
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine(string.Format(" [SetError] Error (\"{0}\") not written as Null, ErrorID \"{1}\")", error.Text, error.errorID));
                logError?.Write(string.Format(" [SetError] Error (\"{0}\") not written as Null, ErrorID \"{1}\")", error.Text, error.errorID), true, ConsoleColor.Yellow);
                logStart?.Write(string.Format(" [SetError] Error (\"{0}\") not written as Null, ErrorID \"{1}\")", error.Text, error.errorID), true, ConsoleColor.Yellow);
                //Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch(ArgumentException)
            {
                //Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine(string.Format(" [SetError] Error (\"{0}\") not written as duplicate ErrorID \"{1}\")", error.Text, error.errorID));
                logError?.Write(string.Format(" [SetError] Error (\"{0}\") not written as duplicate ErrorID \"{1}\")", error.Text, error.errorID), true, ConsoleColor.Yellow);
                logStart?.Write(string.Format(" [SetError] Error (\"{0}\") not written as duplicate ErrorID \"{1}\")", error.Text, error.errorID), true, ConsoleColor.Yellow);
                //Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        
        /// <summary>
        /// unused at present
        /// </summary>
        /// <param name="errorID"></param>
        /// <returns></returns>
        internal static Error GetError(int errorID)
        {
            Error error = new Error();
            if (dictErrors.TryGetValue(errorID, out error))
            { return error; }
            return null;
        }

        /// <summary>
        /// return a count of errors
        /// </summary>
        /// <returns></returns>
        internal static int GetErrorCount()
        { return dictErrors.Count; }

        /// <summary>
        /// adds a timer record to the list and resets stopwatch
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="description"></param>
        internal static void StopTimer(Stopwatch timer, string description)
        {
            timer.Stop();
            TimeSpan ts = timer.Elapsed;
            _totalTime += ts.Milliseconds;
            string elapsedTime = String.Format("{0, -30}time elapsed {1} ms", description, ts.Milliseconds);
            listOfTimers.Add(elapsedTime);
            timer.Reset();
        }

        /// <summary>
        ///Generate a list of all Timers
        /// </summary>
        /// <returns></returns>
        internal static List<Snippet> ShowTimersRL()
        {
            List<Snippet> listOfData = new List<Snippet>();
            foreach (string text in listOfTimers)
            { listOfData.Add(new Snippet(text)); }
            listOfData.Add(new Snippet(""));
            listOfData.Add(new Snippet(string.Format("{0, -30}time Elapsed {1} ms", "Total (measured)", _totalTime)));
            return listOfData;
        }

        private static void InitialiseGameVariables()
        {
            gameTurn = 0; //each turn represents a day
            gameRevolt = constant.GetValue(Global.GAME_REVOLT); //year of revolt (old king replaced by the new king)
            gameStart = constant.GetValue(Global.GAME_EXILE) + gameRevolt; //start of game from Player's point of view
            gameExile = constant.GetValue(Global.GAME_EXILE); //time elapsed between revolt and return of the heir (start of game)
            gameYear = gameStart;
            gameGeneration = 1; //current generation (25 years each)
            mapSize = constant.GetValue(Global.MAP_SIZE);
        }

        /// <summary>
        /// Triggers end of game state
        /// </summary>
        /// <param name="text"></param>
        public static void SetEndGame(string text)
        {
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background3;
            logTurn?.Write("--- SetEndGame (Game.cs)");
            logTurn?.Write("[Alert] " + text);
            List<Snippet> msgList = new List<Snippet>();
            msgList.Add(new Snippet(""));
            msgList.Add(new Snippet("You are exiting the Game", RLColor.White, backColor));
            msgList.Add(new Snippet(""));
            msgList.Add(new Snippet(text, foreColor, backColor));
            msgList.Add(new Snippet(""));
            msgList.Add(new Snippet("Press [Y] to confirm, [N], or [ESC], to cancel", foreColor, backColor));
            msgList.Add(new Snippet(""));
            world.SetConfirmation(msgList);
            //_endFlag = true; //preliminary end game, set to true (_endGame = true) once confirmed.
        }

    }
}
