using System;
using System.Collections.Generic;
using System.IO;

namespace Next_Game
{
    //holds data read in from house.txt. Used for pool of houses.
    struct HouseStruct
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public int Archetype { get; set; }
        public string Capital { get; set; }

        public void ShowHouseStruct()
        {
            Console.WriteLine();
            Console.WriteLine("Structure House {0} ---", Name);
            Console.WriteLine("Motto: {0}", Motto);
            Console.WriteLine("Banner: {0}", Banner);
            Console.WriteLine("ArchetypeID: {0}", Archetype);
            Console.WriteLine("Capital: {0}", Capital);
        }
    }

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

            //read in house pool
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/Houses.txt";
            string[] arrayOfHouseNames = File.ReadAllLines(filePath);
            Console.WriteLine();
            Console.WriteLine("--- House Names Import");
            bool newHouse = false;
            int dataCounter = 0; //number of houses
            List<HouseStruct> listHousePool = new List<HouseStruct>();
            HouseStruct houseStruct = new HouseStruct();
            for (int i = 0; i < arrayOfHouseNames.Length; i++)
            {
                if (arrayOfHouseNames[i] != "")
                {
                    //set up for a new house
                    if(newHouse == false)
                    {
                        newHouse = true;
                        Console.WriteLine();
                        dataCounter++;
                        //new structure
                        houseStruct = new HouseStruct();
                    }
                    string[] tokens = arrayOfHouseNames[i].Split(':');
                    Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                    switch(tokens[0])
                    {
                        case "House":
                            houseStruct.Name = tokens[1];
                            break;
                        case "Motto":
                            houseStruct.Motto = tokens[1];
                            break;
                        case "Banner":
                            houseStruct.Banner = tokens[1];
                            break;
                        case "ArchetypeID":
                            houseStruct.Archetype = Convert.ToInt32(tokens[1]);
                            break;
                        case "Capital":
                            houseStruct.Capital = tokens[1];
                            break;
                    }
                }
                else
                {
                    newHouse = false;
                    //new house structure has been created?
                    if(dataCounter > 0)
                    {
                        //add to housePool
                        listHousePool.Add(houseStruct);
                        //debug
                        houseStruct.ShowHouseStruct();
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("{0} Houses imported", dataCounter);
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