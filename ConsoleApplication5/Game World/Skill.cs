using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum SkillType {None, Combat, Wits, Charm, Treachery, Leadership, Touched, Count} //Note: Count should be last, None should be first
    public enum SkillSex {All, Male, Female, Count} //who does it affect? Either Male or Female plus All for everybody
    public enum SkillAge {Five, Fifteen, Count} //what age the trait is revealed and has an effect, Note: Count should be at end

    //Actor Skills
    class Skill : IEquatable<Skill>
    {
        private static int skillIndex = 1;
        public int SkillID { get; set; }
        public SkillType Type { get; set; } = SkillType.None;
        public SkillSex Sex { get; set; } = SkillSex.All; //applies to males or females?
        public SkillAge Age { get; set; } = SkillAge.Fifteen; //what age does the trait kick in?
        public string Name { get; set; } = "Unknown";
        public string Conflict { get; set; } = "Unknown"; //generic text used for a conflict card, eg. Upset Stomach -> "You feel an urgent need to empty your bowels"
        public int Effect { get; set; } = 0;
        public int Chance { get; set; } = 50;
        private List<string> listOfNicknames;

        public Skill()
        { listOfNicknames = new List<string>(); }

        public Skill(string name, SkillType skill, int effect, SkillSex sex, SkillAge age, int chance, List<string> nicknames)
        {
            SkillID = skillIndex++;
            this.Name = name;
            Type = skill;
            this.Effect = effect;
            this.Sex = sex;
            this.Age = age;
            this.Chance = chance;
            listOfNicknames = new List<string>(nicknames);
        }

        /// <summary>
        /// IEquatable
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Skill skill = obj as Skill;
            if (skill == null) return false;
            else return Equals(skill);
        }

        /// <summary>
        /// IEquatable
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        { return SkillID; }

        /// <summary>
        /// IEquatable (match on Trait ID)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals (Skill other)
        {
            if (other == null) return false;
            return (this.SkillID.Equals(other.SkillID));
        }

        public List<string> GetNickNames()
        { return listOfNicknames; }
    }
}
