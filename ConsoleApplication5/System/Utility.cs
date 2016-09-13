using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{

    /// <summary>
    /// static class to hold all general utility methods
    /// </summary>
    public class Utility
    {

        /// <summary>
        /// utility function
        /// </summary>
        /// <returns></returns>
        public string ShowDate()
        {
            string dateReturn = "Unknown";
            int moonDay = (Game.gameTurn % 30) + 1;
            int moonCycle = (Game.gameTurn / 30) + 1;
            string moonSuffix = "th";
            if (moonCycle == 1)
            { moonSuffix = "st"; }
            else if (moonCycle == 2)
            { moonSuffix = "nd"; }
            else if (moonCycle == 3)
            { moonSuffix = "rd"; }
            dateReturn = string.Format("Day {0} of the {1}{2} Moon in the Year of our Gods {3}  (Turn {4})", moonDay, moonCycle, moonSuffix, Game.gameYear, Game.gameTurn + 1);
            return dateReturn;
        }
    }
}
