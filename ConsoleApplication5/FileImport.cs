using System;
using System.Collections.Generic;
using System.IO;


namespace Next_Game
{
    //holds data read in from house.txt. Used for pool of houses.
    public struct HouseStruct
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public int Archetype { get; set; }
        public int RefID { get; set; }
        public string Capital { get; set; }
    }



    public class FileImport
    {
        string fileDirectory;


        public FileImport(string dir)
        {
            fileDirectory = dir;
        }

        /// <summary>
        /// Standard text file importer, returns an array of data
        /// </summary>
        /// <param name="name">"filename.txt"</param>
        /// <returns></returns>
        private string[] ImportFileData(string fileName)
        {
            string[] importedText = null;
            string path = fileDirectory + fileName;
            if (File.Exists(path))
            { importedText = File.ReadAllLines(path); }
            else
            { Console.WriteLine("ERROR: History.cs, FileImport failed, file name {0}", fileName); }
            return importedText;
        }

        /// <summary>
        /// handles lists of Names
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<string> GetNames(string fileName)
        {
            //
            // read in lists of First Male and Female names ---
            //
            string[] arrayOfCharacterNames = ImportFileData(fileName);
            List<string> listOfNames = new List<string>();
            string tempString = null;
            //read male names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            {
                if (arrayOfCharacterNames[i] != "" && !arrayOfCharacterNames[i].StartsWith("#"))
                {
                    //trim off leading and trailing whitespace
                    tempString = arrayOfCharacterNames[i];
                    tempString = tempString.Trim();
                    listOfNames.Add(tempString);
                }
            }
            return listOfNames;
        }


        public List<HouseStruct> GetHouses(string fileName)
        {
            string[] arrayOfHouseNames = ImportFileData(fileName);
            List<HouseStruct> listHouses = new List<HouseStruct>();
            bool newHouse = false;
            int dataCounter = 0; //number of houses
            HouseStruct houseStruct = new HouseStruct();
            string cleanToken;
            string cleanTag;
            for (int i = 0; i < arrayOfHouseNames.Length; i++)
            {
                if (arrayOfHouseNames[i] != "" && !arrayOfHouseNames[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newHouse == false)
                    {
                        newHouse = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new structure
                        houseStruct = new HouseStruct();
                    }
                    string[] tokens = arrayOfHouseNames[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    //Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                    switch (cleanTag)
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
                        case "ReferenceID":
                            houseStruct.RefID = Convert.ToInt32(cleanToken);
                            break;
                        case "Capital":
                            houseStruct.Capital = cleanToken;
                            //last datapoint - save structure to list
                            if (dataCounter > 0)
                            { listHouses.Add(houseStruct); }
                            break;
                    }
                }
                else
                { newHouse = false; }
            }
            return listHouses;
        }

        //methods above here
    }
}
