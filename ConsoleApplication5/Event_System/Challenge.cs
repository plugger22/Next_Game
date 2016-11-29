using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    /// <summary>
    /// Contains all relevant data for each specific type of Challenge
    /// </summary>
    class Challenge
    {
        public ConflictType Type { get; set; }
        public ConflictCombat CombatType { get; set; }
        public ConflictSocial SocialType { get; set; }
        public ConflictStealth StealthType { get; set; }
        string[] arrayStrategies;
        string[] arrayOutcomes;
        string[] arraySkills;


        public Challenge(ConflictType type, int subType)
        {
            if (type > ConflictType.None)
            {
                this.Type = type;
                if (subType > 0)
                {
                    arrayStrategies = new string[6]; //Plyr Strategies Aggressive/Balanced/Defensive 0/1/2, Opponent Strategies, same 3/4/5
                    arrayOutcomes = new string[6]; //Minor Win/Win/Major Win 0/1/2, Minor Loss/Loss/Major Loss 3/4/5
                    arraySkills = new string[3]; //Primary skill 0, Secondary skills 1/2
                    //set subtype
                    switch (type)
                    {
                        case ConflictType.Combat:
                            CombatType = (ConflictCombat)subType;
                            break;
                        case ConflictType.Social:
                            SocialType = (ConflictSocial)subType;
                            break;
                        case ConflictType.Stealth:
                            StealthType = (ConflictStealth)subType;
                            break;
                    }
                }
                else
                { Game.SetError(new Error(106, "Invalid subType input (less than Zero)")); }
            }
            else
            { Game.SetError(new Error(106, "Invalid ConflictType input (\"None\")")); }
        }


        /// <summary>
        /// Clear then copy new data to array of Strategies
        /// </summary>
        /// <param name="tempArray"></param>
        public void SetStrategies(string[] tempArray)
        {
            if (tempArray != null)
            {
                if (tempArray.Length == 6)
                {
                    Array.Clear(arrayStrategies, 0, arrayStrategies.Length);
                    Array.Copy(tempArray, arrayStrategies, tempArray.Length);
                }
            }
        }


        //place methods above here
    }
}
