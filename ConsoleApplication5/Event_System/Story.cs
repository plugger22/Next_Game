using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
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
        public int Ev_Player_Loc_Base { get; set; } // chance of the Player experiencing a random event
        public int Ev_Player_Loc_Current { get; set; } //actual turn by turn chance of Player experiencing a random event (can differ from base Ev_Player_Loc figure)
        public int Ev_Player_Trav_Base { get; set; }
        public int Ev_Player_Sea_Base { get; set; }
        public int Ev_Player_Adrift_Base { get; set; }
        public int Ev_Player_Dungeon_Base { get; set; }
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

        public Story()
        { }

        public Story(string name, int storyID, StoryAI type)
        {
            this.Name = name;
            this.StoryID = storyID;
            this.Type = type;
            //debug
            Console.WriteLine("Story \"{0}\", {1}, StoryID {2}", Name, Type, StoryID);
            Ev_Player_Loc_Current = Ev_Player_Loc_Base;
        }
    }
}
