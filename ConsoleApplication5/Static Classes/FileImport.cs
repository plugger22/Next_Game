using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
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

    //holds data for Traits (best to use stack, not heap for this)
    struct TraitStruct
    {
        public TraitType Type { get; set; }
        public TraitSex Sex { get; set; }
        public TraitAge Age { get; set; }
        public string Name { get; set; }
        public int Effect { get; set; }
        public int Chance { get; set; }
    }

    /// <summary>
    /// Handles all file Import duties
    /// </summary>
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
        private string[] ImportDataFile(string fileName)
        {
            string[] importedText = null;
            string path = fileDirectory + fileName;
            if (File.Exists(path))
            { importedText = File.ReadAllLines(path); }
            else
            { Game.SetError(new Error(10, string.Format("FileImport failed, file name {0}", fileName))); }
            if (importedText.Length == 0)
            { Game.SetError(new Error(14, string.Format("No data in file {0}", fileName))); }
            return importedText;
        }

        /// <summary>
        /// handles simple string lists
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<string> GetStrings(string fileName)
        {
            // read in lists of strings
            string[] arrayOfStrings = ImportDataFile(fileName);
            List<string> listOfStrings = new List<string>();
            string tempString = null;
            //read straigth strings from array into list (handles blank lines & '#')
            for (int i = 0; i < arrayOfStrings.Length; i++)
            {
                if (arrayOfStrings[i] != "" && !arrayOfStrings[i].StartsWith("#"))
                {
                    //trim off leading and trailing whitespace
                    tempString = arrayOfStrings[i];
                    tempString = tempString.Trim();
                    listOfStrings.Add(tempString);
                }
            }
            return listOfStrings;
        }

        /// <summary>
        /// handles major and minor houses and returns a list of house Structs
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<HouseStruct> GetHouses(string fileName)
        {
            string[] arrayOfHouseNames = ImportDataFile(fileName);
            List<HouseStruct> listHouses = new List<HouseStruct>();
            bool newHouse = false;
            bool validData = true;
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
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new structure
                        houseStruct = new HouseStruct();
                    }
                    string[] tokens = arrayOfHouseNames[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    if (cleanToken.Length == 0)
                    { validData = false; Game.SetError(new Error(16, "Empty data field")); }
                    else
                    {
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
                                try { houseStruct.Archetype = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(18, e.Message)); validData = false; }
                                break;
                            case "ReferenceID":
                                try { houseStruct.RefID = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(18, e.Message)); validData = false; }
                                break;
                            case "Capital": //Major Houses
                                houseStruct.Capital = cleanToken;
                                //last datapoint - save structure to list
                                if (dataCounter > 0 && validData == true )
                                { listHouses.Add(houseStruct); }
                                break;
                            case "Seat": //Minor Houses
                                houseStruct.Capital = cleanToken;
                                //last datapoint - save structure to list
                                if (dataCounter > 0  && validData == true)
                                { listHouses.Add(houseStruct); }
                                break;
                            default:
                                Game.SetError(new Error(15, "Invalid Data in House Input"));
                                break;
                        }
                    }
                }
                else
                { newHouse = false; }
            }
            return listHouses;
        }

        /// <summary>
        /// read in an initialise Constants
        /// </summary>
        /// <param name="fileName"></param>
        public void GetConstants(string fileName)
        {
            string[] arrayOfFileInput = ImportDataFile(fileName); ;
            Console.WriteLine();
            Console.WriteLine("--- Initialise Constants");
            string cleanToken = null;
            string cleanTag = null;
            int index = 0;
            int value = 0;
            Global enumTag = Global.None;
            for (int i = 0; i < arrayOfFileInput.Length; i++)
            {
                if (arrayOfFileInput[i] != "" && !arrayOfFileInput[i].StartsWith("#"))
                {
                    string[] tokens = arrayOfFileInput[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    //convert to #'s
                    try
                    {
                        index = Convert.ToInt32(cleanTag);
                        value = Convert.ToInt32(cleanToken);
                        //get correct enum from Global array
                        enumTag = Game.constant.GetGlobal(index);
                        //initialise data in Constants array
                        Game.constant.SetData(enumTag, value);
                    }
                    catch (Exception e)
                    { Game.SetError(new Error(17, e.Message)); }
                }
            }
        }

        /// <summary>
        /// get traits from all a trait file, return a list of traits
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        internal List<Trait> GetTraits(string fileName, TraitSex sex)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newTrait = false;
            bool validData = true;
            List<Trait> listOfTraits = new List<Trait>();
            string[] arrayOfTraits = ImportDataFile(fileName);
            TraitStruct structTrait = new TraitStruct();
            //loop imported array of strings
            for (int i = 0; i < arrayOfTraits.Length; i++)
            {
                if (arrayOfTraits[i] != "" && !arrayOfTraits[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newTrait == false)
                    {
                        newTrait = true;
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new Trait object
                        structTrait = new TraitStruct();
                        //sex
                        structTrait.Sex = sex;
                    }
                    string[] tokens = arrayOfTraits[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    if (cleanToken.Length == 0 && cleanTag != "Nicknames")
                    { Game.SetError(new Error(20, string.Format("Empty data field, record {0}, {1}", i, fileName))); validData = false;  }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structTrait.Name = cleanToken;
                                break;
                            case "Skill":
                                switch (cleanToken)
                                {
                                    case "Combat":
                                        structTrait.Type = TraitType.Combat;
                                        break;
                                    case "Wits":
                                        structTrait.Type = TraitType.Wits;
                                        break;
                                    case "Charm":
                                        structTrait.Type = TraitType.Charm;
                                        break;
                                    case "Treachery":
                                        structTrait.Type = TraitType.Treachery;
                                        break;
                                    case "Leadership":
                                        structTrait.Type = TraitType.Leadership;
                                        break;
                                    case "Touched":
                                        structTrait.Type = TraitType.Touched;
                                        break;
                                }
                                break;
                            case "Effect":
                                try { structTrait.Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(20, e.Message)); validData = false; }
                                break;
                            case "Chance":
                                try { structTrait.Chance = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(20, e.Message)); validData = false; }
                                break;
                            case "Age":
                                try
                                {
                                    int tempNum = Convert.ToInt32(cleanToken);
                                    if (tempNum == 5)
                                    { structTrait.Age = TraitAge.Five; }
                                    else
                                    { structTrait.Age = TraitAge.Fifteen; }
                                }
                                catch (Exception e)
                                { Game.SetError(new Error(20, e.Message)); validData = false; }
                                break;
                            case "Nicknames":
                                //get list of nicknames
                                string[] arrayOfNames = cleanToken.Split(',');
                                List<string> tempList = new List<string>();
                                //loop nickname array and add all to lists
                                string tempHandle = null;
                                for (int k = 0; k < arrayOfNames.Length; k++)
                                {
                                    tempHandle = arrayOfNames[k].Trim();
                                    if (String.IsNullOrEmpty(tempHandle) == false)
                                    { tempList.Add(tempHandle); }
                                    //dodgy Nickname is ignored, it doesn't invalidate the record (some records deliberately don't have nicknames)
                                    else
                                    {
                                        if (arrayOfNames.Length > 1)
                                        { Game.SetError(new Error(21, string.Format("Invalid Nickname for {0}, {1}", structTrait.Name, fileName))); }
                                    }
                                }
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Trait classTrait = new Trait(structTrait.Name, structTrait.Type, structTrait.Effect, structTrait.Sex, structTrait.Age, structTrait.Chance, tempList);
                                    //last datapoint - save object to list
                                    if (dataCounter > 0)
                                    { listOfTraits.Add(classTrait); }
                                }
                                break;
                            default:
                                Game.SetError(new Error(22, string.Format("Invalid Data, record {0}, {1}", i, fileName)));
                                break;
                        }
                    }
                }
            }
            return listOfTraits;
        }

        /// <summary>
        /// Import GeoNames
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal string[][] GetGeoNames(string fileName)
        {
            //master jagged array which will be returned
            string[][] arrayOfNames = new string[(int)GeoType.Count][];
            string tempString;
            //temporary sub lists for each category of geoNames
            List<string> listOfLargeMountains = new List<string>();
            List<string> listOfMediumMountains = new List<string>();
            List<string> listOfSmallMountains = new List<string>();
            List<string> listOfLargeForests = new List<string>();
            List<string> listOfMediumForests = new List<string>();
            List<string> listOfSmallForests = new List<string>();
            List<string> listOfLargeSeas = new List<string>();
            List<string> listOfMediumSeas = new List<string>();
            List<string> listOfSmallSeas = new List<string>();
            //import data from file
            string[] arrayOfGeoNames = ImportDataFile(fileName);

            //read location names from array into list
            string nameType = null;
            char[] charsToTrim = { '[', ']' };
            for (int i = 0; i < arrayOfGeoNames.Length; i++)
            {
                if (arrayOfGeoNames[i] != "" && !arrayOfGeoNames[i].StartsWith("#"))
                {
                    //which sublist are we dealing with
                    tempString = arrayOfGeoNames[i];
                    //trim off leading and trailing whitespace
                    tempString = tempString.Trim();
                    if (tempString.StartsWith("["))
                    { nameType = tempString.Trim(charsToTrim); }
                    else if (nameType != null)
                    {
                        //place in the correct list
                        switch (nameType)
                        {
                            case "Large Seas":
                                listOfLargeSeas.Add(tempString);
                                break;
                            case "Medium Seas":
                                listOfMediumSeas.Add(tempString);
                                break;
                            case "Small Seas":
                                listOfSmallSeas.Add(tempString);
                                break;
                            case "Large Mountains":
                                listOfLargeMountains.Add(tempString);
                                break;
                            case "Medium Mountains":
                                listOfMediumMountains.Add(tempString);
                                break;
                            case "Small Mountains":
                                listOfSmallMountains.Add(tempString);
                                break;
                            case "Large Forests":
                                listOfLargeForests.Add(tempString);
                                break;
                            case "Medium Forests":
                                listOfMediumForests.Add(tempString);
                                break;
                            case "Small Forests":
                                listOfSmallForests.Add(tempString);
                                break;
                            default:
                                Game.SetError(new Error(23, string.Format("Invalid Category {0}, record {1}", nameType, i)));
                                break;
                        }
                    }
                }
            }
            //size jagged array
            arrayOfNames[(int)GeoType.Large_Mtn] = new string[listOfLargeMountains.Count];
            arrayOfNames[(int)GeoType.Medium_Mtn] = new string[listOfMediumMountains.Count];
            arrayOfNames[(int)GeoType.Small_Mtn] = new string[listOfSmallMountains.Count];
            arrayOfNames[(int)GeoType.Large_Forest] = new string[listOfLargeForests.Count];
            arrayOfNames[(int)GeoType.Medium_Forest] = new string[listOfMediumForests.Count];
            arrayOfNames[(int)GeoType.Small_Forest] = new string[listOfSmallForests.Count];
            arrayOfNames[(int)GeoType.Large_Sea] = new string[listOfLargeSeas.Count];
            arrayOfNames[(int)GeoType.Medium_Sea] = new string[listOfMediumSeas.Count];
            arrayOfNames[(int)GeoType.Small_Sea] = new string[listOfSmallSeas.Count];
            //populate from lists
            arrayOfNames[(int)GeoType.Large_Mtn] = listOfLargeMountains.ToArray();
            arrayOfNames[(int)GeoType.Medium_Mtn] = listOfMediumMountains.ToArray();
            arrayOfNames[(int)GeoType.Small_Mtn] = listOfSmallMountains.ToArray();
            arrayOfNames[(int)GeoType.Large_Forest] = listOfLargeForests.ToArray();
            arrayOfNames[(int)GeoType.Medium_Forest] = listOfMediumForests.ToArray();
            arrayOfNames[(int)GeoType.Small_Forest] = listOfSmallForests.ToArray();
            arrayOfNames[(int)GeoType.Large_Sea] = listOfLargeSeas.ToArray();
            arrayOfNames[(int)GeoType.Medium_Sea] = listOfMediumSeas.ToArray();
            arrayOfNames[(int)GeoType.Small_Sea] = listOfSmallSeas.ToArray();

            return arrayOfNames;
        }

        //methods above here
    }
}
