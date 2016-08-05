using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories - each should have multually exclusive events
    public enum ActorEvent {None, Born, Died, Married}
    public enum KingdomEvent {None, Battle}

    //
    //tracks historical Information & Events ---
    //

    class Record
    {
        private static int eventIndex = 1;
        public int eventID { get; }
        public int Year { get; set; }
        public int Turn { get; set; } = 0; //pre-game start events don't need this, hence '0'
        private List<string> listOfText; //A log entry can have multiple lines of text if needed
        private List<int> listOfActors; //actorID
        private List<int> listOfLocs; //locID
        private List<int> listOfHouses; //refID
        private List<int> listOfItems; //itemID
        //categories
        public ActorEvent actorEvent { get; set; } = 0;
        public KingdomEvent kingdomEvent { get; set; } = 0;

        public Record()
        {
            eventID = eventIndex++;
            Year = Game.gameYear;
            Turn = Game.gameTurn;
            listOfText = new List<string>();
            listOfActors = new List<int>();
            listOfLocs = new List<int>();
            listOfHouses = new List<int>();
            listOfItems = new List<int>();
        }

        public Record(string text, int actorID, int locID, int refID, int itemID = 0)
        {
            //it's a valid record only if there is a descriptive text
            if (text != null)
            {
                eventID = eventIndex++;
                Year = Game.gameYear;
                Turn = Game.gameTurn;
                //lists
                listOfText.Add(text);
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (itemID > 0)
                { listOfItems.Add(itemID); }

            }
        }

        public void AddActor(int actorID)
        {
            if (actorID > 0)
            { listOfActors.Add(actorID); }
        }

        public void AddLoc(int locID)
        {
            if (locID > 0)
            { listOfLocs.Add(locID); }
        }

        public void AddHouse(int refID)
        {
            if (refID > 0)
            { listOfHouses.Add(refID); }
        }

        public void AddItem(int itemID)
        {
            if (itemID > 0)
            { listOfItems.Add(itemID); }
        }

    }
}
