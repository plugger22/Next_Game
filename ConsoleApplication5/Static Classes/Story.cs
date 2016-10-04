using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// Story AI class that manages the game world
    /// </summary>
    public class Story
    {
        List<int> listOfActiveGeoClusters;

        public Story()
        {
            listOfActiveGeoClusters = new List<int>();
        }
    }
}
