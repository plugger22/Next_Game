using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories (can choose multiple) -> Used for Records
    public enum HistActorIncident {None, Born, Died, Married, Conflict, Lordship, Birthing, Knighthood, Coronation, Captured, Wounded, Leadership, Heroic_Deed, Service} //conflict -> actor involved in a battle/siege
    public enum HistHouseIncident {None, Allegiance, Ownership}
    public enum HistKingdomIncident { None, Battle, Siege}

    //categorires -> Used for Messages
    public enum MessageType {None, System, Move, Crow, Activation, Event}

   /// <summary>
   /// Base class (not used directly)
   /// </summary>
    public class Tracker
    {
        private static int trackerIndex = 1;
        public int trackerID { get; }
        public int Year { get; set; }
        public int Day { get; set; } = 0; //pre-game start incidents don't need this, hence '0'
        public string Text { get; set; } //descriptor
        
        public List<int> listOfActors; //actorID
        public List<int> listOfLocs; //locID
        public List<int> listOfHouses; //refID
        public List<int> listOfItems; //itemID
        


        public Tracker()
        {
            trackerID = trackerIndex++;
            //Year = Game.gameYear;
            Day = Game.gameTurn + 1;
            //initialise lists
            listOfActors = new List<int>();
            listOfLocs = new List<int>();
            listOfHouses = new List<int>();
            listOfItems = new List<int>();
            
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

    
    /// <summary>
    /// Historical Game Records
    /// </summary>
    public class Record : Tracker
    {
        public bool Actual { get; set; } = true; //Is the record a true representation of actual incidents or a false one?
        public List<HistActorIncident> listOfActorIncidents; //categories
        public List<HistHouseIncident> listOfHouseIncidents;
        public List<HistKingdomIncident> listOfKingdomIncidents;


        public Record()
        {
            listOfActorIncidents = new List<HistActorIncident>();
            listOfHouseIncidents = new List<HistHouseIncident>();
            listOfKingdomIncidents = new List<HistKingdomIncident>();
        }

        /// <summary>
        /// Actor Incident
        /// </summary>
        /// <param name="description"></param>
        /// <param name="actorID"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="year"></param>
        /// <param name="histActorIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int actorID, int locID, int refID, int year, HistActorIncident histActorIncident = HistActorIncident.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                this.Year = year;
                Actual = happened;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (histActorIncident > 0)
                { listOfActorIncidents.Add(histActorIncident); }
            }

        }

        /// <summary>
        /// House Incident
        /// </summary>
        /// <param name="description"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="year"></param>
        /// <param name="histHouseIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int locID, int refID, int year, HistHouseIncident histHouseIncident = HistHouseIncident.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                this.Year = year;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                //lists
                Text = description;
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (histHouseIncident > 0)
                { listOfHouseIncidents.Add(histHouseIncident); }

            }
        }

        /// <summary>
        /// Kingdom Incident
        /// </summary>
        /// <param name="description"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="year"></param>
        /// <param name="happened"></param>
        /// <param name="happened"></param>
        public Record(string description, int locID, int refID, int year, HistKingdomIncident histKingdomIncident = HistKingdomIncident.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                this.Year = year;
                Actual = happened;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                //lists
                Text = description;
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (histKingdomIncident > 0)
                { listOfKingdomIncidents.Add(histKingdomIncident); }
            }
        }

        public void AddActorIncident(HistActorIncident histIncident)
        { listOfActorIncidents.Add(histIncident); }
    }


    /// <summary>
    /// Handles all messages from Player to Game and vice versa
    /// </summary>
    public class Message : Tracker
    {
        public MessageType Type { get; set; }

        /// <summary>
        /// Active player Message
        /// </summary>
        /// <param name="description"></param>
        /// <param name="actorID"></param>
        /// <param name="locID"></param>
        /// <param name="type"></param>
        public Message(string description, int actorID, int locID, MessageType type = MessageType.None)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                this.Year = Game.gameYear;
                this.Type = type;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
            }
        }

        /// <summary>
        /// Simple message, eg. System type
        /// </summary>
        /// <param name="description"></param>
        /// <param name="type"></param>
        public Message(string description, MessageType type)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                this.Year = Game.gameYear;
                this.Type = type;
                Text = description;
            }
        }

    }
}
 