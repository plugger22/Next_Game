﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories (can choose multiple)
    public enum HistActorEvent {None, Born, Died, Married, Battle, Lordship, Birthing, Knighthood}
    public enum HistHouseEvent {None, Allegiance}
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
        public bool Actual { get; set; } = true; //Is the record a true representation of actual events or a false one?
        public List<int> listOfActors; //actorID
        public List<int> listOfLocs; //locID
        public List<int> listOfHouses; //refID
        public List<int> listOfItems; //itemID
        public List<HistActorEvent> listOfActorEvents; //categories
        public List<HistHouseEvent> listOfHouseEvents;


        public Record()
        {
            eventID = eventIndex++;
            Year = Game.gameYear;
            //Turn = Game.gameTurn;
            //initialise lists
            listOfActors = new List<int>();
            listOfLocs = new List<int>();
            listOfHouses = new List<int>();
            listOfItems = new List<int>();
            listOfActorEvents = new List<HistActorEvent>();
            listOfHouseEvents = new List<HistHouseEvent>();
        }

        public Record(string description, int actorID, int locID, int refID, int year, HistActorEvent histActorEvent = HistActorEvent.None, HistHouseEvent histHouseEvent = HistHouseEvent.None,
            bool actualEvent = true, int itemID = 0)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                eventID = eventIndex++;
                this.Year = year;
                Turn = Game.gameTurn;
                Actual = actualEvent;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfActorEvents = new List<HistActorEvent>();
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
                if (histActorEvent > 0)
                { listOfActorEvents.Add(histActorEvent); }

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

        public void AddActorEvent(HistActorEvent histEvent)
        { listOfActorEvents.Add(histEvent); }

        public void AddItem(int itemID)
        {
            if (itemID > 0)
            { listOfItems.Add(itemID); }
        }

    }
}
