using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    //categories (can choose multiple) -> Used for Historical Records
    public enum HistActorEvent { None, Born, Died, Married, Conflict, Lordship, Birthing, Knighthood, Coronation, Captured, Wounded, Leadership, Heroic_Deed, Service} 
    public enum HistHouseEvent { None, Allegiance, Ownership, Food, Goods, Economy, Population }
    public enum HistKingdomEvent { None, Battle, Siege }
    //categorires -> Used for Messages & current Records
    public enum MessageType { None, System, God, Error, Move, Crow, Activation, Event, Conflict, Known, Search, Incarceration, Horse }
    public enum CurrentActorEvent { None, Condition, Event, Challenge, Travel, Resource, Known, Search, Horse }
    //Relationship List Categories
    public enum RelListType { HousePastGood, HousePastBad, BannerPastGood, BannerPastBad, Lord_1, Lord_2, Lord_3, Lord_4, Lord_5, Count }


    /// <summary>
    /// used to keep records of all horse losses
    /// </summary>
    public struct HorseRecord
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string LocText { get; set; } //'on the road to ...', 'at ...', etc.
        public int Days { get; set;  } // number of days the horse was owned by the player
        public int Health { get; set; } // original horse health (eg. HorseHealthMax)
        public int Turn { get; set; }
        public HorseGone Gone { get; set; } // reason gone
    }


    /// <summary>
    /// Base class (not used directly)
    /// </summary>
    public class Tracker
    {
        private static int trackerIndex = 1;
        public int TrackerID { get; }
        public int Year { get; set; }
        public int Day { get; set; } = 0; //pre-game start incidents don't need this, hence '0'
        public string Text { get; set; } //descriptor

        public List<int> listOfActors; //actorID
        public List<int> listOfLocs; //locID
        public List<int> listOfHouses; //refID
        public List<int> listOfItems; //itemID



        public Tracker()
        {
            TrackerID = trackerIndex++;
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
        public List<HistActorEvent> listOfHistoricalActorIncidents; //categories
        public List<HistHouseEvent> listOfHouseIncidents;
        public List<HistKingdomEvent> listOfKingdomIncidents;
        public List<CurrentActorEvent> listOfCurrentActorIncidents;


        public Record()
        {
            listOfHistoricalActorIncidents = new List<HistActorEvent>();
            listOfHouseIncidents = new List<HistHouseEvent>();
            listOfKingdomIncidents = new List<HistKingdomEvent>();
            listOfCurrentActorIncidents = new List<CurrentActorEvent>();
    }

        /// <summary>
        /// Actor Incident -> Historical
        /// </summary>
        /// <param name="description"></param>
        /// <param name="actorID"></param>
        /// <param name="locID"></param>
        /// <param name="year"></param>
        /// <param name="histActorIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int actorID, int locID, int refID = 0, int year = 0, HistActorEvent histActorIncident = HistActorEvent.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                if (year == 0) { year = Game.gameYear; } else { this.Year = year;}
                Actual = happened;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfHistoricalActorIncidents = new List<HistActorEvent>();
                listOfHouseIncidents = new List<HistHouseEvent>();
                listOfKingdomIncidents = new List<HistKingdomEvent>();
                listOfCurrentActorIncidents = new List<CurrentActorEvent>();
                //lists
                Text = description;
                if (actorID > 0) { listOfActors.Add(actorID); }
                if (locID > 0) { listOfLocs.Add(locID); }
                if (refID > 0) { listOfHouses.Add(refID); }
                if (histActorIncident > 0)
                { listOfHistoricalActorIncidents.Add(histActorIncident); }
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        /// <summary>
        /// Actor Incident -> Current
        /// </summary>
        /// <param name="description"></param>
        /// <param name="actorID"></param>
        /// <param name="locID"></param>
        /// <param name="year"></param>
        /// <param name="histActorIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int actorID, int locID, CurrentActorEvent currentActorIncident = CurrentActorEvent.None, bool happened = true)
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
                listOfHistoricalActorIncidents = new List<HistActorEvent>();
                listOfHouseIncidents = new List<HistHouseEvent>();
                listOfKingdomIncidents = new List<HistKingdomEvent>();
                listOfCurrentActorIncidents = new List<CurrentActorEvent>();
                //lists
                Text = description;
                if (actorID > 0)
                { listOfActors.Add(actorID); }
                if (locID > 0)
                {
                    listOfLocs.Add(locID);
                    listOfHouses.Add(Game.world.ConvertLocToRef(locID));
                }
                if (currentActorIncident > 0)
                { listOfCurrentActorIncidents.Add(currentActorIncident); }
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        /// <summary>
        /// House Incident
        /// </summary>
        /// <param name="description"></param>
        /// <param name="locID"></param>
        /// <param name="refID">will auto convert from LocID if left at 0 but don't do this for pregame stuff that involves calling world.cs</param>
        /// <param name="year"></param>
        /// <param name="histHouseIncident"></param>
        /// <param name="happened"></param>
        public Record(string description, int locID, int refID = 0, int year = 0, HistHouseEvent histHouseIncident = HistHouseEvent.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                if (year == 0) { year = Game.gameYear; } else { this.Year = year; }
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfHistoricalActorIncidents = new List<HistActorEvent>();
                listOfHouseIncidents = new List<HistHouseEvent>();
                listOfKingdomIncidents = new List<HistKingdomEvent>();
                //lists
                Text = description;
                if (locID > 0) { listOfLocs.Add(locID); }
                if (refID == 0) { refID = Game.world.ConvertLocToRef(locID); }
                if (refID > 0) { listOfHouses.Add(refID); }
                if (histHouseIncident > 0)
                { listOfHouseIncidents.Add(histHouseIncident); }
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        /// <summary>
        /// Kingdom Incident
        /// </summary>
        /// <param name="description"></param>
        /// <param name="locID"></param>
        /// <param name="year"></param>
        /// <param name="happened"></param>
        /// <param name="happened"></param>
        public Record(string description, int locID, int refID = 0, int year = 0, HistKingdomEvent histKingdomIncident = HistKingdomEvent.None, bool happened = true)
        {
            //it's a valid record only if there is a descriptive text
            if (description != null)
            {
                if (year == 0) { year = Game.gameYear; } else { this.Year = year; }
                Actual = happened;
                //initialise lists
                listOfActors = new List<int>();
                listOfLocs = new List<int>();
                listOfHouses = new List<int>();
                listOfItems = new List<int>();
                listOfHistoricalActorIncidents = new List<HistActorEvent>();
                listOfHouseIncidents = new List<HistHouseEvent>();
                listOfKingdomIncidents = new List<HistKingdomEvent>();
                listOfCurrentActorIncidents = new List<CurrentActorEvent>();
                //lists
                Text = description;
                if (locID > 0) { listOfLocs.Add(locID); }
                if (refID > 0) { listOfHouses.Add(refID); }
                if (histKingdomIncident > 0)
                { listOfKingdomIncidents.Add(histKingdomIncident); }
            }
            else { Game.SetError(new Error(119, "Invalid Description (null) in Relation Constructor")); }
        }

        public void AddActorIncident(HistActorEvent histIncident)
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
        public string Rumour { get; set; } //text descriptor used for rumour (reason) in the format "due to... <Rumour>"
        public int Change { get; set; } //the effect on a relationship level, eg. +25
        public int Level { get; set; } //current relationship level with that character, AFTER change has been applied (handled automatically by code based on change, NO NEED to specify directly)
        public bool Known { get; set; } //allows relations to be 'Knovwn', eg. visible to the Player, or not. Default true.
        public RelListType Type { get; set; } //type of relationship


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
                Known = true;
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
 