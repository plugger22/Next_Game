﻿using System;
using System.Collections.Generic;
using Next_Game.Cartographic;

namespace Next_Game
{
    //handles living world for the game (data generated by History.cs at game start)
    public class World
    {
        private List<Move> moveList; //list of characters moving through the world
        private Dictionary<int, Character> dictPlayerCharacters; //list of all characters
        public int GameTurn { get; set; } = 1;

        //default constructor
        public World()
        {
            moveList = new List<Move>();
            dictPlayerCharacters = new Dictionary<int, Character>();
        }

        public void IncrementGameTurn()
        { GameTurn++; }

        public int GetGameTurn()
        { return GameTurn; }

        /// <summary>
        /// Sets up Player characters at the specificied location at start of game
        /// </summary>
        /// <param name="listPlayerCharacters"></param>
        /// <param name="locID"></param>
        internal void InitiatePlayerCharacters(List<Character> listPlayerCharacters, int locID)
        {
            //int locID = 
            //loop list and transfer characters to dictionary
            foreach (Character person in listPlayerCharacters)
            {
                //place characters at Location
                person.CharLocationID = locID;
                person.SetCharacterPosition(Game.map.GetCapital());
                dictPlayerCharacters.Add(person.GetCharacterID(), person);
                //add to Location list of Characters
                Location loc = Game.network.GetLocation(locID);
                loc.AddCharacter(person.GetCharacterID());
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
                if (dictPlayerCharacters.ContainsKey(charID))
                {
                    
                    Character person = dictPlayerCharacters[charID];
                    List<int> party = new List<int>(); //list of charID's of all characters in party
                    party.Add(charID);
                    string name = person.GetCharacterName();
                    int speed = person.CharSpeed;
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
                    int locID_Origin = Game.map.GetLocationID(posOrigin.PosX, posOrigin.PosY);
                    Location loc = Game.network.GetLocation(locID_Origin);
                    if (loc != null)
                    {
                        loc.RemoveCharacter(charID);
                        //create new move object
                        Move moveObject = new Move(path, party, speed, true, this.GameTurn);
                        //insert into moveList
                        moveList.Add(moveObject);
                        //update character status to 'travelling'
                        person.SetCharacterStatus(CharStatus.Travelling);
                        //update characterLocationID (now becomes destination)
                        int locID_Destination = Game.map.GetLocationID(posDestination.PosX, posDestination.PosY);
                        person.CharLocationID = locID_Destination;
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
                    int locID = Game.map.GetLocationID(posDestination.PosX, posDestination.PosY);
                    Location loc = Game.network.GetLocation(locID);
                    List<int> charListMoveObject = new List<int>(moveObject.GetCharacterList());
                    //find location, get list, update for each character
                    if (loc != null)
                    {
                        foreach(int charID in charListMoveObject)
                        {
                            loc.AddCharacter(charID);
                            //find character and update details
                            if (dictPlayerCharacters.ContainsKey(charID))
                            {
                                Character person = new Character();
                                person = dictPlayerCharacters[charID];
                                person.SetCharacterStatus(CharStatus.Location);
                                person.SetCharacterPosition(posDestination);
                                person.CharLocationID = locID;
                                Game.messageLog.Add(person.GetCharacterName() + " has arrived safely at " + loc.LocName, GameTurn);
                            }
                            else
                            { Game.messageLog.Add("Error in World.MoveCharacters(): Character not found", GameTurn); }
                        }
                    }
                    else
                    { Game.messageLog.Add("Error in World.MoveCharacters(): Location not found", GameTurn); }
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
                        if (dictPlayerCharacters.ContainsKey(charID))
                        {
                            Character person = dictPlayerCharacters[charID];
                            person.SetCharacterPosition(pos);
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
            foreach (KeyValuePair<int, Character> pair in dictPlayerCharacters)
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
        public List<string> ShowPlayerCharactersRL(bool locationsOnly = false)
        {
            List<string> listToDisplay = new List<string>();
            listToDisplay.Add($"Day of our Lord {GameTurn}");
            listToDisplay.Add("Player Characters ---");
            int status;
            int locID;
            string locName;
            string locStatus = "who knows?";
            string charString; //overall string
            foreach (KeyValuePair<int, Character> pair in dictPlayerCharacters)
            {
                status = (int)pair.Value.GetCharacterStatus();
                locID = pair.Value.CharLocationID;
                locName = GetLocationName(locID);
                if (status == (int)CharStatus.Location)
                { locStatus = "At " + locName; }
                else if (status == (int)CharStatus.Travelling)
                { locStatus = "Travelling to " + locName; }
                //get location coords
                Location loc = Game.network.GetLocation(locID);
                //only show chosen characters (at Location or not depending on parameter)
                if (locationsOnly == true && status == (int)CharStatus.Location || !locationsOnly)
                {
                    charString = string.Format("ID {0,-2} {1,-15} Status: {2,-40} (Loc {3}:{4})", pair.Key, pair.Value.GetCharacterName(), locStatus, loc.GetPosX(), loc.GetPosY());
                    listToDisplay.Add(charString);
                }
            }
            return listToDisplay;
        }

        /// <summary>
        /// used to display character data when first selected by a # key in main menu
        /// </summary>
        /// <param name="inputConsole"></param>
        /// <param name="consoleDisplay"></param>
        /// <param name="charID"></param>
        public string ShowSelectedCharacter(int charID)
        {
            string returnText = "Character NOT KNOWN";
            //find in dictionary
            if (dictPlayerCharacters.ContainsKey(charID))
            {
                Character person = dictPlayerCharacters[charID];
                Position pos = person.GetCharacterPosition();
                returnText = person.GetCharacterName() + " at ";
                returnText += this.GetLocationName(person.CharLocationID);
                returnText += string.Format(" ({0}:{1}) has been selected", pos.PosX, pos.PosY);
            }
            return returnText;
        }

        /*public void ShowAllLocations()
        {
            Console.WriteLine();
            Console.WriteLine("Locations");
            foreach (KeyValuePair<int, Location> pair in dictLocationsByID)
            { Console.WriteLine("ID {0,-2}   {1,-20}", pair.Key, pair.Value.LocName); }
        }*/

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
            int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
            Location loc = Game.network.GetLocation(locID);
            locName = loc.LocName;
            return locName;
        }

        //straight conversion utility to get LocID from a location Position
        /*public int GetLocationID(Position pos)
        {
            int locID = 0;
            Location loc = new Location();
            //Location display
            if (dictLocationsByPos.ContainsKey(pos))
            {
                loc = dictLocationsByPos[pos];
                locID = loc.LocationID;
            }
            return locID;
        }*/


        /// <summary>
        /// Display Location based on input coords
        /// </summary>
        /// <param name="locID"></param>
        /*public void ShowLocation(int locID)
        {
            Location loc = new Location();
            //Location display
            if (dictLocationsByID.ContainsKey(locID))
            {
                loc = dictLocationsByID[locID];
                loc.PrintStatus();
                List<int> charList = loc.GetCharacterList();
                if (charList.Count > 0)
                {
                    //characters at location
                    Console.WriteLine("Characters at " + loc.LocName + " ---");
                    foreach (int charID in charList)
                    {
                        if (dictPlayerCharacters.ContainsKey(charID))
                        {
                            Character person = new Character();
                            person = dictPlayerCharacters[charID];
                            Console.WriteLine("ID {0}: {1}", person.GetCharacterID(), person.GetCharacterName());
                        }
                        else
                        { Console.WriteLine("unknown ID " + Convert.ToString(charID)); }
                    }
                }
            }
            else
            { Console.WriteLine("Debug: Location {0} doesn't exist in the dictLocations", locID); }
        }*/

        /// <summary>
        /// click on a location to get info
        /// </summary>
        /// <param name="pos"></param>
        internal List<string> ShowLocationRL(int locID)
        {
            List<string> locList = new List<string>();
            //Location display
            if (locID > 0)
            {
                Location loc = Game.network.GetLocation(locID);
                string locDetails = string.Format("Location (ID {0}) {1} ---", loc.LocationID, loc.LocName);
                locList.Add(locDetails);
                if (loc.IsCapital() == true)
                { locList.Add("CAPITAL"); }
                if (loc.Connector == true)
                { locList.Add("CONNECTOR"); }
                //characters at location
                List<int> charList = loc.GetCharacterList();
                if (charList.Count > 0)
                {
                    int row = 3;
                    locList.Add("Characters at " + loc.LocName + " ---");
                    string charDetails;
                    foreach (int charID in charList)
                    {
                        row++;
                        if (dictPlayerCharacters.ContainsKey(charID))
                        {
                            Character person = new Character();
                            person = dictPlayerCharacters[charID];
                            charDetails = string.Format("ID {0}: {1}", person.GetCharacterID(), person.GetCharacterName());
                        }
                        else
                        {   charDetails = string.Format("unknown ID " + Convert.ToString(charID)); }
                        locList.Add(charDetails);
                    }
                }
            }
            else if (locID == 0)
            { locList.Add("ERROR: There is no Location present here"); }
            else
            { locList.Add("ERROR: Please click on the map"); }
            return locList;
        }

        /// <summary>
        /// Select a Player Character from the displayed list
        /// </summary>
        /// <returns>Character ID</returns>
        public int ChooseCharacter()
        {
            Console.WriteLine("Which Character do you want to move (Enter ID #)? ");
            int charID = Convert.ToInt32(Console.ReadLine());
            Character person = new Character();
            //check character exists
            if(dictPlayerCharacters.ContainsKey(charID))
            {
                person = dictPlayerCharacters[charID];
                if (person.GetCharacterStatus() != (int)CharStatus.Location)
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
        public string GetCharacterRL(int charID)
        {
            Character person = new Character();
            string charReturn = "Character doesn't exist!";
            //check character exists
            if (dictPlayerCharacters.ContainsKey(charID))
            {
                person = dictPlayerCharacters[charID];
                charReturn = person.GetCharacterName();
                if (person.GetCharacterStatus() != (int)CharStatus.Location)
                { charReturn += " isn't currently available"; }
                else
                {
                    Position pos = person.GetCharacterPosition();
                    charReturn += " is awaiting your orders at ";
                    charReturn += GetLocationName(person.CharLocationID);
                    charReturn += string.Format(" (loc {0}:{1})", pos.PosX, pos.PosY);
                }
            }
            return charReturn;

        }

        internal Position GetCharacterLocationByPos(int charID)
        {
            Position pos = new Position();
            //find in dictionary
            if (dictPlayerCharacters.ContainsKey(charID))
            {
                Character person = dictPlayerCharacters[charID];
                pos = person.GetCharacterPosition();
            }
            else
            { pos = null; }
            return pos;
        }

        
        public int GetCharacterLocationByID(int charID)
        {
            int locID = 0;
            //find in dictionary
            if (dictPlayerCharacters.ContainsKey(charID))
            {
                Character person = dictPlayerCharacters[charID];
                locID = person.CharLocationID;
            }
            return locID;
        }



        //new Methods above here
    }
}