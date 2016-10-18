using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum OptionCheck {None, Trait, Resources, Companion, Item } //special conditions that must be met for the option to trigger (all resolve down to an 'int' value in the dict)

    /// <summary>
    /// Event option
    /// </summary>
    public class Option
    {
        private static int optionIndex = 1; //autoassigned ID's.
        public int OptionID { get;}
        
        public string ReplyGood { get; set; } //text for good outcome or if only a single outcome result possible (eg. interactive)
        public string ReplyBad { get; set; } //text for bad outcome
        public string Tooltip { get; set; }
        private List<Outcome> listGoodOutcomes; //All possible outcomes if success (auto) or if selected (eg. interactive)
        private List<Outcome> listBadOutcomes; //All possible outcomes if fail
        private Dictionary<int, OptionCheck> dictOfTriggers; //trigger conditions, if any, that must be met for the option to be active
        

        public Option()
        {
            OptionID = optionIndex++;
            listGoodOutcomes = new List<Outcome>();
            listBadOutcomes = new List<Outcome>();
            dictOfTriggers = new Dictionary<int, OptionCheck>();
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
        internal void AddTrigger(int data, OptionCheck condition)
        {
            if (data > 0 && condition != OptionCheck.None)
            {
                //validate data input
                switch (condition)
                {
                    case OptionCheck.Trait:

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
        public TraitType Trait { get; set; } //trait type that the option uses as a succeed/fail test (# traits stars x 20 < 1d100 to succeed)
        
        public OptionAuto(TraitType traitToTest)
        {
            Trait = traitToTest;
        }
    }




    /// <summary>
    /// Interactive options, eg. Player event
    /// </summary>
    public class OptionInteractive : Option
    {
        public string Text { get; set; } //option text

        public OptionInteractive(string text)
        {
            this.Text = text;
        }
    }
}
