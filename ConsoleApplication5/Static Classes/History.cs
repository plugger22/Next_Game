using System;
using System.Collections.Generic;
using System.IO;
using Next_Game.Cartographic;
using System.Linq;

namespace Next_Game
{

    //history class handles living world procedural generation at game start. Once created, data is passed to World for game use.
    //Location data flow: create in Map => Network to generate routes => History to generate names and data => World for current state and future changes
    public class History
    {
        //actor names
        private List<Active> listOfPlayerActors;
        private List<string> listOfPlayerNames;
        private List<string> listOfMaleFirstNames;
        private List<string> listOfFemaleFirstNames;
        private List<string> listOfSurnames;
        //house names
        private List<MajorHouse> listOfGreatHouses;
        private List<House> listOfMinorHouses;
        private List<House> listOfSpecialHouses;
        private List<HouseStruct> listHousePool; //used for text file imports and random choice of houses
        private List<HouseStruct> listSpecialHousePool; //used for special house imports and random choice
        //geo names & followers
        private List<GeoCluster> listOfGeoClusters;
        private List<Active> listOfActiveActors;
        private string[][] arrayOfGeoNames;
        //traits
        private List<Skill> listOfTraits; //main
        private Skill[,][] arrayOfTraits; //filtered sets for fast random access
        //secrets
        private List<Secret> listOfSecrets;
        //Mishaps
        private List<string> listOfWounds;
        private List<string> listOfInjuries;
        private List<string> listOfDiseases;
        static Random rnd;

        /// <summary>
        ///default constructor
        /// </summary>
        /// <param name="seed"></param>
        public History(int seed)
        {
            rnd = new Random(seed);
            listOfPlayerActors = new List<Active>();
            listOfPlayerNames = new List<string>();
            listOfMaleFirstNames = new List<string>();
            listOfFemaleFirstNames = new List<string>();
            listOfSurnames = new List<string>();
            listOfGreatHouses = new List<MajorHouse>();
            listOfMinorHouses = new List<House>();
            listOfSpecialHouses = new List<House>();
            listHousePool = new List<HouseStruct>();
            listSpecialHousePool = new List<HouseStruct>();
            listOfGeoClusters = new List<GeoCluster>();
            listOfActiveActors = new List<Active>();
            arrayOfGeoNames = new string[(int)GeoType.Count][];
            listOfTraits = new List<Skill>();
            arrayOfTraits = new Skill[(int)SkillType.Count, (int)ActorSex.Count][];
            listOfSecrets = new List<Secret>();
            listOfWounds = new List<string>();
            listOfInjuries = new List<string>();
            listOfDiseases = new List<string>();
        }


        /// <summary>
        /// Set up history by importing text files and initilaising various data collections
        /// </summary>
        /// <param name="numHousesRequired">uniqueHouses from Network.InitialiseHouses</param>
        public void InitialiseHistory(int numHousesRequired)
        {   
            // First Male and Female names
            listOfMaleFirstNames.AddRange(Game.file.GetStrings("FirstMale.txt"));
            listOfFemaleFirstNames.AddRange(Game.file.GetStrings("FirstFemale.txt"));
            listOfPlayerNames.AddRange(Game.file.GetStrings("PlayerNames.txt"));
            listOfSurnames.AddRange(Game.file.GetStrings("Surnames.txt"));
            //Major houses
            listHousePool.AddRange(Game.file.GetHouses("MajorHouses.txt"));
            InitialiseMajorHouses(numHousesRequired);
            //Minor houses, run AFTER major houses
            listHousePool.Clear();
            listHousePool.AddRange(Game.file.GetHouses("MinorHouses.txt"));
            //Special Houses, run AFTER minor houses
            listSpecialHousePool.AddRange(Game.file.GetHouses("SpecialHouses.txt"));
            //GeoNames
            arrayOfGeoNames = Game.file.GetGeoNames("GeoNames.txt");
            InitialiseGeoClusters();
            //Traits
            listOfTraits.AddRange(Game.file.GetTraits("Traits_All.txt", SkillSex.All));
            listOfTraits.AddRange(Game.file.GetTraits("Traits_Male.txt", SkillSex.Male));
            listOfTraits.AddRange(Game.file.GetTraits("Traits_Female.txt", SkillSex.Female));
            //mishaps
            listOfWounds.AddRange(Game.file.GetStrings("Wounds.txt"));
            //set up filtered sets of traits ready for random access by newly created actors
            InitialiseTraits();
            //Player & Followers (set up later in World)
            InitialisePlayer();
            InitialiseFollowers(Game.file.GetFollowers("Followers.txt"));
            
        }

        /// <summary>
        /// Sets up Great Houses, assumes listHousePool<houseStruct> already populated
        /// </summary>
        /// <param name="numHousesRequired">Will thin out surplus houses if required</param>
        private void InitialiseMajorHouses(int numHousesRequired)
        {
            //remove surplus houses from pool
            int count = listHousePool.Count;
            int index = 0;
            while (count > numHousesRequired)
            {
                index = rnd.Next(0, count);
                Console.WriteLine("Great House {0} removed", listHousePool[index].Name);
                try
                { listHousePool.RemoveAt(index); }
                catch (Exception e)
                { Game.SetError(new Error(61, e.Message)); }
                count = listHousePool.Count;
            }
            Console.WriteLine();
            //loop through structures and initialise House classes
            for (int i = 0; i < listHousePool.Count; i++)
            {
                MajorHouse house = new MajorHouse();
                //copy data from House pool structures
                house.Name = listHousePool[i].Name;
                house.Motto = listHousePool[i].Motto;
                house.Banner = listHousePool[i].Banner;
                house.ArcID = listHousePool[i].ArcID;
                house.RefID = listHousePool[i].RefID;
                house.LocName = listHousePool[i].Capital;
                house.MenAtArms = Game.constant.GetValue(Global.MEN_AT_ARMS);
                house.CastleWalls = listHousePool[i].Castle;
                //add house to listOfHouses
                listOfGreatHouses.Add(house);
                //Console.WriteLine("House {0} added to listOfGreatHouses", house.Name);
            }
        }

        /// <summary>
        /// called by Network.UpdateHouses(), it randomly chooses a minor house from list and initiliases it
        /// </summary>
        /// <param name="locID"></param>
        /// <param name="houseID"></param>
        public void InitialiseMinorHouse(int locID, int houseID)
        {
            //get random minorhouse
            int index = rnd.Next(0, listHousePool.Count);
            MinorHouse house = new MinorHouse();
            //copy data from House pool structures
            house.Name = listHousePool[index].Name;
            house.Motto = listHousePool[index].Motto;
            house.Banner = listHousePool[index].Banner;
            house.ArcID = listHousePool[index].ArcID;
            house.LocName = listHousePool[index].Capital;
            house.RefID = listHousePool[index].RefID;
            house.CastleWalls = listHousePool[index].Castle;
            house.LocID = locID;
            house.HouseID = houseID;
            house.MenAtArms = Game.constant.GetValue(Global.MEN_AT_ARMS) / 2;
            //add house to listOfHouses
            listOfMinorHouses.Add(house);
            //remove minorhouse from pool list to avoid being chosen again
            try
            { listHousePool.RemoveAt(index); }
            catch (Exception e)
            { Game.SetError(new Error(61, e.Message)); }
            //update location details
            Location loc = Game.network.GetLocation(locID);
            loc.LocName = house.LocName;
            loc.RefID = house.RefID;
        }

        /// <summary>
        /// randomly choose special house (eg. inn) and initialise it
        /// </summary>
        public void InitialiseSpecialHouses()
        {
            List<Location> listLocations = Game.network.GetLocationsList();
            int index;

            //NOTE: Assumes at present the only special house type is an Inn 

            //loop locations looking for specials (houseID = 99)
            for (int i = 0; i < listLocations.Count; i++)
            {
                Location loc = listLocations[i];
                if (loc.HouseID == 99)
                {
                    //get random special Inn
                    index = rnd.Next(0, listSpecialHousePool.Count);
                    InnHouse specialInn = new InnHouse();
                    //copy data over from house struct
                    specialInn.Name = listSpecialHousePool[index].Name;
                    specialInn.Motto = listSpecialHousePool[index].Motto;
                    specialInn.Banner = listSpecialHousePool[index].Banner;
                    specialInn.ArcID = listSpecialHousePool[index].ArcID;
                    specialInn.LocName = listSpecialHousePool[index].Name;
                    specialInn.RefID = listSpecialHousePool[index].RefID;
                    specialInn.LocID = loc.LocationID;
                    specialInn.HouseID = loc.HouseID;
                    specialInn.MenAtArms = 0;
                    //add house to listOfHouses
                    listOfSpecialHouses.Add(specialInn);
                    Game.world.AddOtherHouse(specialInn);
                    Console.WriteLine("\"{0}\" Inn initialised, RefID {1}, LocID {2}, HouseID {3}", specialInn.Name, specialInn.RefID, specialInn.LocID, specialInn.HouseID);
                    //remove minorhouse from pool list to avoid being chosen again
                    try
                    { listSpecialHousePool.RemoveAt(index); }
                    catch (Exception e)
                    { Game.SetError(new Error(61, e.Message)); }
                    //update location details
                    loc.LocName = specialInn.LocName;
                    loc.RefID = specialInn.RefID;
                }
            }
        }

        /// <summary>
        /// add names and any historical data to GeoClusters
        /// </summary>
        public void InitialiseGeoClusters()
        {
            listOfGeoClusters = Game.map.GetGeoCluster();
            List<string> tempList = new List<string>();
            int randomNum;
            //convert array data to lists (not very elegant but it'll do the job) - needs to be in list to prevent duplicate names
            List<string> listOfLargeSeas = new List<string>(arrayOfGeoNames[(int)GeoType.Large_Sea].ToList());
            List<string> listOfMediumSeas = new List<string>(arrayOfGeoNames[(int)GeoType.Medium_Sea].ToList()); ;
            List<string> listOfSmallSeas = new List<string>(arrayOfGeoNames[(int)GeoType.Small_Sea].ToList()); ;
            List<string> listOfLargeMountains = new List<string>(arrayOfGeoNames[(int)GeoType.Large_Mtn].ToList()); ;
            List<string> listOfMediumMountains = new List<string>(arrayOfGeoNames[(int)GeoType.Medium_Mtn].ToList()); ;
            List<string> listOfSmallMountains = new List<string>(arrayOfGeoNames[(int)GeoType.Small_Mtn].ToList()); ;
            List<string> listOfLargeForests = new List<string>(arrayOfGeoNames[(int)GeoType.Large_Forest].ToList()); ;
            List<string> listOfMediumForests = new List<string>(arrayOfGeoNames[(int)GeoType.Medium_Forest].ToList()); ;
            List<string> listOfSmallForests = new List<string>(arrayOfGeoNames[(int)GeoType.Small_Forest].ToList()); ;
            //assign a random name
            for (int i = 0; i < listOfGeoClusters.Count; i++)
            {
                GeoCluster cluster = listOfGeoClusters[i];
                if (cluster != null)
                {
                    switch (cluster.Terrain)
                    {
                        case Cluster.Sea:
                            if (cluster.Type == GeoType.Small_Sea)
                            { tempList = listOfSmallSeas;}
                            else if (cluster.Type == GeoType.Large_Sea)
                            { tempList = listOfLargeSeas; }
                            else
                            { tempList = listOfMediumSeas; }
                            break;
                        case Cluster.Mountain:
                            if (cluster.Type == GeoType.Small_Mtn)
                            { tempList = listOfSmallMountains; }
                            else if (cluster.Type == GeoType.Large_Mtn)
                            { tempList = listOfLargeMountains; }
                            else
                            { tempList = listOfMediumMountains; }
                            break;
                        case Cluster.Forest:
                            if (cluster.Type == GeoType.Small_Forest)
                            { tempList = listOfSmallForests; }
                            else if (cluster.Type == GeoType.Large_Forest)
                            { tempList = listOfLargeForests; }
                            else
                            { tempList = listOfMediumForests; }
                            break;

                    }
                    //choose a random name
                    if (tempList.Count > 0)
                    {
                        randomNum = rnd.Next(0, tempList.Count);
                        cluster.Name = tempList[randomNum];
                        //delete from list to prevent reuse
                        try
                        { tempList.RemoveAt(randomNum); }
                        catch (Exception e)
                        { Game.SetError(new Error(61, e.Message)); }
                    }
                    else
                    {
                        cluster.Name = "Unknown";
                        Game.SetError(new Error(24, string.Format("Cluster {0} tempList has no records, index {1}", cluster.Terrain, i)));
                    }
                }
            }
        }

        /// <summary>
        /// Sets up a place holder character with Actor ID 1
        /// </summary>
        private void InitialisePlayer()
        {
            int locID;
            //create player (place holder)
            Player player = new Player("William Tell", ActorType.Usurper);
            //Player should be first record in list and first Active actor initialised (ActID = '1')
            listOfActiveActors.Add(player);
            //assign to random location on map
            locID = Game.network.GetRandomLocation();
            Location loc = Game.network.GetLocation(locID);
            //place characters at Location
            player.LocID = locID;
            player.SetActorPosition(loc.GetPosition());
            player.Type = ActorType.Usurper;
            //add to Location list of Characters
            loc.AddActor(player.ActID);
        }

        /// <summary>
        /// add followers to listofActiveActors from imported data
        /// </summary>
        /// <param name="listOfStructs"></param>
        internal void InitialiseFollowers(List<FollowerStruct> listOfStructs)
        {
            Console.WriteLine(Environment.NewLine + "--- Import Followers");
            int age = (int)SkillAge.Fifteen;
            //Convert FollowerStructs into Follower objects
            foreach (var data in listOfStructs)
            {
                Follower follower = null;
                try
                { follower = new Follower(data.Name, ActorType.Follower, data.FID, data.Sex); }
                catch (Exception e)
                { Game.SetError(new Error(59, e.Message)); continue; /*skip this record*/}
                if (follower != null)
                {
                    //copy data across from struct to object
                    follower.Role = data.Role;
                    follower.Loyalty_Player = data.Loyalty;
                    follower.Description = data.Description;
                    follower.ArcID = data.ArcID;
                    follower.Resources = data.Resources;
                    follower.Age = data.Age;
                    follower.Born = Game.gameStart - data.Age;
                    //trait effects
                    follower.arrayOfTraitEffects[age, (int)SkillType.Combat] = data.Combat_Effect;
                    follower.arrayOfTraitEffects[age, (int)SkillType.Wits] = data.Wits_Effect;
                    follower.arrayOfTraitEffects[age, (int)SkillType.Charm] = data.Charm_Effect;
                    follower.arrayOfTraitEffects[age, (int)SkillType.Treachery] = data.Treachery_Effect;
                    follower.arrayOfTraitEffects[age, (int)SkillType.Leadership] = data.Leadership_Effect;
                    follower.arrayOfTraitEffects[age, (int)SkillType.Touched] = data.Touched_Effect;
                    if (data.Touched_Effect != 0) { follower.Touched = 3; }
                    //trait names
                    follower.arrayOfTraitNames[(int)SkillType.Combat] = data.Combat_Trait;
                    follower.arrayOfTraitNames[(int)SkillType.Wits] = data.Wits_Trait;
                    follower.arrayOfTraitNames[(int)SkillType.Charm] = data.Charm_Trait;
                    follower.arrayOfTraitNames[(int)SkillType.Treachery] = data.Treachery_Trait;
                    follower.arrayOfTraitNames[(int)SkillType.Leadership] = data.Leadership_Trait;
                    follower.arrayOfTraitNames[(int)SkillType.Touched] = data.Touched_Trait;
                    //trait ID's not needed
                    //add to list
                    listOfActiveActors.Add(follower);
                    Console.WriteLine("{0}, Aid {1}, FID {2}, \"{3}\" Loyalty {4}", follower.Name, follower.ActID, follower.FollowerID, follower.Role, follower.Loyalty_Player);
                }
            }
        }

        /*
        /// <summary>
        /// create a base list of Player controlled Characters
        /// </summary>
        /// <param name="numCharacters" how many characters do you want?></param>
        public void CreatePlayerActors(int numCharacters)
        {
            string actorName;
            //rough and ready creation of a handful of basic player characters
            for (int i = 0; i < numCharacters; i++)
            {

            //debug -> temp method to create Player

                int index;
                index = rnd.Next(0, listOfPlayerNames.Count);
                //get name
                actorName = listOfPlayerNames[index];
                //delete record in list to prevent duplicate names
                try
                { listOfPlayerNames.RemoveAt(index); }
                catch (Exception e)
                { Game.SetError(new Error(61, e.Message)); }
                //new character
                //ActorType type = ActorType.Loyal_Follower;
                Active person = null;
                //set player as ursuper
                //if (i == 0)
                //{
                    ActorType type = ActorType.Ursuper;
                    person = new Player(actorName, type) as Player;
                //}
                //else
                //{ person = new Follower(actorName, type, 0); }
                //listOfPlayerActors.Add(person);

           // }
        }
        */


        /// <summary>
        /// Initialise Passive Actors at game start (populate world) - Nobles, Bannerlords only & add to location
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        internal Passive CreateStartingHouseActor(string lastName, ActorType type, Position pos, int locID, int refID, int houseID, ActorSex sex = ActorSex.Male, WifeStatus wifeStatus = WifeStatus.None)
        {
            //get a random first name
            string actorName = GetActorName(lastName, sex, refID);
            string descriptor;
            Passive actor = null;
            //Lord or lady
            if (type == ActorType.Lady || type == ActorType.Lord)
            { actor = new Noble(actorName, type, sex); actor.Realm = ActorRealm.Head_of_House; }
            //bannerlord
            else if (type == ActorType.BannerLord)
            { actor = new BannerLord(actorName, type, sex); actor.Realm = ActorRealm.Head_of_House; }
            //illegal actor type
            else
            { Game.SetError(new Error(8, "invalid ActorType")); return actor; }
            //age (older men, younger wives
            int age = 0;
            if (sex == ActorSex.Male)
            { age = rnd.Next(25, 60); }
            else
            { age = rnd.Next(14, 35); }
            actor.Born = Game.gameRevolt - age;
            actor.Age = age;
            //data
            actor.SetActorPosition(pos);
            actor.LocID = locID;
            actor.RefID = refID;
            actor.HouseID = houseID;
            if (actor is Noble)
            {
                Noble noble = actor as Noble;
                noble.GenID = 1;
                noble.WifeNumber = wifeStatus;
            }
            //add to Location
            Location loc = Game.network.GetLocation(locID);
            loc.AddActor(actor.ActID);
            //house at birth (males the same, females from an adjacent house)
            actor.BornRefID = refID;
            int wifeHouseID = houseID;
            int highestHouseID = listOfGreatHouses.Count;
            if (sex == ActorSex.Female && highestHouseID >= 2)
            {
                /*if (rnd.Next(100) < 50)
                {
                    //wife was born in Great House with lower HouseID
                    wifeHouseID = houseID - 1;
                    //can't be '0', instead roll over
                    if (wifeHouseID == 0)
                    { wifeHouseID = highestHouseID; }
                }
                else
                {
                    //wife born in higher HouseID
                    wifeHouseID = houseID + 1;
                    if (wifeHouseID > highestHouseID)
                    { wifeHouseID = highestHouseID - 2; }
                }
                int bornRefID = Game.world.GetGreatHouseRefID(wifeHouseID);
                */
                int counter = 0;
                MajorHouse wifeHome = null;
                do
                {
                    wifeHome = listOfGreatHouses[rnd.Next(0, highestHouseID)];
                    counter++;
                }
                while (wifeHome.RefID == refID && counter < 4);
                //actor.BornRefID = bornRefID;
                actor.BornRefID = wifeHome.RefID;
                if (actor is Noble)
                {
                    Noble noble = actor as Noble;
                    noble.MaidenName = Game.world.GetGreatHouseName(wifeHouseID);
                    //fertile?
                    if (age >= 13 && age <= 40)
                    { noble.Fertile = true; }
                }
            }

            //create records of being born
            if (type == ActorType.Lord)
            {
                descriptor = string.Format("{0} born, Aid {1}, at {2}", actor.Name, actor.ActID, Game.world.GetLocationName(locID));
                Record recordLord = new Record(descriptor, actor.ActID, locID, refID, actor.Born, HistActorIncident.Born);
                Game.world.SetRecord(recordLord);
            }
            else if (type == ActorType.Lady)
            {
                //location born (different for lady)
                House ladyHouse = Game.world.GetHouse(actor.BornRefID);
                Location locLady = Game.network.GetLocation(ladyHouse.LocID);
                Noble lady = actor as Noble;
                descriptor = string.Format("{0} (nee {1}, Aid {2}) born at {3}", lady.Name, lady.MaidenName, actor.ActID, locLady.LocName);
                Record recordLady = new Record(descriptor, lady.ActID, locLady.LocationID, lady.BornRefID, lady.Born, HistActorIncident.Born);
                Game.world.SetRecord(recordLady);
            }
            else if (type == ActorType.BannerLord)
            {
                //create records of being born
                BannerLord bannerLord = actor as BannerLord;
                descriptor = string.Format("{0}, Aid {1}, born at {2}", bannerLord.Name, bannerLord.ActID, Game.world.GetLocationName(locID));
                Record recordBannerLord = new Record(descriptor, bannerLord.ActID, locID, refID, bannerLord.Born, HistActorIncident.Born);
                Game.world.SetRecord(recordBannerLord);
            }

            //date Noble Lord attained lordship of House
            if (sex == ActorSex.Male && actor is Noble)
            {
                Noble noble = actor as Noble;
                int lordshipAge = rnd.Next(20, age - 2);
                noble.Lordship = actor.Born + lordshipAge;
                descriptor = "unknown";
                if (actor.Type == ActorType.Lord)
                { descriptor = string.Format("{0} assumes Lordship of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                else if (actor.Type == ActorType.BannerLord)
                { descriptor = string.Format("{0} assumes Lordship, BannerLord of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                Record record = new Record(descriptor, actor.ActID, actor.LocID, actor.RefID, noble.Lordship, HistActorIncident.Lordship);
                Game.world.SetRecord(record);
            }
            //date Bannerlord attained lordship of House
            else if (sex == ActorSex.Male && actor is BannerLord)
            {
                BannerLord bannerlord = actor as BannerLord;
                int lordshipAge = rnd.Next(20, age - 2);
                bannerlord.Lordship = actor.Born + lordshipAge;
                descriptor = string.Format("{0} assumes Lordship, BannerLord of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge);
                Record record = new Record(descriptor, actor.ActID, actor.LocID, actor.RefID, bannerlord.Lordship, HistActorIncident.Lordship);
                Game.world.SetRecord(record);
            }

            //assign traits
            InitialiseActorTraits(actor);
            return actor;
        }

        /// <summary>
        /// create a Bannerlord and add to Location (any time) -> no record created, or data for time of attaining Lordship
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="houseID"></param>
        /// <returns></returns>
        internal BannerLord CreateBannerLord(string lastName, Position pos, int locID, int refID, int houseID)
        {
            //get a random first name
            string actorName = GetActorName(lastName, ActorSex.Male, refID);
            BannerLord actor = new BannerLord(actorName);
            actor.Realm = ActorRealm.Head_of_House;
            //age (older men, younger wives
            int age = rnd.Next(25, 60);
            actor.Born = Game.gameRevolt - age;
            actor.Age = age;
            //data
            actor.SetActorPosition(pos);
            actor.LocID = locID;
            actor.RefID = refID;
            actor.HouseID = houseID;
            //add to Location
            Location loc = Game.network.GetLocation(locID);
            loc.AddActor(actor.ActID);
            //house at birth (males the same)
            actor.BornRefID = refID;
            //create records of being born
            string descriptor = string.Format("{0}, Aid {1}, born at {2}", actor.Name, actor.ActID, Game.world.GetLocationName(locID));
            Record recordBannerLord = new Record(descriptor, actor.ActID, locID, refID, actor.Born, HistActorIncident.Born);
            Game.world.SetRecord(recordBannerLord);
            //assign traits
            InitialiseActorTraits(actor);
            return actor;
        }

        /// <summary>
        /// Creates a Regent noble lord (not Head of House, eg. brother to head of house who has died)
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="houseID"></param>
        /// <returns></returns>
        internal Noble CreateRegent(string lastName, Position pos, int locID, int refID, int houseID)
        {
            //get a random first name
            string actorName = GetActorName(lastName, ActorSex.Male, refID);
            Noble actor = new Noble(actorName, ActorType.lord);
            actor.Realm = ActorRealm.Regent;
            //age (older men, younger wives
            int age = rnd.Next(25, 60);
            actor.Born = Game.gameRevolt - age;
            actor.Age = age;
            //data
            actor.SetActorPosition(pos);
            actor.LocID = locID;
            actor.RefID = refID;
            actor.HouseID = houseID;
            //add to Location
            Location loc = Game.network.GetLocation(locID);
            loc.AddActor(actor.ActID);
            //house at birth (males the same)
            actor.BornRefID = refID;
            //create records of being born
            string descriptor = string.Format("{0}, Aid {1}, born at {2}", actor.Name, actor.ActID, Game.world.GetLocationName(locID));
            Record recordBannerLord = new Record(descriptor, actor.ActID, locID, refID, actor.Born, HistActorIncident.Born);
            Game.world.SetRecord(recordBannerLord);
            //assign traits
            InitialiseActorTraits(actor);
            return actor;
        }

        /// <summary>
        /// Create a Knight & add to Location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="houseID"></param>
        /// <returns></returns>
        internal Knight CreateKnight(Position pos, int locID, int refID, int houseID)
        {
            ActorType type = ActorType.Knight;
            string name = "Ser " + GetActorName();
            Knight knight = new Knight(name, type, ActorSex.Male);
            //age (older men, younger wives
            int age = rnd.Next(25, 60);
            knight.Born = Game.gameRevolt - age;
            knight.Age = age;
            //when knighted (age 18 - 27)
            int knighted = rnd.Next(18, 28);
            knight.Knighthood = Game.gameRevolt - (age - knighted);
            //add to Location
            Location loc = Game.network.GetLocation(locID);
            loc.AddActor(knight.ActID);
            //data
            knight.SetActorPosition(pos);
            knight.LocID = locID;
            knight.RefID = refID;
            knight.HouseID = houseID;
            knight.Type = ActorType.Knight;
            knight.Realm = ActorRealm.None;
            InitialiseActorTraits(knight, null, null, SkillType.Combat, SkillType.Treachery);
            //record
            string descriptor = string.Format("{0} knighted and swears allegiance to House {1}, age {2}", knight.Name, Game.world.GetGreatHouseName(knight.HouseID), knighted);
            Record record = new Record(descriptor, knight.ActID, knight.LocID, knight.RefID, knight.Knighthood, HistActorIncident.Knighthood);
            Game.world.SetRecord(record);
            return knight;
        }

        /// <summary>
        /// Create a Royal or Noble House Advisor & Add to Location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="houseID"></param>
        /// <param name="advisorNoble"></param>
        /// <param name="advisorRoyal"></param>
        /// <param name="advisorReligious"></param>
        /// <param name="yearCommenced">If default 0 then will calculate a commencement based on age (pre-Uprising), otherwise it should be specified year</param>
        /// <returns></returns>
        internal Advisor CreateAdvisor(Position pos, int locID, int refID, int houseID, ActorSex sex = ActorSex.Male,
            AdvisorNoble advisorNoble = AdvisorNoble.None, AdvisorRoyal advisorRoyal = AdvisorRoyal.None, int yearCommenced = 0)
        {
            //must be one type of advisor
            if ((int)advisorRoyal == 1 && (int)advisorNoble == 1)
            { Game.SetError(new Error(11, "No valid advisor type provided")); return null;  }
            //can only be a single type of advisor
            else if ((int)advisorRoyal > 1 && (int)advisorNoble > 1)
            { Game.SetError(new Error(12, "Only a single advisor type is allowed")); return null; }
            else
            {
                string descriptor;
                string name = GetActorName();
                Advisor advisor = new Advisor(name, ActorType.Advisor, locID, sex);
                advisor.SetActorPosition(pos);
                advisor.LocID = locID;
                advisor.RefID = refID;
                advisor.HouseID = houseID;
                //age
                int minAge = 20; int maxAge = 60;
                int age = rnd.Next(minAge, maxAge);
                advisor.Born = Game.gameRevolt - age;
                int startAge = yearCommenced - advisor.Born;
                advisor.Age = age;
                //auto calc's start of service if no year provided. Can't be before minAge of advisor)
                if (yearCommenced == 0)
                { int diff = age - minAge; startAge = minAge + rnd.Next(0, diff / 2); ; yearCommenced = advisor.Born + startAge; }
                //startAge = Math.Max(minAge, age - startAge) - temp, about to remove
                advisor.CommenceService = yearCommenced;
                //add to Location
                Location loc = Game.network.GetLocation(locID);
                loc.AddActor(advisor.ActID);
                //traits
                SkillType positiveTrait = SkillType.None;
                SkillType negativeTrait = SkillType.None;
                if ((int)advisorRoyal > 0)
                {
                    advisor.advisorRoyal = advisorRoyal;
                    negativeTrait = SkillType.Combat;
                    if (advisorRoyal == AdvisorRoyal.Master_of_Whisperers) { positiveTrait = SkillType.Treachery; }
                    else if (advisorRoyal == AdvisorRoyal.High_Septon) { positiveTrait = SkillType.Charm; negativeTrait = SkillType.Treachery; }
                    else if (advisorRoyal == AdvisorRoyal.Commander_of_Kings_Guard) { positiveTrait = SkillType.Combat; negativeTrait = SkillType.Treachery; }
                    else if (advisorRoyal == AdvisorRoyal.Commander_of_City_Watch) { positiveTrait = SkillType.Leadership; negativeTrait = SkillType.Treachery; }
                    else if (advisorRoyal == AdvisorRoyal.Hand_of_the_King) { positiveTrait = SkillType.Wits; negativeTrait = SkillType.Treachery; }
                    else { positiveTrait = SkillType.Wits;  }
                    //record
                    descriptor = string.Format("{0} {1}, Aid {2}, commenced serving on the Royal Council, age {3}", advisor.advisorRoyal, advisor.Name, advisor.ActID, startAge);
                    Record record = new Record(descriptor, advisor.ActID, locID, refID, yearCommenced, HistActorIncident.Service);
                    Game.world.SetRecord(record);
                }
                else if ((int)advisorNoble > 0)
                {
                    advisor.advisorNoble = advisorNoble;
                    if (advisorNoble == AdvisorNoble.Castellan) { positiveTrait = SkillType.Leadership; negativeTrait = SkillType.Treachery; }
                    else if (advisorNoble == AdvisorNoble.Septon) { positiveTrait = SkillType.Charm; negativeTrait = SkillType.Treachery; }
                    else { positiveTrait = SkillType.Wits; negativeTrait = SkillType.Combat; }
                    //record
                    descriptor = string.Format("{0} {1}, Aid {2}, entered the service of House {3}, age {4}", advisor.advisorNoble, advisor.Name, advisor.ActID, Game.world.GetGreatHouseName(houseID), startAge);
                    Record record = new Record(descriptor, advisor.ActID, locID, refID, yearCommenced, HistActorIncident.Service );
                    Game.world.SetRecord(record);
                }
                else
                { Game.SetError(new Error(11, "No valid advisor type provided")); return null; }
                //assign traits & return
                InitialiseActorTraits(advisor, null, null, positiveTrait, negativeTrait);
                return advisor;
            }
        }

        /// <summary>
        /// Assigns traits to actors (early, under 15 y.0 - Combat, Wits, Charm, Treachery, Leadership)
        /// </summary>
        /// <param name="person"></param>
        /// <param name="father">supply for when parental genetics come into play</param>
        /// <param name="mother">supply for when parental genetics come into play</param>
        /// <param name="traitPositive">if present this trait will be given preference (high values more likely, but not guaranteed)</param>
        /// <param name="traitNegative">if present this trait will be given malus (low values more likely, but not guaranteed)</param>
        private void InitialiseActorTraits(Actor person, Noble father = null, Noble mother = null, SkillType traitPositive = SkillType.None, SkillType traitNegative = SkillType.None)
        {
            //nicknames from all assigned traits kept here and one is randomly chosen to be given the actor (their 'handle')
            List<string> tempHandles = new List<string>();
            bool needRandomTrait = true;
            int rndRange;
            int startRange = 0; //used for random selection of traits
            int endRange = 0;
            Skill rndTrait;
            int chanceOfTrait;
            int traitID;
            int effect;
            
            //Combat ---
            if (father != null && mother != null)
            {
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a combat trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Combat];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Combat] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Combat];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Combat] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Combat];
                            person.arrayOfSkillID[(int)SkillType.Combat] = parent.arrayOfSkillID[(int)SkillType.Combat];
                            person.arrayOfTraitNames[(int)SkillType.Combat] = parent.arrayOfTraitNames[(int)SkillType.Combat];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Combat trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            else { needRandomTrait = true; }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                rndRange = arrayOfTraits[(int)SkillType.Combat, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == SkillType.Combat) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == SkillType.Combat) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    
                    rndTrait = arrayOfTraits[(int)SkillType.Combat, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        effect = rndTrait.Effect;
                        traitID = rndTrait.SkillID;
                        SkillAge age = rndTrait.Age;
                        //Console.WriteLine("{0}, ID {1} Effect {2} Actor {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfSkillID[(int)SkillType.Combat] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)SkillType.Combat] = effect;
                        person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Combat] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)SkillType.Combat] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Wits ---
            if (father != null && mother != null)
            {
                //parental wits trait (if any)
                traitID = 0;
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to Inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male)
                { parent = father; }
                else { parent = mother; }
                //does parent have a Wits trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Wits];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Wits] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Wits];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Wits] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Wits];
                            person.arrayOfSkillID[(int)SkillType.Wits] = parent.arrayOfSkillID[(int)SkillType.Wits];
                            person.arrayOfTraitNames[(int)SkillType.Wits] = parent.arrayOfTraitNames[(int)SkillType.Wits];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Wits trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            else { needRandomTrait = true; }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                rndRange = arrayOfTraits[(int)SkillType.Wits, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == SkillType.Wits) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == SkillType.Wits) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)SkillType.Wits, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        effect = rndTrait.Effect;
                        traitID = rndTrait.SkillID;
                        SkillAge age = rndTrait.Age;
                        //Console.WriteLine("Wits {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfSkillID[(int)SkillType.Wits] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)SkillType.Wits] = effect;
                        person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Wits] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)SkillType.Wits] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Charm ---
            if (father != null && mother != null)
            {
                //parental Charm trait (if any)
                traitID = 0;
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Charm trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Charm];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Charm] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Charm];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Charm] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Charm];
                            person.arrayOfSkillID[(int)SkillType.Charm] = parent.arrayOfSkillID[(int)SkillType.Charm];
                            person.arrayOfTraitNames[(int)SkillType.Charm] = parent.arrayOfTraitNames[(int)SkillType.Charm];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Charm trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            else { needRandomTrait = true; }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                rndRange = arrayOfTraits[(int)SkillType.Charm, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == SkillType.Charm) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == SkillType.Charm) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)SkillType.Charm, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        effect = rndTrait.Effect;
                        traitID = rndTrait.SkillID;
                        SkillAge age = rndTrait.Age;
                        //Console.WriteLine("Charm {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfSkillID[(int)SkillType.Charm] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)SkillType.Charm] = effect;
                        person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Charm] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)SkillType.Charm] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Treachery ---
            if (father != null && mother != null)
            {
                //parental Treachery trait (if any)
                traitID = 0;
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Treachery trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Treachery];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Treachery] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Treachery];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Treachery] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Treachery];
                            person.arrayOfSkillID[(int)SkillType.Treachery] = parent.arrayOfSkillID[(int)SkillType.Treachery];
                            person.arrayOfTraitNames[(int)SkillType.Treachery] = parent.arrayOfTraitNames[(int)SkillType.Treachery];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Treachery trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            else { needRandomTrait = true; }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                rndRange = arrayOfTraits[(int)SkillType.Treachery, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == SkillType.Treachery) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == SkillType.Treachery) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }
                
                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)SkillType.Treachery, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        effect = rndTrait.Effect;
                        traitID = rndTrait.SkillID;
                        SkillAge age = rndTrait.Age;
                        //Console.WriteLine("{0}, ID {1} Effect {2} Actor {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfSkillID[(int)SkillType.Treachery] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)SkillType.Treachery] = effect;
                        person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Treachery] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)SkillType.Treachery] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Leadership ---
            if (father != null && mother != null)
            {
                //parental Leadership trait (if any)
                traitID = 0;
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Leadership trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Leadership];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Leadership] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Leadership];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Leadership] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Leadership];
                            person.arrayOfSkillID[(int)SkillType.Leadership] = parent.arrayOfSkillID[(int)SkillType.Leadership];
                            person.arrayOfTraitNames[(int)SkillType.Leadership] = parent.arrayOfTraitNames[(int)SkillType.Leadership];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Leadership trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            else { needRandomTrait = true; }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                rndRange = arrayOfTraits[(int)SkillType.Leadership, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == SkillType.Leadership) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == SkillType.Leadership) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)SkillType.Leadership, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        effect = rndTrait.Effect;
                        traitID = rndTrait.SkillID;
                        SkillAge age = rndTrait.Age;
                        //Console.WriteLine("Leadership {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfSkillID[(int)SkillType.Leadership] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)SkillType.Leadership] = effect;
                        person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Leadership] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)SkillType.Leadership] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Touched ---
            if (father != null && mother != null)
            {
                //parental Touched trait (if any)
                traitID = 0;
                Noble parent = new Noble();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Touched trait?
                traitID = parent.arrayOfSkillID[(int)SkillType.Touched];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //give base strength of 3 (prior to any traits)
                        person.Touched = 3;
                        //find trait
                        Skill trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            SkillAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Touched] = parent.arrayOfTraitEffects[(int)age, (int)SkillType.Touched];
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Touched] = parent.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Touched];
                            person.arrayOfSkillID[(int)SkillType.Touched] = parent.arrayOfSkillID[(int)SkillType.Touched];
                            person.arrayOfTraitNames[(int)SkillType.Touched] = parent.arrayOfTraitNames[(int)SkillType.Touched];
                            //add trait nicknames to list of possible handles
                            tempHandles.AddRange(trait.GetNickNames());
                            needRandomTrait = false;

                            Console.WriteLine("Inherited Touched trait, Actor ID {0}, Parent ID {1}", person.ActID, parent.ActID);
                        }
                    }
                    else { needRandomTrait = true; }
                }
            }
            //Random Trait (no genetics apply, either no parents or genetic roll failed)
            if (needRandomTrait == true)
            {
                //This is a special attribute that is normally not present ( value 0).
                if (rnd.Next(100) < Game.constant.GetValue(Global.TOUCHED))
                {
                    //give base strength of 3 (prior to any traits)
                    person.Touched = 3;
                    Console.WriteLine("- {0}, Aid {1} is Touched", person.Name, person.ActID);
                    rndRange = arrayOfTraits[(int)SkillType.Touched, (int)person.Sex].Length;
                    //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                    if (traitPositive == SkillType.Touched) { startRange = 0; endRange = rndRange / 2; }
                    else if (traitNegative == SkillType.Touched) { startRange = rndRange / 2; endRange = rndRange; }
                    else { startRange = 0; endRange = rndRange; }

                    if (rndRange > 0)
                    {
                        rndTrait = arrayOfTraits[(int)SkillType.Touched, (int)person.Sex][rnd.Next(startRange, endRange)];
                        //trait roll (trait only assigned if passes roll, otherwise no trait)
                        chanceOfTrait = rndTrait.Chance;
                        if (rnd.Next(100) < chanceOfTrait)
                        {
                            string name = rndTrait.Name;
                            effect = rndTrait.Effect;
                            traitID = rndTrait.SkillID;
                            SkillAge age = rndTrait.Age;
                            //Console.WriteLine("Touched {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                            //update trait arrays
                            person.arrayOfSkillID[(int)SkillType.Touched] = traitID;
                            person.arrayOfTraitEffects[(int)age, (int)SkillType.Touched] = effect;
                            person.arrayOfTraitEffects[(int)SkillAge.Fifteen, (int)SkillType.Touched] = effect; //any age 5 effect also needs to set for age 15
                            person.arrayOfTraitNames[(int)SkillType.Touched] = name;
                        }
                    }
                    else
                    { Game.SetError(new Error(13, "Invalid Range")); }
                }
            }
            //if touched create a secret
            if (person.Touched != 0)
            {
                string description = string.Format("{0}, Aid {1}, was born under a Dark Moon (Touched)", person.Name, person.ActID);
                Secret_Actor secret = new Secret_Actor(SecretType.Trait, person.Born, description, person.Touched, person.ActID);
                listOfSecrets.Add(secret);
                person.AddSecret(secret.SecretID);
            }

            //choose NickName (handle)
            if (tempHandles.Count > 0)
            { person.Handle = tempHandles[rnd.Next(tempHandles.Count)]; }
        }



        /// <summary>
        /// sets up a bunch of list with filtered Male + All or Female + All traits to enable quick random access (rather than having to run a query each time)
        /// </summary>
        private void InitialiseTraits()
        {

            //Combat male
            IEnumerable<Skill> enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Combat && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Combat, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Combat, (int)ActorSex.Male] = enumTraits.ToArray();
            //Combat female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Combat && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Combat, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Combat, (int)ActorSex.Female] = enumTraits.ToArray();

            //Wits male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Wits && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Wits, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Wits, (int)ActorSex.Male] = enumTraits.ToArray();
            //Wits female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Wits && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Wits, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Wits, (int)ActorSex.Female] = enumTraits.ToArray();

            //Charm male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Charm && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Charm, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Charm, (int)ActorSex.Male] = enumTraits.ToArray();
            //Charm female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Charm && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Charm, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Charm, (int)ActorSex.Female] = enumTraits.ToArray();

            //Treachery male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Treachery && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Treachery, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Treachery, (int)ActorSex.Male] = enumTraits.ToArray();
            //Treachery female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Treachery && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Treachery, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Treachery, (int)ActorSex.Female] = enumTraits.ToArray();

            //Leadership male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Leadership && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Leadership, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Leadership, (int)ActorSex.Male] = enumTraits.ToArray();
            //Leadership female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Leadership && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Leadership, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Leadership, (int)ActorSex.Female] = enumTraits.ToArray();

            //Touched male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Touched && trait.Sex != SkillSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Touched, (int)ActorSex.Male] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Touched, (int)ActorSex.Male] = enumTraits.ToArray();
            //Touched female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == SkillType.Touched && trait.Sex != SkillSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)SkillType.Touched, (int)ActorSex.Female] = new Skill[enumTraits.Count()];
            arrayOfTraits[(int)SkillType.Touched, (int)ActorSex.Female] = enumTraits.ToArray();
        }



        /// <summary>
        /// Game start - Great Family, marry the lord and lady and have kids
        /// </summary>
        /// <param name="place">put house name if not the default house of lord and lady</param>
        internal void CreateFamily(Noble lord, Noble lady, string place = null)
        {
            //different house? (case of newLord in Lore.cs)
            string houseName = Game.world.GetLocationName(lady.LocID);
            if (!String.IsNullOrEmpty(place)) { houseName = place; }
            //age
            int ladyAge = lady.Age;
            int ageLadyMarried = rnd.Next(13, 22);
            //check ageMarried is less than ladies Age
            if (ladyAge <= ageLadyMarried)
            { ageLadyMarried = ladyAge - 2; }
            //year married
            int yearMarried = lady.Born + ageLadyMarried;
            //check Lord at least 13 before being married
            int ageLordMarried = yearMarried - lord.Born;
            if (ageLordMarried < 10)
            {
                int diff = 10 - ageLordMarried;
                yearMarried += diff;
                ageLadyMarried += diff;
                ageLordMarried += diff;
            }
            lady.Married = yearMarried;
            lord.Married = yearMarried;
            int lordAgeMarried = yearMarried - lord.Born;
            //record event - single record tagged to both characters and houses
            string descriptor = string.Format("{0}, age {1}, and {2} (nee {3}, age {4}) married ({5}) at {6}",
                lord.Name, lordAgeMarried, lady.Name, lady.MaidenName, ageLadyMarried, lady.WifeNumber, houseName);
            Record recordLord = new Record(descriptor, lord.ActID, lord.LocID, lord.RefID, lord.Married, HistActorIncident.Married);
            recordLord.AddHouse(lady.BornRefID);
            recordLord.AddActor(lady.ActID);
            Game.world.SetRecord(recordLord);
            //add relatives
            lord.AddRelation(lady.ActID, ActorRelation.Wife);
            lady.AddRelation(lord.ActID, ActorRelation.Husband);
            // kids
            CreateStartingChildren(lord, lady, place);
            //wife has influence over husband (only if smarter)
            //SetWifeInfluence(lord, lady);
            SetInfluence(lord, lady, SkillType.Wits);
        }


        /// <summary>
        /// Keep producing children until a limit is reached (game start)
        /// </summary>
        /// <param name="lord"></param>
        /// <param name="lady"></param>
        private void CreateStartingChildren(Noble lord, Noble lady, string place = null)
        {
            //is woman fertile and within age range?
            if (lady.Fertile == true && lady.Age >= 13 && lady.Age <= 40)
            {
                
                //birthing loop, once every 2 years
                for (int year = lady.Married; year <= Game.gameRevolt; year += 2)
                {
                    //chance of a child every 2 years
                    if (rnd.Next(100) < Game.constant.GetValue(Global.PREGNANT) )
                    {
                        Noble child = CreateChild(lord, lady, year, ActorSex.None, ActorParents.Normal, place);
                        if (lady.Status == ActorStatus.Gone || lady.Fertile == false)
                        { break; }
                    }
                    //over age?
                    if (Game.gameRevolt - year > 40)
                    { lady.Fertile = false; break; }
                }
            }
        }

        /// <summary>
        /// General purpose method to create a new child, born in current year with current generation & add to location
        /// </summary>
        /// <param name="Lord">Provide a Great lord as an official father regardless of status</param>
        /// <param name="Lady">Provide a Great Lady as an official mother regardless of status</param>
        /// <param name="sex">You can specify a type</param>
        /// <param name="parents">Natural,Bastard or Adopted? (Bastard could be sired by either Mother or Father</param>
        /// <returns>Returns child object but this can be ignored unless required</returns>
        internal Noble CreateChild(Noble Lord, Noble Lady, int year, ActorSex childSex = ActorSex.None, ActorParents parents = ActorParents.Normal, string place = null)
        {

            ActorSex sex;
            string houseName;
            
            //determine sex
            if (childSex == ActorSex.Male || childSex == ActorSex.Female)
            { sex = childSex; }
            else
            {
                //new child (50/50 boy/girl)
                sex = ActorSex.Male;
                if (rnd.Next(100) < 50)
                { sex = ActorSex.Female; }
            }
            //get a random first name
            string actorName = GetActorName(Game.world.GetGreatHouseName(Lord.HouseID), sex, Lady.RefID);
            Noble child = new Noble(actorName, ActorType.None, sex);
            int age = Game.gameRevolt - year;
            age = Math.Max(age, 0);
            child.Age = age;
            child.Born = year;
            if (Lady.LocID == 0) { child.LocID = Lord.LocID; }
            else { child.LocID = Lady.LocID; }
            child.RefID = Lady.RefID;
            child.BornRefID = Lord.RefID;
            child.HouseID = Lady.HouseID;
            child.GenID = Game.gameGeneration + 1;
            child.Loyalty_AtStart = Lord.Loyalty_AtStart;
            child.Loyalty_Current = Lord.Loyalty_Current;
            //family relations
            child.AddRelation(Lord.ActID, ActorRelation.Father);
            child.AddRelation(Lady.ActID, ActorRelation.Mother);
            //normal, bastard or adopted?
            child.Parents = parents;
            //get Lord's family
            SortedDictionary<int, ActorRelation> dictTempFamily = Lord.GetFamily();
            int motherID;
            //new child is DAUGHTER
            if (sex == ActorSex.Female)
            {
                child.MaidenName = Game.world.GetGreatHouseName(Lord.HouseID);
                child.Fertile = true;
                child.Type = ActorType.lady;

                //loop list of Lord's family
                foreach (KeyValuePair<int, ActorRelation> kvp in dictTempFamily)
                {
                    if (kvp.Value == ActorRelation.Son)
                    {
                        Noble son = (Noble)Game.world.GetPassiveActor(kvp.Key);
                        //son's family tree
                        SortedDictionary<int, ActorRelation> dictTempSon = son.GetFamily();
                        motherID = Lady.ActID;
                        foreach (KeyValuePair<int, ActorRelation> keyVP in dictTempSon)
                        {
                            //find ID of mother
                            if (keyVP.Value == ActorRelation.Mother)
                            { motherID = keyVP.Key; break; }

                        }
                        //relations
                        if (motherID == Lady.ActID)
                        {
                            //natural brothers and sisters
                            son.AddRelation(child.ActID, ActorRelation.Sister);
                            child.AddRelation(son.ActID, ActorRelation.Brother);
                        }
                        else if (motherID != Lady.ActID)
                        {
                            //half brothers and sister
                            son.AddRelation(child.ActID, ActorRelation.Half_Sister);
                            child.AddRelation(son.ActID, ActorRelation.Half_Brother);
                        }
                    }
                    else if (kvp.Value == ActorRelation.Daughter)
                    {
                        Noble daughter = (Noble)Game.world.GetPassiveActor(kvp.Key);
                        //daughter's family tree
                        SortedDictionary<int, ActorRelation> dictTempDaughter = daughter.GetFamily();
                        motherID = Lady.ActID;
                        foreach (KeyValuePair<int, ActorRelation> keyVP in dictTempDaughter)
                        {
                            //find ID of mother
                            if (keyVP.Value == ActorRelation.Mother)
                            { motherID = keyVP.Key; break; }
                        }
                        //relations
                        if (motherID == Lady.ActID)
                        {
                            //natural sisters
                            daughter.AddRelation(child.ActID, ActorRelation.Sister);
                            child.AddRelation(daughter.ActID, ActorRelation.Sister);
                        }
                        else if (motherID != Lady.ActID)
                        {
                            //half sisters
                            daughter.AddRelation(child.ActID, ActorRelation.Half_Sister);
                            child.AddRelation(daughter.ActID, ActorRelation.Half_Sister);
                        }
                    }
                }
                //update parent relations
                Lord.AddRelation(child.ActID, ActorRelation.Daughter);
                Lady.AddRelation(child.ActID, ActorRelation.Daughter);
            }
            //new child is SON
            else if (sex == ActorSex.Male)
            {
                int sonCounter = 0;
                //there could be sons from a previous marriage
                if (Lady.WifeNumber != WifeStatus.First_Wife)
                {
                    foreach (KeyValuePair<int, ActorRelation> kvp in dictTempFamily)
                    {
                        if (kvp.Value == ActorRelation.Son)
                        //NOTE: if previous son is dead then the count won't be correct
                        { sonCounter++; }
                    }
                }
                //loop list of Lord's family
                foreach (KeyValuePair<int, ActorRelation> kvp in dictTempFamily)
                {
                    if (kvp.Value == ActorRelation.Son)
                    {
                        Noble son = (Noble)Game.world.GetPassiveActor(kvp.Key);
                        //son's family tree
                        SortedDictionary<int, ActorRelation> dictTempSon = son.GetFamily();
                        motherID = Lady.ActID;
                        foreach (KeyValuePair<int, ActorRelation> keyVP in dictTempSon)
                        {
                            //find ID of mother
                            if (keyVP.Value == ActorRelation.Mother)
                            { motherID = keyVP.Key; break; }
                        }
                        //relations
                        if (motherID == Lady.ActID)
                        {
                            //natural born brothers
                            son.AddRelation(child.ActID, ActorRelation.Brother);
                            child.AddRelation(son.ActID, ActorRelation.Brother);
                        }
                        else if (motherID != Lady.ActID)
                        {
                            //half brothers
                            son.AddRelation(child.ActID, ActorRelation.Half_Brother);
                            child.AddRelation(son.ActID, ActorRelation.Half_Brother);
                        }
                        sonCounter++;
                    }
                    else if (kvp.Value == ActorRelation.Daughter)
                    {
                        Noble daughter = (Noble)Game.world.GetPassiveActor(kvp.Key);
                        //daughter's family tree
                        SortedDictionary<int, ActorRelation> dictTempDaughter = daughter.GetFamily();
                        motherID = Lady.ActID;
                        foreach (KeyValuePair<int, ActorRelation> keyVP in dictTempDaughter)
                        {
                            //find ID of mother
                            if (keyVP.Value == ActorRelation.Mother)
                            { motherID = keyVP.Key; break; }
                        }
                        //relations
                        if (motherID == Lady.ActID)
                        {
                            //natural born brother and sister
                            daughter.AddRelation(child.ActID, ActorRelation.Brother);
                            child.AddRelation(daughter.ActID, ActorRelation.Sister);
                        }
                        else if (motherID != Lady.ActID)
                        {
                            //half brother and sister
                            daughter.AddRelation(child.ActID, ActorRelation.Half_Brother);
                            child.AddRelation(daughter.ActID, ActorRelation.Half_Sister);
                        }
                    }
                }

                //status (males - who is in line to inherit?)
                if (sonCounter == 0)
                { child.Type = ActorType.Heir; child.InLine = 1; }
                else
                { child.Type = ActorType.lord; child.InLine = sonCounter; }
                //update parent relations
                Lord.AddRelation(child.ActID, ActorRelation.Son);
                Lady.AddRelation(child.ActID, ActorRelation.Son);
            }
            //assign traits
            InitialiseActorTraits(child, Lord, Lady);
            //add to dictionaries
            Game.world.SetPassiveActor(child);
            //store at location
            Location loc = Game.network.GetLocation(Lord.LocID);
            loc.AddActor(child.ActID);
            //record event
            bool actualEvent = true;
            string secretText = null;
            int secretStrength = 3; //for both being a bastard and adopted

            houseName = Game.world.GetLocationName(Lady.LocID);
            //covers case of wife who died at time of birth when adding heirs in world.CheckGreatLords()
            if (Lady.LocID == 0 && String.IsNullOrEmpty(place)) { houseName = Game.world.GetLocationName(Lord.LocID); }
            //child born in specified place, not home of lord or lady
            else if (!String.IsNullOrEmpty(place)) { houseName = place; }

            string descriptor = string.Format("{0}, Aid {1}, born at {2} to {3} {4} and {5} {6}",
                    child.Name, child.ActID, houseName, Lord.Type, Lord.Name, Lady.Type, Lady.Name);
            if (child.Parents == ActorParents.Adopted)
            {
                secretText = string.Format("{0}, Aid {1}, adopted at {2} by {3} {4} and {5} {6}",
                    child.Name, child.ActID, houseName, Lord.Type, Lord.Name, Lady.Type, Lady.Name);
                actualEvent = false;
            }
            else if (child.Parents == ActorParents.Bastard)
            {
                secretText = string.Format("{0}, Aid {1}, a bastard of {2} {3} and {4} {5}",
                    child.Name, child.ActID, Lord.Type, Lord.Name, Lady.Type, Lady.Name);
                actualEvent = false;
            }
            Record record_0 = new Record(descriptor, child.ActID, child.LocID, child.RefID, child.Born, HistActorIncident.Born, actualEvent);
            record_0.AddActor(Lord.ActID);
            record_0.AddActor(Lady.ActID);
            Game.world.SetRecord(record_0);
            //secret present?
            if (secretText != null)
            {
                Secret_Actor secret = new Secret_Actor(SecretType.Parents, year, secretText, secretStrength, child.ActID);
                listOfSecrets.Add(secret);
                Lord.AddSecret(secret.SecretID);
                Lady.AddSecret(secret.SecretID);
                child.AddSecret(secret.SecretID);
            }
            //childbirth issues
            {
                //only if a normal birth
                if (parents == ActorParents.Normal)
                {
                    int num = rnd.Next(100);
                    if (num < Game.constant.GetValue(Global.CHILDBIRTH_DEATH))
                    {
                        //Mother died at childbirth but child survived
                        descriptor = string.Format("{0} {1}, Aid {2} died while giving birth to {3}, age {4}", Lady.Type, Lady.Name, Lady.ActID, child.Name, Lady.Age);
                        Record record = new Record(descriptor, Lady.ActID, Lady.LocID, Lady.RefID, year, HistActorIncident.Died);
                        record.AddActorIncident(HistActorIncident.Birthing);
                        record.AddActor(Lord.ActID);
                        record.AddActor(child.ActID);
                        Game.world.SetRecord(record);
                        RemoveActor(Lady, year, ActorGone.Childbirth);
                    }
                    else if (num < Game.constant.GetValue(Global.CHILDBIRTH_COMPLICATIONS))
                    {
                        //Complications
                        descriptor = string.Format("{0}, Aid {1} suffered complications while giving birth to {2}", Lady.Name, Lady.ActID, child.Name);
                        Record record_2 = new Record(descriptor, Lady.ActID, Lady.LocID, Lady.RefID, year, HistActorIncident.Birthing);
                        Game.world.SetRecord(record_2);
                        //chance of infertility
                        if (rnd.Next(100) < Game.constant.GetValue(Global.CHILDBIRTH_INFERTILE))
                        {
                            Lady.Fertile = false;
                            descriptor = string.Format("{0}, Aid {1} suffered complications during birth and is unable to have anymore children", Lady.Name, Lady.ActID);
                            Secret_Actor secret = new Secret_Actor(SecretType.Fertility, year, descriptor, 2, Lady.ActID);
                            listOfSecrets.Add(secret);
                            Lady.AddSecret(secret.SecretID);
                        }
                    }
                }
            }
            return child;
        }
            
        


        /// <summary>
        /// takes an optional surname and a sex and returns a full name, eg. 'Edward Stark'
        /// </summary>
        /// <param name="lastName">if not provided a randomly generated surname will be used</param>
        /// <param name="sex"></param>
        /// <returns></returns>
        private string GetActorName(string lastName = null, ActorSex sex = ActorSex.Male, int refID = 0)
        {
            string fullName = null;
            string firstName = null;
            string surname = null;
            int numRecords = 0;
            int index = 0;

            //surname
            if (lastName != null)
            { surname = lastName; }
            else
            {
                //provide a random surname (multiple actors can have the same surname)
                index = rnd.Next(0, listOfSurnames.Count);
                surname = listOfSurnames[index];
            }
            if (sex == ActorSex.Male)
            {
                numRecords = listOfMaleFirstNames.Count;
                index = rnd.Next(0, numRecords);
                firstName = listOfMaleFirstNames[index];
                //check name not already used
                if (refID > 0)
                {
                    House house = Game.world.GetHouse(refID);
                    if (house != null)
                    {
                        //add index ID to list of names
                        int numOfLikeNames = house.AddName(index);
                        if (numOfLikeNames > 1)
                        {
                            //same name repeated - add the 'II', etc., the returned actor name
                            surname += " " + new String('I', numOfLikeNames);
                            Console.WriteLine("- Repeating Name in house {0}: {1} {2}", house.HouseID, firstName, surname);
                        }
                    }
                }
            }
            else if (sex == ActorSex.Female)
            {
                numRecords = listOfFemaleFirstNames.Count;
                index = rnd.Next(0, numRecords);
                firstName = listOfFemaleFirstNames[index];
            }
            fullName = firstName + " " + surname;
            return fullName;
        }

        
        /// <summary>
        /// return list of Initial Player Characters
        /// </summary>
        /// <returns>List of Characters</returns>
        internal List<Active> GetActiveActors()
        { return listOfActiveActors; }

        /// <summary>
        ///return list of Great Houses (one house for each houseID)
        /// </summary>
        /// <returns></returns>
        internal List<MajorHouse> GetGreatHouses()
        { return listOfGreatHouses; }

        /// <summary>
        /// return list of Minor Houses
        /// </summary>
        /// <returns></returns>
        internal List<House> GetMinorHouses()
        { return listOfMinorHouses; }

        internal List<GeoCluster> GetGeoClusters()
        { return listOfGeoClusters; }

        internal List<Skill> GetTraits()
        { return listOfTraits; }

        internal List<Secret> GetSecrets()
        { return listOfSecrets; }

        /// <summary>
        /// returns a Trait from the list of Traits from a provided TraitID
        /// </summary>
        /// <param name="traitID"></param>
        /// <returns></returns>
        private Skill GetTrait(int traitID)
        {
            Skill trait = listOfTraits.Find(x => x.SkillID == traitID);
            return trait;
        }

        /// <summary>
        /// clean up after actor death (or missing & gone from game)
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="year"></param>
        /// <param name="reason"></param>
        internal void RemoveActor(Passive actor, int year, ActorGone reason)
        {
            actor.Gone = year;
            actor.Age = actor.Age - (Game.gameRevolt - year);
            actor.Status = ActorStatus.Gone;
            actor.ReasonGone = reason;
            //remove actor from location
            Location loc = Game.network.GetLocation(actor.LocID);
            loc.RemoveActor(actor.ActID);
            actor.LocID = 0;
        }

        /// <summary>
        /// Determine the starting Rebellion that is the foundation of the Backstory
        /// </summary>
        /// <param name="dictPassiveActors"></param>
        internal void InitialiseOverthrow(Dictionary<int, Passive> dictPassiveActors)
        {
            List<MajorHouse> listOfRoyalists = new List<MajorHouse>();
            List<MajorHouse> listOfRebels = new List<MajorHouse>();
            List<MajorHouse> listOfHousesByPower = new List<MajorHouse>();
            //sorted list of houses by Power (# of locations, largest at the top)
            IEnumerable<MajorHouse> sortedHouses =
                from house in listOfGreatHouses
                let numLocs = house.GetNumBannerLords()
                orderby numLocs descending
                select house;
            listOfHousesByPower = sortedHouses.ToList();
            //seperate into two camps (royalists and rebels)
            bool rebel = true;
            for (int i = 0; i < listOfHousesByPower.Count; i++)
            {
                MajorHouse house = listOfHousesByPower[i];
                if (rebel == false)
                {
                    //Royalist
                    listOfRoyalists.Add(listOfHousesByPower[i]);
                    house.Loyalty_AtStart = KingLoyalty.Old_King;
                    house.Loyalty_Current = KingLoyalty.New_King;
                    rebel = true;
                }
                else
                {
                    //Rebels
                    listOfRebels.Add(listOfHousesByPower[i]);
                    house.Loyalty_AtStart = KingLoyalty.New_King;
                    house.Loyalty_Current = KingLoyalty.New_King;
                    rebel = false;
                }
            }
            //King is the largest Royal Family
            MajorHouse royalHouse = listOfRoyalists[0];
            MajorHouse rebelHouse = listOfRebels[0];
            Game.lore.RoyalRefIDOld = royalHouse.RefID;
            Game.lore.RoyalRefIDNew = rebelHouse.RefID;
            Game.lore.RoyalRefIDCurrent = royalHouse.RefID;
            Game.lore.OldHouseName = royalHouse.Name;
            Game.lore.OldHouseID = royalHouse.HouseID;
            int royalHouseID = royalHouse.HouseID;
            int rebelHouseID = rebelHouse.HouseID;
            //get all actors in Royal House
            List<Passive> listOfRoyals = new List<Passive>();
            IEnumerable<Passive> royalActors =
                from actor in dictPassiveActors
                where actor.Value.HouseID == royalHouseID
                orderby actor.Value.ActID
                select actor.Value;
            listOfRoyals = royalActors.ToList();

            //assign starting loyalties to actors
            for (int i = 0; i < listOfHousesByPower.Count; i++)
            {
                MajorHouse house = listOfHousesByPower[i];
                KingLoyalty loyalty = house.Loyalty_AtStart;
                //get all actors in that house (includes bannerlords)
                List<Passive> listOfActors = new List<Passive>();
                IEnumerable<Passive> tempActors =
                    from actor in dictPassiveActors
                    where actor.Value.HouseID == house.HouseID
                    select actor.Value;
                listOfActors = tempActors.ToList();
                //set loyalty
                foreach (Passive actor in listOfActors)
                { actor.Loyalty_AtStart = loyalty; actor.Loyalty_Current = loyalty; }
            }

            //hive off Royals into separate lists
            List<Passive> listOfRoyalNobles = new List<Passive>();
            List<BannerLord> listOfRoyalBannerLords = new List<BannerLord>();
            List<Knight> listOfRoyalKnights = new List<Knight>();
            List<Advisor> listOfRoyalAdvisors = new List<Advisor>();
            foreach (Passive royal in listOfRoyals)
            {
                if (royal is Noble)
                { listOfRoyalNobles.Add((Noble)royal); }
                else if (royal is BannerLord)
                { listOfRoyalBannerLords.Add((BannerLord)royal); }
                else if (royal is Knight)
                { listOfRoyalKnights.Add((Knight)royal); }
                else if (royal is Advisor)
                { listOfRoyalAdvisors.Add((Advisor)royal); }
                else
                { Game.SetError(new Error(26, "Invalid Royal in listOfRoyals")); }
            }
            //update lore family list
            if (listOfRoyalNobles.Count > 0)
            { Game.lore.SetListOfOldRoyals(listOfRoyalNobles);}
            else { Game.SetError(new Error(27, "listOfRoyalNobles is empty")); }
            //find key characters
            Noble OldKing = null;
            Noble OldQueen = null;
            Noble OldHeir = null;
            Location kingsKeep = Game.network.GetLocation(1);
            string eventText;
            int yearChanged = Game.gameRevolt;
            foreach(Noble royal in listOfRoyalNobles)
            {
                //change location (all)
                kingsKeep.AddActor(royal.ActID);
                Location oldLoc = Game.network.GetLocation(royal.LocID);
                //avoid removing dead wives or children
                if (royal.Status == ActorStatus.AtLocation)
                { oldLoc.RemoveActor(royal.ActID); }
                royal.LocID = 1;
                Record record = null;
                //specific roles & also handle fate of royal family
                switch (royal.Type)
                {
                    case ActorType.Lord:
                        Game.lore.OldKing = royal;
                        OldKing = royal;
                        OldKing.Office = ActorOffice.King;
                        //fate
                        eventText = string.Format("King {0}, Aid {1}, has been convicted of treason and publicly beheaded, age {2}", royal.Name, royal.ActID, royal.Age);
                        record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Died);
                        RemoveActor(royal, yearChanged, ActorGone.Executed);
                        break;
                    case ActorType.Lady:
                        Game.lore.OldQueen = royal;
                        OldQueen = royal;
                        OldQueen.Office = ActorOffice.Queen;
                        //fate
                        eventText = string.Format("Queen {0}, Aid {1}, has been convicted of treason and publicly beheaded, age {2}", royal.Name, royal.ActID, royal.Age);
                        record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Died);
                        RemoveActor(royal, yearChanged, ActorGone.Executed);
                        break;
                    case ActorType.Heir:
                        Game.lore.OldHeir = royal;
                        OldHeir = royal;
                        //fate
                        eventText = string.Format("Heir {0}, Aid {1}, was spirited away to the Free Cities by unknown actors, age {2}", royal.Name, royal.ActID, royal.Age);
                        record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Conflict);
                        RemoveActor(royal, yearChanged, ActorGone.Missing);
                        //update Player (should be the oldHeir)
                        Player player = (Player)Game.world.GetActiveActor(1);
                        if (player != null)
                        {
                            player.Name = OldHeir.Name;
                            player.Age = Game.gameStart - OldHeir.Born;
                            player.Born = OldHeir.Born;
                            player.Handle = OldHeir.Handle;
                            player.HistoryID = OldHeir.ActID;
                            player.arrayOfSkillID = OldHeir.arrayOfSkillID;
                            player.arrayOfTraitEffects = OldHeir.arrayOfTraitEffects;
                            player.arrayOfTraitNames = OldHeir.arrayOfTraitNames;
                            player.SetSecrets(OldHeir.GetSecrets());
                            player.SetFamily(OldHeir.GetFamily());
                            //manually set touched trait
                            player.Touched = OldHeir.Touched;
                       
                        }
                        else { Game.SetError(new Error(66, "Invalid Player (null)")); }
                        break;
                    case ActorType.lord:
                        royal.Type = ActorType.Prince;
                        if (rnd.Next(100) < Game.constant.GetValue(Global.SIBLING_ESCAPE))
                        {
                            //sibling escapes to the free cities
                            eventText = string.Format("Prince {0}, Aid {1}, was spirited away to the Free Cities by unknown actors, age {2}", royal.Name, royal.ActID, royal.Age);
                            record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Conflict);
                            RemoveActor(royal, yearChanged, ActorGone.Missing);
                        }
                        else
                        {
                            //sibling murdered
                            eventText = string.Format("Prince {0}, Aid {1}, has been butchered on orders of the New King, age {2}", royal.Name, royal.ActID, royal.Age);
                            record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Died);
                            RemoveActor(royal, yearChanged, ActorGone.Murdered);
                        }
                        break;
                    case ActorType.lady:
                        royal.Type = ActorType.Princess;
                        if (rnd.Next(100) < Game.constant.GetValue(Global.SIBLING_ESCAPE))
                        {
                            //sibling escapes to the free cities
                            eventText = string.Format("Princess {0}, Aid {1}, was spirited away to the Free Cities by unknown actors, age {2}", royal.Name, royal.ActID, royal.Age);
                            record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Conflict);
                            RemoveActor(royal, yearChanged, ActorGone.Missing);
                        }
                        else
                        {
                            //sibling murdered
                            eventText = string.Format("Princess {0}, Aid {1}, has been butchered on orders of the New King, age {2}", royal.Name, royal.ActID, royal.Age);
                            record = new Record(eventText, royal.ActID, 1, royal.RefID, yearChanged, HistActorIncident.Died);
                            RemoveActor(royal, yearChanged, ActorGone.Murdered);
                        }
                        break;
                }
                Game.world?.SetRecord(record);
            }
            //Coronation for New king
            if (OldKing == null)
            { Game.SetError(new Error(28, "No King Present")); }
            else
            {
                Record record_1;
                int year = rnd.Next(OldKing.Lordship, Game.gameRevolt);
                int age = OldKing.Age - (Game.gameRevolt - year);
                string descriptor = string.Format("{0}, Aid {1}, crowned as King during a royal coronation ceremony at Kingskeep, age {2}", OldKing.Name, OldKing.ActID, age);
                record_1 = new Record(descriptor, OldKing.ActID, OldKing.LocID, OldKing.RefID, year, HistActorIncident.Coronation);
                Game.world?.SetRecord(record_1);
            }

            //create advisors
            Position pos = Game.map.GetCapital();
            List<Advisor> tempListOfAdvisors = new List<Advisor>();
            Advisor royalSepton = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.High_Septon); tempListOfAdvisors.Add(royalSepton);
            Advisor royalCoin = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Master_of_Coin); tempListOfAdvisors.Add(royalCoin);
            Advisor royalLaw = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Master_of_Laws); tempListOfAdvisors.Add(royalLaw);
            Advisor royalShip = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Master_of_Ships); tempListOfAdvisors.Add(royalShip);
            Advisor royalWhisper = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Master_of_Whisperers); tempListOfAdvisors.Add(royalWhisper);
            Advisor royalHand = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Hand_of_the_King); tempListOfAdvisors.Add(royalHand);
            Advisor royalGuard = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Commander_of_Kings_Guard); tempListOfAdvisors.Add(royalGuard);
            Advisor royalWatch = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, AdvisorRoyal.Commander_of_City_Watch); tempListOfAdvisors.Add(royalWatch);
            
            //get all actors in Rebel House
            List<Passive> listOfRebelActors = new List<Passive>();
            IEnumerable<Passive> rebelActors =
                from actor in dictPassiveActors
                where actor.Value.HouseID == rebelHouseID
                orderby actor.Value.ActID
                select actor.Value;
            listOfRebelActors = rebelActors.ToList();
            //hive off Rebels into separate lists
            List<Passive> listOfRebelNobles = new List<Passive>();
            List<BannerLord> listOfRebelBannerLords = new List<BannerLord>();
            List<Knight> listOfRebelKnights = new List<Knight>();
            List<Advisor> listOfRebelAdvisors = new List<Advisor>();
            foreach (Passive rebelActor in listOfRebelActors)
            {
                if (rebelActor is Noble)
                { listOfRebelNobles.Add((Noble)rebelActor); }
                else if (rebelActor is BannerLord)
                { listOfRebelBannerLords.Add((BannerLord)rebelActor); }
                else if (rebelActor is Knight)
                { listOfRebelKnights.Add((Knight)rebelActor); }
                else if (rebelActor is Advisor)
                { listOfRebelAdvisors.Add((Advisor)rebelActor); }
                else
                { Game.SetError(new Error(26, "Invalid Rebel in listOfRebelActors")); }
            }
            //update lore family list
            if (listOfRoyalNobles.Count > 0)
            { Game.lore.SetListOfNewRoyals(listOfRebelNobles); }
            else { Game.SetError(new Error(27, "listOfRoyalNobles is empty")); }
            //find key characters
            Noble NewKing = null;
            Noble NewQueen = null;
            Noble NewHeir = null;
            foreach (Noble rebelActor in listOfRebelNobles)
            {
                //change location (all)
                if (rebelActor.Status != ActorStatus.Gone)
                {
                    kingsKeep.AddActor(rebelActor.ActID);
                    Location oldLoc = Game.network.GetLocation(rebelActor.LocID);
                    oldLoc.RemoveActor(rebelActor.ActID);
                    rebelActor.LocID = 1;
                }

                //specific roles
                switch (rebelActor.Type)
                {
                    case ActorType.Lord:
                        Game.lore.NewKing = rebelActor;
                        NewKing = rebelActor;
                        NewKing.Office = ActorOffice.King;
                        break;
                    case ActorType.Lady:
                        Game.lore.NewQueen = rebelActor;
                        NewQueen = rebelActor;
                        NewQueen.Office = ActorOffice.Queen;
                        break;
                    case ActorType.Heir:
                        Game.lore.NewHeir = rebelActor;
                        NewHeir = rebelActor;
                        break;
                    case ActorType.lord:
                        rebelActor.Type = ActorType.Prince;
                        break;
                    case ActorType.lady:
                        rebelActor.Type = ActorType.Princess;
                        break;
                }
            }

            //fate of Royal household Knight
            if (listOfRoyalKnights.Count > 0)
            {
                Knight knight = listOfRoyalKnights[0];
                if (knight.Status == ActorStatus.AtLocation)
                {
                    eventText = string.Format("{0}, Aid {1}, was hung upside down and gutted on orders of King {2}", knight.Name, knight.ActID, NewKing.Name);
                    Record record = new Record(eventText, knight.ActID, knight.LocID, knight.RefID, Game.gameRevolt, HistActorIncident.Died);
                    Game.world.SetRecord(record);
                    Game.history.RemoveActor(knight, Game.gameRevolt, ActorGone.Murdered);
                }
            }

            //Generate BackStory
            Game.lore.CreateOldKingBackStory(listOfRoyalists, listOfRebels, listOfWounds);
            Game.lore.CreateRoyalFamilyFate();
            Game.lore.CreateNewMajorHouse(listHousePool, listOfRoyalAdvisors);
            
            //Royal Court housekeeping and fate during transition
            foreach (Advisor advisor in tempListOfAdvisors)
            {
                //original loyalty is to old king
                advisor.Loyalty_AtStart = KingLoyalty.Old_King;
                //add to dictionary and location
                Game.world.SetPassiveActor(advisor);
                //fate of Royal Court
                Advisor courtAdvisor = advisor;
                if (Game.lore.FateOfAdvisor(advisor, NewKing) == true)
                {
                    //advisor dismissed and is replaced
                    AdvisorRoyal courtPosition = advisor.advisorRoyal;
                    Advisor newAdvisor = CreateAdvisor(pos, 1, 9999, 9999, ActorSex.Male, AdvisorNoble.None, courtPosition, 1200);
                    newAdvisor.Loyalty_AtStart = KingLoyalty.New_King;
                    newAdvisor.Loyalty_Current = KingLoyalty.New_King;
                    Game.world.SetPassiveActor(newAdvisor);
                    courtAdvisor = newAdvisor;
                }
                //add to Royal court
                Game.world.SetRoyalCourt(courtAdvisor);
            }

            //change Royal house to that of New King
            Game.lore.RoyalRefIDCurrent = Game.lore.RoyalRefIDNew;
            //replace Royal lands with that of the Replacement Great House

        }


        internal void ReplaceDeadLords(Dictionary<int, House> dictHouses)
        {
            int lordID;
            Console.WriteLine(Environment.NewLine + "--- Dead Lords");
            //loop all houses
            foreach (KeyValuePair<int, House> kvp in dictHouses)
            {
                lordID = kvp.Value.LordID;
                Passive lord = Game.world.GetPassiveActor(lordID);
                if (lord.Status == ActorStatus.Gone)
                { Console.WriteLine("{0} {1}, Aid {2}, is {3} from House {4}", lord.Type, lord.Name, lord.ActID, lord.Gone, kvp.Value.Name); }
            }
        }

        /// <summary>
        /// checks all name files for duplicates
        /// </summary>
        /// <returns></returns>
        internal List<string> GetDuplicatesNames()
        {
            List<string> listOfTempDuplicates = new List<string>();
            List<string> listOfAllDuplicates = new List<string>();
            //player names
            IEnumerable<string> tempDuplicates =
                from name in listOfPlayerNames
                group name by name into grouped
                where grouped.Count() > 1
                select "PlayerNames: " + grouped.Key;
            listOfTempDuplicates = tempDuplicates.ToList();
            listOfAllDuplicates.AddRange(listOfTempDuplicates);
            //male first names
            tempDuplicates =
                from name in listOfMaleFirstNames
                group name by name into grouped
                where grouped.Count() > 1
                select "MaleNames: " + grouped.Key;
            listOfTempDuplicates = tempDuplicates.ToList();
            listOfAllDuplicates.AddRange(listOfTempDuplicates);
            //female first names
            tempDuplicates =
                from name in listOfFemaleFirstNames
                group name by name into grouped
                where grouped.Count() > 1
                select "FemaleNames: " + grouped.Key;
            listOfTempDuplicates = tempDuplicates.ToList();
            listOfAllDuplicates.AddRange(listOfTempDuplicates);
            //surnames
            tempDuplicates =
                from name in listOfSurnames
                group name by name into grouped
                where grouped.Count() > 1
                select "Surnames: " + grouped.Key;
            listOfTempDuplicates = tempDuplicates.ToList();
            listOfAllDuplicates.AddRange(listOfTempDuplicates);
            return listOfAllDuplicates;
        }



        /// <summary>
        /// Generic function for one actor to influence another
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="master"></param>
        /// <param name="trait"></param>
        /// <param name="positiveOnly">if true the influence only applies if the master has a higher trait level than the minion</param>
        internal void SetInfluence(Passive minion, Passive master, SkillType trait, bool positiveOnly = false)
        {
            int minionTrait = minion.GetSkill(trait);
            int masterTrait = master.GetSkill(trait);
            if (masterTrait != minionTrait)
            {
                //trait only affected if master trait > minion trait
                if (masterTrait > minionTrait)
                {
                    int influenceAmount = masterTrait - minionTrait;
                    minion.arrayOfSkillInfluences[(int)trait] = influenceAmount;
                    minion.Influencer = master.ActID;
                    //debug
                    Console.WriteLine("- {0}, Aid {1} has adjusted {2} ({3}) due to {4}'s, Aid {5}, influence", minion.Name, minion.ActID, trait, influenceAmount, master.Name, master.ActID);
                }
                //trait affected if master trait < minion trait and not positiveOnly
                if (masterTrait < minionTrait && positiveOnly == false)
                {
                    int influenceAmount = masterTrait - minionTrait;
                    minion.arrayOfSkillInfluences[(int)trait] = influenceAmount;
                    minion.Influencer = master.ActID;
                    //debug
                    Console.WriteLine("- {0}, Aid {1} has adjusted {2} ({3}) due to {4}'s, Aid {5}, influence", minion.Name, minion.ActID, trait, influenceAmount, master.Name, master.ActID);
                }
            }
            else
            { Console.WriteLine("- {0}, Aid {1} has NO adjusted {2} due to {3}'s, Aid {4}, influence (same value)", minion.Name, minion.ActID, trait, master.Name, master.ActID); }
        }

        /// <summary>
        /// only used by lore backstory as will be out of synch with world dictOfSecrets otherwise
        /// </summary>
        /// <param name="secret"></param>
        public void SetSecret(Secret secret)
        { if (secret != null) { listOfSecrets.Add(secret); } }

        /// <summary>
        /// Takes all current (alive) Passive actors (at time of gameRevolt) and ages them up to the time of gameStart.
        /// </summary>
        public void AgePassiveCharacters(Dictionary<int, Passive> dictPassiveActors)
        {
            int elapsedTime = Game.gameExile;
            //Console.WriteLine(Environment.NewLine + "--- Age all Current Passive Actors");
            foreach(var actor in dictPassiveActors)
            {
                //actor currently alive at time of revolt?
                if (actor.Value.Status != ActorStatus.Gone)
                {
                    actor.Value.Age += elapsedTime;
                    //Console.WriteLine("{0}, Aid {1}, has is now age {2}", actor.Value.Name, actor.Value.ActID, actor.Value.Age);
                }
            }
        }

        //add methods above
    }
}