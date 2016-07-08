using System;
using System.Collections.Generic;

//Done is triggered on arrival at destination and is used to clear out the moveObject from the list of such
public enum PartyStatus {Active, Delayed, Done};

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
        PartyStatus partyStatus;
        bool playerInParty; //is a player controlled character in the party? (different color or symbol on map)
        public int MapMarker { get; set; } //# of party on map which is equal to the highest ranked member in party (lowest ID as ID 1 is the player)

        public Move()
        { }

        //main constructor
        public Move(List<Position> listPos, List<int> listParty, int speed, bool playerInParty, int turnDeparted)
        {
            pathList = new List<Position>(listPos);
            characterList = new List<int>(listParty);
            this.speed = speed;
            partyStatus = PartyStatus.Active;
            currentPosIndex = 0;
            this.playerInParty = playerInParty;
            this.turnDeparted = turnDeparted;
            //find highest ranked party member by ID (lowest)
            if (playerInParty)
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
            { MapMarker = 0; }
        }

        /// <summary>
        /// Moves party at speed clicks down list of Positions
        /// </summary>
        /// <returns>True if party has reached destination</returns>
        public bool MoveParty()
        {
            bool atDestination = false;
            int tempSpeed = speed;
            //reduce speed by 1 at start to avoid a speed + 1 move
            if(currentPosIndex == 0 && speed > 1)
            { tempSpeed--; }
            if(currentPosIndex + tempSpeed + 1 >= pathList.Count)
            //at destination
            {
                currentPosIndex = pathList.Count - 1;
                atDestination = true;
            }
            else
            //still enroute
            { currentPosIndex += tempSpeed; }
            return atDestination;
        }

        //returns current Position
        public Position GetCurrentPosition()
        {
            Position pos = new Position();
            if(partyStatus != PartyStatus.Done)
            { pos = pathList[currentPosIndex];}
            else
            //end of the road
            { pos = pathList[pathList.Count - 1]; }
            return pos;
        }

        //return Character List
        public List<int> GetCharacterList()
        { return characterList; }

        public void SetPartyStatus(PartyStatus status)
        { partyStatus = status; }

        public PartyStatus GetPartyStatus()
        { return partyStatus; }
    }
}