using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Cartographic;
using Next_Game.Event_System;

namespace Next_Game.Event_System
{
    /// <summary>
    /// Contains all Act Two events
    /// </summary>
    public class ActTwo
    {
        static Random rnd;

        public ActTwo(int seed)
        {
            rnd = new Random(seed);
        }


        /// <summary>
        /// Auto Location Event for Act Two
        /// </summary>
        internal void CreateAutoEventTwo()
        {
            //get player
            Player player = (Player)Game.world.GetPlayer();
            if (player != null)
            {
            }
            else { Game.SetError(new Error(328, "Invalid Player (null)")); }
        }
        //new methods above here
    }
}
