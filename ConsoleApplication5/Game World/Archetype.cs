using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum ArcType {None, GeoCluster, Location, Road}
    public enum ArcGeo {None, Sea, Mountain, Forest } //geocluster sub category
    public enum ArcLoc {None, Capital, Major, Minor, Inn} //location sub category
    public enum ArcRoad {None, Normal, Kings, Connector} //road sub category
    

    public class Archetype
    {
        //private static int arcIndex = 1;
        public string Name { get; set; }
        public int ArcID { get; }
        public int Chance { get; set; } //% chance of archetype applying to whatever it is (roads are 100%, all others are a minimum of one instance)
        //public int TempID { get; set; }
        public ArcType Type { get; set; } //which class of object it applies to
        public ArcGeo Geo { get; set; } //subtypes, default to 'None' if not applicable
        public ArcLoc Loc { get; set; }
        public ArcRoad Road { get; set; }
        private List<int> listOfEvents; //Event ID list that apply to followers

        public Archetype()
        { }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="events"></param>
        public Archetype(string name, int arcID, int chance, List<int> events)
        {
            //ArcID = arcIndex++;
            this.ArcID = arcID;
            this.Name = name;
            this.Chance = chance;
            //this.TempID = tempID;
            if (events != null) { listOfEvents = new List<int>(events); }
            else { Game.SetError(new Error(48, "Invalid list of Events")); }
            //debug
            Console.WriteLine("ArcID {0}, {1}, Chance {2}%, No. Events {3}", ArcID, Name, Chance, events.Count);
        }

        public List<int> GetEvents()
        { return listOfEvents; }
    }

    /// <summary>
    /// Geocluster Archetype
    /// </summary>
    public class ArcTypeGeo : Archetype
    {

        public ArcTypeGeo(string name, ArcGeo subtype, int arcID, int chance, List<int> events) : base(name, arcID, chance, events)
        {
            Type = ArcType.GeoCluster;
            Geo = subtype;
        }
    }

    /// <summary>
    /// Location Archetype
    /// </summary>
    public class ArcTypeLoc : Archetype
    {

        public ArcTypeLoc(string name, ArcLoc subtype, int arcID, int chance, List<int> events) : base(name, arcID, chance, events)
        {
            Type = ArcType.Location;
            Loc = subtype;
        }
    }

    /// <summary>
    /// Road Archetype
    /// </summary>
    public class ArcTypeRoad : Archetype
    {

        public ArcTypeRoad(string name, ArcRoad subtype, int arcID, int chance, List<int> events) : base(name, arcID, chance, events)
        {
            Type = ArcType.Road;
            Road = subtype;
        }
    }

}
