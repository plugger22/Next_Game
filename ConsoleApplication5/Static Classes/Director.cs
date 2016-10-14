using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
using Next_Game.Cartographic;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum StoryAI { None, Benevolent, Balanced, Evil, Tricky }
    public enum EventType { None, Location, Travelling }

    /// <summary>
    /// used to store all triggered events for the current turn
    /// </summary>
    public class EventPackage
    {
        public Active Person { get; set; }
        public EventFollower EventObject { get; set; }
        public bool Done { get; set; }
    }
    
    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        static Random rnd;
        Story story;
        List<int> listOfActiveGeoClusters; //clusters that have a road through them (GeoID's)
        List<int> listGenEventsForest; //generic events for forests
        List<int> listGenEventsMountain;
        List<int> listGenEventsSea;
        List<int> listGenEventsNormal;
        List<int> listGenEventsKing;
        List<int> listGenEventsConnector;
        List<int> listGenEventsCapital;
        List<int> listGenEventsMajor;
        List<int> listGenEventsMinor;
        List<int> listGenEventsInn;
        List<int> listRoadEventsNormal;
        List<int> listRoadEventsKings;
        List<int> listRoadEventsConnector;
        List<int> listCapitalEvents;
        List<Follower> listOfFollowers;
        List<EventPackage> listCurrentEvents;
        private Dictionary<int, Event> dictEvents;
        private Dictionary<int, Archetype> dictArchetypes;
        private Dictionary<int, Story> dictStories;

        public Director(int seed)
        {
            rnd = new Random(seed);

            //debug
            /*story = new Story("Steady Eddy");
            story.Type = StoryAI.Balanced;
            story.Ev_Follower_Loc = 10;
            story.Ev_Follower_Trav = 60;*/

            listOfActiveGeoClusters = new List<int>();
            listGenEventsForest = new List<int>();
            listGenEventsMountain = new List<int>();
            listGenEventsSea = new List<int>();
            listGenEventsNormal = new List<int>(); //note that Normal road generic events also apply to all types of Roads (Royal generics -> Royal + Normal, for example)
            listGenEventsKing = new List<int>();
            listGenEventsConnector = new List<int>();
            listGenEventsCapital = new List<int>();
            listGenEventsMajor = new List<int>();
            listGenEventsMinor = new List<int>();
            listGenEventsInn = new List<int>();
            listRoadEventsNormal = new List<int>();
            listRoadEventsKings = new List<int>();
            listRoadEventsConnector = new List<int>();
            listCapitalEvents = new List<int>();
            listCurrentEvents = new List<EventPackage>();
            listOfFollowers = new List<Follower>();
            dictEvents = new Dictionary<int, Event>();
            dictArchetypes = new Dictionary<int, Archetype>();
            dictStories = new Dictionary<int, Story>();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
            Console.WriteLine(Environment.NewLine + "--- Import Events");
            //Run FIRST
            dictEvents = Game.file.GetEvents("Events.txt");
            InitialiseGenericEvents();
            Console.WriteLine(Environment.NewLine + "--- Import Archetypes");
            //Run AFTER importing Events
            dictArchetypes = Game.file.GetArchetypes("Archetypes.txt");
            Console.WriteLine(Environment.NewLine + "--- Import Stories");
            //Run AFTER importing Archetypes
            dictStories = Game.file.GetStories("Stories.txt");
            story = SetStory(1); //choose which story to use
            Console.WriteLine(Environment.NewLine + "--- Initialise Archetypes");
            InitialiseArchetypes();
            Console.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// loop all events and place Generic eventID's in their approrpriate lists
        /// </summary>
        private void InitialiseGenericEvents()
        {
            int eventID;
            foreach(var eventObject in dictEvents)
            {
                if (eventObject.Value.Category == EventCategory.Generic)
                {
                    eventID = eventObject.Value.EventID;
                    switch(eventObject.Value.Type)
                    {
                        case ArcType.GeoCluster:
                            switch(eventObject.Value.GeoType)
                            {
                                case ArcGeo.Forest:
                                    listGenEventsForest.Add(eventID);
                                    break;
                                case ArcGeo.Mountain:
                                    listGenEventsMountain.Add(eventID);
                                    break;
                                case ArcGeo.Sea:
                                    listGenEventsSea.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcGeo"));
                                    break;
                            }
                            break;
                        case ArcType.Location:
                            switch(eventObject.Value.LocType)
                            {
                                case ArcLoc.Capital:
                                    listGenEventsCapital.Add(eventID);
                                    break;
                                case ArcLoc.Major:
                                    listGenEventsMajor.Add(eventID);
                                    break;
                                case ArcLoc.Minor:
                                    listGenEventsMinor.Add(eventID);
                                    break;
                                case ArcLoc.Inn:
                                    listGenEventsInn.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcLoc"));
                                    break;
                            }
                            break;
                        case ArcType.Road:
                            switch(eventObject.Value.RoadType)
                            {
                                case ArcRoad.Normal:
                                    listGenEventsNormal.Add(eventID);
                                    break;
                                case ArcRoad.Kings:
                                    listGenEventsKing.Add(eventID);
                                    break;
                                case ArcRoad.Connector:
                                    listGenEventsConnector.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcRoad"));
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// empty out list ready for the next turn
        /// </summary>
        public void ClearCurrentEvents()
        { listCurrentEvents.Clear(); }
        
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
                        { ResolveEvent(actor.Value, EventType.Location); }
                    }
                    else if (actor.Value.Status == ActorStatus.Travelling)
                    {
                        //travelling event
                        if (rnd.Next(100) <= story.Ev_Follower_Trav)
                        { ResolveEvent(actor.Value, EventType.Travelling); }
                    }
                }
            }
        }

        /// <summary>
        /// Resolve an event for an active actor
        /// </summary>
        /// <param name="actor"></param>
        private void ResolveEvent(Active actor, EventType type)
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
                    listEventPool.AddRange(GetValidEvents(listGenEventsCapital));
                    listEventPool.AddRange(GetValidEvents(listCapitalEvents));
                }
                else if (refID > 0 && refID < 100)
                {
                    //Major House
                    listEventPool.AddRange(GetValidEvents(listGenEventsMajor));
                    listEventPool.AddRange(GetValidEvents(loc.GetEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidEvents(house.GetEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Major House (refID)")); }
                }
                else if (refID >= 100 && refID < 1000)
                {
                    //Minor House
                    listEventPool.AddRange(GetValidEvents(listGenEventsMinor));
                    listEventPool.AddRange(GetValidEvents(loc.GetEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidEvents(house.GetEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Minor House (refID)")); }
                }
                else if (houseID == 99)
                {
                    //Special Location - Inn
                    listEventPool.AddRange(GetValidEvents(listGenEventsInn));
                    listEventPool.AddRange(GetValidEvents(loc.GetEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidEvents(house.GetEvents())); }
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
                    listEventPool.AddRange(GetValidEvents(listGenEventsMountain));
                    listEventPool.AddRange(GetValidEvents(cluster.GetEvents()));
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    listEventPool.AddRange(GetValidEvents(listGenEventsForest));
                    listEventPool.AddRange(GetValidEvents(cluster.GetEvents()));
                }
                else if (locID == 0 && terrain == 0)
                {
                    //road event
                    if (road == 1)
                    {
                        //normal road
                        listEventPool.AddRange(GetValidEvents(listGenEventsNormal));
                        listEventPool.AddRange(GetValidEvents(listRoadEventsNormal));
                    }
                    else if (road == 2)
                    {
                        //king's road
                        listEventPool.AddRange(GetValidEvents(listGenEventsKing));
                        listEventPool.AddRange(GetValidEvents(listRoadEventsKings));
                    }
                    else if (road == 3)
                    {
                        //connector road
                        listEventPool.AddRange(GetValidEvents(listGenEventsConnector));
                        listEventPool.AddRange(GetValidEvents(listRoadEventsConnector));

                    }
                }
            }
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
                else { Game.SetError(new Error(53, "Invalid Message (null)")); }
                //store in list of Current Events
                EventPackage current = new EventPackage() { Person = actor, EventObject = eventChosen, Done = false };
                listCurrentEvents.Add(current);
            }
        }

        /// <summary>
        /// Extracts all valid events from a list of EventID's
        /// </summary>
        /// <param name="listEventID"></param>
        /// <returns></returns>
        private List<Event> GetValidEvents(List<int> listEventID)
        {
            int frequency;
            List<Event> listEvents = new List<Event>();
            foreach (int eventID in listEventID)
            {
                Event eventObject = dictEvents[eventID];
                if (eventObject != null && eventObject.Active == true)
                {
                    frequency = (int)eventObject.Frequency;
                    //add # of events to pool equal to (int)EventFrequency
                    for (int i = 0; i < frequency; i++)
                    { listEvents.Add(eventObject); }
                }
            }
            return listEvents;
        }
       
        /// <summary>
        /// Process current events one at a time. Returns true if event present to be processed, false otherwise.
        /// </summary>
        public bool ProcessCurrentEvents()
        {
            bool returnValue = false;
            int ability, rndNum, success;
            string effectText;
            List<Snippet> eventList = new List<Snippet>();
            RLColor foreColor = RLColor.Black;
            RLColor backColor = Color._background1;
            RLColor traitColor;
            //loop all triggered events for this turn
            for (int i = 0; i < listCurrentEvents.Count; i++)
            {
                EventPackage package = listCurrentEvents[i];
                if (package.Done == false)
                {
                    EventFollower eventObject = package.EventObject;
                    Active actor = package.Person;
                    //create event description
                    Cartographic.Position pos = actor.GetActorPosition();
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
                    }
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet(eventObject.EventText, foreColor, backColor));
                    eventList.Add(new Snippet(""));
                    eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                    eventList.Add(new Snippet(""));
                    //resolve event and add to description (add delay to actor if needed)
                    eventList.Add(new Snippet(string.Format("A test of {0}", eventObject.Trait), RLColor.Brown, backColor));
                    eventList.Add(new Snippet(""));
                    effectText = actor.GetTraitEffectText(eventObject.Trait);
                    ability = actor.GetTrait(eventObject.Trait);
                    rndNum = rnd.Next(100);
                    success = ability * 20;
                    //trait stars
                    if (ability < 3) { traitColor = RLColor.LightRed; }
                    else if (ability == 3) { traitColor = RLColor.Gray; }
                    else { traitColor = RLColor.Green; }
                    //enables stars to be centred
                    if (ability != 3)
                    { eventList.Add(new Snippet(string.Format("({0} {1})  {2} {3} {4}", ability, ability == 1 ? "Star" : "Stars",
                        Game.world.GetStars(ability), actor.arrayOfTraitNames[(int)eventObject.Trait], 
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
                        eventList.Add(new Snippet(string.Format("{0} {1}", actor.Name, eventObject.SucceedText), RLColor.Black, backColor));
                    }
                    else
                    {
                        //failure
                        eventList.Add(new Snippet(string.Format("Roll {0}", rndNum), RLColor.LightRed, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet("- o -", RLColor.Gray, backColor));
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet(string.Format("{0} {1}", actor.Name, eventObject.FailText), RLColor.Black, backColor));
                        //delay
                        eventList.Add(new Snippet(""));
                        eventList.Add(new Snippet(string.Format("{0} has been {1} for {2} {3}", actor.Name, eventObject.Type == ArcType.Location ? "indisposed" : "delayed",
                            eventObject.Delay, eventObject.Delay == 1 ? "Day" : "Day's"), RLColor.LightRed, backColor));
                        eventList.Add(new Snippet(""));
                        //update actor 
                        actor.Delay = eventObject.Delay;
                        actor.DelayReason = eventObject.Name;
                        //message
                        Message message = new Message(string.Format("{0} has been {1} (\"{2}\") for {3} {4}", actor.Name, eventObject.Type == ArcType.Location ? "indisposed" : "delayed",
                            actor.DelayReason, eventObject.Delay, eventObject.Delay == 1 ? "Day" : "Day's"), MessageType.Move);
                        Game.world.SetMessage(message);
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
        /// returns the # of current events for the turn
        /// </summary>
        /// <returns></returns>
        public int GetNumCurrentEvents()
        { return listCurrentEvents.Count(); }

        /// <summary>
        /// query whether an event exists based on ID
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool CheckEvent(int eventID)
        {
            bool status = false;
            if(dictEvents.ContainsKey(eventID))
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
                                    cluster.SetEvents(arcSea.GetEvents());
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
                                    cluster.SetEvents(arcMountain.GetEvents());
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
                                    cluster.SetEvents(arcForest.GetEvents());
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
                listRoadEventsNormal.AddRange(arcNormal.GetEvents());
                Console.WriteLine("Normal roads have been initialised with \"{0}\", arcID {1}", arcNormal.Name, arcNormal.ArcID);
            }
            if (arcKings != null)
            {
                listRoadEventsKings.AddRange(arcKings.GetEvents());
                Console.WriteLine("Kings roads have been initialised with \"{0}\", arcID {1}", arcKings.Name, arcKings.ArcID);
            }
            if (arcConnector != null)
            {
                listRoadEventsConnector.AddRange(arcConnector.GetEvents());
                Console.WriteLine("Connector roads have been initialised with \"{0}\", arcID {1}", arcConnector.Name, arcConnector.ArcID);
            }

            //Capital archetype
            Archetype arcCapital = GetArchetype(story.Arc_Loc_Capital);
            //Initialise Capital
            if (arcCapital != null)
            {
                listCapitalEvents.AddRange(arcCapital.GetEvents());
                Console.WriteLine("The Capital at KingsKeep has been initialised with \"{0}\", arcID {1}", arcCapital.Name, arcCapital.ArcID);
            }

            //Location archetypes
            Archetype arcMajor = GetArchetype(story.Arc_Loc_Major);
            Archetype arcMinor = GetArchetype(story.Arc_Loc_Minor);
            Archetype arcInn = GetArchetype(story.Arc_Loc_Inn);
            //Initialise Locations
            Dictionary<int, Location> tempLocations = Game.network.GetLocations();
            int refID, arcID;
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
                        house.SetEvents(archetype.GetEvents());
                        //debug
                        Console.WriteLine("House {0}, refID {1}, has been initialised with \"{2}\", arcID {3}", house.Name, house.RefID, archetype.Name, archetype.ArcID);
                    }
                }
            }

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


        //place Director methods above here
    }
}
