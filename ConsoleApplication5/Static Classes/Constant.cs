using System;


namespace Next_Game
{
    public enum Global
    {
        None, //keep as first
        INHERIT_TRAIT,
        CHILDBIRTH_DEATH,
        CHILDBIRTH_INFERTILE,
        PREGNANT,
        TOUCHED,
        MEN_AT_ARMS,
        BATTLE_EVENTS,
        ADVISOR_REFUSAL,
        SIBLING_ESCAPE,
        MOVE_SPEED,
        CHILDBIRTH_COMPLICATIONS,
        CROW_NUMBER,
        CROW_BONUS,
        MAP_LOCATIONS_MIN,
        MAP_LOCATIONS_MAX,
        MAP_FREQUENCY,
        MAP_SPACING,
        MAP_SIZE,
        GAME_EXILE,
        GAME_REVOLT,
        GAME_STATE,
        GAME_PAST,
        MAP_DIVISOR,
        CONNECTOR_MIN,
        CONNECTOR_MAX,
        TERRAIN_SMALL,
        SEA_LARGE,
        MOUNTAIN_LARGE,
        FOREST_LARGE,
        NUM_FOLLOWERS,
        TRAIT_MULTIPLIER,
        HAND_CARDS_NUM,
        PLAYER_INFLUENCE,
        NEUTRAL_EFFECT,
        ARMY_SIZE,
        SIT_CARD_SPREAD,
        CASTLE_CAPITAL,
        RESULT_FACTOR,
        OPPONENT_MARGIN,
        TALK_THRESHOLD,
        HOUSE_REL_GOOD,
        HOUSE_REL_EFFECT,
        HOUSE_REL_NUM,
        KNOWN_REVERT,
        INQUISITORS,
        AI_CONTINUE_SEARCH,
        AI_CONTINUE_HIDE,
        AI_CONTINUE_WAIT,
        AI_SEARCH_KNOWN,
        AI_SEARCH_HIDE,
        AI_SEARCH_MOVE,
        AI_SEARCH_SEARCH,
        AI_SEARCH_WAIT,
        AI_CAPITAL,
        AI_CONNECTOR,
        AI_HUNT_THRESHOLD,
        Count //keep as last
    }

    public class Constant
    {
        private readonly Global[] arrayOfGlobals; //holds enums which provide indexes for arrayOfConstants
        private readonly int[] arrayOfConstants; //data which that's used in code, index is a Global enum
        private readonly int[,] arrayOfLimits; //holds min and max acceptable values for each, equivalent index, Constant

        public Constant()
            {
            arrayOfGlobals = new Global[(int)Global.Count];
            arrayOfConstants = new int[(int)Global.Count];
            arrayOfLimits = new int[(int)Global.Count, 2];
            //set up array of Globals
            arrayOfGlobals[1] = Global.INHERIT_TRAIT;
            arrayOfGlobals[2] = Global.CHILDBIRTH_DEATH;
            arrayOfGlobals[3] = Global.CHILDBIRTH_INFERTILE;
            arrayOfGlobals[4] = Global.PREGNANT;
            arrayOfGlobals[5] = Global.TOUCHED;
            arrayOfGlobals[6] = Global.MEN_AT_ARMS;
            arrayOfGlobals[7] = Global.BATTLE_EVENTS;
            arrayOfGlobals[8] = Global.ADVISOR_REFUSAL;
            arrayOfGlobals[9] = Global.SIBLING_ESCAPE;
            arrayOfGlobals[10] = Global.MOVE_SPEED;
            arrayOfGlobals[11] = Global.CHILDBIRTH_COMPLICATIONS;
            arrayOfGlobals[12] = Global.CROW_NUMBER;
            arrayOfGlobals[13] = Global.CROW_BONUS;
            arrayOfGlobals[14] = Global.MAP_LOCATIONS_MIN;
            arrayOfGlobals[15] = Global.MAP_LOCATIONS_MAX;
            arrayOfGlobals[16] = Global.MAP_FREQUENCY;
            arrayOfGlobals[17] = Global.MAP_SPACING;
            arrayOfGlobals[18] = Global.MAP_SIZE;
            arrayOfGlobals[19] = Global.GAME_EXILE;
            arrayOfGlobals[20] = Global.GAME_REVOLT;
            arrayOfGlobals[21] = Global.MAP_DIVISOR;
            arrayOfGlobals[22] = Global.CONNECTOR_MIN;
            arrayOfGlobals[23] = Global.CONNECTOR_MAX;
            arrayOfGlobals[24] = Global.TERRAIN_SMALL;
            arrayOfGlobals[25] = Global.SEA_LARGE;
            arrayOfGlobals[26] = Global.MOUNTAIN_LARGE;
            arrayOfGlobals[27] = Global.FOREST_LARGE;
            arrayOfGlobals[28] = Global.NUM_FOLLOWERS;
            arrayOfGlobals[29] = Global.TRAIT_MULTIPLIER;
            arrayOfGlobals[30] = Global.GAME_STATE;
            arrayOfGlobals[31] = Global.HAND_CARDS_NUM;
            arrayOfGlobals[32] = Global.PLAYER_INFLUENCE;
            arrayOfGlobals[33] = Global.NEUTRAL_EFFECT;
            arrayOfGlobals[34] = Global.ARMY_SIZE;
            arrayOfGlobals[35] = Global.SIT_CARD_SPREAD;
            arrayOfGlobals[36] = Global.CASTLE_CAPITAL;
            arrayOfGlobals[37] = Global.RESULT_FACTOR;
            arrayOfGlobals[38] = Global.OPPONENT_MARGIN;
            arrayOfGlobals[39] = Global.TALK_THRESHOLD;
            arrayOfGlobals[40] = Global.HOUSE_REL_GOOD;
            arrayOfGlobals[41] = Global.HOUSE_REL_EFFECT;
            arrayOfGlobals[42] = Global.GAME_PAST;
            arrayOfGlobals[43] = Global.HOUSE_REL_NUM;
            arrayOfGlobals[44] = Global.KNOWN_REVERT;
            arrayOfGlobals[45] = Global.INQUISITORS;
            arrayOfGlobals[46] = Global.AI_CONTINUE_SEARCH;
            arrayOfGlobals[47] = Global.AI_CONTINUE_HIDE;
            arrayOfGlobals[48] = Global.AI_CONTINUE_WAIT;
            arrayOfGlobals[49] = Global.AI_SEARCH_KNOWN;
            arrayOfGlobals[50] = Global.AI_SEARCH_HIDE;
            arrayOfGlobals[51] = Global.AI_SEARCH_MOVE;
            arrayOfGlobals[52] = Global.AI_SEARCH_SEARCH;
            arrayOfGlobals[53] = Global.AI_SEARCH_WAIT;
            arrayOfGlobals[54] = Global.AI_CAPITAL;
            arrayOfGlobals[55] = Global.AI_CONNECTOR;
            arrayOfGlobals[56] = Global.AI_HUNT_THRESHOLD;
            //arrayOfGlobals[57] = Global.AI_HUNT_WAIT;
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
        public Global GetGlobal(int index)
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
        public void SetData( Global index, int data, int low, int high)
        {
            if ((int)index < arrayOfConstants.Length)
            {
                //if out of range, generate error and assign Low value
                if (data < low || data > high)
                {
                    Game.SetError(new Error(9, string.Format("{0} out of MinMax Range, data {1}, Min {2} Max {3} Auto assigned Min (\"{4}\")", index, data, low, high, low)));
                    data = low;
                }
                arrayOfConstants[(int)index] = data;
                arrayOfLimits[(int)index, 0] = low;
                arrayOfLimits[(int)index, 1] = high;
                Console.WriteLine("{0} -> {1} Min {2} Max {3}", index, data, low, high);
            }
            else
            { Game.SetError(new Error(9, string.Format("{0} out of range, data {1}", index, data))); }
        }

        /// <summary>
        /// loops Global enums and checks all have valid data that has been imported from 'Constants.txt' (invalid if data = 0)
        /// </summary>
        public void ErrorCheck()
        {
            for(int index = 1; index < (int)Global.Count; index++)
            {
                if (arrayOfConstants[index] == 0)
                { Game.SetError(new Error(46, string.Format("{0} is missing data. Check Constants.txt is correct", (Global)index))); }
            }
        }
    }
}
