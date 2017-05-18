using System;
using System.Collections.Generic;
using System.Linq;


namespace Next_Game
{
    public enum KingLoyalty { None, Old_King, New_King }
    public enum HouseSpecial { None, Inn }
    public enum CastleDefences { None, Minimal, Weak, Average, Strong, Formidable }

    //
    // Base class
    //
    class House
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public string LocName { get; set; }
        public int HouseID { get; set; } = 0; //unique to Great House (allocated by Network.UpdateHouses)
        public int LocID { get; set; } //unique to location
        public int RefID { get; set; } //unique to house (great or minor)
        public int ArcID { get; set; } //unique archetype for house (specified in House import files)
        public int Branch { get; set; } //branch # House on -> 0 is Capital, 1 is North, 2 is East, 3 is South, 4 is West
        public int MenAtArms { get; set; }
        public int CastleWalls { get; set; } //strength of castle walls (1 to 5)
        public int LordID { get; set; } //actorID of noble Lord currently in charge of house
        public int Resources { get; set; } //same as actor resources (0 to 5). Head of house has access to this level of resources.
        public int SafeHouse { get; set; } //Safe House present if > 0. Number represents how many stars (max 5)
        public bool FriendsAndEnemies { get; private set; } //if true, then display number of friends and enemies at a house
        public KingLoyalty Loyalty_AtStart { get; set; }
        public KingLoyalty Loyalty_Current { get; set; }
        public HouseSpecial Special { get; set; }
        private List<int> listOfFirstNames; //contains ID #'s (listOfMaleFirstNames index) of all first names used by males within the house (eg. 'Eddard Stark II')
        private List<int> listOfSecrets;
        private List<int> listOfFollowerEvents;
        private List<int> listOfPlayerEvents;
        private List<int> listOfRumours;
        private List<Relation> listOfRelations; //relationships (records) with other houses (can have multiple relations with another house)
        private Dictionary<int, int> dictCurrentRelations; //current Relationship levels, key is RefID, value is current Rel lvl

        /// <summary>
        /// default constructor
        /// </summary>
        public House()
        {
            listOfFirstNames = new List<int>();
            listOfSecrets = new List<int>();
            listOfRumours = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listOfRelations = new List<Relation>();
            dictCurrentRelations = new Dictionary<int, int>();
            FriendsAndEnemies = false;
        }



        /// <summary>
        /// adds ID to list of names and returns # of like names in list
        /// </summary>
        /// <param name="nameID"></param>
        /// <returns></returns>
        public int AddName(int nameID)
        {
            int numOfLikeNames = 1;
            listOfFirstNames.Add(nameID);
            numOfLikeNames = listOfFirstNames.Count(i => i.Equals(nameID));
            return numOfLikeNames;
        }

        internal void SetFollowerEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfFollowerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(56, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetFollowerEvents()
        { return listOfFollowerEvents; }

        internal int GetNumFollowerEvents()
        { return listOfFollowerEvents.Count; }

        internal void SetPlayerEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfPlayerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(56, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetPlayerEvents()
        { return listOfPlayerEvents; }

        internal int GetNumPlayerEvents()
        { return listOfPlayerEvents.Count; }

        internal List<int> GetSecrets()
        { return listOfSecrets; }

        internal void SetSecrets(List<int> tempSecrets)
        {
            if (tempSecrets != null)
            { listOfSecrets.Clear(); listOfSecrets.AddRange(tempSecrets); }
            else
            { Game.SetError(new Error(57, "Invalid list of Secrets input (null)")); }
        }

        internal List<int> GetRumours()
        { return listOfRumours; }

        internal void AddRumour(int rumourID)
        {
            if (rumourID > 0)
            { listOfRumours.Add(rumourID); }
            else { Game.SetError(new Error(267, "Invalid rumourID (zero, or less)")); }
        }

        /// <summary>
        /// Can be set directly but used to generate a msg
        /// </summary>
        /// <param name="status"></param>
        public void SetFriendsAndEnemies(bool status)
        {
            FriendsAndEnemies = status;
            Game.logTurn?.Write($"[Notification] House {Name} FriendsAndEnemies status changed to \"{status}\"");
        }

        /// <summary>
        /// import a list of Relations and add to House Relations List (auto  updates current Rel dict)
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetRelations(List<Relation> tempList)
        {
            if (tempList != null && tempList.Count > 0)
            {
                listOfRelations.AddRange(tempList);
                //update current rel dictionary
                foreach(var relation in tempList)
                {  UpdateRelations(relation.RefID, relation.Change);  }
            }
            else { Game.SetError(new Error(132, "Invalid List of Relations Input (null or empty)")); }
        }

        /// <summary>
        /// Add a relationship record (auto  updates current Rel dict)
        /// </summary>
        /// <param name="relation"></param>
        internal void AddRelations(Relation relation)
        {
            if (relation != null)
            {
                listOfRelations.Add(relation);
                //update current Rel dict
                UpdateRelations(relation.RefID, relation.Change);
            }
            else { Game.SetError(new Error(135, "Invalid Relation (null)")); }
        }

        internal List<Relation> GetRelations()
        { return listOfRelations; }

        /// <summary>
        /// Return a sorted (recent -> distant) list of Relations that apply to the House with the input refID. Returns list, (count 0), if none
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        internal List<Relation> GetSpecificRelations(int refID) //NOT SURE IF THIS method WORKS
        {
            List<Relation> tempList = new List<Relation>();
            if (listOfRelations.Count > 0)
            {
                IEnumerable<Relation> houseRels =
                    from relation in listOfRelations
                    where relation.RefID == refID
                    orderby relation.TrackerID descending
                    select relation;
                tempList = houseRels.ToList();
            }
            return tempList;
        }

        /// <summary>
        /// updates current rel dict with latest data, creates new entry if refID doesn't exist. All relationships assumed to start at a base of zero. Auto called whenever a new Relationship record is added
        /// </summary>
        /// <param name="refID"></param>
        /// <param name="change"></param>
        private void UpdateRelations(int refID, int change)
        {
            int relLevel = 0;
            if (dictCurrentRelations.ContainsKey(refID))
            {
                relLevel = dictCurrentRelations[refID];
                //update with change amount to current Rel level
                dictCurrentRelations[refID] = relLevel + change;
            }
            else
            {
                //no entry found, create new one
                try
                { dictCurrentRelations.Add(refID, change); }
                catch (ArgumentException)
                { Game.SetError(new Error(241, $"Invalid RefID \"{refID}\" (duplicate record) -> Relationship not updated")); }
            }
        }

        /// <summary>
        /// looks up house in Dictionary and returns current rel level, '0' if not found (neutral)
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        internal int GetCurrentRelationship(int refID)
        {
            int relLevel = 0;
            if (dictCurrentRelations.ContainsKey(refID))
            { relLevel = dictCurrentRelations[refID];  }
            return relLevel;
        }

    }
    

    //
    // Major (Great) house ---
    //
    class MajorHouse : House
    {
        private List<int> listLordLocations; //locID of all bannerlords
        private List<int> listBannerLords; //refID of all bannerlords
        //private List<int> listHousesToCapital; //unique houses (HID), ignoring special locations -> NOT hooked up
        //private List<int> listHousesToConnector; //unique houses (HID), ignoring special locations -> NOT hooked up
        

        public MajorHouse()
        {
            listLordLocations = new List<int>();
            //listHousesToCapital = new List<int>();  NOT hooked up
            //listHousesToConnector = new List<int>(); NOT hooked up
            listBannerLords = new List<int>();
        }


        /// <summary>
        /// add a location to list of house controlled locations
        /// </summary>
        /// <param name="locID"></param>
        public void AddBannerLordLocation(int locID)
        { listLordLocations.Add(locID); }

        public void AddBannerLord(int refID)
        { listBannerLords.Add(refID); }

        /// <summary>
        /// add a house ID to list of unique houses to capital
        /// </summary>
        /// <param name="houseID"></param>
        /*public void AddHousesToCapital(int houseID)
        {
            //check houseID isn't already in list (only unique HouseID's are stored)
            if (houseID > 0)
            {
                if (listHousesToCapital.Contains(houseID) == false)
                { listHousesToCapital.Add(houseID); }
            }
        }*/

        

        /// <summary>
        /// returns list of Lords (subsidary bannerlord locations)
        /// </summary>
        /// <returns></returns>
        public List<int> GetBannerLordLocations()
        { return listLordLocations; }

        public List<int> GetBannerLords()
        { return listBannerLords; }

        public int GetNumBannerLords()
        { return listLordLocations.Count; }

        /*public List<int> GetHousesToCapital()
        { return listHousesToCapital; }

        public List<int> GetHousesToConnector()
        { return listHousesToConnector; }

        public void SetHousesToCapital(List<int> tempList)
        { listHousesToCapital.Clear(); listHousesToCapital.AddRange(tempList); }

        public void SetHousesToConnector(List<int> tempList)
        { listHousesToConnector.Clear(); listHousesToConnector.AddRange(tempList); }*/

        public void SetLordLocations(List<int> tempList)
        { listLordLocations.Clear(); listLordLocations.AddRange(tempList); }

        public void SetBannerLords(List<int> tempList)
        { listBannerLords.Clear();  listBannerLords.AddRange(tempList); }


    }

   /// <summary>
   /// Bannerlords
   /// </summary>
    class MinorHouse : House
    {
        public MinorHouse()
        { }

    }

    /// <summary>
    /// Special House class 
    /// </summary>
    class SpecialHouse : House
    {
        public SpecialHouse()
        { }
    }

    /// <summary>
    /// Inn's
    /// </summary>
    class InnHouse : SpecialHouse
    {
        private List<int> listOfFollowers; //ActID of followers in the Inn, available for recruitment.

        public InnHouse()
        {
            listOfFollowers = new List<int>();
            Special = HouseSpecial.Inn;
            CastleWalls = 0;
        }

        /// <summary>
        /// adds a follower to the listOfFollowers available for recruitment
        /// </summary>
        /// <param name="actID"></param>
        public void AddFollower(int actID)
        {
            if (actID > 0 && actID < 10)
            {
                listOfFollowers.Add(actID);
                Game.logStart?.Write($"Inn \"{Name}\" -> Follower ActID {actID} added");
            }
        }

        public List<int> GetFollowers()
        { return listOfFollowers; }

        public int GetNumFollowers()
        { return listOfFollowers.Count(); }


        /*/// <summary>
        /// used to update list (passed to world.InitialiseActiveActors, entries possibly deleted, passed back by reference as newly updated version)
        /// </summary>
        /// <param name="listOfUpdatedFollowers"></param>
        public void UpdateFollowers(List<int> listOfUpdatedFollowers)
        {
            if (listOfUpdatedFollowers != null)
            {
                if (listOfUpdatedFollowers.Count > 0)
                {
                    listOfFollowers.Clear();
                    listOfFollowers.AddRange(listOfUpdatedFollowers);
                }
                else { Game.SetError(new Error(226, "Invalid listOfUpdatedFollowers input (no records)")); }
            }
            else { Game.SetError(new Error(226, "Invalid listOfUpdatedFollowers input (null)")); }
        }*/

        /// <summary>
        /// Removes a follower from the list I'cause they've been recruited)
        /// </summary>
        /// <param name="actID"></param>
        public void RemoveFollower(int actID)
        {
            if (actID > 1 && actID < 10)
            {
                bool removed = false;
                for(int i = 0; i < listOfFollowers.Count; i++)
                {
                    if (listOfFollowers[i] == actID)
                    {
                        listOfFollowers.RemoveAt(i);
                        Game.logTurn?.Write($"Follower ActID {actID} has been removed from listOfFollowers at \"{Name}\"");
                        removed = true;
                        break;
                    }
                }
                if (removed == false)
                { Game.SetError(new Error(226, $"Follower, ActID {actID} wasn't found in {Name}'s listOfFollowers -> Not removed")); }
            }
            else { Game.SetError(new Error(226, $"Invalid actID \"{actID}\"")); }
        }
    }
}
