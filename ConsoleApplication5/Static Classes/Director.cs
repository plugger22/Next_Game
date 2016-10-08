using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public EventGeneric EventObject { get; set; }
        public bool Done { get; set; }
    }
    
    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        static Random rnd;
        Story story;
        List<int> listOfActiveGeoClusters; //clusters that have a road through them
        List<int> listGenEventsForest; //generic events for forests
        List<int> listGenEventsMountain;
        List<int> listGenEventsSea;
        List<int> listGenEventsNormal;
        List<int> listGenEventsRoyal;
        List<int> listGenEventsConnector;
        List<int> listGenEventsCapital;
        List<int> listGenEventsMajor;
        List<int> listGenEventsMinor;
        List<int> listGenEventsInn;
        List<EventPackage> listCurrentEvents;
        Dictionary<int, Event> dictEvents;

        public Director(int seed)
        {
            rnd = new Random(seed);

            //debug
            story = new Story("Steady Eddy");
            story.AI = StoryAI.Balanced;
            story.EventLocation = 10;
            story.EventTravelling = 80;

            listOfActiveGeoClusters = new List<int>();
            listGenEventsForest = new List<int>();
            listGenEventsMountain = new List<int>();
            listGenEventsSea = new List<int>();
            listGenEventsNormal = new List<int>(); //note that Normal road generic events also apply to all types of Roads (Royal generics -> Royal + Normal, for example)
            listGenEventsRoyal = new List<int>();
            listGenEventsConnector = new List<int>();
            listGenEventsCapital = new List<int>();
            listGenEventsMajor = new List<int>();
            listGenEventsMinor = new List<int>();
            listGenEventsInn = new List<int>();
            listCurrentEvents = new List<EventPackage>();
            dictEvents = new Dictionary<int, Event>();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
            Console.WriteLine(Environment.NewLine + "--- Import Events");
            dictEvents = Game.file.GetEvents("Events.txt");
            GetGenericEvents();
        }

        /// <summary>
        /// loop all events and place generic eventID's in their approrpriate lists
        /// </summary>
        private void GetGenericEvents()
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
                                case ArcRoad.Royal:
                                    listGenEventsRoyal.Add(eventID);
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
        /// check active characters for random events
        /// </summary>
        public void CheckActivePlayerEvents(Dictionary<int, Active> dictActiveActors)
        {
            //loop all active players
            foreach (var actor in dictActiveActors)
            {
                //not delayed or gone?
                if (actor.Value.Status != ActorStatus.Gone && actor.Value.Delay == 0)
                {
                    if (actor.Value.Status == ActorStatus.AtLocation)
                    {
                        //Location event
                        if (rnd.Next(100) <= story.EventLocation)
                        {
                            /*
                            Console.WriteLine("{0}, Aid {1} at {2}, has experienced a Location event", actor.Value.Name, actor.Value.ActID, Game.world.GetLocationName(actor.Value.LocID));
                            Message message = new Message(string.Format("{0}, Aid {1} at {2}, has experienced a Location event", actor.Value.Name, actor.Value.ActID, 
                                Game.world.GetLocationName(actor.Value.LocID)), MessageType.Event);
                            Game.world.SetMessage(message);
                            */
                            ResolveEvent(actor.Value, EventType.Location);
                        }
                    }
                    else if (actor.Value.Status == ActorStatus.Travelling)
                    {
                        //travelling event
                        if (rnd.Next(100) <= story.EventTravelling)
                        {
                            /*
                            Console.WriteLine("{0}, Aid {1} {2} has experienced a travel event", actor.Value.Name, actor.Value.ActID, Game.world.ShowLocationCoords(actor.Value.LocID));
                            Message message = new Message(string.Format("{0}, Aid {1} {2} has experienced a travel event", actor.Value.Name, actor.Value.ActID, Game.world.ShowLocationCoords(actor.Value.LocID)), MessageType.Event);
                            Game.world.SetMessage(message);
                            */
                            ResolveEvent(actor.Value, EventType.Travelling);
                        }
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
            int geoID, terrain, road, locID, frequency;
            Cartographic.Position pos = actor.GetActorPosition();
            List<Event> listEventPool = new List<Event>();
            //Location event
            if (type == EventType.Location)
            { }
            //Travelling event
            else if (type == EventType.Travelling)
            {
                //Get map data for actor's current location
                geoID = Game.map.GetMapInfo(Cartographic.MapLayer.GeoID, pos.PosX, pos.PosY);
                terrain = Game.map.GetMapInfo(Cartographic.MapLayer.Terrain, pos.PosX, pos.PosY);
                road = Game.map.GetMapInfo(Cartographic.MapLayer.Road, pos.PosX, pos.PosY);
                locID = Game.map.GetMapInfo(Cartographic.MapLayer.LocID, pos.PosX, pos.PosY);
                //get generic terrain & road events
                if (locID == 0 && terrain == 1)
                {
                    //mountains
                    foreach (int eventID in listGenEventsMountain)
                    {
                        Event eventObject = dictEvents[eventID];
                        if (eventObject != null && eventObject.Active == true)
                        {
                            frequency = (int)eventObject.Frequency;
                            //add # of events to pool equal to (int)EventFrequency
                            for (int i = 0; i < frequency; i++)
                            { listEventPool.Add(eventObject); }
                        }
                    }
                }
                else if (locID == 0 && terrain == 2)
                {
                    //forests
                    foreach(int eventID in listGenEventsForest)
                    {
                        Event eventObject = dictEvents[eventID];
                        if (eventObject != null && eventObject.Active == true)
                        {
                            frequency = (int)eventObject.Frequency;
                            //add # of events to pool equal to (int)EventFrequency
                            for(int i = 0; i < frequency; i++)
                            { listEventPool.Add(eventObject); }
                        }
                    }
                }
            }
            //choose an event
            if (listEventPool.Count > 0)
            {
                int rndNum = rnd.Next(0, listEventPool.Count);
                Event eventTemp = listEventPool[rndNum];
                EventGeneric eventChosen = eventTemp as EventGeneric;
                Message message = new Message(string.Format("{0}, Aid {1} {2}, [{3} Event] \"{4}\"", actor.Name, actor.ActID, Game.world.ShowLocationCoords(actor.LocID),
                    type, eventChosen.Name), MessageType.Event);
                Game.world.SetMessage(message);
                //store in list of Current Events
                EventPackage current = new EventPackage() { Person = actor, EventObject = eventChosen, Done = false };
                //current.actor = actor;
                //current.eventObject = eventChosen;
                listCurrentEvents.Add(current);
            }
             
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
                    EventGeneric eventObject = package.EventObject;
                    Active actor = package.Person;
                    //create event description
                    Cartographic.Position pos = actor.GetActorPosition();
                    eventList.Add(new Snippet(string.Format("{0} at Loc {1}:{2}, Aid {3}, travelling towards {4}", actor.Name, pos.PosX, pos.PosY, actor.ActID,
                        Game.world.GetLocationName(actor.LocID)), RLColor.LightGray, backColor));
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
                    if (ability < 3) { traitColor = Color._badTrait; }
                    else if (ability == 3) { traitColor = RLColor.Gray; }
                    else { traitColor = Color._goodTrait; }
                    //enables stars to be centred
                    if (ability != 3)
                    { eventList.Add(new Snippet(string.Format("{0} {1} {2}", Game.world.GetStars(ability), actor.arrayOfTraitNames[(int)eventObject.Trait], effectText), traitColor, backColor)); }
                    else
                    { eventList.Add(new Snippet(string.Format("{0}", Game.world.GetStars(ability)), traitColor, backColor)); }
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
                        eventList.Add(new Snippet(string.Format("{0} has been delayed for {1} {2}", actor.Name, eventObject.Delay, eventObject.Delay == 1 ? "Day" : "Day's"), 
                            RLColor.LightRed, backColor));
                        eventList.Add(new Snippet(""));
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

        //place Director methods above here
    }
}
