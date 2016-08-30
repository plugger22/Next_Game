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
        private List<House> listOfGreatHouses;
        private List<House> listOfMinorHouses;
        private List<HouseStruct> listHousePool; //used for text file imports and random choice of houses
        //geo names
        private List<GeoCluster> listOfGeoClusters;
        private string[][] arrayOfGeoNames;
        //traits
        private List<Trait> listOfTraits; //main
        private Trait[,][] arrayOfTraits; //filtered sets for fast random access
        //secrets
        private List<Secret> listOfSecrets;
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
            listOfGreatHouses = new List<House>();
            listOfMinorHouses = new List<House>();
            listHousePool = new List<HouseStruct>();
            listOfGeoClusters = new List<GeoCluster>();
            arrayOfGeoNames = new string[(int)GeoType.Count][];
            listOfTraits = new List<Trait>();
            arrayOfTraits = new Trait[(int)TraitType.Count, (int)ActorSex.Count][];
            listOfSecrets = new List<Secret>();
        }

        /// <summary>
        /// Set up history by importing text files and initilaising various data collections
        /// </summary>
        /// <param name="numHousesRequired">uniqueHouses from Network.InitialiseHouses</param>
        public void InitialiseHistory(int numHousesRequired)
        {
            // First Male and Female names
            listOfMaleFirstNames.AddRange(Game.file.GetNames("FirstMale.txt"));
            listOfFemaleFirstNames.AddRange(Game.file.GetNames("FirstFemale.txt"));
            listOfPlayerNames.AddRange(Game.file.GetNames("PlayerNames.txt"));
            listOfSurnames.AddRange(Game.file.GetNames("Surnames.txt"));
            //Major houses
            listHousePool.AddRange(Game.file.GetHouses("MajorHouses.txt"));
            InitialiseMajorHouses(numHousesRequired);
            //Minor houses, run AFTER major houses
            listHousePool.Clear();
            listHousePool.AddRange(Game.file.GetHouses("MinorHouses.txt"));
            //GeoNames
            arrayOfGeoNames = Game.file.GetGeoNames("GeoNames.txt");
            InitialiseGeoClusters();
            //Constants
            Game.file.GetConstants("Constants.txt");
            //Traits
            listOfTraits?.AddRange(Game.file.GetTraits("Traits_All.txt", TraitSex.All));
            listOfTraits?.AddRange(Game.file.GetTraits("Traits_Male.txt", TraitSex.Male));
            listOfTraits?.AddRange(Game.file.GetTraits("Traits_Female.txt", TraitSex.Female));
            //set up filtered sets of traits ready for random access by newly created actors
            InitialiseTraits();
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
                listHousePool.RemoveAt(index);
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
                house.ArchetypeID = listHousePool[i].Archetype;
                house.RefID = listHousePool[i].RefID;
                house.LocName = listHousePool[i].Capital;
                //add house to listOfHouses
                listOfGreatHouses.Add(house);
                //Console.WriteLine("House {0} added to listOfGreatHouses", house.Name);
            }
        }

        /// <summary>
        /// called by Network.UpdateHouses(), it randomly chooses a minor house from list and initiliases it
        /// </summary>
        /// <param name="LocID"></param>
        /// <param name="houseID"></param>
        public void InitialiseMinorHouse(int LocID, int houseID)
        {
            //get random minorhouse
            int index = rnd.Next(0, listHousePool.Count);
            MinorHouse house = new MinorHouse();
            //copy data from House pool structures
            house.Name = listHousePool[index].Name;
            house.Motto = listHousePool[index].Motto;
            house.Banner = listHousePool[index].Banner;
            house.ArchetypeID = listHousePool[index].Archetype;
            house.LocName = listHousePool[index].Capital;
            house.RefID = listHousePool[index].RefID;
            house.LocID = LocID;
            house.HouseID = houseID;
            //add house to listOfHouses
            listOfMinorHouses.Add(house);
            //remove minorhouse from pool list to avoid being chosen again
            listHousePool.RemoveAt(index);
            //update location details
            Location loc = Game.network.GetLocation(LocID);
            loc.LocName = house.LocName;
            loc.HouseRefID = house.RefID;
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
                    switch (cluster.ClusterType)
                    {
                        case Cluster.Sea:
                            if (cluster.Size == 1)
                            { tempList = listOfSmallSeas; }
                            else if (cluster.Size >= 20)
                            { tempList = listOfLargeSeas; }
                            else
                            { tempList = listOfMediumSeas; }
                            break;
                        case Cluster.Mountain:
                            if (cluster.Size == 1)
                            { tempList = listOfSmallMountains; }
                            else if (cluster.Size >= 10)
                            { tempList = listOfLargeMountains; }
                            else
                            { tempList = listOfMediumMountains; }
                            break;
                        case Cluster.Forest:
                            if (cluster.Size == 1)
                            { tempList = listOfSmallForests; }
                            else if (cluster.Size >= 10)
                            { tempList = listOfLargeForests; }
                            else
                            { tempList = listOfMediumForests; }
                            break;

                    }
                    //choose a random name
                    randomNum = rnd.Next(0, tempList.Count);
                    cluster.Name = tempList[randomNum];
                    //delete from list to prevent reuse
                    tempList.RemoveAt(randomNum);
                }
            }
        }


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
                int index;
                index = rnd.Next(0, listOfPlayerNames.Count);
                //get name
                actorName = listOfPlayerNames[index];
                //delete record in list to prevent duplicate names
                listOfPlayerNames.RemoveAt(index);
                //new character
                ActorType type = ActorType.Loyal_Follower;
                Active person = null;
                //set player as ursuper
                if (i == 0)
                {
                    type = ActorType.Ursuper;
                    person = new Player(actorName, type) as Player;
                }
                else
                { person = new Active(actorName, type); }
                listOfPlayerActors.Add(person);

            }
        }

        /// <summary>
        /// Initialise Passive Actors at game start (populate world) - Nobles, Bannerlords only
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        internal Passive CreateHouseActor(string lastName, ActorType type, Position pos, int locID, int refID, int houseID, ActorSex sex = ActorSex.Male, WifeStatus wifeStatus = WifeStatus.None)
        {
            //get a random first name
            string actorName = GetActorName(lastName, sex, refID);

            Passive actor = null;
            //Lord or lady
            if (type == ActorType.Lady || type == ActorType.Lord)
            { actor = new Noble(actorName, type, sex); actor.Realm = ActorRealm.Head_of_House; }
            //bannerlord
            else if (type == ActorType.BannerLord)
            { actor = new BannerLord(actorName, type, sex); actor.Realm = ActorRealm.Head_of_House; }
            //illegal actor type
            else
            { Game.SetError(new Error(8, "invalid ActorType")); }
            //age (older men, younger wives
            int age = 0;
            if (sex == ActorSex.Male)
            { age = rnd.Next(25, 60); }
            else
            { age = rnd.Next(14, 35); }
            actor.Born = Game.gameYear - age;
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
            //house at birth (males the same, females from an adjacent house)
            actor.BornRefID = refID;
            int wifeHouseID = refID;
            int highestHouseID = listOfGreatHouses.Count;
            if (sex == ActorSex.Female && highestHouseID >= 2)
            {
                if (rnd.Next(100) < 50)
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
                actor.BornRefID = bornRefID;
                if (actor is Noble)
                {
                    Noble noble = actor as Noble;
                    noble.MaidenName = Game.world.GetGreatHouseName(wifeHouseID);
                    //fertile?
                    if (age >= 13 && age <= 40)
                    { noble.Fertile = true; }
                }
            }
            //date Noble Lord attained lordship of House
            if (sex == ActorSex.Male && actor is Noble)
            {
                Noble noble = actor as Noble;
                int lordshipAge = rnd.Next(20, age - 2);
                noble.Lordship = actor.Born + lordshipAge;
                string descriptor = "unknown";
                if (actor.Type == ActorType.Lord)
                { descriptor = string.Format("{0} assumes Lordship of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                else if (actor.Type == ActorType.BannerLord)
                { descriptor = string.Format("{0} assumes Lordship, BannerLord of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                Record record = new Record(descriptor, actor.ActID, actor.LocID, actor.RefID, noble.Lordship, HistEvent.Lordship);
                Game.world.SetRecord(record);
            }
            //date Bannerlord attained lordship of House
            else if (sex == ActorSex.Male && actor is BannerLord)
            {
                BannerLord bannerlord = actor as BannerLord;
                int lordshipAge = rnd.Next(20, age - 2);
                bannerlord.Lordship = actor.Born + lordshipAge;
                string descriptor = string.Format("{0} assumes Lordship, BannerLord of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge);
                Record record = new Record(descriptor, actor.ActID, actor.LocID, actor.RefID, bannerlord.Lordship, HistEvent.Lordship);
                Game.world.SetRecord(record);
            }
            //assign traits
            InitialiseActorTraits(actor);
            return actor;
        }


        /// <summary>
        /// Create a Knight
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
            knight.Born = Game.gameYear - age;
            knight.Age = age;
            //when knighted (age 18 - 27)
            int knighted = rnd.Next(18, 28);
            knight.Knighthood = Game.gameYear - (age - knighted);

            //data
            knight.SetActorPosition(pos);
            knight.LocID = locID;
            knight.RefID = refID;
            knight.HouseID = houseID;
            knight.Type = ActorType.Knight;
            knight.Realm = ActorRealm.None;
            InitialiseActorTraits(knight, null, null, TraitType.Combat, TraitType.Treachery);
            //record
            string descriptor = string.Format("{0} knighted and swears allegiance to House {1}, age {2}", knight.Name, Game.world.GetGreatHouseName(knight.HouseID), knighted);
            Record record = new Record(descriptor, knight.ActID, knight.LocID, knight.RefID, knight.Knighthood, HistEvent.Knighthood);
            Game.world.SetRecord(record);
            return knight;
        }

        /// <summary>
        /// Create a Royal or Noble House Advisor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="houseID"></param>
        /// <param name="advisorNoble"></param>
        /// <param name="advisorRoyal"></param>
        /// <param name="advisorReligious"></param>
        /// <returns></returns>
        internal Advisor CreateAdvisor(Position pos, int locID, int refID, int houseID, ActorSex sex = ActorSex.Male,
            AdvisorNoble advisorNoble = AdvisorNoble.None, AdvisorRoyal advisorRoyal = AdvisorRoyal.None, AdvisorReligious advisorReligious = AdvisorReligious.None )
        {
            int advisorCheck = (int)advisorRoyal + (int)advisorNoble + (int)advisorReligious;
            //must be one type of advisor
            if (advisorCheck == 3)
            { Game.SetError(new Error(11, "No valid advisor type provided")); return null;  }
            //can only be a single type of advisor
            else if (advisorCheck > 2)
            { Game.SetError(new Error(12, "Only a single advisor type is allowed")); return null; }
            else
            {
                string name = GetActorName();
                Advisor advisor = new Advisor(name, ActorType.Advisor, locID, sex);
                advisor.SetActorPosition(pos);
                advisor.LocID = locID;
                advisor.RefID = refID;
                advisor.HouseID = houseID;
                TraitType positiveTrait = TraitType.None;
                TraitType negativeTrait = TraitType.None;
                if ((int)advisorRoyal > 0)
                {
                    advisor.advisorRoyal = advisorRoyal;
                    negativeTrait = TraitType.Combat;
                    if (advisorRoyal == AdvisorRoyal.Master_of_Whisperers) { positiveTrait = TraitType.Treachery; }
                    else { positiveTrait = TraitType.Wits;  }
                }
                else if ((int)advisorNoble > 0)
                {
                    advisor.advisorNoble = advisorNoble;
                    if (advisorNoble == AdvisorNoble.Castellan) { positiveTrait = TraitType.Leadership; negativeTrait = TraitType.Treachery; }
                    else { positiveTrait = TraitType.Wits; negativeTrait = TraitType.Combat; }
                }
                else if ((int)advisorReligious > 0)
                {
                    advisor.advisorReligious = advisorReligious;
                    positiveTrait = TraitType.Charm; negativeTrait = TraitType.Treachery;
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
        private void InitialiseActorTraits(Actor person, Passive father = null, Passive mother = null, TraitType traitPositive = TraitType.None, TraitType traitNegative = TraitType.None)
        {
            //nicknames from all assigned traits kept here and one is randomly chosen to be given the actor (their 'handle')
            List<string> tempHandles = new List<string>();
            bool needRandomTrait = true;
            int rndRange;
            int startRange = 0; //used for random selection of traits
            int endRange = 0;
            Trait rndTrait;
            int chanceOfTrait;
            
            //Combat ---
            if (person.Age == 0 && father != null && mother != null)
            {
                //parental combat trait (if any)
                int traitID = 0;
                Passive parent = new Passive();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a combat trait?
                traitID = parent.arrayOfTraitID[(int)TraitType.Combat];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Trait trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            TraitAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)TraitType.Combat] = parent.arrayOfTraitEffects[(int)age, (int)TraitType.Combat];
                            person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Combat] = parent.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Combat];
                            person.arrayOfTraitID[(int)TraitType.Combat] = parent.arrayOfTraitID[(int)TraitType.Combat];
                            person.arrayOfTraitNames[(int)TraitType.Combat] = parent.arrayOfTraitNames[(int)TraitType.Combat];
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
                rndRange = arrayOfTraits[(int)TraitType.Combat, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == TraitType.Combat) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == TraitType.Combat) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    
                    rndTrait = arrayOfTraits[(int)TraitType.Combat, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        int effect = rndTrait.Effect;
                        int traitID = rndTrait.TraitID;
                        TraitAge age = rndTrait.Age;
                        //Console.WriteLine("{0}, ID {1} Effect {2} Actor {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfTraitID[(int)TraitType.Combat] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)TraitType.Combat] = effect;
                        person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Combat] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)TraitType.Combat] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Wits ---
            if (person.Age == 0 && father != null && mother != null)
            {
                //parental wits trait (if any)
                int traitID = 0;
                Passive parent = new Passive();
                //genetics can apply (sons have a chance to Inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male)
                { parent = father; }
                else { parent = mother; }
                //does parent have a Wits trait?
                traitID = parent.arrayOfTraitID[(int)TraitType.Wits];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Trait trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            TraitAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)TraitType.Wits] = parent.arrayOfTraitEffects[(int)age, (int)TraitType.Wits];
                            person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Wits] = parent.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Wits];
                            person.arrayOfTraitID[(int)TraitType.Wits] = parent.arrayOfTraitID[(int)TraitType.Wits];
                            person.arrayOfTraitNames[(int)TraitType.Wits] = parent.arrayOfTraitNames[(int)TraitType.Wits];
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
                rndRange = arrayOfTraits[(int)TraitType.Wits, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == TraitType.Wits) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == TraitType.Wits) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)TraitType.Wits, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        int effect = rndTrait.Effect;
                        int traitID = rndTrait.TraitID;
                        TraitAge age = rndTrait.Age;
                        //Console.WriteLine("Wits {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfTraitID[(int)TraitType.Wits] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)TraitType.Wits] = effect;
                        person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Wits] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)TraitType.Wits] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Charm ---
            if (person.Age == 0 && father != null && mother != null)
            {
                //parental Charm trait (if any)
                int traitID = 0;
                Passive parent = new Passive();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Charm trait?
                traitID = parent.arrayOfTraitID[(int)TraitType.Charm];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Trait trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            TraitAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)TraitType.Charm] = parent.arrayOfTraitEffects[(int)age, (int)TraitType.Charm];
                            person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Charm] = parent.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Charm];
                            person.arrayOfTraitID[(int)TraitType.Charm] = parent.arrayOfTraitID[(int)TraitType.Charm];
                            person.arrayOfTraitNames[(int)TraitType.Charm] = parent.arrayOfTraitNames[(int)TraitType.Charm];
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
                rndRange = arrayOfTraits[(int)TraitType.Charm, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == TraitType.Charm) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == TraitType.Charm) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)TraitType.Charm, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        int effect = rndTrait.Effect;
                        int traitID = rndTrait.TraitID;
                        TraitAge age = rndTrait.Age;
                        //Console.WriteLine("Charm {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfTraitID[(int)TraitType.Charm] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)TraitType.Charm] = effect;
                        person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Charm] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)TraitType.Charm] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Treachery ---
            if (person.Age == 0 && father != null && mother != null)
            {
                //parental Treachery trait (if any)
                int traitID = 0;
                Passive parent = new Passive();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Treachery trait?
                traitID = parent.arrayOfTraitID[(int)TraitType.Treachery];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Trait trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            TraitAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)TraitType.Treachery] = parent.arrayOfTraitEffects[(int)age, (int)TraitType.Treachery];
                            person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Treachery] = parent.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Treachery];
                            person.arrayOfTraitID[(int)TraitType.Treachery] = parent.arrayOfTraitID[(int)TraitType.Treachery];
                            person.arrayOfTraitNames[(int)TraitType.Treachery] = parent.arrayOfTraitNames[(int)TraitType.Treachery];
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
                rndRange = arrayOfTraits[(int)TraitType.Treachery, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == TraitType.Treachery) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == TraitType.Treachery) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }
                
                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)TraitType.Treachery, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        int effect = rndTrait.Effect;
                        int traitID = rndTrait.TraitID;
                        TraitAge age = rndTrait.Age;
                        //Console.WriteLine("{0}, ID {1} Effect {2} Actor {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfTraitID[(int)TraitType.Treachery] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)TraitType.Treachery] = effect;
                        person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Treachery] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)TraitType.Treachery] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
            }

            //Leadership ---
            if (person.Age == 0 && father != null && mother != null)
            {
                //parental Leadership trait (if any)
                int traitID = 0;
                Passive parent = new Passive();
                //genetics can apply (sons have a chance to inherit father's trait, daughters from their mothers)
                if (person.Sex == ActorSex.Male) { parent = father; }
                else { parent = mother; }
                //does parent have a Leadership trait?
                traitID = parent.arrayOfTraitID[(int)TraitType.Leadership];
                if (traitID != 0)
                {
                    //random % of trait being passed on
                    if (rnd.Next(100) < Game.constant.GetValue(Global.INHERIT_TRAIT))
                    {
                        //find trait
                        Trait trait = GetTrait(traitID);
                        if (trait != null)
                        {
                            //same trait passed on
                            TraitAge age = trait.Age;
                            person.arrayOfTraitEffects[(int)age, (int)TraitType.Leadership] = parent.arrayOfTraitEffects[(int)age, (int)TraitType.Leadership];
                            person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Leadership] = parent.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Leadership];
                            person.arrayOfTraitID[(int)TraitType.Leadership] = parent.arrayOfTraitID[(int)TraitType.Leadership];
                            person.arrayOfTraitNames[(int)TraitType.Leadership] = parent.arrayOfTraitNames[(int)TraitType.Leadership];
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
                rndRange = arrayOfTraits[(int)TraitType.Leadership, (int)person.Sex].Length;
                //random trait (if a preferred trait choice from top half of traits which are mostly the positive ones)
                if (traitPositive == TraitType.Leadership) { startRange = 0; endRange = rndRange / 2; }
                else if (traitNegative == TraitType.Leadership) { startRange = rndRange / 2; endRange = rndRange; }
                else { startRange = 0; endRange = rndRange; }

                if (rndRange > 0)
                {
                    rndTrait = arrayOfTraits[(int)TraitType.Leadership, (int)person.Sex][rnd.Next(startRange, endRange)];
                    //trait roll (trait only assigned if passes roll, otherwise no trait)
                    chanceOfTrait = rndTrait.Chance;
                    if (rnd.Next(100) < chanceOfTrait)
                    {
                        string name = rndTrait.Name;
                        int effect = rndTrait.Effect;
                        int traitID = rndTrait.TraitID;
                        TraitAge age = rndTrait.Age;
                        //Console.WriteLine("Leadership {0}, ID {1} Effect {2} Actor ID {3} {4}", name, traitID, effect, person.ActID, person.Sex);
                        //update trait arrays
                        person.arrayOfTraitID[(int)TraitType.Leadership] = traitID;
                        person.arrayOfTraitEffects[(int)age, (int)TraitType.Leadership] = effect;
                        person.arrayOfTraitEffects[(int)TraitAge.Fifteen, (int)TraitType.Leadership] = effect; //any age 5 effect also needs to set for age 15
                        person.arrayOfTraitNames[(int)TraitType.Leadership] = name;
                        tempHandles.AddRange(rndTrait.GetNickNames());
                    }
                }
                else
                { Game.SetError(new Error(13, "Invalid Range")); }
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
            IEnumerable<Trait> enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Combat && trait.Sex != TraitSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Combat, (int)ActorSex.Male] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Combat, (int)ActorSex.Male] = enumTraits.ToArray();
            //Combat female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Combat && trait.Sex != TraitSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Combat, (int)ActorSex.Female] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Combat, (int)ActorSex.Female] = enumTraits.ToArray();

            //Wits male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Wits && trait.Sex != TraitSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Wits, (int)ActorSex.Male] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Wits, (int)ActorSex.Male] = enumTraits.ToArray();
            //Wits female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Wits && trait.Sex != TraitSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Wits, (int)ActorSex.Female] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Wits, (int)ActorSex.Female] = enumTraits.ToArray();

            //Charm male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Charm && trait.Sex != TraitSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Charm, (int)ActorSex.Male] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Charm, (int)ActorSex.Male] = enumTraits.ToArray();
            //Charm female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Charm && trait.Sex != TraitSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Charm, (int)ActorSex.Female] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Charm, (int)ActorSex.Female] = enumTraits.ToArray();

            //Treachery male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Treachery && trait.Sex != TraitSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Treachery, (int)ActorSex.Male] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Treachery, (int)ActorSex.Male] = enumTraits.ToArray();
            //Treachery female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Treachery && trait.Sex != TraitSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Treachery, (int)ActorSex.Female] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Treachery, (int)ActorSex.Female] = enumTraits.ToArray();

            //Leadership male
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Leadership && trait.Sex != TraitSex.Female
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Leadership, (int)ActorSex.Male] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Leadership, (int)ActorSex.Male] = enumTraits.ToArray();
            //Leadership female
            enumTraits =
                from trait in listOfTraits
                where trait.Type == TraitType.Leadership && trait.Sex != TraitSex.Male
                orderby trait.Effect descending
                select trait;
            //drop filtered set into the appropriate array slot
            arrayOfTraits[(int)TraitType.Leadership, (int)ActorSex.Female] = new Trait[enumTraits.Count()];
            arrayOfTraits[(int)TraitType.Leadership, (int)ActorSex.Female] = enumTraits.ToArray();
        }



        /// <summary>
        /// Game start - Great Family, marry the lord and lady and have kids
        /// </summary>
        internal void CreatePassiveFamily(Noble lord, Noble lady)
        {
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
                lord.Name, lordAgeMarried, lady.Name, lady.MaidenName, ageLadyMarried, lady.WifeNumber, Game.world.GetLocationName(lady.LocID));
            Record recordLord = new Record(descriptor, lord.ActID, lord.LocID, lord.RefID, lord.Married, HistEvent.Married);
            recordLord.AddHouse(lady.BornRefID);
            recordLord.AddActor(lady.ActID);
            Game.world.SetRecord(recordLord);
            //add relatives
            lord.AddRelation(lady.ActID, ActorRelation.Wife);
            lady.AddRelation(lord.ActID, ActorRelation.Husband);
            // kids
            CreateStartingChildren(lord, lady);
        }


        /// <summary>
        /// Keep producing children until a limit is reached (game start)
        /// </summary>
        /// <param name="lord"></param>
        /// <param name="lady"></param>
        private void CreateStartingChildren(Noble lord, Noble lady)
        {
            //is woman fertile and within age range?
            if (lady.Fertile == true && lady.Age >= 13 && lady.Age <= 40)
            {
                //birthing loop, once every 2 years
                for (int year = lady.Married; year <= Game.gameYear; year += 2)
                {
                    //chance of a child every 2 years
                    if (rnd.Next(100) < 75)
                    {
                        Noble child = CreateChild(lord, lady, year);
                        if (lady.Status == ActorStatus.Dead || lady.Fertile == false)
                        { break; }
                    }
                    //over age?
                    if (Game.gameYear - year > 40)
                    { lady.Fertile = false; break; }
                }
            }
        }

        /// <summary>
        /// General purpose method to create a new child, born in current year with current generation.
        /// </summary>
        /// <param name="Lord">Provide a Great lord as an official father regardless of status</param>
        /// <param name="Lady">Provide a Great Lady as an official mother regardless of status</param>
        /// <param name="sex">You can specify a type</param>
        /// <param name="parents">Natural,Bastard or Adopted? (Bastard could be sired by either Mother or Father</param>
        /// <returns>Returns child object but this can be ignored unless required</returns>
        internal Noble CreateChild(Noble Lord, Noble Lady, int year, ActorSex childSex = ActorSex.None, ActorParents parents = ActorParents.Normal)
        {
            ActorSex sex;
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
            int age = Game.gameYear - year;
            age = Math.Max(age, 0);
            child.Age = age;
            child.Born = year;
            child.LocID = Lady.LocID;
            child.RefID = Lady.RefID;
            child.BornRefID = Lord.RefID;
            child.HouseID = Lady.HouseID;
            child.GenID = Game.gameGeneration + 1;
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
            string descriptor = string.Format("{0}, Aid {1}, born at {2} to {3} {4} and {5} {6}",
                    child.Name, child.ActID, Game.world.GetLocationName(Lady.LocID), Lord.Type, Lord.Name, Lady.Type, Lady.Name);
            if (child.Parents == ActorParents.Adopted)
            {
                secretText = string.Format("{0}, Aid {1}, adopted at {2} by {3} {4} and {5} {6}",
                    child.Name, child.ActID, Game.world.GetLocationName(Lady.LocID), Lord.Type, Lord.Name, Lady.Type, Lady.Name);
                actualEvent = false;
            }
            else if (child.Parents == ActorParents.Bastard)
            {
                secretText = string.Format("{0}, Aid {1}, a bastard of {2} {3} and {4} {5}",
                    child.Name, child.ActID, Lord.Type, Lord.Name, Lady.Type, Lady.Name);
                actualEvent = false;
            }
            Record record_0 = new Record(descriptor, child.ActID, child.LocID, child.RefID, child.Born, HistEvent.Born, actualEvent);
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
                    if (num < (int)Global.CHILDBIRTH_DEATH)
                    {
                        //Mother died at childbirth but child survived
                        PassiveActorFuneral(Lady, year, ActorDied.Childbirth, child, Lord);
                    }
                    else if (num < (int)Global.CHILDBIRTH_INFERTILE)
                    {
                        //Complications -> Mother can no longer have children
                        Lady.Fertile = false;
                        descriptor = string.Format("{0}, Aid {1} suffered complications while giving birth to {2}", Lady.Name, Lady.ActID, child.Name);
                        Record record_2 = new Record(descriptor, Lady.ActID, Lady.LocID, Lady.RefID, year, HistEvent.Birthing);
                        Game.world.SetRecord(record_2);
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
                            fullName += " " + new String('I', numOfLikeNames);
                            Console.WriteLine("Repeating Name in house {0}", house.HouseID);
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
        internal List<Active> GetPlayerActors()
        { return listOfPlayerActors; }

        /// <summary>
        ///return list of Great Houses (one house for each houseID)
        /// </summary>
        /// <returns></returns>
        internal List<House> GetGreatHouses()
        { return listOfGreatHouses; }

        /// <summary>
        /// return list of Minor Houses
        /// </summary>
        /// <returns></returns>
        internal List<House> GetMinorHouses()
        { return listOfMinorHouses; }

        internal List<GeoCluster> GetGeoClusters()
        { return listOfGeoClusters; }

        internal List<Trait> GetTraits()
        { return listOfTraits; }

        internal List<Secret> GetSecrets()
        { return listOfSecrets; }

        /// <summary>
        /// returns a Trait from the list of Traits from a provided TraitID
        /// </summary>
        /// <param name="traitID"></param>
        /// <returns></returns>
        private Trait GetTrait(int traitID)
        {
            Trait trait = listOfTraits.Find(x => x.TraitID == traitID);
            return trait;
        }

        /// <summary>
        /// Call this method whenever an NPC actor dies to handle all the housekeeping
        /// </summary>
        /// <param name="deceased"></param>
        /// <param name="year"></param>
        /// <param name="reason"></param>
        /// <param name="perpetrator">The Actor who caused the death (optional)</param>
        /// <param name="secondary">An additional actor who is affected by the death (optional)</param>
        internal void PassiveActorFuneral(Passive deceased, int year, ActorDied reason, Actor perpetrator = null, Actor secondary = null)
        {
            deceased.Died = year;
            deceased.Age = deceased.Age - (Game.gameYear - year);
            deceased.Status = ActorStatus.Dead;
            Record record = null;
            switch(reason)
            {
                case ActorDied.Childbirth:
                    deceased.ReasonDied = ActorDied.Childbirth;
                    string descriptor = string.Format("{0}, Aid {1}, died while giving birth to {2}, age {3}", deceased.Name, deceased.ActID, perpetrator.Name, deceased.Age);
                    record = new Record(descriptor, deceased.ActID, deceased.LocID, deceased.RefID, year, HistEvent.Died);
                    record.AddEvent(HistEvent.Birthing);
                    record.AddActor(perpetrator.ActID);
                    record.AddActor(secondary.ActID);
                    break;
            }
            
            Game.world?.SetRecord(record);
            //remove actor from location
            Location loc_1 = Game.network.GetLocation(deceased.LocID);
            loc_1.RemoveActor(deceased.ActID);
        }




        //add methods above
    }
}