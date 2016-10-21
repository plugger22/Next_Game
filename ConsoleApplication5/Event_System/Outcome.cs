using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum OutApply { None, Add, Subtract, Random} //Random is rnd.Next(amount)
    

    /// <summary>
    /// Option outcome, event system
    /// </summary>
    class Outcome
    {
        private static int outcomeIndex = 1; //autoassigned ID's. Main focus is the Outcome Class
        public int OutcomeID { get; }
        public int EventID { get; } //could be EventFID (follower) or EventPID (player)
        public int Data { get; set; } //optional stored data point for use with resolve
        public int Amount { get; set; }
        public OutApply Apply { get; set; }
        


        public Outcome(int eventID)
        {
            OutcomeID = outcomeIndex++;
            if (eventID > 0) { EventID = eventID; }
            else { Game.SetError(new Error(67, "Invalid Input (eventID) in constructor")); }
        }

        /// <summary>
        /// virtual Resolve method used by all dervied classes. Data1 to 3 could refer to any ID type. Handles details and gives message
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public virtual void Resolve(int data1, int data2 = 0)
        { }

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
        public override void Resolve(int data1, int data2 = 0)
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
    /// Player outcome -> Initiate a conflict
    /// </summary>
    class OutConflict : Outcome
    {
        public OutConflict(int eventID, int data, int amount, OutApply apply = OutApply.None) : base (eventID)
        {
            this.Data = data;
            this.Amount = amount;
            this.Apply = apply;
        }

        /// <summary>
        /// data1 is actor ID
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public override void Resolve(int data1, int data2 = 0)
        {
            Active actor = Game.world.GetActiveActor(data1);
            Event eventObject = Game.director.GetPlayerEvent(EventID);
            if (actor != null)
            {
                if (eventObject != null)
                {
                    //message
                    Message message = new Message(string.Format("{0} is involved in a CONFLICT", actor.Name), MessageType.Move);
                    Game.world.SetMessage(message);
                }
                else { Game.SetError(new Error(67, "Event not found using EventPID in OutDelay.cs")); }
            }
            else { Game.SetError(new Error(67, "Active Actor not found")); }
        }
    }


    /// <summary>
    /// Player outcome -> changes a Game variable
    /// </summary>
    class OutGame : Outcome
    {
        public OutGame(int eventID, int data, int amount, OutApply apply = OutApply.None) : base(eventID)
        {
            this.Data = data;
            this.Amount = amount;
            this.Apply = apply;
        }

        /// <summary>
        /// data1 is actor ID
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public override void Resolve(int data1, int data2 = 0)
        {
            Active actor = Game.world.GetActiveActor(data1);
            Event eventObject = Game.director.GetPlayerEvent(EventID);
            if (actor != null)
            {
                if (eventObject != null)
                {
                    //message
                    Message message = new Message(string.Format("{0} is involved in changing a GAME VARIABLE", actor.Name), MessageType.Move);
                    Game.world.SetMessage(message);
                }
                else { Game.SetError(new Error(67, "Event not found using EventPID in OutDelay.cs")); }
            }
            else { Game.SetError(new Error(67, "Active Actor not found")); }
        }
    }


    /// <summary>
    /// Player outcome -> do something to an Event
    /// </summary>
    class OutEvent : Outcome
    {
        public OutEvent(int eventID, int data, int amount, OutApply apply = OutApply.None) : base(eventID)
        {
            this.Data = data;
            this.Amount = amount;
            this.Apply = apply;
        }

        /// <summary>
        /// data1 is actor ID
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        public override void Resolve(int data1, int data2 = 0)
        {
            Active actor = Game.world.GetActiveActor(data1);
            Event eventObject = Game.director.GetPlayerEvent(EventID);
            if (actor != null)
            {
                if (eventObject != null)
                {
                    //message
                    Message message = new Message(string.Format("{0} is involved in a changing an EVENT", actor.Name), MessageType.Move);
                    Game.world.SetMessage(message);
                }
                else { Game.SetError(new Error(67, "Event not found using EventPID in OutDelay.cs")); }
            }
            else { Game.SetError(new Error(67, "Active Actor not found")); }
        }
    }

}
