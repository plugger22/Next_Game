using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum OutcomeType { None, Delay, Conflict, Game, Known, EventTimer, EventStatus, EventChain, Resource, Condition, Freedom, Item, Passage };

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
        public OutcomeType Type { get; set; }
        

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
            Type = OutcomeType.Delay;
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
                    int refID = Game.world.GetRefID(actor.LocID);
                    //message
                    string messageText = string.Format("{0} has been {1} (\"{2}\") for {3} {4}", actor.Name, eventObject.Type == ArcType.Location ? "indisposed" : "delayed",
                        actor.DelayReason, Delay, Delay == 1 ? "Day" : "Day's");
                    Message message = new Message(messageText, MessageType.Move);
                    Game.world.SetMessage(message);
                    Game.world.SetCurrentRecord(new Record(messageText, actor.ActID, actor.LocID, refID, CurrentActorIncident.Event));
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
            Type = OutcomeType.None;
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
        public Challenge challenge; //used for adding data that overides the default challenge settings

        public OutConflict(int eventID, int opponentID, ConflictType type, bool challenger = true) : base (eventID)
        {
            this.Data = opponentID;
            Conflict_Type = type;
            this.Challenger = challenger;
            Type = OutcomeType.Conflict;
            challenge = new Challenge(ConflictType.Special, ConflictCombat.None, ConflictSocial.None, ConflictStealth.None); //special mode Challenge (purely for data overides)
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
            Type = OutcomeType.Game;
        }
    }

    /// <summary>
    /// Change Known Status (Known/Unknown). Data +ve then UnKnown, Data -ve then known
    /// </summary>
    class OutKnown : Outcome
    {
        public OutKnown(int eventID, int known) : base(eventID)
        {
            Data = known;
            Type = OutcomeType.Known;
        }
    }

    /// <summary>
    /// if Data > 0, player if free'd (ActorStatus.AtLocation), if Data < 0, player is Captured (ActorStatus.Captured) NOTE: Only applies to Player
    /// </summary>
    class OutFreedom : Outcome
    {
        public OutFreedom(int eventID, int free) : base(eventID)
        {
            Data = free;
            Type = OutcomeType.Freedom;
        }
    }

    /// <summary>
    /// Gain or lose an item
    /// </summary>
    class OutItem : Outcome
    {
        //Calc -> Add to gain, Subtract to lose
        //Data -> select from: +ve Active items only, -ve Passive items only, '0' all items

        public OutItem(int eventID, int itemType, EventCalc calc) : base(eventID)
        {
            Data = itemType;
            this.Calc = calc;
            Type = OutcomeType.Item;
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
            Type = OutcomeType.EventTimer;
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
            Type = OutcomeType.EventStatus;
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
            Type = OutcomeType.EventChain;
        }
    }

    /// <summary>
    /// Change resource level of an actor
    /// </summary>
    class OutResource : Outcome
    {
        public bool PlayerRes { get; set; } //if true adjust Player resource level, otherwise option actorID

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="playerRes">If true then it's the Player affected, otherwise opponent</param>
        /// <param name="amount">Remember resources remain within a 1 to 5 range</param>
        /// <param name="apply">Allowable options are Add / Subtract / Equals</param>
        public OutResource(int eventID, bool playerRes, int amount, EventCalc apply) : base(eventID)
        {
            this.Amount = amount;
            this.Calc = apply;
            this.PlayerRes = playerRes;
            Type = OutcomeType.Resource;
        }
    }

    /// <summary>
    /// Player commences a sea voyage
    /// </summary>
    class OutPassage : Outcome
    {
        //Data is Estimated voyage time (turns)
        public int DestinationID { get; set; }
        public bool VoyageSafe { get; set; } //flag if true, a safe ship, if false, a risky one

        public OutPassage(int eventID, int locID, int voyageTime, bool safePassage = true) : base(eventID)
        {
            DestinationID = locID;
            Data = voyageTime;
            VoyageSafe = safePassage;
            Type = OutcomeType.Passage;
        }

    }

    /// <summary>
    /// Applies a condition to an actor
    /// </summary>
    class OutCondition : Outcome
    {
        public bool PlayerCondition { get; set; } //if true condition applies to Player, otherwise opponent
        public Condition NewCondition;

        public OutCondition(int eventID, bool playerCondition, Condition condition) : base(eventID)
        {
            this.PlayerCondition = playerCondition;
            if (condition != null)
            {
                NewCondition = new Condition(condition.Skill, condition.Effect, condition.Text, condition.Timer);
                /*NewCondition.Text = condition.Text;
                NewCondition.Skill = condition.Skill;
                NewCondition.Effect = condition.Effect;
                NewCondition.Timer = condition.Timer;*/
            }
            else { Game.SetError(new Error(130, "Invalid Condition input (null)")); }
            Type = OutcomeType.Condition;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="outcome"></param>
        public OutCondition(OutCondition condition) : base(condition.EventID)
        {
            PlayerCondition = condition.PlayerCondition;
            NewCondition = new Condition(condition.NewCondition.Skill, condition.NewCondition.Effect, condition.NewCondition.Text, condition.NewCondition.Timer);
            /*NewCondition.Text = condition.NewCondition.Text;
            NewCondition.Skill = condition.NewCondition.Skill;
            NewCondition.Effect = condition.NewCondition.Effect;
            NewCondition.Timer = condition.NewCondition.Timer;*/
        }
    }

}
