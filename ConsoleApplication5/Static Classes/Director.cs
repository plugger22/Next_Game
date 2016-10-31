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
    public enum EventType { None, Location, Travelling }
    
    public enum DataPoint {None, Notoriety, Justice, Legend_Ursurper, Legend_King, Honour_Ursurper, Honour_King, Count } //arrayOfGameStates primary index -> DON"T CHANGE ORDER (mirrored in State.cs)
    public enum DataState { Good, Bad, Change, Count } //arrayOfGameStates secondary index (change indicates item changed since last redraw, +ve # is good, -ve is bad)
    

    /// <summary>
    /// used to store all triggered events for the current turn
    /// </summary>
    public class EventPackage
    {
        public Active Person { get; set; }
        public Event EventObject { get; set; }
        public bool Done { get; set; }
    }

    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        static Random rnd;
        Story story;
        State state;
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
        private Dictionary<int, Archetype> dictArchetypes;
        private Dictionary<int, Story> dictStories;

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
            dictArchetypes = new Dictionary<int, Archetype>();
            dictStories = new Dictionary<int, Story>();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
            //Run FIRST
            Console.WriteLine(Environment.NewLine + "--- Import Follower Events");
            dictFollowerEvents = Game.file.GetFollowerEvents("Events_Follower.txt");
            Console.WriteLine(Environment.NewLine + "--- Import Player Events");
            dictPlayerEvents = Game.file.GetPlayerEvents("Events_Player.txt");
            InitialiseGenericEvents();
            //Run AFTER importing Events
            Console.WriteLine(Environment.NewLine + "--- Import Archetypes");
            dictArchetypes = Game.file.GetArchetypes("Archetypes.txt");
            //Run AFTER importing Archetypes
            Console.WriteLine(Environment.NewLine + "--- Import Stories");
            dictStories = Game.file.GetStories("Stories.txt");
            story = SetStory(1); //choose which story to use
            Console.WriteLine(Environment.NewLine + "--- Initialise Archetypes");
            InitialiseArchetypes();
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
            int popularity = Game.lore.OldKing.GetTrait(TraitType.Charm);
            Game.director.SetGameState(DataPoint.Justice, DataState.Good, popularity * multiplier);
            popularity = Game.lore.NewKing.GetTrait(TraitType.Charm);
            Game.director.SetGameState(DataPoint.Justice, DataState.Bad, popularity * multiplier);
            //Legend_Ursurper -> Combat
            int legend = Game.lore.OldHeir.GetTrait(TraitType.Combat);
            if (legend > 3)
            { Game.director.SetGameState(DataPoint.Legend_Ursurper, DataState.Good, (legend - 3) * multiplier); }
            else if (legend < 3)
            { Game.director.SetGameState(DataPoint.Legend_Ursurper, DataState.Bad, (3 - legend) * multiplier); }
            //Legend_New King -> Combat
            legend = Game.lore.NewKing.GetTrait(TraitType.Combat);
            if (legend > 3)
            { Game.director.SetGameState(DataPoint.Legend_King, DataState.Good, (legend - 3) * multiplier); }
            else if (legend < 3)
            { Game.director.SetGameState(DataPoint.Legend_King, DataState.Bad, (3 - legend) * multiplier); }
            //Honour_Ursurper -> Treachery (good is < 3)
            int treachery = Game.lore.OldHeir.GetTrait(TraitType.Treachery);
            if (treachery > 3)
            { Game.director.SetGameState(DataPoint.Honour_Ursurper, DataState.Bad, (treachery - 3) * multiplier); }
            else if (treachery < 3)
            { Game.director.SetGameState(DataPoint.Honour_Ursurper, DataState.Good, (3 - treachery) * multiplier); }
            //Honour_King -> Treachery (good is < 3)
            treachery = Game.lore.NewKing.GetTrait(TraitType.Treachery);
            if (treachery > 3)
            { Game.director.SetGameState(DataPoint.Honour_King, DataState.Bad, (treachery - 3) * multiplier); }
            else if (treachery < 3)
            { Game.director.SetGameState(DataPoint.Honour_King, DataState.Good, (3 - treachery) * multiplier); }
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
                    eventID = eventObject.Value.EventPID;
                    switch (eventObject.Value.Type)
                    {
                        case ArcType.GeoCluster:
                            switch (eventObject.Value.GeoType)
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
                            switch (eventObject.Value.LocType)
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
                            switch (eventObject.Value.RoadType)
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
            }
        }

        /// <summary>
        /// empty out list ready for the next turn
        /// </summary>
        public void ClearCurrentEvents()
        {
            listFollCurrentEvents.Clear();
            listPlyrCurrentEvents.Clear();
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
                if (player.Status == ActorStatus.AtLocation)
                {
                    //Location event
                    if (rnd.Next(100) <= story.Ev_Player_Loc)
                    { DeterminePlayerEvent(player, EventType.Location); }
                }
                else if (player.Status == ActorStatus.Travelling)
                {
                    //travelling event
                    if (rnd.Next(100) <= story.Ev_Player_Trav)
                    { DeterminePlayerEvent(player, EventType.Travelling); }
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
                Message message = null;
                if (type == EventType.Travelling)
                {
                    message = new Message(string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.ShowLocationCoords(actor.LocID),
                      type, eventChosen.Name), MessageType.Event);
                }
                else if (type == EventType.Location)
                {
                    message = new Message(string.Format("{0}, Aid {1} at {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                      type, eventChosen.Name), MessageType.Event);
                }
                if (message != null)
                { Game.world.SetMessage(message); }
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
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsCapital));
                    listEventPool.AddRange(GetValidPlayerEvents(listPlyrCapitalEvents));
                }
                else if (refID > 0 && refID < 100)
                {
                    //Major House
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMajor));
                    listEventPool.AddRange(GetValidPlayerEvents(loc.GetPlayerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidPlayerEvents(house.GetPlayerEvents())); }
                    else { Game.SetError(new Error(72, "Invalid Major House (refID)")); }
                }
                else if (refID >= 100 && refID < 1000)
                {
                    //Minor House
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMinor));
                    listEventPool.AddRange(GetValidPlayerEvents(loc.GetPlayerEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidPlayerEvents(house.GetPlayerEvents())); }
                    else { Game.SetError(new Error(72, "Invalid Minor House (refID)")); }
                }
                else if (houseID == 99)
                {
                    //Special Location - Inn
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsInn));
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
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsMountain));
                    listEventPool.AddRange(GetValidPlayerEvents(cluster.GetPlayerEvents()));
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    listEventPool.AddRange(GetValidPlayerEvents(listGenPlyrEventsForest));
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
                Message message = null;
                if (type == EventType.Travelling)
                {
                    message = new Message(string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.ShowLocationCoords(actor.LocID),
                      type, eventChosen.Name), MessageType.Event);
                }
                else if (type == EventType.Location)
                {
                    message = new Message(string.Format("{0}, Aid {1} at {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.GetLocationName(actor.LocID),
                      type, eventChosen.Name), MessageType.Event);
                }
                if (message != null)
                { Game.world.SetMessage(message); }
                else { Game.SetError(new Error(72, "Invalid Message (null)")); }
                //store in list of Current Events
                EventPackage current = new EventPackage() { Person = actor, EventObject = eventChosen, Done = false };
                listPlyrCurrentEvents.Add(current);
            }
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
        /// <returns></returns>
        private List<Event> GetValidPlayerEvents(List<int> listEventID)
        {
            int frequency;
            List<Event> listEvents = new List<Event>();
            if (listEventID != null)
            {
                foreach (int eventID in listEventID)
                {
                    Event eventObject = dictPlayerEvents[eventID];
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
                    ability = actor.GetTrait(option.Trait);
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
                    /*
                    //dormant timer (set # of turns till event goes dormant)
                    if (eventObject.TimerDormant > 0)
                    {
                        eventObject.TimerDormant--;
                        Console.WriteLine("Event \"{0}\" Timer Dormant now {1}", eventObject.Name, eventObject.TimerDormant);
                        //if dormant timer has run down to 0, the event is no longer active
                        if (eventObject.TimerDormant == 0)
                        {
                            eventObject.Status = EventStatus.Dormant;
                            Console.WriteLine("Event \"{0}\" Timer Dormant has run down to Zero. Event is now {1}", eventObject.Name, eventObject.Status);
                        }
                    }
                    */
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
                //check each trigger
                foreach(Trigger trigger in listTriggers)
                {
                    switch (trigger.Check)
                    {
                        case TriggerCheck.None:
                            break;
                        case TriggerCheck.Trait:
                            Active player = Game.world.GetActiveActor(1);
                            TraitType type;
                            try
                            { type = (TraitType)trigger.Data; }
                            catch
                            {
                                //set to combat to get the job done but generate an error
                                type = TraitType.Combat;
                                Game.SetError(new Error(76, string.Format("Invalid Trigger Data (\"{0}\"), default Combat trait used instead, for Option \"{1}\"", trigger.Data, option.Text)));
                            }
                            Console.WriteLine("\"{0}\" {1} Trigger, if type {2} is {3} to {4}", option.Text, trigger.Check, trigger.Data, trigger.Calc, trigger.Threshold);
                            if (CheckTrigger(player.GetTrait(type), trigger.Calc, trigger.Threshold) == false)
                                { return false; }
                            break;
                        case TriggerCheck.GameVar:

                            //NOT YET IMPLEMENTED

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
                default:
                    Game.SetError(new Error(77, string.Format("Invalid Trigger Calculation Type (\"{0}\")", comparator)));
                    validCheck = false;
                    break;
            }
            Console.WriteLine("Trigger {0} on \"{1}\"", validCheck == true ? "passed" : "FAILED", data);
            return validCheck;
        }

        /// <summary>
        /// handles outcome resolution for player events, eg. player has chosen option 2 (pressed 'F2').Return 'true' if option Active, false if not.
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="optionNum"></param>
        public bool ResolveOutcome(int eventID, int optionNum)
        {
            bool validOption = true;
            string status;
            List<Snippet> eventList = new List<Snippet>();
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background1;
            //Get eVent
            EventPlayer eventObject = GetPlayerEvent(eventID);
            if (eventObject != null)
            {
                //find option
                List<OptionInteractive> listOptions = eventObject.GetOptions();
                if (listOptions != null)
                {
                    if (listOptions.Count >= optionNum)
                    {
                        OptionInteractive option = listOptions[optionNum - 1];
                        Active actor = Game.world.GetActiveActor(1);
                        //Active option?
                        if (option.Active == true)
                        {
                            //resolve option
                            List<Outcome> listOutcomes = option.GetGoodOutcomes();
                            if (listOutcomes != null)
                            {
                                foreach (Outcome outcome in listOutcomes)
                                {
                                    if (outcome is OutGame)
                                    { state.SetState(eventObject.Name, option.Text, outcome.Type, outcome.Amount, outcome.Calc); }
                                    else if (outcome is OutConflict)
                                    { }
                                    else if (outcome is OutEventTimer)
                                    { }
                                    else if (outcome is OutEventStatus)
                                    { }
                                    //ignore if OutNone (do nothing)
                                }
                            }
                            else { Game.SetError(new Error(73, "Invalid list of Outcomes")); }
                            //display message
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
                            eventList.Add(new Snippet(string.Format("{0}", eventObject.Name), foreColor, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet(string.Format("\"{0}\"", option.Text), foreColor, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet(string.Format("{0}", option.ReplyGood), foreColor, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                            eventList.Add(new Snippet(""));
                            eventList.Add(new Snippet("Press ENTER or ESC to ignore this event", RLColor.LightGray, backColor));
                            //housekeeping
                            Game.infoChannel.SetInfoList(eventList, ConsoleDisplay.Event);
                        }
                        else
                        {
                            //invalid option (trigger/s didn't pass)
                            validOption = false;
                            Console.WriteLine("Inactive (greyed out) Option chosen for \"{0}\", option # {1}", eventObject.Name, optionNum);
                        }
                    }
                    else { Game.SetError(new Error(73, string.Format("No valid option present for \"{0}\", option # {1}", eventObject.Name, optionNum))); }
                }
                else { Game.SetError(new Error(73, string.Format("No options present for \"{0}\"", eventObject.Name))); }
            }
            else { Game.SetError(new Error(73, string.Format("Invalid Event Input \"{0}\"", eventID))); }
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
            bool status = false;
            if(dictFollowerEvents.ContainsKey(eventID))
            { return true; }
            return status;
        }

        /// <summary>
        /// query whether an archetype exists based on ID
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        public bool CheckArchetype(int arcID)
        {
            bool status = false;
            if (dictArchetypes.ContainsKey(arcID))
            { return true; }
            return status;
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
        /// Using Story, set up archetypes for geo / loc / road's
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
        public void SetGameState(DataPoint point, DataState state, int value)
        {
            if (point <= DataPoint.Count && state <= DataState.Count)
            { arrayOfGameStates[(int)point, (int)state] = value; }
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

        //place Director methods above here
    }
}
