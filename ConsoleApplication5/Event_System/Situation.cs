using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    class Situation
    {
        private static int SituationIndex = 1; //provides  a unique ID to each situation 
        public int SitID { get; set; }
        public string Name { get; set; }
        public ConflictType Type { get; set; }
        public CombatType Type_Combat { get; set; }
        public SocialType Type_Social { get; set; }
        public OtherType Type_Other { get; set; }
        public int SitNum { get; set; }
        public int Data1 { get; set; }
        public int Data2 { get; set; }
        private List<string> listGood;
        private List<string> listBad;

        public Situation()
        {
            listGood = new List<string>();
            listBad = new List<string>();
        }

        /// <summary>
        /// default constructor;
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="sitnum"></param>
        /// <param name="combatType"></param>
        /// <param name="socialType"></param>
        /// <param name="otherType"></param>
        public Situation(string name, ConflictType type, int sitnum, CombatType combatType = CombatType.None, SocialType socialType = SocialType.None, OtherType otherType = OtherType.None)
        {
            SitID = SituationIndex++;
            this.Name = name;
            this.Type = type;
            this.SitNum = sitnum;
            Type_Combat = combatType;
            Type_Social = socialType;
            Type_Other = otherType;
            listGood = new List<string>();
            listBad = new List<string>();
        }

        /// <summary>
        /// list of Good outcomes - immersion text
        /// </summary>
        /// <param name="tempList"></param>
        public void SetGood(List<string> tempList)
        {
            if (tempList != null && tempList.Count() > 0)
            {
                listGood.Clear();
                listGood.AddRange(tempList);
            }
            else { Game.SetError(new Error(99, "Invalid Input (null or empty list")); }
        }

        /// <summary>
        /// list of Bad outcomes - immersion text
        /// </summary>
        /// <param name="tempList"></param>
        public void SetBad(List<string> tempList)
        {
            if (tempList != null && tempList.Count() > 0)
            {
                listBad.Clear();
                listBad.AddRange(tempList);
            }
            else { Game.SetError(new Error(100, "Invalid Input (null or empty list")); }
        }


        public List<string> GetGood()
        { return listGood; }

        public List<string> GetBad()
        { return listBad; }

        //add methods above here
    }
}
