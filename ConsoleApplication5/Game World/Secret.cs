using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum SecretType {Parents, Trait, Wound, Torture, Murder };
    public enum SecretRef { Actor, House, GeoCluster, Location, Item }

    public class Secret
    {
        private static int secretIndex = 1; //provides a unique ID to every secret
        public SecretType Type { get; set; }
        public int SecretID { get; set; }
        public int Year { get; set; }
        public int Strength { get; set; } //how tightly the secret is held by those involved (1 to 5 stars)
        public string Description { get; set; }
        public bool Active { get; set; } //only a viable secret if active = true

        public Secret()
        { }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="year"></param>
        /// <param name="description"></param>
        /// <param name="strength"></param>
        public Secret(SecretType type, int year, string description, int strength)
        {
            SecretID = secretIndex++;
            this.Type = type;
            this.Year = year;
            this.Description = description;
            this.Strength = strength;
            this.Active = true;
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
        public Secret_Actor(SecretType type, int year, string description, int strength, int actorID) : base(type, year, description, strength)
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

        public Secret_House(SecretType type, int year, string description, int strength, int refID) : base(type, year, description, strength)
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

        public Secret_GeoCluster(SecretType type, int year, string description, int strength, int geoID) : base(type, year, description, strength)
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

        public Secret_Location(SecretType type, int year, string description, int strength, int locID) : base(type, year, description, strength)
        {
            listOfLocations = new List<int>();
            listOfLocations.Add(locID);
        }

        public void AddLocation(int locID)
        { listOfLocations.Add(locID); }
    }
}
