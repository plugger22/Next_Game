using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum TriggerCheck { None, Trait, GameVar, RelPlyr, Sex, ActorType, ResourcePlyr, Known, Introduction, Desire, Promise, Disguise, TravelMode} 

    /// <summary>
    /// handles triggers for events and options within events
    /// </summary>
    class Trigger
    {
        public TriggerCheck Check { get; set; } = TriggerCheck.None;
        public int Data { get; set; } //multipurpose data point depending on Condition type
        public int Threshold { get; set; } //activation point
        public bool Compulsory { get; set; } //if multiple triggers involved, any successful trigger will pass condition. If Compulsory true, this trigger MUST be true to pass condition.
        public EventCalc Calc { get; set; } //comparison method

        public Trigger()
        {
            //Console.WriteLine("Blank Trigger initialised");
        }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="data"></param>
        /// <param name="threshold"></param>
        /// <param name="calc"></param>
        /// <param name="mustBeTrue">In case of multiple triggers, condition passes if ANY are true unless there is a Compulusory trigger which also must be true.</param>
        public Trigger(TriggerCheck condition, int data, int threshold, EventCalc calc, bool mustBeTrue = true)
        {
            this.Check = condition;
            this.Data = data;
            this.Threshold = threshold;
            this.Calc = calc;
            Compulsory = mustBeTrue;
            //debug
            Console.WriteLine("{0} Trigger, if {1} is {2} to {3}", Check, Data, Calc, Threshold);
        }
    }
}
