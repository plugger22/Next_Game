using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{

    public enum GameStatistic  //NOTE: KEEP order the same as in arrayOfGameStat, eg. enum 1 is arrayOfGameVars[1]
    {
        None,
        Disguise_Days,
        SafeHouse_Days,
        Dungeon_Days,
        Location_Days,
        Travelling_Days,
        AtSea_Days,
        Adrift_Days,
        Known_Days,
        Times_Captured,
        //keep as last
        Count
    }

    /// <summary>
    /// tracks all game statistics used for evaluation. balancing and pdf outputs
    /// </summary>
    public class Statistic
    {
        private readonly GameStatistic[] arrayOfGameStats; //holds enums which provide indexes for arrayOfStatistics
        private readonly int[] arrayOfStatistics; //data which that's used in code, index is a Global enum

        public Statistic()
        {
            arrayOfGameStats = new GameStatistic[(int)GameStatistic.Count];
            arrayOfStatistics = new int[(int)GameStatistic.Count];
            //set up array of Globals
            arrayOfGameStats[1] = GameStatistic.Disguise_Days;
            arrayOfGameStats[2] = GameStatistic.SafeHouse_Days;
            arrayOfGameStats[3] = GameStatistic.Dungeon_Days;
            arrayOfGameStats[4] = GameStatistic.Location_Days;
            arrayOfGameStats[5] = GameStatistic.Travelling_Days;
            arrayOfGameStats[6] = GameStatistic.AtSea_Days;
            arrayOfGameStats[7] = GameStatistic.Adrift_Days;
            arrayOfGameStats[8] = GameStatistic.Known_Days;
            arrayOfGameStats[9] = GameStatistic.Times_Captured;
        }

        public int[] GetArrayOfStatistics()
        { return arrayOfStatistics; }

        public GameStatistic[] GetArrayOfGameStats()
        { return arrayOfGameStats; }

        /// <summary>
        /// return a specific Statistic
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public int GetStatistic(GameStatistic stat)
        { return arrayOfStatistics[(int)stat]; }

        /// <summary>
        /// Main method -> if amount is default '0' then stat++, otherwise stat + Abs(amount)
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="amount"></param>
        public void AddStat(GameStatistic stat, int amount = 0)
        {
            int origValue = arrayOfStatistics[(int)stat];
            if (amount == 0)
            { arrayOfStatistics[(int)stat]++; }
            else
            { arrayOfStatistics[(int)stat] += Math.Abs(amount); }
            Game.logTurn?.Write($"[GameStat] \"{stat}\" -> increased from {origValue} to {arrayOfStatistics[(int)stat]}");
        }

        //new methods above here
    }
}
