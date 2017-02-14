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
        public int Castle { get; set; }
    }

    //Traits
    struct TraitStruct
    {
        public string Name { get; set; }
        public SkillType Type { get; set; }
        public SkillSex Sex { get; set; }
        public SkillAge Age { get; set; }
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
        public SkillType Trait { get; set; }
        public int Delay { get; set; }
    }

    //Player Events
    struct EventPlayerStruct
    {
        public string Name { get; set; }
        public string EventText { get; set; }
        public int EventID { get; set; }
        public int Repeat { get; set; }
        public int Dormant { get; set; }
        public int Live { get; set; }
        public int Cool { get; set; }
        public int SubRef { get; set; }
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
        public int Test { get; set; }
        public string Reply { get; set; }
        public string ReplyBad { get; set; }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="option"></param>
        public OptionStruct(OptionStruct option)
        {
            Text = option.Text;
            Test = option.Test;
            Reply = option.Reply;
            ReplyBad = option.ReplyBad;
        }
    }

    struct OutcomeStruct
    {
        //NOTE: Update copy constructor with new Data & case '[option]' x 2 below

        public string Effect { get; set; }
        public int Data { get; set; }
        public int Amount { get; set; }
        public int Bad { get; set; } //if > 0, flags outcome as Bad
        public bool PlayerRes { get; set; } //if true then Player has Resources changed, otherwise opponent (OutResources only)
        public EventCalc Calc { get; set; }
        public EventStatus NewStatus { get; set; } //specific to EventStatus outcomes
        public EventTimer Timer { get; set; } //specific to EventTimer outcomes
        public EventFilter Filter { get; set; } //which group of people to focus on?
        //Conflict Outcomes
        public bool Challenger { get; set; } //is the player the challenger?
        public ConflictType Conflict_Type { get; set; }
        public ConflictCombat Combat_Type { get; set; }
        public ConflictSocial Social_Type { get; set; }
        public ConflictStealth Stealth_Type { get; set; }
        public ConflictSubType SubType { get; set; } //descriptive purposes only
        //Condition Outcomes
        public bool PlayerCondition { get; set; } //true if condition applies to Player, otherwise other actor
        public string ConditionText { get; set; } // "Old Age", for example
        public SkillType ConditionSkill { get; set; } //Combat, Charm, Wits etc.
        public int ConditionEffect { get; set; } //+/- 1 or 2
        public int ConditionTimer { get; set; } //set to 999 for a permanent condition, otherwise equal to the number of days that condition applies for

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="outcome"></param>
        public OutcomeStruct(OutcomeStruct outcome)
        {
            Effect = outcome.Effect;
            Data = outcome.Data;
            Amount = outcome.Amount;
            Bad = outcome.Bad;
            Calc = outcome.Calc;
            NewStatus = outcome.NewStatus;
            Timer = outcome.Timer;
            Filter = outcome.Filter;
            PlayerRes = outcome.PlayerRes;
            //conflict Outcomes
            Challenger = outcome.Challenger;
            Conflict_Type = outcome.Conflict_Type;
            Combat_Type = outcome.Combat_Type;
            Social_Type = outcome.Social_Type;
            Stealth_Type = outcome.Stealth_Type;
            SubType = outcome.SubType;
            PlayerCondition = outcome.PlayerCondition;
            ConditionText = outcome.ConditionText;
            ConditionSkill = outcome.ConditionSkill;
            ConditionEffect = outcome.ConditionEffect;
            ConditionTimer = outcome.ConditionTimer;
        }
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
        public int Loyalty { get; set; } //loyalty to the player (0 to 100) -> same as relationship to player
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

    //situations
    struct SituationStruct
    {
        public string Name { get; set; }
        public ConflictType Type { get; set; }
        public ConflictState State { get; set; }
        public ConflictSpecial Special { get; set; }
        public ConflictCombat Type_Combat { get; set; }
        public ConflictSocial Type_Social { get; set; }
        public ConflictStealth Type_Stealth { get; set; }
        public int SitNum { get; set; }
        public int Defender { get; set; }
        public int Data { get; set; }
        public List<string> ListGood { get; set; }
        public List<string> ListBad { get; set; }
    }

    //challenges
    struct ChallengeStruct
    {
        public ConflictType Type { get; set; }
        public ConflictCombat CombatType { get; set; }
        public ConflictSocial SocialType { get; set; }
        public ConflictStealth StealthType { get; set; }
    }

    //results
    struct ResultStruct
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int ResultID { get; set; }
        public ResultType Type { get; set; }
        public DataPoint DataPoint { get; set; }
        public int Data { get; set; }
        public int Test { get; set; }
        public EventCalc Calc { get; set;}
        public int Amount { get; set; }
        public bool ConPlayer { get; set; } //optional -> Conditions
        public string ConText { get; set; } //optional -> Conditions
        public SkillType ConSkill { get; set; } //optional -> Conditions
        public int ConEffect { get; set; } //optional -> Conditions
        public int ConTimer { get; set; } //optional -> Conditions
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
        /// Standard text file importer, returns an array of data, null if not found
        /// </summary>
        /// <param name="name">"filename.txt"</param>
        /// <returns></returns>
        private string[] ImportDataFile(string fileName)
        {
            string[] importedText = null;
            string path = fileDirectory + fileName;
            if (File.Exists(path))
            {
                importedText = File.ReadAllLines(path);
                if (importedText.Length == 0)
                { Game.SetError(new Error(14, string.Format("No data in file {0}", fileName))); }
                else { return importedText; }
            }
            else
            { Game.SetError(new Error(10, string.Format("FileImport failed, file name {0}", fileName))); }
            return null;
            
        }

        /// <summary>
        /// handles simple string lists
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<string> GetStrings(string fileName)
        {
            List<string> listOfStrings = new List<string>();
            // read in lists of strings
            string[] arrayOfStrings = ImportDataFile(fileName);
            if (arrayOfStrings != null)
            {
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
            }
            else
            { Game.SetError(new Error(116, string.Format("File not found (\"{0}\")", fileName))); }
            return listOfStrings;
        }

        /// <summary>
        /// handles major and minor houses and returns a list of house Structs
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<HouseStruct> GetHouses(string fileName)
        {
            int tempNum;
            List<HouseStruct> listHouses = new List<HouseStruct>();
            string[] arrayOfHouseNames = ImportDataFile(fileName);
            if (arrayOfHouseNames != null)
            {
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
                        //string[] tokens = arrayOfHouseNames[i].Split(':');
                        string[] tokens = arrayOfHouseNames[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(16, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
                        if (cleanToken.Length == 0)
                        {
                            validData = false;
                            Game.SetError(new Error(16, string.Format("House {0} (Missing data for \"{1}\") \"{2}\"",
                            String.IsNullOrEmpty(houseStruct.Name) ? "?" : houseStruct.Name, cleanTag, fileName)));
                        }
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
                                case "Castle":
                                    try
                                    {
                                        tempNum = Convert.ToInt32(cleanToken);
                                        //value must be within 0 to 5 range
                                        if (tempNum >= 0 && tempNum <= 5)
                                        { houseStruct.Castle = tempNum; }
                                        else { Game.SetError(new Error(18, "Invalid Castle Input (out of range)")); }
                                    }
                                    catch (Exception e)
                                    { Game.SetError(new Error(18, e.Message)); validData = false; }
                                    break;
                                case "Capital": //Major Houses
                                    houseStruct.Capital = cleanToken;
                                    break;
                                case "Seat": //Minor Houses
                                    houseStruct.Capital = cleanToken;
                                    break;
                                case "[end]":
                                case "[End]":
                                    //last datapoint - save structure to list
                                    if (dataCounter > 0 && validData == true)
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
            }
            else
            { Game.SetError(new Error(15, string.Format("File not found (\"{0}\")", fileName))); }
            return listHouses;
        }

        /// <summary>
        /// read in an initialise Constants
        /// </summary>
        /// <param name="fileName"></param>
        public void GetConstants(string fileName)
        {
            string[] arrayOfFileInput = ImportDataFile(fileName);
            if (arrayOfFileInput != null)
            {
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
                        //string[] tokens = arrayOfFileInput[i].Split(':');
                        string[] tokens = arrayOfFileInput[i].Split(new Char[] { ':', ';' });
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
            else
            { Game.SetError(new Error(17, string.Format("File not found (\"{0}\")", fileName))); }
        }

        /// <summary>
        /// get traits from all a trait file, return a list of traits
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        internal List<Skill> GetTraits(string fileName, SkillSex sex)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newTrait = false;
            bool validData = true;
            List<Skill> listOfTraits = new List<Skill>();
            List<string> listOfNickNames = new List<string>();
            string[] arrayOfTraits = ImportDataFile(fileName);
            if (arrayOfTraits != null)
            {
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
                        //string[] tokens = arrayOfTraits[i].Split(':');
                        string[] tokens = arrayOfTraits[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(20, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
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
                                            structTrait.Type = SkillType.Combat;
                                            break;
                                        case "Wits":
                                            structTrait.Type = SkillType.Wits;
                                            break;
                                        case "Charm":
                                            structTrait.Type = SkillType.Charm;
                                            break;
                                        case "Treachery":
                                            structTrait.Type = SkillType.Treachery;
                                            break;
                                        case "Leadership":
                                            structTrait.Type = SkillType.Leadership;
                                            break;
                                        case "Touched":
                                            structTrait.Type = SkillType.Touched;
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
                                        { structTrait.Age = SkillAge.Five; }
                                        else
                                        { structTrait.Age = SkillAge.Fifteen; }
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
                                case "[end]":
                                case "[End]":
                                    if (validData == true)
                                    {
                                        //pass info over to a class instance
                                        Skill classTrait = new Skill(structTrait.Name, structTrait.Type, structTrait.Effect, structTrait.Sex, structTrait.Age,
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
            }
            else
            { Game.SetError(new Error(22, string.Format("File not found (\"{0}\")", fileName))); }
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
            if (arrayOfEvents != null)
            {
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
                        //string[] tokens = arrayOfEvents[i].Split(':');
                        string[] tokens = arrayOfEvents[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(49, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
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
                                            structEvent.Trait = SkillType.Combat;
                                            break;
                                        case "Wits":
                                            structEvent.Trait = SkillType.Wits;
                                            break;
                                        case "Charm":
                                            structEvent.Trait = SkillType.Charm;
                                            break;
                                        case "Treachery":
                                            structEvent.Trait = SkillType.Treachery;
                                            break;
                                        case "Leadership":
                                            structEvent.Trait = SkillType.Leadership;
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
                                case "[end]":
                                case "[End]":
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
            }
            else
            { Game.SetError(new Error(49, string.Format("File not found (\"{0}\")", fileName))); }
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
            if (arrayOfEvents != null)
            {
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
                        //string[] tokens = arrayOfEvents[i].Split(':');
                        string[] tokens = arrayOfEvents[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        //if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(20, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
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
                                        {
                                            OutcomeStruct structOutcomeCopy = new OutcomeStruct(structOutcome);
                                            listSubOutcomes.Add(structOutcomeCopy);
                                            //zero out data as the same structure is reused
                                            structOutcome.Effect = "";
                                            structOutcome.Data = 0;
                                            structOutcome.Amount = 0;
                                            structOutcome.Bad = 0;
                                            structOutcome.Calc = EventCalc.None;
                                            structOutcome.NewStatus = EventStatus.None;
                                            structOutcome.Timer = EventTimer.None;
                                            structOutcome.PlayerRes = false;
                                            structOutcome.PlayerCondition = true;
                                            structOutcome.ConditionText = "";
                                            structOutcome.ConditionSkill = SkillType.None;
                                            structOutcome.ConditionEffect = 0;
                                            structOutcome.ConditionTimer = 0;
                                        }
                                        listAllOutcomes.Add(listSubOutcomes);
                                        OptionStruct structOptionCopy = new OptionStruct(structOption);
                                        listOptions.Add(structOptionCopy);
                                        //zero out optional data as structOption reused
                                        structOption.Test = 0;
                                        structOption.ReplyBad = "";
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
                                    {
                                        OutcomeStruct structOutcomeCopy = new OutcomeStruct(structOutcome);
                                        listSubOutcomes.Add(structOutcomeCopy);
                                        //zero out data as the same structure is reused
                                        structOutcome.Effect = "";
                                        structOutcome.Data = 0;
                                        structOutcome.Amount = 0;
                                        structOutcome.Bad = 0;
                                        structOutcome.Calc = EventCalc.None;
                                        structOutcome.NewStatus = EventStatus.None;
                                        structOutcome.Timer = EventTimer.None;
                                        structOutcome.PlayerRes = false;
                                        structOutcome.PlayerCondition = true;
                                        structOutcome.ConditionText = "";
                                        structOutcome.ConditionSkill = SkillType.None;
                                        structOutcome.ConditionEffect = 0;
                                        structOutcome.ConditionTimer = 0;
                                    }
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
                                        case "geocluster":
                                        case "geoCluster":
                                        case "GeoCluster":
                                            structEvent.Type = ArcType.GeoCluster;
                                            break;
                                        case "location":
                                        case "Location":
                                            structEvent.Type = ArcType.Location;
                                            break;
                                        case "road":
                                        case "Road":
                                            structEvent.Type = ArcType.Road;
                                            break;
                                        case "actor":
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
                                                case "sea":
                                                case "Sea":
                                                    structEvent.Geo = ArcGeo.Sea;
                                                    break;
                                                case "mountain":
                                                case "Mountain":
                                                    structEvent.Geo = ArcGeo.Mountain;
                                                    break;
                                                case "forest":
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
                                                case "capital":
                                                case "Capital":
                                                    structEvent.Loc = ArcLoc.Capital;
                                                    break;
                                                case "major":
                                                case "Major":
                                                    structEvent.Loc = ArcLoc.Major;
                                                    break;
                                                case "minor":
                                                case "Minor":
                                                    structEvent.Loc = ArcLoc.Minor;
                                                    break;
                                                case "inn":
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
                                                case "normal":
                                                case "Normal":
                                                    structEvent.Road = ArcRoad.Normal;
                                                    break;
                                                case "kings":
                                                case "Kings":
                                                    structEvent.Road = ArcRoad.Kings;
                                                    break;
                                                case "connector":
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
                                                case "major":
                                                case "Major":
                                                    structEvent.House = ArcHouse.Major;
                                                    break;
                                                case "minor":
                                                case "Minor":
                                                    structEvent.House = ArcHouse.Minor;
                                                    break;
                                                case "inn":
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
                                                case "player":
                                                case "Player":
                                                    structEvent.Actor = ArcActor.Player;
                                                    break;
                                                case "follower":
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
                                case "subRef":
                                    try
                                    { structEvent.SubRef = Convert.ToInt32(cleanToken); }
                                    catch
                                    { Game.SetError(new Error(49, string.Format("Invalid input for subRef {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                    break;
                                case "Cat":
                                    switch (cleanToken)
                                    {
                                        case "generic":
                                        case "Generic":
                                            structEvent.Cat = EventCategory.Generic;
                                            break;
                                        case "special":
                                        case "Special":
                                            structEvent.Cat = EventCategory.Special;
                                            break;
                                        case "autoreact":
                                        case "autoReact":
                                        case "AutoReact":
                                            structEvent.Cat = EventCategory.AutoReact;
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
                                        case "very_rare":
                                        case "very_Rare":
                                        case "Very_Rare":
                                            structEvent.Frequency = EventFrequency.Very_Rare;
                                            break;
                                        case "rare":
                                        case "Rare":
                                            structEvent.Frequency = EventFrequency.Rare;
                                            break;
                                        case "low":
                                        case "Low":
                                            structEvent.Frequency = EventFrequency.Low;
                                            break;
                                        case "normal":
                                        case "Normal":
                                            structEvent.Frequency = EventFrequency.Normal;
                                            break;
                                        case "common":
                                        case "Common":
                                            structEvent.Frequency = EventFrequency.Common;
                                            break;
                                        case "high":
                                        case "High":
                                            structEvent.Frequency = EventFrequency.High;
                                            break;
                                        case "very_high":
                                        case "very_High":
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
                                        case "active":
                                        case "Active":
                                            structEvent.Status = EventStatus.Active;
                                            break;
                                        case "live":
                                        case "Live":
                                            structEvent.Status = EventStatus.Live;
                                            break;
                                        case "dormant":
                                        case "Dormant":
                                            structEvent.Status = EventStatus.Dormant;
                                            break;
                                        case "dead":
                                        case "Dead":
                                            structEvent.Status = EventStatus.Dead;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Status, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "Repeat":
                                    //Repeat Timer (number of activations before active event goes dormant)
                                    try
                                    {
                                        structEvent.Repeat = Convert.ToInt32(cleanToken);
                                        //can't be 0 or less
                                        if (structEvent.Repeat <= 0)
                                        {
                                            //give default value (constructor -> 1000)
                                            structEvent.Repeat = 0;
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Repeat (timer), (value <= 0, set to default value instead) \"{0}\"", arrayOfEvents[i])));
                                        }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Repeat (timer), (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "Dormant":
                                    //Dormant Timer (# turns before Active event goes dormant)
                                    try
                                    {
                                        structEvent.Dormant = Convert.ToInt32(cleanToken);
                                        //can't be 0 or less
                                        if (structEvent.Dormant < 0)
                                        {
                                            //give default value (constructor -> 1000)
                                            structEvent.Dormant = 0;
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Dormant (timer), (value < 0, set to default value instead) \"{0}\"", arrayOfEvents[i])));
                                        }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Dormant (timer), (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "Live":
                                    //Live Timer (# turns before Live event becomes Active)
                                    try
                                    {
                                        structEvent.Live = Convert.ToInt32(cleanToken);
                                        //can't be 0 or less
                                        if (structEvent.Live < 0)
                                        {
                                            //give default value (constructor -> 1000)
                                            structEvent.Live = 0;
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Live (timer), (value < 0, set to default value instead) \"{0}\"", arrayOfEvents[i])));
                                        }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Live (timer), (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "Cool":
                                    //Cool down Timer (# turns before Live event becomes Active) -> Base value
                                    try
                                    {
                                        structEvent.Cool = Convert.ToInt32(cleanToken);
                                        //can't be 0 or less
                                        if (structEvent.Cool < 0)
                                        {
                                            //give default value (constructor -> 1000)
                                            structEvent.Cool = 0;
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Cool down (timer), (value < 0, set to default value instead) \"{0}\"", arrayOfEvents[i])));
                                        }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Cool down (timer), (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "text":
                                    //Option text
                                    structOption.Text = cleanToken;
                                    break;
                                case "reply":
                                    //Option reply (default good outcome)
                                    structOption.Reply = cleanToken;
                                    break;
                                case "replyBad":
                                    //Option (variable) reply for a bad outcome if test failed
                                    structOption.ReplyBad = cleanToken;
                                    break;
                                case "test":
                                    try
                                    { structOption.Test = Convert.ToInt32(cleanToken); }
                                    catch
                                    { Game.SetError(new Error(49, string.Format("Invalid input for option Test {0}, (\"{1}\")", cleanToken, structEvent.Name))); validData = false; }
                                    break;
                                case "check":
                                    //Trigger Check
                                    switch (cleanToken)
                                    {
                                        case "trait":
                                        case "Trait":
                                            structTrigger.Check = TriggerCheck.Trait;
                                            break;
                                        case "gamevar":
                                        case "gameVar":
                                        case "GameVar":
                                            structTrigger.Check = TriggerCheck.GameVar;
                                            break;
                                        case "sex":
                                        case "Sex":
                                            structTrigger.Check = TriggerCheck.Sex;
                                            break;
                                        case "resourceplyr":
                                        case "resourcePlyr":
                                        case "ResourcePlyr":
                                            structTrigger.Check = TriggerCheck.ResourcePlyr;
                                            break;
                                        case "actortype":
                                        case "actorType":
                                        case "ActorType":
                                            structTrigger.Check = TriggerCheck.ActorType;
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
                                case "bad":
                                    //if > 0 then flagged as a Bad outcome
                                    try
                                    {
                                        dataInt = Convert.ToInt32(cleanToken);
                                        structOutcome.Bad = dataInt;
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Bad, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "effect":
                                    //Outcome effect
                                    switch (cleanToken)
                                    {
                                        case "conflict":
                                        case "Conflict":
                                        case "game":
                                        case "Game":
                                        case "eventtimer":
                                        case "eventTimer":
                                        case "EventTimer":
                                        case "eventstatus":
                                        case "eventStatus":
                                        case "EventStatus":
                                        case "eventchain":
                                        case "eventChain":
                                        case "EventChain":
                                        case "resource":
                                        case "Resource":
                                        case "Condition":
                                        case "condition":
                                        case "none":
                                        case "None":
                                            structOutcome.Effect = cleanToken;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Effect, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "data":
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
                                        case "none":
                                        case "None":
                                            structOutcome.Calc = EventCalc.None;
                                            break;
                                        case "add":
                                        case "Add":
                                            structOutcome.Calc = EventCalc.Add;
                                            break;
                                        case "subtract":
                                        case "Subtract":
                                            structOutcome.Calc = EventCalc.Subtract;
                                            break;
                                        case "equals":
                                        case "Equals":
                                            structOutcome.Calc = EventCalc.Equals;
                                            break;
                                        case "random":
                                        case "Random":
                                            structOutcome.Calc = EventCalc.Random;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Apply, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "filter":
                                    switch (cleanToken)
                                    {
                                        case "none":
                                        case "None":
                                            structOutcome.Filter = EventFilter.None;
                                            break;
                                        case "locals":
                                        case "Locals":
                                            structOutcome.Filter = EventFilter.Locals;
                                            break;
                                        case "visitors":
                                        case "Visitors":
                                            structOutcome.Filter = EventFilter.Visitors;
                                            break;
                                        case "followers":
                                        case "Followers":
                                            structOutcome.Filter = EventFilter.Followers;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome Filter, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "newStatus":
                                    //specific to EventStatus outcomes
                                    switch (cleanToken)
                                    {
                                        case "active":
                                        case "Active":
                                            structOutcome.NewStatus = EventStatus.Active;
                                            break;
                                        case "live":
                                        case "Live":
                                            structOutcome.NewStatus = EventStatus.Live;
                                            break;
                                        case "dormant":
                                        case "Dormant":
                                            structOutcome.NewStatus = EventStatus.Dormant;
                                            break;
                                        case "dead":
                                        case "Dead":
                                            structOutcome.NewStatus = EventStatus.Dead;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome newStatus, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "timer":
                                    //specific to EventTimer outcomes -> values of '1', for example, give a 1 turn effect (internally adjusted to do so)
                                    switch (cleanToken)
                                    {
                                        case "repeat":
                                        case "Repeat":
                                            structOutcome.Timer = EventTimer.Repeat;
                                            break;
                                        case "dormant":
                                        case "Dormant":
                                            structOutcome.Timer = EventTimer.Dormant;
                                            break;
                                        case "live":
                                        case "Live":
                                            structOutcome.Timer = EventTimer.Live;
                                            break;
                                        case "cool":
                                        case "Cool":
                                            structOutcome.Timer = EventTimer.Cool;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome timer, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "plyrRes":
                                    //Change Resource outcomes only -> if true then Player's resources are changed, otherwise opponents
                                    switch (cleanToken)
                                    {
                                        case "Yes":
                                        case "yes":
                                            structOutcome.PlayerRes = true;
                                            break;
                                        case "No":
                                        case "no":
                                            structOutcome.PlayerRes = false;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, PlyrRes, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "conPlyr":
                                    //Condition outcomes - if true applies to Player, otherwise NPC actor
                                    switch (cleanToken)
                                    {
                                        case "Yes":
                                        case "yes":
                                            structOutcome.PlayerCondition = true;
                                            break;
                                        case "No":
                                        case "no":
                                            structOutcome.PlayerCondition = false;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, conPlyr, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "conText":
                                    //Condition outcomes - name of condition, eg. "Old Age"
                                    structOutcome.ConditionText = cleanToken;
                                    break;
                                case "conSkill":
                                    //Condition outcomes - type of skill affected
                                    switch (cleanToken)
                                    {
                                        case "Combat":
                                        case "combat":
                                            structOutcome.ConditionSkill = SkillType.Combat;
                                            break;
                                        case "Wits":
                                        case "wits":
                                            structOutcome.ConditionSkill = SkillType.Wits;
                                            break;
                                        case "Charm":
                                        case "charm":
                                            structOutcome.ConditionSkill = SkillType.Charm;
                                            break;
                                        case "Treachery":
                                        case "treachery":
                                            structOutcome.ConditionSkill = SkillType.Treachery;
                                            break;
                                        case "Leadership":
                                        case "leadership":
                                            structOutcome.ConditionSkill = SkillType.Leadership;
                                            break;
                                        case "Touched":
                                        case "touched":
                                            structOutcome.ConditionSkill = SkillType.Touched;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, conSkill, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "conEffect":
                                    //Condition outcomes - effect of condition on skill
                                    try
                                    {
                                        dataInt = Convert.ToInt32(cleanToken);
                                        if (dataInt >= -2 && dataInt <= 2 && dataInt != 0) { structOutcome.ConditionEffect = dataInt; }
                                        else { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conEffect, (Value outside acceptable range) \"{0}\"", arrayOfEvents[i])));
                                            validData = false; }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conEffect, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "conTimer":
                                    //Condition outcomes - how long condition applies for
                                    try
                                    {
                                        dataInt = Convert.ToInt32(cleanToken);
                                        if (dataInt > 0 && dataInt < 1000) { structOutcome.ConditionTimer = dataInt; }
                                        else
                                        {
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conTimer, (Value outside acceptable range) \"{0}\"", arrayOfEvents[i])));
                                            validData = false;
                                        }
                                    }
                                    catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conTimer, (Conversion) \"{0}\"", arrayOfEvents[i]))); validData = false; }
                                    break;
                                case "chall":
                                    //conflict outcomes - is the Player the challenger?
                                    switch (cleanToken)
                                    {
                                        case "Yes":
                                        case "yes":
                                            structOutcome.Challenger = true;
                                            break;
                                        case "No":
                                        case "no":
                                            structOutcome.Challenger = false;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, Challenger, (\"{0}\")", arrayOfEvents[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "CsubType":
                                    //Conflict outcome subtype
                                    switch (cleanToken)
                                    {
                                        case "personal":
                                        case "Personal":
                                            structOutcome.SubType = ConflictSubType.Personal;
                                            structOutcome.Combat_Type = ConflictCombat.Personal;
                                            structOutcome.Conflict_Type = ConflictType.Combat;
                                            break;
                                        case "tournament":
                                        case "Tournament":
                                            structOutcome.SubType = ConflictSubType.Tournament;
                                            structOutcome.Combat_Type = ConflictCombat.Tournament;
                                            structOutcome.Conflict_Type = ConflictType.Combat;
                                            break;
                                        case "battle":
                                        case "Battle":
                                            structOutcome.SubType = ConflictSubType.Battle;
                                            structOutcome.Combat_Type = ConflictCombat.Battle;
                                            structOutcome.Conflict_Type = ConflictType.Combat;
                                            break;
                                        case "hunting":
                                        case "Hunting":
                                            structOutcome.SubType = ConflictSubType.Hunting;
                                            structOutcome.Combat_Type = ConflictCombat.Hunting;
                                            structOutcome.Conflict_Type = ConflictType.Combat;
                                            break;
                                        case "blackmail":
                                        case "Blackmail":
                                            structOutcome.SubType = ConflictSubType.Blackmail;
                                            structOutcome.Social_Type = ConflictSocial.Blackmail;
                                            structOutcome.Conflict_Type = ConflictType.Social;
                                            break;
                                        case "seduce":
                                        case "Seduce":
                                            structOutcome.SubType = ConflictSubType.Seduce;
                                            structOutcome.Social_Type = ConflictSocial.Seduce;
                                            structOutcome.Conflict_Type = ConflictType.Social;
                                            break;
                                        case "befriend":
                                        case "Befriend":
                                            structOutcome.SubType = ConflictSubType.Befriend;
                                            structOutcome.Social_Type = ConflictSocial.Befriend;
                                            structOutcome.Conflict_Type = ConflictType.Social;
                                            break;
                                        case "infiltrate":
                                        case "Infiltrate":
                                            structOutcome.SubType = ConflictSubType.Infiltrate;
                                            structOutcome.Stealth_Type = ConflictStealth.Infiltrate;
                                            structOutcome.Conflict_Type = ConflictType.Stealth;
                                            break;
                                        case "evade":
                                        case "Evade":
                                            structOutcome.SubType = ConflictSubType.Evade;
                                            structOutcome.Stealth_Type = ConflictStealth.Evade;
                                            structOutcome.Conflict_Type = ConflictType.Stealth;
                                            break;
                                        case "escape":
                                        case "Escape":
                                            structOutcome.SubType = ConflictSubType.Escape;
                                            structOutcome.Stealth_Type = ConflictStealth.Escape;
                                            structOutcome.Conflict_Type = ConflictType.Stealth;
                                            break;
                                        default:
                                            Game.SetError(new Error(49, string.Format("Invalid Input, CsubType, (\"{0}\")", arrayOfEvents[i])));
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
                                        else if (outcomeFlag == false)
                                        { Game.SetError(new Error(49, string.Format("{0} has no Outcomes", structEvent.Name))); validData = false; }
                                        listOptions.Add(structOption);
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
                                            //status -> default 'active' if not present
                                            if (structEvent.Status == EventStatus.None)
                                            { eventTemp.Status = EventStatus.Active; }
                                            else
                                            { eventTemp.Status = structEvent.Status; }
                                            //Repeat Timer -> default 1000 (constructor) if not present
                                            if (structEvent.Repeat > 0) { eventTemp.TimerRepeat = structEvent.Repeat; }
                                            //Dormant Timer -> default 0 (constructor) if not present, given +1 to negate immediate -1 in use
                                            if (structEvent.Dormant > 0) { eventTemp.TimerDormant = structEvent.Dormant + 1; }
                                            //Live Timer -> default 0 (constructor) if not present, given +1 to negate immediate -1 in use
                                            if (structEvent.Live > 0) { eventTemp.TimerLive = structEvent.Live + 1; }
                                            //Cool down Timer -> default 0 (constructor) if not present, given +1 to negate immediate -1 in use
                                            if (structEvent.Cool > 0) { eventTemp.TimerCoolBase = structEvent.Cool + 1; }
                                            //SubRef -> default 0, only applies to AutoReact Player Events
                                            if (structEvent.SubRef > 0) { eventTemp.SubRef = structEvent.SubRef; }
                                            //add options
                                            if (listOptions.Count > 1)
                                            {
                                                for (int index = 0; index < listOptions.Count; index++)
                                                {
                                                    OptionStruct optionTemp = listOptions[index];
                                                    //at least one outcome must be present
                                                    if (listAllOutcomes.Count > 0)
                                                    {
                                                        OptionInteractive optionObject = new OptionInteractive(optionTemp.Text)
                                                        { ReplyGood = optionTemp.Reply, ReplyBad = optionTemp.ReplyBad, Test = optionTemp.Test };

                                                        //Triggers (optional)
                                                        List<Trigger> tempTriggers = listAllTriggers[index];
                                                        Trigger trigger = tempTriggers[0];
                                                        //if first record has a default value of None then it's a blank Trigger put there as a placeholder
                                                        if (trigger.Check != TriggerCheck.None)
                                                        {
                                                            if (index == 0)
                                                            {
                                                                //first, default option, not allowed any triggers (they're ignored)
                                                                Game.SetError(new Error(49, string.Format("No triggers allowed (ignored) for first Option of \"{0}\"", eventTemp.Name)));
                                                            }
                                                            else { optionObject.SetTriggers(tempTriggers); }
                                                        }
                                                        //Outcomes
                                                        List<OutcomeStruct> sublist = listAllOutcomes[index];
                                                        //create appropriate outcome object
                                                        for (int inner = 0; inner < sublist.Count; inner++)
                                                        {
                                                            OutcomeStruct outTemp = sublist[inner];
                                                            Outcome outObject = null;
                                                            //add appropriate outcome object to option object
                                                            switch (outTemp.Effect)
                                                            {
                                                                case "Conflict":
                                                                    outObject = new OutConflict(structEvent.EventID, outTemp.Data, outTemp.Conflict_Type, outTemp.Challenger)
                                                                    {Conflict_Type = outTemp.Conflict_Type, Social_Type = outTemp.Social_Type, Stealth_Type = outTemp.Stealth_Type,
                                                                        Combat_Type = outTemp.Combat_Type, SubType = outTemp.SubType };
                                                                    optionObject.ActorID = outTemp.Data;
                                                                    break;
                                                                case "Game":
                                                                    //check that GameVars (DataPoints) are only 'add' or 'random'
                                                                    if (outTemp.Data <= 6 && outTemp.Calc != EventCalc.Add && outTemp.Calc != EventCalc.Random)
                                                                    {
                                                                        Game.SetError(new Error(49, string.Format("Outcome \"apply: {0}\" changed to \"Add\" for Event (\"{1}\")",
                                                                            outTemp.Calc, structEvent.Name)));
                                                                        outTemp.Calc = EventCalc.Add;
                                                                    }
                                                                    outObject = new OutGame(structEvent.EventID, outTemp.Data, outTemp.Amount, outTemp.Calc);
                                                                    break;
                                                                case "EventTimer":
                                                                    if (outTemp.Data > 0)
                                                                    { outObject = new OutEventTimer(structEvent.EventID, outTemp.Data, outTemp.Amount, outTemp.Calc, outTemp.Timer); }
                                                                    else
                                                                    {
                                                                        Game.SetError(new Error(49, "Invalid Input, Outcome Data, (EventTimer), can't create object)"));
                                                                        validData = false;
                                                                    }
                                                                    break;
                                                                case "EventStatus":
                                                                    if (outTemp.Data > 0)
                                                                    { outObject = new OutEventStatus(structEvent.EventID, outTemp.Data, outTemp.NewStatus); }
                                                                    else
                                                                    {
                                                                        Game.SetError(new Error(49, "Invalid Input, Outcome Data (EventStatus), (Data <= Zero, can't create object)"));
                                                                        validData = false;
                                                                    }
                                                                    break;
                                                                case "EventChain":
                                                                    if (outTemp.Data > 0)
                                                                    { outObject = new OutEventChain(structEvent.EventID, outTemp.Filter); }
                                                                    else
                                                                    {
                                                                        Game.SetError(new Error(49, "Invalid Input, Outcome Data (EventChain), (Data <= Zero, can't create object)"));
                                                                        validData = false;
                                                                    }
                                                                    break;
                                                                case "Resource":
                                                                    if (outTemp.Calc == EventCalc.Add || outTemp.Calc == EventCalc.Subtract || outTemp.Calc == EventCalc.Equals)
                                                                    { outObject = new OutResource(structEvent.EventID, outTemp.PlayerRes, outTemp.Amount, outTemp.Calc); }
                                                                    else
                                                                    {
                                                                        Game.SetError(new Error(49, "Invalid Input, Outcome Calc (Resource), (only Add/Subtract/Equals allowed)"));
                                                                        validData = false;
                                                                    }
                                                                    break;
                                                                case "Condition":
                                                                    if (outTemp.ConditionSkill > SkillType.None && outTemp.ConditionEffect != 0 && String.IsNullOrEmpty(outTemp.ConditionText) == false
                                                                        && outTemp.ConditionTimer > 0)
                                                                    {
                                                                        Condition condition = new Condition(outTemp.ConditionSkill, outTemp.ConditionEffect, outTemp.ConditionText, outTemp.ConditionTimer);
                                                                        outObject = new OutCondition(structEvent.EventID, outTemp.PlayerCondition, condition); }
                                                                    else
                                                                    {
                                                                        Game.SetError(new Error(49, "Invalid Input, Outcome Condition variables (default data exists)"));
                                                                        validData = false;
                                                                    }
                                                                    break;
                                                                case "None":
                                                                    outObject = new OutNone(structEvent.EventID);
                                                                    break;
                                                                default:
                                                                    Game.SetError(new Error(49, string.Format("Invalid Outcome Effect for Event (\"{0}\")", structEvent.Name)));
                                                                    validData = false;
                                                                    break;
                                                            }
                                                            //add Outcome object to Option object
                                                            if (outObject != null)
                                                            {
                                                                if (outTemp.Bad > 0) { optionObject.SetBadOutcome(outObject);}
                                                                else { optionObject.SetGoodOutcome(outObject); }
                                                            }
                                                        }
                                                        //add option object to event object
                                                        eventTemp.SetOption(optionObject);
                                                    }
                                                    else
                                                    { Game.SetError(new Error(49, string.Format("{0} has no Outcome for Option {1}", structEvent.Name, index + 1))); validData = false; }
                                                }
                                            }
                                            else { Game.SetError(new Error(49, string.Format("{0} has insufficient Options", structEvent.Name))); validData = false; }

                                            //last datapoint - save object to dictionary
                                            if (dataCounter > 0)
                                            {
                                                //final checks all passed?
                                                if (validData == true)
                                                {
                                                    try
                                                    { dictOfPlayerEvents.Add(eventTemp.EventPID, eventTemp); }
                                                    catch
                                                    { Game.SetError(new Error(49, string.Format("Event, (\"{0}\" EventID {1}), not Imported (add to Dictionary)", structEvent.Name, structEvent.EventID))); }
                                                }
                                                else
                                                { Game.SetError(new Error(49, string.Format("Event, (\"{0}\" EventID {1}), not Imported (validData false)", structEvent.Name, structEvent.EventID))); }
                                            }
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
                //debug - data dump of Player Events
                Console.WriteLine(Environment.NewLine + "--- Player Events");
                Type type;
                String name;
                //events
                foreach (var eventObject in dictOfPlayerEvents)
                {
                    Console.WriteLine("\"{0}\" Event, ID {1}, Type {2}, Repeat {3}, Dormant {4}, Live {5}, Status {6}", eventObject.Value.Name, eventObject.Value.EventPID, eventObject.Value.Type,
                        eventObject.Value.TimerRepeat, eventObject.Value.TimerDormant, eventObject.Value.TimerLive, eventObject.Value.Status);
                    List<OptionInteractive> listTempOptions = eventObject.Value.GetOptions();
                    //options
                    foreach (OptionInteractive optionObject in listTempOptions)
                    {
                        Console.WriteLine("  Option \"{0}\"", optionObject.Text);
                        List<Outcome> listTempOutcomes = new List<Outcome>(); //need to create a new list otherwise copying by reference and affects records in dictOfPlayerEvents
                        listTempOutcomes.AddRange(optionObject.GetGoodOutcomes());
                        listTempOutcomes.AddRange(optionObject.GetBadOutcomes());
                        List<Trigger> listTempTriggers = optionObject.GetTriggers();
                        //triggers
                        foreach (Trigger triggerObject in listTempTriggers)
                        { Console.WriteLine("   {0} -> if {1} {2} is {3} to {4}", "Trigger", triggerObject.Check, triggerObject.Data, triggerObject.Calc, triggerObject.Threshold); }
                        //outcomes
                        foreach (Outcome outcomeObject in listTempOutcomes)
                        {
                            //extract and isolate name of derived outcome object
                            type = outcomeObject.GetType();
                            name = type.ToString();
                            string[] tokens = name.Split('.');
                            //strip out leading spaces
                            cleanTag = tokens[tokens.Length - 1].Trim();
                            if (outcomeObject is OutEventStatus)
                            {
                                OutEventStatus tempOutcome = outcomeObject as OutEventStatus;
                                Console.WriteLine("    {0} -> Target EventID {1}, New Status {2}", cleanTag, tempOutcome.Data, tempOutcome.NewStatus);
                            }
                            else if (outcomeObject is OutEventChain)
                            {
                                OutEventChain tempOutcome = outcomeObject as OutEventChain;
                                Console.WriteLine("    {0} -> Target EventID {1}", cleanTag, tempOutcome.Data);
                            }
                            else if (outcomeObject is OutEventTimer)
                            {
                                OutEventTimer tempOutcome = outcomeObject as OutEventTimer;
                                Console.WriteLine("    {0} -> Target EventID {1}, {2} timer, amount {3} apply {4}", cleanTag, tempOutcome.Data, tempOutcome.Timer, tempOutcome.Amount, tempOutcome.Calc);
                            }
                            else if (outcomeObject is OutConflict)
                            {
                                OutConflict tempOutcome = outcomeObject as OutConflict;
                                Console.WriteLine("    {0} -> subType {1}, oppID {2}, Challenger {3}", cleanTag, tempOutcome.SubType, tempOutcome.Data, tempOutcome.Challenger);
                            }
                            else if (outcomeObject is OutResource)
                            {
                                OutResource tempOutcome = outcomeObject as OutResource;
                                Console.WriteLine("    {0} -> Player? {1}, amount {2}, apply {3}", cleanTag, tempOutcome.PlayerRes, tempOutcome.Amount, tempOutcome.Calc);
                            }
                            else if (outcomeObject is OutCondition)
                            {
                                OutCondition tempOutcome = outcomeObject as OutCondition;
                                Console.WriteLine("    {0} -> Player? {1}, Condition \"{2}\", {3} {4}, Timer {5}", cleanTag, tempOutcome.PlayerCondition, tempOutcome.NewCondition.Text, 
                                    tempOutcome.NewCondition.Skill, tempOutcome.NewCondition.Effect, tempOutcome.NewCondition.Timer);
                            }
                            else
                            { Console.WriteLine("    {0} -> data {1}, amount {2}, apply {3}", cleanTag, outcomeObject.Data, outcomeObject.Amount, outcomeObject.Calc); }
                        }
                    }
                    Console.WriteLine();
                }
            }
            else
            { Game.SetError(new Error(49, string.Format("File not found (\"{0}\")", fileName))); }
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
            if (arrayOfArchetypes != null)
            {
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
                        //string[] tokens = arrayOfArchetypes[i].Split(':');
                        string[] tokens = arrayOfArchetypes[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(53, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
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
                                        { Game.SetError(new Error(53, string.Format("Invalid EventID for {0}, {1}", structArc.Name, fileName))); }
                                    }
                                    structArc.listOfEvents = tempList;
                                    break;

                                case "[end]":
                                case "[End]":
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
            }
            else
            { Game.SetError(new Error(53, string.Format("File not found (\"{0}\")", fileName))); }
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
            if (arrayOfStories != null)
            {
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

                        //string[] tokens = arrayOfStories[i].Split(':');
                        string[] tokens = arrayOfStories[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        //if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(54, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }

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
                            case "[end]":
                            case "[End]":
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Story storyObject = new Story(structStory.Name, structStory.StoryID, structStory.Type);
                                    storyObject.Ev_Follower_Loc = structStory.Ev_Follower_Loc;
                                    storyObject.Ev_Follower_Trav = structStory.Ev_Follower_Trav;
                                    storyObject.Ev_Player_Loc_Base = structStory.Ev_Player_Loc;
                                    storyObject.Ev_Player_Trav_Base = structStory.Ev_Player_Trav;
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
            }
            else
            { Game.SetError(new Error(54, string.Format("File not found (\"{0}\")", fileName))); }
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
            if (arrayOfGeoNames != null)
            {

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
            }
            else
            { Game.SetError(new Error(23, string.Format("File not found (\"{0}\")", fileName))); }
            return arrayOfNames;
        }

        /// <summary>
        /// Import Relationship lists (Houses and Actors)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal string[][] GetRelations(string fileName)
        {
            //master jagged array which will be returned, NOTE: add to enum 'RelListType' (Tracker.cs) for each unique new Relationship lis
            string[][] arrayOfNames = new string[(int)RelListType.Count][];
            string tempString;
            //temporary sub lists for each category of geoNames
            List<string> listOfHousePastGood = new List<string>();
            List<string> listOfHousePastBad = new List<string>();
            //import data from file
            string[] arrayOfRelTexts = ImportDataFile(fileName);
            if (arrayOfRelTexts != null)
            {

                //read location names from array into list
                string nameType = null;
                char[] charsToTrim = { '[', ']' };
                for (int i = 0; i < arrayOfRelTexts.Length; i++)
                {
                    if (arrayOfRelTexts[i] != "" && !arrayOfRelTexts[i].StartsWith("#"))
                    {
                        //which sublist are we dealing with
                        tempString = arrayOfRelTexts[i];
                        //trim off leading and trailing whitespace
                        tempString = tempString.Trim();
                        if (tempString.StartsWith("["))
                        { nameType = tempString.Trim(charsToTrim); }
                        else if (nameType != null)
                        {
                            //place in the correct list
                            switch (nameType)
                            {
                                case "Major House Good":
                                    listOfHousePastGood.Add(tempString);
                                    break;
                                case "Major House Bad":
                                    listOfHousePastBad.Add(tempString);
                                    break;
                                default:
                                    Game.SetError(new Error(133, string.Format("Invalid Relationship Category {0}, record {1}", nameType, i)));
                                    break;
                            }
                        }
                    }
                }
                //size jagged array
                arrayOfNames[(int)RelListType.HousePastGood] = new string[listOfHousePastGood.Count];
                arrayOfNames[(int)RelListType.HousePastBad] = new string[listOfHousePastBad.Count];
                //populate from lists
                arrayOfNames[(int)RelListType.HousePastGood] = listOfHousePastGood.ToArray();
                arrayOfNames[(int)RelListType.HousePastBad] = listOfHousePastBad.ToArray();
            }
            else
            { Game.SetError(new Error(23, string.Format("File not found (\"{0}\")", fileName))); }
            return arrayOfNames;
        }

        /// <summary>
        /// Import Followers
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal List<FollowerStruct> GetFollowers(string fileName)
        {
            List<FollowerStruct> listOfStructs = new List<FollowerStruct>();
            string[] arrayOfFollowers = ImportDataFile(fileName);
            if (arrayOfFollowers != null)
            {
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
                        //string[] tokens = arrayOfFollowers[i].Split(':');
                        string[] tokens = arrayOfFollowers[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(59, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
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
                                    //should be level 0 to 5
                                    try
                                    {
                                        int tempNum = Convert.ToInt32(cleanToken);
                                        if (tempNum > 5) { tempNum = 5; Game.SetError(new Error(59, string.Format("Invalid Input, Resources > 5 (\"{0}\")", arrayOfFollowers[i]))); }
                                        else if (tempNum < 0) { tempNum = 0; Game.SetError(new Error(59, string.Format("Invalid Input, Resources < 1 (\"{0}\")", arrayOfFollowers[i]))); }
                                        structFollower.Resources = tempNum;
                                    }
                                    catch (Exception e)
                                    { Game.SetError(new Error(59, e.Message)); validData = false; }
                                    break;
                                case "Loyalty":
                                    try
                                    {
                                        //loyalty -> range 0 to 100
                                        int loyalty = Convert.ToInt32(cleanToken);
                                        loyalty = Math.Min(100, loyalty);
                                        loyalty = Math.Max(0, loyalty);
                                        structFollower.Loyalty = loyalty;
                                    }
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
                                case "[end]":
                                case "[End]":
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
            }
            else
            { Game.SetError(new Error(59, string.Format("File not found (\"{0}\")", fileName))); }
            return listOfStructs;
        }

        /// <summary>
        /// Import Situations
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, Situation> GetSituations(string fileName)
        {
            Dictionary<int, Situation> tempDictionary = new Dictionary<int, Situation>();
            List<string> tempListGood = new List<string>();
            List<string> tempListBad = new List<string>();
            string[] arrayOfSituations = ImportDataFile(fileName);
            if (arrayOfSituations != null)
            {
                List<SituationStruct> listOfStructs = new List<SituationStruct>();
                bool newSituation = false;
                bool validData = true;
                int dataCounter = 0; //number of situations
                SituationStruct structSituation = new SituationStruct();
                string cleanToken;
                string cleanTag;
                string subType = "unknown";
                for (int i = 0; i < arrayOfSituations.Length; i++)
                {
                    if (arrayOfSituations[i] != "" && !arrayOfSituations[i].StartsWith("#"))
                    {
                        //set up for a new house
                        if (newSituation == false)
                        {
                            newSituation = true;
                            validData = true;
                            //Console.WriteLine();
                            dataCounter++;
                            //new structure
                            structSituation = new SituationStruct();
                            tempListGood.Clear();
                            tempListBad.Clear();
                        }
                        //string[] tokens = arrayOfSituations[i].Split(':');
                        string[] tokens = arrayOfSituations[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(98, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
                        if (cleanToken.Length == 0)
                        {
                            validData = false;
                            Game.SetError(new Error(98, string.Format("Situation {0} (Missing data for \"{1}\") \"{2}\"",
                            String.IsNullOrEmpty(structSituation.Name) ? "?" : structSituation.Name, cleanTag, fileName)));
                        }
                        else
                        {
                            switch (cleanTag)
                            {
                                case "Name":
                                case "Skill":
                                    structSituation.Name = cleanToken;
                                    break;
                                case "Type":
                                    switch (cleanToken)
                                    {
                                        case "Combat":
                                            structSituation.Type = ConflictType.Combat;
                                            break;
                                        case "Social":
                                            structSituation.Type = ConflictType.Social;
                                            break;
                                        case "Stealth":
                                            structSituation.Type = ConflictType.Stealth;
                                            break;
                                        case "State":
                                        case "Special":
                                            structSituation.Type = ConflictType.None;
                                            break;
                                        default:
                                            structSituation.Type = ConflictType.None;
                                            Game.SetError(new Error(98, string.Format("Invalid Input, Type (\"{0}\")", arrayOfSituations[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "SubType":
                                    switch (cleanToken)
                                    {
                                        //...Combat
                                        case "Tournament":
                                            if (structSituation.Type == ConflictType.Combat)
                                            { structSituation.Type_Combat = ConflictCombat.Tournament; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Personal":
                                            if (structSituation.Type == ConflictType.Combat)
                                            { structSituation.Type_Combat = ConflictCombat.Personal; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Battle":
                                            if (structSituation.Type == ConflictType.Combat)
                                            { structSituation.Type_Combat = ConflictCombat.Battle; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Hunting":
                                            if (structSituation.Type == ConflictType.Combat)
                                            { structSituation.Type_Combat = ConflictCombat.Hunting; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        //...Social
                                        case "Blackmail":
                                            if (structSituation.Type == ConflictType.Social)
                                            { structSituation.Type_Social = ConflictSocial.Blackmail; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Seduce":
                                            if (structSituation.Type == ConflictType.Social)
                                            { structSituation.Type_Social = ConflictSocial.Seduce; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Befriend":
                                            if (structSituation.Type == ConflictType.Social)
                                            { structSituation.Type_Social = ConflictSocial.Befriend; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        //...Stealth
                                        case "Infiltrate":
                                            if (structSituation.Type == ConflictType.Stealth)
                                            { structSituation.Type_Stealth = ConflictStealth.Infiltrate; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Evade":
                                            if (structSituation.Type == ConflictType.Stealth)
                                            { structSituation.Type_Stealth = ConflictStealth.Evade; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Escape":
                                            if (structSituation.Type == ConflictType.Stealth)
                                            { structSituation.Type_Stealth = ConflictStealth.Escape; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        //...State (Game)
                                        case "ArmySize":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.State = ConflictState.Relative_Army_Size; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Fame":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.State = ConflictState.Relative_Fame; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Honour":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.State = ConflictState.Relative_Honour; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Justice":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.State = ConflictState.Relative_Justice; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "Invisibility":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.State = ConflictState.Relative_Invisibility; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        //...Special
                                        case "FortifiedPosition":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.Special = ConflictSpecial.Fortified_Position; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "MountainCountry":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.Special = ConflictSpecial.Mountain_Country; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "ForestCountry":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.Special = ConflictSpecial.Forest_Country; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        case "CastleWalls":
                                            if (structSituation.Type == ConflictType.None)
                                            { structSituation.Special = ConflictSpecial.Castle_Walls; }
                                            else { Game.SetError(new Error(98, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfSituations[i]))); }
                                            break;
                                        default:
                                            Game.SetError(new Error(98, string.Format("Invalid Input, SubType unknown (\"{0}\")", arrayOfSituations[i])));
                                            validData = false;
                                            break;
                                    }
                                    subType = cleanToken;
                                    break;
                                case "SitNum":
                                    try { structSituation.SitNum = Convert.ToInt32(cleanToken); }
                                    catch (Exception e)
                                    { Game.SetError(new Error(98, e.Message)); validData = false; }
                                    break;
                                case "PlyrDef":
                                    //only applies to first situation (def. adv.) if '1' then written from POV of player defending, if '-1' then opponent
                                    try
                                    {
                                        int tempNum = Convert.ToInt32(cleanToken);
                                        if (tempNum != 0)
                                        { structSituation.Defender = tempNum; }
                                        else
                                        { Game.SetError(new Error(98, "Defender input invalid (must be 1 or -1)")); }
                                    }
                                    catch (Exception e)
                                    { Game.SetError(new Error(98, e.Message)); validData = false; }
                                    break;
                                case "Data":
                                    try
                                    { structSituation.Data = Convert.ToInt32(cleanToken); }
                                    catch (Exception e)
                                    { Game.SetError(new Error(98, e.Message)); validData = false; }
                                    break;
                                case "Good1":
                                case "Good2":
                                case "Good3":
                                case "Good4":
                                case "Good5":
                                case "Good6":
                                    tempListGood.Add(cleanToken);
                                    break;
                                case "Bad1":
                                case "Bad2":
                                case "Bad3":
                                case "Bad4":
                                case "Bad5":
                                case "Bad6":
                                    tempListBad.Add(cleanToken);
                                    break;
                                case "[end]":
                                case "[End]":
                                    //last datapoint - save structure to list
                                    if (dataCounter > 0 && validData == true)
                                    {
                                        //new sitnormal, sitGame or sitSpecial -> if situation type = 'None' & conflictState > 'None' then sitGame
                                        Situation situation = null;
                                        if (structSituation.State == ConflictState.None && structSituation.Type > ConflictType.None)
                                        {
                                            situation = new Situation(structSituation.Name, structSituation.Type, structSituation.SitNum, structSituation.Type_Combat,
                                                structSituation.Type_Social, structSituation.Type_Stealth);
                                        }
                                        else if (structSituation.Type == ConflictType.None)
                                        {
                                            //sitGame
                                            if (structSituation.State > ConflictState.None)
                                            { situation = new Situation(structSituation.Name, structSituation.State, structSituation.SitNum); }
                                            //sitSpecial
                                            else if (structSituation.Special > ConflictSpecial.None)
                                            { situation = new Situation(structSituation.Name, structSituation.Special, 0); }
                                        }
                                        //add data
                                        situation.Defender = structSituation.Defender;
                                        situation.Data = structSituation.Data;
                                        situation.SetGood(tempListGood);
                                        situation.SetBad(tempListBad);
                                        tempDictionary.Add(situation.SitID, situation);
                                        if (structSituation.Type > ConflictType.None)
                                        {
                                            Console.WriteLine("\"{0}\" imported, a {1} conflict, {2} good records & {3} bad, SitNum {4}, Def {5}, Data {6}", structSituation.Name, subType,
                                              tempListGood.Count, tempListBad.Count, structSituation.SitNum, structSituation.Defender, structSituation.Data);
                                        }
                                        else if (structSituation.State > ConflictState.None)
                                        {
                                            Console.WriteLine("\"{0}\" imported, {1} good records & {2} bad, SitNum {3}", structSituation.Name, tempListGood.Count, tempListBad.Count,
                                              structSituation.SitNum);
                                        }
                                        else if (structSituation.Special > ConflictSpecial.None)
                                        {
                                            Console.WriteLine("\"{0}\" imported, {1} good records & {2} bad, Def {3}", structSituation.Name, tempListGood.Count, tempListBad.Count,
                                               structSituation.Defender);
                                        }
                                    }
                                    break;
                                default:
                                    Game.SetError(new Error(59, string.Format("Invalid Data \"{0}\" in Situation Input", cleanTag)));
                                    break;
                            }
                        }
                    }
                    else
                    { newSituation = false; }
                }
            }
            else
            { Game.SetError(new Error(98, string.Format("File not found (\"{0}\")", fileName))); }
            return tempDictionary;
        }

        /// <summary>
        /// Import Challenge Data
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<ConflictSubType, Challenge> GetChallenges(string fileName)
        {
            Dictionary<ConflictSubType, Challenge> tempDictionary = new Dictionary<ConflictSubType, Challenge>();
            string[] tempStrategy = new string[6];
            string[] tempOutcome = new string[7]; //0/1/2 Win's, 3/4/5 Losses, 6 No result
            SkillType[] tempSkill = new SkillType[3];
            string[] arrayOfChallenges = ImportDataFile(fileName);
            if (arrayOfChallenges != null)
            {
                List<ChallengeStruct> listOfStructs = new List<ChallengeStruct>();
                //set up blank result list structure
                List<List<int>> listOfResults = new List<List<int>>();
                for (int i = 0; i < (int)ConflictResult.Count; i++)
                {
                    List<int> tempSubList = new List<int>();
                    tempSubList.Add(0);
                    listOfResults.Add(tempSubList);
                }
                bool newChallenge = false;
                bool validData = true;
                int dataCounter = 0; //number of challenges
                int subIndex = 0; //int value of subtype
                ConflictSubType subDict = ConflictSubType.None; //type of subtype used as an index for the dicitonary
                ChallengeStruct structChallenge = new ChallengeStruct();
                string cleanToken;
                string cleanTag;
                string subType = "unknown";
                for (int i = 0; i < arrayOfChallenges.Length; i++)
                {
                    if (arrayOfChallenges[i] != "" && !arrayOfChallenges[i].StartsWith("#"))
                    {
                        //set up for a new house
                        if (newChallenge == false)
                        {
                            newChallenge = true;
                            validData = true;
                            subIndex = 0;
                            subDict = ConflictSubType.None;
                            //Console.WriteLine();
                            dataCounter++;
                            //new structure
                            structChallenge = new ChallengeStruct();
                            Array.Clear(tempStrategy, 0, tempStrategy.Length);
                            Array.Clear(tempOutcome, 0, tempOutcome.Length);
                            Array.Clear(tempSkill, 0, tempSkill.Length);
                            //clear out Results ready for next challenge
                            for (int k = 0; k < (int)ConflictResult.Count; k++)
                            { listOfResults[k].Clear(); }
                        }
                        string[] tokens = arrayOfChallenges[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(110, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }
                        if (cleanToken.Length == 0)
                        {
                            validData = false;
                            Game.SetError(new Error(110, string.Format("Challenge {0} (Missing data for \"{1}\") \"{2}\"", subType, cleanTag, fileName)));
                        }
                        else
                        {
                            switch (cleanTag)
                            {
                                case "Type":
                                    switch (cleanToken)
                                    {
                                        case "Combat":
                                            structChallenge.Type = ConflictType.Combat;
                                            break;
                                        case "Social":
                                            structChallenge.Type = ConflictType.Social;
                                            break;
                                        case "Stealth":
                                            structChallenge.Type = ConflictType.Stealth;
                                            break;
                                        default:
                                            structChallenge.Type = ConflictType.None;
                                            Game.SetError(new Error(110, string.Format("Invalid Input, Type (\"{0}\")", arrayOfChallenges[i])));
                                            validData = false;
                                            break;
                                    }
                                    break;
                                case "SubType":
                                    switch (cleanToken)
                                    {
                                        //...Combat
                                        case "Tournament":
                                            if (structChallenge.Type == ConflictType.Combat)
                                            { structChallenge.CombatType = ConflictCombat.Tournament; subIndex = (int)structChallenge.CombatType; subDict = ConflictSubType.Tournament; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Personal":
                                            if (structChallenge.Type == ConflictType.Combat)
                                            { structChallenge.CombatType = ConflictCombat.Personal; subIndex = (int)structChallenge.CombatType; subDict = ConflictSubType.Personal; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Battle":
                                            if (structChallenge.Type == ConflictType.Combat)
                                            { structChallenge.CombatType = ConflictCombat.Battle; subIndex = (int)structChallenge.CombatType; subDict = ConflictSubType.Battle; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Hunting":
                                            if (structChallenge.Type == ConflictType.Combat)
                                            { structChallenge.CombatType = ConflictCombat.Hunting; subIndex = (int)structChallenge.CombatType; subDict = ConflictSubType.Hunting; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        //...Social
                                        case "Blackmail":
                                            if (structChallenge.Type == ConflictType.Social)
                                            { structChallenge.SocialType = ConflictSocial.Blackmail; subIndex = (int)structChallenge.SocialType; subDict = ConflictSubType.Blackmail; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Seduce":
                                            if (structChallenge.Type == ConflictType.Social)
                                            { structChallenge.SocialType = ConflictSocial.Seduce; subIndex = (int)structChallenge.SocialType; subDict = ConflictSubType.Seduce; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Befriend":
                                            if (structChallenge.Type == ConflictType.Social)
                                            { structChallenge.SocialType = ConflictSocial.Befriend; subIndex = (int)structChallenge.SocialType; subDict = ConflictSubType.Befriend; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        //...Stealth
                                        case "Infiltrate":
                                            if (structChallenge.Type == ConflictType.Stealth)
                                            { structChallenge.StealthType = ConflictStealth.Infiltrate; subIndex = (int)structChallenge.StealthType; subDict = ConflictSubType.Infiltrate; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Evade":
                                            if (structChallenge.Type == ConflictType.Stealth)
                                            { structChallenge.StealthType = ConflictStealth.Evade; subIndex = (int)structChallenge.StealthType; subDict = ConflictSubType.Evade; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                        case "Escape":
                                            if (structChallenge.Type == ConflictType.Stealth)
                                            { structChallenge.StealthType = ConflictStealth.Escape; subIndex = (int)structChallenge.StealthType; subDict = ConflictSubType.Escape; }
                                            else { Game.SetError(new Error(110, string.Format("Invalid Input, SubType (\"{0}\") Doesn't match Type", arrayOfChallenges[i]))); validData = false; }
                                            break;
                                    }
                                    subType = cleanToken;
                                    break;
                                case "PlyrStrAgg":
                                    tempStrategy[0] = cleanToken;
                                    break;
                                case "PlyrStrBal":
                                    tempStrategy[1] = cleanToken;
                                    break;
                                case "PlyrStrDef":
                                    tempStrategy[2] = cleanToken;
                                    break;
                                case "OppStrAgg":
                                    tempStrategy[3] = cleanToken;
                                    break;
                                case "OppStrBal":
                                    tempStrategy[4] = cleanToken;
                                    break;
                                case "OppStrDef":
                                    tempStrategy[5] = cleanToken;
                                    break;
                                case "OutWinMinor":
                                    tempOutcome[0] = cleanToken;
                                    break;
                                case "OutWin":
                                    tempOutcome[1] = cleanToken;
                                    break;
                                case "OutWinMajor":
                                    tempOutcome[2] = cleanToken;
                                    break;
                                case "OutLossMinor":
                                    tempOutcome[3] = cleanToken;
                                    break;
                                case "OutLoss":
                                    tempOutcome[4] = cleanToken;
                                    break;
                                case "OutLossMajor":
                                    tempOutcome[5] = cleanToken;
                                    break;
                                case "OutNone":
                                    tempOutcome[6] = cleanToken;
                                    break;
                                //skills
                                case "SkillPrime":
                                case "SkillSecond":
                                case "SkillThird":
                                    SkillType skill = SkillType.None;
                                    switch (cleanToken)
                                    {
                                        case "Combat":
                                            skill = SkillType.Combat;
                                            break;
                                        case "Wits":
                                            skill = SkillType.Wits;
                                            break;
                                        case "Charm":
                                            skill = SkillType.Charm;
                                            break;
                                        case "Treachery":
                                            skill = SkillType.Treachery;
                                            break;
                                        case "Leadership":
                                            skill = SkillType.Leadership;
                                            break;
                                        default:
                                            Game.SetError(new Error(110, string.Format("Invalid Skill, (\"{0}\", \"{1}\")", arrayOfChallenges[i], cleanToken)));
                                            validData = false;
                                            break;
                                    }
                                    if (cleanTag == "SkillSecond") { tempSkill[1] = skill; }
                                    else if (cleanTag == "SkillThird") { tempSkill[2] = skill; }
                                    else { tempSkill[0] = skill; }
                                    break;
                                //Results
                                case "ResNone":
                                case "ResWinMinor":
                                case "ResWin":
                                case "ResWinMajor":
                                case "ResLossMinor":
                                case "ResLoss":
                                case "ResLossMajor":
                                    //get list of Events
                                    string[] arrayOfResults = cleanToken.Split(',');
                                    List<int> tempList = new List<int>();
                                    int dataInt;
                                    //loop resultID array and add all to lists
                                    string tempHandle = null;

                                    for (int k = 0; k < arrayOfResults.Length; k++)
                                    {
                                        tempHandle = arrayOfResults[k].Trim();
                                        if (String.IsNullOrEmpty(tempHandle) == false)
                                        {
                                            try
                                            {
                                                dataInt = Convert.ToInt32(tempHandle);
                                                if (dataInt > 0)
                                                {
                                                    //check a valid event
                                                    if (Game.director.CheckResult(dataInt))
                                                    { tempList.Add(dataInt); }
                                                    else
                                                    {
                                                        Game.SetError(new Error(110, string.Format("Invalid resultID \"{0}\" (Not found in Dictionary) for {1} -> {2}", dataInt, structChallenge.Type, subType)));
                                                        validData = false;
                                                    }
                                                }
                                                else
                                                { Game.SetError(new Error(110, string.Format("Invalid resultID (Zero, or less) for {0} -> {1}", structChallenge.Type, subType))); validData = false; }
                                            }
                                            catch { Game.SetError(new Error(110, string.Format("Invalid resultID (Conversion Error) for {0} -> {1}", structChallenge.Type, subType))); validData = false; }
                                        }
                                        //dodgy resultID is ignored, it doesn't invalidate the record
                                        else
                                        { Game.SetError(new Error(110, string.Format("Invalid resultID for {0} -> {1}", structChallenge.Type, subType))); }
                                    }
                                    if (tempList.Count > 0)
                                    {
                                        //place list in correct Result sublist
                                        int index = -1;
                                        switch (cleanTag)
                                        {
                                            case "ResNone":
                                                index = (int)ConflictResult.None;
                                                break;
                                            case "ResWinMinor":
                                                index = (int)ConflictResult.MinorWin;
                                                break;
                                            case "ResWin":
                                                index = (int)ConflictResult.Win;
                                                break;
                                            case "ResWinMajor":
                                                index = (int)ConflictResult.MajorWin;
                                                break;
                                            case "ResLossMinor":
                                                index = (int)ConflictResult.MinorLoss;
                                                break;
                                            case "ResLoss":
                                                index = (int)ConflictResult.Loss;
                                                break;
                                            case "ResLossMajor":
                                                index = (int)ConflictResult.MajorLoss;
                                                break;
                                            default:
                                                Game.SetError(new Error(110, string.Format("No valid Tag for Result sublist (default) \"{0}\"", cleanTag)));
                                                break;
                                        }
                                        if (index > -1)
                                        { listOfResults[index].AddRange(tempList); }
                                        else
                                        { Game.SetError(new Error(110, "Invalid index value (\"-1\")")); }
                                    }
                                    else
                                    { Game.SetError(new Error(110, string.Format("No valid data for Result sublist \"{0}\"", cleanTag))); validData = false; }

                                    break;
                                case "[end]":
                                case "[End]":
                                    //last datapoint - save structure to list
                                    if (dataCounter > 0 && validData == true)
                                    {
                                        //validate data
                                        for (int index = 0; index < tempStrategy.Length; index++)
                                        {
                                            if (String.IsNullOrEmpty(tempStrategy[index]) == true)
                                            { Game.SetError(new Error(110, string.Format("Missing data in tempStrategy (record index {0})", index))); validData = false; }
                                            if (String.IsNullOrEmpty(tempOutcome[index]) == true)
                                            { Game.SetError(new Error(110, string.Format("Missing data in tempOutcome (record index {0})", index))); validData = false; }
                                            if (index < tempSkill.Length)
                                            {
                                                if (tempSkill[index] == SkillType.None)
                                                { Game.SetError(new Error(110, string.Format("Missing data in tempSkill (SkillType \"None\")", index))); validData = false; }
                                            }
                                        }
                                        if (subIndex <= 0)
                                        { Game.SetError(new Error(110, "Invalid subIndex (zero or less)")); validData = false; }
                                        //add data
                                        if (validData == true)
                                        {
                                            Challenge challenge = new Challenge(structChallenge.Type, structChallenge.CombatType, structChallenge.SocialType, structChallenge.StealthType);
                                            challenge.SetStrategies(tempStrategy);
                                            challenge.SetOutcomes(tempOutcome);
                                            challenge.SetSkills(tempSkill);
                                            for (int j = 0; j < listOfResults.Count; j++)
                                            { challenge.SetResults((ConflictResult)j, listOfResults[j]); }
                                            //add to dictionary
                                            try
                                            {
                                                tempDictionary.Add(subDict, challenge);
                                                Console.WriteLine("{0} Challenge ({1}), successfully imported", subDict, challenge.Type);
                                            }
                                            catch (ArgumentNullException e)
                                            { Game.SetError(new Error(110, string.Format("{0} Challenge not imported due to errors", challenge.Type))); Console.WriteLine(e.Message); }
                                            catch (ArgumentException e)
                                            { Game.SetError(new Error(110, string.Format("{0} Challenge not imported due to errors", challenge.Type))); Console.WriteLine(e.Message); }

                                        }
                                        else
                                        { Game.SetError(new Error(110, string.Format("{0} Challenge not imported due to errors", structChallenge.Type))); }

                                    }
                                    break;
                                default:
                                    Game.SetError(new Error(110, string.Format("Invalid Data \"{0}\" in Challenge Input", cleanTag)));
                                    break;
                            }
                        }
                    }
                    else
                    { newChallenge = false; }
                }
            }
            else
            { Game.SetError(new Error(110, string.Format("File not found (\"{0}\")", fileName))); }
            return tempDictionary;
        }


        /// <summary>
        /// Import Results data
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal Dictionary<int, Result> GetResults(string fileName)
        {
            int dataCounter = 0;
            string cleanTag;
            string cleanToken;
            bool newResult = false;
            bool validData = true;
            int dataInt;
            List<int> listResultID = new List<int>(); //used to pick up duplicate resultID's
            Dictionary<int, Result> dictOfResults = new Dictionary<int, Result>();
            string[] arrayOfResults = ImportDataFile(fileName);
            if (arrayOfResults != null)
            {
                ResultStruct structResult = new ResultStruct();
                //loop imported array of strings
                for (int i = 0; i < arrayOfResults.Length; i++)
                {
                    if (arrayOfResults[i] != "" && !arrayOfResults[i].StartsWith("#"))
                    {
                        //set up for a new house
                        if (newResult == false)
                        {
                            newResult = true;
                            validData = true;
                            //Console.WriteLine();
                            dataCounter++;
                            //new Trait object
                            structResult = new ResultStruct();
                        }

                        //string[] tokens = arrayOfStories[i].Split(':');
                        string[] tokens = arrayOfResults[i].Split(new Char[] { ':', ';' });
                        //strip out leading spaces
                        cleanTag = tokens[0].Trim();
                        //if (cleanTag == "End" || cleanTag == "end") { cleanToken = "1"; } //any value > 0, irrelevant what it is
                        if (cleanTag[0] == '[') { cleanToken = "1"; } //any value > 0, irrelevant what it is
                                                                      //check for random text elements in the file
                        else
                        {
                            try { cleanToken = tokens[1].Trim(); }
                            catch (System.IndexOutOfRangeException)
                            { Game.SetError(new Error(115, string.Format("Invalid token[1] (empty or null) for label \"{0}\"", cleanTag))); cleanToken = ""; }
                        }

                        switch (cleanTag)
                        {
                            case "Name":
                                if (cleanToken.Length == 0)
                                { Game.SetError(new Error(115, string.Format("Empty Name field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                else { structResult.Name = cleanToken; }
                                break;
                            case "Tag":
                                if (cleanToken.Length == 0)
                                { Game.SetError(new Error(115, string.Format("Empty Tag field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                else { structResult.Tag = cleanToken; }
                                break;
                            case "RID":
                                try
                                { structResult.ResultID = Convert.ToInt32(cleanToken); }
                                catch
                                { Game.SetError(new Error(115, string.Format("Invalid input for ResultID {0}, (\"{1}\")", cleanToken, structResult.Name))); validData = false; }
                                //check for duplicate arcID's
                                if (listResultID.Contains(structResult.ResultID))
                                { Game.SetError(new Error(115, string.Format("Duplicate ResultID {0}, (\"{1}\")", cleanToken, structResult.Name))); validData = false; }
                                else { listResultID.Add(structResult.ResultID); }
                                break;
                            case "Type":
                                if (cleanToken.Length == 0)
                                { Game.SetError(new Error(115, string.Format("Empty data field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                else
                                {
                                    switch (cleanToken)
                                    {
                                        case "DataPoint":
                                            structResult.Type = ResultType.DataPoint;
                                            break;
                                        case "GameVar":
                                            structResult.Type = ResultType.GameVar;
                                            break;
                                        case "RelPlyr":
                                            structResult.Type = ResultType.RelPlyr;
                                            break;
                                        case "RelOther":
                                            structResult.Type = ResultType.RelOther;
                                            break;
                                        case "Favour":
                                            structResult.Type = ResultType.Favour;
                                            break;
                                        case "Introduction":
                                            structResult.Type = ResultType.Introduction;
                                            break;
                                        case "Condition":
                                            structResult.Type = ResultType.Condition;
                                            break;
                                        case "Resource":
                                            structResult.Type = ResultType.Resource;
                                            break;
                                        case "Item":
                                            structResult.Type = ResultType.Item;
                                            break;
                                        case "Secret":
                                            structResult.Type = ResultType.Secret;
                                            break;
                                        case "Army":
                                            structResult.Type = ResultType.Army;
                                            break;
                                        case "Event":
                                            structResult.Type = ResultType.Event;
                                            break;
                                        default:
                                            Game.SetError(new Error(115, string.Format("Invalid Input, Type, (\"{0}\")", arrayOfResults[i])));
                                            validData = false;
                                            break;
                                    }
                                }
                                break;
                            case "Data":
                                if (cleanToken.Length > 0)
                                {
                                    try
                                    { structResult.Data = Convert.ToInt32(cleanToken); }
                                    catch
                                    { Game.SetError(new Error(115, string.Format("Invalid Data (Conversion) for {0}", structResult.Name))); validData = false; }
                                }
                                else
                                { Game.SetError(new Error(115, string.Format("Empty data field (Data), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                break;
                            case "Test":
                                if (cleanToken.Length > 0)
                                {
                                    try
                                    { structResult.Test = Convert.ToInt32(cleanToken); }
                                    catch
                                    { Game.SetError(new Error(115, string.Format("Invalid Test (Conversion) for {0}", structResult.Name))); validData = false; }
                                }
                                else
                                { Game.SetError(new Error(115, string.Format("Empty data field (Test), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                break;
                            case "Calc":
                                if (cleanToken.Length == 0)
                                { Game.SetError(new Error(115, string.Format("Empty Calc field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                else
                                {
                                    switch (cleanToken)
                                    {
                                        case "Add":
                                            structResult.Calc = EventCalc.Add;
                                            break;
                                        case "Subtract":
                                            structResult.Calc = EventCalc.Subtract;
                                            break;
                                        case "Random":
                                            structResult.Calc = EventCalc.Random;
                                            break;
                                        case "Equals":
                                            structResult.Calc = EventCalc.Equals;
                                            break;
                                        case "None":
                                            structResult.Calc = EventCalc.None;
                                            break;
                                        default:
                                            Game.SetError(new Error(115, string.Format("Invalid Input, Calc (\"{0}\")", arrayOfResults[i])));
                                            validData = false;
                                            break;
                                    }
                                }
                                break;
                            case "Amount":
                                if (cleanToken.Length > 0)
                                {
                                    try
                                    {
                                        dataInt = Convert.ToInt32(cleanToken);
                                        if (dataInt != 0)
                                        { structResult.Amount = dataInt; }
                                        else
                                        { Game.SetError(new Error(115, string.Format("Invalid Amount Input \"{0}\" (Zero) for {1}", dataInt, structResult.Name))); validData = false; }
                                    }
                                    catch
                                    { Game.SetError(new Error(115, string.Format("Invalid Amount (Conversion) for {0}", structResult.Name))); validData = false; }
                                }
                                else
                                { Game.SetError(new Error(115, string.Format("Empty input field (Amount), record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                break;
                            //optional tags
                            case "DataPoint":
                                if (cleanToken.Length == 0)
                                { Game.SetError(new Error(115, string.Format("Empty DataPoint field, record {0}, {1}, {2}", i, cleanTag, fileName))); validData = false; }
                                else
                                {
                                    switch (cleanToken)
                                    {
                                        case "Invisibility":
                                            structResult.DataPoint = DataPoint.Invisibility;
                                            break;
                                        case "Justice":
                                            structResult.DataPoint = DataPoint.Justice;
                                            break;
                                        case "Legend_Usurper":
                                            structResult.DataPoint = DataPoint.Legend_Usurper;
                                            break;
                                        case "Legend_King":
                                            structResult.DataPoint = DataPoint.Legend_King;
                                            break;
                                        case "Honour_Usurper":
                                            structResult.DataPoint = DataPoint.Honour_Usurper;
                                            break;
                                        case "Honour_King":
                                            structResult.DataPoint = DataPoint.Honour_King;
                                            break;
                                        default:
                                            Game.SetError(new Error(115, string.Format("Invalid Input, DataPoint (\"{0}\")", arrayOfResults[i])));
                                            validData = false;
                                            break;
                                    }
                                }
                                break;
                            //optional -> Condition Results
                            case "conPlyr":
                                //Condition outcomes - if true applies to Player, otherwise NPC actor
                                switch (cleanToken)
                                {
                                    case "Yes":
                                    case "yes":
                                        structResult.ConPlayer = true;
                                        break;
                                    case "No":
                                    case "no":
                                        structResult.ConPlayer = false;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, conPlyr, (\"{0}\")", arrayOfResults[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "conText":
                                //Condition outcomes - name of condition, eg. "Old Age"
                                structResult.ConText = cleanToken;
                                break;
                            case "conSkill":
                                //Condition outcomes - type of skill affected
                                switch (cleanToken)
                                {
                                    case "Combat":
                                    case "combat":
                                        structResult.ConSkill = SkillType.Combat;
                                        break;
                                    case "Wits":
                                    case "wits":
                                        structResult.ConSkill = SkillType.Wits;
                                        break;
                                    case "Charm":
                                    case "charm":
                                        structResult.ConSkill = SkillType.Charm;
                                        break;
                                    case "Treachery":
                                    case "treachery":
                                        structResult.ConSkill = SkillType.Treachery;
                                        break;
                                    case "Leadership":
                                    case "leadership":
                                        structResult.ConSkill = SkillType.Leadership;
                                        break;
                                    case "Touched":
                                    case "touched":
                                        structResult.ConSkill = SkillType.Touched;
                                        break;
                                    default:
                                        Game.SetError(new Error(49, string.Format("Invalid Input, conSkill, (\"{0}\")", arrayOfResults[i])));
                                        validData = false;
                                        break;
                                }
                                break;
                            case "conEffect":
                                //Condition outcomes - effect of condition on skill
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt >= -2 && dataInt <= 2 && dataInt != 0) { structResult.ConEffect = dataInt; }
                                    else
                                    {
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conEffect, (Value outside acceptable range) \"{0}\"", arrayOfResults[i])));
                                        validData = false;
                                    }
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conEffect, (Conversion) \"{0}\"", arrayOfResults[i]))); validData = false; }
                                break;
                            case "conTimer":
                                //Condition outcomes - how long condition applies for
                                try
                                {
                                    dataInt = Convert.ToInt32(cleanToken);
                                    if (dataInt > 0 && dataInt < 1000) { structResult.ConTimer = dataInt; }
                                    else
                                    {
                                        Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conTimer, (Value outside acceptable range) \"{0}\"", arrayOfResults[i])));
                                        validData = false;
                                    }
                                }
                                catch { Game.SetError(new Error(49, string.Format("Invalid Input, Outcome conTimer, (Conversion) \"{0}\"", arrayOfResults[i]))); validData = false; }
                                break;
                            case "[end]":
                            case "[End]":
                                //write record
                                if (validData == true)
                                {
                                    //pass info over to a class instance
                                    Result resultObject = new Result(structResult.ResultID, structResult.Name, structResult.Type, structResult.Data, structResult.Calc, structResult.Amount);
                                    if (String.IsNullOrEmpty(structResult.Tag) == false) { resultObject.Tag = structResult.Tag; }
                                    if (structResult.Test > 0) { resultObject.Test = structResult.Test; }
                                    //special case Results
                                    switch (structResult.Type)
                                    {
                                        case ResultType.DataPoint:
                                            if (structResult.DataPoint > DataPoint.None) { resultObject.DataPoint = structResult.DataPoint; }
                                            break;
                                        case ResultType.Condition:
                                            resultObject.ConPlayer = structResult.ConPlayer;
                                            resultObject.ConText = structResult.ConText;
                                            resultObject.ConSkill = structResult.ConSkill;
                                            resultObject.ConEffect = structResult.ConEffect;
                                            resultObject.ConTimer = structResult.ConTimer;
                                            break;
                                    }
                                    //last datapoint - save object to dictionary
                                    if (dataCounter > 0)
                                    {
                                        try
                                        {
                                            dictOfResults.Add(resultObject.ResultID, resultObject);
                                            Console.WriteLine("RecordID {0} (\"{1}\"), successfully imported", resultObject.ResultID, resultObject.Description);
                                        }
                                        catch (ArgumentNullException e)
                                        {
                                            Game.SetError(new Error(115, string.Format("RecordID {0} (\"{1}\") not imported due to errors",
                                                resultObject.ResultID, resultObject.Description)));
                                            Console.WriteLine(e.Message);
                                        }
                                        catch (ArgumentException e)
                                        {
                                            Game.SetError(new Error(115, string.Format("RecordID {0} (\"{1}\") not imported due to errors",
                                                resultObject.ResultID, resultObject.Description)));
                                            Console.WriteLine(e.Message);
                                        }
                                    }
                                    else { Game.SetError(new Error(115, "Invalid Input, resultObject")); }
                                }
                                else
                                { Game.SetError(new Error(115, string.Format("Result, (\"{0}\" ResultID {1}), not Imported", structResult.Name, structResult.ResultID))); }
                                break;
                            default:
                                Game.SetError(new Error(115, string.Format("Invalid Input, CleanTag \"{0}\", \"{1}\"", cleanTag, structResult.Name)));
                                break;
                        }

                    }
                    else { newResult = false; }
                }
            }
            else
            { Game.SetError(new Error(115, string.Format("File not found (\"{0}\")", fileName))); }
            return dictOfResults;
        }




        //methods above here
    }
}
