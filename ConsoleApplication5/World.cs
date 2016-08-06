﻿using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using System.Linq;
using RLNET;

namespace Next_Game
{
    //handles living world for the game (data generated by History.cs at game start)
    public class World
    {
        private List<Move> moveList; //list of characters moving through the world
        private Dictionary<int, Active> dictActiveActors; //list of all Player controlled actors keyed off actorID
        private Dictionary<int, Passive> dictPassiveActors; //list of all NPC actors keyed of actorID
        private Dictionary<int, Actor> dictAllActors; //list of all Actors keyed of actorID
        private Dictionary<int, MajorHouse> dictGreatHouses; //list of all Greathouses keyed off houseID
        private Dictionary<int, House> dictAllHouses; //list of all houses & special locations keyed off RefID
        private Dictionary<int, int> dictGreatID; //list of Great Houses, unsorted (Key is House ID, value is # of bannerlords)
        private Dictionary<int, int> dictHousePower; // list of Great Houses, Sorted (key is House ID, value is # of bannerlords (power))
        private Dictionary<int, Record> dictRecords; //all historical records in a central collection (key is eventID)
        //public int GameTurn { get; set; } = 1;

        //default constructor
        public World()
        {
            moveList = new List<Move>();
            dictActiveActors = new Dictionary<int, Active>();
            dictPassiveActors = new Dictionary<int, Passive>();
            dictAllActors = new Dictionary<int, Actor>();
            dictGreatHouses = new Dictionary<int, MajorHouse>();
            dictAllHouses = new Dictionary<int, House>();
            dictGreatID = new Dictionary<int, int>();
            dictHousePower = new Dictionary<int, int>();
            dictRecords = new Dictionary<int, Record>();
        }


        /// <summary>
        /// Sets up Player characters at the specificied location at start of game
        /// </summary>
        /// <param name="listPlayerCharacters"></param>
        /// <param name="locID"></param>
        internal void InitiatePlayerActors(List<Active> listPlayerCharacters, int locID)
        {
            //int locID = 
            //loop list and transfer characters to dictionary
            foreach (Active person in listPlayerCharacters)
            {
                //place characters at Location
                person.LocID = locID;
                person.SetActorPosition(Game.map.GetCapital());
                dictActiveActors.Add(person.GetActorID(), person);
                dictAllActors.Add(person.GetActorID(), person);
                //add to Location list of Characters
                Location loc = Game.network.GetLocation(locID);
                loc.AddActor(person.GetActorID());
            }
        }

        /// <summary>
        /// Initiate character Movement (creates a Move object)
        /// </summary>
        /// <param name="charID">Character</param>
        /// <param name="posOrigin"></param>
        /// <param name="posDestination"></param>
        /// <param name="path">sequenced List of Positions to destination</param>
        internal string InitiateMoveCharacters(int charID, Position posOrigin, Position posDestination, List<Position> path)
        {
            string returnText = "Error in World.InitiateMoveCharacters";
            //viable Character & Position?
            if (charID > 0 && posOrigin != null && posDestination != null && path != null)
            {
                //find in dictionary
                if (dictActiveActors.ContainsKey(charID))
                {
                    
                    Active person = dictActiveActors[charID];
                    List<int> party = new List<int>(); //list of charID's of all characters in party
                    party.Add(charID);
                    string name = person.Name;
                    int speed = person.Speed;
                    int distance = path.Count;
                    int time = (distance / speed) + 1; //prevents 0 result
                    string originLocation = GetLocationName(posOrigin);
                    string destinationLocation = GetLocationName(posDestination);
                    returnText = string.Format("It will take {0} days for {1} to travel from {2} to {3}. The Journey has commenced.", time, name, originLocation, destinationLocation);

                    //are you sure?
                    /*Console.WriteLine("Are you sure [Y] Yes, [N] No?");
                    ConsoleKeyInfo cki;
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Y)*/

                    //remove character from current location 
                    int locID_Origin = Game.map.GetMapInfo(MapLayer.LocID, posOrigin.PosX, posOrigin.PosY);
                    Location loc = Game.network.GetLocation(locID_Origin);
                    if (loc != null)
                    {
                        loc.RemoveCharacter(charID);
                        //create new move object
                        Move moveObject = new Move(path, party, speed, true, Game.gameTurn);
                        //insert into moveList
                        moveList.Add(moveObject);
                        //update character status to 'travelling'
                        person.SetActorStatus(ActorStatus.Travelling);
                        //update characterLocationID (now becomes destination)
                        int locID_Destination = Game.map.GetMapInfo(MapLayer.LocID, posDestination.PosX, posDestination.PosY);
                        person.LocID = locID_Destination;
                    }
                    else
                    { returnText = "ERROR: The Journey has been cancelled (world.InitiateMoveCharacters"; }
                }
            }
            return returnText;
        }

        /// <summary>
        /// Handles movement of all Player characters througout world
        /// </summary>
        /// <returns>returns a dictionary of mapMarkers and coordinates for the "Player" mapGrid layer</returns>
        internal Dictionary<Position, int> MoveCharacters()
        {
            //create a dictionary of position and map markers to return (passed up to game thence to map to update mapgrid
            Dictionary<Position, int> dictMapMarkers = new Dictionary<Position, int>();
            //loop moveList. Update each move object - update Character Location ID
            for(int i = 0; i < moveList.Count; i++)
            {
                //move speed clicks down list of positions (ignore locations at present)
                Move moveObject = new Move();
                moveObject = moveList[i];
                if (moveObject.MoveParty() == true)
                {
                    //update location list at destination
                    Position posDestination = moveObject.GetCurrentPosition();
                    int locID = Game.map.GetMapInfo(MapLayer.LocID, posDestination.PosX, posDestination.PosY);
                    Location loc = Game.network.GetLocation(locID);
                    List<int> charListMoveObject = new List<int>(moveObject.GetCharacterList());
                    //find location, get list, update for each character
                    if (loc != null)
                    {
                        foreach(int charID in charListMoveObject)
                        {
                            loc.AddActor(charID);
                            //find character and update details
                            if (dictActiveActors.ContainsKey(charID))
                            {
                                Active person = new Active();
                                person = dictActiveActors[charID];
                                person.SetActorStatus(ActorStatus.AtLocation);
                                person.SetActorPosition(posDestination);
                                person.LocID = locID;
                                Snippet snippet = new Snippet(string.Format(person.Name + " has arrived safely at " + loc.LocName));
                                Game.messageLog.Add(snippet, Game.gameTurn);
                            }
                            else
                            { Game.messageLog.Add(new Snippet("Error in World.MoveCharacters(): Character not found"), Game.gameTurn); }
                        }
                    }
                    else
                    { Game.messageLog.Add(new Snippet("Error in World.MoveCharacters(): Location not found"), Game.gameTurn); }
                    //update Party status to enable deletion of moveObject from list (below)
                    moveObject.SetPartyStatus(PartyStatus.Done);
                }
                else
                //still enroute
                {
                    //update dictionary
                    dictMapMarkers.Add(moveObject.GetCurrentPosition(), moveObject.MapMarker);
                    //update Characters in list (charPos)
                    Position pos = moveObject.GetCurrentPosition();
                    List<int> characterList = new List<int>(moveObject.GetCharacterList());
                    for (int j = 0; j < characterList.Count; j++)
                    {
                        int charID = characterList[j];
                        //find in dictionary
                        if (dictActiveActors.ContainsKey(charID))
                        {
                            Active person = dictActiveActors[charID];
                            person.SetActorPosition(pos);
                        }
                    }
                }
            }
            //need to put player markers into mapGrid player layer for drawing. (return collection to Game which passes it onto map object)
            //need to update move objects, characters (locID), status

            //reverse loop through list of Moveobjects and delete any that are marked as 'Done'
            for(int i = moveList.Count; i > 0; i--)
            {
                if (moveList[i - 1].GetPartyStatus() == PartyStatus.Done)
                { moveList.RemoveAt(i - 1); }
            }
            //pass dictionary of markers back to map object via Game
            return dictMapMarkers;
        }

        /// <summary>
        /// Display Player Character Pool
        /// </summary>
        /// <param name="locationsOnly">If true only show those at Locations, default is show all</param>
        /*public void ShowPlayerCharacters(bool locationsOnly = false)
        {
            Console.WriteLine();
            Console.WriteLine("Player Characters");
            int status;
            int locID;
            string locName;
            string locStatus = "who knows?";
            foreach (KeyValuePair<int, Character> pair in dictActiveActors)
            {
                status = (int)pair.Value.GetCharacterStatus();
                locID = pair.Value.CharLocationID;
                locName = GetLocationName(locID);
                if(status == (int)CharStatus.Location)
                { locStatus = "At " + locName; }
                else if(status == (int)CharStatus.Travelling)
                { locStatus = "Travelling to " + locName; }
                //only show chosen characters (at Location or not depending on parameter)
                if (locationsOnly == true && status == (int)CharStatus.Location || !locationsOnly)
                { Console.WriteLine("ID {0,-2} {1,-15} Status: {2,-20}", pair.Key, pair.Value.GetCharacterName(), locStatus); }
            }            
        }*/

        /// <summary>
        /// Returns a list of characters in string format to pass to InfoChannel to display in multi-Console
        /// </summary>
        /// <returns>List with info on each character a single, sequential, entry in the list</returns>
        /// <param name="locationsOnly">If true only show those at Locations, default is show all</param>
        public List<Snippet> ShowPlayerActorsRL(bool locationsOnly = false)
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            //listToDisplay.Add(new Snippet($"Day of our Lord {GameTurn}", RLColor.Yellow, RLColor.Black));
            listToDisplay.Add(new Snippet("--- Player Characters", RLColor.Brown, RLColor.Black));
            int status;
            int locID;
            string locName;
            string locStatus = "who knows?";
            string charString; //overall string
            foreach (KeyValuePair<int, Active> pair in dictActiveActors)
            {
                status = (int)pair.Value.GetActorStatus();
                locID = pair.Value.LocID;
                locName = GetLocationName(locID);
                if (status == (int)ActorStatus.AtLocation)
                { locStatus = "At " + locName; }
                else if (status == (int)ActorStatus.Travelling)
                { locStatus = "Travelling to " + locName; }
                //get location coords
                Location loc = Game.network.GetLocation(locID);
                //only show chosen characters (at Location or not depending on parameter)
                if (locationsOnly == true && status == (int)ActorStatus.AtLocation || !locationsOnly)
                {
                    charString = string.Format("Aid {0,-2} {1,-25} Status: {2,-40} (Loc {3}:{4})", pair.Key, pair.Value.Name, locStatus, loc.GetPosX(), loc.GetPosY());
                    listToDisplay.Add(new Snippet(charString));
                }
            }
            return listToDisplay;
        }

        /// <summary>
        /// Display a single Actor in detail
        /// </summary>
        /// <param name="ActID"></param>
        /// <returns></returns>
        public List<Snippet> ShowActorRL(int actorID)
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            Actor person = new Actor();
            if (dictAllActors.TryGetValue(actorID, out person))
            {
                int locID = person.LocID;
                string name = string.Format("{0} {1}, Aid {2}", person.Title, person.Name, actorID);
                RLColor color = RLColor.White;
                string locString = "?";
                if (person.GetActorStatus() == (int)ActorStatus.AtLocation)
                { locString = string.Format("Located at {0} {1}, Lid {2}", GetLocationName(locID), ShowLocationCoords(locID), locID );}
                else if (person.GetActorStatus() == (int)ActorStatus.Travelling)
                {
                    Position pos = person.GetActorPosition();
                    locString = string.Format("Currently at {0}:{1}, travelling towards {2} {3}, Lid {4}", pos.PosX, pos.PosY, GetLocationName(locID), ShowLocationCoords(locID), locID);
                }
                listToDisplay.Add(new Snippet(name, RLColor.Yellow, RLColor.Black));
                if ((int)person.Realm > 0)
                { listToDisplay.Add(new Snippet(string.Format("Realm Title: {0}", person.Realm))); }
                if ((int)person.Office > 0)
                { listToDisplay.Add(new Snippet(string.Format("Office: {0}", person.Office), RLColor.Yellow, RLColor.Black)); }
                if (person.Handle != null)
                { listToDisplay.Add(new Snippet(string.Format("\"{0}\"", person.Handle))); }
                listToDisplay.Add(new Snippet(locString));
                //listToDisplay.Add(new Snippet(string.Format("Title: {0}", person.Title)));
                listToDisplay.Add(new Snippet(string.Format("Description: {0}", person.Description)));
                listToDisplay.Add(new Snippet(string.Format("{0} y.o {1}, born {2}", person.Age, person.Sex, person.Born)));
                //personal history
                List<string> actorHistory = GetActorRecords(person.GetActorID());
                if (actorHistory.Count > 0)
                {
                    listToDisplay.Add(new Snippet("Personal History", RLColor.Brown, RLColor.Black));
                    foreach (string text in actorHistory)
                    { listToDisplay.Add(new Snippet(text)); }
                }
            }
            else
            { listToDisplay.Add(new Snippet("No Character with this ID exists", RLColor.Red, RLColor.Black)); }
            
            return listToDisplay;
        }

        /// <summary>
        /// used to display character data when first selected by a # key in main menu
        /// </summary>
        /// <param name="inputConsole"></param>
        /// <param name="consoleDisplay"></param>
        /// <param name="charID"></param>
        public Snippet ShowSelectedActor(int charID)
        {
            string returnText = "Character NOT KNOWN";
            //find in dictionary
            if (dictActiveActors.ContainsKey(charID))
            {
                Actor person = dictActiveActors[charID];
                Position pos = person.GetActorPosition();
                returnText = person.Name + " at ";
                returnText += this.GetLocationName(person.LocID);
                returnText += string.Format(" ({0}:{1}) has been selected", pos.PosX, pos.PosY);
            }
            return new Snippet(returnText);
        }

        public List<Snippet> ShowRecordsRL()
        {
            List<Snippet> listOfHistory = new List<Snippet>();
            listOfHistory.Add(new Snippet("--- All Records", RLColor.Yellow, RLColor.Black));
            foreach(KeyValuePair<int, Record> kvp in dictRecords)
            {
                string text = string.Format("{0} {1}", kvp.Value.Year, kvp.Value.Text);
                listOfHistory.Add(new Snippet(text));
            }
            return listOfHistory;
        }

        internal string GetLocationName(int locID)
        {
            string locName = "unknown";
            Location loc = Game.network.GetLocation(locID);
            locName = loc.LocName;
            return locName;
        }

        internal string GetLocationName(Position pos)
        {
            string locName = "unknown";
            int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
            Location loc = Game.network.GetLocation(locID);
            locName = loc.LocName;
            return locName;
        }

        /// <summary>
        /// Get location coords in a formatted string
        /// </summary>
        /// <param name="locID"></param>
        /// <returns>returns '(loc 20:4)' format string</returns>
        private string ShowLocationCoords(int locID)
        {
            Location loc = Game.network.GetLocation(locID);
            string coords = string.Format("(loc {0}:{1})", loc.GetPosX(), loc.GetPosY());
            return coords;
        }



        /// <summary>
        /// click on a location to get info
        /// </summary>
        /// <param name="pos"></param>
        internal List<Snippet> ShowLocationRL(int locID)
        {
            List<Snippet> locList = new List<Snippet>();
            //Location display
            if (locID > 0)
            {
                string description = "House";
                RLColor color = RLColor.Cyan;
                bool houseCapital = false;
                Location loc = Game.network.GetLocation(locID);
                House house = GetHouse(loc.HouseRefID);
                //if a House Capital show in Yellow
                if(Game.map.GetMapInfo(MapLayer.Capitals, loc.GetPosX(), loc.GetPosY()) > 0 )
                { color = RLColor.Yellow; houseCapital = true; }
                //ignore the capital and special locations for the moment until they are included in dictAllHouses
                if (house != null)
                {
                    locList.Add(new Snippet(string.Format("House {0} of {1}, Lid {2}", house.Name, loc.LocName, loc.LocationID), color, RLColor.Black));
                    locList.Add(new Snippet(string.Format("Motto \"{0}\"", house.Motto)));
                    locList.Add(new Snippet(string.Format("Banner \"{0}\"", house.Banner)));
                    locList.Add(new Snippet(string.Format("Seated at {0} {1}", house.LocName, ShowLocationCoords(locID))));
                }
                
                //correct location description
                if (loc.HouseID == 99)
                { description = "A homely Inn"; }
                else if (loc.LocationID == 1)
                { description = loc.LocName + ": the Home of the King"; }
                else if (Game.map.GetMapInfo(MapLayer.Capitals, loc.GetPosX(), loc.GetPosY()) == 0)
                { description = "Banner Lord of House"; }
                //bannerlord details if applicable
                if (houseCapital == false)
                {
                    string locDetails = string.Format("{0} {1}", description, GetGreatHouseName(loc.HouseID));
                    locList.Add(new Snippet(locDetails));
                }
                
                if (loc.IsCapital() == true)
                { locList.Add(new Snippet("CAPITAL", RLColor.Yellow, RLColor.Black)); }
                if (loc.Connector == true)
                { locList.Add(new Snippet("CONNECTOR", RLColor.Red, RLColor.Black)); }
                //characters at location
                List<int> charList = loc.GetActorList();
                if (charList.Count > 0)
                {
                    int row = 3;
                    locList.Add(new Snippet(string.Format("Characters at {0}", loc.LocName), RLColor.Brown, RLColor.Black));
                    string actorDetails;
                    foreach (int charID in charList)
                    {
                        row++;
                        if (dictAllActors.ContainsKey(charID))
                        {
                            Actor person = new Actor();
                            person = dictAllActors[charID];
                            actorDetails = string.Format("Aid {0} {1} {2}", person.GetActorID(), person.Title, person.Name);
                        }
                        else
                        {   actorDetails = string.Format("unknown ID " + Convert.ToString(charID)); }
                        locList.Add(new Snippet(actorDetails));
                    }
                }
            }
            else if (locID == 0)
            { locList.Add(new Snippet("ERROR: There is no Location present here", RLColor.Red, RLColor.Black)); }
            else
            { locList.Add(new Snippet("ERROR: Please click on the map", RLColor.Red, RLColor.Black)); }
            return locList;
        }

        /// <summary>
        /// Display House data to main infoConsole
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        internal List<Snippet> ShowHouseRL(int houseID)
        {
            MajorHouse majorHouse = GetGreatHouse(houseID) as MajorHouse;
            List<Snippet> houseList = new List<Snippet>();
            if (majorHouse != null)
            {
                houseList.Add(new Snippet("House " + majorHouse.Name, RLColor.Yellow, RLColor.Black));
                string motto = string.Format("Motto \"{0}\"", majorHouse.Motto);
                houseList.Add(new Snippet(motto));
                string banner = string.Format("Banner \"{0}\"", majorHouse.Banner);
                houseList.Add(new Snippet(banner));
                string seat = string.Format("Seated at {0} {1} ", majorHouse.LocName, ShowLocationCoords(majorHouse.LocID));
                houseList.Add(new Snippet(seat));
                //bannerlords
                List<int> listLordLocations = majorHouse.GetLords();
                if (listLordLocations.Count > 0)
                {
                    houseList.Add(new Snippet("BannerLords", RLColor.Yellow, RLColor.Black));
                    string bannerLord;
                    int refID;
                    foreach (int locID in listLordLocations)
                    {
                        Location loc = Game.network.GetLocation(locID);
                        refID = Game.map.GetMapInfo(MapLayer.RefID, loc.GetPosX(), loc.GetPosY());
                        House house = GetHouse(refID);
                        bannerLord = string.Format("House {0} at {1} {2}", house.Name, GetLocationName(locID), ShowLocationCoords(locID));
                        houseList.Add(new Snippet(bannerLord));
                    }
                }
                //house history
                List<string> houseHistory = GetHouseRecords(majorHouse.RefID);
                if (houseHistory.Count > 0)
                {
                    houseList.Add(new Snippet("House History", RLColor.Brown, RLColor.Black));
                    foreach(string text in houseHistory)
                    { houseList.Add(new Snippet(text)); }
                }
            }
            return houseList;
        }

        /// <summary>
        /// Select a Player Character from the displayed list
        /// </summary>
        /// <returns>Character ID</returns>
        public int ChooseCharacter()
        {
            Console.WriteLine("Which Character do you want to move (Enter ID #)? ");
            int charID = Convert.ToInt32(Console.ReadLine());
            Active person = new Active();
            //check character exists
            if(dictActiveActors.ContainsKey(charID))
            {
                person = dictActiveActors[charID];
                if (person.GetActorStatus() != (int)ActorStatus.AtLocation)
                { Console.WriteLine("That Character isn't at a Location and can't be selected"); charID = 0; }
            }
            else
            { Console.WriteLine("Character doesn't exist!"); charID = 0; }
            return charID;
        }

        /// <summary>
        /// returns string showing character name and status (at 'x' loc, travelling)
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public Snippet GetCharacterRL(int charID)
        {
            Actor person = new Actor();
            string charReturn = "Character doesn't exist!";
            //check character exists
            if (dictActiveActors.ContainsKey(charID))
            {
                person = dictActiveActors[charID];
                charReturn = person.Name;
                if (person.GetActorStatus() != (int)ActorStatus.AtLocation)
                { charReturn += " isn't currently available"; }
                else
                {
                    Position pos = person.GetActorPosition();
                    charReturn += " is awaiting your orders at ";
                    charReturn += GetLocationName(person.LocID);
                    charReturn += string.Format(" (loc {0}:{1})", pos.PosX, pos.PosY);
                }
            }
            return new Snippet(charReturn);

        }

        internal Position GetActiveActorLocationByPos(int charID)
        {
            Position pos = new Position();
            //find in dictionary
            if (dictActiveActors.ContainsKey(charID))
            {
                Active person = dictActiveActors[charID];
                pos = person.GetActorPosition();
            }
            else
            { pos = null; }
            return pos;
        }

        
        public int GetActiveActorLocByID(int charID)
        {
            int locID = 0;
            //find in dictionary
            if (dictActiveActors.ContainsKey(charID))
            {
                Active person = dictActiveActors[charID];
                locID = person.LocID;
            }
            return locID;
        }

        /// <summary>
        /// receives list of Houses from Network and places in releveant House Dictionaries for permanent use
        /// </summary>
        /// <param name="listOfHouses"></param>
        internal void InitialiseHouses()
        {
            //great houses
            List<House> listOfGreatHouses = Game.history.GetGreatHouses();
            foreach(MajorHouse house in listOfGreatHouses)
            {
                dictGreatHouses.Add(house.HouseID, house);
                dictAllHouses.Add(house.RefID, house);
                dictGreatID.Add(house.HouseID, house.GetNumBannerLords());
                /*//create Lord and Lady for house
                Location loc = Game.network.GetLocation(house.LocID);
                Position pos = loc.GetPosition();
                Passive actorLord = Game.history.CreatePassiveActor(house.Name, ActorTitle.Lord, pos, house.LocID, house.RefID);
                Passive actorLady = Game.history.CreatePassiveActor(house.Name, ActorTitle.Lady, pos, house.LocID, house.RefID, ActorSex.Female);
                //add to dictionaries of actors
                dictPassiveActors.Add(actorLord.GetActorID(), actorLord);
                dictPassiveActors.Add(actorLady.GetActorID(), actorLady);
                dictAllActors.Add(actorLord.GetActorID(), actorLord);
                dictAllActors.Add(actorLady.GetActorID(), actorLady);
                //create records of being born
                string descriptor = string.Format("{0} born at {1}", actorLord.Name, loc.LocName);
                Record recordLord = new Record(descriptor, actorLord.GetActorID(), loc.LocationID, house.RefID, actorLord.Born, HistEvent.Born);
                dictRecords.Add(recordLord.eventID, recordLord);
                //location born (different for lady)
                House ladyHouse = GetHouse(actorLady.BornRefID);
                Location locLady = Game.network.GetLocation(ladyHouse.LocID);
                descriptor = string.Format("{0} born at {1}", actorLady.Name, locLady.LocName);
                Record recordLady = new Record(descriptor, actorLady.GetActorID(), locLady.LocationID, house.RefID, actorLady.Born, HistEvent.Born);
                dictRecords.Add(recordLady.eventID, recordLady);
                //store actors in location
                loc.AddActor(actorLord.GetActorID());
                loc.AddActor(actorLady.GetActorID());*/
            }
            //populate sorted dictionary (descending) of house ID's by Power (# of BannerLords)
            foreach (KeyValuePair<int, int> kvp in dictGreatID.OrderByDescending(key => key.Value))
            { dictHousePower.Add(kvp.Key, kvp.Value); }
            //minor houses
            List<House> listOfMinorHouses = Game.history.GetMinorHouses();
            foreach (House house in listOfMinorHouses)
            { dictAllHouses.Add(house.RefID, house); }
            //update Map layer for RefID
            int locID = 0;
            int refID = 0;
            foreach (KeyValuePair<int, House> record in dictAllHouses)
            {
                locID = record.Value.LocID;
                refID = record.Value.RefID;
                Location loc = Game.network.GetLocation(locID);
                Game.map.SetMapInfo(MapLayer.RefID, loc.GetPosX(), loc.GetPosY(), refID);
            }
            //fill Great Houses with Lords and Ladies
            foreach (KeyValuePair<int, MajorHouse> kvp in dictGreatHouses)
            {
                //create Lord and Lady for house
                Location loc = Game.network.GetLocation(kvp.Value.LocID);
                Position pos = loc.GetPosition();
                Passive actorLord = Game.history.CreatePassiveActor(kvp.Value.Name, ActorTitle.Lord, pos, kvp.Value.LocID, kvp.Value.RefID, kvp.Value.HouseID);
                Passive actorLady = Game.history.CreatePassiveActor(kvp.Value.Name, ActorTitle.Lady, pos, kvp.Value.LocID, kvp.Value.RefID, kvp.Value.HouseID, ActorSex.Female);
                //add to dictionaries of actors
                dictPassiveActors.Add(actorLord.GetActorID(), actorLord);
                dictPassiveActors.Add(actorLady.GetActorID(), actorLady);
                dictAllActors.Add(actorLord.GetActorID(), actorLord);
                dictAllActors.Add(actorLady.GetActorID(), actorLady);
                //create records of being born
                string descriptor = string.Format("{0} born at {1}", actorLord.Name, loc.LocName);
                Record recordLord = new Record(descriptor, actorLord.GetActorID(), loc.LocationID, kvp.Value.RefID, actorLord.Born, HistEvent.Born);
                SetRecord(recordLord.eventID, recordLord);
                //location born (different for lady)
                House ladyHouse = GetHouse(actorLady.BornRefID);
                Location locLady = Game.network.GetLocation(ladyHouse.LocID);
                descriptor = string.Format("{0} (nee {1}) born at {2}", actorLady.Name, actorLady.MaidenName, locLady.LocName);
                Record recordLady = new Record(descriptor, actorLady.GetActorID(), locLady.LocationID, actorLady.BornRefID, actorLady.Born, HistEvent.Born);
                SetRecord(recordLady.eventID, recordLady);
                Game.history.CreatePassiveFamily(actorLord, actorLady);
                //store actors in location
                loc.AddActor(actorLord.GetActorID());
                loc.AddActor(actorLady.GetActorID());
            }
        }

        /// <summary>
        /// places a message in info panel detailing all relevant data for a single generation
        /// </summary>
        public void ShowGeneratorStatsRL()
        {
            List<Snippet> listStats = new List<Snippet>();
            //calcs
            int numLocs = Game.network.GetNumLocations();
            int numGreatHouses = dictGreatHouses.Count;
            int numSpecialLocs = Game.network.GetNumSpecialLocations();
            int numBannerLords = dictAllHouses.Count - numGreatHouses;
            //data
            listStats.Add(new Snippet("--- Generation Statistics", RLColor.Yellow, RLColor.Black));
            listStats.Add(new Snippet(string.Format("{0} Locations", numLocs )));
            listStats.Add(new Snippet(string.Format("{0} Great Houses", numGreatHouses )));
            listStats.Add(new Snippet(string.Format("{0} Banner Lords", numBannerLords)));
            listStats.Add(new Snippet(string.Format("{0} Special Locations", numSpecialLocs)));
            listStats.Add(new Snippet("1 Capital"));
            //checksum
            if (numLocs != numGreatHouses + numSpecialLocs + numBannerLords + 1)
                listStats.Add(new Snippet("Error: Locations don't tally", RLColor.Red, RLColor.Black));
            //list of all Greathouses by power
            listStats.Add(new Snippet("Great Houses", RLColor.Yellow, RLColor.Black));
            string housePower;
            foreach (KeyValuePair<int, int> kvp in dictHousePower)
            {
                MajorHouse house = GetGreatHouse(kvp.Key);
                housePower = string.Format("Hid {0} House {1} has {2} BannerLords", house.HouseID, house.Name, house.GetNumBannerLords());
                listStats.Add(new Snippet(housePower));
            }
            //display data
            Game.infoChannel.SetInfoList(listStats, ConsoleDisplay.Multi);
        }

        /// <summary>
        /// Quickly access a house name using houseID
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        public string GetGreatHouseName(int houseID)
        {
            string houseName = "";
            MajorHouse house = new MajorHouse();
            if(dictGreatHouses.TryGetValue(houseID, out house))
            { houseName = house.Name; }
            return houseName;
        }

        /// <summary>
        /// Returns Great house if found, otherwise null, keyed of
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        private MajorHouse GetGreatHouse(int houseID)
        {
            MajorHouse house = new MajorHouse();
            if (dictGreatHouses.TryGetValue(houseID, out house))
            { return house; }
            return null;
        }

        /// <summary>
        /// Returns house (any type) if found, otherwise null, keyed off refID)
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        private House GetHouse(int refID)
        {
            House house = new House();
            if (dictAllHouses.TryGetValue(refID, out house))
            { return house; }
            return null;
        }

        /// <summary>
        /// Query to return list of strings containing selected actor's personal history
        /// </summary>
        /// <param name="actorID"></param>
        /// <returns></returns>
        private List<string> GetActorRecords(int actorID)
        {
            List<string> actorRecords = new List<string>();
            //query
            IEnumerable<string> actorHistory =
                from actor in dictRecords
                from actID in actor.Value.listOfActors
                where actID == actorID
                select Convert.ToString(actor.Value.Year + " " + actor.Value.Text);
            //place filtered data into list
            actorRecords = actorHistory.ToList();
            return actorRecords;
        }

        /// <summary>
        /// Query to return list of strings containing selected house's history
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        private List<string> GetHouseRecords(int refID)
        {
            List<string> houseRecords = new List<string>();
            //query
            IEnumerable<string> houseHistory =
                from house in dictRecords
                from _refID in house.Value.listOfHouses
                where _refID == refID
                select Convert.ToString(house.Value.Year + " " + house.Value.Text);
            //place filtered data into list
            houseRecords = houseHistory.ToList();
            return houseRecords;
        }

        public int GetGreatHouseRefID(int houseID)
        {
            MajorHouse house = new MajorHouse();
            if (dictGreatHouses.TryGetValue(houseID, out house))
            { return house.RefID; }
            return 0;
        }

        internal void SetRecord(int eventID, Record record)
        {
            if (record != null && eventID > 0)
            { dictRecords.Add(eventID, record); }
        }

        //new Methods above here
    }
}