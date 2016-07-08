using System;
using System.Collections.Generic;

namespace Next_Game
{
    //history class handles living world procedural generation at game start. Once created, data is passed to World for game use.
    public class History
    {
        private List<Character> playerCharacters;

        public History()
        {
            playerCharacters = new List<Character>();

            //Location data flow: create in Map -> Network to generate routes -> History to generate names and data -> World for current state and future changes
        }

        /// <summary>
        /// create a base list of Player controlled Characters
        /// </summary>
        /// <param name="numCharacters" how many characters do you want?></param>
        public void CreatePlayerCharacters(int numCharacters)
        {
            //rough and ready creation of a handful of basic player characters
            for (int i = 0; i < numCharacters; i++)
            {
                Character person = new Character("Player_" + Convert.ToString(i +1));
                playerCharacters.Add(person);
            }
        }

        /// <summary>
        /// return list of Initial Player Characters
        /// </summary>
        /// <returns>List of Characters</returns>
        internal List<Character> GetPlayerCharacters()
        { return playerCharacters; }
    }
}