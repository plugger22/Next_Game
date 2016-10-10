using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// AI Personality
    /// </summary>
    class Story
    {
        public string Name { get; set; }
        public int StoryID {get; set; }
        public StoryAI Type { get; set; }
        public int Ev_Follower_Loc { get; set; } // chance of a follower experiencing a random event when at a Location
        public int Ev_Follower_Trav { get; set; } // chance of a follower experiencing a random event when travelling
        public int Ev_Player_Loc { get; set; } // chance of the Player experiencing a random event
        public int Ev_Player_Trav { get; set; }
        //categoryies of archetypes
        public int Arc_Geo_Sea { get; set; }
        public int Arc_Geo_Mountain { get; set; }
        public int Arc_Geo_Forest { get; set; }
        public int Arc_Loc_Capital { get; set; }
        public int Arc_Loc_Major { get; set; }
        public int Arc_Loc_Minor { get; set; }
        public int Arc_Loc_Inn { get; set; }
        public int Arc_Road_Normal { get; set; }
        public int Arc_Road_Kings { get; set; }
        public int Arc_Road_Connector { get; set; }


        public Story(string name, int storyID, StoryAI type)
        {
            this.Name = name;
            this.StoryID = storyID;
            this.Type = Type;
        }
    }
}
