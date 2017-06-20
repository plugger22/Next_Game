using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;

namespace Next_Game
{
    public enum GameVar  
    {
        //NOTE: KEEP order the same as in arrayOfGameVar, eg. enum 1 is arrayOfGameVars[1]
        None,
        Promises_Num,
        View_Index,
        New_RelRumours,
        View_Rollover,
        Account_Timer,
        Inquisitor_Budget,
        Lifestyle_Budget,
        Corruption_Factor,
        //keep as last, DO NOT change order of GameVars
        Count
    }

    /// <summary>
    /// Variables that are tracked game wide, mostly used by Events
    /// </summary>
    public class Variable
    {
        static Random rnd;
        private readonly GameVar[] arrayOfGameVars; //holds enums which provide indexes for arrayOfVariables
        private readonly int[] arrayOfVariables; //data which that's used in code, index is a Global enum

        public Variable(int seed)
        {
            rnd = new Random(seed);
            arrayOfGameVars = new GameVar[(int)GameVar.Count];
            arrayOfVariables = new int[(int)GameVar.Count];
            //set up array of Globals
            arrayOfGameVars[1] = GameVar.Promises_Num;
            arrayOfGameVars[2] = GameVar.View_Index;
            arrayOfGameVars[3] = GameVar.New_RelRumours;
            arrayOfGameVars[4] = GameVar.View_Rollover;
            arrayOfGameVars[5] = GameVar.Account_Timer;
            arrayOfGameVars[6] = GameVar.Inquisitor_Budget;
            arrayOfGameVars[7] = GameVar.Lifestyle_Budget;
            arrayOfGameVars[8] = GameVar.Corruption_Factor;
        }

        public GameVar[] GetArrayOfGameVars()
        { return arrayOfGameVars; }

        public int[] GetArrayOfVariables()
        { return arrayOfVariables; }

        /// <summary>
        /// returns GameVar value
        /// </summary>
        /// <param name="globalVar"></param>
        /// <returns></returns>
        public int GetValue(GameVar globalVar)
        { return arrayOfVariables[(int)globalVar]; }

        /// <summary>
        /// returns a GameVar enum from the arrayOfVariables
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameVar GetGameVar(int index)
        {
            if (index < arrayOfGameVars.Length && index > 0)
            { return arrayOfGameVars[index]; }
            //return default value if an incorrect index
            return GameVar.None;
        }


        /// <summary>
        /// Set GameVar directly to this
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public void SetValue(GameVar index, int data)
        {
            if ((int)index < arrayOfVariables.Length)
            {
                int origValue = arrayOfVariables[(int)index];
                arrayOfVariables[(int)index] = data;
                Game.logTurn?.Write($"[GameVar] {index} changed from {origValue} to {data}");
            }
            else
            { Game.SetError(new Error(232, string.Format("index for {0} is out of range, data {1}", index, data))); }
        }

        /// <summary>
        /// GameVar is changed by this amount in this manner. Add/Subtract/RandomPlus/RandomMinus -> +/- rnd.Next(amount)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        /// <param name="apply"></param>
        public void ChangeValue(GameVar index, int amount, EventCalc apply)
        {
            if ((int)index < arrayOfVariables.Length)
            {
                if (amount != 0)
                {
                    int newValue = 0;
                    int tempNum;
                    int origValue = arrayOfVariables[(int)index];
                    switch (apply)
                    {
                        case EventCalc.Add:
                            newValue = origValue + amount;
                            break;
                        case EventCalc.Subtract:
                            //can't go below zero
                            newValue = origValue - amount;
                            if (newValue < 0)
                            {
                                Game.logTurn?.Write($"[Alert] GameVar \"{index}\" has a negative value of {newValue}. MinCapped at Zero");
                                newValue = Math.Max(0, newValue);
                            }
                            break;
                        case EventCalc.RandomPlus:
                            tempNum = rnd.Next(amount);
                            tempNum = Math.Max(1, tempNum);
                            newValue = origValue + tempNum;
                            break;
                        case EventCalc.RandomMinus:
                            tempNum = rnd.Next(amount);
                            tempNum = Math.Max(1, tempNum);
                            newValue = origValue - tempNum;
                            break;
                        default:
                            Game.SetError(new Error(232, $"Invalid EventCalc input \"{apply}\" for {index} -> GameVar not changed"));
                            newValue = origValue;
                            break;
                    }
                    arrayOfVariables[(int)index] = newValue;
                    Game.logTurn?.Write($"[GameVar] {index} changed from {origValue} to {newValue} as a result of {apply} {amount}");
                }
                else { Game.SetError(new Error(232, $"Invalid amount input (zero) -> {index} not changed")); }
            }
            else
            { Game.SetError(new Error(232, $"Index for {index} is out of range")); }
        }
        
    }
}
