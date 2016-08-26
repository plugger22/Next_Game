using System;


namespace Next_Game
{
    public enum Global
    {
        None, //keep as first
        INHERIT_TRAIT,
        CHILDBIRTH_DEATH,
        CHILDBIRTH_INFERTILE,
        Count //keep as last
    }

    public class Constant
    {
        private readonly Global[] arrayOfGlobals; //holds enums which provide indexes for arrayOfConstants
        private readonly int[] arrayOfConstants; //what's used in code, index is a Global enum

        public Constant()
            {
            arrayOfGlobals = new Global[(int)Global.Count];
            arrayOfConstants = new int[(int)Global.Count];
            //set up array of Globals
            arrayOfGlobals[1] = Global.INHERIT_TRAIT;
            arrayOfGlobals[2] = Global.CHILDBIRTH_DEATH;
            arrayOfGlobals[3] = Global.CHILDBIRTH_INFERTILE;
            }

        /// <summary>
        /// returns constant value
        /// </summary>
        /// <param name="globalVar"></param>
        /// <returns></returns>
        public int GetValue( Global globalVar)
        { return arrayOfConstants[(int)globalVar]; }

        /// <summary>
        /// returns a Global enum from the arrayOfGlobals
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Global GetGlobal( int index)
        {
            if (index < arrayOfGlobals.Length && index > 0)
            { return arrayOfGlobals[index]; }
            //return default value if an incorrect index
            return Global.None;
        }

        /// <summary>
        /// initialise data in arrayOfConstants
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public void SetData( Global index, int data)
        {
            if ((int)index < arrayOfConstants.Length)
            {
                arrayOfConstants[(int)index] = data;
                Console.WriteLine("{0} {1} initialised", index, data);
            }
            else
            { Console.WriteLine("ERROR: Constant.cs, {0} out of range, data {1}", index, data); }
        }
    }
}
