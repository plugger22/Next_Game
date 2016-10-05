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
    public enum ArcRoad {None, Normal, Royal, Connector} //road sub category

    class Archetype
    {
        private static int arcIndex = 1;
        public string Name { get; set; }
        public int arcID { get; }
        public ArcType AppliesTo { get; set; } //which class of object it applies to
        private List<int> listOfEvents; //Event ID list

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="events"></param>
        public Archetype(string name, ArcType type, List<int> events)
        {
            arcID = arcIndex++;
            this.Name = name;
            AppliesTo = type;
            if (events != null) { listOfEvents = new List<int>(events); }
            else { Game.SetError(new Error(48, "Invalid list of Events")); }
        }
    }
}
