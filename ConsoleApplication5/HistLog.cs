using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum ActorEvent {Born, Died, Married}

    //
    //tracks historical Information & Events ---
    //

    //Base class
    class HistLog
    {
        private static int eventIndex = 1;
        public int eventID { get; }
        public int Year { get; set; }
        public int Turn { get; set; } = 0; //pre-game start events don't need this, hence '0'
        private List<string> description; //A log entry can have multiple lines of text if needed

        public HistLog()
        {
            description = new List<string>();
            eventID = eventIndex++;
        }
    }

    //Actor event log
    class HLogActor : HistLog
    {
        public ActorEvent Type { get; set; }
        private List<int> listOfActors;
        private List<int> listOfLocs;
        private List<int> listOfHouses;
        private List<int> listOfItems;
        
        public HLogActor()
        {
            listOfActors = new List<int>();
            listOfLocs = new List<int>();
            listOfHouses = new List<int>();
            listOfItems = new List<int>();
        } 
    }
}
