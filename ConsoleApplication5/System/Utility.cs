using System;
using System.Collections.Generic;

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

        /// <summary>
        /// word wrap a long sentence
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxCharacters"></param>
        /// <returns></returns>
        public List<string> WordWrap(string input, int maxCharacters)
        {
            List<string> lines = new List<string>();
            if (!input.Contains(" "))
            {
                int start = 0;
                while (start < input.Length)
                {
                    lines.Add(input.Substring(start, Math.Min(maxCharacters, input.Length - start)));
                    start += maxCharacters;
                }
            }
            else
            {
                string[] words = input.Split(' ');
                string line = "";
                foreach (string word in words)
                {
                    if ((line + word).Length > maxCharacters)
                    {
                        lines.Add(line.Trim());
                        line = "";
                    }
                    line += string.Format("{0} ", word);
                }
                if (line.Length > 0)
                { lines.Add(line.Trim()); }
            }
            return lines;
        }


        //methods above here
    }
}
