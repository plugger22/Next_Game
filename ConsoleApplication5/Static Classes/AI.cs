using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// handles all AI related matters
    /// </summary>
    public class AI
    {
        static Random rnd;
        private int[,] arrayAI; //'0' -> # enemies at capital, '1,2,3,4' -> # enemies patrolling each branch, [0,] -> actual, [1,] -> desired [2,] -> temp data

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="seed"></param>
        public AI(int seed)
        {
            rnd = new Random(seed);
            arrayAI = new int[3, 5];
        }

        internal void InitialiseAI()
        {
            SetAI();
            InitialiseEnemyActors();
        }

        /// <summary>
        /// Analyses map and sets up the desired part of the AI array (# enemies at capital and # enemies to allocate to each branch)
        /// </summary>
        internal void SetAI()
        {
            Game.logStart?.Write("--- InitialiseAI (AI.cs)");
            int connectorBonus = Game.constant.GetValue(Global.AI_CONNECTOR);
            //work out branch priorities
            int numBranches = Game.network.GetNumBranches();
            int numLocs = Game.network.GetNumLocations() - 1; //ignore capital
            int[] arrayTemp = new int[5]; // (1 to 4 branches with 0 being Capital)
            //allocate # loc's to each branch
            int tempNumLocs = 0;
            for (int i = 1; i < arrayTemp.Length; i++)
            { arrayTemp[i] = Game.network.GetNumLocsByBranch(i); tempNumLocs += arrayTemp[i]; }
            //tallies match?
            if (tempNumLocs != numLocs)
            { Game.SetError(new Error(165, string.Format("Loc's don't tally (tempNumLocs (GetNumLocsByBranch) {0} numLocs (GetNumLocations) {1})", tempNumLocs, numLocs))); }
            //allow for connectors (provide more flexibility and make a branch more valuable to the enemy if present)
            int adjustedNumLocs = 0;
            for (int i = 1; i < arrayTemp.Length; i++)
            {
                if (Game.network.GetBranchConnectorStatus(i) == true)
                { arrayTemp[i] += connectorBonus; }
                adjustedNumLocs += arrayTemp[i];
            }
            //work out how many enemies should stay in the captial (normal operations)
            int totalEnemies = Game.constant.GetValue(Global.INQUISITORS);
            int enemiesInCapital = (int)Math.Round((double)(totalEnemies * Game.constant.GetValue(Global.AI_CAPITAL)) / 100);
            int remainingEnemies = totalEnemies - enemiesInCapital;
            //boundary check
            if (remainingEnemies == 0)
            {
                Game.SetError(new Error(165, "Invalid Remaining Enemies (Zero)"));
                remainingEnemies = 1; enemiesInCapital -= 1; enemiesInCapital = Math.Max(enemiesInCapital, 1);
            }
            arrayAI[1, 0] = enemiesInCapital;
            //assign the desired number of enemies to each relevant branch
            int percent, branchEnemies;
            int poolOfEnemies = remainingEnemies;
            for (int i = 1; i < arrayTemp.Length; i++)
            {
                percent = (int)Math.Round((double)(arrayTemp[i] * 100) / adjustedNumLocs);
                branchEnemies = (int)Math.Round((double)(remainingEnemies * percent) / 100);
                //check we aren't over allocating
                if (branchEnemies > poolOfEnemies)
                {
                    if (poolOfEnemies == 0)
                    {
                        if (enemiesInCapital > 0)
                        {
                            //move an enemy from Capital duty to branch duty
                            arrayAI[1, 0]--;
                        }
                        else { branchEnemies = 0; }
                    }
                    else { branchEnemies = poolOfEnemies; }
                }
                arrayAI[1, i] = branchEnemies;
                //track total number of allocated enemies
                poolOfEnemies -= branchEnemies;
            }
            //any unallocated actors get placed in the capital
            if (poolOfEnemies > 0)
            { arrayAI[1, 0] += poolOfEnemies; }
            //copy finalised data from range 1 to range 2 (temp data used for assigning enemies in InitialiseEnemyActors)
            for (int i = 0; i <= arrayAI.GetUpperBound(1); i++)
            { arrayAI[2, i] = arrayAI[1, i]; }
            //display arrayAI

            for (int i = 0; i <= arrayAI.GetUpperBound(1); i++)
            {
                Game.logStart?.Write(string.Format(" {0} {1} -> Current {2} -> Desired {3} -> adjusted Loc's {4}", i > 0 ? "Branch " : "Capital", i, arrayAI[0, i], arrayAI[1, i],
                  arrayTemp[i]));
            }
        }


        /// <summary>
        /// set up inquisitors and any other enemies -> NOTE: must come AFTER InitialiseAI
        /// </summary>
        internal void InitialiseEnemyActors()
        {
            Game.logStart?.Write("--- InitialiseEnemyActors (AI.cs)");
            Dictionary<int, Enemy> dictEnemyActors = Game.world.GetEnemyActors();
            int numInquisitors = Game.constant.GetValue(Global.INQUISITORS);
            //loop for # of inquisitors
            for (int i = 0; i < numInquisitors; i++)
            {
                //create at capital
                Game.history.CreateInquisitor(1);
            }
            //create The Nemesis
            Game.history.CreateNemesis(1);
            //assign specific enemies to tasks (based on InitialiseAI)
            Game.logStart?.Write("- Assign Enemies");

            foreach (var enemy in dictEnemyActors)
            {
                enemy.Value.MoveOut = true;
                enemy.Value.HuntMode = false;
                if (enemy.Value is Nemesis)
                {
                    enemy.Value.AssignedBranch = 0;
                    Game.logStart?.Write(string.Format(" [Goal -> {0}] {1}, ActID {2} Branch -> {3}", enemy.Value.Title, enemy.Value.Name, enemy.Value.ActID, enemy.Value.AssignedBranch));
                }
                else
                {
                    for (int i = 0; i <= arrayAI.GetUpperBound(1); i++)
                    {
                        if (enemy.Value is Inquisitor)
                        {
                            if (arrayAI[2, i] > 0)
                            {
                                enemy.Value.AssignedBranch = i;
                                arrayAI[2, i]--;
                                Game.logStart?.Write(string.Format(" [Goal -> {0}] {1}, ActID {2} Branch -> {3}", enemy.Value.Title, enemy.Value.Name, enemy.Value.ActID, i));
                                break;
                            }
                        }
                    }
                }
            }
        }


        //new methods above here
    }
}
