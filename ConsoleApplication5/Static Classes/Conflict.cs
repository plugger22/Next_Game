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
        // methods above here
    }
}
