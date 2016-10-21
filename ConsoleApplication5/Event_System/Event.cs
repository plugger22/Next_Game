using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum EventFrequency { None, Very_Rare, Rare, Low, Normal, Common, High, Very_High } //determines how many entries are placed in the event pool -> (int)EventFrequency (1 to 7)
    public enum EventCategory { None, Generic, Special } //specials are used by archetypes, generics apply to all
    public enum EventStatus { None, Active, Live, Dormant, Dead} //sequential event states from dead to activated

    /// <summary>
    /// Base Event class
    /// NOTE: 'event' is a C# keyword -> use 'eventObject' instead. 'Event' is O.K
    /// </summary>
    public class Event
    {
        public string Name { get; set; }
        public int TimerLive { get; set; } = 0; //turns to change from Live -> Active
        public int TimerDormant { get; set; } = 0; //turns to change from Active -> Dormant
        public int TimerRepeat { get; set; } = 1000; //# times remaining for the event to repeat (if 0 then reverts to dormant)
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

        public EventPlayer(int eventID, string name, EventFrequency frequency) : base( name, frequency)
        {
            EventPID = eventID;
            listOfOptions = new List<OptionInteractive>();
            //debug
            Console.WriteLine("EventID {0}, \"{1}\", {2} frequency", EventPID, Name, Frequency);
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
    }

    /// <summary>
    /// GeoCluster Player generic event
    /// </summary>
    public class EventPlyGeo : EventPlayer
    {

        public EventPlyGeo(int eventID, string name, EventFrequency frequency, ArcGeo subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.GeoCluster;
            GeoType = subtype;
        }
    }

    /// <summary>
    /// Location Player generic event
    /// </summary>
    public class EventPlyLoc : EventPlayer
    {

        public EventPlyLoc(int eventID, string name, EventFrequency frequency, ArcLoc subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Location;
            LocType = subtype;
        }
    }

    /// <summary>
    /// Road Player generic event
    /// </summary>
    public class EventPlyRoad : EventPlayer
    {

        public EventPlyRoad(int eventID, string name, EventFrequency frequency, ArcRoad subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Road;
            RoadType = subtype;
        }
    }

    /// <summary>
    /// House Player specific event
    /// </summary>
    public class EventPlyHouse : EventPlayer
    {

        public EventPlyHouse(int eventID, string name, EventFrequency frequency, ArcHouse subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.House;
            HouseType = subtype;
        }
    }

    /// <summary>
    ///  Player specific event
    /// </summary>
    public class EventPlyActor : EventPlayer
    {

        public EventPlyActor(int eventID, string name, EventFrequency frequency, ArcActor subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Actor;
            ActorType = subtype;
        }
    }



}
