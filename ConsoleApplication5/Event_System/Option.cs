using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

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
        private List<Trigger> listTriggers; //Triggers for option to be active (if any)
        

        public Option()
        {
            OptionID = optionIndex++;
            listGoodOutcomes = new List<Outcome>();
            listBadOutcomes = new List<Outcome>();
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

    }



    /// <summary>
    /// Automatic options, eg. Follower event
    /// </summary>
    public class OptionAuto : Option
    {
        public TraitType Trait { get; set; } //trait type that the option uses as a succeed/fail test (# traits stars x 20 < 1d100 to succeed)

        public OptionAuto()
        { }
        
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

        public OptionInteractive()
        { }

        public OptionInteractive(string text)
        {
            this.Text = text;
        }
    }
}
