using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    /// <summary>
    /// Handles the card Conflict system. Single class as there is only ever a single instance in existence.
    /// </summary>
    class Conflict
    {
        //protagonists
        Actor opponent;
        Active player;
        //type of conflict
        ConflictType conflictType;
        CombatType combatType;
        SocialType socialType;
        //strategy
        public int Strategy_Player { get; set; }
        public int Strategy_Opponent { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public Conflict(Actor opponent)
        {
            if (opponent != null)
            {
                //actors
                player = Game.world.GetActiveActor(1);
                this.opponent = opponent;
            }
            else { Game.SetError(new Error(84, "Invalid Opponent input (null)")); }
        }


        // methods above here
    }
}
