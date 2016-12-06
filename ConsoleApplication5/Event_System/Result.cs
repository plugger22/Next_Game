using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum ResultType { None, GameVar, Actor, Resource, Item, Secret, Army}

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
        {
            ResultID = resultIndex++;
        }
    }
}
