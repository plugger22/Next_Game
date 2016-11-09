using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// default Constructor
        /// </summary>
        public Conflict(int seed)
        {
            rnd = new Random(seed);
            //get player (always the player vs. somebody else)
            player = Game.world.GetActiveActor(1);
            if (player != null)
            { }
            else { Game.SetError(new Error(84, "Player not found (null)")); }
        }
        
        /*
        /// <summary>
        /// default constructor -> Combat conflict
        /// </summary>
        public Conflict(int opponentID, ConflictType conflictType, CombatType combatType)
        {
            if (opponentID > 0)
            {
                //actors
                player = Game.world.GetActiveActor(1);
                opponent = Game.world.GetActiveActor(opponentID);
                if (player != null || opponent != null)
                {
                    this.conflictType = conflictType;
                    this.combatType = combatType;
                }
                else { Game.SetError(new Error(84, "Player or Opponent not found (null)")); }
            }
            else { Game.SetError(new Error(84, "Invalid Opponent input (null)")); }
        }


        /// <summary>
        /// default constructor -> Social conflict
        /// </summary>
        public Conflict(int opponentID, ConflictType conflictType, SocialType socialType)
        {
            if (opponentID > 0)
            {
                //actors
                player = Game.world.GetActiveActor(1);
                opponent = Game.world.GetActiveActor(opponentID);
                if (player != null || opponent != null)
                {
                    this.conflictType = conflictType;
                    this.socialType = socialType;
                }
                else { Game.SetError(new Error(84, "Player or Opponent not found (null)")); }
            }
            else { Game.SetError(new Error(84, "Invalid Opponent input (null)")); }
        }
        */

        /// <summary>
        /// Master method that handles all conflict set up
        /// </summary>
        public void InitialiseConflict()
        {
            SetPlayerStrategy();
            SetSituation();
            SetOpponentStrategy();

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
                opponent = Game.world.GetActiveActor(actorID);
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
                            tempArray[1] = "A Bright Sun, Low on the Horizon";
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
            string[] tempArray = new string[9];
            //type of conflict
            tempArray[0] = "A " + Convert.ToString(Combat_Type);
            //descriptions of outcomes (minor/standard/major wins and losses)
            switch (Conflict_Type)
            {
                case ConflictType.Combat:
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
                            tempArray[1] = "Your Opponent withdraws from the contest but refuses to accept that he lost";
                            tempArray[2] = "You unhorse your Opponent and claim victory and glory";
                            tempArray[3] = "Your Opponent is thrown from the saddle and badly Injured";
                            tempArray[4] = "";
                            tempArray[5] = "";
                            tempArray[6] = "";
                            break;
                        case CombatType.Battle:
                            tempArray[1] = "";
                            tempArray[2] = "";
                            tempArray[3] = "";
                            tempArray[4] = "";
                            tempArray[5] = "";
                            tempArray[6] = "";
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
            //send to layout
            if (tempArray.Length == 9)
            { Game.layout.SetOutcome(tempArray); }
            else
            { Game.SetError(new Error(89, "Invalid Situation, Layout not updated")); }
        }

        // methods above here
    }
}
