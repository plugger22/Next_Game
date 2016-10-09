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
    

    class Archetype
    {
        private static int arcIndex = 1;
        public string Name { get; set; }
        public int ArcID { get; }
        public int TempID { get; }
        public ArcType Type { get; set; } //which class of object it applies to
        private List<int> listFollowerEvents; //Event ID list that apply to followers

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="events"></param>
        public Archetype(string name, ArcType type, int tempID, List<int> events)
        {
            ArcID = arcIndex++;
            this.Name = name;
            Type = type;
            this.TempID = tempID;
            if (events != null) { listFollowerEvents = new List<int>(events); }
            else { Game.SetError(new Error(48, "Invalid list of Events")); }
        }
    }
}
