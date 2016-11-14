using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;
using RLNET;

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
        Active player;
        public bool Challenger { get; set; } //is the Player the Challenger?
        //type of conflict
        public ConflictType Conflict_Type { get; set; }
        public CombatType Combat_Type { get; set; }
        public SocialType Social_Type { get; set; }
        //Card Pool
        private List<Card_Conflict> listCardPool;
        private List<Card_Conflict> listCardHand; //hand that will be played
        private List<Snippet> listBreakdown; //description of card pool contents
        //skills
        public SkillType PrimarySkill { get; set; } //each skill level counts as 2 cards
        public SkillType OtherSkill_1 { get; set; } //only trait effects count
        public SkillType OtherSkill_2 { get; set; }
        //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
        private int[] arrayPool;
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
            listCardPool = new List<Card_Conflict>();
            listCardHand = new List<Card_Conflict>();
            listBreakdown = new List<Snippet>();
            //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
            arrayPool = new int[3];
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
            SetSituation();
            SetOpponentStrategy();
            SetOutcome();
            SetCardPool();
            SetHand();
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
                        case CombatType.Personal:
                            tempArray[0] = "Go for the Throat";
                            tempArray[1] = "Be Flexible";
                            tempArray[2] = "Focus on Staying Alive";
                            break;
                        case CombatType.Tournament:
                            tempArray[0] = "Knock them to the Ground";
                            tempArray[1] = "Wait for an Opportunity";
                            tempArray[2] = "Stay in the Saddle";
                            break;
                        case CombatType.Battle:
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
                        case SocialType.Befriend:
                            tempArray[0] = "Do what Whatever it Takes";
                            tempArray[1] = "Extend the Hand of Friendship";
                            tempArray[2] = "Approach them with Caution";
                            break;
                        case SocialType.Blackmail:
                            tempArray[0] = "Lean on Them. Hard.";
                            tempArray[1] = "Explain the Facts of Life";
                            tempArray[2] = "Gently Nudge Them";
                            break;
                        case SocialType.Seduce:
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
        /// Determine opponent's strategy and send to layout
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
        internal void SetOpponent(int actorID)
        {
            if (actorID > 0)
            {
                opponent = Game.world.GetAnyActor(actorID);
                if (opponent == null)
                { Game.SetError(new Error(88, "Opponent not found (null)")); }
            }
            else { Game.SetError(new Error(88, "Invalid actorID input (<= 0)")); }
        }

        /// <summary>
        /// Set up the situation and send to layout
        /// </summary>
        private void SetSituation()
        {
            string[] tempArray = new string[3];
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
                    switch (Combat_Type)
                    {
                        case CombatType.Personal:
                            tempArray[0] = "Uneven Ground";
                            tempArray[1] = "The Sun at Your Back";
                            tempArray[2] = "Outnumbered";
                            break;
                        case CombatType.Tournament:
                            tempArray[0] = "A Weakened Jousting Lance";
                            tempArray[1] = "Cheers of the Crowd";
                            tempArray[2] = "Opponent has Momentum";
                            break;
                        case CombatType.Battle:
                            tempArray[0] = "A Defendable Hill";
                            tempArray[1] = "Muddy Ground";
                            tempArray[2] = "Relative Army Sizes";
                            break;
                        default:
                            Game.SetError(new Error(86, "Invalid Combat Type"));
                            break;
                    }
                    break;
                case ConflictType.Social:
                    switch (Social_Type)
                    {
                        case SocialType.Befriend:
                            tempArray[0] = "A fine Arbor Wine";
                            tempArray[1] = "A Pleasant Lunch";
                            tempArray[2] = "Your Reputation Precedes you";
                            break;
                        case SocialType.Blackmail:
                            tempArray[0] = "A Noisey Room full of People";
                            tempArray[1] = "The Threat of Retaliation";
                            tempArray[2] = "Difficulty of a Meaningful Threat";
                            break;
                        case SocialType.Seduce:
                            tempArray[0] = "A Romantic Venue";
                            tempArray[1] = "A Witch's Aphrodisiac";
                            tempArray[2] = "The Lady is Happily Married";
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
            //send to layout
            if (tempArray.Length <= 3 && tempArray.Length > 0)
            { Game.layout.SetSituation(tempArray); }
            else
            { Game.SetError(new Error(89, "Invalid Situation, Layout not updated")); }
        }

        /// <summary>
        /// determine # of cards
        /// </summary>
        /// <returns></returns>
        private int GetSituationCard()
        {
            int numCards = 1;
            int rndNum = rnd.Next(100);
            if (rndNum >= 70)
            {
                if (rndNum >= 98) { numCards = 5; }
                else if (rndNum >= 94) { numCards = 4; }
                else if (rndNum >= 86) { numCards = 3; }
                else { numCards = 2; }
            }
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
                        case CombatType.Personal:
                            tempArray[1] = "Your Opponent retires with a minor wound and an injured ego";
                            tempArray[2] = "Your Opponent Yields and you can claim an Advantage from him";
                            tempArray[3] = "Your Opponent Suffers a Major Wound and may die";
                            tempArray[4] = "You suffer a minor wound and retire defeated";
                            tempArray[5] = "You are Forced to Yield to a superior Opponent who can demand an Advantage";
                            tempArray[6] = "You have been Badly Injured and Lose any Special Items";
                            break;
                        case CombatType.Tournament:
                            tempArray[1] = "You make the final group but fail to go any further";
                            tempArray[2] = "You reach the top three jousters and gain glory and recognition";
                            tempArray[3] = "You are named Tournament Champion and gain a Ladies Favour";
                            tempArray[4] = "You are unhorsed midway through the tournament";
                            tempArray[5] = "You are unhorsed early on by a mid ranked jouster";
                            tempArray[6] = "You fall off your horse and break bones on your first joust. Disgrace!";
                            break;
                        case CombatType.Battle:
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
                        case SocialType.Befriend:
                            tempArray[1] = "Your relationship improves";
                            tempArray[2] = "You have become firm friends";
                            tempArray[3] = "You have gained an ardent supporter";
                            tempArray[4] = "Your relationship deteriorates";
                            tempArray[5] = "You have become disliked";
                            tempArray[6] = "Your opponent is actively campaigning against you";
                            break;
                        case SocialType.Blackmail:
                            tempArray[1] = "You have gained a small amount of influence";
                            tempArray[2] = "Your opponent agrees to your demands";
                            tempArray[3] = "Your opponent has become your minion";
                            tempArray[4] = "You relationship has deteroriated";
                            tempArray[5] = "You have been firmly rebuffed";
                            tempArray[6] = "Your opponent is now your enemy";
                            break;
                        case SocialType.Seduce:
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
            tempArray[9] = string.Format("{0} {1}{2} vs. {3} {4}{5}", player.Type, player.Name, handle_player, title, opponent.Name, handle_opponent);
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
                        case CombatType.Personal:
                            PrimarySkill = SkillType.Combat;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Treachery;
                            break;
                        case CombatType.Tournament:
                            PrimarySkill = SkillType.Combat;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Treachery;
                            break;
                        case CombatType.Battle:
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
                        case SocialType.Befriend:
                            PrimarySkill = SkillType.Charm;
                            OtherSkill_1 = SkillType.Treachery;
                            OtherSkill_2 = SkillType.Wits;
                            break;
                        case SocialType.Blackmail:
                            PrimarySkill = SkillType.Treachery;
                            OtherSkill_1 = SkillType.Wits;
                            OtherSkill_2 = SkillType.Charm;
                            break;
                        case SocialType.Seduce:
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
            //card pool analysis (0 - # good cards, 1 - # neutral cards, 2 - # bad cards)
            Array.Clear(arrayPool, 0 , arrayPool.Length);
            //three lists to consolidate into pool breakdown description
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
                //reverse the sign (good/bad) if opponent (who is anyone else other than the Ursurper)
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
        /// Draw cards to form a challenge hand
        /// </summary>
        private void SetHand()
        {
            int numCards = Game.constant.GetValue(Global.HAND_CARDS_NUM);
            //check enough cards in pool, if not downsize hand
            if (numCards > listCardPool.Count)
            { numCards = listCardPool.Count; Game.SetError(new Error(93, "Not enough cards in the Pool")); }
            //clear out the list
            listCardHand.Clear();
            int rndNum;
            for(int i = 0; i < numCards; i++)
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
        

    
        // methods above here
    }
}
