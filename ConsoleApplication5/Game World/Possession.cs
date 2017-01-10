using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum PossessionType { None, Secret, Promise, Introduction, Disguise, Item }
    public enum PossSecretType {Parents, Trait, Wound, Torture, Murder, Loyalty, Glory,Fertility };
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
}
