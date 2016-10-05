using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum StoryAI {None, Benevolent, Balanced, Evil, Tricky}
    
    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        Story story;
        List<int> listOfActiveGeoClusters; //clusters that have a road through them

        public Director()
        {
            listOfActiveGeoClusters = new List<int>();
        }

        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
        }
    }
}
