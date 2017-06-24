using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
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


        /// <summary>
        /// Master AI controller, checked each turn, determines HuntMode for each enemy based on big picture analysis
        /// </summary>
        internal void UpdateAIController()
        {
            int threshold = Game.constant.GetValue(Global.AI_HUNT_THRESHOLD); //max # turns since Player last known that AI will continue to hunt
            Dictionary<int, Enemy> dictEnemyActors = Game.world.GetEnemyActors();
            int targetLocID, distance, enemyDM;
            int turnsToDestination = 0; //# of turns for Player to reach their destination if travelling (used to adjust threshold)
            Game.logTurn?.Write("--- UpdateAIController (World.cs)");
            //get target
            int targetActID = Game.variable.GetValue(GameVar.Inquisitor_Target);
            Actor target = Game.world.GetAnyActor(targetActID);
            int knownStatus = Game.world.GetTrackingStatus(target.ActID); //if '0' then Known, if > 0 then # of days since last known
            if (target != null)
            {
                //Travelling? Allow for time taken to reach destination
                if (target.Status == ActorStatus.Travelling)
                {
                    List<Move> listMoveObjects = Game.world.GetMoveObjects();
                    if (listMoveObjects != null)
                    {
                        //loop List of Move objects looking for Target's party
                        for (int i = 0; i < listMoveObjects.Count; i++)
                        {
                            Move moveObject = listMoveObjects[i];
                            if (moveObject.CheckInParty(target.ActID) == true)
                            {
                                //Target found, determine how many route segments left
                                turnsToDestination = moveObject.CheckTurnsToDestination();
                                Game.logTurn?.Write(string.Format(" [AI -> Notification] Target is Travelling -> {0} turns from their destination", turnsToDestination));
                                break;
                            }
                        }
                    }
                    else { Game.SetError(new Error(167, "Invalid listMoveObjects (null) -> Target travel situation not taken into account")); }
                }
                //ignore all this if player Incarcerated in a dungeon?
                if (target.Status == ActorStatus.AtLocation || target.Status == ActorStatus.Travelling)
                {
                    //threshold is adjusted upwards if Player enroute to a destination
                    threshold += turnsToDestination;
                    //Target location is current, if known, or last known if not. Will be destination if travelling.
                    if (knownStatus == 0) { targetLocID = target.LocID; }
                    else { targetLocID = target.LastKnownLocID; }
                    if (targetLocID > 0)
                    {
                        //can only hunt a recently known player for so long before reverting back to normal behaviour (there is a time taken test below which tests threshold on a tighter basis)
                        if (knownStatus <= threshold)
                        {
                            //Known
                            Location loc = Game.network.GetLocation(targetLocID);
                            if (loc != null)
                            {
                                Position targetPos = loc.GetPosition();
                                //dictionary to handle sorted distance data
                                Dictionary<int, int> tempDict = new Dictionary<int, int>();
                                foreach (var enemy in dictEnemyActors)
                                {
                                    //store enemies in tempDict by dist to player (key is ActID, value distance)
                                    Position enemyPos = enemy.Value.GetPosition();
                                    if (targetPos != null && enemyPos != null)
                                    {
                                        //only check enemies at a location (those who are travelling will have to wait)
                                        if (enemy.Value.Status == ActorStatus.AtLocation)
                                        {
                                            List<Route> route = Game.network.GetRouteAnywhere(enemyPos, targetPos);
                                            distance = Game.network.GetDistance(route);
                                            try
                                            { tempDict.Add(enemy.Value.ActID, distance); }
                                            catch (ArgumentException)
                                            { Game.SetError(new Error(167, string.Format("Invalid enemy ID {0} (duplicate)", enemy.Value.ActID))); }
                                        }
                                        else
                                        {
                                            Game.logTurn?.Write(string.Format(" [AI -> Notification] {0} {1}, ActID {2} is Travelling to {3}", enemy.Value.Title, enemy.Value.Name,
                                            enemy.Value.ActID, Game.world.GetLocationName(enemy.Value.LocID)));
                                        }
                                    }
                                    else
                                    {
                                        Game.SetError(new Error(167, string.Format("Invalid Target ({0}:{1}) or Enemy Position ({2}:{3})", targetPos.PosX, targetPos.PosY,
                                     enemyPos.PosX, enemyPos.PosY)));
                                    }
                                }
                                //sort dictionary by distance
                                if (tempDict.Count > 0)
                                {
                                    var sorted = from pair in tempDict orderby pair.Value ascending select pair;
                                    foreach (var pair in sorted)
                                    {
                                        Enemy enemy = (Enemy)Game.world.GetAnyActor(pair.Key);
                                        if (enemy != null)
                                        {
                                            //set nemesis to have double the normal threshold (will hunt at twice the distance from the player than an inquisitor)
                                            enemyDM = 1;
                                            if (enemy is Nemesis) { enemyDM = 2; }
                                            //can enemy reach player loc within threshold time? (distance / speed = # of turns <= threshold # of turns allowed
                                            if ((pair.Value / enemy.Speed) <= (threshold * enemyDM))
                                            { enemy.HuntMode = true; }
                                            else { enemy.HuntMode = false; }
                                            Game.logTurn?.Write(string.Format(" [AI -> Mode] enemyID {0},  distance -> {1}  Threshold (turns) -> {2}  Mode -> {3}", pair.Key, pair.Value,
                                                threshold * enemyDM, enemy.HuntMode == true ? "Hunt" : "Normal"));
                                        }
                                        else { Game.SetError(new Error(167, string.Format("Invalid enemy, ID {0} (null)", pair.Key))); }
                                    }
                                }
                                else { Game.logTurn?.Write(" [AI -> Notification] tempDictionary has too few records to sort (zero)"); }
                            }
                            else { Game.SetError(new Error(167, "Invalid Loc (null) Dictionary not updated")); }
                        }
                        else
                        {
                            //Unknown -> all enemies at a location are set to normal mode (huntmode 'false')
                            foreach (var enemy in dictEnemyActors)
                            {
                                if (enemy.Value.Status == ActorStatus.AtLocation)
                                {
                                    enemy.Value.HuntMode = false;
                                    Game.logTurn?.Write(string.Format(" [AI -> Target Unknown] {0} {1}, Act ID {2} Mode -> Normal", enemy.Value.Title, enemy.Value.Name, enemy.Value.ActID));
                                }
                            }
                        }
                    }
                    else { Game.SetError(new Error(167, "Warning -> LocID of Target has returned Zero")); }
                }
                else if (target.Status == ActorStatus.Captured)
                {
                    //Incarcerated in a dungeon -> all enemies at a location are set to normal mode (huntmode 'false')
                    foreach (var enemy in dictEnemyActors)
                    {
                        if (enemy.Value.Status == ActorStatus.AtLocation)
                        {
                            enemy.Value.HuntMode = false;
                            Game.logTurn?.Write(string.Format(" [AI -> Target Captured] {0} {1}, Act ID {2} Mode -> Normal", enemy.Value.Title, enemy.Value.Name, enemy.Value.ActID));
                        }
                    }
                }
            }
            else { Game.SetError(new Error(167, "Invalid Target (Null), set Enemy AI bypassed")); }
        }

        /// <summary>
        /// handles AI for all enemies, also updates status (known, etc.)
        /// </summary>
        internal void SetEnemyActivity()
        {
            Dictionary<int, Enemy> dictEnemyActors = Game.world.GetEnemyActors();
            //get target
            int targetActID = Game.variable.GetValue(GameVar.Inquisitor_Target);
            Actor target = Game.world.GetAnyActor(targetActID);
            if (target != null)
            {
                int turnsDM; //DM for the # of turns spent on the same goal (prevents enemy being locked into a set goal due to bad rolls)
                int targetLocID = target.LocID;
                int turnsUnknown = target.TurnsUnknown;
                bool huntStatus;
                int ai_search = Game.constant.GetValue(Global.AI_CONTINUE_SEARCH);
                int ai_hide = Game.constant.GetValue(Global.AI_CONTINUE_HIDE);
                int ai_wait = Game.constant.GetValue(Global.AI_CONTINUE_WAIT);
                int revert = Game.constant.GetValue(Global.KNOWN_REVERT);
                Game.logTurn?.Write("--- SetEnemyActivity (World.cs)");
                //loop enemy dictionary
                foreach (var enemy in dictEnemyActors)
                {
                    //debug -> random chance of enemy being known
                    if (enemy.Value.Known == false && rnd.Next(100) < 20)
                    {
                        enemy.Value.Known = true; enemy.Value.Revert = revert;
                        Game.logTurn?.Write(string.Format(" [Enemy -> Known] {0} ActID {1} has become KNOWN", enemy.Value.Name, enemy.Value.ActID));
                    }

                    //update status -> unknown
                    if (enemy.Value.Known == false) { enemy.Value.TurnsUnknown++; enemy.Value.Revert = 0; }
                    else
                    {
                        //known
                        enemy.Value.TurnsUnknown = 0;
                        enemy.Value.LastKnownLocID = enemy.Value.LocID;
                        enemy.Value.LastKnownPos = enemy.Value.GetPosition();
                        enemy.Value.LastKnownGoal = enemy.Value.Goal;
                        enemy.Value.Revert--;
                        if (enemy.Value.Revert <= 0)
                        {
                            enemy.Value.Revert = 0; enemy.Value.Known = false; enemy.Value.TurnsUnknown++;
                            Game.logTurn?.Write(string.Format(" [Enemy -> Unknown] {0} ActID {1} has reverted to Unknown status (timer elapsed)", enemy.Value.Name, enemy.Value.ActID));
                        }
                    }
                    //continue on with existing goal or get a new one?
                    if (enemy.Value is Inquisitor || enemy.Value is Nemesis)
                    {
                        //inquisitors -> if Move then automatic (continues on with Move)
                        if (enemy.Value.Goal != ActorAIGoal.Move)
                        {
                            huntStatus = enemy.Value.HuntMode;
                            turnsDM = enemy.Value.GoalTurns; //+1 % chance of changing goal per turn spent on existing goal
                            enemy.Value.GoalTurns++;
                            switch (enemy.Value.Goal)
                            {
                                case ActorAIGoal.None:
                                    //auto assign new goal
                                    SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown);
                                    break;
                                case ActorAIGoal.Wait:
                                    if (huntStatus == true)
                                    {
                                        //Player Known -> Will Search if same Loc
                                        SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown);
                                    }
                                    else
                                    {
                                        //Player Unknown
                                        if (rnd.Next(100) > (ai_wait + turnsDM))
                                        { SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown); }
                                        else { Game.logTurn?.Write(string.Format(" [Enemy -> Goal] {0}, ActID {1} retains Goal -> {2}", enemy.Value.Name, enemy.Value.ActID, enemy.Value.Goal)); }
                                    }
                                    break;
                                case ActorAIGoal.Search:
                                    if (huntStatus == true)
                                    {
                                        //Player Known -> if actor at different location then new goal
                                        if (enemy.Value.LocID != targetLocID)
                                        { SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown); }
                                    }
                                    else
                                    {
                                        //Player Unknown
                                        if (rnd.Next(100) > (ai_search + turnsDM))
                                        { SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown); }
                                        else { Game.logTurn?.Write(string.Format(" [Enemy -> Goal] {0}, ActID {1} retains Goal -> {2}", enemy.Value.Name, enemy.Value.ActID, enemy.Value.Goal)); }
                                    }
                                    break;
                                case ActorAIGoal.Hide:
                                    if (huntStatus == true)
                                    {
                                        //Player Known -> if actor at different location then new goal
                                        if (enemy.Value.LocID != targetLocID)
                                        { SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown); }
                                    }
                                    else
                                    {
                                        //Player Unknown
                                        if (rnd.Next(100) > (ai_hide + turnsDM))
                                        { SetEnemyGoal(enemy.Value, huntStatus, targetLocID, turnsUnknown); }
                                        else { Game.logTurn?.Write(string.Format(" [Enemy -> Goal] {0}, ActID {1} retains Goal -> {2}", enemy.Value.Name, enemy.Value.ActID, enemy.Value.Goal)); }
                                    }
                                    break;
                                default:
                                    Game.SetError(new Error(155, string.Format("Invalid Enemy Goal (\"{0}\")", enemy.Value.Goal)));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //all other enemies
                    }
                }
            }
            else { Game.SetError(new Error(155, "Invalid Target (null)")); }
        }

        /// <summary>
        /// sub method to provide a new goal when required -> Incorporates all necessary AI logic
        /// <param name="knowStatus">if '0' then player status is Unknown</param>
        /// <param name="targetLocID">current locID or Target's destination locId if travelling</param>
        /// <param name="turnsUnknown">Number of Turns player has been unknown for ('0' -> known)</param>
        /// </summary>
        /// <param name="enemy"></param>
        private void SetEnemyGoal(Enemy enemy, bool huntStatus, int targetLocID, int turnsUnknown)
        {
            Game.logTurn?.Write("--- SetEnemyGoal (World.cs)");
            bool huntMoveFlag = false;
            int rndNum, refID, tempDistance, enemyDistance, tempLocID;
            int currentBranch = -1;
            ActorAIGoal newGoal = ActorAIGoal.None;
            if (enemy != null)
            {
                if (targetLocID > 0)
                {
                    //get location of enemy
                    Location loc = Game.network.GetLocation(enemy.LocID);
                    if (loc != null)
                    {
                        //get branch info
                        refID = Game.world.ConvertLocToRef(enemy.LocID);
                        if (refID > 0)
                        {
                            House house = null;
                            if (refID == 9999) //capital
                            { currentBranch = 0; }
                            else
                            {
                                house = Game.world.GetHouse(refID);
                                currentBranch = house.Branch;
                            }
                            //debug
                            Game.logTurn?.Write(string.Format(" [Goal -> Branch] {0}, ActID {1} Assigned Branch -> {2} Current Branch -> {3}", enemy.Name, enemy.ActID, enemy.AssignedBranch, currentBranch));
                            //Mode -> Hunt or Normal (set by UpdateAIController)
                            if (huntStatus == true)
                            {
                                //Player Known, Hunt Mode -> not at same location
                                if (enemy.LocID != targetLocID)
                                {
                                    newGoal = ActorAIGoal.Move;
                                    huntMoveFlag = true;
                                }
                                //Player Known, Hunt Mode -> Same location -> Search
                                else { newGoal = ActorAIGoal.Search; }
                            }
                            else
                            {
                                //Normal Mode -> Player Unknown
                                rndNum = rnd.Next(100);
                                //Possible goals depend on location type
                                if (house != null)
                                {
                                    if (house is MajorHouse)
                                    {
                                        //Major House -> Wait 30, Hide 20, Move 50
                                        if (rndNum <= 30) { newGoal = ActorAIGoal.Wait; }
                                        else if (rndNum >= 50) { newGoal = ActorAIGoal.Move; }
                                        else { newGoal = ActorAIGoal.Hide; }
                                    }
                                    else if (house is MinorHouse)
                                    {
                                        //Minor House -> Wait 30, Move 70
                                        if (rndNum <= 30) { newGoal = ActorAIGoal.Wait; }
                                        else { newGoal = ActorAIGoal.Move; }
                                    }
                                    else if (house is InnHouse)
                                    {
                                        //Inn -> Wait 20, Hide 30, Move 50
                                        if (rndNum <= 20) { newGoal = ActorAIGoal.Wait; }
                                        else if (rndNum >= 50) { newGoal = ActorAIGoal.Move; }
                                        else { newGoal = ActorAIGoal.Hide; }
                                    }
                                    else { Game.SetError(new Error(156, "Invalid House type (not in list)")); }
                                }
                                else if (refID == 9999)
                                {
                                    //Capital
                                    if (enemy.AssignedBranch == 0)
                                    {
                                        //Capital (where enemy should be) -> Wait 70, Hide 30
                                        if (rndNum <= 70) { newGoal = ActorAIGoal.Wait; }
                                        else { newGoal = ActorAIGoal.Hide; }
                                    }
                                    else
                                    {
                                        //Capital -> Wait 30, Hide 20, Move 50
                                        if (rndNum <= 30) { newGoal = ActorAIGoal.Wait; }
                                        else if (rndNum >= 50) { newGoal = ActorAIGoal.Move; }
                                        else { newGoal = ActorAIGoal.Hide; }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Game.SetError(new Error(156, string.Format("Enemy {0}, ID {1}, LocID {2} has an Invalid RefID (zero or less)", enemy.Name, enemy.ActID, enemy.LocID)));
                            //give default goal of Move
                            newGoal = ActorAIGoal.Move;
                        }

                        //reset Goal turns if new goal different to old goal
                        if (newGoal != enemy.Goal)
                        {
                            enemy.GoalTurns = 0;
                            //assign new goal
                            enemy.Goal = newGoal;
                            Game.logTurn?.Write(string.Format(" [Goal -> New] {0}, ActID {1}, {2}, assigned new Goal -> {3}", enemy.Name, enemy.ActID, Game.world.GetLocationCoords(enemy.LocID),
                                enemy.Goal));
                        }
                        //
                        // --- Handle all Move logic here
                        //
                        if (newGoal == ActorAIGoal.Move)
                        {
                            Position posOrigin = enemy.GetPosition();
                            List<int> listNeighbours = loc.GetNeighboursLocID();
                            int destinationLocID = 0;
                            //
                            //-- HUNT mode
                            //
                            if (huntMoveFlag == true)
                            {
                                // - Move One Node closer to Player's last known location
                                if (turnsUnknown > 3 && rnd.Next(100) < 50)
                                {
                                    Location locTarget = Game.network.GetLocation(targetLocID);
                                    if (locTarget != null)
                                    {
                                        Position posTarget = locTarget.GetPosition();
                                        List<Position> pathTemp = Game.network.GetPathAnywhere(posOrigin, posTarget);
                                        //loop path looking for the first viable location along path
                                        for (int i = 0; i < pathTemp.Count; i++)
                                        {
                                            Position posTemp = pathTemp[i];
                                            if (posTemp != null)
                                            {
                                                tempLocID = Game.map.GetMapInfo(MapLayer.LocID, posTemp.PosX, posTemp.PosY);
                                                if (tempLocID > 0)
                                                {
                                                    Game.logTurn?.Write(string.Format(" [Goal -> Move] {0}, ActID {1} -> One Node closer to Target -> {2}, LocID {3}", enemy.Name, enemy.ActID,
                                                    Game.world.GetLocationName(tempLocID), tempLocID));
                                                    destinationLocID = tempLocID; break;
                                                }
                                            }
                                            else { Game.SetError(new Error(156, "Invalid Position (null) in pathTemp")); }
                                        }
                                        //error check
                                        if (destinationLocID == 0)
                                        {
                                            destinationLocID = targetLocID;
                                            Game.logTurn?.Write(string.Format(" [Goal -> Alert] {0}, ActID [1} has been assigned a default TargetLocID [move One Node closer] as no viable node was found",
                                                enemy.Name, enemy.ActID));
                                        }
                                    }
                                    else { Game.SetError(new Error(156, "Invalid locTarget (null) Viable Node not searched for")); }
                                }
                                // - Move Directly to Player's last known location
                                else
                                {
                                    Game.logTurn?.Write(string.Format(" [Goal -> Move] {0}, ActID {1} -> Target's last known location -> {2}, LocID {3}", enemy.Name, enemy.ActID,
                                                   Game.world.GetLocationName(targetLocID), targetLocID));
                                    destinationLocID = targetLocID;
                                }
                            }
                            //
                            // -- NORMAL Mode
                            //
                            else
                            {
                                // - Correct Branch
                                if (enemy.AssignedBranch == currentBranch)
                                {
                                    //Not at Capital
                                    if (enemy.LocID > 1)
                                    {
                                        List<int> tempLocList = new List<int>();
                                        //change direction of travel?
                                        bool reverseStatus = false;
                                        switch (enemy.MoveOut)
                                        {
                                            case true:
                                                //slightly higher chance of reversing outward movement in order to keep inquisitors closer to the capital
                                                if (rnd.Next(100) < 15)
                                                { reverseStatus = true; }
                                                break;
                                            case false:
                                                if (rnd.Next(100) < 10)
                                                { reverseStatus = true; }
                                                break;
                                        }
                                        if (reverseStatus == true)
                                        {
                                            Game.logTurn?.Write(string.Format(" [Goal -> Alert] {0}, ActID {1} has reversed their MoveOut status from {2} to {3}", enemy.Name, enemy.ActID, enemy.MoveOut,
                                                enemy.MoveOut == true ? "False" : "True"));
                                            if (enemy.MoveOut == true) { enemy.MoveOut = false; }
                                            else { enemy.MoveOut = true; }
                                        }
                                        enemyDistance = loc.DistanceToCapital;
                                        for (int i = 0; i < listNeighbours.Count; i++)
                                        {
                                            Location locTemp = Game.network.GetLocation(listNeighbours[i]);
                                            if (locTemp != null)
                                            {
                                                tempDistance = locTemp.DistanceToCapital;

                                                if (enemy.MoveOut == true)
                                                {
                                                    //move outwards towards capital -> select any that are further out (also check not going across a connector)
                                                    if (tempDistance > enemyDistance && locTemp.GetBranch() == enemy.AssignedBranch)
                                                    { tempLocList.Add(listNeighbours[i]); }
                                                }
                                                else
                                                {
                                                    //move inwards towards capital -> select any that are further in
                                                    if (tempDistance < enemyDistance)
                                                    { tempLocList.Add(listNeighbours[i]); }
                                                }
                                            }
                                            else
                                            { Game.SetError(new Error(156, "Invalid LocID (zero or less) [Not at Capital] in ListOfNeighbours")); }
                                        }
                                        //any viable selections?
                                        if (tempLocList.Count > 0)
                                        {
                                            //randomly select a destination
                                            destinationLocID = tempLocList[rnd.Next(0, tempLocList.Count)];
                                            Game.logTurn?.Write(string.Format(" [Goal -> Move] {0}, ActID {1} -> Move {2} -> {3}, LocID {4}", enemy.Name, enemy.ActID,
                                                enemy.MoveOut == true ? "Outwards" : "Inwards", Game.world.GetLocationName(destinationLocID), destinationLocID));
                                        }
                                        else
                                        {
                                            //else must have reached the end of a branch -> reverse move direction to prevent an endless loop
                                            Game.logTurn?.Write(string.Format(" [Goal -> Alert] {0}, ActID {1} has reversed their MoveOut status [end of the Road] from {2} to {3}", enemy.Name, enemy.ActID,
                                                enemy.MoveOut, enemy.MoveOut == true ? "False" : "True"));
                                            if (enemy.MoveOut == true) { enemy.MoveOut = false; }
                                            else { enemy.MoveOut = true; }
                                            //NOTE: destinationLocID left unassigned (default '0') so that a random selection will be made at the end.
                                        }
                                    }
                                    //At Capital
                                    else if (enemy.LocID == 1)
                                    {
                                        //Currently at the Capital -> Shouldn't get to this situation (see above)
                                        Game.logTurn?.Write(string.Format(" [Goal -> Alert] Normal Mode, {0}, ActID {1} At Capital with correct branch -> Unassigned", enemy.Name, enemy.ActID));
                                    }
                                }
                                // - Incorrect Branch
                                else if (enemy.AssignedBranch != currentBranch && currentBranch > -1)
                                {
                                    //Not at Capital
                                    if (enemy.LocID > 1)
                                    {
                                        //return to Capital
                                        destinationLocID = 1;
                                        Game.logTurn?.Write(string.Format(" [Goal -> Move] {0}, ActID {1} -> Return to Capital -> {2}, LocID {3}", enemy.Name, enemy.ActID,
                                                   Game.world.GetLocationName(destinationLocID), destinationLocID));
                                    }
                                    //At Capital
                                    else if (enemy.LocID == 1)
                                    {
                                        for (int i = 0; i < listNeighbours.Count; i++)
                                        {
                                            if (listNeighbours[i] > 0)
                                            {
                                                Location locTemp = Game.network.GetLocation(listNeighbours[i]);
                                                if (locTemp != null)
                                                {
                                                    if (locTemp.GetBranch() == enemy.AssignedBranch)
                                                    {
                                                        destinationLocID = listNeighbours[i];
                                                        Game.logTurn?.Write(string.Format(" [Goal -> Move] {0}, ActID {1} -> Capital to Correct Branch -> {2}, LocID {3}", enemy.Name, enemy.ActID,
                                                        Game.world.GetLocationName(destinationLocID), destinationLocID));
                                                        break;
                                                    }
                                                }
                                                else { Game.SetError(new Error(156, "Invalid Loc (null) Enemy Goal not set")); }
                                            }
                                            else
                                            { Game.SetError(new Error(156, "Invalid LocID (zero or less) [at Capital] in ListOfNeighbours")); }
                                        }
                                    }
                                }
                                else
                                { Game.SetError(new Error(156, "Invalid branch value (default of -1)")); }
                            }
                            //valid destination found ? -> otherwise assign random neighbour
                            if (destinationLocID == 0)
                            {
                                destinationLocID = listNeighbours[rnd.Next(0, listNeighbours.Count)];
                                Game.logTurn?.Write(string.Format(" [Goal -> Alert] No valid destination found for {0}, ActID {1}. Assigned Random neighbour, {2}, LocID {3}", enemy.Name, enemy.ActID,
                                    Game.world.GetLocationName(destinationLocID), destinationLocID));
                            }
                            //Move enemy
                            Location locMove = Game.network.GetLocation(destinationLocID);
                            if (locMove != null)
                            {
                                Position posDestination = locMove.GetPosition();
                                Game.world.InitialiseMoveActor(enemy.ActID, posOrigin, posDestination);
                            }
                            else { Game.SetError(new Error(156, "Invalid locMove (null) Enemy isn't Moved")); }
                        }
                    }
                    else { Game.SetError(new Error(156, string.Format("Invalid targetLocID (zero or less), existing goal retained for actID {0}", enemy.ActID))); }
                }
                else { Game.SetError(new Error(156, string.Format("Invalid Enemy Location (null), for LocID {0}, Enemy ID {1}", enemy.LocID, enemy.ActID))); }
            }
            else { Game.SetError(new Error(156, "Invalid enemy input (null), existing goal retained")); }
        }

        //new methods above here
    }
}
