using System;
using System.Collections.Generic;
using System.Linq;


namespace Next_Game
{
    public enum KingLoyalty {None, Old_King, New_King}

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
        public int ArcID { get; set; } //unique to archetype
        public int Branch { get; set; }
        public int MenAtArms { get; set; }
        public int LordID { get; set; } //actorID of noble Lord currently in charge of house
        public KingLoyalty Loyalty_AtStart { get; set; }
        public KingLoyalty Loyalty_Current { get; set; }
        private List<int> listOfFirstNames; //contains ID #'s (listOfMaleFirstNames index) of all first names used by males within the house (eg. 'Eddard Stark II')
        private List<int> listOfSecrets;
        private List<int> listOfEvents;

        /// <summary>
        /// default constructor
        /// </summary>
        public House()
        {
            listOfFirstNames = new List<int>();
            listOfSecrets = new List<int>();
            listOfEvents = new List<int>();
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

        internal void SetEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(56, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetEvents()
        { return listOfEvents; }

        internal List<int> GetSecrets()
        { return listOfSecrets; }

        internal void SetSecrets(List<int> tempSecrets)
        {
            if (tempSecrets != null)
            { listOfSecrets.Clear();  listOfSecrets.AddRange(tempSecrets); }
            else
            { Game.SetError(new Error(57, "Invalid list of Secrets input (null)")); }
        }

    }

    //
    // Great house ---
    //
    class MajorHouse : House
    {
        private List<int> listLordLocations; //locID of all bannerlords
        private List<int> listBannerLords; //refID of all bannerlords
        private List<int> listHousesToCapital; //unique houses (HID), ignoring special locations
        private List<int> listHousesToConnector; //unique houses (HID), ignoring special locations
        

        public MajorHouse()
        {
            listLordLocations = new List<int>();
            listHousesToCapital = new List<int>();
            listHousesToConnector = new List<int>();
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
        public void AddHousesToCapital(int houseID)
        {
            //check houseID isn't already in list (only unique HouseID's are stored)
            if (houseID > 0)
            {
                if (listHousesToCapital.Contains(houseID) == false)
                { listHousesToCapital.Add(houseID); }
            }
        }

        

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

        public List<int> GetHousesToCapital()
        { return listHousesToCapital; }

        public List<int> GetHousesToConnector()
        { return listHousesToConnector; }

        public void SetBannerLords(List<int> tempList)
        { listBannerLords.Clear();  listBannerLords.AddRange(tempList); }

        public void SetHousesToCapital(List<int> tempList)
        { listHousesToCapital.Clear(); listHousesToCapital.AddRange(tempList); }

        public void SetHousesToConnector(List<int> tempList)
        { listHousesToConnector.Clear(); listHousesToConnector.AddRange(tempList); }

        public void SetLordLocations(List<int> tempList)
        { listLordLocations.Clear(); listLordLocations.AddRange(tempList); }


    }

    //
    //Bannerlords ---
    //
    class MinorHouse : House
    {
        public MinorHouse()
        { }

    }
}
