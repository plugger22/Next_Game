using Next_Game.Cartographic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Next_Game
{
    /// <summary>
    /// keeps tab on actor (all types) turn by turn
    /// </summary>
    class ActorSpy
    {
        public int Turn { get; set; }
        public int ActID { get; set; }
        public Position Pos { get; set; } //position at start of turn
        public ActorStatus Status { get; set; } //at end of ProcessStartTurn
        public ActorGoal Goal { get; set; } //enemy Actors only -> at end of ProcessStartTurn
        public bool Known { get; set; } //actor known or not (all) -> at end of ProcessStartTurn
        public bool HuntMode { get; set; } //enemy Actors only -> at end of ProcessStartTurn

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="actID"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        /// <param name="status"></param>
        /// <param name="known"></param>
        /// <param name="goal"></param>
        public ActorSpy(int actID, Position pos, ActorStatus status, bool known, ActorGoal goal = ActorGoal.None, bool huntMode = false)
        {
            if (actID > 0)
            {
                if (pos != null)
                {
                    Turn = Game.gameTurn;
                    this.ActID = actID;
                    this.Pos= pos;
                    this.Status = status;
                    this.Known = known;
                    this.Goal = goal;
                    this.HuntMode = huntMode;
                }
                else { Game.SetError(new Error(168, string.Format("Invalid Position input (Pos {0}:{1})", pos.PosX, pos.PosY))); }
            }
            else { Game.SetError(new Error(168, string.Format("Invalid ActID input (\"{0}\")", actID))); }
        }
    }

    /// <summary>
    /// tracks game data for analysis and 'see what happened' at game end
    /// </summary>
    class BloodHound
    {
        private List<ActorSpy> listActiveActors;
        private List<ActorSpy> listEnemyActors;

        /// <summary>
        /// Constructor
        /// </summary>
        public BloodHound()
        {
            listActiveActors = new List<ActorSpy>();
            listEnemyActors = new List<ActorSpy>();
        }

        /// <summary>
        /// Add a list to Active Actors
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetActiveActors(List<ActorSpy> tempList)
        {
            if (tempList != null)
            {
                if (tempList.Count > 0)
                {
                    listActiveActors.AddRange(tempList);
                }
                else { Game.SetError(new Error(169, "Invalid tempList input (null)")); }
            }
            else { Game.SetError(new Error(169, "Invalid tempList input (Empty)")); }
        }

        /// <summary>
        /// Add a list to Enemy Actors
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetEnemyActors(List<ActorSpy> tempList)
        {
            if (tempList != null)
            {
                if (tempList.Count > 0)
                {
                    listEnemyActors.AddRange(tempList);
                }
                else { Game.SetError(new Error(170, "Invalid tempList input (null)")); }
            }
            else { Game.SetError(new Error(170, "Invalid tempList input (Empty)")); }
        }

        internal List<ActorSpy> GetActiveActors()
        { return listActiveActors; }

        internal List<ActorSpy> GetEnemyActors()
        { return listEnemyActors; }

        //add new methods above here
    }
}
