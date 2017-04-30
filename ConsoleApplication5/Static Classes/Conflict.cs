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
    

    /// <summary>
    /// Handles the card Conflict system. Single class as there is only ever a single instance in existence.
    /// </summary>
    public class Conflict
    {
        static Random rnd;
        //protagonists
        Actor opponent;
        Player player;
        public bool Challenger { get; set; } //is the Player the Challenger?
        int numberOfCards;
        private Challenge challenge;
        //type of conflict
        public ConflictType Conflict_Type { get; set; }
        public ConflictCombat Combat_Type { get; set; }
        public ConflictSocial Social_Type { get; set; }
        public ConflictStealth Stealth_Type { get; set; }
        public ConflictSubType Sub_Type { get; set; }
        //Game specific Situation
        public ConflictState Game_State { get; set; }
        public CardType Game_Type { get; set; }
        public string Game_Title { get; set; } //used for card title
        public string Game_Description { get; set; } //used for card breakdown
        public int NumSupporters { get; } //max # of supporters for each participant
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
        private string[] arraySupportGood;
        private string[] arraySupportBad;
        private string[,] arrayItems;
        //three lists to consolidate into pool breakdown description
        private List<Snippet> listPlayerCards;
        private List<Snippet> listOpponentCards;
        private List<Snippet> listSituationCards;
        private List<Snippet> listSupporterCards;
        private List<Snippet> listItemCards;

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
            arrayPool = new int[4]; //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards, 3 - # defender specific cards)
            arrayModifiers = new int[3]; //modifier (DM) for GetSituationCardNumber, 0/1/2 refer to the three situation cards (def adv/neutral/game specific) -> DM for 0 & 1, # of cards for Game ('2')
            arraySituation = new string[3, 3];
            NumSupporters = Game.constant.GetValue(Global.NUM_SUPPORTERS);
            arraySupportGood = new string[NumSupporters]; //max 'NumSupporters' supporters for Player ('0' -> name supporter)
            arraySupportBad = new string[NumSupporters]; //max 'NumSupporters' supporters for Opponent ('0' -> name supporter)
            arrayItems = new string[2, 5]; // Items ([0,] -> Player, [1,] -> Opponent) [,0] -> Description [,1] -> CardText [,2] -> CardNum [,3] -> Good outcome txt [,4] -> Bad outcome txt
            //lists to consolidate into pool breakdown description
            listPlayerCards = new List<Snippet>();
            listOpponentCards = new List<Snippet>();
            listSituationCards = new List<Snippet>();
            listSupporterCards = new List<Snippet>();
            listItemCards = new List<Snippet>();
            //get player (always the player vs. somebody else)
            player = (Player)Game.world.GetActiveActor(1);
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
            Game.logTurn?.Write(string.Format("- Initialise {0} Challenge", Conflict_Type));
            if (GetChallenge() == true) //get relevant challenge data
            {
                Game.layout.InitialiseData();
                SetPlayerStrategy();
                SetSkills();
                SetSituation(Game.director.GetSituationsNormal(), Game.director.GetSituationsGame());
                CheckSpecialSituations();
                SetSupporters();
                SetItems();
                SetCardPool();
                SetOpponentStrategy();
                SetRecommendedStrategy();
                SetOutcome();
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
        /// Gets any supporters for both participants of the conflict
        /// </summary>
        private void SetSupporters()
        {
            Game.logTurn?.Write("--- SetSupporters (Conflict.cs)");
            int relThresholdPlyr = Game.constant.GetValue(Global.FRIEND_THRESHOLD); //relationship level (>=) with Player needed to be a supporter of Player
            int relThresholdOpp = Game.constant.GetValue(Global.ENEMY_THRESHOLD); //relationship level (<=) with Player needed to be a supporter of Opponent
            Dictionary<int, int> dictPlyrSupporters = new Dictionary<int, int>();
            Dictionary<int, int> dictOppSupporters = new Dictionary<int, int>();
            string descriptor;
            //get list of all actors at location -> No supporters if travelling (on the road)
            if (player.Status == ActorStatus.AtLocation || player.Status == ActorStatus.Captured)
            {
                Location loc = Game.network.GetLocation(player.LocID);
                if (loc != null)
                {
                    List<int> listActors = loc.GetActorList();
                    int actorID;
                    for(int i = 0; i < listActors.Count; i++)
                    {
                        actorID = listActors[i];
                        if (actorID > 0)
                        {
                            Actor actor = Game.world.GetAnyActor(actorID);
                            int relPlyr;
                            if (actor != null)
                            {
                                //exclude player and opponent from relationship checks
                                if (actor.ActID != player.ActID && actor.ActID != opponent.ActID)
                                {
                                    //relationship with Player
                                    relPlyr = actor.GetRelPlyr();
                                    if (relPlyr >= relThresholdPlyr)
                                    {
                                        //relationship high enough to be a supporter, add to dictionary
                                        try
                                        { dictPlyrSupporters.Add(actorID, relPlyr); }
                                        catch (ArgumentException)
                                        { Game.SetError(new Error(193, string.Format("Invalid actorID (\"{0}\") -> Duplicate Key (dictPlyrSupporters), with relPlyr {1}", actorID, relPlyr))); }
                                    }
                                    //relationship with Opponent
                                    if (relPlyr <= relThresholdOpp)
                                    {
                                        //relationship low enough to be an enemy (supporter of Opponent) -> add to dict
                                        try
                                        { dictOppSupporters.Add(actorID, relPlyr); }
                                        catch (ArgumentException)
                                        { Game.SetError(new Error(193, string.Format("Invalid actorID (\"{0}\") -> Duplicate Key (dictOppSupporters), with relPlyr {1}", actorID, relPlyr))); }
                                    }
                                }
                            }
                            else { Game.SetError(new Error(193, string.Format("Invalid actor (null) from actorID \"{0}\"", actorID))); }
                        }
                        else { Game.SetError(new Error(193, "Invalid actorID (zero or less)")); }
                    }
                    //Sort (highest to lowest) dictionaries -> Player
                    Game.logTurn?.Write(string.Format(" [Notification] dictPlyrSupporters has {0} records", dictPlyrSupporters.Count));
                    if (dictPlyrSupporters.Count > 0)
                    {
                        var sortedDict = from entry in dictPlyrSupporters orderby entry.Value descending select entry;
                        //use top 'NumSupporters' entries only (highest relationships)
                        int index = 0;
                        foreach(var relationship in sortedDict)
                        {
                            Actor actor = Game.world.GetAnyActor(relationship.Key);
                            if (actor != null)
                            {
                                descriptor = string.Format("{0} {1} \"{2}\", Friend", actor.Title, actor.Name, actor.Handle);
                                arraySupportGood[index] = descriptor;
                                index++;
                                Game.logTurn?.Write(string.Format(string.Format(" [Supporter -> Good] {0}, ActID {1}, relPlyr {2}", descriptor, actor.ActID, actor.GetRelPlyr())));
                                if (index >= NumSupporters) { break; }
                            }
                            else { Game.SetError(new Error(193, "Invalid actor (null) in sortedDict -> dictPlyrSupporters")); }
                        }
                    }
                    //Sort (lowest to highest) dictionaries -> Opponent
                    Game.logTurn?.Write(string.Format(" [Notification] dictOppSupporters has {0} records", dictOppSupporters.Count));
                    if (dictOppSupporters.Count > 0)
                    {
                        var sortedDict = from entry in dictOppSupporters orderby entry.Value ascending select entry;
                        //use top 'NumSupporters' entries only (highest relationships)
                        int index = 0;
                        foreach (var relationship in sortedDict)
                        {
                            Actor actor = Game.world.GetAnyActor(relationship.Key);
                            if (actor != null)
                            {
                                descriptor = string.Format("{0} {1} \"{2}\", Enemy", actor.Title, actor.Name, actor.Handle);
                                arraySupportBad[index] = descriptor;
                                index++;
                                Game.logTurn?.Write(string.Format(string.Format(" [Supporter -> Bad] {0}, ActID {1}, relPlyr {2}", descriptor, actor.ActID, actor.GetRelPlyr())));
                                if (index >= NumSupporters) { break; }
                            }
                            else { Game.SetError(new Error(193, "Invalid actor (null) in sortedDict -> dictOppSupporters")); }
                        }
                    }
                }
                else { Game.SetError(new Error(193, "Invalid Player.LocID (null Location)")); }
            }
        }

        /// <summary>
        /// Checks both participants for items useable in a challenge
        /// </summary>
        private void SetItems()
        {
            Game.logTurn?.Write("--- SetItems (Conflict.cs)");
            //player
            List<int> tempList = new List<int>();
            tempList.AddRange(player.GetItems());
            if (tempList.Count > 0)
            {
                //loop list looking for active items that can be used in a challenge. Take the first found (should only be one anyway)
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] > 0)
                    {
                        Possession possession = Game.world.GetPossession(tempList[i]);
                        if (possession != null)
                        {
                            if (possession.Active == true && possession is Item)
                            {
                                Item item = possession as Item;
                                if (item.ChallengeFlag == true)
                                {
                                    //item is valid for this type of Conflict?
                                    if (item.CheckChallengeType(Sub_Type) == true)
                                    {
                                        arrayItems[0, 0] = item.Description;
                                        arrayItems[0, 1] = item.CardText;
                                        arrayItems[0, 2] = item.CardNum.ToString();
                                        if (Challenger == true)
                                        {
                                            //Player is the Challenger
                                            string[] tempTexts = item.GetOutcomeTexts();
                                            if (tempTexts.Length == 4)
                                            {
                                                //Good Text
                                                arrayItems[0, 3] = tempTexts[0];
                                                //Bad Text
                                                arrayItems[0, 4] = tempTexts[1];
                                            }
                                            else { arrayItems[0, 3] = "Unknown"; arrayItems[0, 4] = "Unknown"; }
                                        }
                                        else
                                        {
                                            //Player is the Defender
                                            string[] tempTexts = item.GetOutcomeTexts();
                                            if (tempTexts.Length == 4)
                                            {
                                                //Good Text
                                                arrayItems[0, 3] = tempTexts[2];
                                                //Bad Text
                                                arrayItems[0, 4] = tempTexts[3];
                                            }
                                            else { arrayItems[0, 3] = "Unknown"; arrayItems[0, 4] = "Unknown"; }
                                        }
                                    }
                                    else { Game.logTurn?.Write(string.Format(" [Notification] Player Item \"{0}\" isn't valid for this Challenge", item.Description)); }
                                }
                                else { Game.logTurn?.Write(string.Format(" [Notification] Player Item \"{0}\" has no challenge effect", item.Description)); }
                            }
                        }
                        else { Game.SetError(new Error(203, "Invalid Player Item Possession (null)")); }
                    }
                    else { Game.SetError(new Error(203, string.Format("Invalid Player Item PossID (\"{0}\")", tempList[i]))); }
                }
            }
            else { Game.logTurn?.Write(" [Notification] Player has no Items"); }
            //opponent
            tempList.Clear();
            tempList.AddRange(opponent.GetItems());
            if (tempList.Count > 0)
            {
                //loop list looking for active items that can be used in a challenge. Take the first found (should only be one anyway)
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] > 0)
                    {
                        Possession possession = Game.world.GetPossession(tempList[i]);
                        if (possession != null)
                        {
                            if (possession.Active == true && possession is Item)
                            {
                                Item item = possession as Item;
                                if (item.ChallengeFlag == true)
                                {
                                    //item is valid for this type of Conflict?
                                    if (item.CheckChallengeType(Sub_Type) == true)
                                    {
                                        //Opponent is the Challenger
                                        arrayItems[1, 0] = item.Description;
                                        arrayItems[1, 1] = item.CardText;
                                        arrayItems[1, 2] = item.CardNum.ToString();
                                        if (Challenger == true)
                                        {
                                            string[] tempTexts = item.GetOutcomeTexts();
                                            if (tempTexts.Length == 4)
                                            {
                                                //Good Text
                                                arrayItems[1, 3] = tempTexts[0];
                                                //Bad Text
                                                arrayItems[1, 4] = tempTexts[1];
                                            }
                                            else { arrayItems[1, 3] = "Unknown"; arrayItems[1, 4] = "Unknown"; }
                                        }
                                        else
                                        {
                                            //Opponent is the Defender
                                            string[] tempTexts = item.GetOutcomeTexts();
                                            if (tempTexts.Length == 4)
                                            {
                                                //Good Text
                                                arrayItems[1, 3] = tempTexts[2];
                                                //Bad Text
                                                arrayItems[1, 4] = tempTexts[3];
                                            }
                                            else { arrayItems[1, 3] = "Unknown"; arrayItems[1, 4] = "Unknown"; }
                                        }
                                    }
                                    else { Game.logTurn?.Write(string.Format(" [Notification] Opponent Item \"{0}\" isn't valid for this Challenge", item.Description)); }
                                }
                                else { Game.logTurn?.Write(string.Format(" [Notification] Opponent Item \"{0}\" has no challenge effect", item.Description)); }
                            }
                        }
                        else { Game.SetError(new Error(203, "Invalid Opponent Item Possession (null)")); }
                    }
                    else { Game.SetError(new Error(203, string.Format("Invalid Opponent Item PossID (\"{0}\")", tempList[i]))); }
                }
            }
            else { Game.logTurn?.Write(" [Notification] Opponent has no Items"); }
        }


        /// <summary>
        /// Gets relevant challenge from dictionary. Returns true if valid challenge found.
        /// </summary>
        private bool GetChallenge()
        {
            bool proceed = true;
            Sub_Type = ConflictSubType.None;
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    switch(Combat_Type)
                    {
                        case ConflictCombat.Personal:
                            Sub_Type = ConflictSubType.Personal;
                            break;
                        case ConflictCombat.Tournament:
                            Sub_Type= ConflictSubType.Tournament;
                            break;
                        case ConflictCombat.Battle:
                            Sub_Type= ConflictSubType.Battle;
                            break;
                        case ConflictCombat.Hunting:
                            Sub_Type= ConflictSubType.Hunting;
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
                            Sub_Type= ConflictSubType.Befriend;
                            break;
                        case ConflictSocial.Blackmail:
                            Sub_Type= ConflictSubType.Blackmail;
                            break;
                        case ConflictSocial.Seduce:
                            Sub_Type= ConflictSubType.Seduce;
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
                            Sub_Type= ConflictSubType.Infiltrate;
                            break;
                        case ConflictStealth.Evade:
                            Sub_Type= ConflictSubType.Evade;
                            break;
                        case ConflictStealth.Escape:
                            Sub_Type = ConflictSubType.Escape;
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
                challenge = Game.director.GetChallenge(Sub_Type);
                if (challenge == null)
                { Game.SetError(new Error(111, "Challenge not found (returns null)")); return false; }
                else
                {
                    //Is there a Special challenge in the dictionary (used to overide standard challenge data)
                    if (Game.director.CheckChallenge(ConflictSubType.Special) == true)
                    {
                        Challenge overide = Game.director.GetChallenge(ConflictSubType.Special);
                        //where data exists in overideChallenge it overwrites existing data -> Strategy
                        string[] copyArray = overide.GetStrategies();
                        string[] originalArray = challenge.GetStrategies();
                        for(int i = 0; i < copyArray.Length; i++)
                        {
                            if (copyArray[i]?.Length > 0)
                            { originalArray[i] = copyArray[i]; }
                        }
                        //outcomes
                        copyArray = overide.GetOutcomes();
                        originalArray = challenge.GetOutcomes();
                        for(int i = 0; i < copyArray.Length; i++)
                        {
                            if (copyArray[i]?.Length > 0)
                            { originalArray[i] = copyArray[i]; }
                        }
                        //skills
                        SkillType[] copySkillArray = overide.GetSkills();
                        SkillType[] originalSkillArray = challenge.GetSkills();
                        for (int i = 0; i < copySkillArray.Length; i++)
                        {
                            if (copySkillArray[i] > SkillType.None)
                            { originalSkillArray[i] = copySkillArray[i]; }
                        }
                        //results
                        for (int i = 0; i < (int)ConflictResult.Count; i++)
                        {
                            List<int> subList = overide.GetResults((ConflictResult)i);
                            if (subList.Count > 0)
                            { challenge.SetResults((ConflictResult)i, subList); }
                        }
                    }
                    return true;
                }
            }
            else
            { Game.SetError(new Error(111, "Challenge not found, invalid Conflict_Type or SubType")); return false; }
        }


        /// <summary>
        /// Determine opponent's strategy and send to layout (attack/balanced/defend)
        /// </summary>
        private void SetOpponentStrategy()
        {
            //NOTE: Doesn't take into account secrets
            Game.logTurn?.Write("--- SetOpponentStrategy (Conflict.cs)");
            int good = arrayPool[0];
            int bad = arrayPool[2];
            int defenderSpecific = arrayPool[3]; //defender advantage cards (only apply if defender chooses a defensive strategy)
            int margin = Game.constant.GetValue(Global.OPPONENT_MARGIN); //threshold needed to move from a balanced strategy
            int opponentWits = opponent.GetSkill(SkillType.Wits);
            int opponentPrimarySkill = opponent.GetSkill(PrimarySkill);
            //higher their wits (ability to judge a situation), the less the random affect and more likely to make a straight logical decision (Wits 1 star -> 0 - 5, Wits 5 stars -> 0 - 1)
            int rndFactor = rnd.Next(0, 7 - opponentWits); 
            //Factor is applied depending on Primary skill -> if < 3 then factor works against an aggressive strategy, if > 3 then towards & if factor = 0 then no random factor 
            if (opponentPrimarySkill < 3) { rndFactor *= -1; } 
            else if (opponentPrimarySkill == 3) { rndFactor = 0; }
            Game.logTurn?.Write(string.Format("[Conflict -> Debug] Good -> {0} Bad -> {1} Margin -> {2} rndFactor -> {3} Opp Wits -> {4} Opp PSkill -> {5} Def Spec -> {6} Challenger -> {7}", good, bad, margin,
                        rndFactor, opponentWits, opponentPrimarySkill, defenderSpecific, Challenger));
            if (Challenger == true)
            {
                //opponent is the defender 
                if (good + rndFactor - bad - defenderSpecific > margin)
                {
                    //aggressive
                    Game.layout.Strategy_Opponent = 0;
                    Game.logTurn?.Write(string.Format("[Conflict -> Opp Def's] Good {0} + rndFactor {1} - bad {2} - defSpec {3} > margin {4}", good, rndFactor, bad, defenderSpecific, margin));
                }
                else if (bad - good > margin)
                {
                    //defensive
                    Game.layout.Strategy_Opponent = 2;
                    Game.logTurn?.Write(string.Format("[Conflict -> Opp Def's] Bad {0} - Good {1} > margin {2}", bad, good, margin));
                }
                else
                {
                    //balanced
                    Game.layout.Strategy_Opponent = 1;
                } 
            }
            else
            {
                //opponent is attacker 
                //if (good + rndFactor - bad > margin)
                if (bad + rndFactor - good > margin)
                {
                    //aggressive
                    Game.layout.Strategy_Opponent = 0;
                    Game.logTurn?.Write(string.Format("[Conflict -> Plyr Def's] Bad {0} + rndFactor {1} - good {2} > margin {3}", bad, rndFactor, good, margin));
                }
                //else if (bad - good > margin)
                else if (bad - good <= 0)
                {
                    //defensive
                    Game.layout.Strategy_Opponent = 2;
                    Game.logTurn?.Write(string.Format("[Conflict -> Plyr Def's] Bad {0} - Good {1} <= 0", bad, good, margin));
                }
                else
                {
                    //balanced
                    Game.layout.Strategy_Opponent = 1;
                }
            }
        }

        /// <summary>
        /// Determines a recommended strategy based on straight card pool stats and if player is a defender, or not
        /// </summary>
        private void SetRecommendedStrategy()
        {
            int good = arrayPool[0];
            int bad = arrayPool[2];
            int defenderSpecific = arrayPool[3]; //defender advantage cards (only apply if defender chooses a defensive strategy)
            int margin = Game.constant.GetValue(Global.OPPONENT_MARGIN); //threshold needed to move from a balanced strategy
            if (Challenger == false)
            {
                //Player is the defender 
                if (good - bad - defenderSpecific > margin) { Game.layout.Strategy_Recommended = 0; } //aggressive
                else if (bad - good > margin) { Game.layout.Strategy_Recommended = 2; } //defensive
                else { Game.layout.Strategy_Recommended = 1; } //balanced
            }
            else
            {
                //Player is attacker 
                if (good - bad > margin) { Game.layout.Strategy_Recommended = 0; } //aggressive
                else if (bad - good > margin) { Game.layout.Strategy_Recommended = 2; } //defensive
                else { Game.layout.Strategy_Recommended = 1; } //balanced
            }
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
                //get Player's status (convert Travelling to 1, AtLocation to 2) -> used to filter situations if required (by Data)
                int status = 0;
                Active player = Game.world.GetActiveActor(1);
                if (player != null)
                {
                    if (player.Status == ActorStatus.Travelling) { status = 1; }
                    else if (player.Status == ActorStatus.AtLocation) { status = 2; }
                }
                else { Game.SetError(new Error(86, "Invalid Player (null)")); }
                switch (Conflict_Type)
                {
                    case ConflictType.Combat:
                        //Get all Combat situations of the correct subType from dictionary
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
                                where situation.Data == 0 || situation.Data == status
                                where situation.Defender == whoDefends
                                select situation;
                            listFirstSituations = firstSituationSet.ToList();
                            if (listFirstSituations.Count == 0)
                            { Game.SetError(new Error(89, "listFirstSituations (Combat) has no data")); invalidData = true; }

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
                                where situation.Data == 0 || situation.Data == status
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
                        //Get all Social situations of the correct subType from dictionary
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
                                where situation.Data == 0 || situation.Data == status
                                select situation;
                            listFirstSituations = firstSituationSet.ToList();
                            if (listFirstSituations.Count == 0)
                            { Game.SetError(new Error(89, "listFirstSituations (Social) has no data")); invalidData = true; }

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
                                where situation.Data == 0 || situation.Data == status
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
                    case ConflictType.Stealth:
                        //Get all Stealth situations of the correct subType from dictionary
                        List<Situation> listFilteredStealthSituations = new List<Situation>();
                        IEnumerable<Situation> situationStealthSet =
                            from situation in normalDictionary
                            where situation.Value.Type == ConflictType.Stealth
                            where situation.Value.Type_Stealth == Stealth_Type
                            //orderby situation.Value.SitID
                            select situation.Value;
                        listFilteredStealthSituations = situationStealthSet.ToList();
                        if (listFilteredStealthSituations.Count == 0) { Game.SetError(new Error(89, "listFilteredStealthSituations has no data")); }
                        else
                        {
                            //filter for Defensive Advantage card (1st situation)
                            List<Situation> listFirstSituations = new List<Situation>();
                            IEnumerable<Situation> firstSituationSet =
                                from situation in listFilteredStealthSituations
                                where situation.SitNum == 0
                                where situation.Defender == whoDefends
                                where situation.Data == 0 || situation.Data == status
                                select situation;
                            listFirstSituations = firstSituationSet.ToList();
                            if (listFirstSituations.Count == 0)
                            { Game.SetError(new Error(89, "listFirstSituations (Stealth) has no data")); invalidData = true; }

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
                                from situation in listFilteredStealthSituations
                                where situation.SitNum == 1
                                where situation.Data == 0 || situation.Data == status
                                select situation;
                            listSecondSituations = secondSituationSet.ToList();
                            if (listSecondSituations.Count == 0)
                            {
                                //set default empty data if nothing found
                                Game.SetError(new Error(89, "listSecondSituations (Stealth) has no data"));
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
        /// returns a situation containing lists of good/bad immersion texts for the Touched skill involved in the challenge.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        /// <param name="challenger">true if the Player is the challenger</param>
        /// <param name="playerSkill">true if referring to the Player's skill, false if it's your Opponents skill</param>
        /// <returns>null if no valid data</returns>
        private Situation GetTouchedText(ConflictType type, int subType, bool challenger, bool playerSkill)
        {
            if (type > ConflictType.None)
            {
                if (subType > 0)
                {
                    Situation situation = null;
                    Dictionary<int, Situation> tempDictionary = Game.director.GetSituationsTouched();
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
                                { Game.SetError(new Error(196, "Incorrect count for Combat Situations (listCombatSituations)")); }
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
                                { Game.SetError(new Error(196, "Incorrect count for Social Situations (listSocialSituations)")); }
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
                                { Game.SetError(new Error(196, "Incorrect count for Other Situations (listOtherSituations)")); }
                                break;
                            default:
                                Game.SetError(new Error(196, string.Format("Invalid type (\"{0}\")", type)));
                                break;
                        }
                    }
                    else
                    { Game.SetError(new Error(196, "Invalid Dictionary input (null or empty)")); }
                    return situation;
                }
                else
                { Game.SetError(new Error(196, "Invalid subType input (Zero or less)")); }
            }
            else
            { Game.SetError(new Error(196, "Invalid ConflictType input (\"None\")")); }
            return null;
        }

        /// <summary>
        /// returns a situation containing lists of good/bad immersion texts for the Supporters involved in the challenge.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        /// <param name="challenger">true if the Player is the challenger</param>
        /// <returns>null if no valid data</returns>
        private Situation GetSupporterText(ConflictType type, int subType, bool challenger)
        {
            if (type > ConflictType.None)
            {
                if (subType > 0)
                {
                    Situation situation = null;
                    Dictionary<int, Situation> tempDictionary = Game.director.GetSituationsSupporter();
                    //get dictionary of skill situations
                    if (tempDictionary != null && tempDictionary.Count > 0)
                    {
                        //work out correct Player defending status
                        int plyrDef = 1;
                        if (challenger == true) { plyrDef = -1; }
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
                                    select sitTemp.Value;
                                listCombatSituations = situationCombatSet.ToList();
                                //should be a single situation
                                if (listCombatSituations.Count == 1)
                                { situation = listCombatSituations[0]; }
                                else
                                { Game.SetError(new Error(194, "Incorrect count for Combat Situations")); }
                                break;
                            case ConflictType.Social:
                                //Get suitable situations from dictionary
                                List<Situation> listSocialSituations = new List<Situation>();
                                IEnumerable<Situation> situationSocialSet =
                                    from sitTemp in tempDictionary
                                    where sitTemp.Value.Type == ConflictType.Social
                                    where (int)sitTemp.Value.Type_Social == subType
                                    where sitTemp.Value.Defender == plyrDef
                                    select sitTemp.Value;
                                listSocialSituations = situationSocialSet.ToList();
                                //should be a single situation
                                if (listSocialSituations.Count == 1)
                                { situation = listSocialSituations[0]; }
                                else
                                { Game.SetError(new Error(194, "Incorrect count for Social Situations")); }
                                break;
                            case ConflictType.Stealth:
                                //Get suitable situations from dictionary
                                List<Situation> listOtherSituations = new List<Situation>();
                                IEnumerable<Situation> situationOtherSet =
                                    from sitTemp in tempDictionary
                                    where sitTemp.Value.Type == ConflictType.Stealth
                                    where (int)sitTemp.Value.Type_Stealth == subType
                                    where sitTemp.Value.Defender == plyrDef
                                    select sitTemp.Value;
                                listOtherSituations = situationOtherSet.ToList();
                                //should be a single situation
                                if (listOtherSituations.Count == 1)
                                { situation = listOtherSituations[0]; }
                                else
                                { Game.SetError(new Error(194, "Incorrect count for Stealth Situations")); }
                                break;
                            default:
                                Game.SetError(new Error(194, string.Format("Invalid type (\"{0}\")", type)));
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
            Game.logTurn?.Write("--- GetSituationCardNumber (Conflict.cs)");
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
            Game.logTurn?.Write(string.Format(" modifier {0}, rndNum {1}, net {2}, numCards {3}", modifier, rndDebug, rndNum, numCards));
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
                    tempArray[0] = string.Format("An {0} {1} Challenge", Stealth_Type, Conflict_Type);
                    break;
                default:
                    Game.SetError(new Error(86, "Invalid Conflict Type"));
                    break;
            }
            //Who has the advantage and a recommendation
            string strategyText = "A Balanced (\"F2\")";
            int strategyNum = Game.layout.Strategy_Recommended;
            if (strategyNum == 0) { strategyText = "An Aggressive (\"F1\")"; }
            else if (strategyNum == 2) { strategyText = "A Defensive (\"F3\")"; }
            if (Challenger == true)
            { tempArray[7] = "You are the Challenger"; }
            else { tempArray[7] = "You are Defending"; }
            tempArray[8] = string.Format("{0} Strategy is Recommended", strategyText);
            //protagonists
            string title;
            if (opponent.Office == ActorOffice.None) { /*title = Convert.ToString(opponent.Type);*/ title = opponent.Title; }
            else { title = Convert.ToString(opponent.Office); }
            /*string handle_player, handle_opponent;
            if (player.Handle != null) { handle_player = string.Format(" \"{0}\" ", player.Handle); } else { handle_player = null; }
            if (opponent.Handle != null) { handle_opponent = string.Format(" \"{0}\" ", opponent.Handle); } else { handle_opponent = null; }*/
            //order protagnoists so that challenger is first and defender is second
            if (Challenger == true)
            { tempArray[9] = string.Format("{0} {1} \"{2}\" vs. {3} {4} \"{5}\"", player.Type, player.Name, player.Handle, title, opponent.Name, opponent.Handle); }
            else
            { tempArray[9] = string.Format("{0} {1} \"{2}\" vs. {3} {4} \"{5}\"", title, opponent.Name, opponent.Handle, player.Type, player.Name, player.Handle); }
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
        private void 
            SetCardPool()
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
            listSupporterCards.Clear();
            listItemCards.Clear();
            //headers
            listPlayerCards.Add(new Snippet("Your Cards", RLColor.Blue, backColor));
            listOpponentCards.Add(new Snippet("Opponent's Cards", RLColor.Blue, backColor));
            listSituationCards.Add(new Snippet("Situation Cards", RLColor.Blue, backColor));
            if (arraySupportGood[0]?.Length > 0 || arraySupportBad[0]?.Length > 0)
            { listSupporterCards.Add(new Snippet("Supporter Cards", RLColor.Blue, backColor)); }
            if (arrayItems[0, 0]?.Length > 0 || arrayItems[1, 0]?.Length > 0)
            { listItemCards.Add(new Snippet("Item Cards", RLColor.Blue, backColor)); }
            //primary skills (2 cards per skill level)
            int skill_player = player.GetSkill(PrimarySkill);
            int skill_opponent = opponent.GetSkill(PrimarySkill);
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

            //Touched Skill -> 1 card per level (ignore traits) -> applies to all conflict types
            int touchedPlyrCards = player.GetSkill(SkillType.Touched);
            int touchedOppCards = opponent.GetSkill(SkillType.Touched);
            //Player is touched?
            if (touchedPlyrCards > 0)
            {
                type = CardType.Good; foreColor = RLColor.Black; arrayPool[0] += touchedPlyrCards;
                //get relevant touched situation (for lists of immersion texts)
                Situation situationTouchedPlyr = GetTouchedText(Conflict_Type, subType, Challenger, true);
                if (situationTouchedPlyr != null)
                { tempListGood = situationTouchedPlyr.GetGood(); tempListBad = situationTouchedPlyr.GetBad(); }
                else { Game.SetError(new Error(105, string.Format("situation (Player Touched Skill) came back Null", Conflict_Type))); }
                goodIndex = rnd.Next(tempListGood.Count);
                badIndex = rnd.Next(tempListBad.Count);
                for (int i = 0; i < touchedPlyrCards; i++)
                {
                    Card_Conflict cardPlayer = new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", opponent.Name, SkillType.Touched), description);
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
                text = string.Format("{0}'s Touched Skill, {1} card{2}, applies to all challenges ({3} stars) ", player.Name, touchedPlyrCards, touchedPlyrCards != 1 ? "s" : "",
                    touchedPlyrCards);
                listPlayerCards.Add(new Snippet(text, foreColor, backColor));
            }
            //Opponent is touched?
            if (touchedOppCards > 0)
            {
                type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += touchedOppCards;
                //get relevant touched situation (for lists of immersion texts)
                Situation situationTouchedOpp = GetTouchedText(Conflict_Type, subType, Challenger, true);
                if (situationTouchedOpp != null)
                { tempListGood = situationTouchedOpp.GetGood(); tempListBad = situationTouchedOpp.GetBad(); }
                else { Game.SetError(new Error(105, string.Format("situation (Opponent Touched Skill) came back Null", Conflict_Type))); }
                goodIndex = rnd.Next(tempListGood.Count);
                badIndex = rnd.Next(tempListBad.Count);
                for (int i = 0; i < touchedOppCards; i++)
                {
                    Card_Conflict cardOpponent = new Card_Conflict(CardConflict.Skill, type, string.Format("{0}'s {1} Skill", opponent.Name, SkillType.Touched), description);
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
                text = string.Format("{0}'s Touched Skill, {1} card{2}, applies to all challenges ({3} stars) ", opponent.Name, touchedOppCards, touchedOppCards != 1 ? "s" : "",
                    touchedOppCards);
                listOpponentCards.Add(new Snippet(text, foreColor, backColor));
            }

            //Situation Cards
            numCards = GetSituationCardNumber(arrayModifiers[0]);
            //...advantage defender card -> First situation
            if (Challenger == true) { type = CardType.Bad; foreColor = RLColor.Red; arrayPool[2] += numCards; arrayPool[3] += numCards; }
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
                    else if (type == CardType.Bad) { foreColor = RLColor.Red;  arrayPool[2] += 1; arrayPool[3] += 1; }
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

            //Supporters -> Good
            for (int i = 0; i < arraySupportGood.Length; i++)
            {
                string supporterText = arraySupportGood[i];
                string supportDescription = "Lends a helping hand";
                type = CardType.Good;
                foreColor = RLColor.Black;
                //Situation immersion texts
                Situation situationHelp = GetSupporterText(Conflict_Type, subType, Challenger);
                if (situationHelp != null)
                { tempListGood = situationHelp.GetGood(); tempListBad = situationHelp.GetBad(); }
                else { Game.SetError(new Error(105, string.Format("situation (Supporters -> Good (Help)) came back Null", Conflict_Type))); }
                //no more valid data
                if (String.IsNullOrEmpty(supporterText) == true)
                { break; }
                Card_Conflict card = new Card_Conflict(CardConflict.Supporter, type, supporterText, supportDescription);
                //get immersion texts, if present
                if (tempListGood.Count > 0) { card.PlayedText = tempListGood[rnd.Next(0, tempListGood.Count)]; }
                if (tempListBad.Count > 0) { card.IgnoredText = tempListBad[rnd.Next(0, tempListBad.Count)]; }
                arrayPool[0]++;
                listCardPool.Add(card);
                listSupporterCards.Add(new Snippet(supporterText, foreColor, backColor)); //TO DO
            }
            //Supporters -> Bad
            for (int i = 0; i < arraySupportBad.Length; i++)
            {
                string supporter = arraySupportBad[i];
                string support_description = "Attempts to hinder you";
                type = CardType.Bad;
                foreColor = RLColor.Red;
                //Situation immersion texts
                Situation situationHinder = GetSupporterText(Conflict_Type, subType, Challenger);
                if (situationHinder != null)
                { tempListGood = situationHinder.GetGood(); tempListBad = situationHinder.GetBad(); }
                else { Game.SetError(new Error(105, string.Format("situation (Supporters -> Bad (Hinder)) came back Null", Conflict_Type))); }
                //no more valid data
                if (String.IsNullOrEmpty(supporter) == true)
                { break; }
                Card_Conflict card = new Card_Conflict(CardConflict.Supporter, type, supporter, support_description);
                //get immersion texts, if present
                if (tempListGood.Count > 0) { card.PlayedText = tempListGood[rnd.Next(0, tempListGood.Count)]; }
                if (tempListBad.Count > 0) { card.IgnoredText = tempListBad[rnd.Next(0, tempListBad.Count)]; }
                arrayPool[2]++;
                listCardPool.Add(card);
                listSupporterCards.Add(new Snippet(supporter, foreColor, backColor)); //TO DO
            }

            //Items -> Max one per protagonist
            if (arrayItems[0, 0]?.Length > 0)
            {
                //Challenger item
                int numItemCards = 0;
                try
                { numItemCards = Convert.ToInt32(arrayItems[0, 2]); }
                catch (FormatException)
                { Game.SetError(new Error(105, string.Format("Invalid input for Opponent Item CardNum (arrayItems[0, 2] \"{0}\" (format excepction)", arrayItems[0, 2]))); }
                catch (OverflowException)
                { Game.SetError(new Error(105, string.Format("Invalid input for Opponent Item CardNum (arrayItems[0, 2] \"{0}\" (overflow excepction)", arrayItems[0, 2]))); }
                string itemText = string.Format("{0}, \"{1}\", {2} Card{3} (Good)", arrayItems[0, 0], arrayItems[0, 1], numItemCards, numItemCards != 1 ? "s" : "");
                listItemCards.Add(new Snippet(itemText, RLColor.Black, backColor));
                for (int i = 0; i < numItemCards; i++)
                {
                    Card_Conflict card = new Card_Conflict(CardConflict.Item, CardType.Good, arrayItems[0, 0], arrayItems[0, 1])
                    { PlayedText = arrayItems[0, 3], IgnoredText = arrayItems[0, 4] };
                    listCardPool.Add(card);
                    arrayPool[0]++;
                }
            }
            if (arrayItems[1, 0]?.Length > 0)
            {
                //Opponent item
                int numItemCards = 0;
                try
                { numItemCards = Convert.ToInt32(arrayItems[1, 2]); }
                catch (FormatException)
                { Game.SetError(new Error(105, string.Format("Invalid input for Opponent Item CardNum (arrayItems[1, 2] \"{0}\" (format excepction)", arrayItems[1, 2]))); }
                catch (OverflowException)
                { Game.SetError(new Error(105, string.Format("Invalid input for Opponent Item CardNum (arrayItems[1, 2] \"{0}\" (overflow excepction)", arrayItems[1, 2]))); }
                string itemText = string.Format("{0}, \"{1}\", {2} Card{3} (Bad)", arrayItems[1, 0], arrayItems[1, 1], numItemCards, numItemCards != 1 ? "s" : "");
                listItemCards.Add(new Snippet(itemText, RLColor.Red, backColor));
                for (int i = 0; i < numItemCards; i++)
                {
                    Card_Conflict card = new Card_Conflict(CardConflict.Item, CardType.Good, arrayItems[1, 0], arrayItems[1, 1])
                    { PlayedText = arrayItems[1, 3], IgnoredText = arrayItems[1, 4] };
                    listCardPool.Add(card);
                    arrayPool[2]++;
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
            listBreakdown.Add(new Snippet(""));
            listBreakdown.AddRange(listSupporterCards);
            listBreakdown.Add(new Snippet(""));
            listBreakdown.AddRange(listItemCards);
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
                string traitText = actor.GetTraitName(skill);
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
                case ConflictState.Known_Status:
                    //Visibility of the Usurper - higher Revert, more visible if known and higher TurnsUnknown, less visible, if unknown. No neutral outcome.
                    if (player.Known == true)
                    { modifier = player.Revert * 20; difference = -100; description = "you are Known (Bad)"; }
                    else
                    { modifier = player.TurnsUnknown * 20; difference = 100; description = "you are Unknown (Good)"; }
                    modifier = Math.Min(100, modifier);
                    description = string.Format("how well you are Known in the Land {0}{1} %", difference > 0 ? "+" : "", difference);
                    /*difference = Game.director.CheckGameState(DataPoint.Invisibility) - 50;
                    modifier = Math.Abs(difference);
                    description = string.Format("relative Invisibility of Your Cause is {0}{1} %", difference > 0 ? "+" : "", difference);*/
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
            Game.logTurn?.Write(string.Format("SetGameSituation] -> \"{0}\", difference {1} Modifier {2} numCards {3}", description, difference, modifier, numCards));
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
        /// A conflict is always the Player vs. Opponent (or vice versa)
        /// </summary>
        /// <returns></returns>
        public Actor GetPlayer()
        { return player; }

        /// <summary>
        /// returns a string showing 'Title Player vs. Title Opponent', or vice versa depending on who challenged
        /// </summary>
        /// <returns></returns>
        public string GetProtagonists()
        {
            string returnText = null;
            if (Challenger == true) { returnText = string.Format("{0} {1} vs. {2} {3}", player.Title, player.Name, opponent.Title, opponent.Name ); }
            else { returnText = string.Format("{0} {1} vs. {2} {3}", opponent.Title, opponent.Name, player.Title, player.Name); }
            return returnText;
        }

        /// <summary>
        /// Input special, decision derived, situations. Always def.Adv. cards that help the defender. NOTE: Run AFTER SetOpponent()
        /// </summary>
        /// <param name="specialType"></param>
        /// <param name="numCards">leave at default '0' if you want a random #</param>
        /// <param name="unique">Add the enum for a unique card property</param>
        public void SetSpecialSituation(ConflictSpecial specialType, int numCards = 0, CardUnique unique = CardUnique.None)
        {
            Game.logTurn?.Write("--- SetSpecialSituation (Conflict.cs)");
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
                            Game.logTurn?.Write(string.Format("\"{0}\", numCards {1}, Added to listCardsSpecial", situation.Name, numCards));
                            //add cards to special card list
                            for (int i = 0; i < number; i++)
                            {
                                Card_Conflict card = new Card_Conflict(CardConflict.Situation, type, situation.Name, "A Special Situation");
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
        /// Resolves results for the particular outcome achieved
        /// </summary>
        /// <param name="result"></param>
        /// <returns>A snippet list describing each individual result, suitable for display, green for good, red for bad</returns>
        internal List<Snippet> ResolveResults(ConflictResult outcome)
        {
            Game.logTurn?.Write("--- ResolveResults (Conflict.cs)");
            List<Snippet> tempList = new List<Snippet>();
            List<int> resultList = challenge.GetResults(outcome);
            int resultID;
            string tempText;
            string testText = "";
            RLColor tempColor;
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
                        tempText = "";
                        RLColor backColor = Game.layout.Outcome_FillColor;
                        //Test result? Only applies is test passed
                        if (result.Test > 0)
                        {
                            int rndNum = rnd.Next(0, 100);
                            testText = string.Format("(needed {0} %, or less, rolled {1})", result.Test, rndNum);
                            if (rndNum > result.Test)
                            { type = ResultType.None; Game.logTurn?.Write(string.Format("{0} failed Test {1}", result.Description, testText)); }
                            else { Game.logTurn?.Write(string.Format("{0} PASSED Test {1}", result.Description, testText)); }
                        }
                        //check for a random outcome -> then it's random 'amount', eg. amount is 100 then it's d100, -100 then it's -1d100
                        if (calc == EventCalc.Random)
                        {
                            if (amount > 1) { amount = rnd.Next(1, amount); calc = EventCalc.Add; }
                            else if (amount < -1) { amount = rnd.Next(1, Math.Abs(amount)) * -1; calc = EventCalc.Add; }
                            int halfAmount;
                            Game.logTurn?.Write(string.Format("RelPlyr amount beforehand {0}", amount));
                            //if random amount comes in at less than half the range then bump it up to half (effective range = Amount/2 -> Amount)
                            if (result.Amount > 0) { halfAmount = result.Amount / 2; if (amount < halfAmount) { amount = halfAmount; } }
                            if (result.Amount < 0) { halfAmount = result.Amount / 2; if (amount > halfAmount) { amount = halfAmount; } }
                            Game.logTurn?.Write(string.Format("RelPlyr amount afterwards {0}", amount));
                        }
                        //resolve the actual result
                        Message message = null;
                        int refID = Game.world.GetRefID(player.LocID);
                        switch (type)
                        {
                            case ResultType.DataPoint:
                                Game.logTurn?.Write(string.Format("A {0} Result, ID {1}, Data {2}, Calc {3}, Amount {4}", type, result.ResultID, result.Data, result.Calc, result.Amount));
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
                                            tempText = string.Format("{0} has increased by {1} as a result of the {2} conflict", result.DataPoint, amount, Conflict_Type);
                                            tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                            message = new Message(string.Format("{0}", tempText), MessageType.Conflict);
                                        }
                                        else
                                        {
                                            int oldValue = Game.director.GetGameState(result.DataPoint, DataState.Bad);
                                            int newValue = Math.Abs(amount) + oldValue;
                                            Game.director.SetGameState(result.DataPoint, DataState.Bad, newValue, true);
                                            tempText = string.Format("{0} has decreased by {1} as a result of the {2} conflict", result.DataPoint, amount, Conflict_Type);
                                            tempList.Add(new Snippet(tempText, RLColor.Red, backColor));
                                            message = new Message(tempText, MessageType.Conflict);
                                        }
                                        if (message != null)
                                        {
                                            //record
                                            Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                        }
                                    }
                                    else Game.SetError(new Error(113, "Invalid DataPoint Amount (zero)"));
                                }
                                break;
                            case ResultType.GameVar:
                                //Change a GameVar
                                tempText = Game.director.ChangeGameVarStatus(result.GameVar, result.Amount, result.Calc);
                                if (tempText.Length > 0)
                                {
                                    tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                    message = new Message(tempText, MessageType.Conflict);
                                }
                                break;
                            case ResultType.Known:
                                //change Player's Known status
                                tempText = Game.world.SetActiveActorKnownStatus(1, result.Data);
                                if (result.Data > 0) { tempList.Add(new Snippet(tempText, RLColor.Green, backColor)); }
                                else { tempList.Add(new Snippet(tempText, RLColor.Red, backColor)); }
                                message = new Message(tempText, MessageType.Conflict);
                                Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                break;
                            case ResultType.RelPlyr:
                                //change Opponent's relationship with Player
                                opponent.AddRelEventPlyr(new Relation(result.Description, result.Tag, amount));
                                tempText = string.Format("{0} {1}'s relationship with you has {2} by {3}{4}", opponent.Title, opponent.Name,
                                   amount > 0 ? "improved" : "worsened", amount > 0 ? "+" : "", amount);
                                tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                message = new Message(tempText, MessageType.Conflict);
                                Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                Game.world.SetCurrentRecord(new Record(tempText, opponent.ActID, opponent.LocID, refID, CurrentActorIncident.Challenge));
                                break;
                            case ResultType.RelOther:
                                break;
                            case ResultType.Condition:
                                //add a condition to either the Player or Opponent
                                Condition condition = new Condition(result.ConSkill, result.ConEffect, result.ConText, result.ConTimer);
                                string nameText = "unknown";
                                Actor tempActor = null;
                                //Player condition
                                if (result.ConPlayer == true) { tempActor = player; }
                                //Opponent condition
                                else { tempActor = opponent; }
                                //Does the character already have this condition?
                                if (tempActor.CheckConditionPresent(condition.Text) == false)
                                {
                                    tempActor.AddCondition(condition);
                                    nameText = string.Format("{0} {1}", tempActor.Title, tempActor.Name);
                                    tempText = string.Format("{0} gains the \"{1}\" Condition, {2} {3}{4}", nameText, condition.Text, condition.Skill, condition.Effect > 0 ? "+" : "", 
                                        condition.Effect);
                                }
                                //existing identical condition already present -> Reset existing condition timer to the max value.
                                else
                                {
                                    tempActor.ResetConditionTimer(condition.Text, condition.Timer);
                                    nameText = string.Format("{0} {1}", tempActor.Title, tempActor.Name);
                                    tempText = string.Format("\"{0}\" Condition already acquired by {1}, Timer reset to {2}", condition.Text, nameText, condition.Timer);
                                }
                                //housekeeping
                                tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                message = new Message(tempText, MessageType.Conflict);
                                //Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                break;
                            case ResultType.Resource:
                                break;
                            case ResultType.Item:
                                int rndIndex;
                                tempColor = RLColor.Green;
                                //what type of item filter -> +ve then Active Items, -ve Passive Items, '0' for all
                                PossItemType filter = PossItemType.Both;
                                if (result.Data > 0) { filter = PossItemType.Active; }
                                else if (result.Data < 0) { filter = PossItemType.Passive; }
                                //Gain an item -> if your Opponent has one
                                if (result.Calc == EventCalc.Add && opponent.CheckItems(filter) == true)
                                {
                                    List<int> tempItems = opponent.GetItems(filter);
                                    rndIndex = rnd.Next(tempItems.Count);
                                    int possID = tempItems[rndIndex];
                                    if (possID > 0)
                                    {
                                        player.AddItem(possID);
                                        opponent.RemoveItem(possID);
                                        Item item = Game.world.GetItem(possID);
                                        tempText = string.Format("You have gained possession of \"{0}\", itemID {1}, from {2} {3} {4}", item.Description, item.ItemID, opponent.Title,
                                            opponent.Name, opponent.Handle);
                                    }
                                }
                                //Lose an item -> if you have one
                                if (result.Calc == EventCalc.Subtract && player.CheckItems(filter) == true)
                                {
                                    tempColor = RLColor.Red;
                                    List<int> tempItems = player.GetItems(filter);
                                    rndIndex = rnd.Next(tempItems.Count);
                                    int possID = tempItems[rndIndex];
                                    if (possID > 0)
                                    {
                                        opponent.AddItem(possID);
                                        player.RemoveItem(possID);
                                        Item item = Game.world.GetItem(possID);
                                        tempText = string.Format("You have lost possession of \"{0}\", itemID {1}, to {2} {3} {4}", item.Description, item.ItemID, opponent.Title,
                                            opponent.Name, opponent.Handle);
                                    }
                                }
                                //admin
                                if (tempText.Length > 0)
                                {
                                    tempList.Add(new Snippet(tempText, tempColor, backColor));
                                    message = new Message(tempText, MessageType.Conflict);
                                }
                                break;
                            case ResultType.Secret:
                                break;
                            case ResultType.Favour:
                                //data indicates the strength of the favour granted to Player
                                tempText = string.Format("{0} {1} grants you a Favour", opponent.Title, opponent.Name);
                                Favour newFavour = new Favour(tempText, data, opponent.ActID);
                                //add to dictionary and Player's list
                                if (Game.world.AddPossession(newFavour.PossID, newFavour) == true)
                                {
                                    player.AddFavour(newFavour.PossID);
                                    tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                    message = new Message(tempText, MessageType.Conflict);
                                    Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                }
                                break;
                            case ResultType.Introduction:
                                //data indicates the strength of the introduction granted to Player
                                tempText = string.Format("{0} {1} grants you an Introduction", opponent.Title, opponent.Name);
                                Introduction newIntroduction = new Introduction(tempText, data, opponent.ActID);
                                //add to dictionary and Player's list
                                if (Game.world.AddPossession(newIntroduction.PossID, newIntroduction) == true)
                                {
                                    player.AddIntroduction(newIntroduction.PossID);
                                    tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                    message = new Message(tempText, MessageType.Conflict);
                                    Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, refID, CurrentActorIncident.Challenge));
                                }
                                break;
                            case ResultType.Army:
                                break;
                            case ResultType.Event:
                                //data is the Player Event ID that is made Active (currently Dormant)
                                if (data > 0)
                                {
                                    //ordinary event
                                    if (data < 1000)
                                    {
                                        EventPlayer eventObject = Game.director.GetPlayerEvent(data);
                                        if (eventObject != null)
                                        {
                                            //activate event provided it isn't already or isn't dead
                                            if (eventObject.Status == EventStatus.Dormant || eventObject.Status == EventStatus.Live)
                                            {
                                                eventObject.Status = EventStatus.Active;
                                                tempText = string.Format("Event \"{0}\" has been activated", eventObject.Name);
                                                tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                                message = new Message(string.Format("{0} {1}", tempText, testText), MessageType.Conflict);
                                            }
                                        }
                                    }
                                    //AutoReact event
                                    else if (data >= 1000)
                                    {
                                        string multiText = "";
                                        EventPlayer eventAuto = Game.director.GetAutoEvent(data);
                                        if (eventAuto != null)
                                        {
                                            //make a copy of the event
                                            EventPlayer eventObject = new EventPlayer(eventAuto);
                                            //subRef applies? (specific filter for event)
                                            if (eventObject.SubRef > 0)
                                            {
                                                //Loc event, needs to match houseID of opponent
                                                if (eventObject.Type == ArcType.Location)
                                                {
                                                    if (Conflict_Type == ConflictType.Social)
                                                    {
                                                        if (Social_Type == ConflictSocial.Seduce)
                                                        {
                                                            multiText = "Lord"; if (opponent.Sex == ActorSex.Male) { multiText = "Lady"; }
                                                            eventObject.Name = Game.utility.CheckTagsAuto(eventObject.Name, multiText);
                                                        }
                                                    }
                                                    eventObject.Text = Game.utility.CheckTagsAuto(eventObject.Text, multiText, opponent);
                                                    Passive tempOpponent = opponent as Passive;
                                                    eventObject.SubRef = tempOpponent.HouseID;
                                                    Game.logTurn?.Write(string.Format("Event \"{0}\" assigned subRef {1} (HouseID)", eventObject.Name, eventObject.SubRef));
                                                }
                                                //Terrain cluster, needs to match GeoID of cluster
                                                else if (eventObject.Type == ArcType.GeoCluster)
                                                {
                                                    Location loc = Game.network.GetLocation(player.LocID);
                                                    if (loc != null)
                                                    {
                                                        eventObject.SubRef = Game.map.GetMapInfo(MapLayer.GeoID, loc.GetPosX(), loc.GetPosY());
                                                        GeoCluster cluster = Game.world.GetGeoCluster(eventObject.SubRef);
                                                        eventObject.Text = Game.utility.CheckTagsAuto(eventObject.Text, null, null, cluster);
                                                        Game.logTurn?.Write(string.Format("Event \"{0}\" assigned subRef {1} (GeoID)", eventObject.Name, eventObject.SubRef));
                                                    }
                                                    else { Game.SetError(new Error(113, "Invalid Terrain Cluster Loc (null)")); }
                                                }
                                            }
                                            //assign to correct event list & master dictionary
                                            if (Game.director.AddPlayerEvent(eventObject) == true)
                                            {
                                                Game.director.AssignPlayerEvent(eventObject);
                                                Game.director.NumAutoReactEvents++;
                                                tempText = string.Format("Event \"{0}\" has been activated", eventObject.Name);
                                                tempList.Add(new Snippet(tempText, RLColor.Green, backColor));
                                                message = new Message(string.Format("{0} {1}", tempText, testText), MessageType.Conflict);
                                            }
                                        }
                                    }
                                }
                                break;
                            //prevents infinite loop (ignored result)
                            case ResultType.None:
                                break;
                            case ResultType.Freedom:
                                //data > 0, player status -> AtLocation, data < 0, player status -> Captured, data 0 -> ignored
                                tempText = "";
                                tempColor = RLColor.Green;
                                if (data > 0)
                                {
                                    //only valid if Player is already captured -> at a location
                                    if (player.Status == ActorStatus.Captured)
                                    {
                                        player.Status = ActorStatus.AtLocation;
                                        tempText = string.Format("You have managed to escape the {0} dungeons", player.LocID);
                                    }
                                    else { Game.SetError(new Error(113, "Player Status isn't currently 'Captured'(Result)")); }
                                }
                                else if (data < 0)
                                {
                                    tempColor = RLColor.Red;
                                    tempText = string.Format("You have been Captured by {0} {1}, ActID {2}", opponent.Title, opponent.Name, opponent.ActID);
                                    //items lost
                                    if (player.CheckItems() == true)
                                    {
                                        List<int> tempItems = player.GetItems();
                                        int numItems = tempItems.Count();
                                        tempText += " and any items that you possess will be confiscated";
                                    }
                                    //Incarcerated
                                    Game.world.SetPlayerCaptured(opponent.ActID);
                                }
                                else { Game.SetError(new Error(113, "Invalid Data value (zero) for Freedom Result")); }
                                if (String.IsNullOrEmpty(tempText) == false)
                                { tempList.Add(new Snippet(tempText, tempColor, backColor)); }
                                break;
                            default:
                                Game.SetError(new Error(113, string.Format("Invalid ResultType (\"{0}\")", type)));
                                break;
                        }
                        if (message != null)
                        { Game.world.SetMessage(message); }
                    }
                    else
                    { Game.logTurn?.Write(string.Format("[Result -> Notification] Invalid, or Missing, result (null returned, resultID \"{0}\")", resultID)); }
                }
            }
            else { Game.SetError(new Error(113, string.Format("Invalid Input (no results from \"{0}\")", outcome))); }
            return tempList;
        }

        // methods above here
    }
}
