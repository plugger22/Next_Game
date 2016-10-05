using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{

    public enum StoryGeneral {Good, Neutral, Bad}
    public enum StorySpecific {Tricky}

    /// <summary>
    /// Story AI class that manages the game world
    /// </summary>
    public class Story
    {
        List<int> listOfActiveGeoClusters; //clusters that have a road through them

        public Story()
        {
            listOfActiveGeoClusters = new List<int>();
        }

        public void InitialiseStory()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
        }
    }
}
