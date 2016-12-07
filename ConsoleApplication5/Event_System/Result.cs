using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum ResultType { None, GameVar, Relationship, Condition, Resource, Item, Secret, Army, Event, Count} //relationship change with actor, condition affect on actor, event triggered
    //NOTE: Conflict.cs ResolveResults uses the above for a switch statement that needs to be tweaked if the above ResultType enum is changed or added to.

    /// <summary>
    /// Conflict Results (Events have outcomes). Single class, access different results via enum ResultType
    /// </summary>
    class Result
    {
        private static int resultIndex = 1; //autoassigned ID's. Main focus is the Result Class
        public string Description { get; set; }
        public int ResultID { get; set; }
        public ResultType Type { get; set; }
        public int Data { get; set; }
        public EventCalc Calc { get; set; }
        public int Amount { get; set; }

        public Result()
        { }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="calc"></param>
        /// <param name="amount"></param>
        public Result(string description, ResultType type, int data, EventCalc calc, int amount)
        {
            ResultID = resultIndex++;
            this.Description = description;
            this.Type = type;
            this.Data = data;
            this.Calc = calc;
            this.Amount = amount;
        }
    }
}
