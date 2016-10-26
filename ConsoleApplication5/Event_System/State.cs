using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum GameVar { None,
        Notoriety, //don't change order -> corresponds to Director DataPoint
        Justice, //don't change order -> corresponds to Director DataPoint
        Legend_Ursurper, //don't change order -> corresponds to Director DataPoint
        Legend_King, //don't change order -> corresponds to Director DataPoint
        Honour_Ursurper, //don't change order -> corresponds to Director DataPoint
        Honour_King, //don't change order -> corresponds to Director DataPoint
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
            int amountNum = Math.Abs(amount); //must be positive 
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
                    case GameVar.Notoriety:
                        oldData = Game.director.GetGameState(DataPoint.Notoriety, state);
                        //apply change
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Notoriety, state, newData);
                        break;
                    case GameVar.Justice:
                        oldData = Game.director.GetGameState(DataPoint.Justice, state);
                        //apply change (positive #)
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Justice, state, newData);
                        break;
                    case GameVar.Legend_Ursurper:
                        oldData = Game.director.GetGameState(DataPoint.Legend_Ursurper, state);
                        //apply change (positive #)
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Legend_Ursurper, state, newData);
                        break;
                    case GameVar.Legend_King:
                        oldData = Game.director.GetGameState(DataPoint.Legend_King, state);
                        //apply change (positive #)
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Legend_King, state, newData);
                        break;
                    case GameVar.Honour_Ursurper:
                        oldData = Game.director.GetGameState(DataPoint.Honour_Ursurper, state);
                        //apply change (positive #)
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Honour_Ursurper, state, newData);
                        break;
                    case GameVar.Honour_King:
                        oldData = Game.director.GetGameState(DataPoint.Honour_King, state);
                        //apply change (positive #)
                        newData = Math.Abs(ChangeData(oldData, amountNum, apply));
                        //update 
                        Game.director.SetGameState(DataPoint.Honour_King, state, newData);
                        break;
                    default:
                        Game.SetError(new Error(74, string.Format("Invalid input (enum \"{0}\") for eventPID {1}", gameVar, eventTxt)));
                        stateChanged = false;
                        break;
                }
                //update Change state if required
                if (stateChanged == true)
                {
                    //specific for Director DataPoint variables (game states)
                    if (gameVar <= GameVar.Honour_King)
                    {
                        //Note: Director GameVar enum needs to be in same numerical order and position as State DataPoint enum
                        DataPoint dataPoint = (DataPoint)outType;
                        if (state == DataState.Good)
                        {
                            //good increase
                            Game.director.SetGameState(dataPoint, DataState.Change, 1);
                        }
                        else
                        {
                            //Bad increase
                            Game.director.SetGameState(dataPoint, DataState.Change, -1);
                        }
                    }
                    //message
                    Message message = new Message(string.Format("Event \"{0}\", Option \"{1}\", {2} \"{3}\" {4} from {5} to {6}", eventTxt, optionTxt, gameVar,
                        state, oldData > newData ? "decreased" : "increased", oldData, newData), 1, 0, MessageType.Event);
                    Game.world.SetMessage(message);
                    //debug
                    Console.WriteLine(string.Format("Event \"{0}\", Option \"{1}\", {2} \"{3}\" {4} from {5} to {6}", eventTxt, optionTxt, gameVar,
                        state, oldData > newData ? "decreased" : "increased", oldData, newData));
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
                case OutApply.Subtract: //NOT for Director.cs DataPoint enums
                    newValue -= amount;
                    break;
                case OutApply.Random:
                    int rndNum = rnd.Next(amount);
                    newValue += rndNum;
                    break;
            }
            return newValue;
        }

        //add new methods above here
    }
}
