using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories (can choose multiple)
    public enum HistEvent {None, Born, Died, Married, Battle}

    //
    //tracks historical Information & Events ---
    //

    class Record
    {
        private static int eventIndex = 1;
        public int eventID { get; }
        public int Year { get; set; }
        public int Turn { get; set; } = 0; //pre-game start events don't need this, hence '0'
        public string Text { get; set; } //descriptor
        public List<int> listOfActors; //actorID
        public List<int> listOfLocs; //locID
        public List<int> listOfHouses; //refID
        public List<int> listOfItems; //itemID
        public List<HistEvent> listOfEvents; //categories

        public Record()
        {
            eventID = eventIndex++;
            Year = Game.gameYear;
            Turn = Game.gameTurn;
            //initialise lists
            listOfActors = new List<int>();
            listOfLocs = new List<int>();
            listOfHouses = new List<int>();
            listOfItems = new List<int>();
            listOfEvents = new List<HistEvent>();
        }

        public Record(string description, int actorID, int locID, int refID, int year, HistEvent histEvent = 0, int itemID = 0)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                eventID = eventIndex++;
                this.Year = year;
                Turn = Game.gameTurn;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfEvents = new List<HistEvent>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (itemID > 0)
                { listOfItems.Add(itemID); }
                if (histEvent > 0)
                { listOfEvents.Add(histEvent); }

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
