using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using System.IO;


namespace Next_Game
{
    //NOTE: structures are needed as using objects within the import routines runs into Heap reference issues. Beaware that you're using the Stack and memory could be an issue.

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

    //holds data for Events
    struct EventStruct
    {
        public string Name { get; set; }
        public string EventText { get; set; }
        public string SucceedText { get; set; }
        public string FailText { get; set; }
        public int TempID { get; set; }
        public ArcType Type { get; set; }
        public ArcGeo Geo { get; set; }
        public ArcRoad Road { get; set; }
        public ArcLoc Loc { get; set; }
        public EventCategory Cat { get; set; }
        public EventFrequency Frequency { get; set; }
        public TraitType Trait { get; set; }
        public int Delay { get; set; }
    }

    //archetypes
    struct ArcStruct
    {
        public string Name { get; set; }
        public int TempID { get; set; }
        public ArcType Type { get; set; } //which class of object it applies to
        public ArcGeo Geo { get; set; }
        public ArcLoc Loc { get; set; }
        public ArcRoad Road { get; set; }
        public string SubType { get; set; }
        public List<int> listOfEvents { get; set; }
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
            //check all required data has been successfully imported
            Game.constant.ErrorCheck();
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
        /// Import Events
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, Event> GetEvents(string fileName)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newEvent = false;
            bool validData = true;
            int dataInt;
            List<int> listTempID = new List<int>(); //used to pick up duplicate tempID's
            Dictionary<int, Event> dictOfEvents = new Dictionary<int, Event>();
            string[] arrayOfEvents = ImportDataFile(fileName);
            EventStruct structEvent = new EventStruct();
            //loop imported array of strings
            for (int i = 0; i < arrayOfEvents.Length; i++)
            {
                if (arrayOfEvents[i] != "" && !arrayOfEvents[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newEvent == false)
                    {
                        newEvent = true;
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new Trait object
                        structEvent = new EventStruct();
                    }
                    string[] tokens = arrayOfEvents[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    if (cleanToken.Length == 0)
                    { Game.SetError(new Error(20, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structEvent.Name = cleanToken;
                                break;
                            case "TempID":
                                try
                                { structEvent.TempID = Convert.ToInt32(cleanToken); }
                                catch
                                { Game.SetError(new Error(49, string.Format("Invalid input for TempID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                //check for duplicate TempID's
                                if (listTempID.Contains(structEvent.TempID))
                                { Game.SetError(new Error(49, string.Format("Duplicate TempID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                else { listTempID.Add(structEvent.TempID); }
                                break;
                            case "Type":
                                switch (cleanToken)
                                {
                                    case "GeoCluster":
                                        structEvent.Type = ArcType.GeoCluster;
                                        break;
                                    case "Location":
                                        structEvent.Type = ArcType.Location;
                                        break;
                                    case "Road":
                                        structEvent.Type = ArcType.Road;
                                        break;
                                    default:
                                        structEvent.Type = ArcType.None;
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "subType":
                                //NOTE: subType needs to be AFTER Type in text file
                                switch (structEvent.Type)
                                {
                                    case ArcType.GeoCluster:
                                        switch (cleanToken)
                                        {
                                            case "Sea":
                                                structEvent.Geo = ArcGeo.Sea;
                                                break;
                                            case "Mountain":
                                                structEvent.Geo = ArcGeo.Mountain;
                                                break;
                                            case "Forest":
                                                structEvent.Geo = ArcGeo.Forest;
                                                break;
                                            default:
                                                structEvent.Geo = ArcGeo.None;
                                                Game.SetError(new Error(49, string.Format("Invalid Input, GeoCluster Type, (\"{0}\")", arrayOfEvents[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Location:
                                        switch (cleanToken)
                                        {
                                            case "Capital":
                                                structEvent.Loc = ArcLoc.Capital;
                                                break;
                                            case "Major":
                                                structEvent.Loc = ArcLoc.Major;
                                                break;
                                            case "Minor":
                                                structEvent.Loc = ArcLoc.Minor;
                                                break;
                                            case "Inn":
                                                structEvent.Loc = ArcLoc.Inn;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, Location Type, (\"{0}\")", arrayOfEvents[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Road:
                                        switch (cleanToken)
                                        {
                                            case "Normal":
                                                structEvent.Road = ArcRoad.Normal;
                                                break;
                                            case "Kings":
                                                structEvent.Road = ArcRoad.Kings;
                                                break;
                                            case "Connector":
                                                structEvent.Road = ArcRoad.Connector;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, Road Type, (\"{0}\")", arrayOfEvents[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Category":
                                switch (cleanToken)
                                {
                                    case "Generic":
                                        structEvent.Cat = EventCategory.Generic;
                                        break;
                                    case "Special":
                                        structEvent.Cat = EventCategory.Special;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Category, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Frequency":
                                switch (cleanToken)
                                {
                                    case "Very_Rare":
                                        structEvent.Frequency = EventFrequency.Very_Rare;
                                        break;
                                    case "Rare":
                                        structEvent.Frequency = EventFrequency.Rare;
                                        break;
                                    case "Low":
                                        structEvent.Frequency = EventFrequency.Low;
                                        break;
                                    case "Normal":
                                        structEvent.Frequency = EventFrequency.Normal;
                                        break;
                                    case "High":
                                        structEvent.Frequency = EventFrequency.High;
                                        break;
                                    case "Common":
                                        structEvent.Frequency = EventFrequency.Common;
                                        break;
                                    case "Very_Common":
                                        structEvent.Frequency = EventFrequency.Very_Common;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Frequency, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "EventText":
                                structEvent.EventText = cleanToken;
                                break;
                            case "SucceedText":
                                structEvent.SucceedText = cleanToken;
                                break;
                            case "FailText":
                                structEvent.FailText = cleanToken;
                                break;
                            case "Trait":
                                switch (cleanToken)
                                {
                                    case "Combat":
                                        structEvent.Trait = TraitType.Combat;
                                        break;
                                    case "Wits":
                                        structEvent.Trait = TraitType.Wits;
                                        break;
                                    case "Charm":
                                        structEvent.Trait = TraitType.Charm;
                                        break;
                                    case "Treachery":
                                        structEvent.Trait = TraitType.Treachery;
                                        break;
                                    case "Leadership":
                                        structEvent.Trait = TraitType.Leadership;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Trait, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Delay":
                                dataInt = Convert.ToInt32(cleanToken);
                                if (dataInt > 0)
                                { structEvent.Delay = dataInt; }
                                else
                                { Game.SetError(new Error(49, string.Format("Invalid Input, Delay, (\"{0}\")", arrayOfEvents[i]))); validData = false; }
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Event eventObject = null;
                                    switch (structEvent.Type)
                                    {
                                        case ArcType.GeoCluster:
                                            eventObject = new EventGeo(structEvent.TempID, structEvent.Name, structEvent.Frequency, structEvent.Geo);
                                            break;
                                        case ArcType.Location:
                                            eventObject = new EventLoc(structEvent.TempID, structEvent.Name, structEvent.Frequency, structEvent.Loc);
                                            break;
                                        case ArcType.Road:
                                            eventObject = new EventRoad(structEvent.TempID, structEvent.Name, structEvent.Frequency, structEvent.Road);
                                            break;
                                    }
                                    if (eventObject != null)
                                    {
                                        EventGeneric eventTemp = eventObject as EventGeneric;
                                        eventTemp.EventText = structEvent.EventText;
                                        eventTemp.SucceedText = structEvent.SucceedText;
                                        eventTemp.FailText = structEvent.FailText;
                                        eventTemp.Trait = structEvent.Trait;
                                        eventTemp.Delay = structEvent.Delay;
                                        eventTemp.Category = structEvent.Cat;
                                        //last datapoint - save object to list
                                        if (dataCounter > 0)
                                        { dictOfEvents.Add(eventTemp.EventID, eventTemp); }
                                    }
                                    else { Game.SetError(new Error(49, "Invalid Input, eventObject")); }
                                }
                                else
                                { Game.SetError(new Error(49, string.Format("Event, (\"{0}\" TempID {1}), not Imported", structEvent.Name, structEvent.TempID))); }
                                break;
                            default:
                                Game.SetError(new Error(49, "Invalid Input, CleanTag"));
                                break;
                        }
                    }
                }
                else { newEvent = false; }
            }
            return dictOfEvents;
        }

        /// <summary>
        /// Import Archetypes
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, Archetype> GetArchetypes(string fileName)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newArc = false;
            bool validData = true;
            int dataInt;
            List<int> listTempID = new List<int>(); //used to pick up duplicate tempID's
            Dictionary<int, Archetype> dictOfArchetypes = new Dictionary<int, Archetype>();
            string[] arrayOfArchetypes = ImportDataFile(fileName);
            ArcStruct structArc = new ArcStruct();
            //loop imported array of strings
            for (int i = 0; i < arrayOfArchetypes.Length; i++)
            {
                if (arrayOfArchetypes[i] != "" && !arrayOfArchetypes[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newArc == false)
                    {
                        newArc = true;
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new Trait object
                        structArc = new ArcStruct();
                    }
                    string[] tokens = arrayOfArchetypes[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    if (cleanTag == "End") 
                    { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else
                    { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    { Game.SetError(new Error(53, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structArc.Name = cleanToken;
                                break;
                            case "TempID":
                                structArc.TempID = Convert.ToInt32(cleanToken);
                                break;
                            case "Type":
                                switch (cleanToken)
                                {
                                    case "GeoCluster":
                                        structArc.Type = ArcType.GeoCluster;
                                        break;
                                    case "Location":
                                        structArc.Type = ArcType.Location;
                                        break;
                                    case "Road":
                                        structArc.Type = ArcType.Road;
                                        break;
                                    default:
                                        structArc.Type = ArcType.None;
                                        Game.SetError(new Error(53, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfArchetypes[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "subType":
                                //NOTE: subType needs to be AFTER Type in text file
                                switch (structArc.Type)
                                {
                                    case ArcType.GeoCluster:
                                        switch (cleanToken)
                                        {
                                            case "Sea":
                                                structArc.Geo = ArcGeo.Sea;
                                                break;
                                            case "Mountain":
                                                structArc.Geo = ArcGeo.Mountain;
                                                break;
                                            case "Forest":
                                                structArc.Geo = ArcGeo.Forest;
                                                break;
                                            default:
                                                structArc.Geo = ArcGeo.None;
                                                Game.SetError(new Error(53, string.Format("Invalid Input, GeoCluster Type, (\"{0}\")", arrayOfArchetypes[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Location:
                                        switch (cleanToken)
                                        {
                                            case "Capital":
                                                structArc.Loc = ArcLoc.Capital;
                                                break;
                                            case "Major":
                                                structArc.Loc = ArcLoc.Major;
                                                break;
                                            case "Minor":
                                                structArc.Loc = ArcLoc.Minor;
                                                break;
                                            case "Inn":
                                                structArc.Loc = ArcLoc.Inn;
                                                break;
                                            default:
                                                Game.SetError(new Error(53, string.Format("Invalid Input, Location Type, (\"{0}\")", arrayOfArchetypes[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Road:
                                        switch (cleanToken)
                                        {
                                            case "Normal":
                                                structArc.Road = ArcRoad.Normal;
                                                break;
                                            case "Kings":
                                                structArc.Road = ArcRoad.Kings;
                                                break;
                                            case "Connector":
                                                structArc.Road = ArcRoad.Connector;
                                                break;
                                            default:
                                                Game.SetError(new Error(53, string.Format("Invalid Input, Road Type, (\"{0}\")", arrayOfArchetypes[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    default:
                                        Game.SetError(new Error(53, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfArchetypes[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Ev_Follower":
                                //get list of Events
                                string[] arrayOfEvents = cleanToken.Split(',');
                                List<int> tempList = new List<int>();
                                //loop eventID array and add all to lists
                                string tempHandle = null;
                                for (int k = 0; k < arrayOfEvents.Length; k++)
                                {
                                    tempHandle = arrayOfEvents[k].Trim();
                                    if (String.IsNullOrEmpty(tempHandle) == false)
                                    {
                                        try
                                        {
                                            dataInt = Convert.ToInt32(tempHandle);
                                            if (dataInt > 0)
                                            { tempList.Add(dataInt); }
                                            else
                                            { Game.SetError(new Error(53, string.Format("Invalid EventID (Zero Value) for {0}, {1}", structArc.Name, fileName))); validData = false; }
                                        }
                                        catch { Game.SetError(new Error(53, string.Format("Invalid EventID (Conversion Error) for {0}, {1}", structArc.Name, fileName))); validData = false; }
                                    }
                                    //dodgy EventID is ignored, it doesn't invalidate the record (some records deliberately don't have nicknames)
                                    else
                                    { Game.SetError(new Error(53, string.Format("Invalid EventID for {0}, {1}", structArc.Name, fileName))); validData = false; }
                                }
                                structArc.listOfEvents = tempList;
                                break;
                            case "End":
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Archetype arcObject = null;
                                    switch (structArc.Type)
                                    {
                                        case ArcType.GeoCluster:
                                            arcObject = new ArcTypeGeo(structArc.Name, structArc.Geo, structArc.TempID, structArc.listOfEvents);
                                            break;
                                        case ArcType.Location:
                                            arcObject = new ArcTypeLoc(structArc.Name, structArc.Loc, structArc.TempID, structArc.listOfEvents);
                                            break;
                                        case ArcType.Road:
                                            arcObject = new ArcTypeRoad(structArc.Name, structArc.Road, structArc.TempID, structArc.listOfEvents);
                                            break;
                                    }
                                    if (arcObject != null)
                                    {
                                        Archetype arcTemp = arcObject as Archetype;
                                        
                                        //last datapoint - save object to list
                                        if (dataCounter > 0)
                                        { dictOfArchetypes.Add(arcTemp.ArcID, arcTemp); }
                                    }
                                    else { Game.SetError(new Error(53, "Invalid Input, arcObject")); }
                                }
                                else
                                { Game.SetError(new Error(53, string.Format("Archetype, (\"{0}\" TempID {1}), not Imported", structArc.Name, structArc.TempID))); }
                                break;
                            default:
                                Game.SetError(new Error(53, string.Format("Invalid Input, CleanTag {0}, \"{1}\"", cleanTag, structArc.Name)));
                                break;
                        }
                    }
                }
                else { newArc = false; }
            }
            return dictOfArchetypes;
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
