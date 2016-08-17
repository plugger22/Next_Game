using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum TraitType {None, Combat, Wits, Charm, Treachery, Leadership, Administration}
    public enum TraitSex {All, Male, Female}

    //Actor Traits
    class Trait
    {
        private static int traitIndex = 1;
        public int TraitID { get; set; }
        public TraitType Type { get; set; } = TraitType.None;
        public TraitSex Sex { get; set; } = TraitSex.All; //applies to males or females?
        public string Name { get; set; } = "Unknown";
        public int Effect { get; set; } = 0;
        private List<string> Nicknames;

        public Trait()
        {
            TraitID = traitIndex++;
            Nicknames = new List<string>();
        }

        /// <summary>
        /// add nicknames to list of possibles
        /// </summary>
        /// <param name="listOfNames"></param>
        public void SetNicknames(List<string> listOfNames)
        { Nicknames.AddRange(listOfNames); }
    }
}
