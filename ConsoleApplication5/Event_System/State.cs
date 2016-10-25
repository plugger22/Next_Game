using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum GameVar { None, Threat, Count} //use descriptive tags as variable names are referenced via methods

    /// <summary>
    /// Enables Player events to change game state variables (not constants). Only accessed through Director.cs
    /// </summary>
    public class State
    {

        public State()
        { }

        /// <summary>
        /// adjusts a state
        /// </summary>
        /// <param name="data">GameVar enum index</param>
        /// <param name="amount">how much</param>
        /// <param name="apply">how to apply it</param>
        public void SetState(string eventTxt, string optionTxt, int data, int amount, OutApply apply)
        {
            GameVar gameVar;
            bool stateChanged = false;
            //convert to a GameVar enum
            if ((int)GameVar.Count <= data)
            {
                gameVar = (GameVar)data;
                stateChanged = true;
                OptionInteractive option = new OptionInteractive();
                switch (gameVar)
                {
                    case GameVar.Threat:

                        break;

                    default:
                        Game.SetError(new Error(74, string.Format("Invalid input (enum \"{0}\") for eventPID {1}", gameVar, eventTxt)));
                        stateChanged = false;
                        break;
                }
                //tracker
                if (stateChanged == true)
                {
                    
                    //message

                    //debug
                    Console.WriteLine(string.Format(""));
                }
            }
            else { Game.SetError(new Error(74, string.Format("Invalid input (data \"{0}\") for eventPID {1}", data, eventTxt))); }
        }

    }
}
