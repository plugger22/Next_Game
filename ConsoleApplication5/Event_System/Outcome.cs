using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{


    /// <summary>
    /// Option outcome, event system
    /// </summary>
    class Outcome
    {
        private static int outcomeIndex = 1; //autoassigned ID's. Main focus is the Outcome Class
        public int OutcomeID { get; }
        public int EventID { get; } //could be EventFID (follower) or EventPID (player)
        public int Data { get; set; } //optional multipurpose type for use with resolve
        public int Amount { get; set; }
        public EventCalc Calc { get; set; }
        


        public Outcome(int eventID)
        {
            OutcomeID = outcomeIndex++;
            if (eventID > 0) { EventID = eventID; }
            else { Game.SetError(new Error(67, "Invalid Input (eventID) in constructor")); }
        }
        
    }


    // --- Follower subclass ---

    /// <summary>
    /// Gives a delay to an Active Actor
    /// </summary>
    class OutDelay : Outcome
    {
        public int Delay { get; set; } //number of turns for the delay

        public OutDelay(int delay, int eventID) : base (eventID)
        {
            
            if (delay > 0)
            { this.Delay = delay; }
            else { Game.SetError(new Error(67, "Invalid Input (Delay) in OutDelay")); }
        }

        /// <summary>
        /// data1 is ActorID -> message & update actor delay
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public void Resolve(int data1, int data2 = 0)
        {
            Active actor = Game.world.GetActiveActor(data1);
            Event eventObject = Game.director.GetFollowerEvent(EventID);
            if (actor != null)
            {
                if (eventObject != null)
                {
                    actor.Delay += Delay;
                    actor.DelayReason = eventObject.Name;
                    //message
                    Message message = new Message(string.Format("{0} has been {1} (\"{2}\") for {3} {4}", actor.Name, eventObject.Type == ArcType.Location ? "indisposed" : "delayed",
                        actor.DelayReason, Delay, Delay == 1 ? "Day" : "Day's"), MessageType.Move);
                    Game.world.SetMessage(message);
                }
                else { Game.SetError(new Error(67, "Event not found using EventID in OutDelay.cs")); }
            }
            else { Game.SetError(new Error(67, "Active Actor not found")); }
        }
    }


    //--- Player subclasses ---

    /// <summary>
    /// Do nothing outcome
    /// </summary>
    class OutNone : Outcome
    {
        public OutNone(int eventID) : base (eventID)
        {
            //set all to default (they aren't used but do show up on debug messages)
            Data = 0;
            Amount = 0;
            Calc = EventCalc.None;
        }
    }

    /// <summary>
    /// Player outcome -> Initiate a conflict
    /// </summary>
    class OutConflict : Outcome
    {
        public bool Challenger { get; set; } //is the Player the Challenger?
        public ConflictType Conflict_Type { get; set; }
        public ConflictCombat Combat_Type { get; set; }
        public ConflictSocial Social_Type { get; set; }
        public ConflictStealth Stealth_Type { get; set; }
        public ConflictSubType SubType { get; set; } //descriptive purposes only

        public OutConflict(int eventID, int opponentID, ConflictType type, bool challenger = true) : base (eventID)
        {
            this.Data = opponentID;
            Conflict_Type = type;
            this.Challenger = challenger;
        }

    }


    /// <summary>
    /// Player outcome -> changes a Game variable
    /// </summary>
    class OutGame : Outcome
    {
        public OutGame(int eventID, int type, int amount, EventCalc apply = EventCalc.None) : base(eventID)
        {
            this.Data = type;
            this.Amount = amount;
            this.Calc = apply;
        }

    }


    /// <summary>
    /// Player outcome -> change an Event's Timer
    /// </summary>
    class OutEventTimer : Outcome
    {
        public EventTimer Timer { get; set; }

        public OutEventTimer(int eventID, int targetEventID, int amount, EventCalc apply, EventTimer timer) : base(eventID)
        {
            Data = targetEventID;
            this.Amount = amount;
            this.Calc = apply;
            this.Timer = timer;
        }
    }

    /// <summary>
    /// Player outcome -> change an Event's Status
    /// </summary>
    class OutEventStatus : Outcome
    {
        public EventStatus NewStatus { get; set; }
        //

        public OutEventStatus(int eventID, int targetEventID, EventStatus status) : base(eventID)
        {
            NewStatus = status;
            Data = targetEventID;
        }
    }


    /// <summary>
    /// Chain Event outcome ->  immediate trigger of specified Player event
    /// </summary>
    class OutEventChain : Outcome
    {
        public EventFilter Filter { get; set; }
       
        public OutEventChain(int eventID, EventFilter filter) : base(eventID)
        {
            Data = 0;
            this.Filter = filter;
        }
    }

}
