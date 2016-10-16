using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum EventFrequency { None, Very_Rare, Rare, Low, Normal, High, Common, Very_Common } //determines how many entries are placed in the event pool -> (int)EventFrequency (1 to 7)
    public enum EventCategory { None, Generic, Special } //specials are used by archetypes, generics apply to all
    public enum EventStatus { Active, Live, Dormant, Dead} //sequential event states from dead to activated

    /// <summary>
    /// Base Event class
    /// NOTE: 'event' is a C# keyword -> use 'eventObject' instead. 'Event' is O.K
    /// </summary>
    public class Event
    {
        //private static int eventIndex = 1;
        //public int EventID { get; }
        public string Name { get; set; }
        public int EventID { get; }
        public int TimerLive { get; set; } = 0; //turns to change from Live -> Active
        public int TimerDormant { get; set; } = 0; //turns to change from Active -> Dormant
        public int TimerRepeat { get; set; } = 0; //# times remaining for the event to repeat (if 0 then reverts to dormant)
        public bool Active { get; set; } = true; //can only be used if active
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
        public Event(int eventID, string name, EventFrequency frequency)
        {
            //EventID = eventIndex++;
            this.EventID = eventID;
            this.Name = name;
            this.Frequency = frequency;
        }
    }


    /// <summary>
    /// derived base class for Generic Events (followers)
    /// </summary>
    public class EventFollower : Event
    {
        public string EventText { get; set;}
        public string SucceedText { get; set; } //text to display if follower succeeds test
        public string FailText { get; set; } //text to display if follower fails test
        public TraitType Trait { get; set; } //trait type to test against
        public int Delay { get; set; } //# of days delayed if fail test


        /// <summary>
        /// pass through constructor for subclasses
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="name"></param>
        /// <param name="frequency"></param>
        public EventFollower(int eventID, string name, EventFrequency frequency) : base(eventID, name, frequency)
        {
            //debug
            Console.WriteLine("EventID {0}, {1}, Frequency {2}", EventID, Name, Frequency);
        }
    }

    /// <summary>
    /// GeoCluster generic event
    /// </summary>
    public class EventGeo : EventFollower
    {

        public EventGeo (int eventID, string name, EventFrequency frequency, ArcGeo subtype) : base (eventID, name, frequency)
        {
            Type = ArcType.GeoCluster;
            GeoType = subtype;
        }
    }

    /// <summary>
    /// Location generic event
    /// </summary>
    public class EventLoc : EventFollower
    {

        public EventLoc(int eventID, string name, EventFrequency frequency, ArcLoc subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Location;
            LocType = subtype;
        }
    }

    /// <summary>
    /// Road generic event
    /// </summary>
    public class EventRoad : EventFollower
    {

        public EventRoad(int eventID, string name, EventFrequency frequency, ArcRoad subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Road;
            RoadType = subtype;
        }
    }

    /// <summary>
    /// House specific event
    /// </summary>
    public class EventHouse : EventFollower
    {

        public EventHouse(int eventID, string name, EventFrequency frequency, ArcHouse subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.House;
            HouseType = subtype;
        }
    }

    /// <summary>
    /// Active actor specific event
    /// </summary>
    public class EventActor : EventFollower
    {

        public EventActor(int eventID, string name, EventFrequency frequency, ArcActor subtype) : base(eventID, name, frequency)
        {
            Type = ArcType.Actor;
            ActorType = subtype;
        }
    }
}
