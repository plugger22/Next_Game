using System;
using System.Collections.Generic;
using System.IO;

namespace Next_Game
{
    //history class handles living world procedural generation at game start. Once created, data is passed to World for game use.
    public class History
    {
        private List<Character> playerCharacters;
        private List<string> listOfCharacterNames;
        static Random rnd;

        public History(int seed)
        {
            rnd = new Random(seed);
            playerCharacters = new List<Character>();
            listOfCharacterNames = new List<string>();
            //Location data flow: create in Map -> Network to generate routes -> History to generate names and data -> World for current state and future changes
        }


        public void InitialiseHistory()
        {
            //read in location names
            string filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/Names.txt";
            string[] arrayOfCharacterNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            { listOfCharacterNames.Add(arrayOfCharacterNames[i]); }
        }

        /// <summary>
        /// create a base list of Player controlled Characters
        /// </summary>
        /// <param name="numCharacters" how many characters do you want?></param>
        public void CreatePlayerCharacters(int numCharacters)
        {
            string charName;
            //rough and ready creation of a handful of basic player characters
            for (int i = 0; i < numCharacters; i++)
            {
                int index;
                index = rnd.Next(0, listOfCharacterNames.Count - 1);
                //get name
                charName = listOfCharacterNames[index];
                //delete record in list to prevent duplicate names
                listOfCharacterNames.RemoveAt(index);
                //new character
                Character person = new Character(charName);
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