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
        public SkillType Trait { get; set; } //trait type that the option uses as a succeed/fail test (# traits stars x 20 < 1d100 to succeed)

        public OptionAuto()
        { }
        
        public OptionAuto(SkillType traitToTest)
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
        public bool Active { get; set; } //option triggers allow option to fire? (true) Set at time of Option use (Director.cs ResolvePlayerEvents)
        private List<Trigger> listTriggers; //Triggers for option to be active (if any)
        public int ActorID { get; set; } //actor who the option is referring to (ignore if not applicable)

        public OptionInteractive()
        {
            listTriggers = new List<Trigger>();
        }

        public OptionInteractive(string text)
        {
            this.Text = text;
            listTriggers = new List<Trigger>();
        }

        /// <summary>
        /// used for CreateAutoEvents options where the player has choices
        /// </summary>
        /// <param name="text"></param>
        /// <param name="actorID"></param>
        public OptionInteractive(string text, int actorID)
        {
            this.Text = text;
            listTriggers = new List<Trigger>();
            this.ActorID = actorID;
        }

        /// <summary>
        /// Add a list of Triggers (options may or may not have triggers (can be multiple)
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetTriggers(List<Trigger> tempList)
        { listTriggers.AddRange(tempList); }

        internal List<Trigger> GetTriggers()
        { return listTriggers; }
    }

}
