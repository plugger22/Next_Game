using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{

    public enum GameStat  //NOTE: KEEP order the same as in arrayOfGameStat, eg. enum 1 is arrayOfGameVars[1]
    {
        None,
        Disguise_Days,
        SafeHouse_Days,
        Dungeon_Days,
        Location_Days,
        Travelling_Days,
        AtSea_Days,
        Adrift_Days,
        //keep as last
        Count
    }

    /// <summary>
    /// tracks all game statistics used for evaluation. balancing and pdf outputs
    /// </summary>
    public class Statistic
    {
        private readonly GameStat[] arrayOfGameStats; //holds enums which provide indexes for arrayOfStatistics
        private readonly int[] arrayOfStatistics; //data which that's used in code, index is a Global enum

        public Statistic()
        {
            arrayOfGameStats = new GameStat[(int)GameStat.Count];
            arrayOfStatistics = new int[(int)GameStat.Count];
            //set up array of Globals
            arrayOfGameStats[1] = GameStat.Disguise_Days;
            arrayOfGameStats[2] = GameStat.SafeHouse_Days;
            arrayOfGameStats[3] = GameStat.Dungeon_Days;
            arrayOfGameStats[4] = GameStat.Location_Days;
            arrayOfGameStats[5] = GameStat.Travelling_Days;
            arrayOfGameStats[6] = GameStat.AtSea_Days;
            arrayOfGameStats[7] = GameStat.Adrift_Days;
        }

        public int[] GetArrayOfStatistics()
        { return arrayOfStatistics; }

        public GameStat[] GetArrayOfGameStats()
        { return arrayOfGameStats; }

        /// <summary>
        /// return a specific Statistic
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public int GetStatistic(GameStat stat)
        { return arrayOfStatistics[(int)stat]; }

        /// <summary>
        /// Main method -> if amount is default '0' then stat++, otherwise stat + Abs(amount)
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="amount"></param>
        public void AddStat(GameStat stat, int amount = 0)
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
