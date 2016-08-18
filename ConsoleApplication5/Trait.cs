using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum TraitType {None, Combat, Wits, Charm, Treachery, Leadership, Administration, Count} //Note: Count should be last, None should be first
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
        public int Chance { get; set; } = 50;
        private List<string> listOfNicknames;

        public Trait()
        { listOfNicknames = new List<string>(); }

        public Trait(string name, TraitType skill, int effect, TraitSex sex, int chance, List<string> nicknames)
        {
            TraitID = traitIndex++;
            this.Name = name;
            Type = skill;
            this.Effect = effect;
            this.Sex = sex;
            this.Chance = chance;
            listOfNicknames = new List<string>(nicknames);
        }
    }
}
