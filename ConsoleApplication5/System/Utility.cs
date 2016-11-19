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

        /// <summary>
        /// Gives 2D distance between two coordinates
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int GetDistance(int x1, int y1, int x2, int y2)
        {
            double dX = x1 - x2;
            double dY = y1 - y2;
            double multi = dX * dX + dY * dY;
            double distance = Math.Sqrt(multi);
            return (int)distance;
        }

        /// <summary>
        /// Checks a string for actor related text tags and swaps them over for correct texts 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public string CheckTagsActor(string text, Actor opponent)
        {
            string checkedText = text;
            if (String.IsNullOrEmpty(text) == false)
            {

                string tag, replaceText;
                int tagStart, tagFinish, length; //indexes

                //loop whilever tags are present
                while (checkedText.Contains("<") == true)
                {
                    tagStart = checkedText.IndexOf("<");
                    tagFinish = checkedText.IndexOf(">");
                    length = tagFinish - tagStart;
                    tag = checkedText.Substring(tagStart + 1, length - 1);
                    //strip brackets
                    replaceText = null;
                    switch(tag)
                    {
                        case "men":
                            replaceText = string.Format("{0} {1}'s Men-At-Arms", opponent.Office, opponent.Name);
                            break;
                        case "name":
                            replaceText = string.Format("{0} {1}", opponent.Office, opponent.Name);
                            break;
                        case "him":
                            replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "him" : "her");
                            break;
                        case "he":
                            replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "he" : "she");
                            break;
                        case "He":
                            replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "He" : "She");
                            break;
                    }
                    if (replaceText != null)
                    {
                        //swap tag for text
                        checkedText = checkedText.Remove(tagStart, length + 1);
                        checkedText = checkedText.Insert(tagStart, replaceText);
                    }
                }
            }
            else
            { Game.SetError(new Error(101, "Invalid Input (null or empty)")); }
               
            return checkedText;
        }

        //methods above here
    }
}
