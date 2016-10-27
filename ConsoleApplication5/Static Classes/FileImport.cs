using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using Next_Game.Event_System;
using System.IO;


namespace Next_Game
{
    //NOTE: structures are needed as using objects within the import routines runs into Heap reference issues. Beaware that you're using the Stack and memory could be an issue.

    //Houses
    public struct HouseStruct
    {
        public string Name { get; set; }
        public string Special { get; set; } //special houses only, ignore otherwise
        public string Motto { get; set; }
        public string Banner { get; set; }
        public int ArcID { get; set; }
        public int RefID { get; set; }
        public string Capital { get; set; }
    }

    //Traits
    struct TraitStruct
    {
        public string Name { get; set; }
        public TraitType Type { get; set; }
        public TraitSex Sex { get; set; }
        public TraitAge Age { get; set; }
        public int Effect { get; set; }
        public int Chance { get; set; }
    }

    //Follower Events
    struct EventFollowerStruct
    {
        public string Name { get; set; }
        public string EventText { get; set; }
        public string SucceedText { get; set; }
        public string FailText { get; set; }
        public int EventID { get; set; }
        public ArcType Type { get; set; }
        public ArcGeo Geo { get; set; }
        public ArcRoad Road { get; set; }
        public ArcLoc Loc { get; set; }
        public ArcHouse House { get; set; }
        public ArcActor Actor { get; set; }
        public EventCategory Cat { get; set; }
        public EventStatus Status { get; set; }
        public EventFrequency Frequency { get; set; }
        public TraitType Trait { get; set; }
        public int Delay { get; set; }
    }

    //Player Events
    struct EventPlayerStruct
    {
        public string Name { get; set; }
        public string EventText { get; set; }
        public int EventID { get; set; }
        public ArcType Type { get; set; }
        public ArcGeo Geo { get; set; }
        public ArcRoad Road { get; set; }
        public ArcLoc Loc { get; set; }
        public ArcHouse House { get; set; }
        public ArcActor Actor { get; set; }
        public EventCategory Cat { get; set; }
        public EventStatus Status { get; set; }
        public EventFrequency Frequency { get; set; }
    }

    struct OptionStruct
    {
        public string Text { get; set; }
        public string Reply { get; set; }
    }

    struct OutcomeStruct
    {
        public string Effect { get; set; }
        public int Data { get; set; }
        public int Amount { get; set; }
        public EventCalc Apply { get; set; }
    }

    struct TriggerStruct
    {
        public TriggerCheck Check { get; set; }
        public int Item { get; set; }
        public int Threshold { get; set; }
        public EventCalc Calc { get; set; }
    }

    //archetypes
    struct ArcStruct
    {
        public string Name { get; set; }
        public int ArcID { get; set; }
        public int Chance { get; set; }
        public ArcType Type { get; set; } //which class of object it applies to
        public ArcGeo Geo { get; set; }
        public ArcLoc Loc { get; set; }
        public ArcRoad Road { get; set; }
        public ArcHouse House { get; set; }
        public ArcActor Actor { get; set; }
        public string SubType { get; set; }
        public List<int> listOfEvents { get; set; }
    }

    //story
    struct StoryStruct
    {
        public string Name { get; set; }
        public int StoryID { get; set; }
        public StoryAI Type { get; set; }
        public int Ev_Follower_Loc { get; set; } // chance of a follower experiencing a random event when at a Location
        public int Ev_Follower_Trav { get; set; } // chance of a follower experiencing a random event when travelling
        public int Ev_Player_Loc { get; set; } // chance of the Player experiencing a random event
        public int Ev_Player_Trav { get; set; }
        //categoryies of archetypes
        public int Sea { get; set; }
        public int Mountain { get; set; }
        public int Forest { get; set; }
        public int Capital { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Inn { get; set; }
        public int Normal { get; set; }
        public int Kings { get; set; }
        public int Connector { get; set; }
    }

    //follower
    struct FollowerStruct
    {
        public string Name { get; set; }
        public int FID { get; set; } //follower ID
        public ActorSex Sex { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public string Special { get; set; } //weakness or strength peculiar to the follower
        public int ArcID { get; set; } //archetype for events tied into the Special trait
        public int Resources { get; set; } //any starting resources
        public int Loyalty { get; set; } //loyalty to the player (1 to 5 stars)
        public int Combat_Effect { get; set; }
        public int Wits_Effect { get; set; }
        public int Charm_Effect { get; set; }
        public int Treachery_Effect { get; set; }
        public int Leadership_Effect { get; set; }
        public int Touched_Effect { get; set; }
        public string Combat_Trait { get; set; }
        public string Wits_Trait { get; set; }
        public string Charm_Trait { get; set; }
        public string Treachery_Trait { get; set; }
        public string Leadership_Trait { get; set; }
        public string Touched_Trait { get; set; }
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
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    {
                        validData = false;
                        Game.SetError(new Error(16, string.Format("House {0} (Missing data for \"{1}\") \"{2}\"", 
                        String.IsNullOrEmpty(houseStruct.Name) ? "?" : houseStruct.Name, cleanTag, fileName))); }
                    else
                    {
                        //Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                        switch (cleanTag)
                        {
                            case "Name":
                            case "House":
                                houseStruct.Name = cleanToken;
                                break;
                            case "Special":
                                //special house types only, ignore otherwise
                                houseStruct.Special = cleanToken;
                                break;
                            case "Motto":
                                houseStruct.Motto = cleanToken;
                                break;
                            case "Sign":
                            case "Banner":
                                houseStruct.Banner = cleanToken;
                                break;
                            case "ArcID":
                                try { houseStruct.ArcID = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(18, e.Message)); validData = false; }
                                break;
                            case "RefID":
                                try { houseStruct.RefID = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(18, e.Message)); validData = false; }
                                break;
                            case "Capital": //Major Houses
                                houseStruct.Capital = cleanToken;
                                break;
                            case "Seat": //Minor Houses
                                houseStruct.Capital = cleanToken;
                                break;
                            case "end":
                            case "End":
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
            List<string> listOfNickNames = new List<string>();
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
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0 && cleanTag != "Nicknames")
                    {
                        validData = false;
                        Game.SetError(new Error(20, string.Format("Trait {0} (Missing data for \"{1}\") \"{2}\"",
                            String.IsNullOrEmpty(structTrait.Name) ? "?" : structTrait.Name, cleanTag, fileName)));
                    }
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
                                listOfNickNames.Clear();
                                string[] arrayOfNames = cleanToken.Split(',');
                                //loop nickname array and add all to lists
                                string tempHandle = null;
                                for (int k = 0; k < arrayOfNames.Length; k++)
                                {
                                    tempHandle = arrayOfNames[k].Trim();
                                    if (String.IsNullOrEmpty(tempHandle) == false)
                                    { listOfNickNames.Add(tempHandle); }
                                    //dodgy Nickname is ignored, it doesn't invalidate the record (some records deliberately don't have nicknames)
                                    else
                                    {
                                        if (arrayOfNames.Length > 1)
                                        { Game.SetError(new Error(21, string.Format("Invalid Nickname for {0}, {1}", structTrait.Name, fileName))); }
                                    }
                                }
                                break;
                            case "end":
                            case "End":
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Trait classTrait = new Trait(structTrait.Name, structTrait.Type, structTrait.Effect, structTrait.Sex, structTrait.Age, 
                                        structTrait.Chance, listOfNickNames);
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
        /// Import Follower Events
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, EventFollower> GetFollowerEvents(string fileName)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newEvent = false;
            bool validData = true;
            int dataInt;
            List<int> listEventID = new List<int>(); //used to pick up duplicate eventID's
            Dictionary<int, EventFollower> dictOfFollowerEvents = new Dictionary<int, EventFollower>();
            string[] arrayOfEvents = ImportDataFile(fileName);
            EventFollowerStruct structEvent = new EventFollowerStruct();
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
                        structEvent = new EventFollowerStruct();
                    }
                    string[] tokens = arrayOfEvents[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    { Game.SetError(new Error(20, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structEvent.Name = cleanToken;
                                break;
                            case "ID":
                                try
                                { structEvent.EventID = Convert.ToInt32(cleanToken); }
                                catch
                                { Game.SetError(new Error(49, string.Format("Invalid input for EventID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                //check for duplicate EventID's
                                if (listEventID.Contains(structEvent.EventID))
                                { Game.SetError(new Error(49, string.Format("Duplicate EventID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                else { listEventID.Add(structEvent.EventID); }
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
                                    case "Actor":
                                        structEvent.Type = ArcType.Actor;
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
                                    case ArcType.House:
                                        switch (cleanToken)
                                        {
                                            case "Major":
                                                structEvent.House = ArcHouse.Major;
                                                break;
                                            case "Minor":
                                                structEvent.House = ArcHouse.Minor;
                                                break;
                                            case "Inn":
                                                structEvent.House = ArcHouse.Inn;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, House Type, (\"{0}\")", arrayOfEvents[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Actor:
                                        switch (cleanToken)
                                        {
                                            case "Player":
                                                structEvent.Actor = ArcActor.Player;
                                                break;
                                            case "Follower":
                                                structEvent.Actor = ArcActor.Follower;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, Actor Type, (\"{0}\")", arrayOfEvents[i])));
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
                            case "Cat":
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
                            case "Freq":
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
                                    case "Common":
                                        structEvent.Frequency = EventFrequency.Common;
                                        break;
                                    case "High":
                                        structEvent.Frequency = EventFrequency.High;
                                        break;
                                    case "Very_High":
                                        structEvent.Frequency = EventFrequency.Very_High;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Frequency, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Event":
                                structEvent.EventText = cleanToken;
                                break;
                            case "Succeed":
                                structEvent.SucceedText = cleanToken;
                                break;
                            case "Fail":
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
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structEvent.Delay = dataInt; }
                                    else
                                    { Game.SetError(new Error(49, string.Format("Invalid Input, Delay, (\"{0}\")", arrayOfEvents[i]))); validData = false; }
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Delay, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                break;
                            case "Status":
                                switch (cleanToken)
                                {
                                    case "Active":
                                        structEvent.Status = EventStatus.Active;
                                        break;
                                    case "Live":
                                        structEvent.Status = EventStatus.Live;
                                        break;
                                    case "Dormant":
                                        structEvent.Status = EventStatus.Dormant;
                                        break;
                                    case "Dead":
                                        structEvent.Status = EventStatus.Dead;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Status, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "end":
                            case "End":
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Event eventObject = null;
                                    switch (structEvent.Type)
                                    {
                                        case ArcType.GeoCluster:
                                            eventObject = new EventFolGeo(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Geo);
                                            break;
                                        case ArcType.Location:
                                            eventObject = new EventFolLoc(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Loc);
                                            break;
                                        case ArcType.Road:
                                            eventObject = new EventFolRoad(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Road);
                                            break;
                                        case ArcType.House:
                                            eventObject = new EventFolHouse(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.House);
                                            break;
                                        case ArcType.Actor:
                                            eventObject = new EventFolActor(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Actor);
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid ArcType for Object (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    if (eventObject != null)
                                    {
                                        EventFollower eventTemp = eventObject as EventFollower;
                                        eventTemp.Text = structEvent.EventText;
                                        eventTemp.Category = structEvent.Cat;
                                        //if no data provided for Status then default to 'Active'
                                        if (structEvent.Status == EventStatus.None)
                                        { eventTemp.Status = EventStatus.Active; }
                                        else { eventTemp.Status = structEvent.Status; }
                                        //add option
                                        OptionAuto option = new OptionAuto(structEvent.Trait) { ReplyGood = structEvent.SucceedText, ReplyBad = structEvent.FailText };
                                        option.SetBadOutcome(new OutDelay(structEvent.Delay, eventTemp.EventFID));
                                        eventTemp.SetOption(option);
                                        //last datapoint - save object to list
                                        if (dataCounter > 0)
                                        { dictOfFollowerEvents.Add(eventTemp.EventFID, eventTemp); }
                                    }
                                    else { Game.SetError(new Error(49, "Invalid Input, eventObject")); }
                                }
                                else
                                { Game.SetError(new Error(49, string.Format("Event, (\"{0}\" EventID {1}), not Imported", structEvent.Name, structEvent.EventID))); }
                                break;
                            default:
                                Game.SetError(new Error(49, "Invalid Input, CleanTag"));
                                break;
                        }
                    }
                }
                else { newEvent = false; }
            }
            return dictOfFollowerEvents;
        }


        /// <summary>
        /// Import Player Events
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, EventPlayer> GetPlayerEvents(string fileName)
        {
            int dataCounter = 0;
            bool optionFlag = false;
            bool outcomeFlag = false;
            bool triggerFlag = false;
            string cleanTag;
            string cleanToken;
            bool newEvent = false;
            bool validData = true;
            int dataInt;
            List<int> listEventID = new List<int>(); //used to pick up duplicate eventID's
            Dictionary<int, EventPlayer> dictOfPlayerEvents = new Dictionary<int, EventPlayer>();
            string[] arrayOfEvents = ImportDataFile(fileName);
            EventPlayerStruct structEvent = new EventPlayerStruct();
            OptionStruct structOption = new OptionStruct();
            OutcomeStruct structOutcome = new OutcomeStruct();
            TriggerStruct structTrigger = new TriggerStruct();
            List<OptionStruct> listOptions = null;
            List<List<OutcomeStruct>> listAllOutcomes = null; //all outcomes for an event (each option can have multiple outcomes)
            List<OutcomeStruct> listSubOutcomes = new List<OutcomeStruct>(); //list of outcomes for an individual option
            List<Trigger> listSubTriggers = new List<Trigger>(); //list of individual triggers specific to an option
            List<List<Trigger>> listAllTriggers = null; //all triggers for an event (each option can have multiple triggers)
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
                        //new objects
                        structEvent = new EventPlayerStruct();
                        listOptions = new List<OptionStruct>();
                        listAllOutcomes = new List<List<OutcomeStruct>>();
                        listAllTriggers = new List<List<Trigger>>();
                        //set flags to false
                        optionFlag = false;
                        outcomeFlag = false;
                        triggerFlag = false;
                    }
                    string[] tokens = arrayOfEvents[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    //if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    { Game.SetError(new Error(20, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "[option]":
                                //option complete, save
                                if (optionFlag == true)
                                {
                                    if (outcomeFlag == true)
                                    { listSubOutcomes.Add(structOutcome); }
                                    listAllOutcomes.Add(listSubOutcomes);
                                    listOptions.Add(structOption);
                                    outcomeFlag = false;
                                    //add Triggers to a list in same sequential order as options (place a blank trigger in the list if none exists)
                                    if (listSubTriggers.Count > 0)
                                    {
                                        if (structTrigger.Check > 0)
                                        {
                                            //add remaining trigger to list
                                            Trigger trigger = new Trigger(structTrigger.Check, structTrigger.Item, structTrigger.Threshold, structTrigger.Calc);
                                            listSubTriggers.Add(trigger);
                                            //reset to default value
                                            structTrigger.Check = TriggerCheck.None;
                                        }
                                        List<Trigger> tempTriggers = new List<Trigger>(listSubTriggers);
                                        listAllTriggers.Add(tempTriggers);
                                    }
                                    else
                                    {
                                        Trigger trigger;
                                        //if not default value then write full data
                                        if (structTrigger.Check > 0)
                                        {
                                            trigger = new Trigger(structTrigger.Check, structTrigger.Item, structTrigger.Threshold, structTrigger.Calc);
                                            //reset to default value
                                            structTrigger.Check = TriggerCheck.None;
                                        }
                                        else
                                        {
                                            //otherwise create list with a single blank Trigger (effect = none)
                                            trigger = new Trigger();
                                        }
                                        listSubTriggers.Add(trigger);
                                        List<Trigger> tempTriggers = new List<Trigger>(listSubTriggers);
                                        listAllTriggers.Add(tempTriggers);
                                    }
                                    //zero out listSubTriggers
                                    listSubTriggers.Clear();
                                    triggerFlag = false;
                                }
                                //set flag to true so option is saved on next tag
                                else
                                { optionFlag = true; }
                                break;
                            case "[outcome]":
                                //outcome complete, save
                                if (outcomeFlag == true)
                                { listSubOutcomes.Add(structOutcome); }
                                else
                                {
                                    outcomeFlag = true;
                                    //create new sublist of outcomes for each option
                                    listSubOutcomes = new List<OutcomeStruct>();
                                }
                                break;
                            case "[trigger]":
                                //trigger complete, save
                                if (triggerFlag == true)
                                {
                                    try
                                    {
                                        //add trigger to list
                                        Trigger trigger = new Trigger(structTrigger.Check, structTrigger.Item, structTrigger.Threshold, structTrigger.Calc);
                                        listSubTriggers.Add(trigger);
                                        //reset to default value
                                        structTrigger.Check = TriggerCheck.None;
                                    }
                                    catch (Exception e)
                                    { Game.SetError(new Error(48, e.Message)); validData = false; }
                                }
                                else
                                { triggerFlag = true; }
                                break;
                            case "Name":
                                structEvent.Name = cleanToken;
                                break;
                            case "ID":
                                try
                                { structEvent.EventID = Convert.ToInt32(cleanToken); }
                                catch
                                { Game.SetError(new Error(49, string.Format("Invalid input for EventID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                //check for duplicate EventID's
                                if (listEventID.Contains(structEvent.EventID))
                                { Game.SetError(new Error(49, string.Format("Duplicate EventID {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                else { listEventID.Add(structEvent.EventID); }
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
                                    case "Actor":
                                        structEvent.Type = ArcType.Actor;
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
                                    case ArcType.House:
                                        switch (cleanToken)
                                        {
                                            case "Major":
                                                structEvent.House = ArcHouse.Major;
                                                break;
                                            case "Minor":
                                                structEvent.House = ArcHouse.Minor;
                                                break;
                                            case "Inn":
                                                structEvent.House = ArcHouse.Inn;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, House Type, (\"{0}\")", arrayOfEvents[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Actor:
                                        switch (cleanToken)
                                        {
                                            case "Player":
                                                structEvent.Actor = ArcActor.Player;
                                                break;
                                            case "Follower":
                                                structEvent.Actor = ArcActor.Follower;
                                                break;
                                            default:
                                                Game.SetError(new Error(49, string.Format("Invalid Input, Actor Type, (\"{0}\")", arrayOfEvents[i])));
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
                            case "Cat":
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
                            case "Freq":
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
                                    case "Common":
                                        structEvent.Frequency = EventFrequency.Common;
                                        break;
                                    case "High":
                                        structEvent.Frequency = EventFrequency.High;
                                        break;
                                    case "Very_High":
                                        structEvent.Frequency = EventFrequency.Very_High;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Frequency, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Event":
                                structEvent.EventText = cleanToken;
                                break;
                            case "Status":
                                switch (cleanToken)
                                {
                                    case "Active":
                                        structEvent.Status = EventStatus.Active;
                                        break;
                                    case "Live":
                                        structEvent.Status = EventStatus.Live;
                                        break;
                                    case "Dormant":
                                        structEvent.Status = EventStatus.Dormant;
                                        break;
                                    case "Dead":
                                        structEvent.Status = EventStatus.Dead;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Status, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "text":
                                //Option text
                                structOption.Text = cleanToken;
                                break;
                            case "reply":
                                //Option reply
                                structOption.Reply = cleanToken;
                                break;
                            case "check":
                                //Trigger Check
                                switch (cleanToken)
                                {
                                    case "Trait":
                                        structTrigger.Check = TriggerCheck.Trait;
                                        break;
                                    case "GameVar":
                                        structTrigger.Check = TriggerCheck.GameVar;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Trigger Check, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "item":
                                //Trigger item
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0) { structTrigger.Item = dataInt; }
                                    else
                                    { Game.SetError(new Error(49, string.Format("Invalid Input, Trigger Item, (value less than 0) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Trigger Item, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                break;
                            case "thresh":
                                //Trigger Threshold
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    structTrigger.Threshold = dataInt;
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Trigger Item, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                break;
                            case "calc":
                                //Trigger Comparator
                                switch (cleanToken)
                                {
                                    case ">=":
                                        structTrigger.Calc = EventCalc.GreaterThanOrEqual;
                                        break;
                                    case "<=":
                                        structTrigger.Calc = EventCalc.LessThanOrEqual;
                                        break;
                                    case "=":
                                        structTrigger.Calc = EventCalc.Equals;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Trigger Calc, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;

                                }
                                break;
                            case "effect":
                                //Outcome effect
                                switch (cleanToken)
                                {
                                    case "Conflict":
                                    case "Game":
                                    case "Event":
                                        structOutcome.Effect = cleanToken;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Effect, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "out":
                                //outcome type (multipurpose)
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    structOutcome.Data = dataInt;
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Data, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                break;
                            case "amount":
                                try
                                { structOutcome.Amount = Convert.ToInt32(cleanToken); }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Amount, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                break;
                            case "apply":
                                switch (cleanToken)
                                {
                                    case "None":
                                        structOutcome.Apply = Event_System.EventCalc.None;
                                        break;
                                    case "Add":
                                        structOutcome.Apply = Event_System.EventCalc.Add;
                                        break;
                                    case "Subtract":
                                        structOutcome.Apply = Event_System.EventCalc.Subtract;
                                        break;
                                    case "Random":
                                        structOutcome.Apply = Event_System.EventCalc.Random;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Apply, (\"{0}\")", arrayOfEvents[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "[end]":
                            case "[End]":
                                //tidy up outstanding options
                                if (optionFlag == true)
                                {
                                    //tidy up last outcome
                                    if (outcomeFlag == true)
                                    {
                                        listSubOutcomes.Add(structOutcome);
                                        listAllOutcomes.Add(listSubOutcomes);
                                        //add Triggers to a list in same sequential order as options (place a blank trigger in the list if none exists)
                                        if (listSubTriggers.Count > 0)
                                        { listAllTriggers.Add(listSubTriggers); }
                                        else
                                        {
                                            Trigger trigger;
                                            //if not default value then write full data
                                            if (structTrigger.Check > 0)
                                            {
                                                trigger = new Trigger(structTrigger.Check, structTrigger.Item, structTrigger.Threshold, structTrigger.Calc);
                                                //reset to default value
                                                structTrigger.Check = TriggerCheck.None;
                                            }
                                            else
                                            {
                                                //otherwise create list with a single blank Trigger (effect = none)
                                                trigger = new Trigger();
                                            }
                                            //create list with a single blank Trigger (effect = none)
                                            listSubTriggers.Add(trigger);
                                            List<Trigger> tempTriggers = new List<Trigger>(listSubTriggers);
                                            listAllTriggers.Add(tempTriggers);
                                        }
                                        //zero out listSubTriggers
                                        listSubTriggers.Clear();
                                    }
                                    listOptions.Add(structOption);
                                    //structEvent.Options.Add(structOption);
                                }
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Event eventObject = null;
                                    switch (structEvent.Type)
                                    {
                                        case ArcType.GeoCluster:
                                            eventObject = new EventPlyGeo(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Geo);
                                            break;
                                        case ArcType.Location:
                                            eventObject = new EventPlyLoc(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Loc);
                                            break;
                                        case ArcType.Road:
                                            eventObject = new EventPlyRoad(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Road);
                                            break;
                                        case ArcType.House:
                                            eventObject = new EventPlyHouse(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.House);
                                            break;
                                        case ArcType.Actor:
                                            eventObject = new EventFolActor(structEvent.EventID, structEvent.Name, structEvent.Frequency, structEvent.Actor);
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid ArcType for Object (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    if (eventObject != null)
                                    {
                                        EventPlayer eventTemp = eventObject as EventPlayer;
                                        eventTemp.Text = structEvent.EventText;
                                        eventTemp.Category = structEvent.Cat;
                                        eventTemp.Status = EventStatus.Active;
                                        //add options
                                        if (listOptions.Count > 1)
                                        {
                                            for (int index = 0; index < listOptions.Count; index++)
                                            {
                                                OptionStruct optionTemp = listOptions[index];
                                                //at least one outcome must be present
                                                if (listAllOutcomes.Count > 0)
                                                {
                                                    OptionInteractive optionObject = new OptionInteractive(optionTemp.Text) { ReplyGood = optionTemp.Reply};

                                                    //Triggers (optional)
                                                    List<Trigger> tempTriggers = listAllTriggers[index];
                                                    Trigger trigger = tempTriggers[0];
                                                    //if first record has a default value of None then it's a blank Trigger put there as a placeholder
                                                    if (trigger.Check != TriggerCheck.None)
                                                    { optionObject.SetTriggers(tempTriggers); }

                                                    //Outcomes
                                                    List <OutcomeStruct> sublist = listAllOutcomes[index];
                                                    //create appropriate outcome object
                                                    for (int inner = 0; inner < sublist.Count; inner++)
                                                    {
                                                        OutcomeStruct outTemp = sublist[inner];
                                                        Outcome outObject = null;
                                                        //add appropriate outcome object to option object
                                                        switch (outTemp.Effect)
                                                        {
                                                            case "Conflict":
                                                                outObject = new OutConflict(structEvent.EventID, outTemp.Data, outTemp.Amount, outTemp.Apply);
                                                                break;
                                                            case "Game":
                                                                outObject = new OutGame(structEvent.EventID, outTemp.Data, outTemp.Amount, outTemp.Apply);
                                                                break;
                                                            case "Event":
                                                                outObject = new OutEvent(structEvent.EventID, outTemp.Data, outTemp.Amount, outTemp.Apply);
                                                                break;
                                                            default:
                                                                Game.SetError(new Error(49, string.Format("Invalid Outcome Effect for Event (\"{0}\")", structEvent.Name)));
                                                                validData = false;
                                                                break;
                                                        }
                                                        //add Outcome object to Option object
                                                        if (outObject != null)
                                                        { optionObject.SetGoodOutcome(outObject); }
                                                    }
                                                    //add option object to event object
                                                    eventTemp.SetOption(optionObject);
                                                }
                                                else { Game.SetError(new Error(49, string.Format("{0} has no Outcome for Option {1}", structEvent.Name, index + 1))); validData = false; }
                                            }
                                        }
                                        else { Game.SetError(new Error(49, string.Format("{0} has insufficient Options", structEvent.Name))); validData = false; }

                                        //last datapoint - save object to list
                                        if (dataCounter > 0 && validData == true)
                                        { dictOfPlayerEvents.Add(eventTemp.EventPID, eventTemp); }
                                    }
                                    else { Game.SetError(new Error(49, "Invalid Input, eventObject")); }
                                }
                                else
                                { Game.SetError(new Error(49, string.Format("Event, (\"{0}\" EventID {1}), not Imported", structEvent.Name, structEvent.EventID))); }
                                break;
                            default:
                                Game.SetError(new Error(49, "Invalid Input, CleanTag"));
                                break;
                        }
                    }
                }
                else { newEvent = false; }
            }
            return dictOfPlayerEvents;
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
            List<int> listArcID = new List<int>(); //used to pick up duplicate arcID's
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
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    { Game.SetError(new Error(53, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structArc.Name = cleanToken;
                                break;
                            case "ArcID":
                                try
                                { structArc.ArcID = Convert.ToInt32(cleanToken); }
                                catch
                                { Game.SetError(new Error(53, string.Format("Invalid input for ArcID {0}, (\"{1}\")", cleanToken, structArc.Name))); validData = false; }
                                //check for duplicate arcID's
                                if (listArcID.Contains(structArc.ArcID))
                                { Game.SetError(new Error(53, string.Format("Duplicate ArcID {0}, (\"{1}\")", cleanToken, structArc.Name))); validData = false; }
                                else { listArcID.Add(structArc.ArcID); }
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
                                    case "House":
                                        structArc.Type = ArcType.House;
                                        break;
                                    case "Actor":
                                        structArc.Type = ArcType.Actor;
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
                                    case ArcType.House:
                                        switch (cleanToken)
                                        {
                                            case "Major":
                                                structArc.House = ArcHouse.Major;
                                                break;
                                            case "Minor":
                                                structArc.House = ArcHouse.Minor;
                                                break;
                                            case "Inn":
                                                structArc.House = ArcHouse.Inn;
                                                break;
                                            default:
                                                Game.SetError(new Error(53, string.Format("Invalid Input, House Type, (\"{0}\")", arrayOfArchetypes[i])));
                                                validData = false;
                                                break;
                                        }
                                        break;
                                    case ArcType.Actor:
                                        switch (cleanToken)
                                        {
                                            case "Player":
                                                structArc.Actor = ArcActor.Player;
                                                break;
                                            case "Follower":
                                                structArc.Actor = ArcActor.Follower;
                                                break;
                                            default:
                                                Game.SetError(new Error(53, string.Format("Invalid Input, Actor Type, (\"{0}\")", arrayOfArchetypes[i])));
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
                            case "Chance":
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structArc.Chance = dataInt; }
                                    else
                                    { Game.SetError(new Error(53, string.Format("Invalid Chance \"{0}\" (Zero) for {1}", dataInt, structArc.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(53, string.Format("Invalid Chance (Conversion) for  {0}", structArc.Name))); validData = false; }
                                break;
                            case "Events":
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
                                            {
                                                //check a valid event
                                                if (Game.director.CheckEvent(dataInt))
                                                { tempList.Add(dataInt); }
                                                else
                                                { Game.SetError(new Error(53, string.Format("Invalid EventID \"{0}\" (Not found in Dictionary) for {1}", dataInt, structArc.Name))); validData = false; }
                                            }
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
                            
                            case "end":
                            case "End":
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Archetype arcObject = null;
                                    switch (structArc.Type)
                                    {
                                        case ArcType.GeoCluster:
                                            arcObject = new ArcTypeGeo(structArc.Name, structArc.Geo, structArc.ArcID, structArc.Chance, structArc.listOfEvents);
                                            break;
                                        case ArcType.Location:
                                            arcObject = new ArcTypeLoc(structArc.Name, structArc.Loc, structArc.ArcID, structArc.Chance, structArc.listOfEvents);
                                            break;
                                        case ArcType.Road:
                                            arcObject = new ArcTypeRoad(structArc.Name, structArc.Road, structArc.ArcID, structArc.Chance, structArc.listOfEvents);
                                            break;
                                        case ArcType.House:
                                            arcObject = new ArcTypeHouse(structArc.Name, structArc.House, structArc.ArcID, structArc.Chance, structArc.listOfEvents);
                                            break;
                                        case ArcType.Actor:
                                            arcObject = new ArcTypeActor(structArc.Name, structArc.Actor, structArc.ArcID, structArc.Chance, structArc.listOfEvents);
                                            break;
                                    }
                                    if (arcObject != null)
                                    {
                                        Archetype arcTemp = arcObject as Archetype;
                                        arcTemp.Type = arcObject.Type;
                                        //last datapoint - save object to list
                                        if (dataCounter > 0)
                                        { dictOfArchetypes.Add(arcTemp.ArcID, arcTemp); }
                                    }
                                    else { Game.SetError(new Error(53, "Invalid Input, arcObject")); }
                                }
                                else
                                { Game.SetError(new Error(53, string.Format("Archetype, (\"{0}\" ArcID {1}), not Imported", structArc.Name, structArc.ArcID))); }
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
        /// Import Stories
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, Story> GetStories(string fileName)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newStory = false;
            bool validData = true;
            int dataInt;
            List<int> listStoryID = new List<int>(); //used to pick up duplicate storyID's
            Dictionary<int, Story> dictOfStories = new Dictionary<int, Story>();
            string[] arrayOfStories = ImportDataFile(fileName);
            StoryStruct structStory = new StoryStruct();
            //loop imported array of strings
            for (int i = 0; i < arrayOfStories.Length; i++)
            {
                if (arrayOfStories[i] != "" && !arrayOfStories[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newStory == false)
                    {
                        newStory = true;
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new Trait object
                        structStory = new StoryStruct();
                    }

                    string[] tokens = arrayOfStories[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }

                    switch (cleanTag)
                    {
                        case "Name":
                            if (cleanToken.Length == 0)
                            { Game.SetError(new Error(54, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            else { structStory.Name = cleanToken; }
                            break;
                        case "StoryID":
                            try
                            { structStory.StoryID = Convert.ToInt32(cleanToken); }
                            catch
                            { Game.SetError(new Error(54, string.Format("Invalid input for StoryID {0}, (\"{1}\")", cleanToken, structStory.Name))); validData = false; }
                            //check for duplicate arcID's
                            if (listStoryID.Contains(structStory.StoryID))
                            { Game.SetError(new Error(54, string.Format("Duplicate StoryID {0}, (\"{1}\")", cleanToken, structStory.Name))); validData = false; }
                            else { listStoryID.Add(structStory.StoryID); }
                            break;
                        case "Type":
                            if (cleanToken.Length == 0)
                            { Game.SetError(new Error(54, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            else
                            {
                                switch (cleanToken)
                                {
                                    case "Benevolent":
                                        structStory.Type = StoryAI.Benevolent;
                                        break;
                                    case "Balanced":
                                        structStory.Type = StoryAI.Balanced;
                                        break;
                                    case "Evil":
                                        structStory.Type = StoryAI.Evil;
                                        break;
                                    case "Tricky":
                                        structStory.Type = StoryAI.Tricky;
                                        break;
                                    default:
                                        structStory.Type = StoryAI.None;
                                        Game.SetError(new Error(54, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfStories[i])));
                                        validData = false;
                                        break;
                                }
                            }
                            break;
                        case "Ev_Follower_Loc":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Ev_Follower_Loc = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Ev_Follower_Loc \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Ev_Follower_Loc (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            else
                            { Game.SetError(new Error(54, string.Format("Empty data field (Ev_Follower_Loc), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            break;
                        case "Ev_Follower_Trav":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Ev_Follower_Trav = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Ev_Follower_Trav \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Ev_Follower_Trav (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            else
                            { Game.SetError(new Error(54, string.Format("Empty data field (Ev_Follower_Trav), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            break;
                        case "Ev_Player_Loc":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Ev_Player_Loc = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Ev_Player_Loc \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Ev_Player_Loc (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            else
                            { Game.SetError(new Error(54, string.Format("Empty data field (Ev_Player_Loc), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            break;
                        case "Ev_Player_Trav":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Ev_Player_Trav = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Ev_Player_Trav \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Ev_Player_Trav (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            else
                            { Game.SetError(new Error(54, string.Format("Empty data field (Ev_Player_Trav), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                            break;
                        case "Arc_Geo_Sea":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Sea = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Sea \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Sea (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Geo_Mountain":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Mountain = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Mountain \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Mountain (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Geo_Forest":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Forest = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Forest \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Forest (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Loc_Capital":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Capital = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Capital \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Capital (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Loc_Major":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Major = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Major \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Major (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Loc_Minor":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Minor = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Minor \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Minor (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Loc_Inn":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Inn = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Inn \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Inn (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Road_Normal":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Normal = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Road Normal \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Road Normal (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Road_Kings":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Kings = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Kings Road \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Kings Road (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "Arc_Road_Connector":
                            if (cleanToken.Length > 0)
                            {
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0)
                                    { structStory.Connector = dataInt; }
                                    else
                                    { Game.SetError(new Error(54, string.Format("Invalid Connector Road \"{0}\" (Zero) for {1}", dataInt, structStory.Name))); validData = false; }
                                }
                                catch
                                { Game.SetError(new Error(54, string.Format("Invalid Connector Road (Conversion) for  {0}", structStory.Name))); validData = false; }
                            }
                            break;
                        case "end":
                        case "End":
                            //write record
                            if (validData == true)
                            {
                                //pass info over to a class instance
                                Story storyObject = new Story(structStory.Name, structStory.StoryID, structStory.Type);
                                storyObject.Ev_Follower_Loc = structStory.Ev_Follower_Loc;
                                storyObject.Ev_Follower_Trav = structStory.Ev_Follower_Trav;
                                storyObject.Ev_Player_Loc = structStory.Ev_Player_Loc;
                                storyObject.Ev_Player_Trav = structStory.Ev_Player_Trav;
                                storyObject.Arc_Geo_Sea = structStory.Sea;
                                storyObject.Arc_Geo_Mountain = structStory.Mountain;
                                storyObject.Arc_Geo_Forest = structStory.Forest;
                                storyObject.Arc_Loc_Capital = structStory.Capital;
                                storyObject.Arc_Loc_Major = structStory.Major;
                                storyObject.Arc_Loc_Minor = structStory.Minor;
                                storyObject.Arc_Loc_Inn = structStory.Inn;
                                storyObject.Arc_Road_Normal = structStory.Normal;
                                storyObject.Arc_Road_Kings = structStory.Kings;
                                storyObject.Arc_Road_Connector = structStory.Connector;
                                //last datapoint - save object to list
                                if (dataCounter > 0)
                                { dictOfStories.Add(storyObject.StoryID, storyObject); }
                                else { Game.SetError(new Error(54, "Invalid Input, storyObject")); }
                            }
                            else
                            { Game.SetError(new Error(54, string.Format("Story, (\"{0}\" StoryID {1}), not Imported", structStory.Name, structStory.StoryID))); }
                            break;
                        default:
                            Game.SetError(new Error(54, string.Format("Invalid Input, CleanTag \"{0}\", \"{1}\"", cleanTag, structStory.Name)));
                            break;
                    }

                }
                else { newStory = false; }
            }
            return dictOfStories;
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

        /// <summary>
        /// Import Followers
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal List<FollowerStruct> GetFollowers(string fileName)
        {
            string[] arrayOfFollowers = ImportDataFile(fileName);
            List <FollowerStruct> listOfStructs = new List<FollowerStruct>();
            bool newFollower = false;
            bool validData = true;
            int dataCounter = 0; //number of followers
            FollowerStruct structFollower = new FollowerStruct();
            string cleanToken;
            string cleanTag;
            for (int i = 0; i < arrayOfFollowers.Length; i++)
            {
                if (arrayOfFollowers[i] != "" && !arrayOfFollowers[i].StartsWith("#"))
                {
                    //set up for a new house
                    if (newFollower == false)
                    {
                        newFollower = true;
                        validData = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new structure
                        structFollower = new FollowerStruct();
                    }
                    string[] tokens = arrayOfFollowers[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                    else { cleanToken = tokens[1].Trim(); }
                    if (cleanToken.Length == 0)
                    {
                        validData = false;
                        Game.SetError(new Error(59, string.Format("Follower {0} (Missing data for \"{1}\") \"{2}\"",
                        String.IsNullOrEmpty(structFollower.Name) ? "?" : structFollower.Name, cleanTag, fileName)));
                    }
                    else
                    {
                        switch (cleanTag)
                        {
                            case "Name":
                                structFollower.Name = cleanToken;
                                break;
                            case "FID":
                                try { structFollower.FID = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Sex":
                                switch (cleanToken)
                                {
                                    case "Male":
                                        structFollower.Sex = ActorSex.Male;
                                        break;
                                    case "Female":
                                        structFollower.Sex = ActorSex.Female;
                                        break;
                                    default:
                                        structFollower.Sex = ActorSex.None;
                                        Game.SetError(new Error(59, string.Format("Invalid Input, Sex (\"{0}\")", arrayOfFollowers[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "Role":
                                structFollower.Role = cleanToken;
                                break;
                            case "Description":
                                structFollower.Description = cleanToken;
                                break;
                            case "Age":
                                try { structFollower.Age = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Special":
                                //NOTE: not yet sure of what special field will represent
                                structFollower.Special = cleanToken;
                                break;
                            case "ArcID":
                                try { structFollower.ArcID = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Resources":
                                try { structFollower.Resources = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Loyalty":
                                try { structFollower.Loyalty = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Combat_Effect":
                                try { structFollower.Combat_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Wits_Effect":
                                try { structFollower.Wits_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Charm_Effect":
                                try { structFollower.Charm_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Treachery_Effect":
                                try { structFollower.Treachery_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Leadership_Effect":
                                try { structFollower.Leadership_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Touched_Effect":
                                try { structFollower.Touched_Effect = Convert.ToInt32(cleanToken); }
                                catch (Exception e)
                                { Game.SetError(new Error(59, e.Message)); validData = false; }
                                break;
                            case "Combat_Trait":
                                structFollower.Combat_Trait = cleanToken;
                                break;
                            case "Wits_Trait":
                                structFollower.Wits_Trait = cleanToken;
                                break;
                            case "Charm_Trait":
                                structFollower.Charm_Trait = cleanToken;
                                break;
                            case "Treachery_Trait":
                                structFollower.Treachery_Trait = cleanToken;
                                break;
                            case "Leadership_Trait":
                                structFollower.Leadership_Trait = cleanToken;
                                break;
                            case "Touched_Trait":
                                structFollower.Touched_Trait = cleanToken;
                                break;
                            case "end":
                            case "End":
                                //last datapoint - save structure to list
                                if (dataCounter > 0 && validData == true)
                                { listOfStructs.Add(structFollower); }
                                break;
                            default:
                                Game.SetError(new Error(59, string.Format("Invalid Data \"{0}\" in Follower Input", cleanTag)));
                                break;
                        }
                    }
                }
                else
                { newFollower = false; }
            }
            return listOfStructs;
        }

        //methods above here
    }
}
