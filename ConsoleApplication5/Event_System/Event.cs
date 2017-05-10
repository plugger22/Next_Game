using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum EventFrequency { None, Very_Rare, Rare, Low, Normal, Common, High, Very_High } //determines how many entries are placed in the event pool -> (int)EventFrequency (1 to 7)
    public enum EventCategory { None, Generic, Archetype, AutoCreate, AutoReact } //archetypes only for arc's, generics apply to all, AutoCreate (eg. do what?), AutoReact (eg. 'Angry Lord')
    public enum EventStatus { None, Active, Live, Dormant, Dead} //sequential event states from dead to activated
    public enum EventCalc { None, Add, Subtract, Random, Equals, NotEqual, GreaterThanOrEqual, LessThanOrEqual, RandomPlus, RandomMinus, Count} //used within events for triggers (>, <, =, !=) and outcomes (+, -, Rnd)
    public enum EventTimer { None, Repeat, Dormant, Live, Cool } //used for EventTimer outcomes to specify a timer -> Cool is Cooldown timer
    public enum EventType { None, Location, Travelling, Sea, Dungeon, Adrift }
    public enum EventAutoFilter { None, Court, Visitors, Followers, Advisors, Interact, Docks, FindShip, BribeCaptain, Recruit, TheyWant, YouWant, SafeHouse} //used by autoloc plyr events to determine which group of actors to show as options

    /// <summary>
    /// Base Event class
    /// NOTE: 'event' is a C# keyword -> use 'eventObject' instead. 'Event' is O.K
    /// </summary>
    public class Event
    {
        public string Name { get; set; } //short descriptor (used in messages and records)
        public int TimerLive { get; set; } = 0; //turns to change from Live -> Active
        public int TimerDormant { get; set; } = 0; //turns to change from Active -> Dormant (lasts a fixed number of turns, not activations as with TimerRepeat)
        public int TimerRepeat { get; set; } = 1000; //# times remaining for the event to repeat (if 0 then reverts to dormant)
        public int TimerCoolBase { get; set; } = 0; //# turns of cool down before event can be ready to fire again (base value, needed to reset TimerCoolDown)
        public int TimerCoolDown { get; set; } = 0; //# turns of cool down -> event can only fire if '0'. Reset to TimerCoolBase whenever event fires. 
        public int SubRef { get; set; } //multipurpose ID to limit events to particular houseID's, geoID's etc. OPTIONAL
        public string Text { get; set; } //main text for event
        public EventFrequency Frequency { get; set; }
        public EventCategory Category { get; set; } = EventCategory.None;
        public EventStatus Status { get; set; } = EventStatus.Active;
        public ArcType Type { get; set; } = ArcType.None;
        public ArcGeo GeoType { get; set; } = ArcGeo.None;
        public ArcLoc LocType { get; set; } = ArcLoc.None;
        public ArcRoad RoadType { get; set; } = ArcRoad.None;
        public ArcHouse HouseType { get; set; } = ArcHouse.None;
        public ArcActor ActorType { get; set; } = ArcActor.None;
        
        

        public Event()
        { }

        /// <summary>
        /// base class constructor
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="name"></param>
        /// <param name="frequency"></param>
        public Event(string name, EventFrequency frequency)
        {
            this.Name = name;
            this.Frequency = frequency;
            
        }
    }


    /// <summary>
    /// derived base class for Generic Events (followers) -----------------
    /// </summary>
    public class EventFollower : Event
    {
        public int EventFID {get; set;} //unique number to follower events
        private List<OptionAuto> listOfOptions;


        /// <summary>
        /// pass through constructor for subclasses
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="name"></param>
        /// <param name="frequency"></param>
        public EventFollower(int eventID, string name, EventFrequency frequency) : base(name, frequency)
        {
            EventFID = eventID;
            listOfOptions = new List<OptionAuto>();
            //debug
            Console.WriteLine("EventID {0}, {1}, Frequency {2}", EventFID, Name, Frequency);
        }

        /// <summary>
        /// add an option
        /// </summary>
        /// <param name="option"></param>
        public void SetOption(OptionAuto option)
        {
            if (option != null)
            { listOfOptions.Add(option); }
            else
            { Game.SetError(new Error(69, "Invalid Option Input (null)")); }
        }

        public List<OptionAuto> GetOptions()
        { return listOfOptions; }
    }

    /// <summary>
    /// GeoCluster Follower generic event
    /// </summary>
    public class EventFolGeo : EventFollower
    {

        public EventFolGeo (int eventID, string name, EventFrequency frequency, ArcGeo subtype) : base (eventID, name, frequency)
        {
            Type = ArcType.GeoCluster;
            GeoType = subtype;
        }
    }

    /// <summary>
    /// Location Follower generic event
    /// </summary>
    public class EventFolLoc : EventFollower
    {

        public EventFolLoc(int eventID, string name, EventFrequency frequency, ArcLoc subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Location;
            LocType = subtype;
        }
    }

    /// <summary>
    /// Road Follower generic event
    /// </summary>
    public class EventFolRoad : EventFollower
    {

        public EventFolRoad(int eventID, string name, EventFrequency frequency, ArcRoad subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Road;
            RoadType = subtype;
        }
    }

    /// <summary>
    /// House Follower specific event
    /// </summary>
    public class EventFolHouse : EventFollower
    {

        public EventFolHouse(int eventID, string name, EventFrequency frequency, ArcHouse subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.House;
            HouseType = subtype;
        }
    }

    /// <summary>
    ///  Follower actor specific event
    /// </summary>
    public class EventFolActor : EventFollower
    {

        public EventFolActor(int eventID, string name, EventFrequency frequency, ArcActor subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Actor;
            ActorType = subtype;
        }
    }





    /// <summary>
    /// Player events --------------------------------
    /// </summary>
    public class EventPlayer : Event
    {
        public int EventPID { get; set; }
        private List<OptionInteractive> listOfOptions;

        public EventPlayer(int eventID, string name, EventFrequency frequency) : base(name, frequency)
        {
            EventPID = eventID;
            listOfOptions = new List<OptionInteractive>();
            //debug
            Console.WriteLine("EventID {0}, \"{1}\", Frequency {2}", EventPID, Name, Frequency);
        }

        /// <summary>
        /// Copy constructor, used for creating new events from auto event archetypes
        /// </summary>
        /// <param name="other"></param>
        public EventPlayer(EventPlayer other)
        {
            EventPID = Game.director.EventAutoID++;
            Name = other.Name;
            TimerLive = other.TimerLive;
            TimerDormant = other.TimerDormant;
            TimerRepeat = other.TimerRepeat;
            TimerCoolBase = other.TimerCoolBase;
            TimerCoolDown = other.TimerCoolDown;
            Text = other.Text;
            Frequency = other.Frequency;
            Category = other.Category;
            Status = other.Status;
            Type = other.Type;
            SubRef = other.SubRef;
            GeoType = other.GeoType;
            LocType = other.LocType;
            RoadType = other.RoadType;
            HouseType = other.HouseType;
            ActorType = other.ActorType;
            listOfOptions = new List<OptionInteractive>(other.listOfOptions);
        }

        /// <summary>
        /// add an option
        /// </summary>
        /// <param name="option"></param>
        public void SetOption(OptionInteractive option)
        {
            if (option != null)
            { listOfOptions.Add(option); }
            else
            { Game.SetError(new Error(69, "Invalid Option Input (null)")); }
        }

        public List<OptionInteractive> GetOptions()
        { return listOfOptions; }

        public void ClearOptions()
        { listOfOptions.Clear(); }
    }


    /// <summary>
    /// GeoCluster Player generic event
    /// </summary>
    public class EventPlyrGeo : EventPlayer
    {

        public EventPlyrGeo(int eventID, string name, EventFrequency frequency, ArcGeo subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.GeoCluster;
            GeoType = subtype;
        }
    }

    /// <summary>
    /// Location Player generic event
    /// </summary>
    public class EventPlyrLoc : EventPlayer
    {

        public EventPlyrLoc(int eventID, string name, EventFrequency frequency, ArcLoc subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Location;
            LocType = subtype;
        }
    }

    /// <summary>
    /// Road Player generic event
    /// </summary>
    public class EventPlyrRoad : EventPlayer
    {

        public EventPlyrRoad(int eventID, string name, EventFrequency frequency, ArcRoad subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Road;
            RoadType = subtype;
        }
    }

    /// <summary>
    /// House Player specific event
    /// </summary>
    public class EventPlyrHouse : EventPlayer
    {

        public EventPlyrHouse(int eventID, string name, EventFrequency frequency, ArcHouse subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.House;
            HouseType = subtype;
        }
    }

    /// <summary>
    ///  Player specific event
    /// </summary>
    public class EventPlyrActor : EventPlayer
    {

        public EventPlyrActor(int eventID, string name, EventFrequency frequency, ArcActor subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Actor;
            ActorType = subtype;
        }
    }

    /// <summary>
    /// Dungeon event for when Player is Captured
    /// </summary>
    public class EventPlyrDungeon : EventPlayer
    {

        public EventPlyrDungeon(int eventID, string name, EventFrequency frequency) : base(eventID, name, frequency)
        {
            Type = ArcType.Dungeon;
        }
    }

    /// <summary>
    /// Adrift event for when Player is cast adrift in the ocean
    /// </summary>
    public class EventPlyrAdrift : EventPlayer
    {
        public EventPlyrAdrift(int eventID, string name, EventFrequency frequency) : base(eventID, name, frequency)
        {
            Type = ArcType.Adrift;
        }
    }




}
