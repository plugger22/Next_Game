using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
using Next_Game.Cartographic;
using Next_Game.Event_System;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum StoryAI { None, Benevolent, Balanced, Evil, Tricky }
    public enum DataPoint { None, Justice, Legend_Usurper, Legend_King, Honour_Usurper, Honour_King, Count } //arrayOfGameStates primary index -> DON"T CHANGE ORDER (mirrored in State.cs)
    public enum DataState { Good, Bad, Change, Count } //arrayOfGameStates secondary index (change indicates item changed since last redraw, +ve # is good, -ve is bad)
    public enum ConflictType { None, Combat, Social, Stealth, Special} //broad category (special is solely for overriding default challenge data -> used by autocreate events only)
    public enum ConflictCombat { None, Personal, Tournament, Battle, Hunting} //sub category -> a copy should be in ConflictSubType
    public enum ConflictSocial { None, Blackmail, Seduce, Befriend} //sub category -> a copy should be in ConflictSubType
    public enum ConflictStealth { None, Infiltrate, Evade, Escape} //sub category -> a copy should be in ConflictSubType
    public enum ConflictSubType { None, Personal, Tournament, Battle, Hunting, Blackmail, Seduce, Befriend, Infiltrate, Evade, Escape, Special} //combined list of all subtypes (add to as needed)
    public enum ConflictSpecial { None, Fortified_Position, Mountain_Country, Forest_Country, Castle_Walls } //special situations
    public enum ConflictResult { None, MinorWin, Win, MajorWin, MinorLoss, Loss, MajorLoss, Count} //result of challenge
    public enum ConflictState { None, Relative_Army_Size, Relative_Fame, Relative_Honour, Relative_Justice, Known_Status } //game specific states that are used for situations
    public enum ResourceLevel { None, Meagre, Moderate, Substantial, Wealthy, Excessive}
   

    /// <summary>
    /// used to store all triggered events for the current turn
    /// </summary>
    public class EventPackage
    {
        public Active Person { get; set; }
        public Event EventObject { get; set; }
        public bool Done { get; set; }
        public int OpponentID { get; set; } //optional
        public int LocationID { get; set; } //optional
        public int HouseID { get; set; } //optional
        public int RefID { get; set; } //optional
    }

    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        static Random rnd;
        Story story;
        State state;
        public int EventAutoID { get; set; } = 2000; //used to provide a unique Player Event ID for auto created events
        public int NumAutoReactEvents { get; set; } = 0; //number of active autoReact events currently out there as Player events
        private int[,] arrayOfGameStates; //tracks game states (enum DataPoints), all are index 0 -> good, index 1 -> bad
        //events
        List<int> listOfActiveGeoClusters; //clusters that have a road through them (GeoID's)
        List<int> listGenFollEventsForest; //generic events for followers
        List<int> listGenFollEventsMountain;
        List<int> listGenFollEventsSea;
        List<int> listGenFollEventsNormal;
        List<int> listGenFollEventsKing;
        List<int> listGenFollEventsConnector;
        List<int> listGenFollEventsCapital;
        List<int> listGenFollEventsMajor;
        List<int> listGenFollEventsMinor;
        List<int> listGenFollEventsInn;
        //archetype follower events
        List<int> listFollRoadEventsNormal;
        List<int> listFollRoadEventsKings;
        List<int> listFollRoadEventsConnector;
        List<int> listFollCapitalEvents;
        //Player generic events
        List<int> listGenPlyrEventsForest;
        List<int> listGenPlyrEventsMountain;
        List<int> listGenPlyrEventsSea;
        List<int> listGenPlyrEventsNormal;
        List<int> listGenPlyrEventsKing;
        List<int> listGenPlyrEventsConnector;
        List<int> listGenPlyrEventsCapital;
        List<int> listGenPlyrEventsMajor;
        List<int> listGenPlyrEventsMinor;
        List<int> listGenPlyrEventsInn;
        /*List<int> listAutoPlyrEventsCapital;
        List<int> listAutoPlyrEventsMajor;
        List<int> listAutoPlyrEventsMinor;
        List<int> listAutoPlyrEventsInn;*/
        //archetype player events
        List<int> listPlyrRoadEventsNormal;
        List<int> listPlyrRoadEventsKings;
        List<int> listPlyrRoadEventsConnector;
        List<int> listPlyrCapitalEvents;
        //other
        List<Follower> listOfFollowers;
        List<EventPackage> listFollCurrentEvents; //follower
        List<EventPackage> listPlyrCurrentEvents; //player
        private Dictionary<int, EventFollower> dictFollowerEvents;
        private Dictionary<int, EventPlayer> dictPlayerEvents;
        private Dictionary<int, EventPlayer> dictAutoEvents;
        private Dictionary<int, Archetype> dictArchetypes;
        private Dictionary<int, Story> dictStories;
        private Dictionary<int, Situation> dictSituationsNormal;
        private Dictionary<int, Situation> dictSituationsGame;
        private Dictionary<int, Situation> dictSituationsSpecial;
        private Dictionary<int, Situation> dictSituationsSkill;
        private Dictionary<int, Result> dictResults;
        private Dictionary<ConflictSubType, Challenge> dictChallenges;


        public Director(int seed)
        {
            rnd = new Random(seed);
            state = new State(seed);
            arrayOfGameStates = new int[(int)DataPoint.Count, (int)DataState.Count];
            //follower generic events
            listOfActiveGeoClusters = new List<int>();
            listGenFollEventsForest = new List<int>();
            listGenFollEventsMountain = new List<int>();
            listGenFollEventsSea = new List<int>();
            listGenFollEventsNormal = new List<int>(); //note that Normal road generic events also apply to all types of Roads (Royal generics -> Royal + Normal, for example)
            listGenFollEventsKing = new List<int>();
            listGenFollEventsConnector = new List<int>();
            listGenFollEventsCapital = new List<int>();
            listGenFollEventsMajor = new List<int>();
            listGenFollEventsMinor = new List<int>();
            listGenFollEventsInn = new List<int>();
            //archetype follower events
            listFollRoadEventsNormal = new List<int>();
            listFollRoadEventsKings = new List<int>();
            listFollRoadEventsConnector = new List<int>();
            listFollCapitalEvents = new List<int>();
            //Player events
            listGenPlyrEventsForest = new List<int>();
            listGenPlyrEventsMountain = new List<int>();
            listGenPlyrEventsSea = new List<int>();
            listGenPlyrEventsNormal = new List<int>();
            listGenPlyrEventsKing = new List<int>();
            listGenPlyrEventsConnector = new List<int>();
            listGenPlyrEventsCapital = new List<int>();
            listGenPlyrEventsMajor = new List<int>();
            listGenPlyrEventsMinor = new List<int>();
            listGenPlyrEventsInn = new List<int>();
            //archetype player events
            listPlyrRoadEventsNormal = new List<int>();
            listPlyrRoadEventsKings = new List<int>();
            listPlyrRoadEventsConnector = new List<int>();
            listPlyrCapitalEvents = new List<int>();
            //other
            listFollCurrentEvents = new List<EventPackage>(); //follower events
            listPlyrCurrentEvents = new List<EventPackage>(); //player events
            listOfFollowers = new List<Follower>();
            dictFollowerEvents = new Dictionary<int, EventFollower>();
            dictPlayerEvents = new Dictionary<int, EventPlayer>();
            dictAutoEvents = new Dictionary<int, EventPlayer>();
            dictArchetypes = new Dictionary<int, Archetype>();
            dictStories = new Dictionary<int, Story>();
            dictSituationsNormal = new Dictionary<int, Situation>(); //first two situations (def. adv. & neutral)
            dictSituationsGame = new Dictionary<int, Situation>(); //third, game specific, situation
            dictSituationsSpecial = new Dictionary<int, Situation>(); //decisions-derived special situations
            dictSituationsSkill = new Dictionary<int, Situation>(); //primary skill involved in a challenge
            dictResults = new Dictionary<int, Result>(); //predefined results of a challenge outcome
            dictChallenges = new Dictionary<ConflictSubType, Challenge>(); //challenge data unique to individual challenge types
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters()); //Run FIRST
            Console.WriteLine(Environment.NewLine + "--- Import Follower Events");
            dictFollowerEvents = Game.file.GetFollowerEvents("Events_Follower.txt");
            Console.WriteLine(Environment.NewLine + "--- Import Player Events");
            dictPlayerEvents = Game.file.GetPlayerEvents("Events_Player.txt");
            Console.WriteLine(Environment.NewLine + "--- Import AutoReact Events");
            AddAutoEvents(Game.file.GetPlayerEvents("Events_AutoReact.txt"));
            InitialiseGenericEvents();
            Console.WriteLine(Environment.NewLine + "--- Import Archetypes"); //Run AFTER importing Events
            dictArchetypes = Game.file.GetArchetypes("Archetypes.txt");
            Console.WriteLine(Environment.NewLine + "--- Import Stories"); //Run AFTER importing Archetypes
            dictStories = Game.file.GetStories("Stories.txt");
            story = SetStory(1); //choose which story to use
            Console.WriteLine(Environment.NewLine + "--- Initialise Archetypes");
            InitialiseArchetypes();
            Console.WriteLine(Environment.NewLine + "--- Initialise Normal Situations");
            dictSituationsNormal = Game.file.GetSituations("SitNormal.txt");
            Console.WriteLine(Environment.NewLine + "--- Initialise Game Specific Situations");
            dictSituationsGame = Game.file.GetSituations("SitGame.txt");
            Console.WriteLine(Environment.NewLine + "--- Initialise Special Situations");
            dictSituationsSpecial = Game.file.GetSituations("SitSpecial.txt");
            Console.WriteLine(Environment.NewLine + "--- Initialise Skill (Situations)");
            dictSituationsSkill = Game.file.GetSituations("SitSkill.txt");
            Console.WriteLine(Environment.NewLine + "--- Initialise Results");
            dictResults = Game.file.GetResults("Results.txt");
            Console.WriteLine(Environment.NewLine + "--- Initialise Challenges"); //run AFTER GetResults
            dictChallenges = Game.file.GetChallenges("Challenge.txt");
            Console.WriteLine(Environment.NewLine);
            InitialiseGameStates();
        }

        /// <summary>
        ///Initialise all game states
        /// </summary>
        private void InitialiseGameStates()
        {
            int multiplier = Game.constant.GetValue(Global.GAME_STATE);
            //Justice -> Old King popularity (charm) - New King
            int popularity = Game.lore.OldKing.GetSkill(SkillType.Charm);
            Game.director.SetGameState(DataPoint.Justice, DataState.Good, popularity * multiplier);
            popularity = Game.lore.NewKing.GetSkill(SkillType.Charm);
            Game.director.SetGameState(DataPoint.Justice, DataState.Bad, popularity * multiplier);
            //Legend_Usurper -> Combat
            int legend = Game.lore.OldHeir.GetSkill(SkillType.Combat);
            if (legend > 3)
            { Game.director.SetGameState(DataPoint.Legend_Usurper, DataState.Good, (legend - 3) * multiplier); }
            else if (legend < 3)
            { Game.director.SetGameState(DataPoint.Legend_Usurper, DataState.Bad, (3 - legend) * multiplier); }
            //Legend_New King -> Combat
            legend = Game.lore.NewKing.GetSkill(SkillType.Combat);
            if (legend > 3)
            { Game.director.SetGameState(DataPoint.Legend_King, DataState.Good, (legend - 3) * multiplier); }
            else if (legend < 3)
            { Game.director.SetGameState(DataPoint.Legend_King, DataState.Bad, (3 - legend) * multiplier); }
            //Honour_Usurper -> Treachery (good is < 3)
            int treachery = Game.lore.OldHeir.GetSkill(SkillType.Treachery);
            if (treachery > 3)
            { Game.director.SetGameState(DataPoint.Honour_Usurper, DataState.Bad, (treachery - 3) * multiplier); }
            else if (treachery < 3)
            { Game.director.SetGameState(DataPoint.Honour_Usurper, DataState.Good, (3 - treachery) * multiplier); }
            //Honour_King -> Treachery (good is < 3)
            treachery = Game.lore.NewKing.GetSkill(SkillType.Treachery);
            if (treachery > 3)
            { Game.director.SetGameState(DataPoint.Honour_King, DataState.Bad, (treachery - 3) * multiplier); }
            else if (treachery < 3)
            { Game.director.SetGameState(DataPoint.Honour_King, DataState.Good, (3 - treachery) * multiplier); }
        }

        /// <summary>
        /// adds imported auto events to Player dictionary
        /// </summary>
        /// <param name="autoDictionary"></param>
        public void AddAutoEvents(Dictionary<int, EventPlayer> autoDictionary)
        {
            if (autoDictionary.Count > 0)
            {
                foreach(var eventObject in autoDictionary)
                {
                    try
                    { dictAutoEvents.Add(eventObject.Value.EventPID, eventObject.Value); Console.WriteLine("\"{0}\" successfully added to DictAutoEvents", eventObject.Value.Name); }
                    catch (ArgumentNullException)
                    { Game.SetError(new Error(117, string.Format("Invalid eventObject (null), eventID {0} in AddAutoEvents", eventObject.Value.EventPID))); }
                    catch (ArgumentException)
                    { Game.SetError(new Error(117, string.Format("Invalid eventObject (duplicate ID), eventPID {0} in AddAutoEvents", eventObject.Value.EventPID))); }
                }
            }
            else { Game.SetError(new Error(117, "Invalid auto Dictionary input (no records)")); }
        }

        /// <summary>
        /// loop all events and place Generic eventID's in their approrpriate lists for both Follower and Player event types
        /// </summary>
        private void InitialiseGenericEvents()
        {
            int eventID;
            //Follower events
            foreach (var eventObject in dictFollowerEvents)
            {
                if (eventObject.Value.Category == EventCategory.Generic)
                {
                    eventID = eventObject.Value.EventFID;
                    switch (eventObject.Value.Type)
                    {
                        case ArcType.GeoCluster:
                            switch (eventObject.Value.GeoType)
                            {
                                case ArcGeo.Forest:
                                    listGenFollEventsForest.Add(eventID);
                                    break;
                                case ArcGeo.Mountain:
                                    listGenFollEventsMountain.Add(eventID);
                                    break;
                                case ArcGeo.Sea:
                                    listGenFollEventsSea.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, string.Format("Invalid Type, ArcGeo, Follower Event, ID {0}", eventID)));
                                    break;
                            }
                            break;
                        case ArcType.Location:
                            switch (eventObject.Value.LocType)
                            {
                                case ArcLoc.Capital:
                                    listGenFollEventsCapital.Add(eventID);
                                    break;
                                case ArcLoc.Major:
                                    listGenFollEventsMajor.Add(eventID);
                                    break;
                                case ArcLoc.Minor:
                                    listGenFollEventsMinor.Add(eventID);
                                    break;
                                case ArcLoc.Inn:
                                    listGenFollEventsInn.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, string.Format("Invalid Type, ArcLoc, Follower Event, ID {0}", eventID)));
                                    break;
                            }
                            break;
                        case ArcType.Road:
                            switch (eventObject.Value.RoadType)
                            {
                                case ArcRoad.Normal:
                                    listGenFollEventsNormal.Add(eventID);
                                    break;
                                case ArcRoad.Kings:
                                    listGenFollEventsKing.Add(eventID);
                                    break;
                                case ArcRoad.Connector:
                                    listGenFollEventsConnector.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, string.Format("Invalid Type, ArcRoad, Follower Event, ID {0}", eventID)));
                                    break;
                            }
                            break;
                        default:
                            Game.SetError(new Error(50, string.Format("Invalid Type, Unknown, Follower Event, ID {0}", eventID)));
                            break;
                    }
                }
            }
            //Player Events
            foreach (var eventObject in dictPlayerEvents)
            {
                if (eventObject.Value.Category == EventCategory.Generic)
                {
                    //assign to the correct event list
                    EventPlayer playerEventObject = eventObject.Value;
                    AssignPlayerEvent(playerEventObject);
                }
            }
        }

        /// <summary>
        /// Assigns Player event to correct eventList (called by InitialiseGenericEvents and AutoReact events)
        /// </summary>
        /// <param name="eventObject"></param>
        public void AssignPlayerEvent(EventPlayer eventObject)
        {
            int eventID;
            //assign to the correct list
                eventID = eventObject.EventPID;
            switch (eventObject.Type)
            {
                case ArcType.GeoCluster:
                    switch (eventObject.GeoType)
                    {
                        case ArcGeo.Forest:
                            listGenPlyrEventsForest.Add(eventID);
                            break;
                        case ArcGeo.Mountain:
                            listGenPlyrEventsMountain.Add(eventID);
                            break;
                        case ArcGeo.Sea:
                            listGenPlyrEventsSea.Add(eventID);
                            break;
                        default:
                            Game.SetError(new Error(50, string.Format("Invalid Type, ArcGeo, Player Event, ID {0}", eventID)));
                            break;
                    }
                    break;
                case ArcType.Location:
                    switch (eventObject.LocType)
                    {
                        case ArcLoc.Capital:
                            listGenPlyrEventsCapital.Add(eventID);
                            break;
                        case ArcLoc.Major:
                            listGenPlyrEventsMajor.Add(eventID);
                            break;
                        case ArcLoc.Minor:
                            listGenPlyrEventsMinor.Add(eventID);
                            break;
                        case ArcLoc.Inn:
                            listGenPlyrEventsInn.Add(eventID);
                            break;
                        default:
                            Game.SetError(new Error(50, string.Format("Invalid Type, ArcLoc, Player Event, ID {0}", eventID)));
                            break;
                    }
                    break;
                case ArcType.Road:
                    switch (eventObject.RoadType)
                    {
                        case ArcRoad.Normal:
                            listGenPlyrEventsNormal.Add(eventID);
                            break;
                        case ArcRoad.Kings:
                            listGenPlyrEventsKing.Add(eventID);
                            break;
                        case ArcRoad.Connector:
                            listGenPlyrEventsConnector.Add(eventID);
                            break;
                        default:
                            Game.SetError(new Error(50, string.Format("Invalid Type, ArcRoad, Player Event, ID {0}", eventID)));
                            break;
                    }
                    break;
                default:
                    Game.SetError(new Error(50, string.Format("Invalid Type, Unknown, Player Event, ID {0}", eventID)));
                    break;
            }
        }

        /// <summary>
        /// check active (Follower only) characters for random events
        /// </summary>
        public void CheckFollowerEvents(Dictionary<int, Active> dictActiveActors)
        {
            //loop all active players
            foreach (var actor in dictActiveActors)
            {
                //not delayed, gone or the Player?
                if (actor.Value is Follower && actor.Value.Status != ActorStatus.Gone && actor.Value.Delay == 0)
                {
                    if (actor.Value.Status == ActorStatus.AtLocation)
                    {
                        //Location event
                        if (rnd.Next(100) <= story.Ev_Follower_Loc)
                        { DetermineFollowerEvent(actor.Value, EventType.Location); }
                    }
                    else if (actor.Value.Status == ActorStatus.Travelling)
                    {
                        //travelling event
                        if (rnd.Next(100) <= story.Ev_Follower_Trav)
                        { DetermineFollowerEvent(actor.Value, EventType.Travelling); }
                    }
                }
            }
        }

        /// <summary>
        /// handles all Player events
        /// </summary>
        public void CheckPlayerEvents()
        {
            Active player = Game.world.GetActiveActor(1);
            if (player != null && player.Status != ActorStatus.Gone && player.Delay == 0)
            {
                //check first if any enemy is about to capture the Player
                if (player.Capture == true)
                {  CreateAutoEnemyEvent(); }
                else
                {
                    //Situation Normal...
                    if (player.Status == ActorStatus.AtLocation)
                    {
                        //Location event
                        if (rnd.Next(100) <= story.Ev_Player_Loc_Current)
                        {
                            DeterminePlayerEvent(player, EventType.Location);
                            //chance of event halved after each occurence (prevents a long string of random events and gives space for necessary system events)
                            story.Ev_Player_Loc_Current /= 2;
                            Console.WriteLine("Chance of Player Location event {0} %", story.Ev_Player_Loc_Current);
                        }
                        else
                        {
                            CreateAutoLocEvent(EventFilter.None);
                            //reset back to base figure
                            story.Ev_Player_Loc_Current = story.Ev_Player_Loc_Base;
                            Console.WriteLine("Chance of Player Location event {0} %", story.Ev_Player_Loc_Current);
                        }
                    }
                    else if (player.Status == ActorStatus.Travelling)
                    {
                        //travelling event
                        if (rnd.Next(100) <= story.Ev_Player_Trav_Base)
                        { DeterminePlayerEvent(player, EventType.Travelling); }
                    }
                }
            }
            else
            { Game.SetError(new Error(71, "Player not found")); }
        }

        /// <summary>
        /// Determine which event applies to a Follower
        /// </summary>
        /// <param name="actor"></param>
        private void DetermineFollowerEvent(Active actor, EventType type)
        {
            int geoID, terrain, road, locID, refID, houseID;
            string tempText;
            Cartographic.Position pos = actor.GetActorPosition();
            List<Event> listEventPool = new List<Event>();
            locID = actor.LocID;
            refID = 0;
            //Location event
            if (type == EventType.Location)
            {
                refID = Game.map.GetMapInfo(Cartographic.MapLayer.RefID, pos.PosX, pos.PosY);
                houseID = Game.map.GetMapInfo(Cartographic.MapLayer.HouseID, pos.PosX, pos.PosY);
                Location loc = Game.network.GetLocation(locID);
                if (locID == 1)
                {
                    //capital
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsCapital));
                    listEventPool.AddRange(GetValidFollowerEvents(listFollCapitalEvents));
                }
                else if (refID > 0 && refID < 100)
                {
                    //Major House
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsMajor));
                    listEventPool.AddRange(GetValidFollowerEvents(loc.GetFollowerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidFollowerEvents(house.GetFollowerEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Major House (refID)")); }
                }
                else if (refID >= 100 && refID < 1000)
                {
                    //Minor House
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsMinor));
                    listEventPool.AddRange(GetValidFollowerEvents(loc.GetFollowerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidFollowerEvents(house.GetFollowerEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Minor House (refID)")); }
                }
                else if (houseID == 99)
                {
                    //Special Location - Inn
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsInn));
                    listEventPool.AddRange(GetValidFollowerEvents(loc.GetFollowerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidFollowerEvents(house.GetFollowerEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Inn (refID)")); }
                }
                else
                { Game.SetError(new Error(52, "Invalid Location Event Type")); }
            }
            //Travelling event
            else if (type == EventType.Travelling)
            {
                //Get map data for actor's current location
                geoID = Game.map.GetMapInfo(Cartographic.MapLayer.GeoID, pos.PosX, pos.PosY);
                terrain = Game.map.GetMapInfo(Cartographic.MapLayer.Terrain, pos.PosX, pos.PosY);
                road = Game.map.GetMapInfo(Cartographic.MapLayer.Road, pos.PosX, pos.PosY);
                GeoCluster cluster = Game.world.GetGeoCluster(geoID);
                //get terrain & road events
                if (locID == 0 && terrain == 1)
                {
                    //mountains
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsMountain));
                    listEventPool.AddRange(GetValidFollowerEvents(cluster.GetFollowerEvents()));
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsForest));
                    listEventPool.AddRange(GetValidFollowerEvents(cluster.GetFollowerEvents()));
                }
                else if (locID == 0 && terrain == 0)
                {
                    //road event
                    if (road == 1)
                    {
                        //normal road
                        listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsNormal));
                        listEventPool.AddRange(GetValidFollowerEvents(listFollRoadEventsNormal));
                    }
                    else if (road == 2)
                    {
                        //king's road
                        listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsKing));
                        listEventPool.AddRange(GetValidFollowerEvents(listFollRoadEventsKings));
                    }
                    else if (road == 3)
                    {
                        //connector road
                        listEventPool.AddRange(GetValidFollowerEvents(listGenFollEventsConnector));
                        listEventPool.AddRange(GetValidFollowerEvents(listFollRoadEventsConnector));
                    }
                }
            }
            //character specific events
            if (actor.GetNumFollowerEvents() > 0)
            { listEventPool.AddRange(GetValidFollowerEvents(actor.GetFollowerEvents())); }
            //choose an event
            if (listEventPool.Count > 0)
            {
                int rndNum = rnd.Next(0, listEventPool.Count);
                Event eventTemp = listEventPool[rndNum];
                EventFollower eventChosen = eventTemp as EventFollower;
                Message message = null; tempText = "";
                if (type == EventType.Travelling)
                {
                    tempText = string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.ShowLocationCoords(actor.LocID),
                      type, eventChosen.Name);
                    message = new Message(tempText, MessageType.Event);
                }
                else if (type == EventType.Location)
                {
                    tempText = string.Format("{0}, Aid {1} at {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                      type, eventChosen.Name);
                    message = new Message(tempText, MessageType.Event);
                }
                if (message != null)
                {
                    Game.world.SetMessage(message);
                    if (String.IsNullOrEmpty(tempText) == false)
                    { Game.world.SetCurrentRecord(new Record(tempText, actor.ActID, actor.LocID, refID, CurrentActorIncident.Event)); }
                }
                else { Game.SetError(new Error(52, "Invalid Message (null)")); }
                //store in list of Current Events
                EventPackage current = new EventPackage() { Person = actor, EventObject = eventChosen, Done = false };
                listFollCurrentEvents.Add(current);
            }
        }

        /// <summary>
        /// Determine which event applies to the Player
        /// </summary>
        /// <param name="actor"></param>
        private void DeterminePlayerEvent(Active actor, EventType type)
        {
            int geoID, terrain, road, locID, refID, houseID;
            string tempText;
            houseID = 0; refID = 0;
            Cartographic.Position pos = actor.GetActorPosition();
            List<Event> listEventPool = new List<Event>();
            locID = Game.map.GetMapInfo(Cartographic.MapLayer.LocID, pos.PosX, pos.PosY);

            //Location event
            if (type == EventType.Location)
            {
                refID = Game.map.GetMapInfo(Cartographic.MapLayer.RefID, pos.PosX, pos.PosY);
                houseID = Game.map.GetMapInfo(Cartographic.MapLayer.HouseID, pos.PosX, pos.PosY);
                Location loc = Game.network.GetLocation(locID);

                if (locID == 1)
                {
                    //capital
                    if (type == EventType.Location)
                    {
                        listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsCapital));
                        listEventPool.AddRange(GetValidPlayerEvents(listPlyrCapitalEvents));
                    }
                }
                else if (refID > 0 && refID < 100)
                {
                    //Major House
                    if (type == EventType.Location)
                    {
                        listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMajor, houseID));
                        listEventPool.AddRange(GetValidPlayerEvents(loc.GetPlayerEvents()));
                        House house = Game.world.GetHouse(refID);
                        if (house != null)
                        { listEventPool.AddRange(GetValidPlayerEvents(house.GetPlayerEvents())); }
                        else { Game.SetError(new Error(72, "Invalid Major House (refID)")); }
                    }
                }
                else if (refID >= 100 && refID < 1000)
                {
                    //Minor House
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMinor, houseID));
                    listEventPool.AddRange(GetValidPlayerEvents(loc.GetPlayerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidPlayerEvents(house.GetPlayerEvents())); }
                    else { Game.SetError(new Error(72, "Invalid Minor House (refID)")); }
                }
                else if (houseID == 99)
                {
                    //Special Location - Inn
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsInn, houseID));
                    listEventPool.AddRange(GetValidPlayerEvents(loc.GetPlayerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidPlayerEvents(house.GetPlayerEvents())); }
                    else { Game.SetError(new Error(72, "Invalid Inn (refID)")); }
                }
                else
                { Game.SetError(new Error(72, "Invalid Location Event Type")); }
            }
            //Travelling event
            else if (type == EventType.Travelling)
            {
                //Get map data for actor's current location
                geoID = Game.map.GetMapInfo(Cartographic.MapLayer.GeoID, pos.PosX, pos.PosY);
                terrain = Game.map.GetMapInfo(Cartographic.MapLayer.Terrain, pos.PosX, pos.PosY);
                road = Game.map.GetMapInfo(Cartographic.MapLayer.Road, pos.PosX, pos.PosY);
                GeoCluster cluster = Game.world.GetGeoCluster(geoID);
                //get terrain & road events
                if (locID == 0 && terrain == 1)
                {
                    //mountains
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMountain, geoID));
                    listEventPool.AddRange(GetValidPlayerEvents(cluster.GetPlayerEvents()));
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsForest, geoID));
                    listEventPool.AddRange(GetValidPlayerEvents(cluster.GetPlayerEvents()));
                }
                else if (locID == 0 && terrain == 0)
                {
                    //road event
                    if (road == 1)
                    {
                        //normal road
                        listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsNormal));
                        listEventPool.AddRange(GetValidPlayerEvents(listPlyrRoadEventsNormal));
                    }
                    else if (road == 2)
                    {
                        //king's road
                        listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsKing));
                        listEventPool.AddRange(GetValidPlayerEvents(listPlyrRoadEventsKings));
                    }
                    else if (road == 3)
                    {
                        //connector road
                        listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsConnector));
                        listEventPool.AddRange(GetValidPlayerEvents(listPlyrRoadEventsConnector));
                    }
                }
            }
            //character specific events
            if (actor.GetNumPlayerEvents() > 0)
            { listEventPool.AddRange(GetValidPlayerEvents(actor.GetPlayerEvents())); }
            //choose an event
            if (listEventPool.Count > 0)
            {
                int rndNum = rnd.Next(0, listEventPool.Count);
                Event eventTemp = listEventPool[rndNum];
                EventPlayer eventChosen = eventTemp as EventPlayer;
                Message message = null; tempText = "";
                if (type == EventType.Travelling)
                {
                    tempText = string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.ShowLocationCoords(actor.LocID),
                      type, eventChosen.Name);
                    message = new Message(tempText, MessageType.Event);
                }
                else if (type == EventType.Location)
                {
                    tempText = string.Format("{0}, Aid {1} at {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                      type, eventChosen.Name);
                    message = new Message(tempText, MessageType.Event);
                }
                if (message != null)
                {
                    Game.world.SetMessage(message);
                    if (String.IsNullOrEmpty(tempText) == false)
                    { Game.world.SetPlayerRecord(new Record(tempText, actor.ActID, actor.LocID, refID, CurrentActorIncident.Event)); }
                }
                else { Game.SetError(new Error(72, "Invalid Message (null)")); }
                //store in list of Current Events
                EventPackage current = new EventPackage() { Person = actor, EventObject = eventChosen, Done = false };
                listPlyrCurrentEvents.Add(current);
            }
            
        }

        /// <summary>
        /// Create a dynamic auto player event (at player's current location) whenever they are about to be captured by an enemy
        /// </summary>
        private void CreateAutoEnemyEvent()
        {
            //get player
            Active player = Game.world.GetActiveActor(1);
            if (player != null)
            {
                //assumes Player.Captured == true and Player.listOfEnemies.Count > 0
                List<int> listEnemies = player.GetListOfEnemies();
                //find enemy with highest threat rating
                int enemyID = 0;
                int highestThreat = 0;
                Enemy enemy = null;
                if (listEnemies.Count > 0)
                {
                    foreach (int actID in listEnemies)
                    {
                        Enemy tempEnemy = Game.world.GetEnemyActor(actID);
                        if (tempEnemy != null)
                        {
                            if (tempEnemy.Threat > highestThreat)
                            { highestThreat = tempEnemy.Threat; enemyID = tempEnemy.ActID; enemy = tempEnemy; }
                        }
                        else { Game.SetError(new Error(173, string.Format("Invalid enemy (null) for ActID \"{0}\"", actID))); }
                    }

                    if (enemy != null)
                    {
                        string eventName = "Unknown";
                        string eventText = "Unknown";
                        //type of enemy
                        if (enemy is Inquisitor)
                        {
                            eventName = "Enemy Afoot";
                            eventText = "The Dark Hooded man staring intently at you is an Inquisitor. " + enemy.Name + " calls on you to YIELD";
                        }
                        else if (enemy is Nemesis)
                        {
                            eventName = "Nemesis Catches Up";
                            eventText = "The Gods must be truly angry as your Nemesis, " + enemy.Name + " is now before you.";
                        }
                        EventPlayer eventObject = new EventPlayer(1000, eventName, EventFrequency.Low)
                        { Category = EventCategory.AutoCreate, Status = EventStatus.Active, Type = ArcType.Actor, Text = eventText };

                        //default option -> Surrender
                        OptionInteractive option_1 = new OptionInteractive("Lay down your Weapons") { ActorID = enemy.ActID };
                        option_1.ReplyGood = string.Format("{0} forcibly restrains you and leads you to the nearest dungeon", enemy.Name);
                        OutNone outcome_1 = new OutNone(eventObject.EventPID);
                        option_1.SetGoodOutcome(outcome_1);
                        eventObject.SetOption(option_1);

                        //fight option
                        OptionInteractive option_2 = new OptionInteractive("Draw your Sword") { ActorID = enemy.ActID }; 
                        option_2.ReplyGood = string.Format("{0} reaches for his weapon and lunges at you", enemy.Name);
                        OutConflict outcome_2 = new OutConflict(eventObject.EventPID, enemy.ActID, ConflictType.Combat) { Combat_Type = ConflictCombat.Personal, SubType = ConflictSubType.Personal };
                        //customise conflict data -> Outcome texts and results
                        string[] overideOutcomes_2 = new string[7] {
                        "Dazed, you manage to escape, barely",
                        "You opponent is left flat footed and winded as you make your escape",
                        "You leave your opponent sprawled, dazed, on the ground as you calmly depart",
                        "You have been captured, but luckily your are uninjured",
                        "You have been captured and sustain minor injuries",
                        "You have been captured and have been badly injured",
                        "Breathing hard, your opponent scowls at you."};
                        outcome_2.challenge.SetOutcomes(overideOutcomes_2);
                        outcome_2.challenge.SetResults(ConflictResult.MinorWin, new List<int> { 28 });
                        outcome_2.challenge.SetResults(ConflictResult.MinorLoss, new List<int> { 45 });
                        outcome_2.challenge.SetResults(ConflictResult.Loss, new List<int> { 45, 28 });
                        outcome_2.challenge.SetResults(ConflictResult.MajorLoss, new List<int> { 45, 42 });
                        outcome_2.challenge.SetOveride(true);
                        option_2.SetGoodOutcome(outcome_2);
                        eventObject.SetOption(option_2);

                        //flee option -> player is the defender
                        OptionInteractive option_3 = new OptionInteractive("Run like the Wind") { ActorID = enemy.ActID };
                        option_3.ReplyGood = string.Format("{0} spits, curses and gives pursuit", enemy.Name);
                        OutConflict outcome_3 = new OutConflict(eventObject.EventPID, enemy.ActID, ConflictType.Stealth, false) { Stealth_Type = ConflictStealth.Evade, SubType = ConflictSubType.Evade };
                        //customise conflict data -> Outcome texts and results
                        string[] overideOutcomes_3 = new string[7] {
                        "It was very close but you've given them the slip. You're exhausted",
                        "You are free and clear",
                        "You are as slippery as an eel. They have no clue where you are",
                        "You tried and failed. You've been captured",
                        "You have been captured and are exhausted",
                        "You have been captured and have been injured",
                        "They know where you are but they can't reach you. It's an impasse."};
                        outcome_3.challenge.SetOutcomes(overideOutcomes_3);
                        outcome_3.challenge.SetResults(ConflictResult.MinorWin, new List<int> { 46 });
                        outcome_3.challenge.SetResults(ConflictResult.MinorLoss, new List<int> { 45 });
                        outcome_3.challenge.SetResults(ConflictResult.Loss, new List<int> { 45, 46});
                        outcome_3.challenge.SetResults(ConflictResult.MajorLoss, new List<int> { 45, 42 });
                        outcome_3.challenge.SetOveride(true);
                        option_3.SetGoodOutcome(outcome_3);
                        eventObject.SetOption(option_3);

                        //Create & Add Event Package
                        EventPackage package = new EventPackage() { Person = player, EventObject = eventObject, Done = false };
                        listPlyrCurrentEvents.Add(package);
                        //if more than the current event present the original one (autocreated) needs to be deleted
                        if (listPlyrCurrentEvents.Count > 1) { listPlyrCurrentEvents.RemoveAt(0); }
                        //add to Player dictionary (ResolveOutcome looks for it there) -> check not an instance present already
                        if (dictPlayerEvents.ContainsKey(1000)) { dictPlayerEvents.Remove(1000); }
                        dictPlayerEvents.Add(1000, eventObject);
                        //message
                        string locText = "Unknown";
                        if (player.Status == ActorStatus.AtLocation) { locText = "at " + Game.world.GetLocationName(player.LocID); }
                        else if (player.Status == ActorStatus.Travelling) { locText = "travelling to " + Game.world.GetLocationName(player.LocID); }
                        string tempText = string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", player.Name, player.ActID, locText, eventObject.Type, eventObject.Name);
                        Message message = new Message(tempText, MessageType.Event);
                        Game.world.SetMessage(message);
                        Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, Game.world.GetRefID(player.LocID), CurrentActorIncident.Event));
                    }
                    else { Game.SetError(new Error(173, "Invalid enemy (null) from search for the highest threat rated enemy loop")); }
                }
                else { Game.SetError(new Error(173, "Invalid listEnemies (Empty)")); }
            }
            else
            { Game.SetError(new Error(173, "Invalid Player (null)")); }
        }

        /// <summary>
        /// create a dynamic auto player location event - assumed to be at player's current location
        /// <param name="filter">Which group of people should the event focus on (from pool of people present at the location)</param>
        /// </summary>
        private void CreateAutoLocEvent(EventFilter filter, int actorID = 0)
        {
            //get player
            Active player = Game.world.GetActiveActor(1);
            if (player != null)
            {
                Console.WriteLine("- What to do");
                List<Actor> listActors = new List<Actor>();
                List<Passive> listLocals = new List<Passive>();
                List<Passive> listAdvisors = new List<Passive>();
                List<Passive> listVisitors = new List<Passive>();
                List<Follower> listFollowers = new List<Follower>();
                int limit; //loop counter, prevents overshooting the # of available function keys
                int locID = player.LocID;
                int locType = 0; //1 - capital, 2 - MajorHouse, 3 - MinorHouse, 4 - Inn
                int talkRel = Game.constant.GetValue(Global.TALK_THRESHOLD);
                string actorText = "unknown"; string optionText = "unknown";
                Location loc = Game.network.GetLocation(locID);
                string locName = Game.world.GetLocationName(locID);
                int houseID = Game.map.GetMapInfo(MapLayer.HouseID, loc.GetPosX(), loc.GetPosY());
                int refID = Game.map.GetMapInfo(MapLayer.RefID, loc.GetPosX(), loc.GetPosY());
                string houseName = "Unknown";
                string tempText;
                //if (houseID > 0) { houseName = Game.world.GetGreatHouseName(houseID); }
                if (refID > 0) { houseName = Game.world.GetHouseName(refID); }
                int testRefID; //which refID (loc) to use when checking who's present
                //what type of location?
                if (locID == 1) { locType = 1; }
                else if (refID > 0 && refID < 100) { locType = 2; }
                else if (refID >= 100 && refID < 1000) { locType = 3; }
                else if (refID >= 1000 && houseID == 99)
                {
                    locType = 4;
                    //can't be locals present at an Inn, only Visitors and Followers
                    if (filter == EventFilter.Locals) { filter = EventFilter.Visitors; Game.SetError(new Error(118, "Invalid filter (Locals when at an Inn)")); }
                }
                else { Game.SetError(new Error(118, "Invalid locType (doesn't fit any criteria)")); }
                //Get actors present at location
                List<int> actorIDList = loc.GetActorList();
                
                if (actorIDList.Count > 0)
                {
                    //get actual actors
                    for (int i = 0; i < actorIDList.Count; i++)
                    {
                        Actor tempActor = Game.world.GetAnyActor(actorIDList[i]);
                        if (tempActor != null)
                        {   //exclude player from list (they are always present) & you
                            if (tempActor.ActID != 1)
                            { listActors.Add(tempActor); Console.WriteLine("\"{0}\", ID {1} added to list of Actors", tempActor.Name, tempActor.ActID); } 
                        }
                        else { Game.SetError(new Error(118, string.Format("Invalid tempActor ID {0} (Null)", actorIDList[i]))); }
                    }
                    //filter actors accordingly
                    for (int i = 0; i < listActors.Count; i++)
                    {
                        Actor actor = listActors[i];
                        if (actor is Passive)
                        {
                            Passive tempPassive = actor as Passive;
                            testRefID = refID;
                            if (locType == 1) { testRefID = Game.lore.RoyalRefIDNew; }
                            if (tempPassive.RefID == testRefID && !(actor is Advisor))
                            {
                                if (tempPassive.Type == ActorType.Lord || tempPassive.Age >= 15)
                                { listLocals.Add(tempPassive); Console.WriteLine("- \"{0}\", ID {1} added to list of Locals", tempPassive.Name, tempPassive.ActID); }
                            }
                            else if (actor is Advisor)
                            { listAdvisors.Add(tempPassive); Console.WriteLine("- \"{0}\", ID {1} added to list of Advisors", tempPassive.Name, tempPassive.ActID); }
                            else
                            {
                                if (tempPassive.Age >= 15)
                                { listVisitors.Add(tempPassive); Console.WriteLine("- \"{0}\", ID {1} added to list of Visitors", tempPassive.Name, tempPassive.ActID); }
                            }
                        }
                        else if (actor is Follower)
                        {
                            Follower tempFollower = actor as Follower;
                            listFollowers.Add(tempFollower);
                            Console.WriteLine("- \"{0}\", ID {1} added to list of Followers", tempFollower.Name, tempFollower.ActID);
                        }
                    }
                    //new event (auto location events always have eventPID of '1000' -> old version in Player dict is deleted before new one added)
                    EventPlayer eventObject = new EventPlayer(1000, "What to do?", EventFrequency.Low) {Category = EventCategory.AutoCreate, Status = EventStatus.Active, Type = ArcType.Location };
                    
                    switch (filter)
                    {
                        case EventFilter.None:
                            eventObject.Text = string.Format("You are at {0}. How will you fill your day?", locName);
                            
                            //option -> audience with local House member
                            if (listLocals.Count() > 0)
                            {
                                OptionInteractive option = null;
                                
                                if (locType != 1)
                                {
                                    option = new OptionInteractive(string.Format("Seek an Audience with a member of House {0} ({1} present)", houseName, listLocals.Count));
                                    option.ReplyGood = string.Format("House {0} is willing to consider the matter", houseName);
                                }
                                else
                                {
                                    //capital
                                    option = new OptionInteractive(string.Format("Seek an Audience with a member of the Royal Household ({0} present)", listLocals.Count));
                                    option.ReplyGood = string.Format("The Royal Clerk has advised that the household is willing to consider the matter");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Locals);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Advisor
                            if (listAdvisors.Count() > 0)
                            {
                                OptionInteractive option = null;
                                if (locType != 1)
                                {
                                    option = new OptionInteractive(string.Format("Seek an Audience with an Advisor to House {0} ({1} present)", houseName, listAdvisors.Count));
                                    option.ReplyGood = string.Format("House {0} is willing to let you talk to whoever you wish", houseName);
                                }
                                else
                                {
                                    //capital
                                    option = new OptionInteractive(string.Format("Seek an Audience with a Royal Advisor ({0} present)", listAdvisors.Count));
                                    option.ReplyGood = string.Format("The Royal Clerk has advised that the household is willing to consider the matter");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Advisors);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Visitor
                            if (listVisitors.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Seek an Audience with a Visitor to House {0} ({1} present)", houseName, listVisitors.Count));
                                option.ReplyGood = string.Format("House {0} is willing to let you talk to whoever you wish", houseName);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Visitors);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Follower
                            if (listFollowers.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Talk to one of your Loyal Followers ({0} present)", listFollowers.Count));
                                option.ReplyGood = "A conversation may well be possible";
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Followers);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> seek information
                            if (player.Known == false )
                            {
                                OptionInteractive option = new OptionInteractive("Ask around for Information");
                                option.ReplyGood = "You make some discreet enquiries";
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutKnown outcome = new OutKnown(eventObject.EventPID, -1);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> lay low
                            if (player.Known == false)
                            {
                                OptionInteractive option = new OptionInteractive("Lay Low");
                                option.ReplyGood = "You find a safe house of a loyal supporter who offers you refuge";
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> Leave
                            OptionInteractive option_L = new OptionInteractive("Leave");
                            option_L.ReplyGood = "It's a mistake to be here. Time to go.";
                            OutNone outcome_L = new OutNone(eventObject.EventPID);
                            option_L.SetGoodOutcome(outcome_L);
                            eventObject.SetOption(option_L);
                            break;
                        case EventFilter.Locals:
                            eventObject.Name = "Talk to Locals";
                            eventObject.Text = string.Format("Which members of House {0} do you wish to talk to?", houseName);
                            //options -> one for each member present
                            limit = listLocals.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (listLocals.Count > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind and decide to Leave");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for(int i = 0; i < limit; i++)
                            {
                                Passive local = listLocals[i];
                                if (local.Office > ActorOffice.None)
                                { actorText = string.Format("{0} {1}", local.Office, local.Name); }
                                else { actorText = string.Format("{0} {1}", local.Type, local.Name); }
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                List<Trigger> listTriggers = new List<Trigger>();
                                listTriggers.Add(new Trigger(TriggerCheck.RelPlyr, local.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                option.SetTriggers(listTriggers);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Advisors:
                            eventObject.Name = "Talk to Advisors";
                            eventObject.Text = string.Format("Which Advisor do you wish to talk to?");
                            //options -> one for each member present
                            limit = listAdvisors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (listAdvisors.Count > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind and decide to Leave");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive local = listAdvisors[i];
                                actorText = string.Format("{0} {1}", local.Title, local.Name);
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                List<Trigger> listTriggers = new List<Trigger>();
                                listTriggers.Add(new Trigger(TriggerCheck.RelPlyr, local.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                option.SetTriggers(listTriggers);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Visitors:
                            eventObject.Name = "Talk to Visitors";
                            eventObject.Text = string.Format("You are at {0}. Which visitor do you wish to talk to?", locName);
                            //options -> one for each member present
                            limit = listVisitors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (listVisitors.Count > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind and decide to Leave");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive visitor = listVisitors[i];
                                actorText = string.Format("{0} {1}", visitor.Title, visitor.Name);
                                //actorText = string.Format("{0} {1}", visitor.Type, visitor.Name);
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = visitor.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                List<Trigger> listTriggers = new List<Trigger>();
                                listTriggers.Add(new Trigger(TriggerCheck.RelPlyr, visitor.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                option.SetTriggers(listTriggers);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Followers:
                            eventObject.Name = "Talk to Followers";
                            eventObject.Text = string.Format("You are at {0}. Which follower do you wish to talk to?", locName);
                            //options -> one for each member present
                            limit = listFollowers.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (listFollowers.Count > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind and decide to Leave");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Follower follower = listFollowers[i];
                                actorText = string.Format("{0} {1}", follower.Type, follower.Name);
                                optionText = string.Format("Find time to talk with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = follower.ActID };
                                option.ReplyGood = string.Format("{0} is happy to sit down for a chat", actorText);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Interact:
                            //inteact with the selected individual
                            if (actorID > 1 && Game.world.CheckActorPresent(actorID, locID ) == true)
                            {
                                Actor person = Game.world.GetAnyActor(actorID);
                                if (person != null)
                                {
                                    if (person is Advisor)
                                    {
                                        Advisor advisor = person as Advisor;
                                        if (advisor.advisorRoyal > AdvisorRoyal.None) { actorText = string.Format("{0} {1}", advisor.advisorRoyal, advisor.Name); }
                                        else { actorText = string.Format("{0} {1}", advisor.advisorNoble, advisor.Name); }
                                    }
                                    else if (person.Office > ActorOffice.None)
                                    { actorText = string.Format("{0} {1}", person.Office, person.Name); }
                                    else { actorText = string.Format("{0} {1}", person.Type, person.Name); }
                                    eventObject.Name = "Interact";
                                    eventObject.Text = string.Format("How would you like to interact with {0}?", actorText);
                                    //default
                                    OptionInteractive option_0 = new OptionInteractive("Make yourself known") { ActorID = actorID };
                                    option_0.ReplyGood = string.Format("{0} acknowledges your presence", actorText);
                                    OutNone outcome_0 = new OutNone(eventObject.EventPID);
                                    option_0.SetGoodOutcome(outcome_0);
                                    eventObject.SetOption(option_0);
                                    //befriend
                                    OptionInteractive option_1 = new OptionInteractive("Befriend") { ActorID = actorID };
                                    option_1.ReplyGood = string.Format("{0} looks at you expectantly", actorText);
                                    List<Trigger> listTriggers_1 = new List<Trigger>();
                                    listTriggers_1.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), 50, EventCalc.GreaterThanOrEqual));
                                    option_1.SetTriggers(listTriggers_1);
                                    OutConflict outcome_1 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Befriend, SubType = ConflictSubType.Befriend};
                                    option_1.SetGoodOutcome(outcome_1);
                                    eventObject.SetOption(option_1);
                                    //blackmail
                                    OptionInteractive option_2 = new OptionInteractive("Blackmail") { ActorID = actorID };
                                    option_2.ReplyGood = string.Format("{0} frowns, their expression darkens", actorText);
                                    List<Trigger> listTriggers_2 = new List<Trigger>();
                                    listTriggers_2.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), 40, EventCalc.GreaterThanOrEqual));
                                    option_2.SetTriggers(listTriggers_2);
                                    OutConflict outcome_2 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Blackmail, SubType = ConflictSubType.Blackmail};
                                    option_2.SetGoodOutcome(outcome_2);
                                    eventObject.SetOption(option_2);
                                    //seduce
                                    OptionInteractive option_3 = new OptionInteractive("Seduce") { ActorID = actorID };
                                    option_3.ReplyGood = string.Format("{0} flutters their eyelids at you", actorText);
                                    List<Trigger> listTriggers_3 = new List<Trigger>();
                                    listTriggers_3.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), 60, EventCalc.GreaterThanOrEqual));
                                    listTriggers_3.Add(new Trigger(TriggerCheck.Sex, 0, (int)person.Sex, EventCalc.NotEqual)); //must be opposite sex
                                    option_3.SetTriggers(listTriggers_3);
                                    OutConflict outcome_3 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Seduce, SubType = ConflictSubType.Seduce};
                                    option_3.SetGoodOutcome(outcome_3);
                                    eventObject.SetOption(option_3);
                                    //support
                                    OptionInteractive option_4 = new OptionInteractive("Ask for their Allegiance") { ActorID = actorID };
                                    option_4.ReplyGood = string.Format("{0} kneels at your feet", actorText);
                                    List<Trigger> listTriggers_4 = new List<Trigger>();
                                    listTriggers_4.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), 70, EventCalc.GreaterThanOrEqual));
                                    listTriggers_4.Add(new Trigger(TriggerCheck.ActorType, (int)person.Type, (int)ActorType.Lord, EventCalc.Equals)); //must be a Lord
                                    option_4.SetTriggers(listTriggers_4);
                                    OutNone outcome_4 = new OutNone(eventObject.EventPID);
                                    option_4.SetGoodOutcome(outcome_4);
                                    eventObject.SetOption(option_4);
                                    //gift
                                    OptionInteractive option_5 = new OptionInteractive("Give Information or a Gift") { ActorID = actorID };
                                    option_5.ReplyGood = string.Format("{0} thanks you for your gift", actorText);
                                    OutNone outcome_5 = new OutNone(eventObject.EventPID);
                                    option_5.SetGoodOutcome(outcome_5);
                                    eventObject.SetOption(option_5);
                                }
                                else { Game.SetError(new Error(73, "Invalid actorID from AutoCreateEvent (null from dict)")); }
                            }
                            break;
                        default:
                            Game.SetError(new Error(118, string.Format("Invalid EventFilter (\"{0}\")", filter)));
                            break;
                    }
                    //Create & Add Event Package
                    EventPackage package = new EventPackage() { Person = player, EventObject = eventObject, Done = false };
                    listPlyrCurrentEvents.Add(package);
                    //if more than the current event present the original one (autocreated) needs to be deleted
                    if (listPlyrCurrentEvents.Count > 1) { listPlyrCurrentEvents.RemoveAt(0); }
                    
                    //add to Player dictionary (ResolveOutcome looks for it there) -> check not an instance present already
                    if (dictPlayerEvents.ContainsKey(1000)) { dictPlayerEvents.Remove(1000); }
                    dictPlayerEvents.Add(1000, eventObject);
                    //message
                    tempText = string.Format("{0}, Aid {1} at {2} {3}, [{4} Event] \"{5}\"", player.Name, player.ActID, locName, Game.world.ShowLocationCoords(player.LocID),
                          eventObject.Type, eventObject.Name);
                    Message  message = new Message(tempText, MessageType.Event);
                    Game.world.SetMessage(message);
                    Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Event));
                }
                else { Game.SetError(new Error(118, "Invalid List of Actors (Zero present at Location")); }
            }
            else { Game.SetError(new Error(118, "Invalid Player (returns Null)")); }
        }

        /// <summary>
        /// clean up events
        /// </summary>
        public void HousekeepEvents()
        {
            //remove any dormant AutoReact Events from Player event Lists and the master dictionary. Only do so if the are above a certain # (avoid processing overhead)
            if (NumAutoReactEvents > 10)
            {
                List<int> tempList = new List<int>(); //temp list to hold eventPID of deleted events
                //check dictionary first
                Console.WriteLine("- Director Housekeeping");
                int counter = 0;
                foreach (var eventObject in dictPlayerEvents)
                {
                    //looking for Dormant, AutoReact events 
                    if (eventObject.Value.Status == EventStatus.Dormant && eventObject.Value.EventPID >= 2000)
                    {
                        tempList.Add(eventObject.Value.EventPID);
                        counter++;
                        Console.WriteLine("\"{0}\" autoReact Event found, Status {1}", eventObject.Value.Name, eventObject.Value.Status);
                    }
                    
                }
                //any to remove?
                if (counter > 0)
                {
                    foreach (int eventID in tempList)
                    {
                        //remove from dictionary
                        if (dictPlayerEvents.Remove(eventID) == true)
                        {
                            Console.WriteLine("\"eventID {0}\" Player Event has been removed from the dictPlayerEvents", eventID);
                            NumAutoReactEvents--;
                        }
                    }
                    //remove from lists (asumes that each individual eventID will be present in only one list)
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        if (listGenPlyrEventsCapital.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsCapital", i); continue; }
                        if (listGenPlyrEventsMajor.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsMajor", i); continue; }
                        if (listGenPlyrEventsMinor.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsMinor", i); continue; }
                        if (listGenPlyrEventsInn.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsInn", i); continue; }
                        if (listGenPlyrEventsForest.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsForest", i); continue; }
                        if (listGenPlyrEventsMountain.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsMountain", i); continue; }
                        if (listGenPlyrEventsSea.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsSea", i); continue; }
                        if (listGenPlyrEventsNormal.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsNormal", i); continue; }
                        if (listGenPlyrEventsKing.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsKing", i); continue; }
                        if (listGenPlyrEventsConnector.Remove(tempList[i]) == true) { Console.WriteLine("EventPID {0} removed from listGenPlyrEventsConnector", i); continue; }
                        //any hit above would have skipped this code
                        Game.SetError(new Error(126, string.Format("Warning! EventPID {0} wasn't found in any list (HousekeepEvents, tidy up AutoReact events", i)));
                    }
                    NumAutoReactEvents = Math.Max(0, NumAutoReactEvents);
                }
            }
            //Remove any existing autoLoc created player events prior to next turn (Process end of turn)
            if (dictPlayerEvents.ContainsKey(1000)) { dictPlayerEvents.Remove(1000); }
            //clear out current events
            listFollCurrentEvents.Clear();
            listPlyrCurrentEvents.Clear();
        }

        /// <summary>
        /// Extracts all valid Follower events from a list of EventID's
        /// </summary>
        /// <param name="listEventID"></param>
        /// <returns></returns>
        private List<Event> GetValidFollowerEvents(List<int> listEventID)
        {
            int frequency;
            List<Event> listEvents = new List<Event>();
            if (listEventID != null)
            {
                foreach (int eventID in listEventID)
                {
                    Event eventObject = dictFollowerEvents[eventID];
                    if (eventObject != null && eventObject.Status == EventStatus.Active)
                    {
                        frequency = (int)eventObject.Frequency;
                        //add # of events to pool equal to (int)EventFrequency
                        for (int i = 0; i < frequency; i++)
                        { listEvents.Add(eventObject); }
                    }
                }
            }
            return listEvents;
        }

        /// <summary>
        /// Extracts all valid Player events from a list of EventID's
        /// </summary>
        /// <param name="listEventID"></param>
        /// <param name="data">optional paramater that is multipurpose, eg. houseID for a location</param>
        /// <returns></returns>
        private List<Event> GetValidPlayerEvents(List<int> listEventID, int data = 0)
        {
            int frequency;
            List<Event> listEvents = new List<Event>();
            if (listEventID != null)
            {
                foreach (int eventID in listEventID)
                {
                    Event eventObject = dictPlayerEvents[eventID];
                    if (eventObject != null && eventObject.Status == EventStatus.Active && eventObject.TimerCoolDown == 0)
                    {
                        bool proceed = true;
                        //is the event limited in any way?
                        if (eventObject.SubRef > 0)
                        {
                            //if data > 0 then SubRef must match the HouseID or GeoID in order for event to qualify
                            if (data > 0)
                            {
                                if (eventObject.LocType > ArcLoc.None)
                                {
                                    if (data != eventObject.SubRef) { proceed = false; Console.WriteLine("Event \"{0}\" failed HouseID check", eventObject.Name); }
                                    else { Console.WriteLine("Event \"{0}\" PASSED HouseID check", eventObject.Name); }
                                }
                                else if (eventObject.GeoType > ArcGeo.None)
                                {
                                    if (data != eventObject.SubRef) { proceed = false; Console.WriteLine("Event \"{0}\" failed GeoID check", eventObject.Name); }
                                    else { Console.WriteLine("Event \"{0}\" PASSED HouseID check", eventObject.Name); }
                                }
                            }
                        }
                        {
                            if (proceed == true)
                            {
                                frequency = (int)eventObject.Frequency;
                                //add # of events to pool equal to (int)EventFrequency
                                for (int i = 0; i < frequency; i++)
                                { listEvents.Add(eventObject); }
                            }
                        }
                    }
                }
            }
            return listEvents;
        }

        /// <summary>
        /// returns an Event from follower dict, null if not found
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        internal EventFollower GetFollowerEvent(int eventID)
        {
            EventFollower eventObject = null;
            if (dictFollowerEvents.TryGetValue(eventID, out eventObject))
            { return eventObject; }
            return eventObject;
        }

        /// <summary>
        /// returns an Event from Player dict, null if not found
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        internal EventPlayer GetPlayerEvent(int eventID)
        {
            EventPlayer eventObject = null;
            if (dictPlayerEvents.TryGetValue(eventID, out eventObject))
            { return eventObject; }
            return eventObject;
        }

        /// <summary>
        /// returns an Auto Event archetype from Auto Event dict, null if not found
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        internal EventPlayer GetAutoEvent(int eventID)
        {
            EventPlayer eventObject = null;
            if (dictAutoEvents.TryGetValue(eventID, out eventObject))
            { return eventObject; }
            return eventObject;
        }


        /// <summary>
        /// Resolve current Follower events one at a time. Returns true if event present to be processed, false otherwise.
        /// </summary>
        public bool ResolveFollowerEvents()
        {
            bool returnValue = false;
            int ability, rndNum, success;
            int traitMultiplier = Game.constant.GetValue(Global.TRAIT_MULTIPLIER);
            string effectText, status;
            List<Snippet> eventList = new List<Snippet>();
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background1;
            RLColor traitColor;
            //loop all triggered events for this turn
            for (int i = 0; i < listFollCurrentEvents.Count; i++)
            {
                EventPackage package = listFollCurrentEvents[i];
                if (package.Done == false)
                {
                    EventFollower eventObject = (EventFollower)package.EventObject;
                    List<OptionAuto> listOptions = eventObject.GetOptions();
                    //assume only a single option
                    OptionAuto option = null;
                    if (listOptions != null) { option = listOptions[0]; }
                    else { Game.SetError(new Error(70, "Invalid ListOfOptions input (null)")); break; }
                    Active actor = package.Person;
                    //create event description
                    Position pos = actor.GetActorPosition();
                    switch (eventObject.Type)
                    {
                        case ArcType.GeoCluster:
                        case ArcType.Road:
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}, at Loc {2}:{3} travelling towards {4}", actor.Name, actor.ActID, pos.PosX, pos.PosY,
                                Game.world.GetLocationName(actor.LocID)), RLColor.LightGray, backColor));
                            break;
                        case ArcType.Location:
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2} (Loc {3}:{4})", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                                pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                            break;
                        case ArcType.Actor:
                            if (actor.Status == ActorStatus.AtLocation) { status = Game.world.GetLocationName(actor.LocID) + " "; }
                            else { status = null; }
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2}(Loc {3}:{4})", actor.Name, actor.ActID, status,
                                pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                            break;
                    }
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet(eventObject.Text, foreColor, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    //resolve event and add to description (add delay to actor if needed)
                    eventList.Add(new Snippet(string.Format("A test of {0}", option.Trait), RLColor.Brown, backColor));
                    eventList.Add(new Snippet(""));
                    effectText = actor.GetTraitEffectText(option.Trait);
                    ability = actor.GetSkill(option.Trait);
                    rndNum = rnd.Next(100);
                    success = ability * traitMultiplier;
                    //trait stars
                    if (ability < 3) { traitColor = RLColor.LightRed; }
                    else if (ability == 3) { traitColor = RLColor.Gray; }
                    else { traitColor = RLColor.Green; }
                    //enables stars to be centred
                    if (ability != 3)
                    { eventList.Add(new Snippet(string.Format("({0} {1})  {2} {3} {4}", ability, ability == 1 ? "Star" : "Stars",
                        Game.world.GetStars(ability), actor.arrayOfTraitNames[(int)option.Trait],
                        effectText), traitColor, backColor)); }
                    else
                    { eventList.Add(new Snippet(string.Format("({0} {1})  {2}", ability, ability == 1 ? "Star" : "Stars", Game.world.GetStars(ability)), traitColor, backColor)); }
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet(string.Format("Success on {0}% or less", success), RLColor.Brown, backColor));
                    eventList.Add(new Snippet(""));
                    if (rndNum < success)
                    {
                        //success
                        eventList.Add(new Snippet(string.Format("Roll {0}", rndNum), RLColor.Gray, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet(string.Format("{0} {1}", actor.Name, option.ReplyGood), RLColor.Black, backColor));
                        //outcomes
                        List<Outcome> listGoodOutcomes = option.GetGoodOutcomes();
                        //ignore if none present
                        if (listGoodOutcomes != null && listGoodOutcomes.Count > 0)
                        {
                            //Loop outcomes (can be multiple)
                            for (int k = 0; k < listGoodOutcomes.Count; k++)
                            {
                                Outcome outTemp = listGoodOutcomes[k];
                                //Type of Outcome
                                if (outTemp is OutDelay)
                                {
                                    /*
                                    SAMPLE PLACEHOLDER CODE
                                    */
                                }
                                else
                                {
                                    //fault condition
                                    Game.SetError(new Error(70, "Invalid Good Outcome Type (not covered by code)"));
                                    eventList.Add(new Snippet(""));
                                    eventList.Add(new Snippet("NO VALID GOOD OUTCOME PRESENT", RLColor.LightRed, backColor));
                                }
                            }
                        }
                    }
                    else
                    {
                        //failure
                        eventList.Add(new Snippet(string.Format("Roll {0}", rndNum), RLColor.LightRed, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet(string.Format("{0} {1}", actor.Name, option.ReplyBad), RLColor.Black, backColor));
                        //outcomes
                        List<Outcome> listBadOutcomes = option.GetBadOutcomes();
                        if (listBadOutcomes != null && listBadOutcomes.Count > 0)
                        {
                            //Loop outcomes (can be multiple)
                            for (int k = 0; k < listBadOutcomes.Count; k++)
                            {
                                Outcome outTemp = listBadOutcomes[k];
                                //Type of Outcome
                                if (outTemp is OutDelay)
                                {
                                    //Delay
                                    OutDelay outcome = outTemp as OutDelay;
                                    outcome.Resolve(actor.ActID);
                                    eventList.Add(new Snippet(""));
                                    eventList.Add(new Snippet(string.Format("{0} has been {1} for {2} {3}", actor.Name, eventObject.Type == ArcType.Location ? "indisposed" : "delayed",
                                        outcome.Delay, outcome.Delay == 1 ? "Day" : "Day's"), RLColor.LightRed, backColor));
                                    eventList.Add(new Snippet(""));
                                }
                                else
                                {
                                    //fault condition
                                    Game.SetError(new Error(70, "Invalid Bad Outcome Type (not covered by code)"));
                                    eventList.Add(new Snippet(""));
                                    eventList.Add(new Snippet("NO VALID BAD OUTCOME PRESENT", RLColor.LightRed, backColor));
                                }
                            }
                        }
                        else
                        {
                            //no bad outcomes present
                            Game.SetError(new Error(70, "Invalid ListOfBadOutcomes input (null)"));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("NO VALID BAD OUTCOME PRESENT", RLColor.LightRed, backColor));
                        }
                    }
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("Press ENTER or ESC to continue", RLColor.LightGray, backColor));
                    //housekeeping
                    Game.infoChannel.SetInfoList(eventList, ConsoleDisplay.Event);
                    returnValue = true;
                    package.Done = true;
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// returns true if there are any Follower Events still to be resolved this turn
        /// </summary>
        /// <returns></returns>
        public bool CheckRemainingFollowerEvents()
        {
            for (int i = 0; i < listFollCurrentEvents.Count; i++)
            {
                EventPackage package = listFollCurrentEvents[i];
                if (package.Done == false)
                { return true; }
            }
            return false;
        }

        /// <summary>
        /// returns true if there are any Player Events still to be resolved this turn
        /// </summary>
        /// <returns></returns>
        public bool CheckRemainingPlayerEvents()
        {
            for (int i = 0; i < listPlyrCurrentEvents.Count; i++)
            {
                EventPackage package = listPlyrCurrentEvents[i];
                if (package.Done == false)
                { return true; }
            }
            return false;
        }

        /// <summary>
        /// Resolve current Player events one at a time. Returns true if event present to be processed, false otherwise.
        /// </summary>
        public bool ResolvePlayerEvents()
        {
            bool returnValue = false;
            string status;
            List<Snippet> eventList = new List<Snippet>();
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background1;
            //loop all triggered events for this turn
            for (int i = 0; i < listPlyrCurrentEvents.Count; i++)
            {
                EventPackage package = listPlyrCurrentEvents[i];
                if (package.Done == false)
                {
                    EventPlayer eventObject = (EventPlayer)package.EventObject;
                    Active actor = package.Person;
                    Game._eventID = eventObject.EventPID;
                    //create event description
                    Position pos = actor.GetActorPosition();
                    switch (eventObject.Type)
                    {
                        case ArcType.GeoCluster:
                        case ArcType.Road:
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}, at Loc {2}:{3} travelling towards {4}", actor.Name, actor.ActID, pos.PosX, pos.PosY,
                                Game.world.GetLocationName(actor.LocID)), RLColor.LightGray, backColor));
                            break;
                        case ArcType.Location:
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2} (Loc {3}:{4})", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                                pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                            break;
                        case ArcType.Actor:
                            if (actor.Status == ActorStatus.AtLocation) { status = Game.world.GetLocationName(actor.LocID) + " "; }
                            else { status = null; }
                            eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2}(Loc {3}:{4})", actor.Name, actor.ActID, status,
                                pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                            break;
                    }
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet(eventObject.Text, foreColor, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    //display options (F1, F2, F3 ...)
                    List<OptionInteractive> listOptions = eventObject.GetOptions();
                    string optionText;
                    int ctr = 1;
                    int maxWidth = 0;
                    RLColor optionColor;
                    if (listOptions != null)
                    {
                        foreach(OptionInteractive option in listOptions)
                        {
                            //check any option triggers 
                            if (CheckOption(option) == true)
                            { optionColor = RLColor.Blue; option.Active = true; }
                            else
                            {
                                //invalid triggers, option shown greyed out and unusable
                                optionColor = RLColor.LightGray;
                                option.Active = false;
                            }
                            optionText = string.Format("[F{0}]  {1}", ctr++, option.Text);
                            if (optionText.Length > maxWidth) { maxWidth = optionText.Length; }
                            eventList.Add(new Snippet(string.Format("{0, -40}", optionText), optionColor, backColor));
                            eventList.Add(new Snippet(""));
                        }
                    }
                    else { Game.SetError(new Error(70, "Invalid ListOfOptions Player input (null)")); break; }
                    
                    //repeat timer (set # of activations till event goes dormant)
                    if (eventObject.TimerRepeat > 0)
                    {
                        eventObject.TimerRepeat--;
                        Console.WriteLine("Event \"{0}\" Timer Repeat now {1}", eventObject.Name, eventObject.TimerRepeat);
                        //if repeat timer has run down to 0, the event is no longer active
                        if (eventObject.TimerRepeat == 0)
                        {
                            eventObject.Status = EventStatus.Dormant;
                            Console.WriteLine("Event \"{0}\" Timer Repeat has run down to Zero. Event is now {1}", eventObject.Name, eventObject.Status);
                        }
                    }
                    //reset cool down timer
                    eventObject.TimerCoolDown = eventObject.TimerCoolBase;
                    Console.WriteLine("Event \"{0}\" Cooldown Timer has been reset to {1}", eventObject.Name, eventObject.TimerCoolBase);
                    //info at bottom
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("Press ENTER or ESC to ignore this event", RLColor.LightGray, backColor));
                    //housekeeping
                    Game.infoChannel.SetInfoList(eventList, ConsoleDisplay.Event);
                    returnValue = true;
                    package.Done = true;
                    break;
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Checks any triggers for the option and determines if it's active (triggers are all good -> returns true), or not
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private bool CheckOption(OptionInteractive option)
        {
            bool validCheck = true;
            List<Trigger> listTriggers = option.GetTriggers();
            if (listTriggers.Count > 0)
            {
                Active player = Game.world.GetActiveActor(1);
                //check each trigger
                foreach (Trigger trigger in listTriggers)
                {
                    switch (trigger.Check)
                    {
                        case TriggerCheck.None:
                            break;
                        case TriggerCheck.Trait:
                            SkillType type;
                            try
                            { type = (SkillType)trigger.Data; }
                            catch
                            {
                                //set to combat to get the job done but generate an error
                                type = SkillType.Combat;
                                Game.SetError(new Error(76, string.Format("Invalid Trigger Data (\"{0}\"), default Combat trait used instead, for Option \"{1}\"", trigger.Data, option.Text)));
                            }
                            Console.WriteLine("\"{0}\" {1} Trigger, if type {2} is {3} to {4}", option.Text, trigger.Check, trigger.Data, trigger.Calc, trigger.Threshold);
                            if (CheckTrigger(player.GetSkill(type), trigger.Calc, trigger.Threshold) == false) { return false; }
                            break;
                        case TriggerCheck.GameVar:
                            //NOT YET IMPLEMENTED
                            break;
                        case TriggerCheck.RelPlyr:
                            if (CheckTrigger(trigger.Data, trigger.Calc, trigger.Threshold) == false) { return false; }
                            break;
                        case TriggerCheck.Sex:
                            //Threshold = (int)ActorSex -> Male 1, Female 2 (sex of actor). Must be opposite sex (seduction
                            if (CheckTrigger((int)player.Sex, trigger.Calc, trigger.Threshold) == false) { Console.WriteLine("Trigger: Same sex, Seduction not possible"); return false; }
                            break;
                        case TriggerCheck.ActorType:
                            //Data = ActorType, Threshold is required type. Must be equal
                            if (CheckTrigger(trigger.Data, trigger.Calc, trigger.Threshold) == false) { Console.WriteLine("Trigger: Incorrect ActorType"); return false; }
                            break;
                        case TriggerCheck.ResourcePlyr:
                            if (CheckTrigger(player.Resources, trigger.Calc, trigger.Threshold) == false) { Console.WriteLine("Trigger: Player has wrong amount of Resources"); return false; }
                            break;
                        default:
                            Game.SetError(new Error(76, string.Format("Invalid Trigger Check Type (\"{0}\") for Option \"{1}\"", trigger.Check, option.Text)));
                            break;
                    }
                }
            }
            return validCheck;
        }

        /// <summary>
        /// Checks the validity of any trigger (if passes -> return true)
        /// </summary>
        /// <param name="data">any number</param>
        /// <param name="comparator"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private bool CheckTrigger(int data, EventCalc comparator, int threshold)
        {
            bool validCheck = true;
            switch(comparator)
            {
                case EventCalc.GreaterThanOrEqual:
                    if (data < threshold) { validCheck = false; }
                    break;
                case EventCalc.LessThanOrEqual:
                    if (data > threshold) { validCheck = false; }
                    break;
                case EventCalc.Equals:
                    if (data != threshold) { validCheck = false; }
                    break;
                case EventCalc.NotEqual:
                    if (data == threshold) { validCheck = false; }
                    break;
                case EventCalc.Add:
                case EventCalc.Subtract:
                case EventCalc.Random:
                    break;
                default:
                    Game.SetError(new Error(77, string.Format("Invalid Trigger Calculation Type (\"{0}\")", comparator)));
                    validCheck = false;
                    break;
            }
            Console.WriteLine("Trigger {0} on \"{1}\" {2} {3}", validCheck == true ? "passed" : "FAILED", data, comparator, threshold);
            return validCheck;
        }

        /// <summary>
        /// handles outcome resolution for player events, eg. player has chosen option 2 (pressed 'F2').Return '1' if true, '2' if a conflict, '0' if false.
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="optionNum"></param>
        public int ResolveOutcome(int eventID, int optionNum)
        {
            int validOption = 1;
            int actorID;
            string status;
            string outcomeText = "";
            string tempText = "";
            List<Snippet> eventList = new List<Snippet>();
            List<Snippet> resultList = new List<Snippet>();
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background1;
            //Get eVent
            EventPlayer eventObject = GetPlayerEvent(eventID);
            if (eventObject != null)
            {
                //find option
                List<OptionInteractive> listOptions = eventObject.GetOptions();
                string optionReply = "unknown option Reply";
                if (listOptions != null)
                {
                    if (listOptions.Count >= optionNum)
                    {
                        OptionInteractive option = listOptions[optionNum - 1];
                        Active player = Game.world.GetActiveActor(1);
                        int refID = Game.world.GetRefID(player.LocID);
                        //Active option?
                        if (option.Active == true)
                        {
                            List<Outcome> listOutcomes = new List<Outcome>();
                            //resolve option -> Is it a variable (chance of a good or bad outcome) option?
                            if (option.Test > 0)
                            {
                                if (String.IsNullOrEmpty(option.ReplyBad) == true)
                                { Game.SetError(new Error(73, string.Format("Invalid Test option (no ReplyBad) for \"{0}\"", option.Text))); }
                                else
                                {
                                    int rndNum = rnd.Next(0, 100);
                                    if (rndNum <= option.Test)
                                    {
                                        listOutcomes = option.GetGoodOutcomes(); optionReply = option.ReplyGood;
                                        Console.WriteLine("Option \"{0}\" Passed test ({1} % needed, rolled {2})", option.Text, option.Test, rndNum);
                                    }
                                    else
                                    {
                                        listOutcomes = option.GetBadOutcomes(); optionReply = option.ReplyBad;
                                        Console.WriteLine("Option \"{0}\" Failed test ({1} % needed, rolled {2})", option.Text, option.Test, rndNum);
                                    }
                                }
                            }
                            else { listOutcomes = option.GetGoodOutcomes(); optionReply = option.ReplyGood; }
                            //resolve each Outcome
                            if (listOutcomes != null)
                            {
                                foreach (Outcome outcome in listOutcomes)
                                {
                                    switch (outcome.Type)
                                    {
                                        case OutcomeType.Game:
                                            //Change a Game state variable, eg. Honour, Justice, etc.
                                            outcomeText = state.SetState(eventObject.Name, option.Text, outcome.Data, outcome.Amount, outcome.Calc);
                                            if (String.IsNullOrEmpty(outcomeText) == false)
                                            { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                            break;
                                        case OutcomeType.Known:
                                            //change an Active Actor's Known/Unknown status
                                            outcomeText = Game.world.SetActiveActorKnownStatus(1, outcome.Data);
                                            if (String.IsNullOrEmpty(outcomeText) == false)
                                            { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                            //message
                                            tempText = string.Format("Event \"{0}\", Option \"{1}\", {2}", eventObject.Name, option.Text, outcomeText);
                                            Message messageKnown = new Message(tempText, 1, 0, MessageType.Event);
                                            Game.world.SetMessage(messageKnown);
                                            Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Event));
                                            break;
                                        case OutcomeType.Freedom:
                                            //change a player's status
                                            if (outcome.Data > 0)
                                            {
                                                //only valid if Player is already captured -> must be at a location
                                                if (player.Status == ActorStatus.Captured)
                                                { player.Status = ActorStatus.AtLocation; }
                                                else { Game.SetError(new Error(73, "Player Status isn't currently 'Captured' (Outcome)")); }
                                            }
                                            else if (outcome.Data < 0) { Game.world.SetPlayerCaptured(option.ActorID); }
                                            else { Game.SetError(new Error(73, "Invalid Data value (zero) for OutcomeType -> Freedom")); }
                                            break;
                                        case OutcomeType.EventTimer:
                                            //Change an Event Timer
                                            OutEventTimer timerOutcome = outcome as OutEventTimer;
                                            outcomeText = ChangePlayerEventTimer(timerOutcome);
                                            if (String.IsNullOrEmpty(outcomeText) == false)
                                            { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                            break;
                                        case OutcomeType.EventStatus:
                                            //change Event Status
                                            OutEventStatus statusOutcome = outcome as OutEventStatus;
                                            outcomeText = ChangePlayerEventStatus(statusOutcome.Data, statusOutcome.NewStatus);
                                            if (String.IsNullOrEmpty(outcomeText) == false)
                                            { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                            break;
                                        case OutcomeType.Resource:
                                            //adjust the resource level of Player or an NPC actor
                                            OutResource resourceOutcome = outcome as OutResource;
                                            if (resourceOutcome.PlayerRes == false) { actorID = option.ActorID; }
                                            else { actorID = 1; }
                                            Actor personRes = Game.world.GetAnyActor(actorID);
                                            if (personRes != null)
                                            {
                                                outcomeText = personRes.ChangeResources(resourceOutcome.Amount, resourceOutcome.Calc);
                                                if (String.IsNullOrEmpty(outcomeText) == false)
                                                { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                            }
                                            break;
                                        case OutcomeType.Condition:
                                            //apply a condition to the Player or an NPC actor -> use copy constructor to pass by value, not reference (otherwise all timers are indentical)
                                            OutCondition conditionOutcome = new OutCondition(outcome as OutCondition);
                                            if (conditionOutcome.PlayerCondition == false) { actorID = option.ActorID; }
                                            else { actorID = 1; }
                                            Actor personCon = Game.world.GetAnyActor(actorID);
                                            if (personCon != null)
                                            {
                                                //does the character already have this condition?
                                                if (personCon.CheckConditionPresent(conditionOutcome.NewCondition.Text) == false)
                                                {
                                                    //not present -> add new condition
                                                    outcomeText = personCon.AddCondition(conditionOutcome.NewCondition);
                                                    if (String.IsNullOrEmpty(outcomeText) == false)
                                                    { resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet("")); }
                                                }
                                                else
                                                {
                                                    //existing identical condition already present. Reset existing condition timer to the max value.
                                                    personCon.ResetConditionTimer(conditionOutcome.NewCondition.Text, conditionOutcome.NewCondition.Timer);
                                                    outcomeText = string.Format("\"{0}\" Condition already acquired by {1}, Timer reset to {2} days", conditionOutcome.NewCondition.Text, personCon.Name,
                                                        conditionOutcome.NewCondition.Timer);
                                                    resultList.Add(new Snippet(outcomeText, foreColor, backColor)); resultList.Add(new Snippet(""));
                                                }
                                            }
                                            break;
                                        case OutcomeType.EventChain:
                                            //chain events -> used by CreateAuto Loc Events
                                            actorID = option.ActorID;
                                            OutEventChain chainOutcome = outcome as OutEventChain;
                                            CreateAutoLocEvent(chainOutcome.Filter, actorID);
                                            Game._eventID = eventObject.EventPID;
                                            break;
                                        case OutcomeType.Conflict:
                                            //seque straight into a Conflict
                                            actorID = option.ActorID;
                                            if (actorID > 0)
                                            {
                                                validOption = 2; //activates conflict in Game.cs SetSpecialModeInput()
                                                OutConflict conflictOutcome = outcome as OutConflict;
                                                Game.conflict.Conflict_Type = conflictOutcome.Conflict_Type;
                                                Game.conflict.Combat_Type = conflictOutcome.Combat_Type;
                                                Game.conflict.Social_Type = conflictOutcome.Social_Type;
                                                Game.conflict.Stealth_Type = conflictOutcome.Stealth_Type;
                                                Game.conflict.SetOpponent(actorID, conflictOutcome.Challenger);
                                                //create a new challenge in dictionary that's used to overide data in standard challenge
                                                if (conflictOutcome.challenge.GetOveride() == true)
                                                {
                                                    //is there an existing special challenge (overide) in the dictionary? delete is so
                                                    if (dictChallenges.ContainsKey(ConflictSubType.Special) == true)
                                                    { dictChallenges.Remove(ConflictSubType.Special); }
                                                    //create new Special challenge & copy data across from OutConflict.challenge
                                                    Challenge challenge = new Challenge(conflictOutcome.challenge);
                                                    //add to dictionary
                                                    try
                                                    { dictChallenges.Add(ConflictSubType.Special, challenge); }
                                                    catch (ArgumentException)
                                                    { Game.SetError(new Error(73, "Invalid challenge key when adding to dictChallenges (duplicate)")); }
                                                }
                                                //message
                                                Actor opponent = Game.world.GetAnyActor(actorID);
                                                if (opponent != null)
                                                {
                                                    tempText = string.Format("A {0} {1} Conflict initiated with {2} {3}, Aid {4}", conflictOutcome.SubType,
                                                        conflictOutcome.Conflict_Type, opponent.Title, opponent.Name, opponent.ActID);
                                                    Message messageConflict = new Message(tempText, MessageType.Conflict);
                                                    Game.world.SetMessage(messageConflict);
                                                    Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                                }
                                                //which state to use?
                                                ConflictState state = ConflictState.None;
                                                switch (conflictOutcome.Conflict_Type)
                                                {
                                                    case ConflictType.Combat:
                                                        switch(conflictOutcome.Combat_Type)
                                                        {
                                                            case ConflictCombat.Battle:
                                                                state = ConflictState.Relative_Army_Size;
                                                                break;
                                                            case ConflictCombat.Personal:
                                                            case ConflictCombat.Tournament:
                                                            case ConflictCombat.Hunting:
                                                                state = ConflictState.Relative_Fame;
                                                                break;
                                                        }
                                                        break;
                                                    case ConflictType.Social:
                                                        switch(conflictOutcome.Social_Type)
                                                        {
                                                            case ConflictSocial.Befriend:
                                                                state = ConflictState.Relative_Honour;
                                                                break;
                                                            case ConflictSocial.Seduce:
                                                            case ConflictSocial.Blackmail:
                                                                state = ConflictState.Relative_Fame;
                                                                break;
                                                        }
                                                        break;
                                                    case ConflictType.Stealth:
                                                        state = ConflictState.Known_Status;
                                                        break;
                                                }
                                                if (state == ConflictState.None)
                                                { Game.SetError(new Error(73, "Invalid state (ConflictState.None) -> changed to Justice")); state = ConflictState.Relative_Justice; } 
                                                Game.conflict.SetGameSituation(state);
                                            }
                                            else
                                            { Game.SetError(new Error(73, string.Format("Invalid actorID for OutConflict (zero or less) \"{0}\", option # {1}", eventObject.Name, optionNum))); }
                                            break;
                                    }
                                }
                            }
                            else { Game.SetError(new Error(73, "Invalid list of Outcomes")); }
                            //display message
                            Position pos = player.GetActorPosition();
                            switch (eventObject.Type)
                            {
                                case ArcType.GeoCluster:
                                case ArcType.Road:
                                    eventList.Add(new Snippet(string.Format("{0}, Aid {1}, at Loc {2}:{3} travelling towards {4}", player.Name, player.ActID, pos.PosX, pos.PosY,
                                        Game.world.GetLocationName(player.LocID)), RLColor.LightGray, backColor));
                                    break;
                                case ArcType.Location:
                                    eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2} (Loc {3}:{4})", player.Name, player.ActID, Game.world.GetLocationName(player.LocID),
                                        pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                                    break;
                                case ArcType.Actor:
                                    if (player.Status == ActorStatus.AtLocation) { status = Game.world.GetLocationName(player.LocID) + " "; }
                                    else { status = null; }
                                    eventList.Add(new Snippet(string.Format("{0}, Aid {1}. at {2}(Loc {3}:{4})", player.Name, player.ActID, status,
                                        pos.PosX, pos.PosY), RLColor.LightGray, backColor));
                                    break;
                            }
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet(string.Format("{0}", eventObject.Name), foreColor, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet(string.Format("\"{0}\"", option.Text), foreColor, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet(string.Format("{0}", optionReply), RLColor.LightBlue, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                            eventList.Add(new Snippet(""));
                            if (resultList.Count > 0)
                            {
                                eventList.AddRange(resultList);
                                eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                                eventList.Add(new Snippet(""));
                            }
                            eventList.Add(new Snippet("Press ENTER or ESC to ignore this event", RLColor.LightGray, backColor));
                            //housekeeping
                            Game.infoChannel.SetInfoList(eventList, ConsoleDisplay.Event);
                        }
                        else
                        {
                            //invalid option (trigger/s didn't pass)
                            validOption = 0;
                            Console.WriteLine("Inactive (greyed out) Option chosen for \"{0}\", option # {1}", eventObject.Name, optionNum);
                        }
                    }
                    else { Game.SetError(new Error(73, string.Format("No valid option present for \"{0}\", option # {1}", eventObject.Name, optionNum))); }
                }
                else { Game.SetError(new Error(73, string.Format("No options present for \"{0}\"", eventObject.Name))); }
            }
            else
            {
                Game.SetError(new Error(73, string.Format("Invalid Event Input \"{0}\"", eventID)));
                validOption = 0;
            }
            return validOption;
        }

        /// <summary>
        /// returns the # of current events for the turn
        /// </summary>
        /// <returns></returns>
        public int GetNumCurrentEvents()
        { return listFollCurrentEvents.Count(); }

        /// <summary>
        /// query whether an event exists based on ID
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool CheckEvent(int eventID)
        {
            if (dictFollowerEvents.ContainsKey(eventID))
            { return true; }
            return false;
        }

        /// <summary>
        /// query whetehr a result exists based on ID
        /// </summary>
        /// <param name="resultID"></param>
        /// <returns></returns>
        public bool CheckResult(int resultID)
        {
            if (dictResults.ContainsKey(resultID))
            { return true; }
            return false;
        }

        /// <summary>
        /// query whether an archetype exists based on ID
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        public bool CheckArchetype(int arcID)
        {
            if (dictArchetypes.ContainsKey(arcID))
            { return true; }
            return false;
        }

        /// <summary>
        /// Returns a story to be used by Director
        /// </summary>
        /// <param name="storyID"></param>
        /// <returns></returns>
        private Story SetStory(int storyID)
        {
            if( dictStories.TryGetValue(storyID, out story))
            { return story; }
            return null;
        }

        /// <summary>
        /// Using Story, set up archetypes for geo / loc / road's (doesn't apply to player)
        /// </summary>
        public void InitialiseArchetypes()
        {
            int refID, arcID;
            //GeoCluster archetypes
            Archetype arcSea = GetArchetype(story.Arc_Geo_Sea);
            Archetype arcMountain = GetArchetype(story.Arc_Geo_Mountain);
            Archetype arcForest = GetArchetype(story.Arc_Geo_Forest);
            //Initialise active GeoClusters (ones with roads through them)
            foreach (int geoID in listOfActiveGeoClusters)
            {
                //get cluster
                GeoCluster cluster = Game.world.GetGeoCluster(geoID);
                if (cluster != null)
                {
                    switch(cluster.Terrain)
                    {
                        case Cluster.Sea:
                            if (arcSea != null)
                            {
                                //% chance of applying to each instance
                                if (rnd.Next(100) < arcSea.Chance)
                                {
                                    //copy Archetype event ID's across to GeoCluster
                                    cluster.SetFollowerEvents(arcSea.GetEvents());
                                    cluster.Archetype = arcSea.ArcID;
                                    //debug
                                    Console.WriteLine("{0}, geoID {1}, has been initialised with \"{2}\", arcID {3}", cluster.Name, cluster.GeoID, arcSea.Name, arcSea.ArcID);
                                }
                            }
                            break;
                        case Cluster.Mountain:
                            if (arcMountain != null)
                            {
                                //% chance of applying to each instance
                                if (rnd.Next(100) < arcMountain.Chance)
                                {
                                    //copy Archetype event ID's across to GeoCluster
                                    cluster.SetFollowerEvents(arcMountain.GetEvents());
                                    cluster.Archetype = arcMountain.ArcID;
                                    //debug
                                    Console.WriteLine("{0}, geoID {1}, has been initialised with \"{2}\", arcID {3}", cluster.Name, cluster.GeoID, arcMountain.Name, arcMountain.ArcID);
                                }
                            }
                            break;
                        case Cluster.Forest:
                            if (arcForest != null)
                            {
                                //% chance of applying to each instance
                                if (rnd.Next(100) < arcForest.Chance)
                                {
                                    //copy Archetype event ID's across to GeoCluster
                                    cluster.SetFollowerEvents(arcForest.GetEvents());
                                    cluster.Archetype = arcForest.ArcID;
                                    //debug
                                    Console.WriteLine("{0}, geoID {1}, has been initialised with \"{2}\", arcID {3}", cluster.Name, cluster.GeoID, arcForest.Name, arcForest.ArcID);
                                }
                            }
                            break;
                    }
                }
            }

            //Road archetypes
            Archetype arcNormal = GetArchetype(story.Arc_Road_Normal);
            Archetype arcKings = GetArchetype(story.Arc_Road_Kings);
            Archetype arcConnector = GetArchetype(story.Arc_Road_Connector);
            //Initialise Roads
            if (arcNormal != null)
            {
                listFollRoadEventsNormal.AddRange(arcNormal.GetEvents());
                Console.WriteLine("Normal roads have been initialised with \"{0}\", arcID {1}", arcNormal.Name, arcNormal.ArcID);
            }
            if (arcKings != null)
            {
                listFollRoadEventsKings.AddRange(arcKings.GetEvents());
                Console.WriteLine("Kings roads have been initialised with \"{0}\", arcID {1}", arcKings.Name, arcKings.ArcID);
            }
            if (arcConnector != null)
            {
                listFollRoadEventsConnector.AddRange(arcConnector.GetEvents());
                Console.WriteLine("Connector roads have been initialised with \"{0}\", arcID {1}", arcConnector.Name, arcConnector.ArcID);
            }

            //Capital archetype
            Archetype arcCapital = GetArchetype(story.Arc_Loc_Capital);
            //Initialise Capital
            if (arcCapital != null)
            {
                listFollCapitalEvents.AddRange(arcCapital.GetEvents());
                Console.WriteLine("The Capital at KingsKeep has been initialised with \"{0}\", arcID {1}", arcCapital.Name, arcCapital.ArcID);
            }

            //Location archetypes
            Archetype arcMajor = GetArchetype(story.Arc_Loc_Major);
            Archetype arcMinor = GetArchetype(story.Arc_Loc_Minor);
            Archetype arcInn = GetArchetype(story.Arc_Loc_Inn);
            //Initialise Locations
            Dictionary<int, Location> tempLocations = Game.network.GetLocations();
            
            foreach(var loc in tempLocations)
            {
                refID = loc.Value.RefID;
                //location present (excludes capital)
                if (refID > 0)
                {
                    if (refID < 100)
                    {
                        //Major House
                        if (arcMajor != null)
                        {
                            //% chance of applying to each instance
                            if (rnd.Next(100) < arcMajor.Chance)
                            {
                                //copy Archetype event ID's across to GeoCluster
                                loc.Value.SetEvents(arcMajor.GetEvents());
                                loc.Value.ArcID = arcMajor.ArcID;
                                //debug
                                Console.WriteLine("{0}, locID {1}, has been initialised with \"{2}\", arcID {3}", Game.world.GetLocationName(loc.Key), loc.Key, arcMajor.Name, arcMajor.ArcID);
                            }
                        }

                    }
                    else if (refID >= 100 && refID < 1000)
                    {
                        //Minor House
                        if (arcMinor != null)
                        {
                            //% chance of applying to each instance
                            if (rnd.Next(100) < arcMinor.Chance)
                            {
                                //copy Archetype event ID's across to GeoCluster
                                loc.Value.SetEvents(arcMinor.GetEvents());
                                loc.Value.ArcID = arcMinor.ArcID;
                                //debug
                                Console.WriteLine("{0}, locID {1}, has been initialised with \"{2}\", arcID {3}", Game.world.GetLocationName(loc.Key), loc.Key, arcMinor.Name, arcMinor.ArcID);
                            }
                        }
                    }
                    else if (refID >= 1000)
                    {
                        //Inn
                        if (arcInn != null)
                        {
                            //% chance of applying to each instance
                            if (rnd.Next(100) < arcInn.Chance)
                            {
                                //copy Archetype event ID's across to GeoCluster
                                loc.Value.SetEvents(arcInn.GetEvents());
                                loc.Value.ArcID = arcInn.ArcID;
                                //debug
                                Console.WriteLine("{0}, locID {1}, has been initialised with \"{2}\", arcID {3}", Game.world.GetLocationName(loc.Key), loc.Key, arcInn.Name, arcInn.ArcID);
                            }
                        }
                    }
                    //House specific archetypes
                    House house = Game.world.GetHouse(refID);
                    arcID = house.ArcID;
                    if (arcID > 0)
                    {
                        Archetype archetype = GetArchetype(arcID);
                        house.SetFollowerEvents(archetype.GetEvents());
                        //debug
                        Console.WriteLine("House {0}, refID {1}, has been initialised with \"{2}\", arcID {3}", house.Name, house.RefID, archetype.Name, archetype.ArcID);
                    }
                }
            }
            //Player & Follower specific archetypes
            Dictionary<int, Active> tempActiveActors = Game.world.GetAllActiveActors();
            if (tempActiveActors != null)
            {
                foreach(var actor in tempActiveActors)
                {
                    arcID = actor.Value.ArcID;
                    if (arcID > 0)
                    {
                        Archetype archetype = GetArchetype(arcID);
                        actor.Value.SetEvents(archetype.GetEvents());
                        //debug
                        Console.WriteLine("\"{0}\", AiD {1}, has been initialised with \"{2}\", arcID {3}", actor.Value.Name, actor.Value.ActID, archetype.Name, archetype.ArcID);
                    }
                }
            }
            else { Game.SetError(new Error(64, "Invalid Dictionary Input (null)")); }
        }

        /// <summary>
        /// Returns an Archetype from dictionary
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        private Archetype GetArchetype(int arcID)
        {
            Archetype arc = new Archetype();
            if (dictArchetypes.TryGetValue(arcID, out arc))
            { return arc; }
            return null;
        }

        /// <summary>
        /// Returns a Result from dictionary
        /// </summary>
        /// <param name="resultID"></param>
        /// <returns></returns>
        internal Result GetResult(int resultID)
        {
            Result result = new Result();
            if (dictResults.TryGetValue(resultID, out result))
            { return result; }
            return null;
        }

        /// <summary>
        /// Returns name of Archetype based on arcID. Null if not found.
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        public string GetArchetypeName(int arcID)
        {
            Archetype arc = new Archetype();
            if (dictArchetypes.TryGetValue(arcID, out arc))
            { return arc.Name; }
            return null;
        }


        /// <summary>
        /// Set data in arrayOfGameStates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="state"></param>
        /// <param name="value"></param>
        /// <param name="setChange">True if you want color highlight on UI of change</param>
        public void SetGameState(DataPoint point, DataState state, int value, bool setChange = false)
        {
            if (point <= DataPoint.Count && state <= DataState.Count)
            {
                arrayOfGameStates[(int)point, (int)state] = value;
                //change - will show color highlight on UI
                if (setChange == true)
                {
                    if (state == DataState.Good) { arrayOfGameStates[(int)point, (int)DataState.Change] = 1; }
                    else if (state == DataState.Bad) { arrayOfGameStates[(int)point, (int)DataState.Change] = -1; }
                }
            }
            else
            { Game.SetError(new Error(75, "Invalid Input (exceeds enum)")); }
        }

        /// <summary>
        /// Get a game state. Returns -999 if not found
        /// </summary>
        /// <param name="point"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetGameState(DataPoint point, DataState state)
        {
            if (point <= DataPoint.Count && state <= DataState.Count)
            { return arrayOfGameStates[(int)point, (int)state]; }
            else
            { Game.SetError(new Error(75, "Invalid Input (exceeds enum)")); }
            return -999;
        }

        /// <summary>
        /// returns a % value for a Game state based on proportion of good vs. bad
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int CheckGameState(DataPoint point)
        {
            int returnValue = 0;
            float good = arrayOfGameStates[(int)point, (int)DataState.Good];
            float bad = arrayOfGameStates[(int)point, (int)DataState.Bad];
            float difference = good - bad;
            if (difference == 0 || good + bad == 0) { returnValue = 50; }
            else
            {
                float percentage = good / (good + bad) * 100;
                percentage = Math.Min(100, percentage);
                percentage = Math.Max(0, percentage);
                returnValue = Convert.ToInt32(percentage);
            }
            return returnValue;
        }

        /// <summary>
        /// returns change state (-ve for an increase in bad, +ve for increase in good, 0 for none)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int CheckGameStateChange(DataPoint point)
        {
            int change = arrayOfGameStates[(int)point, (int)DataState.Change];
            //zero out any change, once queried
            if (change != 0)
            { arrayOfGameStates[(int)point, (int)DataState.Change] = 0; }
            return change;
        }

        /// <summary>
        /// checks event timers each turn
        /// </summary>
        internal void CheckEventTimers()
        {
            //PLAYER events
            foreach (var eventObject in dictPlayerEvents)
            {
                switch (eventObject.Value.Status)
                {
                    case EventStatus.Active:
                        //decrement DORMANT timer, if present
                        if (eventObject.Value.TimerDormant > 0)
                        {
                            eventObject.Value.TimerDormant--;
                            Console.WriteLine("Event \"{0}\" Dormant Timer decremented to {1}", eventObject.Value.Name, eventObject.Value.TimerDormant);
                            //if dormant timer has run down to 0, the event is no longer active
                            if (eventObject.Value.TimerDormant == 0)
                            {
                                eventObject.Value.Status = EventStatus.Dormant;
                                Console.WriteLine("Event \"{0}\" Dormant Timer has run down to Zero. Event is now {1}", eventObject.Value.Name, eventObject.Value.Status);
                            }
                        }
                        //decrement Cool down timers
                        if (eventObject.Value.TimerCoolDown > 0)
                        {
                            eventObject.Value.TimerCoolDown--;
                            Console.WriteLine("\"{0}\" event, Cooldown Timer decremented from {1} to {2}", eventObject.Value.Name, eventObject.Value.TimerCoolDown + 1, 
                                eventObject.Value.TimerCoolDown);
                        }
                        break;
                    case EventStatus.Live:
                        //decrement Live timer, if present
                        if (eventObject.Value.TimerLive > 0)
                        {
                            eventObject.Value.TimerLive--;
                            Console.WriteLine("Event \"{0}\" Live Timer decremented to {1}", eventObject.Value.Name, eventObject.Value.TimerLive);
                            //if Lie timer has run down to 0, the event goes active
                            if (eventObject.Value.TimerLive == 0)
                            {
                                eventObject.Value.Status = EventStatus.Active;
                                Console.WriteLine("Event \"{0}\" Live Timer has run down to Zero. Event is now {1}", eventObject.Value.Name, eventObject.Value.Status);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Changes Player events from one status to another
        /// </summary>
        /// <param name="targetEventID"></param>
        /// <param name="newStatus"></param>
        private string ChangePlayerEventStatus(int targetEventID, EventStatus newStatus)
        {
            string resultText = "";
            //get player event
            EventPlayer eventObject = GetPlayerEvent(targetEventID);
            if (eventObject != null)
            {
                if (newStatus > EventStatus.None)
                {
                    if (eventObject.Status != newStatus)
                    {
                        eventObject.Status = newStatus;
                        resultText = string.Format("\"{0}\", eventPID {1}, has changed status to \"{2}\"", eventObject.Name, eventObject.EventPID, newStatus);
                        Console.WriteLine(resultText);
                    }
                    else
                    { Game.SetError(new Error(78, string.Format("\"{0}\" identical to existing, eventID {1} status unchanged", newStatus, targetEventID))); }
                }
                else
                { Game.SetError(new Error(78, string.Format("Invalid newStatus {0}, eventID {1} status unchanged", newStatus, targetEventID))); }
            }
            else
            { Game.SetError(new Error(78, string.Format("Target Event ID {0} not found, event status unchanged", targetEventID))); }
            return resultText;
        }

        /// <summary>
        /// Change Player event Timer 
        /// </summary>
        /// <param name="outcome"></param>
        private string ChangePlayerEventTimer(OutEventTimer outcome)
        {
            string resultText = "";
            if (outcome != null)
            {
                try
                {
                    EventPlayer eventObject = GetPlayerEvent(outcome.EventID);
                    int oldValue, newValue;
                    switch (outcome.Timer)
                    {
                        case EventTimer.Repeat:
                            oldValue = eventObject.TimerRepeat;
                            newValue = ChangeData(oldValue, outcome.Amount, outcome.Calc);
                            eventObject.TimerRepeat = newValue;
                            resultText = string.Format("\"{0}\", EventPID {1}, {2} timer changed from {3} to {4}", eventObject.Name, eventObject.EventPID, outcome.Timer, oldValue, newValue);
                            Console.WriteLine(resultText);
                            break;
                        case EventTimer.Dormant:
                            oldValue = eventObject.TimerDormant;
                            newValue = ChangeData(oldValue, outcome.Amount, outcome.Calc);
                            eventObject.TimerDormant = newValue;
                            resultText = string.Format("\"{0}\", EventPID {1}, {2} timer changed from {3} to {4}", eventObject.Name, eventObject.EventPID, outcome.Timer, oldValue, newValue);
                            Console.WriteLine(resultText);
                            break;
                        case EventTimer.Live:
                            oldValue = eventObject.TimerLive;
                            newValue = ChangeData(oldValue, outcome.Amount, outcome.Calc);
                            eventObject.TimerLive = newValue;
                            resultText = string.Format("\"{0}\", EventPID {1}, {2} timer changed from {3} to {4}", eventObject.Name, eventObject.EventPID, outcome.Timer, oldValue, newValue);
                            Console.WriteLine(resultText);
                            break;
                        case EventTimer.Cool:
                            oldValue = eventObject.TimerCoolBase;
                            newValue = ChangeData(oldValue, outcome.Amount, outcome.Calc);
                            eventObject.TimerCoolBase = newValue;
                            resultText = string.Format("\"{0}\", EventPID {1}, {2} timer changed from {3} to {4}", eventObject.Name, eventObject.EventPID, outcome.Timer, oldValue, newValue);
                            Console.WriteLine(resultText);
                            break;
                        default:
                            Game.SetError(new Error(79, string.Format("Invalid Timer \"{0}\", EventID \"{1}\"", outcome.Timer, outcome.EventID)));
                            break;
                    }
                }
                catch
                { Game.SetError(new Error(79, string.Format("Invalid EventID \"{0}\" (not found)", outcome.EventID))); }
            }
            else
            { Game.SetError(new Error(79, "Invalid Outcome argument (null)")); }
            return resultText;
        }


        /// <summary>
        /// implements actual changes
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="amount"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public int ChangeData(int currentValue, int amount, EventCalc apply)
        {
            int newValue = currentValue;
            switch (apply)
            {
                case EventCalc.Add:
                    newValue += amount;
                    break;
                case EventCalc.Subtract:
                    newValue -= amount;
                    break;
                case EventCalc.Random:
                    //Adds
                    int rndNum = rnd.Next(amount);
                    newValue += rndNum;
                    break;
                case EventCalc.Equals:
                    newValue = amount;
                    break;
                case EventCalc.NotEqual:
                case EventCalc.LessThanOrEqual:
                case EventCalc.GreaterThanOrEqual:
                    break;
            }
            return newValue;
        }

        /// <summary>
        /// return dictionary of Situations to conflict.GetSituations
        /// </summary>
        /// <returns></returns>
        internal Dictionary<int, Situation> GetSituationsNormal()
        { return dictSituationsNormal; }

        internal Dictionary<int, Situation> GetSituationsGame()
        { return dictSituationsGame; }

        internal Dictionary<int, Situation> GetSituationsSpecial()
        { return dictSituationsSpecial; }

        internal Dictionary<int, Situation> GetSituationsSkill()
        { return dictSituationsSkill; }

        /// <summary>
        /// return a challenge from the dictionary, null if not found
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        internal Challenge GetChallenge(ConflictSubType subType)
        {
            Challenge challenge = null;
            try
            {
                if (dictChallenges.ContainsKey(subType))
                { challenge = dictChallenges[subType]; }
                else
                { Game.SetError(new Error(112, string.Format("{0} Challenge doesn't exist in dictChallenges", subType))); }
            }
            catch (ArgumentNullException)
            { Game.SetError(new Error(112, "Invalid subType (null)")); }
            return challenge;
        }

        /// <summary>
        /// returns true if a specified conflict subType exists in dictionary
        /// </summary>
        /// <returns></returns>
        internal bool CheckChallenge(ConflictSubType type)
        {
            if (dictChallenges.ContainsKey(type) == true)
            { return true; }
            return false;
        }

        /// <summary>
        /// add an autoReact new Player Event to the main dictionary, returns true if successful
        /// </summary>
        /// <param name="eventObject"></param>
        /// <returns></returns>
        public bool AddPlayerEvent(EventPlayer eventObject)
        {
            try
            { dictPlayerEvents.Add(eventObject.EventPID, eventObject); return true; }
            catch (ArgumentNullException)
            { Game.SetError(new Error(125, "Invalid Player Event Object (null)")); }
            catch (ArgumentException)
            { Game.SetError(new Error(125, string.Format("Invalid EventPID (duplicate ID) for \"{0}\"", eventObject.Name))); }
            return false;
        }



        //place Director methods above here
    }
}
