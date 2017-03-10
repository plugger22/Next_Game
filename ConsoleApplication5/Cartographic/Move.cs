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
        bool playerInParty; //is a player controlled character in the party? (different color or symbol on map)
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
            currentPosIndex = 0;
            this.playerInParty = playerInParty;
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
        /// Moves party at speed clicks down list of Positions
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
            for (int i = 0; i < tempSpeed; i++)
            {
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
                //check for being in the same place as an enemy -> Active characters only
                charID = characterList[0];
                Actor person = Game.world.GetAnyActor(charID);
                if (person != null)
                {
                    Position pos = pathList[currentPosIndex];
                    if (pos != null)
                    {
                        if (person is Active)
                        {
                            //loop enemydict and check if a match
                            Game.world.CheckIfFoundActive(pos, charID);
                        }
                        else if (person is Enemy)
                        {
                            
                        }
                    }
                    else { Game.SetError(new Error(160, string.Format("Invalid charID \"{0}\" Position (null)", charID))); }
                }
                else { Game.SetError(new Error(160, string.Format("Invalid charID \"{0}\" (null)", charID))); }

                //need to check for enemy movements as well

            }
            return atDestination;
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

        //return Character List
        public List<int> GetCharacterList()
        { return characterList; }

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
    }
}