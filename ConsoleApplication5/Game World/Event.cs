using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum EventFrequency { Rare, Low, Normal, High, Common }

    /// <summary>
    /// Base Event class
    /// </summary>
    public class Event
    {
        private static int eventIndex = 1;
        public int EventID { get; }
        public string Name { get; set; }
        public int TempID { get; set; }
        public bool Active { get; set; } = true; //can only be used if active
        public EventFrequency Frequency { get; set; }
        public ArcType AppliesTo { get; set; } = ArcType.None;
        public ArcGeo GeoType { get; set; } = ArcGeo.None;
        public ArcLoc LocType { get; set; } = ArcLoc.None;
        public ArcRoad RoadType { get; set; } = ArcRoad.None;
        public ArcCat Category { get; set; } = ArcCat.None;

        public Event()
        { }

        /// <summary>
        /// base class constructor
        /// </summary>
        /// <param name="tempID"></param>
        /// <param name="name"></param>
        /// <param name="frequency"></param>
        public Event(int tempID, string name, EventFrequency frequency)
        {
            EventID = eventIndex++;
            this.TempID = tempID;
            this.Name = name;
            this.Frequency = frequency;
            //debug
            Console.WriteLine("EventID {0], {1} Frequency {2} TempID {3}", EventID, Name, Frequency, TempID);
        }
    }


    /// <summary>
    /// derived base class for Generic Events (followers)
    /// </summary>
    public class EventGeneric : Event
    {
        public string EventText { get; set; }
        public string SuccessText { get; set; } //text to display if follower succeeds test
        public string FailText { get; set; } //text to display if follower fails test
        public TraitType Trait { get; set; } //trait type to test against
        public int Delay { get; set; } //# of days delayed if fail test


        /// <summary>
        /// pass through constructor for subclasses
        /// </summary>
        /// <param name="tempID"></param>
        /// <param name="name"></param>
        /// <param name="frequency"></param>
        public EventGeneric(int tempID, string name, EventFrequency frequency) : base(tempID, name, frequency)
        {
            
        }
    }

    /// <summary>
    /// GeoCluster generic event
    /// </summary>
    public class EventGeo : EventGeneric
    {

        public EventGeo (int tempID, string name, EventFrequency frequency, ArcGeo subtype) : base (tempID, name, frequency)
        {
            AppliesTo = ArcType.GeoCluster;
            GeoType = subtype;
        }
    }

    /// <summary>
    /// Location generic event
    /// </summary>
    public class EventLoc : EventGeneric
    {

        public EventLoc(int tempID, string name, EventFrequency frequency, ArcLoc subtype) : base(tempID, name, frequency)
        {
            AppliesTo = ArcType.Location;
            LocType = subtype;
        }
    }

    /// <summary>
    /// Road generic event
    /// </summary>
    public class EventRoad : EventGeneric
    {

        public EventRoad(int tempID, string name, EventFrequency frequency, ArcRoad subtype) : base(tempID, name, frequency)
        {
            AppliesTo = ArcType.Road;
            RoadType = subtype;
        }
    }

}
