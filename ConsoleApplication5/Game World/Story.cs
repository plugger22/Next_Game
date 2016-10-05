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
        public StoryAI AI { get; set; }

        public Story(string name)
        {
            this.Name = name;
        }
    }
}
