using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum OpCondition {None, Trait, Resources, Companion, Item } //special conditions that must be met for the option to trigger (all resolve down to an 'int' value in the dict)

    /// <summary>
    /// Event option
    /// </summary>
    public class Option
    {
        public int OptionID { get; set; }
        
        public string ReplyGood { get; set; } //text for good outcome
        public string ReplyBad { get; set; } //text for bad outcome
        private List<Outcome> listGoodOutcomes; //All possible outcomes if success or chosen
        private List<Outcome> listBadOutcomes; //All possible outcomes if fail or ignored
        private Dictionary<int, OpCondition> dictOfTriggers; //trigger conditions, if any, that must be met for the option to be active
        

        public Option()
        {
            listGoodOutcomes = new List<Outcome>();
            listBadOutcomes = new List<Outcome>();
            dictOfTriggers = new Dictionary<int, OpCondition>();
        }

        /// <summary>
        /// add good outcome
        /// </summary>
        /// <param name="effect"></param>
        internal void SetGoodOutcome(Outcome effect)
        {
            if (effect != null)
            { listGoodOutcomes.Add(effect); }
            else { Game.SetError(new Error(68, "Invalid input (Good Outcome) in Option.cs")); }
        }

        /// <summary>
        /// add bad outcome
        /// </summary>
        /// <param name="effect"></param>
        internal void SetBadOutcome(Outcome effect)
        {
            if (effect != null)
            { listBadOutcomes.Add(effect); }
            else { Game.SetError(new Error(68, "Invalid input (Bad Outcome) in Option.cs")); }
        }

        internal List<Outcome> GetGoodOutcomes()
        { return listGoodOutcomes; }

        internal List<Outcome> GetBadOutcomes()
        { return listBadOutcomes; }

        /// <summary>
        /// set up a trigger for the option (won't show unless condition met)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        internal void AddTrigger(int data, OpCondition condition)
        {
            if (data > 0 && condition != OpCondition.None)
            {
                //validate data input
                switch (condition)
                {
                    case OpCondition.Trait:

                        break;
                }
            }
            else { Game.SetError(new Error(68, "Invalid input (Trigger) in Option.cs")); }
        }

    }

    /// <summary>
    /// Automatic options, eg. Follower event
    /// </summary>
    public class OptionAuto : Option
    {
        public TraitType Trait { get; set; } //trait type that the option uses as a succeed/fail test
        public string Text { get; set; } //main text
        public string Tooltip { get; set; }
    }


    /// <summary>
    /// Interactive options, eg. Player event
    /// </summary>
    public class OptionInter : Option
    { }
}
