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
            Console.WriteLine("Seat: {0}", Capital);
        }
    }

    //history class handles living world procedural generation at game start. Once created, data is passed to World for game use.
    public class History
    {
        private List<Character> playerCharacters;
        private List<string> listOfCharacterNames;
        private List<House> listOfGreatHouses;
        static Random rnd;

        public History(int seed)
        {
            rnd = new Random(seed);
            playerCharacters = new List<Character>();
            listOfCharacterNames = new List<string>();
            listOfGreatHouses = new List<House>();
            //Location data flow: create in Map -> Network to generate routes -> History to generate names and data -> World for current state and future changes
        }

        /// <summary>
        /// Set up history
        /// </summary>
        /// <param name="numHousesRequired">uniqueHouses from Network.InitialiseHouses</param>
        public void InitialiseHistory(int numHousesRequired)
        {
            //read in location names
            string filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/Names.txt";
            string[] arrayOfCharacterNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            { listOfCharacterNames.Add(arrayOfCharacterNames[i]); }

            //read in house pool
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/GreatHouses.txt";
            string[] arrayOfHouseNames = File.ReadAllLines(filePath);
            Console.WriteLine();
            Console.WriteLine("--- House Names Import");
            bool newHouse = false;
            int dataCounter = 0; //number of houses
            List<HouseStruct> listHousePool = new List<HouseStruct>();
            HouseStruct houseStruct = new HouseStruct();
            string cleanToken;
            string cleanTag;
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
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                    switch(cleanTag)
                    {
                        case "House":
                            houseStruct.Name = cleanToken;
                            break;
                        case "Motto":
                            houseStruct.Motto = cleanToken;
                            break;
                        case "Banner":
                            houseStruct.Banner = cleanToken;
                            break;
                        case "ArchetypeID":
                            houseStruct.Archetype = Convert.ToInt32(cleanToken);
                            break;
                        case "Capital":
                            houseStruct.Capital = cleanToken;
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
            Console.WriteLine("{0} Houses imported, {1} Houses required", dataCounter, numHousesRequired);
            Console.WriteLine();
            //remove surplus houses from pool
            int count = listHousePool.Count;
            int index = 0;
            while(count > numHousesRequired)
            {
                index = rnd.Next(0, count);
                Console.WriteLine("House {0} removed", listHousePool[index].Name);
                listHousePool.RemoveAt(index);
                count = listHousePool.Count;
            }
            Console.WriteLine();
            //loop through structures and initialise House classes
            for(int i = 0; i < listHousePool.Count; i++)
            {
                MajorHouse house = new MajorHouse();
                //copy data from House pool structures
                house.Name = listHousePool[i].Name;
                house.Motto = listHousePool[i].Motto;
                house.Banner = listHousePool[i].Banner;
                house.ArchetypeID = listHousePool[i].Archetype;
                house.LocName = listHousePool[i].Capital;
                //add house to listOfHouses
                listOfGreatHouses.Add(house);
                Console.WriteLine("House {0} added to listOfHouses", house.Name);
            }
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
                index = rnd.Next(0, listOfCharacterNames.Count);
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

        /// <summary>
        ///return list of Houses (one house for each houseID)
        /// </summary>
        /// <returns></returns>
        internal List<House> GetHouses()
        { return listOfGreatHouses; }

        //add methods above
    }
}