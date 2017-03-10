using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories (can choose multiple) -> Used for Historical Records
    public enum HistActorIncident { None, Born, Died, Married, Conflict, Lordship, Birthing, Knighthood, Coronation, Captured, Wounded, Leadership, Heroic_Deed, Service} 
    public enum HistHouseIncident { None, Allegiance, Ownership }
    public enum HistKingdomIncident { None, Battle, Siege }
    //categorires -> Used for Messages & current Records
    public enum MessageType { None, System, Move, Crow, Activation, Event, Conflict, Known, Search }
    public enum CurrentActorIncident { None, Condition, Event, Challenge, Travel, Resource, Known, Search }
    //Relationship List Categories
    public enum RelListType { HousePastGood, HousePastBad, BannerPastGood, BannerPastBad, Lord_1, Lord_2, Lord_3, Lord_4, Lord_5, Count }

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
            if (Year == 0) { Year = Game.gameYear; }
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
    /// Historical & Current Game Records specific to actors, houses and kingdom
    /// </summary>
    public class Record : Tracker
    {
        public bool Actual { get; set; } = true; //Is the record a true representation of actual incidents or a false one?
        public List<HistActorIncident> listOfHistoricalActorIncidents; //categories
        public List<HistHouseIncident> listOfHouseIncidents;
        public List<HistKingdomIncident> listOfKingdomIncidents;
        public List<CurrentActorIncident> listOfCurrentActorIncidents;


        public Record()
        {
            listOfHistoricalActorIncidents = new List<HistActorIncident>();
            listOfHouseIncidents = new List<HistHouseIncident>();
            listOfKingdomIncidents = new List<HistKingdomIncident>();
            listOfCurrentActorIncidents = new List<CurrentActorIncident>();
    }

        /// <summary>
        /// Actor Incident -> Historical
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
                listOfHistoricalActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                listOfCurrentActorIncidents = new List<CurrentActorIncident>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (histActorIncident > 0)
                { listOfHistoricalActorIncidents.Add(histActorIncident); }
                //debug
                Console.WriteLine("ACTOR RECORD: ID {0}, {1}", actorID, description);
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        /// <summary>
        /// Actor Incident -> Current
        /// </summary>
        /// <param name="description"></param>
        /// <param name="actorID"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="year"></param>
        /// <param name="histActorIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int actorID, int locID, int refID, CurrentActorIncident currentActorIncident = CurrentActorIncident.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                Actual = happened;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfHistoricalActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                listOfCurrentActorIncidents = new List<CurrentActorIncident>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (currentActorIncident > 0)
                { listOfCurrentActorIncidents.Add(currentActorIncident); }
                //debug
                Console.WriteLine("ACTOR RECORD: ID {0}, {1}", actorID, description);
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
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
                listOfHistoricalActorIncidents = new List<HistActorIncident>();
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
                //debug
                Console.WriteLine("HOUSE RECORD: refID {0}, {1}", refID, description);
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
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
                listOfHistoricalActorIncidents = new List<HistActorIncident>();
                listOfHouseIncidents = new List<HistHouseIncident>();
                listOfKingdomIncidents = new List<HistKingdomIncident>();
                listOfCurrentActorIncidents = new List<CurrentActorIncident>();
                //lists
                Text = description;
                if (locID > 0)
                { listOfLocs.Add(locID); }
                if (refID > 0)
                { listOfHouses.Add(refID); }
                if (histKingdomIncident > 0)
                { listOfKingdomIncidents.Add(histKingdomIncident); }
                //debug
                Console.WriteLine("KINGDOM RECORD: {0}", description);
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        public void AddActorIncident(HistActorIncident histIncident)
        { listOfHistoricalActorIncidents.Add(histIncident); }
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
                //this.Year = Game.gameYear;
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
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
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
                //this.Year = Game.gameYear;
                this.Type = type;
                Text = description;
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

    }

    /// <summary>
    /// all messages related to relationships. Stored with each actor. Also used to track relationship levels.
    /// </summary>
    public class Relation : Tracker
    {
        public int ActorID { get; set; } //actor to whom this relationship effect applies to (if other's involved place them in the list of Actors
        public int RefID { get; set; } //house to whom this relationship effect applies (used for House -> House relationships)
        public string Tag { get; set; } //short form description (max 4 words)
        public int Change { get; set; } //the effect on a relationship level, eg. +25
        public int Level { get; set; } //current relationship level with that character, AFTER change has been applied

        /// <summary>
        /// for relationships with Player
        /// </summary>
        /// <param name="description"></param>
        /// <param name="tag"></param>
        /// <param name="change"></param>
        public Relation(string description, string tag, int change)
        {
            if (description != null && tag != null)
            {
                //Level is taken care of by Actor.cs AddRelEventPlyr
                Text = description;
                this.Tag = tag;
                this.ActorID = 1; //default Player relationshp for this constructor
                this.Change = change;
            }
            else
            {
                Game.SetError(new Error(119, "Invalid Description, or Tag, (null) in Relation Constructor"));
                if (String.IsNullOrEmpty(description)) { Text = "unknown"; }
                if (String.IsNullOrEmpty(tag)) { Tag = "unknown"; }
            }
        }

        /// <summary>
        /// for relationships with all others (their Lord)
        /// </summary>
        /// <param name="description"></param>
        /// <param name="tag"></param>
        /// <param name="change"></param>
        /// <param name="actorID"></param>
        public Relation(string description, string tag, int change, int actorID)
        {
            if (description != null && tag != null)
            {
                //Level is taken care of by Actor.cs AddRelEventPlyr
                Text = description;
                this.Tag = tag;
                this.ActorID = actorID;
                this.Change = change;
            }
            else
            {
                Game.SetError(new Error(119, "Invalid Description, or Tag, (null) in Relation Constructor"));
                if (String.IsNullOrEmpty(description)) { Text = "unknown"; }
                if (String.IsNullOrEmpty(tag)) { Tag = "unknown"; }
            }
        }


        /// <summary>
        /// returns a text string of combined relation data
        /// </summary>
        /// <returns></returns>
        public string GetRelationText()
        { return string.Format("{0} \"{1}\", Rel {2}, (Change {3}{4})", Year, Text, Level, Change > 0 ? "+" : "", Change); }

    }
}
 