using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;
using RLNET;
using Next_Game.Cartographic;

namespace Next_Game
{
    public enum ConflictState   //game specific states that are used for situations
    {
        None,
        Relative_Army_Size, //battle conflicts
        Relative_Fame, //social conflicts
        Relative_Honour, //social conflicts
        Relative_Justice,
        Relative_Invisibility,
    }

    /// <summary>
    /// Handles the card Conflict system. Single class as there is only ever a single instance in existence.
    /// </summary>
    public class Conflict
    {
        static Random rnd;
        //protagonists
        Actor opponent;
        Active player;
        public bool Challenger { get; set; } //is the Player the Challenger?
        int numberOfCards;
        private Challenge challenge;
        //type of conflict
        public ConflictType Conflict_Type { get; set; }
        public ConflictCombat Combat_Type { get; set; }
        public ConflictSocial Social_Type { get; set; }
        public ConflictStealth Stealth_Type { get; set; }
        //Game specific Situation
        public ConflictState Game_State { get; set; }
        public CardType Game_Type { get; set; }
        public string Game_Title { get; set; } //used for card title
        public string Game_Description { get; set; } //used for card breakdown
        //Card Pool
        private List<Card_Conflict> listCardPool;
        private List<Card_Conflict> listCardHand; //hand that will be played
        private List<Card_Conflict> listCardSpecials; //special, decision derived, situation cards
        private List<Snippet> listBreakdown; //description of card pool contents
        //skills
        public SkillType PrimarySkill { get; set; } //each skill level counts as 2 cards
        public SkillType OtherSkill_1 { get; set; } //only trait effects count
        public SkillType OtherSkill_2 { get; set; }
        //collections
        private int[] arrayPool;
        private int[] arrayModifiers; 
        private string[,] arraySituation;
        //three lists to consolidate into pool breakdown description
        private List<Snippet> listPlayerCards;
        private List<Snippet> listOpponentCards;
        private List<Snippet> listSituationCards;

        /// <summary>
        /// default Constructor
        /// </summary>
        public Conflict(int seed)
        {
            rnd = new Random(seed);
            numberOfCards = Game.constant.GetValue(Global.HAND_CARDS_NUM);
            listCardPool = new List<Card_Conflict>();
            listCardHand = new List<Card_Conflict>();
            listCardSpecials = new List<Card_Conflict>();
            listBreakdown = new List<Snippet>();
            //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
            arrayPool = new int[3]; //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
            arrayModifiers = new int[3]; //modifier (DM) for GetSituationCardNumber, 0/1/2 refer to the three situation cards (def adv/neutral/game specific) -> DM for 0 & 1, # of cards for Game ('2')
            arraySituation = new string[3, 3];
            //three lists to consolidate into pool breakdown description
            listPlayerCards = new List<Snippet>();
            listOpponentCards = new List<Snippet>();
            listSituationCards = new List<Snippet>();
            //get player (always the player vs. somebody else)
            player = Game.world.GetActiveActor(1);
            if (player != null) {}
            else { Game.SetError(new Error(84, "Player not found (null)")); }
            //pass Points matrix to Layout (only needed once)
            SetPoints();
        }
        
   
        /// <summary>
        /// Master method that handles all conflict set up. Returns true for a valid conflict, false otherwise, if a problem
        /// </summary>
        public bool InitialiseConflict()
        {
            //debug
            Console.WriteLine("Initialise {0} Challenge", Conflict_Type);
            if (GetChallenge() == true)
            {
                Game.layout.InitialiseData();
                SetPlayerStrategy();
                SetSkills();
                SetSituation(Game.director.GetSituationsNormal(), Game.director.GetSituationsGame());
                CheckSpecialSituations();
                SetOpponentStrategy();
                SetOutcome();
                SetCardPool();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine Strategy and send to Layout. Player strategies are always 0/1/2, Opponent 3/4/5
        /// </summary>
        private void SetPlayerStrategy()
        {
            string[] tempArray = challenge.GetStrategies();
            string[] playerArray = new string[6];
            if (tempArray.Length == 6)
            {
                if (Challenger == true)
                {
                    playerArray[0] = tempArray[0];
                    playerArray[1] = tempArray[1];
                    playerArray[2] = tempArray[2];
                    playerArray[3] = tempArray[3];
                    playerArray[4] = tempArray[4];
                    playerArray[5] = tempArray[5];
                }
                else
                {
                    //player is defending (different strategy texts)
                    playerArray[0] = tempArray[3];
                    playerArray[1] = tempArray[4];
                    playerArray[2] = tempArray[5];
                    playerArray[3] = tempArray[0];
                    playerArray[4] = tempArray[1];
                    playerArray[5] = tempArray[2];
                }
                //pass data to Layout
                Game.layout.SetStrategy(playerArray);
            }
            else
            { Game.SetError(new Error(86, "Invalid Strategy (incorrect array size), Layout not updated")); }
        }

        /// <summary>
        /// Gets relevant challenge from dictionary. Returns true if valid challenge found.
        /// </summary>
        private bool GetChallenge()
        {
            bool proceed = true;
            ConflictSubType subType = ConflictSubType.None;
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    switch(Combat_Type)
                    {
                        case ConflictCombat.Personal:
                            subType = ConflictSubType.Personal;
                            break;
                        case ConflictCombat.Tournament:
                            subType = ConflictSubType.Tournament;
                            break;
                        case ConflictCombat.Battle:
                            subType = ConflictSubType.Battle;
                            break;
                        case ConflictCombat.Hunting:
                            subType = ConflictSubType.Hunting;
                            break;
                        default:
                            Game.SetError(new Error(111, "Invalid Combat_Type"));
                            proceed = false;
                            break;
                    }
                    break;
                case ConflictType.Social:
                    switch(Social_Type)
                    {
                        case ConflictSocial.Befriend:
                            subType = ConflictSubType.Befriend;
                            break;
                        case ConflictSocial.Blackmail:
                            subType = ConflictSubType.Blackmail;
                            break;
                        case ConflictSocial.Seduce:
                            subType = ConflictSubType.Seduce;
                            break;
                        default:
                            Game.SetError(new Error(111, "Invalid Social_Type"));
                            proceed = false;
                            break;
                    }
                    break;
                case ConflictType.Stealth:
                    switch(Stealth_Type)
                    {
                        case ConflictStealth.Infiltrate:
                            subType = ConflictSubType.Infiltrate;
                            break;
                        case ConflictStealth.Evade:
                            subType = ConflictSubType.Evade;
                            break;
                        case ConflictStealth.Escape:
                            subType = ConflictSubType.Escape;
                            break;
                        default:
                            Game.SetError(new Error(111, "Invalid Stealth_Type"));
                            proceed = false;
                            break;
                    }
                    break;
                default:
                    Game.SetError(new Error(111, "Invalid Conflict_Type"));
                    proceed = false;
                    break;
            }
            if (proceed == true)
            {
                challenge = Game.director.GetChallenge(subType);
                if (challenge == null)
                { Game.SetError(new Error(111, "Challenge not found (returns null)")); return false; }
                else { return true; }
            }
            else
            { Game.SetError(new Error(111, "Challenge not found, invalid Conflict_Type or SubType")); return false; }
        }

        
        /// <summary>
        /// Determine opponent's strategy and send to layout (attack/balanced/defend)
        /// </summary>
        private void SetOpponentStrategy()
        {
            //placeholder 
            Game.layout.Strategy_Opponent = rnd.Next(0, 3);
        }

        /// <summary>
        /// specify opponent (it's always the player vs. opponent). NOTE: Run BEFORE SetSpecialSituation
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="challenger">true if player is the challenger</param>
        internal void SetOpponent(int actorID, bool challenger)
        {
            if (actorID > 0)
            {
                opponent = Game.world.GetAnyActor(actorID);
                this.Challenger = challenger;
                Game.layout.Challenger = Challenger;
                if (opponent == null)
                { Game.SetError(new Error(88, "Opponent not found (null)")); }
            }
            else { Game.SetError(new Error(88, "Invalid actorID input (<= 0)")); }
        }

        /// <summary>
        /// Set up the situation (text descriptors) (first two situations only) -> NOTE: arraySituation[2] is here for Debug purposes only, it should instead come from SetGameSituation
        /// </summary>
        private void SetSituation(Dictionary<int, Situation> normalDictionary, Dictionary<int, Situation> gameDictionary)
        {
            if (normalDictionary.Count > 0)
            {
                List<string> tempListGood = new List<string>();
                List<string> tempListBad = new List<string>();
                bool invalidData = false;
                //is the first situation (def. adv.) a good or bad card for the player?
                int whoDefends;
                if (Challenger == true) { whoDefends = -1; }
                else { whoDefends = 1; }
                switch (Conflict_Type)
                {
                    case ConflictType.Combat:
                        //Get suitable situations from dictionary
                        List<Situation> listFilteredCombatSituations = new List<Situation>();
                        IEnumerable<Situation> situationCombatSet =
                            from situation in normalDictionary
                            where situation.Value.Type == ConflictType.Combat
                            where situation.Value.Type_Combat == Combat_Type
                            //orderby situation.Value.SitID
                            select situation.Value;
                        listFilteredCombatSituations = situationCombatSet.ToList();
                        if (listFilteredCombatSituations.Count == 0) { Game.SetError(new Error(89, "listFilteredCombatSituations has no data")); }
                        else
                        {
                            //filter for Defensive Advantage card (1st situation)
                            List<Situation> listFirstSituations = new List<Situation>();
                            IEnumerable<Situation> firstSituationSet =
                                from situation in listFilteredCombatSituations
                                where situation.SitNum == 0
                                where situation.Defender == whoDefends
                                select situation;
                            listFirstSituations = firstSituationSet.ToList();
                            if (listFirstSituations.Count == 0) { Game.SetError(new Error(89, "listFirstSituations (Combat) has no data")); invalidData = true; }

                            if (invalidData == false)
                            {
                                Situation situationFirst = listFirstSituations[rnd.Next(0, listFirstSituations.Count)];
                                tempListGood = situationFirst.GetGood();
                                tempListBad = situationFirst.GetBad();
                                //place data in array (name and immersion texts for played/ignored outcomes)
                                arraySituation[0, 0] = situationFirst.Name;
                                arraySituation[0, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                                arraySituation[0, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                            }
                            else
                            {
                                //set default empty data if nothing found
                                arraySituation[0, 0] = "";
                                arraySituation[0, 1] = "";
                                arraySituation[0, 2] = "";
                            }
                            //filter for Neutral card (2nd situation)
                            List<Situation> listSecondSituations = new List<Situation>();
                            IEnumerable<Situation> secondSituationSet =
                                from situation in listFilteredCombatSituations
                                where situation.SitNum == 1
                                select situation;
                            listSecondSituations = secondSituationSet.ToList();
                            if (listSecondSituations.Count == 0)
                            {
                                //set default empty data if nothing found
                                Game.SetError(new Error(89, "listSecondSituations (Combat) has no data"));
                                arraySituation[1, 0] = "";
                                arraySituation[1, 1] = "";
                                arraySituation[1, 2] = "";
                            }
                            else
                            {
                                Situation situationSecond = listSecondSituations[rnd.Next(0, listSecondSituations.Count)];
                                tempListGood = situationSecond.GetGood();
                                tempListBad = situationSecond.GetBad();
                                //place data in array (name and immersion texts for played/ignored outcomes)
                                arraySituation[1, 0] = situationSecond.Name;
                                arraySituation[1, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                                arraySituation[1, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                            }
                        }
                        break;
                    case ConflictType.Social:
                        //Get suitable situations from dictionary
                        List<Situation> listFilteredSocialSituations = new List<Situation>();
                        IEnumerable<Situation> situationSocialSet =
                            from situation in normalDictionary
                            where situation.Value.Type == ConflictType.Social
                            where situation.Value.Type_Social == Social_Type
                            //orderby situation.Value.SitID
                            select situation.Value;
                        listFilteredSocialSituations = situationSocialSet.ToList();
                        if (listFilteredSocialSituations.Count == 0) { Game.SetError(new Error(89, "listFilteredSocialSituations has no data")); }
                        else
                        {
                            //filter for Defensive Advantage card (1st situation)
                            List<Situation> listFirstSituations = new List<Situation>();
                            IEnumerable<Situation> firstSituationSet =
                                from situation in listFilteredSocialSituations
                                where situation.SitNum == 0
                                where situation.Defender == whoDefends
                                select situation;
                            listFirstSituations = firstSituationSet.ToList();
                            if (listFirstSituations.Count == 0) { Game.SetError(new Error(89, "listFirstSituations (Social) has no data")); invalidData = true; }

                            if (invalidData == false)
                            {
                                Situation situationFirst = listFirstSituations[rnd.Next(0, listFirstSituations.Count)];
                                tempListGood = situationFirst.GetGood();
                                tempListBad = situationFirst.GetBad();
                                //place data in array (name and immersion texts for played/ignored outcomes)
                                arraySituation[0, 0] = situationFirst.Name;
                                arraySituation[0, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                                arraySituation[0, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                            }
                            else
                            {
                                //set default empty data if nothing found
                                arraySituation[0, 0] = "";
                                arraySituation[0, 1] = "";
                                arraySituation[0, 2] = "";
                            }
                            //filter for Neutral card (2nd situation)
                            List<Situation> listSecondSituations = new List<Situation>();
                            IEnumerable<Situation> secondSituationSet =
                                from situation in listFilteredSocialSituations
                                where situation.SitNum == 1
                                select situation;
                            listSecondSituations = secondSituationSet.ToList();
                            if (listSecondSituations.Count == 0)
                            {
                                //set default empty data if nothing found
                                Game.SetError(new Error(89, "listSecondSituations (Social) has no data"));
                                arraySituation[1, 0] = "";
                                arraySituation[1, 1] = "";
                                arraySituation[1, 2] = "";
                            }
                            else
                            {
                                Situation situationSecond = listSecondSituations[rnd.Next(0, listSecondSituations.Count)];
                                tempListGood = situationSecond.GetGood();
                                tempListBad = situationSecond.GetBad();
                                //place data in array (name and immersion texts for played/ignored outcomes)
                                arraySituation[1, 0] = situationSecond.Name;
                                arraySituation[1, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                                arraySituation[1, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                            }
                        }
                        break;
                    default:
                        Game.SetError(new Error(86, "Invalid Conflict Type"));
                        break;
                }
                //filter suitable game situation from dictionary
                List<Situation> listGameSituations = new List<Situation>();
                IEnumerable<Situation> situationSet =
                    from situation in gameDictionary
                    where situation.Value.State == Game_State
                    orderby situation.Value.SitNum
                    select situation.Value;
                listGameSituations = situationSet.ToList();
                if (listGameSituations.Count == 0) { Game.SetError(new Error(89, "listGameSituations has no data")); }
                else if (listGameSituations.Count == 3)
                {
                    string backupTitle = "unknown backup";
                    //list ordered in sequence sitNum -1/0/1 -> player adv / neutral / opponent adv in whatever game state is being measured
                    switch (Game_Type)
                    {
                        case CardType.Good:
                            tempListGood = listGameSituations[2].GetGood();
                            tempListBad = listGameSituations[2].GetBad();
                            backupTitle = listGameSituations[2].Name;
                            break;
                        case CardType.Neutral:
                            tempListGood = listGameSituations[1].GetGood();
                            tempListBad = listGameSituations[1].GetBad();
                            backupTitle = listGameSituations[1].Name;
                            break;
                        case CardType.Bad:
                            tempListGood = listGameSituations[0].GetGood();
                            tempListBad = listGameSituations[0].GetBad();
                            backupTitle = listGameSituations[0].Name;
                            break;
                        default:
                            Game.SetError(new Error(89, "invalid Game_Type"));
                            break;
                    }
                    //give correct title for game specific situation, if present
                    if (String.IsNullOrEmpty(Game_Title) || Game_Title == "unknown") { Game_Title = backupTitle; }
                    arraySituation[2, 0] = Game_Title;
                    arraySituation[2, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                    arraySituation[2, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                }
                else { Game.SetError(new Error(89, string.Format("listGameSituations has incorrect number of entries (\"{0}\")", listGameSituations.Count))); }
                //send to layout
                if (arraySituation.GetUpperBound(0) == 2 && arraySituation.GetUpperBound(0) > 0)
                { Game.layout.SetSituation(arraySituation); }
                else
                { Game.SetError(new Error(89, "Invalid Situation, Layout not updated")); }
            }
            else
            { Game.SetError(new Error(89, "Invalid Dictionary Input, dictionary null or empty")); }
        }

        /// <summary>
        /// returns a situation containing lists of good/bad immersion texts for the primary skill involved in the challenge.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        /// <param name="challenger">true if the Player is the challenger</param>
        /// <param name="playerSkill">true if referring to the Player's skill, false if it's your Opponents skill</param>
        /// <returns>null if no valid data</returns>
        private Situation GetSkillText(ConflictType type, int subType, bool challenger, bool playerSkill)
        {
            if (type > ConflictType.None)
            {
                if (subType > 0)
                {
                    Situation situation = null;
                    Dictionary<int, Situation> tempDictionary = Game.director.GetSituationsSkill();
                    //get dictionary of skill situations
                    if (tempDictionary != null && tempDictionary.Count > 0)
                    {
                        //work out correct Player defending status
                        int plyrDef = 1;
                        if (challenger == true) { plyrDef = -1; }
                        //work out who's point of view
                        int pointOfView = 1;
                        if (playerSkill == true) { pointOfView = 0; }
                        //get correct subtype
                        switch (type)
                        {
                            case ConflictType.Combat:
                                //Get suitable situations from dictionary
                                List<Situation> listCombatSituations = new List<Situation>();
                                IEnumerable<Situation> situationCombatSet =
                                    from sitTemp in tempDictionary
                                    where sitTemp.Value.Type == ConflictType.Combat
                                    where (int)sitTemp.Value.Type_Combat == subType
                                    where sitTemp.Value.Defender == plyrDef
                                    where sitTemp.Value.Data == pointOfView
                                select sitTemp.Value;
                                listCombatSituations = situationCombatSet.ToList();
                                //should be a single situation
                                if (listCombatSituations.Count == 1)
                                { situation = listCombatSituations[0]; }
                                else
                                { Game.SetError(new Error(104, "Incorrect count for Combat Situations (listSkillSituations)")); }
                                break;
                            case ConflictType.Social:
                                //Get suitable situations from dictionary
                                List<Situation> listSocialSituations = new List<Situation>();
                                IEnumerable<Situation> situationSocialSet =
                                    from sitTemp in tempDictionary
                                    where sitTemp.Value.Type == ConflictType.Social
                                    where (int)sitTemp.Value.Type_Social == subType
                                    where sitTemp.Value.Defender == plyrDef
                                    where sitTemp.Value.Data == pointOfView
                                    select sitTemp.Value;
                                listSocialSituations = situationSocialSet.ToList();
                                //should be a single situation
                                if (listSocialSituations.Count == 1)
                                { situation = listSocialSituations[0]; }
                                else
                                { Game.SetError(new Error(104, "Incorrect count for Social Situations (listSkillSituations)")); }
                                break;
                            case ConflictType.Stealth:
                                //Get suitable situations from dictionary
                                List<Situation> listOtherSituations = new List<Situation>();
                                IEnumerable<Situation> situationOtherSet =
                                    from sitTemp in tempDictionary
                                    where sitTemp.Value.Type == ConflictType.Stealth
                                    where (int)sitTemp.Value.Type_Stealth == subType
                                    where sitTemp.Value.Defender == plyrDef
                                    where sitTemp.Value.Data == pointOfView
                                    select sitTemp.Value;
                                listOtherSituations = situationOtherSet.ToList();
                                //should be a single situation
                                if (listOtherSituations.Count == 1)
                                { situation = listOtherSituations[0]; }
                                else
                                { Game.SetError(new Error(104, "Incorrect count for Other Situations (listSkillSituations)")); }
                                break;
                            default:
                                Game.SetError(new Error(104, string.Format("Invalid type (\"{0}\")", type)));
                                break;
                        }
                    }
                    else
                    { Game.SetError(new Error(104, "Invalid Dictionary input (null or empty)")); }
                    return situation;
                }
                else
                { Game.SetError(new Error(104, "Invalid subType input (Zero or less)")); }
            }
            else
            { Game.SetError(new Error(104, "Invalid ConflictType input (\"None\")")); }
            return null;
        }

        /// <summary>
        /// determine # of cards (worked on remainder / 8x / 4x / 2x / 1x for 1/2/3/4/5 cards). Only for the first two situations (def adv & neutral)
        /// </summary>
        /// <param name="modifier">Optional DM to die roll</param>
        /// <returns></returns>
        private int GetSituationCardNumber(int modifier = 0)
        {
            int numCards = 1;
            int rndNum = rnd.Next(100);
            int rndDebug = rndNum;
            //calculate spread
            int spreadNum = Game.constant.GetValue(Global.SIT_CARD_SPREAD);
            int remainder = 100 - spreadNum - spreadNum * 2 - spreadNum * 4 - spreadNum * 8;
            int upper = 100 - spreadNum - spreadNum * 2;
            int lower = upper - spreadNum * 4;
            int top = 100 - spreadNum;
            if (modifier != 0) { rndNum += modifier; }
            if (rndNum >= remainder)
            {
                if (rndNum >= top) { numCards = 5; }
                else if (rndNum >= upper) { numCards = 4; }
                else if (rndNum >= lower) { numCards = 3; }
                else { numCards = 2; }
            }
            //debug
            Console.WriteLine("SitCard: modifier {0}, rndNum {1}, net {2}, numCards {3}", modifier, rndDebug, rndNum, numCards);
            //return 
            return numCards;
        }

        /// <summary>
        /// Set up outcomes and send to layout
        /// </summary>
        private void SetOutcome()
        {
            string[] tempArray = new string[11];
            string[] outcomeArray = challenge.GetOutcomes();
            if (outcomeArray.Length == 7)
            {
                //win x 3 and loss x 3
                tempArray[1] = outcomeArray[0];
                tempArray[2] = outcomeArray[1];
                tempArray[3] = outcomeArray[2];
                tempArray[4] = outcomeArray[3];
                tempArray[5] = outcomeArray[4];
                tempArray[6] = outcomeArray[5];
                //no result
                tempArray[10] = outcomeArray[6];
            }
            else
            { Game.SetError(new Error(86, "Invalid outcome Array size")); }
            
            //descriptions of outcomes (minor/standard/major wins and losses)
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    tempArray[0] = string.Format("A {0} {1} Challenge", Combat_Type, Conflict_Type);
                    break;
                case ConflictType.Social:
                    tempArray[0] = string.Format("A {0} {1} Challenge", Social_Type, Conflict_Type);
                    break;
                case ConflictType.Stealth:
                    tempArray[0] = string.Format("A {0} {1} Challenge", Stealth_Type, Conflict_Type);
                    break;
                default:
                    Game.SetError(new Error(86, "Invalid Conflict Type"));
                    break;
            }
            //Who has the advantage and a recommendation
            if (Challenger == true)
            { tempArray[7] = "You are the Challenger and have the Advantage"; }
            else { tempArray[7] = "You are Defending"; }
            tempArray[8] = "An Aggressive Strategy is Recommended";
            //protagonists
            string title;
            if (opponent.Office == ActorOffice.None) { title = Convert.ToString(opponent.Type); }
            else { title = Convert.ToString(opponent.Office); }
            string handle_player, handle_opponent;
            if (player.Handle != null) { handle_player = string.Format(" \"{0}\" ", player.Handle); } else { handle_player = null; }
            if (opponent.Handle != null) { handle_opponent = string.Format(" \"{0}\" ", opponent.Handle); } else { handle_opponent = null; }
            //order protagnoists so that challenger is first and defender is second
            if (Challenger == true)
            { tempArray[9] = string.Format("{0} {1}{2} vs. {3} {4}{5}", player.Type, player.Name, handle_player, title, opponent.Name, handle_opponent); }
            else
            { tempArray[9] = string.Format("{0} {1}{2} vs. {3} {4}{5}", title, opponent.Name, handle_opponent, player.Type, player.Name, handle_player); }
            //send to layout
            Game.layout.SetOutcome(tempArray);
        }

        /// <summary>
        /// Set up Primary and Secondary Skills in use
        /// </summary>
        private void SetSkills()
        {
            SkillType[] tempArray = challenge.GetSkills();
            if (tempArray.Length == 3)
            {
                PrimarySkill = tempArray[0];
                OtherSkill_1 = tempArray[1];
                OtherSkill_2 = tempArray[2];
            }
            else { Game.SetError(new Error(91, "Invalid Skill array Length")); }
        }

        /// <summary>
        /// Set up Card Pool
        /// </summary>
        private void SetCardPool()
        {
            RLColor backColor = Game.layout.Back_FillColor;
            RLColor foreColor = RLColor.Black;
            CardType type;
            string text;
            int numCards, subType, goodIndex, badIndex;
            //immersion text lists
            List<string> tempListGood = new List<string>();
            List<string> tempListBad = new List<string>();
            //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
            Array.Clear(arrayPool, 0 , arrayPool.Length);
            //clear lists
            listCardPool.Clear();
            listPlayerCards.Clear();
            listOpponentCards.Clear();
            listSituationCards.Clear();
            //headers
            listPlayerCards.Add(new Snippet("Your Cards", RLColor.Blue, backColor));
            listOpponentCards.Add(new Snippet("Opponent's Cards", RLColor.Blue, backColor));
            listSituationCards.Add(new Snippet("Situation Cards", RLColor.Blue, backColor));
            //primary skills (2 cards per skill level)
            int skill_player = player.GetSkill(PrimarySkill, SkillAge.Fifteen, true);
            int skill_opponent = opponent.GetSkill(PrimarySkill, SkillAge.Fifteen, true);
            int cards_player = skill_player * 2;
            int cards_opponent = skill_opponent * 2;
            //create cards, add to pool and breakdown lists
            string description = "Get a Random Description from a Pool of Possibilities";
            //get subtype for Primary skills
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    subType = (int)Combat_Type;
                    break;
                case ConflictType.Social:
                    subType = (int)Social_Type;
                    break;
                case ConflictType.Stealth:
                    subType = (int)Stealth_Type;
                    break;
                default:
                    Game.SetError(new Error(105, string.Format("Unknown Conflict_Type (\"{0}\") -> subType set to 1 ", Conflict_Type)));
                    subType = 1; //set to a workable number to keep things going
                    break;
            }
            //...Player Primary skill
            type = CardType.Good; foreColor = RLColor.Black; arrayPool[0] += cards_player;
            //get relevant skill situation (for lists of immersion texts)
            Situation situationPlayer = GetSkillText(Conflict_Type, subType, Challenger, true);
            if (situationPlayer != null)
            { tempListGood = situationPlayer.GetGood(); tempListBad = situationPlayer.GetBad(); }
            else { Game.SetError(new Error(105, string.Format("situation (Player Primary Skill) came back Null", Conflict_Type))); }
            goodIndex = rnd.Next(tempListGood.Count);
            badIndex = rnd.Next(tempListBad.Count);
            for (int i = 0; i < cards_player; i++)
            {
                Card_Conflict cardPlayer = new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", player.Name, PrimarySkill), description);
                //get immersion texts, if present
                if (tempListGood.Count > 0) { cardPlayer.PlayedText = tempListGood[goodIndex]; }
                if (tempListBad.Count > 0) { cardPlayer.IgnoredText = tempListBad[badIndex]; }
                //increment and rollover indexes (so there's an equal distribution of immersion texts)
                goodIndex++; badIndex++;
                if (goodIndex == tempListGood.Count) { goodIndex = 0; }
                if (badIndex == tempListBad.Count) { badIndex = 0; }
                //add card to pool
                listCardPool.Add(cardPlayer);
            }
            text = string.Format("{0}'s {1} Skill ({2}), {3} cards, Primary Challenge skill ({4} stars) ", player.Name, PrimarySkill, type, cards_player, skill_player);
            listPlayerCards.Add(new Snippet(text, foreColor, backColor));

            //...Opponent Primary skill
            type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += cards_opponent;
            //get relevant skill situation (for lists of immersion texts)
            Situation situationOpponent = GetSkillText(Conflict_Type, subType, Challenger, false);
            if (situationOpponent != null)
            { tempListGood = situationOpponent.GetGood(); tempListBad = situationOpponent.GetBad(); }
            else { Game.SetError(new Error(105, string.Format("situation (Opponent Primary Skill) came back Null", Conflict_Type))); }
            goodIndex = rnd.Next(tempListGood.Count);
            badIndex = rnd.Next(tempListBad.Count);
            for (int i = 0; i < cards_opponent; i++)
            {
                Card_Conflict cardOpponent = new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", opponent.Name, PrimarySkill), description);
                //get immersion texts, if present
                if (tempListGood.Count > 0) { cardOpponent.PlayedText = tempListGood[goodIndex]; }
                if (tempListBad.Count > 0) { cardOpponent.IgnoredText = tempListBad[badIndex]; }
                //increment and rollover indexes (so there's an equal distribution of immersion texts)
                goodIndex++; badIndex++;
                if (goodIndex == tempListGood.Count) { goodIndex = 0; }
                if (badIndex == tempListBad.Count) { badIndex = 0; }
                //add card to pool
                listCardPool.Add(cardOpponent);
            }
            text = string.Format("{0}'s {1} Skill ({2}), {3} cards, Primary Challenge skill ({4} stars) ", opponent.Name, PrimarySkill, type, cards_opponent, skill_opponent);
            listOpponentCards.Add(new Snippet(text, foreColor, backColor));

            //Primary Skill Traits
            CheckActorTrait(player, PrimarySkill, listPlayerCards);
            CheckActorTrait(opponent, PrimarySkill, listOpponentCards);
            //Secondary Skill Traits
            CheckActorTrait(player, OtherSkill_1, listPlayerCards);
            CheckActorTrait(player, OtherSkill_2, listPlayerCards);
            CheckActorTrait(opponent, OtherSkill_1, listOpponentCards);
            CheckActorTrait(opponent, OtherSkill_2, listOpponentCards);

            //Situation Cards
            numCards = GetSituationCardNumber(arrayModifiers[0]);
            //...advantage defender card -> First situation
            if (Challenger == true) { type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += numCards; }
            else { type = CardType.Good; foreColor = RLColor.Black; arrayPool[0] += numCards; }
            for (int i = 0; i < numCards; i++)
            {
                //only applies if Defender chooses an [F3] Defence strategy
                Card_Conflict card = new Card_Conflict(CardConflict.Situation, type, string.Format("{0}", arraySituation[0, 0]), "An advantage to the Defender") { TypeDefend = CardType.Good };
                card.PlayedText = arraySituation[0, 1]; card.IgnoredText = arraySituation[0, 2];
                listCardPool.Add(card);
            }
            text = string.Format("\"{0}\", {1} card{2}. ONLY AVAILABLE if Defender ({3}) chooses an [F3] Strategy", arraySituation[0, 0], numCards, numCards > 1 ? "s" : "",
                Challenger == false ? "Player" : "Opponent");
            listSituationCards.Add(new Snippet(text, foreColor, backColor));
            //...neutral card -> Second Situation
            numCards = GetSituationCardNumber(arrayModifiers[1]);
            type = CardType.Neutral; foreColor = RLColor.Magenta; arrayPool[1] += numCards;
            for (int i = 0; i < numCards; i++)
            {
                Card_Conflict card = new Card_Conflict(CardConflict.Situation, type, string.Format("{0}", arraySituation[1, 0]), "Could go either way...");
                card.PlayedText = arraySituation[1, 1]; card.IgnoredText = arraySituation[1, 2];
                listCardPool.Add(card);
            }
            text = string.Format("\"{0}\", {1} card{2}, A Neutral Card (Good if played, Bad if ignored)", arraySituation[1, 0], numCards, numCards > 1 ? "s" : "");
            listSituationCards.Add(new Snippet(text, foreColor, backColor));
            //...Game Specific Situation
            if (Game_Type != CardType.None)
            {
                string textCard = "Unknown";
                numCards = arrayModifiers[2];
                type = Game_Type;
                if (type == CardType.Bad) { foreColor = RLColor.Red; arrayPool[2] += numCards; textCard = "Advantage Opponent"; }
                else if (type == CardType.Good) { foreColor = RLColor.Black; arrayPool[0] += numCards; textCard = "Advantage Player"; }
                else if (type == CardType.Neutral) { foreColor = RLColor.Magenta; arrayPool[1] += numCards; textCard = "Could go either way..."; }
                for (int i = 0; i < numCards; i++)
                {
                    Card_Conflict card = new Card_Conflict(CardConflict.Situation, type, string.Format("{0}", arraySituation[2, 0]), textCard);
                    card.PlayedText = arraySituation[2, 1]; card.IgnoredText = arraySituation[2, 2];
                    listCardPool.Add(card);
                }
                text = string.Format("\"{0}\", {1} card{2} ({3}), {4}", arraySituation[2, 0], numCards, numCards > 1 ? "s" : "", Game_Type, Game_Description);
                listSituationCards.Add(new Snippet(text, foreColor, backColor));
            }
            else { arraySituation[2, 0] = ""; }

            //Special Situations
            if (listCardSpecials.Count > 0)
            {
                numCards = listCardSpecials.Count();
                string oldTitle = "";
                string newTitle = "";
                string explanation = string.Format("ONLY AVAILABLE if Defender ({0}) chooses an [F3] Strategy", Challenger == false ? "Player" : "Opponent");
                for (int i = 0; i < numCards; i++)
                {
                    Card_Conflict cardSpecial = listCardSpecials[i];
                    newTitle = cardSpecial.Title;
                    type = cardSpecial.Type;
                    if (type == CardType.Good) { foreColor = RLColor.Black; arrayPool[0] += 1;}
                    else if (type == CardType.Bad) { foreColor = RLColor.Red;  arrayPool[2] += 1; }
                    if (newTitle != oldTitle)
                    {
                        //number of cards of this special type
                        int counter = 0;
                        foreach (Card_Conflict tempCard in listCardSpecials)
                        { if (tempCard.TypeSpecial == cardSpecial.TypeSpecial) { counter++; } }
                        //add card
                        oldTitle = newTitle;
                        text = string.Format("\"{0}\", {1} card{2}, {3}{4}", cardSpecial.Title, counter, counter > 1 ? "s" : "", explanation, 
                            cardSpecial.Unique > CardUnique.None ? " (" + cardSpecial.UniqueText + ")" : "");
                        listSituationCards.Add(new Snippet(text, foreColor, backColor));
                    }
                    listCardPool.Add(cardSpecial);
                }
            }
            //clear master list and add headers
            listBreakdown.Clear();
            //consolidate lists
            listBreakdown.AddRange(listPlayerCards);
            listBreakdown.Add(new Snippet(""));
            listBreakdown.AddRange(listOpponentCards);
            listBreakdown.Add(new Snippet(""));
            listBreakdown.AddRange(listSituationCards);
            //send to layout
            Game.layout.SetCardBreakdown(listBreakdown);
            Game.layout.SetCardPool(arrayPool);
            //empty special situations ready for next conflict
            listCardSpecials.Clear();
        }

        /// <summary>
        /// internal submethod used to check an actor's trait and add cards to the appropriate pools
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="skill"></param>
        private void CheckActorTrait(Actor actor, SkillType skill, List<Snippet> cardList)
        {
            CardType type;
            int numCards;
            int tempNumCards;
            string text;
            string traitExpanded;
            string description = "Get a Random Description from a Pool of Possibilities";
            RLColor backColor = Game.layout.Back_FillColor;
            RLColor foreColor = RLColor.Black;
            //get skill level -> NOT the influenced skill
            int skill_actor = actor.GetSkill(skill);
            if (actor.ActID == 1)
            {
                numCards = skill_actor - 3;
                traitExpanded = string.Format("{0} skill {1}{2}", skill, numCards > 0 ? "+" : "", numCards);
            }
            else
            {
                //reverse the sign (good/bad) if opponent (who is anyone else other than the Usurper)
                numCards = 3 - skill_actor;
                tempNumCards = numCards * -1;
                traitExpanded = string.Format("{0} skill {1}{2}", skill, tempNumCards > 0 ? "+" : "", tempNumCards);
            }
            //ignore if no trait present (skill level = 3)
            if (numCards != 0)
            {
                string traitText = actor.GetTrait(skill);
                if (numCards > 0)
                {
                    //positive trait
                    type = CardType.Good; foreColor = RLColor.Black; arrayPool[0] += numCards;
                    for (int i = 0; i < numCards; i++)
                    { listCardPool.Add(new Card_Conflict(CardConflict.Trait, type, string.Format("{0}'s {1} Trait", actor.Name, traitText), description)); }
                    text = string.Format("{0}'s {1} Trait ({2}), {3} card{4}, ({5})", actor.Name, traitText, type, numCards, numCards > 1 ? "s" : "", traitExpanded);
                    cardList.Add(new Snippet(text, foreColor, backColor));
                }
                else
                {
                    //negative trait
                    numCards = Math.Abs(numCards);
                    type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += numCards;
                    for (int i = 0; i < numCards; i++)
                    { listCardPool.Add(new Card_Conflict(CardConflict.Trait, type, string.Format("{0}'s {1} Trait", actor.Name, traitText), description)); }
                    text = string.Format("{0}'s {1} Trait ({2}), {3} card{4}, ({5})", actor.Name, traitText, type, numCards, numCards > 1 ? "s" : "", traitExpanded);
                    cardList.Add(new Snippet(text, foreColor, backColor));
                }
            }
        }

        /// <summary>
        /// Called from layout and updates pool as a result of chosen strategies
        /// </summary>
        /// <param name="defenderStrategy"></param>
        public void UpdateCardPool(int defenderStrategy)
        {
            //only need to update if defender hasn't chosen a defensive [F3] strategy
            if (defenderStrategy != 2)
            {
                //reverse loop card pool
                for (int i = listCardPool.Count() - 1; i >= 0; i--)
                {
                    Card_Conflict card = listCardPool[i];
                    //remove any situation cards that advantage the defender
                    if (card.Conflict_Type == CardConflict.Situation && card.TypeDefend == CardType.Good)
                    {
                        //update pool stats
                        if (card.Type == CardType.Good) { arrayPool[0]--; }
                        else if (card.Type == CardType.Bad) { arrayPool[2]--; }
                        //remove card from pool
                        listCardPool.RemoveAt(i);
                    }
                }
                //send updated Card pool array to Layout
                Game.layout.SetCardPool(arrayPool);
            }
        }

        /// <summary>
        /// Draw cards to form a challenge hand (called from Layout 'cause of variable situation cards)
        /// </summary>
        public void SetHand()
        {
            //check enough cards in pool, if not downsize hand
            if (numberOfCards > listCardPool.Count)
            { numberOfCards = listCardPool.Count; Game.SetError(new Error(93, "Not enough cards in the Pool")); }
            //clear out the list
            listCardHand.Clear();
            int rndNum;
            for (int i = 0; i < numberOfCards; i++)
            {
                rndNum = rnd.Next(0, listCardPool.Count);
                Card_Conflict card = listCardPool[rndNum];
                listCardHand.Add(card);
                //delete record as you don't want repeats
                listCardPool.RemoveAt(rndNum);
            }
            //send to layout
            Game.layout.SetCardHand(listCardHand);
        }

        /// <summary>
        /// Passes a matrix of points (strategy vs. strategy) over to Layout
        /// </summary>
        private void SetPoints()
        {
            int[,,] tempArray = new int[3,3,4];
            //attk vs attk (good play/good ignore/bad ignore/bad play)
            tempArray[0, 0, 0] = 12;
            tempArray[0, 0, 1] = 4;
            tempArray[0, 0, 2] = -4;
            tempArray[0, 0, 3] = -12;
            //attk vs balanced
            tempArray[0, 1, 0] = 10;
            tempArray[0, 1, 1] = 3;
            tempArray[0, 1, 2] = -1;
            tempArray[0, 1, 3] = -6;
            //attk vs. Def
            tempArray[0, 2, 0] = 8;
            tempArray[0, 2, 1] = 2;
            tempArray[0, 2, 2] = 0;
            tempArray[0, 2, 3] = -4;

            //bal vs. attk
            tempArray[1, 0, 0] = 6;
            tempArray[1, 0, 1] = 1;
            tempArray[1, 0, 2] = -3;
            tempArray[1, 0, 3] = -10;
            //bal vs balanced
            tempArray[1, 1, 0] = 8;
            tempArray[1, 1, 1] = 2;
            tempArray[1, 1, 2] = -2;
            tempArray[1, 1, 3] = -8;
            //bal vs. Def
            tempArray[1, 2, 0] = 6;
            tempArray[1, 2, 1] = 1;
            tempArray[1, 2, 2] = 0;
            tempArray[1, 2, 3] = -4;

            //def vs. attk
            tempArray[2, 0, 0] = 4;
            tempArray[2, 0, 1] = 0;
            tempArray[2, 0, 2] = -2;
            tempArray[2, 0, 3] = -8;
            //def vs balanced
            tempArray[2, 1, 0] = 6;
            tempArray[2, 1, 1] = 1;
            tempArray[2, 1, 2] = -2;
            tempArray[2, 1, 3] = -8;
            //def vs. Def
            tempArray[2, 2, 0] = 4;
            tempArray[2, 2, 1] = 0;
            tempArray[2, 2, 2] = 0;
            tempArray[2, 2, 3] = -4;
            //send to layout
            Game.layout.SetPoints(tempArray);
        }
        
    
        /// <summary>
        /// pass details of the third, game specific, situation. Run before Conflict.Initialise
        /// </summary>
        /// <param name="gameState">Game specific state</param>
        /// <param name="title"> leave blank if you want the situation name as the card title</param>
        /// <param name="mod_0">Modifier to def adv situation card #'s (first)</param>
        /// <param name="mod_1">Modifier to neutral situation card #'s (second)</param>
        public void SetGameSituation(ConflictState gameState, string title = null, int mod_0 = 0, int mod_1 = 0)
        {
            int numCards;
            int modifier = 0;
            int difference = 0;
            string description = "unknown";
            Game_State = gameState;
            if (String.IsNullOrEmpty(title) == false)
            { Game_Title = title; }
            
            //determine card #'s (modifier), type (difference) and Description (description)
            switch (gameState)
            {
                case ConflictState.Relative_Army_Size:
                    int factor = Game.constant.GetValue(Global.ARMY_SIZE);
                    //Difference in two sides Armies - PLACEHOLDER
                    difference = rnd.Next(1, 101) * factor - rnd.Next(1, 101) * factor;
                    modifier = Math.Abs(difference / factor);
                    description = string.Format("{0} {1:N0} more Men-At-Arms than {2}", difference > 0 ? "You have" : "The King has", modifier * factor, difference > 0 ? "the King" : "You");
                    break;
                case ConflictState.Relative_Fame:
                    //Usurpers Legend - Kings Legend
                    difference = Game.director.CheckGameState(DataPoint.Legend_Usurper) - Game.director.CheckGameState(DataPoint.Legend_King);
                    modifier = Math.Abs(difference);
                    description = string.Format("difference between Your's and the King's Legend is {0}{1} %", difference > 0 ? "+" : "", difference);
                    break;
                case ConflictState.Relative_Honour:
                    //Usurpers Honour - Kings Honour
                    difference = Game.director.CheckGameState(DataPoint.Honour_Usurper) - Game.director.CheckGameState(DataPoint.Honour_King);
                    modifier = Math.Abs(difference);
                    description = string.Format("difference between Your's and the King's Honour is {0}{1} %", difference > 0 ? "+" : "", difference);
                    break;
                case ConflictState.Relative_Justice:
                    //Relative Justice of the Usurpers Cause
                    difference = Game.director.CheckGameState(DataPoint.Justice) - 50;
                    modifier = Math.Abs(difference);
                    description = string.Format("relative Justice of Your Cause is {0}{1} %", difference > 0 ? "+" : "", difference);
                    break;
                case ConflictState.Relative_Invisibility:
                    //Relative Invisibility of the Usurper
                    difference = Game.director.CheckGameState(DataPoint.Invisibility) - 50;
                    modifier = Math.Abs(difference);
                    description = string.Format("relative Invisibility of Your Cause is {0}{1} %", difference > 0 ? "+" : "", difference);
                    break;
            }
            //First couple of situation modifiers
            arrayModifiers[0] = mod_0;
            arrayModifiers[1] = mod_1;
            // '2' contains the actual number of cards (random for 0 & 1 but fixed for game specific situation)
            numCards = modifier / 20 + 1;
            numCards = Math.Min(5, numCards);
            arrayModifiers[2] = numCards;
            //debug
            Console.WriteLine("Situation: \"{0}\", difference {1} Modifier {2} numCards {3}", description, difference, modifier, numCards);
            //Type (if difference between -10 & +10 then card is neutral
            if (difference > 10) { Game_Type = CardType.Good; }
            else if (difference < -10) { Game_Type = CardType.Bad; }
            else { Game_Type = CardType.Neutral; }
            //description
            Game_Description = description;
            //let layout know what type of situation card 
            Game.layout.GameSituation = Game_Type;
        }

        /// <summary>
        /// get opponent in conflict
        /// </summary>
        /// <returns></returns>
        public Actor GetOpponent()
        { return opponent; }

        /// <summary>
        /// Input special, decision derived, situations. Always def.Adv. cards that help the defender. NOTE: Run AFTER SetOpponent()
        /// </summary>
        /// <param name="specialType"></param>
        /// <param name="numCards">leave at default '0' if you want a random #</param>
        /// <param name="unique">Add the enum for a unique card property</param>
        public void SetSpecialSituation(ConflictSpecial specialType, int numCards = 0, CardUnique unique = CardUnique.None)
        {
            //get dictionary of specials
            Dictionary<int, Situation> specialDictionary = Game.director.GetSituationsSpecial();
            if (specialDictionary.Count > 0)
            {
                if (specialType > ConflictSpecial.None)
                {
                    
                    int defender = 1;
                    CardType type = CardType.Good;
                    if (Challenger == true) { defender = -1; type = CardType.Bad; }
                    //Get special situations from dictionary
                    List<Situation> listFilteredSituations = new List<Situation>();
                    IEnumerable<Situation> situationSet =
                        from situation in specialDictionary
                        where situation.Value.Special == specialType
                        where situation.Value.Defender == defender
                            //orderby situation.Value.SitID
                            select situation.Value;
                    listFilteredSituations = situationSet.ToList();
                    //should be a single situation in the list
                    if (listFilteredSituations.Count == 1)
                    {
                        Situation situation = listFilteredSituations[rnd.Next(0, listFilteredSituations.Count)];
                        List<string> tempListGood = situation.GetGood();
                        List<string> tempListBad = situation.GetBad();
                        //check that the special situation isn't already present
                        bool proceed = true;
                        if (listCardSpecials.Count > 0)
                        {
                            foreach (Card_Conflict card in listCardSpecials)
                            {
                                if (specialType == card.TypeSpecial)
                                { proceed = false; Game.SetError(new Error(102, "Special Situation already exists")); break; }
                            }
                        }
                        //no duplicates
                        if (proceed == true)
                        {
                            if (numCards == 0) { numCards = GetSituationCardNumber(); }
                            int number = numCards;
                            //debug
                            Console.WriteLine("SetSpecialSituation: \"{0}\", numCards {1}, Added to listCardsSpecial", situation.Name, numCards);
                            //add cards to special card list
                            for (int i = 0; i < number; i++)
                            {
                                Card_Conflict card = new Card_Conflict(CardConflict.Situation, type, situation.Name, "A Special Situation");
                                //Console.WriteLine("SPECIAL Situation \"{0}\" -> {1}, Challenger {2}", card.Title, card.Type, Challenger);
                                card.PlayedText = tempListGood[rnd.Next(0, tempListGood.Count)];
                                card.IgnoredText = tempListBad[rnd.Next(0, tempListBad.Count)];
                                card.TypeDefend = CardType.Good; //as long as it's not CardType.None
                                card.TypeSpecial = specialType;
                                if (unique > CardUnique.None) { SetUniqueCardAbility(card, unique); }
                                listCardSpecials.Add(card);
                            }
                        }
                    }
                    else { Game.SetError(new Error(102, "No (or too many) special situations found that match criteria")); }
                }
                else { Game.SetError(new Error(102, "Invalid ConflictSpecial Input (\"None\")")); }
            }
            else { Game.SetError(new Error(102, "Invalid Dictionary Input (empty)")); }
        }

        /// <summary>
        /// checks position of player and automatically adds any relevant special situations -> cityWalls, forest & mountain terrain (adds a random # of cards)
        /// </summary>
        private void CheckSpecialSituations()
        {
            //get player as they are involved in all conflicts
            Active player = Game.world.GetActiveActor(1);
            if (player != null)
            {
                //Only check for conflict types where automatic special situations are possible
                if (Conflict_Type == ConflictType.Combat)
                {
                    //Battles are terrain dependant
                    if (Combat_Type == ConflictCombat.Battle)
                    {
                        Position pos = player.GetActorPosition();
                        if (player.Status == ActorStatus.Travelling)
                        {
                            //get terrain type (1 - Mountain, 2 - Forest)
                            int terrain = Game.map.GetMapInfo(MapLayer.Terrain, pos.PosX, pos.PosY);
                            if (terrain == 1) { SetSpecialSituation(ConflictSpecial.Mountain_Country, 0); }
                            else if (terrain == 2) { SetSpecialSituation(ConflictSpecial.Forest_Country, 0); }
                        }
                        else if (player.Status == ActorStatus.AtLocation)
                        {
                            if (player.LocID == 1) { SetSpecialSituation(ConflictSpecial.Castle_Walls, 5); } //Kings keep, max castle walls
                            else
                            {
                                int refID = Game.map.GetMapInfo(MapLayer.RefID, pos.PosX, pos.PosY);
                                if (refID > 0)
                                {
                                    House house = Game.world.GetHouse(refID);
                                    if (refID < 100)
                                    {
                                        //Great house
                                        SetSpecialSituation(ConflictSpecial.Castle_Walls, house.CastleWalls, CardUnique.Effect2X);
                                    }
                                    else if (refID > 99 && refID < 1000)
                                    {
                                        //BannerLord, weak walls, always 1
                                        SetSpecialSituation(ConflictSpecial.Castle_Walls, house.CastleWalls, CardUnique.Effect2X);
                                    }
                                }
                                else { Game.SetError(new Error(97, "RefID comes back ZERO, no Auto Special Situation created")); }
                            }
                        }
                    }
                }
            }
            else { Game.SetError(new Error(97, "Player not found (null), no Auto Special Situation created")); }
        }

        /// <summary>
        /// Initialise Unique Card Ability (max one per card)
        /// </summary>
        /// <param name="card"></param>
        /// <param name="unique"></param>
        private void SetUniqueCardAbility(Card_Conflict card, CardUnique unique)
        {
            if (card != null)
            {
                if (card.Unique == CardUnique.None)
                {
                    if (unique != CardUnique.None)
                    {
                        //assign unique quality
                        card.Unique = unique;
                        switch (unique)
                        {
                            case CardUnique.Effect2X:
                                card.Effect = 2;
                                card.UniqueText = "Double Effect";
                                break;
                        }
                    }
                    else
                    { Game.SetError(new Error(103, "Invalid Unique Input (\"None\")")); }
                }
                else
                { Game.SetError(new Error(103, "Card already has a Unique Ability")); }
            }
            else
            { Game.SetError(new Error(103, "Invalid card Input (null)")); }
        }

        /// <summary>
        /// Resolves results for the particular outcome acheived
        /// </summary>
        /// <param name="result"></param>
        /// <returns>A snippet list describing each individual result, suitable for display, green for good, red for bad</returns>
        internal List<Snippet> ResolveResults(ConflictResult outcome)
        {
            List<Snippet> tempList = new List<Snippet>();
            List<int> resultList = challenge.GetResults(outcome);
            int resultID;
            if (resultList.Count > 0)
            {
                for(int i = 0; i < resultList.Count; i++)
                {
                    resultID = resultList[i];
                    Result result = Game.director.GetResult(resultID);
                    if (result != null)
                    {
                        ResultType type = result.Type;
                        int data = result.Data;
                        int amount = result.Amount;
                        EventCalc calc = result.Calc;
                        RLColor backColor = Game.layout.Outcome_FillColor;
                        //check for a random outcome -> then it's random 'amount', eg. amount is 100 then it's d100
                        if (calc == EventCalc.Random)
                        {
                            if (amount > 1) { amount = rnd.Next(1, amount); calc = EventCalc.Add; }
                            else if (amount < -1) { amount = rnd.Next(-1, amount); calc = EventCalc.Add; }
                        }
                        //resolve the actual result
                        switch(type)
                        {
                            case ResultType.DataPoint:
                                Console.WriteLine("A {0} Result, ID {1}, Data {2}, Calc {3}, Amount {4}", type, result.ResultID, result.Data, result.Calc, result.Amount);
                                if (result.DataPoint > DataPoint.None)
                                {
                                    if (amount != 0)
                                    {
                                        //automatic ADD by amount. If Data > 0 then Good, otherwise Bad
                                        if (data > 0)
                                        {
                                            int oldValue = Game.director.GetGameState(result.DataPoint, DataState.Good);
                                            int newValue = Math.Abs(amount) + oldValue;
                                            Game.director.SetGameState(result.DataPoint, DataState.Good, newValue, true);
                                            string tempText = string.Format("{0} has increased by {1}", result.DataPoint, amount);
                                            tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                        }
                                        else
                                        {
                                            int oldValue = Game.director.GetGameState(result.DataPoint, DataState.Bad);
                                            int newValue = Math.Abs(amount) + oldValue;
                                            Game.director.SetGameState(result.DataPoint, DataState.Bad, newValue, true);
                                            string tempText = string.Format("{0} has decreased by {1}", result.DataPoint, amount);
                                            tempList.Add(new Snippet(tempText, RLColor.Red, backColor));
                                        }
                                    }
                                    else Game.SetError(new Error(113, "Invalid DataPoint Amount (zero)"));
                                }
                                break;
                            case ResultType.GameVar:
                                break;
                            case ResultType.Relationship:
                                break;
                            case ResultType.Condition:
                                break;
                            case ResultType.Resource:
                                break;
                            case ResultType.Item:
                                break;
                            case ResultType.Secret:
                                break;
                            case ResultType.Army:
                                break;
                            case ResultType.Event:
                                break;
                            //prevents infinite loop (ignored result)
                            case ResultType.None:
                                break;
                            default:
                                Game.SetError(new Error(113, string.Format("Invalid ResultType (\"{0}\")", type)));
                                break;
                        }
                    }
                    else { Game.SetError(new Error(113, string.Format("Invalid result (null returned, resultID \"{0}\")", resultID))); }
                }
            }
            else { Game.SetError(new Error(113, string.Format("Invalid Input (no results from \"{0}\")", outcome))); }
            return tempList;
        }

        // methods above here
    }
}
