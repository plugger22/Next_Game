using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// Keeps track of all key game lore (backstory)
    /// </summary>
    public class Lore
    {
        //HouseID's
        public int RoyalHouseOld { get; set; }
        public int RoyalHouseNew { get; set; }
        public int RoyalHouseCurrent { get; set; }
        //Royal Family and retainers
        private List<Passive> listOfOldRoyals; //at time of revolt
        private List<Passive> listOfNewRoyals; //at time of taking power
        public Noble OldKing { get; set; }
        public Noble OldQueen { get; set; }
        public Noble OldHeir { get; set; }
        public Noble NewKing { get; set; }
        public Noble NewQueen { get; set; }
        public Noble NewHeir { get; set; }
        //each list a series of sentences that make a paragraph about the topic
        private List<string> listOldKingRule;
        private List<string> listRevoltBackStory;
        private List<string> listUprising;
        private List<string> listRoyalFamilyFate;
        private List<string> listNewKingRule;
        private List<string> listChanges; //location's, bannerlords etc.

        public Lore()
        {
            listOfOldRoyals = new List<Passive>();
            listOldKingRule = new List<string>();
            listRevoltBackStory = new List<string>();
            listUprising = new List<string>();
            listRoyalFamilyFate = new List<string>();
            listNewKingRule = new List<string>();
            listChanges = new List<string>();
        }

        internal void SetListOfOldRoyals(List<Passive> listRoyals)
        { listOfOldRoyals.AddRange(listRoyals); }

        /// <summary>
        /// generates reason and populates lore lists
        /// </summary>
        internal void CreateOldKingBackStory()
        {

        }
    }
}
