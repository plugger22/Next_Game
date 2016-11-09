using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;

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
        //type of conflict
        public ConflictType Conflict_Type { get; set; }
        public CombatType Combat_Type { get; set; }
        public SocialType Social_Type { get; set; }
        //Card Pool
        private List<Card> listCardPool;
        private List<Snippet> listBreakdown; //description of card pool contents
        //skills
        public SkillType PrimarySkill { get; set; } //each skill level counts as 2 cards
        public SkillType OtherSkill_1 { get; set; } //only trait effects count
        public SkillType OtherSkill_2 { get; set; }

        /// <summary>
        /// default Constructor
        /// </summary>
        public Conflict(int seed)
        {
            rnd = new Random(seed);
            listCardPool = new List<Card>();
            listBreakdown = new List<Snippet>();
            //get player (always the player vs. somebody else)
            player = Game.world.GetActiveActor(1);
            if (player != null) {}
            else { Game.SetError(new Error(84, "Player not found (null)")); }
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
        }

        /// <summary>
        /// Determine Strategy and send to Layout
        /// </summary>
        public void SetPlayerStrategy()
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
            { Game.layout.SetStrategy(tempArray); }
            else
            { Game.SetError(new Error(86, "Invalid Strategy, Layout not updated")); }
        }

        /// <summary>
        /// Determine opponent's strategy and send to layout
        /// </summary>
        public void SetOpponentStrategy()
        {
            //placeholder 
            Game.layout.Strategy_Opponent = rnd.Next(0, 3);
        }

        /// <summary>
        /// specify opponent (it's always the player vs. opponent)
        /// </summary>
        /// <param name="actorID"></param>
        public void SetOpponent(int actorID)
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
        public void SetSituation()
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
        /// Set up outcomes and send to layout
        /// </summary>
        public void SetOutcome()
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
                            tempArray[1] = "";
                            tempArray[2] = "";
                            tempArray[3] = "";
                            tempArray[4] = "";
                            tempArray[5] = "";
                            tempArray[6] = "";
                            break;
                        case SocialType.Blackmail:
                            tempArray[1] = "";
                            tempArray[2] = "";
                            tempArray[3] = "";
                            tempArray[4] = "";
                            tempArray[5] = "";
                            tempArray[6] = "";
                            break;
                        case SocialType.Seduce:
                            tempArray[1] = "";
                            tempArray[2] = "";
                            tempArray[3] = "";
                            tempArray[4] = "";
                            tempArray[5] = "";
                            tempArray[6] = "";
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
            tempArray[7] = "You have the Advantage";
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
        public void SetTraits()
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
        public void SetCardPool()
        {

        }

        // methods above here
    }
}
