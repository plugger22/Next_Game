using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum TraitType {None, Combat, Wits, Charm, Treachery, Leadership, Touched, Count} //Note: Count should be last, None should be first
    public enum TraitSex {All, Male, Female, Count} //who does it affect? Either Male or Female plus All for everybody
    public enum TraitAge {Five, Fifteen, Count} //what age the trait is revealed and has an effect, Note: Count should be at end

    //Actor Traits
    class Trait
    {
        private static int traitIndex = 1;
        public int TraitID { get; set; }
        public TraitType Type { get; set; } = TraitType.None;
        public TraitSex Sex { get; set; } = TraitSex.All; //applies to males or females?
        public TraitAge Age { get; set; } = TraitAge.Fifteen; //what age does the trait kick in?
        public string Name { get; set; } = "Unknown";
        public string Conflict { get; set; } = "Unknown"; //generic text used for a conflict card, eg. Upset Stomach -> "You feel an urgent need to empty your bowels"
        public int Effect { get; set; } = 0;
        public int Chance { get; set; } = 50;
        private List<string> listOfNicknames;

        public Trait()
        { listOfNicknames = new List<string>(); }

        public Trait(string name, TraitType skill, int effect, TraitSex sex, TraitAge age, int chance, List<string> nicknames)
        {
            TraitID = traitIndex++;
            this.Name = name;
            Type = skill;
            this.Effect = effect;
            this.Sex = sex;
            this.Age = age;
            this.Chance = chance;
            listOfNicknames = new List<string>(nicknames);
        }

        public List<string> GetNickNames()
        { return listOfNicknames; }
    }
}
