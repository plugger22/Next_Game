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
        None,
        //promises
        PromisesNum,
        //keep as last
        Count
    }

    /// <summary>
    /// Variables that are tracked game wide
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
            arrayOfGameVars[1] = GameVar.PromisesNum;
        }

        /// <summary>
        /// returns GameVar value
        /// </summary>
        /// <param name="globalVar"></param>
        /// <returns></returns>
        public int GetValue(GameVar globalVar)
        { return arrayOfVariables[(int)globalVar]; }


        /// <summary>
        /// straight GameVar equals this
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public void SetValue(GameVar index, int data)
        {
            if ((int)index < arrayOfVariables.Length)
            {
                int origValue = arrayOfVariables[(int)index];
                arrayOfVariables[(int)index] = data;
                Game.logTurn?.Write($"{index} changed from {origValue} to {data}");
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
        public void SetValue(GameVar index, int amount, EventCalc apply)
        {
            if ((int)index < arrayOfVariables.Length)
            {
                if (amount != 0)
                {
                    int newValue = 0;
                    int origValue = arrayOfVariables[(int)index];
                    switch (apply)
                    {
                        case EventCalc.Add:
                            newValue = origValue + amount;
                            break;
                        case EventCalc.Subtract:
                            newValue = origValue - amount;
                            break;
                        case EventCalc.RandomPlus:
                            newValue = origValue + rnd.Next(amount);
                            break;
                        case EventCalc.RandomMinus:
                            newValue = origValue - rnd.Next(amount);
                            break;
                        default:
                            Game.SetError(new Error(232, $"Invalid EventCalc input \"{apply}\" for {index} -> GameVar not changed"));
                            newValue = origValue;
                            break;
                    }
                    arrayOfVariables[(int)index] = newValue;
                    Game.logTurn?.Write($"{index} changed from {origValue} to {newValue} as a result of {apply} {amount}");
                }
                else { Game.SetError(new Error(232, $"Invalid amount input (zero) -> {index} not changed")); }
            }
            else
            { Game.SetError(new Error(232, $"Index for {index} is out of range")); }
        }
        
    }
}
