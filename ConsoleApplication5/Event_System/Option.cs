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
        public string Text { get; set; } //main text
        public string Tooltip { get; set; } 
        private List<string> listOfReplies; //texts for outcomes 0, 1 etc.
        private List<Outcome> listGoodOutcomes; //All possible outcomes if success or chosen
        private List<Outcome> listBadOutcomes; //All possible outcomes if fail or ignored
        private Dictionary<int, OpCondition> dictOfTriggers; //trigger conditions, if any, that must be met for the option to be active
        

        public Option()
        {
            listOfReplies = new List<string>();
            listGoodOutcomes = new List<Outcome>();
            listBadOutcomes = new List<Outcome>();
            dictOfTriggers = new Dictionary<int, OpCondition>();
        }

        
    }

    /// <summary>
    /// Automatic options, eg. Follower event
    /// </summary>
    public class OptionAuto : Option
    {
        public TraitType TraitCondition { get; set; } //trait type that the option uses as a succeed/fail test

    }


    /// <summary>
    /// Interactive options, eg. Player event
    /// </summary>
    public class OptionInter : Option
    { }
}
