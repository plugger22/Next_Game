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
        public int EventLocation { get; set; } // chance of an active character experiencing a random event when at a Location
        public int EventTravelling { get; set; } // chance of an active character experiencing a random event when travelling
        public StoryAI AI { get; set; }

        public Story(string name)
        {
            this.Name = name;
        }
    }
}
