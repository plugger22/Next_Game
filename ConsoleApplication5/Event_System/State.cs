using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum GameVar { None,
        Threat, //don't change order
        Justice, //don't change order
        Legend_Ursurper, //don't change order
        Legend_King, //don't change order
        Honour_Ursurper, //don't change order
        Honour_King, //don't change order
        Count } //use descriptive tags as variable names are referenced via methods

    /// <summary>
    /// Enables Player events to change game state variables (not constants). Only accessed through Director.cs
    /// </summary>
    public class State
    {
        static Random rnd;

        public State(int seed)
        { rnd = new Random(seed); }

        /// <summary>
        /// adjusts a state
        /// </summary>
        /// <param name="outType">GameVar enum index. If positive then DataState.Good, if negative then DataState.Bad</param>
        /// <param name="amount">how much</param>
        /// <param name="apply">how to apply it</param>
        public void SetState(string eventTxt, string optionTxt, int outType, int amount, OutApply apply)
        {
            GameVar gameVar;
            bool stateChanged = false;
            DataState state = DataState.Good;
            if (outType < 0) { state = DataState.Bad; outType *= -1; }
            int newData = 0;
            int oldData = 0;
            //convert to a GameVar enum
            if (outType <= (int)GameVar.Count)
            {
                gameVar = (GameVar)outType;
                stateChanged = true;
                OptionInteractive option = new OptionInteractive();
                switch (gameVar)
                {
                    case GameVar.Threat:
                        oldData = Game.director.GetGameState(DataPoint.Threat, state);
                        //apply change
                        newData = ChangeData(oldData, amount, apply);
                        //update 
                        Game.director.SetGameState(DataPoint.Threat, state, newData);
                        break;
                    case GameVar.Justice:
                        oldData = Game.director.GetGameState(DataPoint.Justice, state);
                        //apply change
                        newData = ChangeData(oldData, amount, apply);
                        //update 
                        Game.director.SetGameState(DataPoint.Justice, state, newData);
                        break;
                    case GameVar.Honour_Ursurper:
                        oldData = Game.director.GetGameState(DataPoint.Honour_Ursurper, state);
                        //apply change
                        newData = ChangeData(oldData, amount, apply);
                        //update 
                        Game.director.SetGameState(DataPoint.Honour_Ursurper, state, newData);
                        break;
                    case GameVar.Honour_King:
                        oldData = Game.director.GetGameState(DataPoint.Honour_King, state);
                        //apply change
                        newData = ChangeData(oldData, amount, apply);
                        //update 
                        Game.director.SetGameState(DataPoint.Honour_King, state, newData);
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
                    Message message = new Message(string.Format("Event \"{0}\", Option \"{1}\", {2} level {3} from {4} to {5}", eventTxt, optionTxt, gameVar, 
                        oldData > newData ? "decreased" : "increased" , oldData, newData), 1, 0, MessageType.Event);
                    Game.world.SetMessage(message);
                    //debug
                    Console.WriteLine(string.Format("Event \"{0}\", Option \"{1}\", {2} level {3} from {4} to {5}", eventTxt, optionTxt, gameVar,
                        oldData > newData ? "decreased" : "increased", oldData, newData));
                }
            }
            else { Game.SetError(new Error(74, string.Format("Invalid input (data \"{0}\") for eventPID {1}", outType, eventTxt))); }
        }

        /// <summary>
        /// implements actual changes
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="amount"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        private int ChangeData(int currentValue, int amount, OutApply apply)
        {
            int newValue = currentValue;
            switch(apply)
            {
                case OutApply.Add:
                    newValue += amount;
                    break;
                case OutApply.Subtract:
                    newValue -= amount;
                    break;
                case OutApply.Random:
                    bool addRandom = true;
                    //negative amount indicates SUBTRACT
                    if (amount < 0)
                    { addRandom = false; amount *= -1; }
                    int rndNum = rnd.Next(amount);
                    if (addRandom == true) { newValue += rndNum; }
                    else { newValue -= rndNum; }
                    break;
            }
            return newValue;
        }

        //add new methods above here
    }
}
