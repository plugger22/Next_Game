using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum PossessionType { None, Secret, Promise, Favour, Introduction, Disguise, Item }
    public enum PossSecretType {Parents, Trait, Wound, Torture, Murder, Loyalty, Glory,Fertility };
    public enum PossItemEffect { None }
    public enum PossItemType { None, Passive, Active, Both} //active items provide benefits, passive items are used as bargaining chips, both is used for method filtering in Actor.cs -> CAREFUL!!!
    //public enum PossSecretRef { Actor, House, GeoCluster, Location, Item }

    public class Possession
    {
        private static int possessionIndex = 1; //provides a unique ID to every possession
        public int PossID { get; set; }
        public int Year { get; set; }
        public PossessionType Type { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; } //some possessions can be owned but inactive, default true

        public Possession()
        { }
            
        public Possession(string description, int year)
        {
            PossID = possessionIndex++;
            this.Year = year;
            this.Description = description;
            this.Active = true;
        }
    }


    // Secrets ---

    public class Secret : Possession
    {
        //private static int secretIndex = 1; //provides a unique ID to every secret
        public PossSecretType SecretType { get; set; }
        public int Strength { get; set; } //how tightly the secret is held by those involved (1 to 5 stars)
        
        

        public Secret()
        { }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="year"></param>
        /// <param name="description"></param>
        /// <param name="strength"></param>
        public Secret(PossSecretType type, string description, int year, int strength) : base (description, year)
        {
            this.SecretType = type;
            this.Strength = strength;
            Strength = Math.Min(5, Strength);
            Strength = Math.Max(1, Strength);
            Type = PossessionType.Secret;
        }

    }

    /// <summary>
    ///Secret related to Actors (active or passive)
    /// </summary>
    public class Secret_Actor : Secret
    {
        private List<int> listOfActors; //who this secret revolves around

        /// <summary>
        /// Main constructor - actorID for who is affected. Add extra via AddActor()
        /// </summary>
        /// <param name="type"></param>
        /// <param name="year"></param>
        /// <param name="description"></param>
        /// <param name="strength"></param>
        /// <param name="actorID"></param>
        public Secret_Actor(PossSecretType type, int year, string description, int strength, int actorID) : base(type, description, year, strength)
        {
            listOfActors = new List<int>();
            listOfActors.Add(actorID);
        }

        public void AddActor(int actorID)
        { listOfActors.Add(actorID); }
    }

    /// <summary>
    /// Secrets related to Houses (Major or Minor)
    /// </summary>
    public class Secret_House : Secret
    {
        private List<int> listOfHouses; //which houses this affects (RefID stored, NOT houseID)

        public Secret_House(PossSecretType type, int year, string description, int strength, int refID) : base(type, description, year, strength)
        {
            listOfHouses = new List<int>();
            listOfHouses.Add(refID);
        }

        public void AddHouse(int refID)
        { listOfHouses.Add(refID); }
    }

    /// <summary>
    /// Secrets related to GeoClusters
    /// </summary>
    public class Secret_GeoCluster : Secret
    {
        private List<int> listOfGeoClusters; //which Geo's this affects

        public Secret_GeoCluster(PossSecretType type, int year, string description, int strength, int geoID) : base(type, description, year, strength)
        {
            listOfGeoClusters = new List<int>();
            listOfGeoClusters.Add(geoID);
        }

        public void AddGeoCluster(int geoID)
        { listOfGeoClusters.Add(geoID); }
    }

    /// <summary>
    /// Secrets related to Locations
    /// </summary>
    public class Secret_Location : Secret
    {
        private List<int> listOfLocations; //which Loc's this effects

        public Secret_Location(PossSecretType type, int year, string description, int strength, int locID) : base(type, description, year, strength)
        {
            listOfLocations = new List<int>();
            listOfLocations.Add(locID);
        }

        public void AddLocation(int locID)
        { listOfLocations.Add(locID); }
    }


    // Favours ---

    public class Favour : Possession
    {
        public int Strength { get; set; } //strength 1 to 5
        public int ActorID { get; set; } //which actor granted the favour

        public Favour( string description, int year, int strength, int actorID) : base(description, year)
        {
            this.Strength = strength;
            Strength = Math.Min(5, Strength);
            Strength = Math.Max(1, Strength);
            if (actorID > 0) { this.ActorID = actorID; } else { Game.SetError(new Error(122, "Invalid ActorID input (zero or less) Favour.cs")); }
            Type = PossessionType.Favour;
        }
    }

    // Introductions ---

    public class Introduction : Possession
    {
        public int Strength { get; set; } //strength 1 to 5
        public int ActorID { get; set; } //which actor granted the favour

        public Introduction(string description, int year, int strength, int actorID) : base(description, year)
        {
            this.Strength = strength;
            Strength = Math.Min(5, Strength);
            Strength = Math.Max(1, Strength);
            if (actorID > 0) { this.ActorID = actorID; } else { Game.SetError(new Error(122, "Invalid ActorID input (zero or less) Introduction.cs")); }
            Type = PossessionType.Favour;
        }
    }

    // Items ---

        /// <summary>
        /// Items -> player or NPC
        /// </summary>
    public class Item : Possession
    {
        public int ItemID { get; set; } //unique item ID (in addition to autoassigned PossID)
        public int ArcID { get; set; } //archetype ID for related events (optional, default '0')
        public string Lore { get; set; } //background description
        public PossItemType ItemType { get; set; } //if true item is Active (provides beneficial effects to the Player), if false item is Passive (something that can be used for 
        //unique effect
        public PossItemEffect Effect { get; set; } //unique effect (unrelated to the Items challenge effect)
        public int Amount { get; set; } //multipurpose value related to unique effect, default 0
        //challenge effect
        public bool ChallengeFlag { get; set; } //if true can be used in challenges
        private List<ConflictSubType> listOfChallenges; //list of all challenge subtypes that the item can be used for. Leave empty for none.
        public int CardNum { get; set; } //number of cards item gives in a challenge
        public string CardText { get; set; } //card text
        private string[] outcomeText; //outcome texts for when card played or ignored


        /// <summary>
        /// default constructor -> year refers to when item was made (could be hundreds of years old)
        /// </summary>
        /// <param name="description"></param>
        /// <param name="year"></param>
        public Item(string description, string lore, int year, PossItemEffect effect, int amount) : base(description, year)
        {
            this.Lore = lore;
            this.Effect = effect;
            this.Amount = amount;
            Type = PossessionType.Item;
            listOfChallenges = new List<ConflictSubType>();
            outcomeText = new string[4]; //[0] -> Plyr Challenger Good (plays card), [1] -> Plyr Challenger Bad (ignores card), [2] -> Plyr Def Good (plays), [3] -> Plyr Def Bad (ignores)
        }

        internal void SetConflictChallenges(List<ConflictSubType> tempList)
        {
            if (tempList != null)
            { listOfChallenges.AddRange(tempList); }
            else { Game.SetError(new Error(201, "Invalid list of ConflictSubTypes Input (null)")); }
        }

        /// <summary>
        /// returns true if item is valid for the input ConflictSubType challenge
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal bool CheckChallengeType(ConflictSubType type)
        {
            if (ChallengeFlag == true)
            {
                for (int i = 0; i < listOfChallenges.Count; i++)
                {
                    if (listOfChallenges[i] == type)
                    { return true; }
                }
            }
            return false;
        }

        internal void SetOutcomeTexts(string[] tempArray)
        {
            if (tempArray.Length == 4)
            {
                for (int i = 0; i < tempArray.Length; i++)
                { outcomeText[i] = tempArray[i]; }
            }
            else { Game.SetError(new Error(201, string.Format("Invalid Outcome Texts Array Length (should be 4, not {0})", tempArray.Length))); }
        }

        internal string[] GetOutcomeTexts()
        { return outcomeText; }
    }


    //new classes above here
}
