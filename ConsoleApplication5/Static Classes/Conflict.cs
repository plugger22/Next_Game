using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;
using RLNET;

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
        //type of conflict
        public ConflictType Conflict_Type { get; set; }
        public ConflictCombat Combat_Type { get; set; }
        public ConflictSocial Social_Type { get; set; }
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
        //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
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
            arrayPool = new int[3];
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
        /// Master method that handles all conflict set up
        /// </summary>
        public void InitialiseConflict()
        {
            SetPlayerStrategy();
            SetTraits();
            SetSituation(Game.director.GetSituationsNormal(), Game.director.GetSituationsGame());
            SetOpponentStrategy();
            SetOutcome();
            SetCardPool();
            //SetHand(); -> called from Layout.UpdateCards on first run through
            //debug
            //string test = Game.utility.CheckTagsActor("Why is there no joy in the world?", opponent);
            //string sentence = Game.utility.CheckTagsActor("The mud slowed <men> to a crawl. <He> fell off his horse and <he> wasn't happy", opponent);
            
        }

        /// <summary>
        /// Determine Strategy and send to Layout
        /// </summary>
        private void SetPlayerStrategy()
        {
            string[] tempArray = new string[3];
            switch(Conflict_Type)
            {
                case ConflictType.Combat:
                    switch( Combat_Type)
                    {
                        case ConflictCombat.Personal:
                            tempArray[0] = "Go for the Throat";
                            tempArray[1] = "Be Flexible";
                            tempArray[2] = "Focus on Staying Alive";
                            break;
                        case ConflictCombat.Tournament:
                            tempArray[0] = "Knock them to the Ground";
                            tempArray[1] = "Wait for an Opportunity";
                            tempArray[2] = "Stay in the Saddle";
                            break;
                        case ConflictCombat.Battle:
                            tempArray[0] = "Take the Fight to the Enemy";
                            tempArray[1] = "Push but don't Overextend";
                            tempArray[2] = "Hold Firm";
                            break;
                        default:
                            Game.SetError(new Error(86, "Invalid Combat Type"));
                            break;
                    }
                    break;
                case ConflictType.Social:
                    switch (Social_Type)
                    {
                        case ConflictSocial.Befriend:
                            tempArray[0] = "Do what Whatever it Takes";
                            tempArray[1] = "Extend the Hand of Friendship";
                            tempArray[2] = "Approach them with Caution";
                            break;
                        case ConflictSocial.Blackmail:
                            tempArray[0] = "Lean on Them. Hard.";
                            tempArray[1] = "Explain the Facts of Life";
                            tempArray[2] = "Gently Nudge Them";
                            break;
                        case ConflictSocial.Seduce:
                            tempArray[0] = "Actively Flirt and Pursue";
                            tempArray[1] = "Make your Intentions Clear";
                            tempArray[2] = "Infer Wonderful Possibilities";
                            break;
                        default:
                            Game.SetError(new Error(86, "Invalid Social Type"));
                            break;
                    }
                    break;
                default:
                    Game.SetError(new Error(86, "Invalid Conflict Type"));
                    break;
            }
            //update Layout
            if (tempArray.Length == 3)
            { Game.layout.SetStrategy(tempArray); Game.layout.Challenger = Challenger; }
            else
            { Game.SetError(new Error(86, "Invalid Strategy, Layout not updated")); }
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
        /// specify opponent (it's always the player vs. opponent)
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="challenger">true if player is the challenger</param>
        internal void SetOpponent(int actorID, bool challenger)
        {
            if (actorID > 0)
            {
                opponent = Game.world.GetAnyActor(actorID);
                this.Challenger = challenger;
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
                //give correct title for game specific situation, if present
                if (String.IsNullOrEmpty(Game_Title) == false)
                {
                    
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
                        if (Game_Title == "unknown") { Game_Title = backupTitle; }
                        arraySituation[2, 0] = Game_Title;
                        arraySituation[2, 1] = tempListGood[rnd.Next(0, tempListGood.Count)];
                        arraySituation[2, 2] = tempListBad[rnd.Next(0, tempListBad.Count)];
                    }
                    else { Game.SetError(new Error(89, string. Format("listGameSituations has incorrect number of entries (\"{0}\")", listGameSituations.Count))); }
                }
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
        /// returns a neutral situation description appropriate to the conflict
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <returns></returns>
        private string GetSituationNeutral(ConflictType type, int subtype)
        {
            string description = "unknown";

            return description;
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
            string[] tempArray = new string[10];
            //descriptions of outcomes (minor/standard/major wins and losses)
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    //type of conflict
                    tempArray[0] = string.Format("A {0} {1} Challenge", Combat_Type, Conflict_Type);
                    switch (Combat_Type)
                    {
                        case ConflictCombat.Personal:
                            tempArray[1] = "Your Opponent retires with a minor wound and an injured ego";
                            tempArray[2] = "Your Opponent Yields and you can claim an Advantage from him";
                            tempArray[3] = "Your Opponent Suffers a Major Wound and may die";
                            tempArray[4] = "You suffer a minor wound and retire defeated";
                            tempArray[5] = "You are Forced to Yield to a superior Opponent who can demand an Advantage";
                            tempArray[6] = "You have been Badly Injured and Lose any Special Items";
                            break;
                        case ConflictCombat.Tournament:
                            tempArray[1] = "You make the final group but fail to go any further";
                            tempArray[2] = "You reach the top three jousters and gain glory and recognition";
                            tempArray[3] = "You are named Tournament Champion and gain a Ladies Favour";
                            tempArray[4] = "You are unhorsed midway through the tournament";
                            tempArray[5] = "You are unhorsed early on by a mid ranked jouster";
                            tempArray[6] = "You fall off your horse and break bones on your first joust. Disgrace!";
                            break;
                        case ConflictCombat.Battle:
                            tempArray[1] = "The enemy pulls back hurt but isn't defeated";
                            tempArray[2] = "You carry the day and the enemy retreat";
                            tempArray[3] = "The enemy rout and suffer horrendous casualties";
                            tempArray[4] = "You are forced to withdraw but hold your army together";
                            tempArray[5] = "You army suffers substantial casualties and is defeated";
                            tempArray[6] = "Your army breaks. You flee the field in order to save yourself";
                            break;
                        default:
                            Game.SetError(new Error(86, "Invalid Combat Type"));
                            break;
                    }
                    break;
                case ConflictType.Social:
                    //type of conflict
                    tempArray[0] = string.Format("A {0} {1} Challenge", Social_Type, Conflict_Type);
                    switch (Social_Type)
                    {
                        case ConflictSocial.Befriend:
                            tempArray[1] = "Your relationship improves";
                            tempArray[2] = "You have become firm friends";
                            tempArray[3] = "You have gained an ardent supporter";
                            tempArray[4] = "Your relationship deteriorates";
                            tempArray[5] = "You have become disliked";
                            tempArray[6] = "Your opponent is actively campaigning against you";
                            break;
                        case ConflictSocial.Blackmail:
                            tempArray[1] = "You have gained a small amount of influence";
                            tempArray[2] = "Your opponent agrees to your demands";
                            tempArray[3] = "Your opponent has become your minion";
                            tempArray[4] = "You relationship has deteroriated";
                            tempArray[5] = "You have been firmly rebuffed";
                            tempArray[6] = "Your opponent is now your enemy";
                            break;
                        case ConflictSocial.Seduce:
                            tempArray[1] = "Your relationship has improved";
                            tempArray[2] = "You seduce your opponent and gain an advantage";
                            tempArray[3] = "Your opponent has become an ardent supporter and lover";
                            tempArray[4] = "Your relationship has deteriorated";
                            tempArray[5] = "You have been firmly rebuffed";
                            tempArray[6] = "Your opponent has taken offence and is now your enemy";
                            break;
                        default:
                            Game.SetError(new Error(86, "Invalid Social Type"));
                            break;
                    }
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
            if (tempArray.Length == 10)
            { Game.layout.SetOutcome(tempArray); }
            else
            { Game.SetError(new Error(89, "Invalid Situation, Layout not updated")); }
        }

        /// <summary>
        /// Set up Primary and Secondary Traits in use
        /// </summary>
        private void SetTraits()
        {
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    switch (Combat_Type)
                    {
                        case ConflictCombat.Personal:
                            PrimarySkill = SkillType.Combat;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Treachery;
                            break;
                        case ConflictCombat.Tournament:
                            PrimarySkill = SkillType.Combat;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Treachery;
                            break;
                        case ConflictCombat.Battle:
                            PrimarySkill = SkillType.Leadership;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Treachery;
                            break;
                        default:
                            Game.SetError(new Error(91, "Invalid Combat Type"));
                            break;
                    }
                    break;
                case ConflictType.Social:
                    switch (Social_Type)
                    {
                        case ConflictSocial.Befriend:
                            PrimarySkill = SkillType.Charm;
                            OtherSkill_1 = SkillType.Treachery;
                            OtherSkill_2 = SkillType.Wits;
                            break;
                        case ConflictSocial.Blackmail:
                            PrimarySkill = SkillType.Treachery;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Charm;
                            break;
                        case ConflictSocial.Seduce:
                            PrimarySkill = SkillType.Charm;
                            OtherSkill_1 = SkillType.Treachery;
                            OtherSkill_2 = SkillType.Wits;
                            break;
                        default:
                            Game.SetError(new Error(91, "Invalid Social Type"));
                            break;
                    }
                    break;
                default:
                    Game.SetError(new Error(91, "Invalid Conflict Type"));
                    break;
            }
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
            int numCards;
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
            //...Player Primary skill
            type = CardType.Good; foreColor = RLColor.Black; arrayPool[0] += cards_player;
            for (int i = 0; i < cards_player; i++)
            { listCardPool.Add(new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", player.Name, PrimarySkill), description)); }
            text = string.Format("{0}'s {1} Skill ({2}), {3} cards, Primary Challenge skill ({4} stars) ", player.Name, PrimarySkill, type, cards_player, skill_player);
            listPlayerCards.Add(new Snippet(text, foreColor, backColor));
            //...Opponent Primary skill
            type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += cards_opponent;
            for (int i = 0; i < cards_opponent; i++)
            { listCardPool.Add(new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", opponent.Name, PrimarySkill), description)); }
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
            listSpecialCards.Clear();
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
            else
            { Game.SetError(new Error(97, "Invalid Title Input (null)")); Game_Title = "unknown"; }
            
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
            Console.WriteLine(Environment.NewLine + "Situation: \"{0}\", difference {1} Modifier {2} numCards {3}", description, difference, modifier, numCards);
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
        /// Input special, decision derived, situations. 
        /// </summary>
        /// <param name="specialType"></param>
        /// <param name="cardType"></param>
        /// <param name="numCards">leave at default '0' if you want a random #</param>
        public void SetSpecialSituation(ConflictSpecial specialType, CardType cardType, int numCards = 0)
        {
            //get dictionary of specials
            Dictionary<int, Situation> specialDictionary = Game.director.GetSituationsSpecial();
            if (specialDictionary.Count > 1)
            {
                if (specialType > ConflictSpecial.None)
                {
                    if (cardType > CardType.None)
                    {
                        int defender = 0;
                        if (cardType == CardType.Good) { defender = 1; } else if (cardType == CardType.Bad) { defender = -1; }
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
                        if (listFilteredSituations.Count > 0)
                        {
                            Situation situation = listFilteredSituations[rnd.Next(0, listFilteredSituations.Count)];
                            List<string> tempListGood = situation.GetGood();
                            List<string> tempListBad = situation.GetBad();
                            int number = numCards;
                            if (numCards == 0) { numCards = GetSituationCardNumber(); }
                            //add cards to special card list
                            for (int i = 0; i < number; i++)
                            {
                                Card_Conflict card = new Card_Conflict(CardConflict.Situation, cardType, situation.Name, "special situation");
                                card.PlayedText = tempListGood[rnd.Next(0, tempListGood.Count)];
                                card.IgnoredText = tempListBad[rnd.Next(0, tempListBad.Count)];
                                card.TypeDefend = CardType.Good;
                                listCardSpecials.Add(card);
                            }
                        }
                    }
                }
            }
        }

        // methods above here
    }
}
