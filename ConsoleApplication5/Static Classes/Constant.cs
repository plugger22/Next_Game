using System;


namespace Next_Game
{
    public enum Global
    {
        None,
        //Actors
        INHERIT_TRAIT,
        CHILDBIRTH_DEATH,
        CHILDBIRTH_INFERTILE,
        CHILDBIRTH_COMPLICATIONS,
        PREGNANT,
        TOUCHED,
        LAND_SPEED,
        SEA_SPEED,
        //Followers
        START_FOLLOWERS,
        MAX_FOLLOWERS,
        RECRUIT_FOLLOWERS,
        //Houses
        MEN_AT_ARMS,
        CASTLE_CAPITAL,
        //History
        BATTLE_EVENTS,
        ADVISOR_REFUSAL,
        SIBLING_ESCAPE,
        //Crows
        CROW_NUMBER,
        CROW_BONUS,
        //Map Generation
        MAP_LOCATIONS_MIN,
        MAP_LOCATIONS_MAX,
        MAP_FREQUENCY,
        MAP_SPACING,
        MAP_SIZE,
        MAP_DIVISOR,
        CONNECTOR_MIN,
        CONNECTOR_MAX,
        TERRAIN_SMALL,
        SEA_LARGE,
        MOUNTAIN_LARGE,
        FOREST_LARGE,
        //Game
        GAME_EXILE,
        GAME_REVOLT,
        GAME_STATE,
        GAME_PAST,
        KNOWN_REVERT,
        INQUISITORS,
        //Events
        TRAIT_MULTIPLIER,
        TEST_MULTIPLIER,
        //Challenges
        HAND_CARDS_NUM,
        PLAYER_INFLUENCE,
        NEUTRAL_EFFECT,
        ARMY_SIZE,
        SIT_CARD_SPREAD,
        RESULT_FACTOR,
        OPPONENT_MARGIN,
        TALK_THRESHOLD,
        SEDUCE_THRESHOLD,
        ALLEGIANCE_THRESHOLD,
        BLACKMAIL_THRESHOLD,
        IMPROVE_THRESHOLD,
        FRIEND_THRESHOLD,
        ENEMY_THRESHOLD,
        NUM_SUPPORTERS,
        //Relationships
        HOUSE_REL_GOOD,
        HOUSE_REL_EFFECT,
        HOUSE_REL_NUM,
        //AI
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
        //Incarceration
        LOSS_OF_LEGEND,
        //keep as last
        Count 
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
            arrayOfGlobals[10] = Global.LAND_SPEED;
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
            arrayOfGlobals[28] = Global.START_FOLLOWERS;
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
            arrayOfGlobals[57] = Global.LOSS_OF_LEGEND;
            arrayOfGlobals[58] = Global.FRIEND_THRESHOLD;
            arrayOfGlobals[59] = Global.NUM_SUPPORTERS;
            arrayOfGlobals[60] = Global.ENEMY_THRESHOLD;
            arrayOfGlobals[61] = Global.SEDUCE_THRESHOLD;
            arrayOfGlobals[62] = Global.ALLEGIANCE_THRESHOLD;
            arrayOfGlobals[63] = Global.BLACKMAIL_THRESHOLD;
            arrayOfGlobals[64] = Global.IMPROVE_THRESHOLD;
            arrayOfGlobals[65] = Global.SEA_SPEED;
            arrayOfGlobals[66] = Global.TEST_MULTIPLIER;
            arrayOfGlobals[67] = Global.MAX_FOLLOWERS;
            arrayOfGlobals[68] = Global.RECRUIT_FOLLOWERS;
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
                Game.logStart?.Write($"{index} -> {data} Min {low} Max {high}");
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
