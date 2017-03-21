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
        private bool overide; //if true this challenge is used to overide data in another challenge
        public ConflictType Type { get; set; }
        public ConflictCombat CombatType { get; set; }
        public ConflictSocial SocialType { get; set; }
        public ConflictStealth StealthType { get; set; }
        string[] arrayStrategies;
        string[] arrayOutcomes;
        SkillType[] arraySkills;
        List<List<int>> listResults;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        public Challenge(ConflictType type, ConflictCombat combat, ConflictSocial social, ConflictStealth stealth)
        {
            if (type > ConflictType.None)
            {
                this.Type = type;
                CombatType = combat;
                SocialType = social;
                StealthType = stealth;
                arrayStrategies = new string[6]; //Plyr Strategies Aggressive/Balanced/Defensive 0/1/2, Opponent Strategies, same 3/4/5
                arrayOutcomes = new string[7]; //Minor Win/Win/Major Win 0/1/2, Minor Loss/Loss/Major Loss 3/4/5, No Result 6
                arraySkills = new SkillType[3]; //Primary skill 0, Secondary skills 1/2
                listResults = new List<List<int>>();
                //create a blank list of lists 6 entries long (one for each ConflictResult enum)
                for(int i = 0; i < (int)ConflictResult.Count; i++)
                {
                    List<int> subList = new List<int>() { 0 };
                    listResults.Add(subList);
                }
            }
            else
            { Game.SetError(new Error(106, "Invalid ConflictType input (\"None\")")); }
        }

        /// <summary>
        /// Copy constructor, used for copying overide challenge data from Outcome to a new Special mode challenge in dictChallenges
        /// </summary>
        /// <param name="other"></param>
        public Challenge(Challenge other)
        {

            Type = other.Type;
            CombatType = other.CombatType;
            SocialType = other.SocialType;
            StealthType = other.StealthType;
            //initialise collections prior to copying otherwise a null collection error occurs for each attempt
            arrayStrategies = new string[6];
            arrayOutcomes = new string[7];
            arraySkills = new SkillType[3];
            listResults = new List<List<int>>();
            for (int i = 0; i < (int)ConflictResult.Count; i++)
            {
                List<int> subList = new List<int>() { 0 };
                listResults.Add(subList);
            }
            other.arrayStrategies.CopyTo(arrayStrategies, 0);
            other.arrayOutcomes.CopyTo(arrayOutcomes, 0);
            other.arraySkills.CopyTo(arraySkills, 0);
            for(int i = 0; i < (int)ConflictResult.Count; i++)
            {
                List<int> subList = new List<int>();
                subList.AddRange(other.listResults[i]);
                listResults[i].Clear();
                listResults[i].AddRange(subList);
            }
        }

        /// <summary>
        /// If true indicates that this challenge is used to overide data in whatever other challenge is selected
        /// </summary>
        /// <param name="flag"></param>
        public void SetOveride(bool flag)
        { overide = flag; }

        public bool GetOveride()
        { return overide; }

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
                else
                { Game.SetError(new Error(107, "Invalid input (array incorrect length)")); }
            }
            else
            { Game.SetError(new Error(107, "Invalid input (Null array)")); }
        }

        /// <summary>
        /// Clear then copy new data to array of Outcomes. 
        /// </summary>
        /// <param name="tempArray">tempArray[7], Minor Win/Win/Major Win 0/1/2, Minor Loss/Loss/Major Loss 3/4/5, No Result 6</param>
        public void SetOutcomes(string[] tempArray)
        {
            if (tempArray != null)
            {
                if (tempArray.Length == 7)
                {
                    Array.Clear(arrayOutcomes, 0, arrayOutcomes.Length);
                    Array.Copy(tempArray, arrayOutcomes, tempArray.Length);
                }
                else
                { Game.SetError(new Error(108, "Invalid input (array incorrect length)")); }
            }
            else
            { Game.SetError(new Error(108, "Invalid input (Null array)")); }
        }

        /// <summary>
        /// Clear then copy new data to array of Skills
        /// </summary>
        /// <param name="tempArray"></param>
        public void SetSkills(SkillType[] tempArray)
        {
            if (tempArray != null)
            {
                if (tempArray.Length == 3)
                {
                    Array.Clear(arraySkills, 0, arraySkills.Length);
                    Array.Copy(tempArray, arraySkills, tempArray.Length);
                }
                else
                { Game.SetError(new Error(109, "Invalid input (array incorrect length)")); }
            }
            else
            { Game.SetError(new Error(109, "Invalid input (Null array)")); }
        }

        /// <summary>
        /// Add new results to appropriate sublist which is cleared first
        /// </summary>
        /// <param name="result">Specify the correct sublist</param>
        /// <param name="tempList">list of result ID's</param>
        public void SetResults(ConflictResult result, List<int> tempList)
        {
            if (tempList != null)
            {
                listResults[(int)result].Clear();
                listResults[(int)result].AddRange(tempList);
            }
            else
            { Game.SetError(new Error(112, "Invalid Input List (null)")); }
        }


        public string[] GetStrategies()
        { return arrayStrategies; }

        public string[] GetOutcomes()
        { return arrayOutcomes; }

        public SkillType[] GetSkills()
        { return arraySkills; }

        /// <summary>
        /// return specific sublist of result ID's
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public List<int> GetResults(ConflictResult result)
        { return listResults[(int)result]; }



        //place methods above here
    }
}
