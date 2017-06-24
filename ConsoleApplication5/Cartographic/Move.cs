using System;
using System.Collections.Generic;

//Done is triggered on arrival at destination and is used to clear out the moveObject from the list of such
public enum PartyStatus {Active, Delayed, Done};
//public enum PartyVisibility { Visible, Invisible}; //does the party show on the map (visibility is from the Player's point of view)

namespace Next_Game.Cartographic
{
    //class that handles movement of characters/NPC's throughout the world. Move objects are kept in a moveList in World.
    //a new Move object is created everytime somebody embarks on a journey.
    internal class Move
    {
        List<int> characterList; //who is in the party? Stores Character ID's Multiple characters can be in a party.
        List<Position> pathList; //sequence of cells for the route to be followed
        int speed; //speed party travels in cells per turn
        int currentPosIndex; //index for positionList to track parties current position
        int turnDeparted;
        public PartyStatus Status { get; set; }
        //public PartyVisibility Visibility { get; set; } //does the party show on the map?
        public bool PlayerInParty { get; set; } //is a player controlled character in the party? (different color or symbol on map)
        public int MapMarker { get; set; } //# of party on map which is equal to the highest ranked member in party (lowest ID as ID 1 is the player)

        public Move()
        { }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="listPos"></param>
        /// <param name="listParty"></param>
        /// <param name="speed"></param>
        /// <param name="playerInParty"></param>
        /// <param name="turnDeparted"></param>
        /// <param name="visibility">default Visible</param>
        public Move(List<Position> listPos, List<int> listParty, int speed, bool playerInParty, int turnDeparted)
        {
            pathList = new List<Position>(listPos);
            characterList = new List<int>(listParty);
            this.speed = speed;
            Status = PartyStatus.Active;
            currentPosIndex = 0; //how far the party currently is along the path List
            this.PlayerInParty = playerInParty;
            this.turnDeparted = turnDeparted;
            MapMarker = characterList[0];
            //find highest ranked party member by ID (lowest)
            /*if (playerInParty)
            {
                MapMarker = Int32.MaxValue;
                for (int i = 0; i < characterList.Count; i++)
                {
                    if (characterList[i] < MapMarker)
                    { MapMarker = characterList[i]; }
                }
            }
            else
            //TODO need a way of numbering NPC parties
            { MapMarker = 0; }*/
        }

        /// <summary>
        /// Returns true if nominated actor is in Party
        /// </summary>
        /// <returns></returns>
        public bool CheckInParty(int actorID)
        {
            bool status = false;
            //int targetActID = Game.variable.GetValue(GameVar.Inquisitor_Target);
            for(int i = 0; i < characterList.Count; i++)
            {
                if (characterList[i] == actorID) { status = true;  Game.logTurn?.Write($"Actor ActID {actorID} found in MoveObject \"{MapMarker}\""); }
            }
            return status;
        }

        /// <summary>
        /// gives # of turns to reach destination from current position
        /// </summary>
        /// <returns></returns>
        internal int CheckTurnsToDestination()
        { return (pathList.Count - currentPosIndex) / speed; }

        /// <summary>
        /// Moves party at speed clicks down list of Positions, returns True if party at Destination
        /// </summary>
        /// <returns>True if party has reached destination</returns>
        public bool MoveParty()
        {
            bool atDestination = false;
            int tempSpeed = speed;
            int charID;
            //reduce speed by 1 at start to avoid a speed + 1 move
            if (currentPosIndex == 0 && speed > 1)
            { tempSpeed--; }
            //step along path
            for(int i = 0; i < tempSpeed; i++)
            {
                /*if ((currentPosIndex + i + 1) >= pathList.Count)
                {
                    //at destination
                    currentPosIndex = pathList.Count - 1;
                    atDestination = true;
                }
                else
                {
                    //still enroute
                    currentPosIndex++; 
                }*/
                //check for being in the same place as an enemy
                charID = characterList[0];
                Actor person = Game.world.GetAnyActor(charID);
                if (person != null)
                {
                    Position pos = pathList[currentPosIndex];
                    if (pos != null)
                    {
                        if (person is Active)
                        {
                            //Active character moving
                            Game.world.CheckIfFoundTarget(pos, charID);
                        }
                        else if (person is Enemy)
                        {
                            //Enemy character moving
                            Game.world.CheckIfFoundEnemy(pos, charID);
                        }
                    }
                    else { Game.SetError(new Error(160, string.Format("Invalid charID \"{0}\" Position (null)", charID))); }

                    //move actgor along path
                    if ((currentPosIndex + i + 1) >= pathList.Count)
                    {
                        //at destination
                        currentPosIndex = pathList.Count - 1;
                        atDestination = true;
                    }
                    else
                    {
                        //still enroute
                        currentPosIndex++;
                    }
                }
                else { Game.SetError(new Error(160, string.Format("Invalid charID \"{0}\" (null)", charID))); }
            }
            return atDestination;
        }

        public void ChangeSpeed(TravelMode mode)
        {
            switch (mode)
            {
                case TravelMode.Mounted:
                    speed = Game.constant.GetValue(Global.MOUNTED_SPEED);
                    break;
                case TravelMode.Foot:
                    speed = Game.constant.GetValue(Global.FOOT_SPEED);
                    break;
                case TravelMode.None:
                    speed = 0;
                    break;
            }
            Game.logTurn?.Write($"[Notification -> Move] MoveObject, mapmarker {MapMarker}, changed to TravelMode {mode}, speed {(int)mode}");
        }

        //returns current Position
        public Position GetCurrentPosition()
        {
            Position pos = new Position();
            if (Status != PartyStatus.Done)
            { pos = pathList[currentPosIndex]; }
            else
            //end of the road
            { pos = pathList[pathList.Count - 1]; }
            return pos;
        }

        ///return Character List
        public List<int> GetCharacterList()
        { return characterList; }

        /// <summary>
        /// returns main character in party 
        /// </summary>
        /// <returns></returns>
        public int GetPrimaryCharacter()
        { return characterList[0]; }

        /// <summary>
        /// directly set Party status
        /// </summary>
        /// <param name="status"></param>
        //public void SetPartyStatus(PartyStatus status)
        //{ Status = status; }

        /// <summary>
        /// Polls characters in party to check for any that are delayed (affects all party members, if so)
        /// </summary>
        public void UpdatePartyStatus()
        {
            foreach (int actorID in characterList)
            {
                Actor actor = Game.world.GetAnyActor(actorID);
                if (actor != null)
                {
                    if (actor.Delay > 0)
                    { Status = PartyStatus.Delayed; break; }
                    else
                    { Status = PartyStatus.Active; }
                }
                else { Game.SetError(new Error(51, "Invalid Actor in characterList")); }
            }
        }

        /// <summary>
        /// returns name of destination location
        /// </summary>
        /// <returns></returns>
        public string GetDestination()
        {
            string destination = "Unknown";
            Position pos = pathList[pathList.Count - 1];
            if (pos != null)
            { destination = Game.world.GetLocationName(pos); }
            return destination;
        }
    }
}