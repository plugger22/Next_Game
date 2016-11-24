using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public class Situation
    {
        private static int SituationIndex = 1; //provides  a unique ID to each situation 
        public int SitID { get; set; }
        public string Name { get; set; }
        public int SitNum { get; set; } //'0' for when card is Played and '1' for when card Ignored
        public int Defender { get; set; } //'1' if written from POV of Player as defender, if '-1' then opponent as defender. Only applies to first situation (def. adv.)
        public int Data { get; set; } //multi-purpose data points, ignore if not needed, eg. able to specify terrain type for def adv. card
        public ConflictType Type { get; set; } //applies to first two situations
        public ConflictCombat Type_Combat { get; set; } //applies to first two situations
        public ConflictSocial Type_Social { get; set; } //applies to first two situations
        public ConflictOther Type_Other { get; set; } //applies to first two situations
        public ConflictState State { get; set; } //only applies to game specific situations
        public ConflictSpecial Special { get; set; } //decision derived special situations
        private List<string> listGood;
        private List<string> listBad;


        public Situation()
        { }

        /// <summary>
        /// default constructor for the first two situations
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="sitnum"></param>
        /// <param name="combatType"></param>
        /// <param name="socialType"></param>
        /// <param name="otherType"></param>
        public Situation(string name, ConflictType type, int sitnum, ConflictCombat combatType = ConflictCombat.None, ConflictSocial socialType = ConflictSocial.None, ConflictOther otherType = ConflictOther.None)
        {
            SitID = SituationIndex++;
            listGood = new List<string>();
            listBad = new List<string>();
            this.Name = name;
            this.SitNum = sitnum;
            this.Type = type;
            Type_Combat = combatType;
            Type_Social = socialType;
            Type_Other = otherType;
        }

        /// <summary>
        /// default constructor for a game specific situation which is indepedent of conflict type/subtype (they can apply to all)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <param name="sitnum"></param>
        public Situation(string name, ConflictState state, int sitnum)
        {
            SitID = SituationIndex++;
            listGood = new List<string>();
            listBad = new List<string>();
            this.Name = name;
            this.SitNum = sitnum;
            this.State = state;
            Type = ConflictType.None;
            Type_Combat = ConflictCombat.None;
            Type_Social = ConflictSocial.None;
            Type_Other = ConflictOther.None;
        }

        /// <summary>
        /// default constructor for special situations which are independet of conflict type/substype
        /// </summary>
        /// <param name="name"></param>
        /// <param name="special"></param>
        /// <param name="sitnum"></param>
        public Situation(string name, ConflictSpecial special, int sitnum)
        {
            SitID = SituationIndex++;
            listGood = new List<string>();
            listBad = new List<string>();
            this.Name = name;
            this.SitNum = sitnum;
            this.Special = special;
            Type = ConflictType.None;
            Type_Combat = ConflictCombat.None;
            Type_Social = ConflictSocial.None;
            Type_Other = ConflictOther.None;
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
