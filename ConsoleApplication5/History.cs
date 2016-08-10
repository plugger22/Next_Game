using System;
using System.Collections.Generic;
using System.IO;
using Next_Game.Cartographic;

namespace Next_Game
{
    //holds data read in from house.txt. Used for pool of houses.
    struct HouseStruct
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public int Archetype { get; set; }
        public int RefID { get; set; }
        public string Capital { get; set; }

        public void ShowHouseStruct()
        {
            Console.WriteLine();
            Console.WriteLine("Structure House {0} ---", Name);
            Console.WriteLine("Motto: {0}", Motto);
            Console.WriteLine("Banner: {0}", Banner);
            Console.WriteLine("ArchetypeID: {0}", Archetype);
            Console.WriteLine("Seat: {0}", Capital);
        }
    }

    //history class handles living world procedural generation at game start. Once created, data is passed to World for game use.
    //Location data flow: create in Map => Network to generate routes => History to generate names and data => World for current state and future changes
    public class History
    {
        private List<Active> listOfPlayerActors;
        private List<string> listOfActorNames;
        private List<string> listOfMaleFirstNames;
        private List<string> listOfFemaleFirstNames;
        private List<House> listOfGreatHouses;
        private List<House> listOfMinorHouses;
        private List<HouseStruct> listHousePool; //used for text file imports and random choice of houses
        static Random rnd;

        /// <summary>
        ///default constructor
        /// </summary>
        /// <param name="seed"></param>
        public History(int seed)
        {
            rnd = new Random(seed);
            listOfPlayerActors = new List<Active>();
            listOfActorNames = new List<string>();
            listOfMaleFirstNames = new List<string>();
            listOfFemaleFirstNames = new List<string>();
            listOfGreatHouses = new List<House>();
            listOfMinorHouses = new List<House>();
            listHousePool = new List<HouseStruct>(); 
        }

        /// <summary>
        /// Set up history
        /// </summary>
        /// <param name="numHousesRequired">uniqueHouses from Network.InitialiseHouses</param>
        public void InitialiseHistory(int numHousesRequired)
        {
            //
            // read in lists of First Male and Female names ---
            //
            string filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/FirstMale.txt";
            string[] arrayOfCharacterNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            {
                if (arrayOfCharacterNames[i] != "")
                { listOfMaleFirstNames.Add(arrayOfCharacterNames[i]); }
            }
            //female
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/FirstFemale.txt";
            arrayOfCharacterNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            {
                if (arrayOfCharacterNames[i] != "")
                { listOfFemaleFirstNames.Add(arrayOfCharacterNames[i]); }
            }
            //
            // read in Player Names ---
            //
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/PlayerNames.txt";
            arrayOfCharacterNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfCharacterNames.Length; i++)
            { listOfActorNames.Add(arrayOfCharacterNames[i]); }
            //
            //read in house pool for GreatHouses ---
            //
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/GreatHouses.txt";
            string[] arrayOfHouseNames = File.ReadAllLines(filePath);
            Console.WriteLine();
            //Console.WriteLine("--- House Names Import");
            bool newHouse = false;
            int dataCounter = 0; //number of houses
            HouseStruct houseStruct = new HouseStruct();
            string cleanToken;
            string cleanTag;
            for (int i = 0; i < arrayOfHouseNames.Length; i++)
            {
                if (arrayOfHouseNames[i] != "")
                {
                    //set up for a new house
                    if(newHouse == false)
                    {
                        newHouse = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new structure
                        houseStruct = new HouseStruct();
                    }
                    string[] tokens = arrayOfHouseNames[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    //Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                    switch(cleanTag)
                    {
                        case "House":
                            houseStruct.Name = cleanToken;
                            break;
                        case "Motto":
                            houseStruct.Motto = cleanToken;
                            break;
                        case "Banner":
                            houseStruct.Banner = cleanToken;
                            break;
                        case "ArchetypeID":
                            houseStruct.Archetype = Convert.ToInt32(cleanToken);
                            break;
                        case "ReferenceID":
                            houseStruct.RefID = Convert.ToInt32(cleanToken);
                            break;
                        case "Capital":
                            houseStruct.Capital = cleanToken;
                            //last datapoint - save structure to list
                            if (dataCounter > 0)
                            {
                                //add to housePool
                                listHousePool.Add(houseStruct);
                                //debug
                                //houseStruct.ShowHouseStruct();
                            }
                            break;
                    }
                }
                else
                { newHouse = false; }
            }
            //Console.WriteLine();
            Console.WriteLine("{0} Great Houses imported, {1} Houses required", dataCounter, numHousesRequired);
            Console.WriteLine();
            //remove surplus houses from pool
            int count = listHousePool.Count;
            int index = 0;
            while(count > numHousesRequired)
            {
                index = rnd.Next(0, count);
                Console.WriteLine("Great House {0} removed", listHousePool[index].Name);
                listHousePool.RemoveAt(index);
                count = listHousePool.Count;
            }
            Console.WriteLine();
            //loop through structures and initialise House classes
            for(int i = 0; i < listHousePool.Count; i++)
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
                Console.WriteLine("House {0} added to listOfGreatHouses", house.Name);
            }
            //
            // Minor bannerlord Houses ---
            //
            //read in house pool
            filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/MinorHouses.txt";
            arrayOfHouseNames = null;
            arrayOfHouseNames = File.ReadAllLines(filePath);
            //Console.WriteLine();
            //Console.WriteLine("--- Minor House Names Import");
            newHouse = false;
            dataCounter = 0; //number of houses
            listHousePool.Clear();
            //HouseStruct houseStruct = new HouseStruct();
            for (int i = 0; i < arrayOfHouseNames.Length; i++)
            {
                if (arrayOfHouseNames[i] != "")
                {
                    //set up for a new house
                    if (newHouse == false)
                    {
                        newHouse = true;
                        //Console.WriteLine();
                        dataCounter++;
                        //new structure
                        houseStruct = new HouseStruct();
                    }
                    string[] tokens = arrayOfHouseNames[i].Split(':');
                    //strip out leading spaces
                    cleanTag = tokens[0].Trim();
                    cleanToken = tokens[1].Trim();
                    //Console.WriteLine("{0}: {1}", tokens[0], tokens[1]);
                    switch (cleanTag)
                    {
                        case "House":
                            houseStruct.Name = cleanToken;
                            break;
                        case "Motto":
                            houseStruct.Motto = cleanToken;
                            break;
                        case "Banner":
                            houseStruct.Banner = cleanToken;
                            break;
                        case "ArchetypeID":
                            houseStruct.Archetype = Convert.ToInt32(cleanToken);
                            break;
                        case "ReferenceID":
                            houseStruct.RefID = Convert.ToInt32(cleanToken);
                            break;
                        case "Seat":
                            houseStruct.Capital = cleanToken;
                            //last datapoint - save structure to list
                            if (dataCounter > 0)
                            {
                                //add to housePool
                                listHousePool.Add(houseStruct);
                                //debug
                                //houseStruct.ShowHouseStruct();
                            }
                            break;
                    }
                }
                else
                { newHouse = false; }
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
                index = rnd.Next(0, listOfActorNames.Count);
                //get name
                actorName = listOfActorNames[index];
                //delete record in list to prevent duplicate names
                listOfActorNames.RemoveAt(index);
                //new character
                ActorTitle title = ActorTitle.Loyal_Follower;
                //set player as ursuper
                if (i == 0)
                { title = ActorTitle.Ursuper; }
                Active person = new Active(actorName, title);
                listOfPlayerActors.Add(person);
                
            }
        }

        /// <summary>
        /// Initialise Passive Actors at game start (populate world)
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="title"></param>
        /// <param name="pos"></param>
        /// <param name="locID"></param>
        /// <param name="refID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        internal Passive CreatePassiveActor(string lastName, ActorTitle title, Position pos, int locID, int refID, int houseID, ActorSex sex = ActorSex.Male )
        {
            Passive actor = null;
            //get a random first name
            string actorName = GetActorName(lastName, sex);
            //create actor
            if (title == ActorTitle.BannerLord)
            { actor = new BannerLord(actorName, title, sex); actor.Realm = ActorRealm.Head_of_House; }
            else if (title == ActorTitle.Lord)
            { actor = new Family (actorName, title, sex); actor.Realm = ActorRealm.Head_of_Noble_House; }
            else if (title == ActorTitle.Lady)
            { actor = new Family(actorName, title, sex); }
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
            actor.GenID = 1;
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
                actor.MaidenName = Game.world.GetGreatHouseName(wifeHouseID);
                //fertile?
                if (age >= 13 && age <= 40)
                { actor.Fertile = true; }
            }
            //date Lord attained lordship of House
            if (sex == ActorSex.Male)
            {
                int lordshipAge = rnd.Next(20, age - 2);
                actor.Lordship = actor.Born + lordshipAge;
                string descriptor = "unknown";
                if (actor.Title == ActorTitle.Lord)
                { descriptor = string.Format("{0} assumes Lordship of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                else if (actor.Title == ActorTitle.BannerLord)
                { descriptor = string.Format("{0} assumes Lordship, BannerLord of House {1}, age {2}", actor.Name, Game.world.GetGreatHouseName(actor.HouseID), lordshipAge); }
                Record record = new Record(descriptor, actor.ActID, actor.LocID, actor.RefID, actor.Lordship, HistEvent.Lordship);
                Game.world.SetRecord(record);
            }
            //return
            return actor;
        }

        /// <summary>
        /// Game start - Great Family, marry the lord and lady and have kids
        /// </summary>
        internal void CreatePassiveFamily(Passive lord, Passive lady)
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
            string descriptor = string.Format("{0}, age {1}, and {2} (nee {3}, age {4}) married at {5}", 
                lord.Name, lordAgeMarried, lady.Name, lady.MaidenName, ageLadyMarried, Game.world.GetLocationName(lady.LocID));
            Record recordLord = new Record(descriptor, lord.ActID, lord.LocID, lord.RefID, lord.Married, HistEvent.Married);
            recordLord.AddHouse(lady.BornRefID);
            recordLord.AddActor(lady.ActID);
            Game.world.SetRecord(recordLord);
            //add relatives
            lord.AddRelation(lady.ActID, Relation.Wife);
            lady.AddRelation(lord.ActID, Relation.Husband);
            // kids
            CreateChildren(lord, lady);
        }

        /// <summary>
        /// Keep producing children until a limit is reached
        /// </summary>
        /// <param name="lord"></param>
        /// <param name="lady"></param>
        private void CreateChildren(Passive lord, Passive lady)
        {
            //is woman fertile and within age range?
            if (lady.Fertile == true && lady.Age >= 13 && lady.Age <= 40)
            {
                //birthing loop, once every 2 years
                for (int year = lady.Married; year <= Game.gameYear; year += 2)
                {
                    ActorSex sex = ActorSex.Male;
                    //new child (50/50 boy/girl)
                    if (rnd.Next(100) < 50)
                    { sex = ActorSex.Female; }
                    //get a random first name
                    string actorName = GetActorName(Game.world.GetGreatHouseName(lord.HouseID), sex);
                    Passive child = new Family(actorName, ActorTitle.None, sex);
                    child.Age = Game.gameYear - year;
                    child.Born = year;
                    child.LocID = lady.LocID;
                    child.RefID = lady.RefID;
                    child.BornRefID = lady.RefID;
                    child.HouseID = lady.HouseID;
                    child.GenID = 2;
                    //family relations
                    child.AddRelation(lord.ActID, Relation.Father);
                    child.AddRelation(lady.ActID, Relation.Mother);
                    //get Lord's family
                    SortedDictionary<int, Relation> dictTempFamily = lord.GetFamily();
                    //new child is DAUGHTER
                    if (sex == ActorSex.Female)
                    {
                        child.MaidenName = Game.world.GetGreatHouseName(lord.HouseID);
                        child.Fertile = true;
                        child.Title = ActorTitle.lady;
                        //loop list of lord's family
                        foreach(KeyValuePair<int, Relation> kvp in dictTempFamily)
                        {
                            if (kvp.Value == Relation.Son)
                            {
                                Passive son = Game.world.GetPassiveActor(kvp.Key);
                                //relations
                                son.AddRelation(child.ActID, Relation.Sister);
                                child.AddRelation(son.ActID, Relation.Brother);
                            }
                            else if (kvp.Value == Relation.Daughter)
                            {
                                Passive daughter = Game.world.GetPassiveActor(kvp.Key);
                                //relations
                                daughter.AddRelation(child.ActID, Relation.Sister);
                                child.AddRelation(daughter.ActID, Relation.Sister);
                            }
                        }
                        //update parent relations
                        lord.AddRelation(child.ActID, Relation.Daughter);
                        lady.AddRelation(child.ActID, Relation.Daughter);
                    }
                    //new child is SON
                    else if (sex == ActorSex.Male)
                    {
                        int sonCounter = 0;
                        //loop list of lord's family
                        foreach (KeyValuePair<int, Relation> kvp in dictTempFamily)
                        {
                            if (kvp.Value == Relation.Son)
                            {
                                Passive son = Game.world.GetPassiveActor(kvp.Key);
                                //relations
                                son.AddRelation(child.ActID, Relation.Brother);
                                child.AddRelation(son.ActID, Relation.Brother);
                                sonCounter++;
                            }
                            else if (kvp.Value == Relation.Daughter)
                            {
                                Passive daughter = Game.world.GetPassiveActor(kvp.Key);
                                //relations
                                daughter.AddRelation(child.ActID, Relation.Brother);
                                child.AddRelation(daughter.ActID, Relation.Sister);
                            }
                        }
                        //status (males - who is in line to inherit?)
                        if (sonCounter == 0)
                        { child.Title = ActorTitle.Heir; child.InLine = 1; }
                        else
                        { child.Title = ActorTitle.lord;  child.InLine = sonCounter; }
                        //update parent relations
                        lord.AddRelation(child.ActID, Relation.Son);
                        lady.AddRelation(child.ActID, Relation.Son);
                    }
                    //add to dictionaries
                    Game.world.SetPassiveActor(child);
                    //store at location
                    Location loc = Game.network.GetLocation(lord.LocID);
                    loc.AddActor(child.ActID);
                    //record event
                    string descriptor = string.Format("{0}, Aid {1}, born at {2} to {3} {4} and {5} {6}",
                        child.Name, child.ActID, Game.world.GetLocationName(lady.LocID), lord.Title, lord.Name, lady.Title, lady.Name );
                    Record record_0 = new Record(descriptor, child.ActID, child.LocID, child.RefID, year, HistEvent.Born);
                    record_0.AddActor(lord.ActID);
                    record_0.AddActor(lady.ActID);
                    Game.world.SetRecord(record_0);
                    //over age?
                    if (Game.gameYear - year > 40)
                        { lady.Fertile = false; break; }
                    else
                    {
                        int num = rnd.Next(100);
                        if (num < 10)
                        {
                            //mother died at childbirth (10%) but child survived
                            PassiveActorFuneral(lady, year, ActorDied.Childbirth, child);
                            /*lady.ReasonDied = ActorDied.Childbirth;
                            lady.Died = year;
                            lady.Age = lady.Age - (Game.gameYear - year);
                            lady.Status = ActorStatus.Dead;
                            descriptor = string.Format("{0}, Aid {1}, died while giving birth to {2}, age {3}", lady.Name, lady.ActID, child.Name, lady.Age);
                            Record record_1 = new Record(descriptor, lady.ActID, lady.LocID, lady.RefID, year, HistEvent.Died);
                            record_1.AddEvent(HistEvent.Birthing);
                            record_1.AddActor(child.ActID);
                            Game.world.SetRecord(record_1);
                            //remove actor from location
                            Location loc_1 = Game.network.GetLocation(lady.LocID);
                            loc_1.RemoveActor(lady.ActID);*/
                            break;
                        }
                        else if (num < 30)
                        {
                            //can no longer have children
                            lady.Fertile = false;
                            descriptor = string.Format("{0} suffered complications while giving birth to {1} and can no longer have children", lady.Name, child.Name);
                            Record record_2 = new Record(descriptor, lady.ActID, lady.LocID, lady.RefID, year, HistEvent.Birthing);
                            Game.world.SetRecord(record_2);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// takes a surname and a sex and returns a full name, eg. 'Edward Stark'
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        private string GetActorName(string lastName, ActorSex sex)
        {
            string fullName = null;
            List<string> listOfNames = new List<string>();
            int numRecords = 0;
            if (sex == ActorSex.Male)
            {
                numRecords = listOfMaleFirstNames.Count;
                int index = rnd.Next(0, numRecords);
                fullName = listOfMaleFirstNames[index] + " " + lastName;
            }
            else
            {
                numRecords = listOfFemaleFirstNames.Count;
                int index = rnd.Next(0, numRecords);
                fullName = listOfFemaleFirstNames[index] + " " + lastName;
            }
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

        /// <summary>
        /// Call this method whenever an NPC actor dies to handle all the housekeeping
        /// </summary>
        /// <param name="deceased"></param>
        /// <param name="year"></param>
        /// <param name="reason"></param>
        /// <param name="perpetrator">The Actor who caused the death, if any</param>
        internal void PassiveActorFuneral(Passive deceased, int year, ActorDied reason, Actor perpetrator = null)
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