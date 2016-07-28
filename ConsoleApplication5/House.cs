using System;
using System.Collections.Generic;


namespace Next_Game
{
    class House
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public string CapitalName { get; set; }
        public int HouseID { get; set; }
        public int CapitalLocID { get; set; }
        public int ArchetypeID { get; set; }
        public int Branch { get; set; }
        private List<int> listLocations;
        private List<int> listHousesToCapital;
        private List<int> listHousesToConnector;

        public House()
        {
            listLocations = new List<int>();
            listHousesToCapital = new List<int>();
            listHousesToConnector = new List<int>();
        }
        
        /// <summary>
        /// add a location to list of house controlled locations
        /// </summary>
        /// <param name="locID"></param>
        public void AddLocation(int locID)
        { listLocations.Add(locID); }
    }
}
