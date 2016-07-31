using System;
using System.Collections.Generic;


namespace Next_Game
{
    //
    // Base class ---
    //
    class House
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public string LocName { get; set; }
        public int HouseID { get; set; } //unique to Great House (allocated by Network.cs)
        public int LocID { get; set; } //unique to location
        public int RefID { get; set; } //unique to house (great or minor)
        public int ArchetypeID { get; set; }
        public int Branch { get; set; }

        public House()
        { }        
    }

    //
    // Great house ---
    //
    class MajorHouse : House
    {
        private List<int> listLordLocations;
        private List<int> listHousesToCapital; //unique houses (HID), ignoring special locations
        private List<int> listHousesToConnector; //unique houses (HID), ignoring special locations

        public MajorHouse()
        {
            listLordLocations = new List<int>();
            listHousesToCapital = new List<int>();
            listHousesToConnector = new List<int>();
        }

        /// <summary>
        /// add a location to list of house controlled locations
        /// </summary>
        /// <param name="locID"></param>
        public void AddLordLocations(int locID)
        { listLordLocations.Add(locID); }

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
        public List<int> GetLords()
        { return listLordLocations; }

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
