﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    public enum TriggerCheck { None, Trait, GameVar}

    /// <summary>
    /// handles triggers for events and options within events
    /// </summary>
    class Trigger
    {
        public TriggerCheck Check { get; set; }
        public int Data { get; set; } //multipurpose data point depending on Condition type
        public int Threshold { get; set; } //activation point
        public EventCalc Calc { get; set; } //comparison method

        public Trigger()
        { }

        public Trigger(TriggerCheck condition, int data, int threshold, EventCalc calc)
        {
            this.Check = condition;
            this.Data = data;
            this.Threshold = threshold;
            this.Calc = calc;
        }
    }
}
