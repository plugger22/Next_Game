﻿using RLNET;
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
        List<int> listGenFolEventsForest; //generic events for followers
        List<int> listGenFolEventsMountain;
        List<int> listGenFolEventsSea;
        List<int> listGenFolEventsNormal;
        List<int> listGenFolEventsKing;
        List<int> listGenFolEventsConnector;
        List<int> listGenFolEventsCapital;
        List<int> listGenFolEventsMajor;
        List<int> listGenFolEventsMinor;
        List<int> listGenFolEventsInn;
        //archetype follower events
        List<int> listFolRoadEventsNormal; 
        List<int> listFolRoadEventsKings;
        List<int> listFolRoadEventsConnector;
        List<int> listFolCapitalEvents;
        //Player events
        List<int> listPlyEventsForest;
        List<int> listPlyEventsMountain;
        List<int> listPlyEventsSea;
        List<int> listPlyEventsNormal;
        List<int> listPlyEventsKing;
        List<int> listPlyEventsConnector;
        List<int> listPlyEventsCapital;
        List<int> listPlyEventsMajor;
        List<int> listPlyEventsMinor;
        List<int> listPlyEventsInn;
        //other
        List<Follower> listOfFollowers;
        List<EventPackage> listCurrentEvents;
        private Dictionary<int, EventFollower> dictFollowerEvents;
        private Dictionary<int, EventPlayer> dictPlayerEvents;
        private Dictionary<int, Archetype> dictArchetypes;
        private Dictionary<int, Story> dictStories;

        public Director(int seed)
        {
            rnd = new Random(seed);
            //follower generic events
            listOfActiveGeoClusters = new List<int>();
            listGenFolEventsForest = new List<int>();
            listGenFolEventsMountain = new List<int>();
            listGenFolEventsSea = new List<int>();
            listGenFolEventsNormal = new List<int>(); //note that Normal road generic events also apply to all types of Roads (Royal generics -> Royal + Normal, for example)
            listGenFolEventsKing = new List<int>();
            listGenFolEventsConnector = new List<int>();
            listGenFolEventsCapital = new List<int>();
            listGenFolEventsMajor = new List<int>();
            listGenFolEventsMinor = new List<int>();
            listGenFolEventsInn = new List<int>();
            //Player events
            listPlyEventsForest = new List<int>();
            listPlyEventsMountain = new List<int>();
            listPlyEventsSea = new List<int>();
            listPlyEventsNormal = new List<int>();
            listPlyEventsKing = new List<int>();
            listPlyEventsConnector = new List<int>();
            listPlyEventsCapital = new List<int>();
            listPlyEventsMajor = new List<int>();
            listPlyEventsMinor = new List<int>();
            listPlyEventsInn = new List<int>();
            //archetype follower events
            listFolRoadEventsNormal = new List<int>();
            listFolRoadEventsKings = new List<int>();
            listFolRoadEventsConnector = new List<int>();
            listFolCapitalEvents = new List<int>();
            //other
            listCurrentEvents = new List<EventPackage>();
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
        }

        /// <summary>
        /// loop all events and place Generic eventID's in their approrpriate lists
        /// </summary>
        private void InitialiseGenericEvents()
        {
            int eventID;
            //Follower events
            foreach(var eventObject in dictFollowerEvents)
            {
                if (eventObject.Value.Category == EventCategory.Generic)
                {
                    eventID = eventObject.Value.EventFID;
                    switch(eventObject.Value.Type)
                    {
                        case ArcType.GeoCluster:
                            switch(eventObject.Value.GeoType)
                            {
                                case ArcGeo.Forest:
                                    listGenFolEventsForest.Add(eventID);
                                    break;
                                case ArcGeo.Mountain:
                                    listGenFolEventsMountain.Add(eventID);
                                    break;
                                case ArcGeo.Sea:
                                    listGenFolEventsSea.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, string.Format("Invalid Type, ArcGeo, Follower Event, ID {0}", eventID)));
                                    break;
                            }
                            break;
                        case ArcType.Location:
                            switch(eventObject.Value.LocType)
                            {
                                case ArcLoc.Capital:
                                    listGenFolEventsCapital.Add(eventID);
                                    break;
                                case ArcLoc.Major:
                                    listGenFolEventsMajor.Add(eventID);
                                    break;
                                case ArcLoc.Minor:
                                    listGenFolEventsMinor.Add(eventID);
                                    break;
                                case ArcLoc.Inn:
                                    listGenFolEventsInn.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, string.Format("Invalid Type, ArcLoc, Follower Event, ID {0}", eventID)));
                                    break;
                            }
                            break;
                        case ArcType.Road:
                            switch(eventObject.Value.RoadType)
                            {
                                case ArcRoad.Normal:
                                    listGenFolEventsNormal.Add(eventID);
                                    break;
                                case ArcRoad.Kings:
                                    listGenFolEventsKing.Add(eventID);
                                    break;
                                case ArcRoad.Connector:
                                    listGenFolEventsConnector.Add(eventID);
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
                                    listPlyEventsForest.Add(eventID);
                                    break;
                                case ArcGeo.Mountain:
                                    listPlyEventsMountain.Add(eventID);
                                    break;
                                case ArcGeo.Sea:
                                    listPlyEventsSea.Add(eventID);
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
                                    listPlyEventsCapital.Add(eventID);
                                    break;
                                case ArcLoc.Major:
                                    listPlyEventsMajor.Add(eventID);
                                    break;
                                case ArcLoc.Minor:
                                    listPlyEventsMinor.Add(eventID);
                                    break;
                                case ArcLoc.Inn:
                                    listPlyEventsInn.Add(eventID);
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
                                    listPlyEventsNormal.Add(eventID);
                                    break;
                                case ArcRoad.Kings:
                                    listPlyEventsKing.Add(eventID);
                                    break;
                                case ArcRoad.Connector:
                                    listPlyEventsConnector.Add(eventID);
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
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsCapital));
                    listEventPool.AddRange(GetValidEvents(listFolCapitalEvents));
                }
                else if (refID > 0 && refID < 100)
                {
                    //Major House
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsMajor));
                    listEventPool.AddRange(GetValidEvents(loc.GetEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidEvents(house.GetEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Major House (refID)")); }
                }
                else if (refID >= 100 && refID < 1000)
                {
                    //Minor House
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsMinor));
                    listEventPool.AddRange(GetValidEvents(loc.GetEvents()));
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    { listEventPool.AddRange(GetValidEvents(house.GetEvents())); }
                    else { Game.SetError(new Error(52, "Invalid Minor House (refID)")); }
                }
                else if (houseID == 99)
                {
                    //Special Location - Inn
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsInn));
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
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsMountain));
                    listEventPool.AddRange(GetValidEvents(cluster.GetEvents()));
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    listEventPool.AddRange(GetValidEvents(listGenFolEventsForest));
                    listEventPool.AddRange(GetValidEvents(cluster.GetEvents()));
                }
                else if (locID == 0 && terrain == 0)
                {
                    //road event
                    if (road == 1)
                    {
                        //normal road
                        listEventPool.AddRange(GetValidEvents(listGenFolEventsNormal));
                        listEventPool.AddRange(GetValidEvents(listFolRoadEventsNormal));
                    }
                    else if (road == 2)
                    {
                        //king's road
                        listEventPool.AddRange(GetValidEvents(listGenFolEventsKing));
                        listEventPool.AddRange(GetValidEvents(listFolRoadEventsKings));
                    }
                    else if (road == 3)
                    {
                        //connector road
                        listEventPool.AddRange(GetValidEvents(listGenFolEventsConnector));
                        listEventPool.AddRange(GetValidEvents(listFolRoadEventsConnector));
                    }
                }
            }
            //character specific events
            if (actor.GetNumEvents() > 0)
            { listEventPool.AddRange(GetValidEvents(actor.GetEvents())); }
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
                Event eventObject = dictFollowerEvents[eventID];
                if (eventObject != null && eventObject.Status == EventStatus.Active)
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
        /// Resolve current events one at a time. Returns true if event present to be processed, false otherwise.
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
            for (int i = 0; i < listCurrentEvents.Count; i++)
            {
                EventPackage package = listCurrentEvents[i];
                if (package.Done == false)
                {
                    EventFollower eventObject = package.EventObject;
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
                            for(int k = 0; k < listBadOutcomes.Count; k++)
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
                listFolRoadEventsNormal.AddRange(arcNormal.GetEvents());
                Console.WriteLine("Normal roads have been initialised with \"{0}\", arcID {1}", arcNormal.Name, arcNormal.ArcID);
            }
            if (arcKings != null)
            {
                listFolRoadEventsKings.AddRange(arcKings.GetEvents());
                Console.WriteLine("Kings roads have been initialised with \"{0}\", arcID {1}", arcKings.Name, arcKings.ArcID);
            }
            if (arcConnector != null)
            {
                listFolRoadEventsConnector.AddRange(arcConnector.GetEvents());
                Console.WriteLine("Connector roads have been initialised with \"{0}\", arcID {1}", arcConnector.Name, arcConnector.ArcID);
            }

            //Capital archetype
            Archetype arcCapital = GetArchetype(story.Arc_Loc_Capital);
            //Initialise Capital
            if (arcCapital != null)
            {
                listFolCapitalEvents.AddRange(arcCapital.GetEvents());
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
                        house.SetEvents(archetype.GetEvents());
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


        //place Director methods above here
    }
}
