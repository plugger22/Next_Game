using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RLNET;

//TODO optimise route search routines by having four separate route lists, one for each branch

namespace Next_Game.Cartographic
{
    public enum NetGrid { Locations, Connections, Houses, WorkingLocs, Specials, Count } //arrayOfNetworkAnalysis -> Connections is the # Connections for first Loc out from Capital on branch

    public class Network
    {
        static int capitalX;
        static int capitalY;
        static Random rnd;
        public int[,] ArrayOfConnectors { get; set; }
        private List<Route> ListOfRoutes { get; set; }
        private List<Route> ListOfConnectorRoutes { get; set; } //list of all special branch Connector routes
        private List<Location> ListOfLocations { get; set; }
        //Interface class to enable dictionary keys (Position) to be compared
        List<string> listOfLocationNames = new List<string>(); //list of all location names
        Dictionary<int, Location> dictLocations;
        //see NetGrid enum above
        private int[,] arrayOfNetworkAnalysis;
        //four lists (sorted by distance to capital) of locations, one for each branch.
        private List<Location> listNorthBranch;
        private List<Location> listEastBranch;
        private List<Location> listSouthBranch;
        private List<Location> listWestBranch;
        //house analysis data
        private int uniqueHouses;
        private List<List<int>> listUniqueHousesByBranch;
        private List<List<int>> listIndividualHouseLocID;
        private int[] arrayOfCapitals; //locID of capitals, indexed by houseID #, eg. houseID #2 is arrayOfCapital[2]
        private int numSpecialLocations;

        /// <summary>
        /// default constructor with seed for random # generator
        /// </summary>
        /// <param name="seed">Random seed passed from Game</param>
        public Network(int seed)
        {
            //posEqC = new PositionEqualityComparer();
            dictLocations = new Dictionary<int, Location>();
            rnd = new Random(seed);
            //set up data structures
            ListOfRoutes = Game.map.GetRoutes();
            ListOfLocations = Game.map.GetLocations();
            ListOfConnectorRoutes = Game.map.GetConnectors();
            ArrayOfConnectors = Game.map.GetArrayOfConnectors();
            arrayOfNetworkAnalysis = new int[5, (int)NetGrid.Count + 1];
            listNorthBranch = new List<Location>();
            listEastBranch = new List<Location>();
            listSouthBranch = new List<Location>();
            listWestBranch = new List<Location>();
            listUniqueHousesByBranch = new List<List<int>>();
            listIndividualHouseLocID = new List<List<int>>();
        }

        /// <summary>
        /// set up Network collections
        /// </summary>
        public void InitialiseNetwork()
        {
            //set up locations dictionary
            //one entry for each location, keyed off it's ID
            if ( dictLocations != null)
            {
                foreach(Location loc in ListOfLocations)
                {
                    //add to Loc dictionary
                    dictLocations.Add(loc.LocationID, loc);
                    //tally up number of locations on each branch
                    int branch = loc.GetCapitalRouteDirection();
                    arrayOfNetworkAnalysis[branch, 0]++;
                    //add to correct branch list
                    switch( branch)
                        {
                        case 1:
                            listNorthBranch.Add(loc);
                            break;
                        case 2:
                            listEastBranch.Add(loc);
                            break;
                        case 3:
                            listSouthBranch.Add(loc);
                            break;
                        case 4:
                            listWestBranch.Add(loc);
                            break;
                        }
                }
                
                //create list of neighbours for each location
                InitialiseNeighbours();
                //create list of routes from locations back to the capital
                InitialiseRoutesToCapital();
                InitialiseRoutesToConnectors();
                ShowNetworkAnalysis();
                InitialiseHouseLocations(8, 3);
            }
        }

        /// <summary>
        /// sorts out all locations on a branch
        /// </summary>
        private void InitialiseRoutesToCapital()
        {
            List<Position> listOfNeighbours = new List<Position>();
            Position[] arrayOfCapitalNeighbours = new Position[5];
            capitalX = -1; capitalY = -1;
            listOfNeighbours = null;
            //find Capital
            foreach(Location loc in ListOfLocations)
            {
                if(loc.IsCapital() == true)
                {
                    listOfNeighbours = loc.GetNeighboursPos();
                    capitalX = loc.GetPosX();
                    capitalY = loc.GetPosY();
                    //Capital pos in array[0], 4 directions from capital (initial loc's in that direction) store in array[1...4]
                    arrayOfCapitalNeighbours[0] = loc.GetPosition();
                    break;
                }
            }
            //Capital found
            if(listOfNeighbours != null)
            {
                //loop list of Neighbours to Capital
                foreach(Position pos in listOfNeighbours)
                {
                    //Console.WriteLine("Debug: Neighbour found at {0}:{1}", pos.PosX, pos.PosY);
                    //look up neighbour in Dictionary of Locations
                    Location loc = new Location();
                    int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                    int direction = 0;
                    if( dictLocations.TryGetValue(locID, out loc))
                    {
                        direction = loc.GetCapitalRouteDirection();
                        arrayOfCapitalNeighbours[direction] = loc.GetPosition();
                    }
                }
            }
            //loop through locations and get route & distance from capital
            Position posCapital = arrayOfCapitalNeighbours[0];
            for (int i = 0; i < ListOfLocations.Count; i++)
            {
                Location loc = ListOfLocations[i];
                Position posDestination = loc.GetPosition();
                int direction = loc.GetCapitalRouteDirection();
                Position posFirst = arrayOfCapitalNeighbours[direction];
                List<Route> listOfRoutesToDestination = new List<Route>();
                listOfRoutesToDestination = GetRouteOnSameBranch(posCapital, posDestination, posFirst);
                listOfRoutesToDestination.Reverse();
                //store route & generate reciprocal route
                loc.SetRoutesFromCapital(listOfRoutesToDestination);
                loc.InitialiseRouteToCapital();
                //store distance
                int routeDistance = 0;
                List<Position> path = new List<Position>();
                foreach (Route route in listOfRoutesToDestination)
                {
                    path = route.GetPath();
                    routeDistance += path.Count - 1;
                }
                loc.DistanceToCapital = routeDistance;
                //find first loc out from capital in each direction
                if(listOfRoutesToDestination.Count == 1)
                { arrayOfNetworkAnalysis[direction, 1] = loc.Connections; }
            }
            //sort branch lists (distance from Capital)
            listNorthBranch.Sort();
            listEastBranch.Sort();
            listSouthBranch.Sort();
            listWestBranch.Sort();
        }


        /// <summary>
        /// sets up routes to connectors (if one exists for that branch)
        /// </summary>
        private void InitialiseRoutesToConnectors()
        {
            //loop through array to check if a connector exists for the branch
            for(int index = 1; index <= 4; index++)
            {
                //connector exists?
                if (ArrayOfConnectors[index, 0] > 0)
                {
                    //get data
                    Position posOrigin = new Position();
                    posOrigin.PosX = ArrayOfConnectors[index, 2];
                    posOrigin.PosY = ArrayOfConnectors[index, 3];
                    //loop through list of locations
                    for (int i = 0; i < ListOfLocations.Count; i++)
                    {
                        Location loc = ListOfLocations[i];
                        //correct branch
                        if (loc.GetCapitalRouteDirection() == index)
                        {
                            //not Connector = true (posOrigin)
                            if (loc.Connector == false)
                            {
                                Position posDestination = loc.GetPosition();
                                List<Route> listOfRoutesToDestination = GetRouteOnSameBranch(posOrigin, posDestination);
                                //store route
                                listOfRoutesToDestination.Reverse();
                                loc.SetRoutesFromConnector(listOfRoutesToDestination);
                                loc.InitialiseRouteToConnector();
                                //store distance
                                int routeDistance = 0;
                                List<Position> path = new List<Position>();
                                foreach (Route route in listOfRoutesToDestination)
                                {
                                    path = route.GetPath();
                                    routeDistance += path.Count - 1;
                                }
                                loc.DistanceToConnector = routeDistance;
                            }
                        }
                    }
                }
            }
        }

        //input origin and destination positions and method returns a list of routes from A to B (only on same branch derived from the capital)
        //send to map method DrawRoute(List<Route>)
        //posFirst is an optional position to go to first (used for InitiatilseRoutesToCapital)
        private List<Route> GetRouteOnSameBranch(Position posOrigin, Position posDestination, Position posFirst = null)
        {
            Stack<PositionPair> stackOfPositionPairs = new Stack<PositionPair>(); //stack of Position Pairs used for pathfinding (faster than using routes)
            List<Route> listOfFoundRoutes = new List<Route>(); //stack converted to list of routes at end and returned
            List<Position> originNeighbours = new List<Position>(); //dynamic list to enable searching in different directions from origin
            List<Position> baseNeighbours = new List<Position>(); //list of neighbours from posBase
            List<Position> newNeighbours = new List<Position>(); //list of neighbours from posNew
            List<PositionPair> alreadySearched = new List<PositionPair>(); //keeps a list of failed searched pos pairs to prevent backtracking
            bool keepLooking = true; //global search status
            bool foundPos = true; //local search status
            Location baseLoc = new Location();
            Location newLoc = new Location();
            Position posBase = new Position();
            Position posNew = new Position();
            Position posSearch = new Position();
            int locID = Game.map.GetMapInfo(MapLayer.LocID, posOrigin.PosX, posOrigin.PosY);
            //position pair (route origin/destination)
            //lookup origin location in dictionary
            if (dictLocations.TryGetValue(locID, out baseLoc))
            {
                baseNeighbours = baseLoc.GetNeighboursPos();
                //set up originNeighbours
                for (int i = 0; i < baseNeighbours.Count; i++)
                {
                    Position pos = baseNeighbours[i];
                    //if capital one link away ignore
                    if (pos.PosX != capitalX || pos.PosY != capitalY)
                    { originNeighbours.Add(pos); }
                }
                //Initialise 
                if (originNeighbours.Count > 0)
                {
                    
                    posBase.PosX = posOrigin.PosX;
                    posBase.PosY = posOrigin.PosY;
                    //if posFirst valid then use that instead (also there will be no need for origin neighbours as you'll be going outwards from the Capital)
                    if (posFirst == null)
                    {
                        posNew.PosX = originNeighbours[0].PosX;
                        posNew.PosY = originNeighbours[0].PosY;
                    }
                    else
                    {
                        posNew.PosX = posFirst.PosX;
                        posNew.PosY = posFirst.PosY;
                    }
                    posSearch.PosX = posNew.PosX;
                    posSearch.PosY = posNew.PosY;
                    //delete as that direction is now being searched
                    try
                    { originNeighbours.RemoveAt(0); }
                    catch (Exception e)
                    { Game.SetError(new Error(61, e.Message)); }
                }
                else
                {
                    //the only neighbour is the capital
                    listOfFoundRoutes = baseLoc.GetRouteFromCapital();
                    return listOfFoundRoutes;
                }
                //Console.WriteLine("Debug: GetRouteOnSameBranch ---");
                //Console.WriteLine("Debug: INITIALISATION posBase {0}:{1}, posNew {2}:{3}, posSearch {4}:{5}", posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY, posSearch.PosX, posSearch.PosY);
                do
                {
                    //initialise PositionPair
                    PositionPair posPair = new PositionPair();
                    posPair.PosX = posBase.PosX;
                    posPair.PosY = posBase.PosY;
                    posPair.Pos2X = posNew.PosX;
                    posPair.Pos2Y = posNew.PosY;
                    //Console.WriteLine();
                    //Console.WriteLine("Debug: DO posPair {0}:{1}, {2}:{3} ---", posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY);
                    
                    //PUSH PositionPair onto STACK = = =

                    if (foundPos == true)
                    {
                        //check that you're not doubling back
                        if (stackOfPositionPairs.Count > 0)
                        {
                            //ColourText("PEEK", ConsoleColor.DarkGray);
                            PositionPair posPairPeek = stackOfPositionPairs.Peek();
                            //Console.WriteLine("Debug: FoundPos TRUE, PosPair {0}:{1} - {2}:{3} posPairPeek {4}:{5} - {6}:{7}", posPair.PosX, posPair.PosY, posPair.Pos2X, posPair.Pos2Y,
                            //    posPairPeek.PosX, posPairPeek.PosY, posPairPeek.Pos2X, posPairPeek.Pos2Y);
                            bool push = true;
                            //if doubling back then need to pop, not push
                            if((posPair.PosX == posPairPeek.PosX && posPair.PosY == posPairPeek.PosY) || (posPair.PosX == posPairPeek.Pos2X && posPair.PosY == posPairPeek.Pos2Y))
                            {
                                if ((posPair.Pos2X == posPairPeek.PosX && posPair.Pos2Y == posPairPeek.PosY) || (posPair.Pos2X == posPairPeek.Pos2X && posPair.Pos2Y == posPairPeek.Pos2Y))
                                { push = false; }
                            }
                            //posPair new, push to stack
                            if (push == true)
                            {
                            
                            stackOfPositionPairs.Push(posPair);
                            //ColourText("PUSH", ConsoleColor.Blue);
                            //Console.WriteLine("Debug: PosPair ON STACK, {0} records", stackOfPositionPairs.Count);
                            //Console.WriteLine();
                            }
                            //posPair matches top of stack, need to pop
                            else if (push == false)
                            {
                                PositionPair posPairInvalid = stackOfPositionPairs.Pop();
                                alreadySearched.Add(posPairInvalid);
                                //ColourText("POP", ConsoleColor.DarkRed);
                                //Console.WriteLine("Debug: POP STACK - doubling back"); 
                                //Console.WriteLine();
                            }
                        }
                        else
                        {
                            //can't be doubling back if nothing on stack, must be back at origin
                            if (posPair.Pos2X != posOrigin.PosX || posPair.Pos2Y != posOrigin.PosY)
                            {
                                stackOfPositionPairs.Push(posPair);
                                //ColourText("PUSH", ConsoleColor.Blue);
                                //Console.WriteLine("Debug: PosPair ON STACK, {0} records", stackOfPositionPairs.Count);
                                //Console.WriteLine();
                            }
                            else
                            { /*Console.WriteLine("Debug: PosPair NOT PUT ON STACK - back at origin"); Console.WriteLine();*/ }
                        }

                    }
                    //backing up, check if route needs to be deleted
                    else if(foundPos == false)
                    {
                        //Console.WriteLine("Debug: Doubling Back, test top of stack");
                        if (stackOfPositionPairs.Count > 0)
                        {
                            //delete top route on stack if equivalent to current searched route
                            PositionPair posPairPeek = stackOfPositionPairs.Peek();
                            //ColourText("PEEK", ConsoleColor.DarkGray);
                            //Console.WriteLine("Debug: FoundPos FALSE, PosPair {0}:{1} - {2}:{3} posPairPeek {4}:{5} - {6}:{7}", posPair.PosX, posPair.PosY, posPair.Pos2X, posPair.Pos2Y,
                            //    posPairPeek.PosX, posPairPeek.PosY, posPairPeek.Pos2X, posPairPeek.Pos2Y);
                            if ((posPair.PosX == posPairPeek.Pos2X || posPair.Pos2Y == posPairPeek.PosY) && (posPair.Pos2X == posPairPeek.PosX || posPair.Pos2Y == posPairPeek.PosY))
                            {
                                PositionPair posPairInvalid = stackOfPositionPairs.Pop();
                                alreadySearched.Add(posPairInvalid);
                                //ColourText("POP", ConsoleColor.DarkRed);
                                //Console.WriteLine("Debug: PosPair POP STACK, doubling back, {0} records", stackOfPositionPairs.Count);
                                //Console.WriteLine();
                                //update posBase & PosNew (otherwise out of sync)
                                if (stackOfPositionPairs.Count > 0)
                                {
                                    PositionPair posPairUpdate = stackOfPositionPairs.Peek();
                                    posBase.PosX = posPairUpdate.Pos2X;
                                    posBase.PosY = posPairUpdate.Pos2Y;
                                    posNew.PosX = posPairUpdate.Pos2X;
                                    posNew.PosY = posPairUpdate.Pos2Y;
                                    //Console.WriteLine("Debug: UPDATE posNew & PosBase {0}:{1}", posBase.PosX, posBase.PosY);
                                }
                                else
                                {
                                    //stack empty, must be at origin
                                    //ColourText("BASE", ConsoleColor.Green);
                                    //Console.WriteLine("Debug: NOTHING ON STACK, back at base");
                                    posBase.PosX = posOrigin.PosX;
                                    posBase.PosY = posOrigin.PosY;
                                    //get next record in originNeighbours as searching in a different direction
                                    if (originNeighbours.Count > 0)
                                    {
                                        posNew.PosX = originNeighbours[0].PosX;
                                        posNew.PosY = originNeighbours[0].PosY;
                                        //delete record to indicate that direction has been searched
                                        try
                                        { originNeighbours.RemoveAt(0); }
                                        catch (Exception e)
                                        { Game.SetError(new Error(61, e.Message)); }
                                        //place new position on stack
                                        PositionPair posPairTemp = new PositionPair();
                                        posPairTemp.PosX = posBase.PosX;
                                        posPairTemp.PosY = posBase.PosY;
                                        posPairTemp.Pos2X = posNew.PosX;
                                        posPairTemp.Pos2Y = posNew.PosY;
                                        stackOfPositionPairs.Push(posPairTemp);
                                        //ColourText("PUSH", ConsoleColor.Blue);
                                        //Console.WriteLine("Debug: PUSH STACK, doubling back, ORIGIN, empty stack, {0} records", stackOfPositionPairs.Count);
                                    }
                                    //no more directions to search from origin, all over
                                    else
                                    {
                                        //Console.WriteLine("ERROR - originNeighbours EMPTY");
                                        keepLooking = false;
                                    }
                                }
                            }
                        }
                        //backing up and Stack empty. Must be at location (if already there skip this)
                        else if(posBase.PosX != posOrigin.PosX && posBase.PosY != posOrigin.PosY)
                        {
                            //ColourText("BASE", ConsoleColor.Green);
                            //Console.WriteLine("Debug: NOTHING ON STACK, back at base");
                            posBase.PosX = posOrigin.PosX;
                            posBase.PosY = posOrigin.PosY;
                            //get next record in originNeighbours as searching in a different direction
                            if (originNeighbours.Count > 0)
                            {
                                posNew.PosX = originNeighbours[0].PosX;
                                posNew.PosY = originNeighbours[0].PosY;
                                //delete record to indicate that direction has been searched
                                try
                                { originNeighbours.RemoveAt(0); }
                                catch (Exception e)
                                { Game.SetError(new Error(61, e.Message)); }
                                //place new position on stack
                                PositionPair posPairTemp = new PositionPair();
                                posPairTemp.PosX = posBase.PosX;
                                posPairTemp.PosY = posBase.PosY;
                                posPairTemp.Pos2X = posNew.PosX;
                                posPairTemp.Pos2Y = posNew.PosY;
                                stackOfPositionPairs.Push(posPairTemp);
                                //ColourText("PUSH", ConsoleColor.Blue);
                                //Console.WriteLine("Debug: PUSH STACK, backing up, ORIGIN, empty stack, {0} records", stackOfPositionPairs.Count);
                            }
                            //no more directions to search from origin, all over
                            else
                            {
                                Game.SetError(new Error(5, "originNeighbours EMPTY"));
                                keepLooking = false;
                            }
                        }
                    }

                    //test = = =

                    if (posNew.PosX == posDestination.PosX && posNew.PosY == posDestination.PosY)
                    { keepLooking = false; /*Console.WriteLine("Debug: TEST - found destination, keeplooking FALSE");*/ }

                    //continue SEARCHING = = =

                    else
                    {
                        //Console.WriteLine("Debug: TEST - Continue");
                        foundPos = false;
                        //get newLoc by searching dictionary with newPos
                        locID = Game.map.GetMapInfo(MapLayer.LocID, posNew.PosX, posNew.PosY);
                        if (dictLocations.TryGetValue(locID, out newLoc))
                        {
                            //Console.WriteLine("Debug: DICTIONARY SEARCH SUCCESSFUL");
                            PositionPair posPairPeek = new PositionPair();
                            if(stackOfPositionPairs.Count > 0)
                            { posPairPeek = stackOfPositionPairs.Peek();}
                            else
                            {
                                //empty stack, back at origin, already have correct data
                                posPairPeek.PosX = posBase.PosX;
                                posPairPeek.PosY = posBase.PosY;
                                posPairPeek.Pos2X = posNew.PosX;
                                posPairPeek.Pos2Y = posNew.PosY;
                            }
                            
                            //get newNeighbours
                            newNeighbours = newLoc.GetNeighboursPos();
                            //find next position on newNeighbours list that != posBase || ! = posSearch || != Capital || != Origin
                            foreach (Position pos in newNeighbours)
                            {
                                //Console.WriteLine("Debug: newLoc.newNeighbours pos {0}:{1}", pos.PosX, pos.PosY);
                                if ((pos.PosX != posBase.PosX || pos.PosY != posBase.PosY) && (pos.PosX != posSearch.PosX || pos.PosY != posSearch.PosY) && 
                                    (pos.PosX != capitalX || pos.PosY != capitalY) && (pos.PosX != posOrigin.PosX || pos.PosY != posOrigin.PosY))
                                {
                                    //CHECK List of previous search pairs and ignore any base + pos / pos + base combo's to prevent repeat searches)
                                    bool validPosition = true;
                                    foreach (PositionPair pp in alreadySearched)
                                    {
                                        //Console.WriteLine("Debug: SEARCH alreadySearched pos1 {0}:{1} pos2 {2}:{3} Records {4}", pp.PosX, pp.PosY, pp.Pos2X, pp.Pos2Y, alreadySearched.Count);
                                        if (pp.PosX == posBase.PosX && pp.PosY == posBase.PosY)
                                        {
                                            if (pp.Pos2X == pos.PosX && pp.Pos2Y == pos.PosY)
                                            {
                                                //existing search
                                                validPosition = false; /*Console.WriteLine("Debug: SEARCH BREAK - existing_1");*/ break;
                                            }
                                        }
                                        else if (pp.Pos2X == posBase.PosX && pp.Pos2Y == posBase.PosY)
                                        {
                                            if (pp.PosX == pos.PosX && pp.PosY == pos.PosY)
                                            {
                                                //existing search
                                                validPosition = false; /*Console.WriteLine("Debug: SEARCH BREAK - existing_2");*/ break;
                                            }
                                        }
                                    }
                                    //check not top pair on stack (doubling back)
                                    if(validPosition == true)
                                    {
                                        //Console.WriteLine("Debug: SEARCH check Stack, Pos {0}:{1} - PosNew {2}:{3} posPairPeek {4}:{5} - {6}:{7}", pos.PosX, pos.PosY, posNew.PosX, posNew.PosY,
                                        //posPairPeek.PosX, posPairPeek.PosY, posPairPeek.Pos2X, posPairPeek.Pos2Y);
                                        if ((pos.PosX == posPairPeek.PosX && pos.PosY == posPairPeek.PosY) && (posNew.PosX == posPairPeek.Pos2X && posNew.PosY == posPairPeek.Pos2Y))
                                        { validPosition = false; }
                                    }
                                    //pos and posBase don't exist in list of already searched routes
                                    if (validPosition == true)
                                    {
                                        //found a suitable Position on list of neighbours
                                        foundPos = true;
                                        posSearch.PosX = pos.PosX;
                                        posSearch.PosY = pos.PosY;
                                        //Console.WriteLine("Debug: FOUND A POS {0}:{1}", pos.PosX, pos.PosY);
                                        break;
                                    }

                                }
                            }
                        }
                        //can't find loc in dictionary
                        else
                        {
                            //Console.WriteLine("Debug: CAN'T FIND in Dictionary");
                            //if origin and have reached end of listOfNeighbours (shouldn't happen but just in case)
                            if (posBase.PosX == posOrigin.PosX && posBase.PosY == posOrigin.PosY)
                            { /*Console.WriteLine("Debug: Reached end of Origin listOfNeighbours, keepLooking = false");*/ keepLooking = false; }
                        }
                        //update data for next search cyce
                        if (foundPos == true)
                        {
                            //Console.WriteLine("Debug: UPDATE DATA");
                            posBase.PosX = posNew.PosX;
                            posBase.PosY = posNew.PosY;
                            posNew.PosX = posSearch.PosX;
                            posNew.PosY = posSearch.PosY;
                            baseNeighbours = newNeighbours; //this line serves no real purpose
                            //Console.WriteLine("Debug: posBase {0}:{1}, posNew {2}:{3}, posSearch {4}:{5}", posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY, posSearch.PosX, posSearch.PosY);
                        }

                        //no new node found, BACK UP A NODE = = =

                        else if (foundPos == false)
                        {
                            //Console.WriteLine();
                            //Console.WriteLine("Debug: BACK UP A NODE");
                            // access top of stack and get data to back up
                            if (stackOfPositionPairs.Count > 0)
                            {
                                //pop PosPair off the stack, reset data
                                PositionPair posPairInvalid = stackOfPositionPairs.Pop();
                                //store invalid search path
                                alreadySearched.Add(posPairInvalid);
                                //ColourText("POP", ConsoleColor.DarkRed);
                                //Console.WriteLine("Debug: POP STACK, {0} records", stackOfPositionPairs.Count);
                            }
                            //look for next viable node
                            if (stackOfPositionPairs.Count > 0)
                            {
                                PositionPair posPairTemp = stackOfPositionPairs.Peek();
                                posBase.PosX = posPairTemp.Pos2X;
                                posBase.PosY = posPairTemp.Pos2Y;
                                posNew.PosX = posPairTemp.PosX;
                                posNew.PosY = posPairTemp.PosY;
                                //ColourText("PEEK", ConsoleColor.DarkGray);
                                //Console.WriteLine("Debug: PEEK STACK  posBase {0}:{1} posNew {2}:{3}", posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY);
                                //Console.WriteLine();

                                //find loc in dictionary and get correct list of Neighbours to search for any remaining viable routes
                                Location searchLoc = new Location();
                                List<Position> searchNeighbours = new List<Position>();
                                locID = Game.map.GetMapInfo(MapLayer.LocID, posBase.PosX, posBase.PosY);
                                if (dictLocations.TryGetValue(locID, out searchLoc))
                                { searchNeighbours = searchLoc.GetNeighboursPos(); }
                                //check that there isn't a viable pos from the base that hasn't already been searched != Capital, != Base, != origin, != posNew default (from peek) != posSearch
                                foreach (Position pos in searchNeighbours)
                                {
                                    //Console.WriteLine("Debug: PEEK SEARCH newLoc.searchNeighbours pos {0}:{1}", pos.PosX, pos.PosY);
                                    if ((pos.PosX != posNew.PosX || pos.PosY != posNew.PosY) && (pos.PosX != capitalX || pos.PosY != capitalY) && 
                                        (pos.PosX != posOrigin.PosX || pos.PosY != posOrigin.PosY))
                                    {
                                        //CHECK List of previous search pairs and ignore any base + pos / pos + base combo's to prevent repeat searches)
                                        bool validPosition = true;
                                        foreach(PositionPair pp in alreadySearched)
                                        {
                                            //Console.WriteLine("Debug: alreadySearched pos1 {0}:{1} pos2 {2}:{3} Records {4}", pp.PosX, pp.PosY, pp.Pos2X, pp.Pos2Y, alreadySearched.Count);
                                            if(pp.PosX == posBase.PosX && pp.PosY == posBase.PosY)
                                            {
                                                if(pp.Pos2X == pos.PosX && pp.Pos2Y == pos.PosY)
                                                {
                                                    //existing search
                                                    validPosition = false; /*Console.WriteLine("Debug: BREAK - existing_1");*/ break;
                                                }
                                            }
                                            else if(pp.Pos2X == posBase.PosX && pp.Pos2Y == posBase.PosY)
                                            {
                                                if(pp.PosX == pos.PosX && pp.PosY == pos.PosY)
                                                {
                                                    //existing search
                                                    validPosition = false; /*Console.WriteLine("Debug: BREAK - existing_2");*/ break;
                                                }
                                            }
                                        }
                                        //found a suitable Position on list of neighbours
                                        if (validPosition == true)
                                        {
                                            foundPos = true;
                                            posSearch.PosX = pos.PosX;
                                            posSearch.PosY = pos.PosY;
                                            posNew.PosX = pos.PosX;
                                            posNew.PosY = pos.PosY;
                                            //Console.WriteLine("Debug: PEEK SEARCH - Found a new Node {0}:{1}", pos.PosX, pos.PosY);
                                            break;
                                        }
                                    }
                                }
                            }
                            //nothing on stack, must be back at base
                            else
                            {
                                //ColourText("BASE", ConsoleColor.Green);
                                //Console.WriteLine("Debug: NOTHING ON STACK, back at base");
                                posBase.PosX = posOrigin.PosX;
                                posBase.PosY = posOrigin.PosY;
                                //get next record in originNeighbours as searching in a different direction
                                if (originNeighbours.Count > 0)
                                {
                                    posNew.PosX = originNeighbours[0].PosX;
                                    posNew.PosY = originNeighbours[0].PosY;
                                    //delete record to indicate that direction has been searched
                                    try
                                    { originNeighbours.RemoveAt(0); }
                                    catch (Exception e)
                                    { Game.SetError(new Error(61, e.Message)); }
                                    foundPos = true;
                                }
                                //no more directions to search from origin, all over
                                else
                                { keepLooking = false; }
                            }
                            //Console.WriteLine("Debug: Back Up a Node, posBase {0}:{1}, posNew {2}:{3}", posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY);
                        }
                    }
                }
                while (keepLooking == true);
                //Console.WriteLine("Debug: EXIT WHILE");

                //CONVERT stack of PositionPairs into listOfRoutes = = =

                int size = stackOfPositionPairs.Count;
                PositionPair[] arrayOfPositionPairs = new PositionPair[size];
                //copy stack to the temporary array
                stackOfPositionPairs.CopyTo(arrayOfPositionPairs, 0);
                //loop through array
                for (int i = 0; i < size; i++)
                {
                    PositionPair posPairConvert = arrayOfPositionPairs[i];
                    posBase.PosX = posPairConvert.PosX;
                    posBase.PosY = posPairConvert.PosY;
                    posNew.PosX = posPairConvert.Pos2X;
                    posNew.PosY = posPairConvert.Pos2Y;
                    //Console.WriteLine("Debug: ARRAY_POSPAIRS posPair1 {0}:{1} posPair2 {2}:{3} Record {4} of {5}"
                        //, posBase.PosX, posBase.PosY, posNew.PosX, posNew.PosY, i, stackOfPositionPairs.Count);
                    //search list of routes to find correct route
                    for (int j = 0; j < ListOfRoutes.Count; j++)
                    {
                        Route route = ListOfRoutes[j];
                        Position pos1 = route.GetLoc1();
                        Position pos2 = route.GetLoc2();
                        //Console.WriteLine("Debug: LIST_ROUTES pos1 {0}:{1} pos2 {2}:{3} Record {4} of {5}", pos1.PosX, pos1.PosY, pos2.PosX, pos2.PosY, j, ListOfRoutes.Count);
                        if (posBase.PosX == pos1.PosX && posBase.PosY == pos1.PosY)
                        {
                            if (posNew.PosX == pos2.PosX && posNew.PosY == pos2.PosY)
                            { listOfFoundRoutes.Add(route); break; }
                        }
                        //need to reverse route
                        else if ((posBase.PosX == pos2.PosX && posBase.PosY == pos2.PosY))
                        {
                            if (posNew.PosX == pos1.PosX && posNew.PosY == pos1.PosY)
                            {
                                //correct route, add to list (correct direction, default is capital outwards, reverse if not)
                                List<Position> tempPath = new List<Position>(route.GetPath());
                                if (pos1.PosX != posOrigin.PosX || pos1.PosY != posOrigin.PosY)
                                { tempPath.Reverse(); }      
                                Route tempRoute = new Route(tempPath);
                                listOfFoundRoutes.Add(tempRoute);
                                //Console.WriteLine("Debug: ADD ROUTE to ListOfRoutes, record {0}", listOfFoundRoutes.Count);
                                break;
                            }
                        }
                    }
                }
            }
            return listOfFoundRoutes;
        }

        /// <summary>
        /// Returns a route in the form of a continuous List<Position>
        /// </summary>
        /// <param name="posOrigin"></param>
        /// <param name="posDestination"></param>
        /// <returns></returns>
        internal List<Position> GetPathAnywhere(Position posOrigin, Position posDestination)
        {
            List<Position> path = new List<Position>();
            List<Route> listOfRoutes = GetRouteAnywhere(posOrigin, posDestination);
            //convert route into a path
            for(int i = 0; i < listOfRoutes.Count; i++)
            {
                Route route = listOfRoutes[i];
                List<Position> posList = route.GetPath();
                for(int j = 1; j < posList.Count; j++)
                { path.Add(posList[j]); }
            }
            return path;

        }

        ///Main Route Finding Algorithim from A to B anywhere on the map (Note: Loc -> Loc, NOT Pos -> Loc)
        ///Returns List<Route> to enable a Map object to draw the route on the map
        ///data validation of A & B positions is done by Network.RouteInput()
        internal List<Route> GetRouteAnywhere(Position posOrigin, Position posDestination)
        {
            List<Route> listOfDirectRoutes = new List<Route>(); //default straight shot routes
            List<Route> listOfAlternateRoutes = new List<Route>(); //alternate routes via connectors
            //Get origin
            Location originLoc = new Location();
            Location destinationLoc = new Location();
            List<Route> originToCapitalRoutes = new List<Route>();
            List<Route> originFromCapitalRoutes = new List<Route>(); //capital -> origin
            List<Route> destinationToCapitalRoutes = new List<Route>();
            List<Route> destinationFromCapitalRoutes = new List<Route>(); //capital -> destination
            int distanceToCapital = 0;
            int distanceToConnector = 0;
            int distanceAcrossConnector = 0; //distance to traverse connector route
            int distanceAlternate = 0; //distance from a loc to capital down an alternative branch via a connector
            int distanceToDestination = 0; //distance from Capital to Destination
            int originBranch = 0;
            int destinationBranch = 0;
            int alternateBranch = 0;
            //Get origin routes
            int locID = Game.map.GetMapInfo(MapLayer.LocID, posOrigin.PosX, posOrigin.PosY);
            if (dictLocations.TryGetValue(locID, out originLoc))
            {
                originToCapitalRoutes.AddRange(originLoc.GetRouteToCapital());
                originFromCapitalRoutes.AddRange(originLoc.GetRouteFromCapital());
            }
            //Get destination routes
            locID = Game.map.GetMapInfo(MapLayer.LocID, posDestination.PosX, posDestination.PosY);
            if (dictLocations.TryGetValue(locID, out destinationLoc))
            {
                destinationToCapitalRoutes.AddRange(destinationLoc.GetRouteToCapital());
                destinationFromCapitalRoutes.AddRange(destinationLoc.GetRouteFromCapital());
            }
            //Is the Capital Involved - Origin  or Destination ---
            if ((originLoc.Capital == true) || (destinationLoc.Capital == true))
            {
                //Capital -> Destination  
                if (originLoc.Capital == true)
                {
                    //default straight shot out from Capital along branch, Capital -> Destination
                    listOfDirectRoutes.AddRange(destinationFromCapitalRoutes);
                    distanceToCapital = destinationLoc.DistanceToCapital;
                    distanceToConnector = destinationLoc.DistanceToConnector;
                    //connector exists?
                    originBranch = destinationLoc.GetCapitalRouteDirection();
                    if(ArrayOfConnectors[originBranch,0] > 0)
                    {
                        //get Position
                        Position pos = new Position();
                        pos.PosX = ArrayOfConnectors[originBranch, 4];
                        pos.PosY = ArrayOfConnectors[originBranch, 5];
                        //get distance to traverse connector
                        distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                        //find loc in Dictionary
                        Location loc = new Location();
                        locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                        if (dictLocations.TryGetValue(locID, out loc))
                        {
                            //get distance to Capital
                            distanceAlternate = loc.DistanceToCapital;
                            //check combined distance < Capital to Destination
                            if (distanceToCapital > (distanceAlternate + distanceToConnector + distanceAcrossConnector))
                            {
                                //compile combined route (capital -> connector -> destination)
                                listOfAlternateRoutes.AddRange(loc.GetRouteFromCapital());
                                //find connector route
                                foreach (Route route in ListOfConnectorRoutes)
                                {
                                    //NOTE changed 12thAug16 this to GetLoc1() as the route was reversed (mapSeed 29938)
                                    Position posEnd = route.GetLoc1();
                                    if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                    {
                                        //add to combined route ('Add' because it's a single route, not a collection)
                                        listOfAlternateRoutes.Add(route);
                                        break;
                                    }
                                }
                                //add to combined route
                                listOfAlternateRoutes.AddRange(destinationLoc.GetRouteFromConnector());
                            }
                        }
                    }
                }
                //Origin -> Capital
                else
                {
                    //default straight shot along branch to capital
                    listOfDirectRoutes.AddRange(originToCapitalRoutes);
                    distanceToCapital = originLoc.DistanceToCapital;
                    distanceToConnector = originLoc.DistanceToConnector;
                    //connector exists?
                    originBranch = originLoc.GetCapitalRouteDirection();
                    if (ArrayOfConnectors[originBranch, 0] > 0)
                    {
                        //get Position of Connector origin loc
                        Position pos = new Position();
                        pos.PosX = ArrayOfConnectors[originBranch, 2];
                        pos.PosY = ArrayOfConnectors[originBranch, 3];
                        //get distance to traverse connector
                        distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                        //find destination of connector route loc in Dictionary
                        Location loc = new Location();
                        locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                        if (dictLocations.TryGetValue(locID, out loc))
                        {
                            //get distance to Capital (need the far end of the connector for this)
                            int farLocID = loc.ConnectorID;
                            Location locFar = new Location();
                            if (dictLocations.TryGetValue(farLocID, out locFar))
                            {
                                //NOTE (12thAug16) needs loc at other side of connector. distance alternate taken from wrong end of connector.
                                distanceAlternate = locFar.DistanceToCapital;
                                //check combined distance < Capital to Destination
                                if (distanceToCapital > (distanceAlternate + distanceToConnector + distanceAcrossConnector))
                                {
                                    //compile combined route (origin -> connector -> destination -> capital)
                                    listOfAlternateRoutes.AddRange(originLoc.GetRouteToConnector());
                                    //find connector route
                                    foreach (Route route in ListOfConnectorRoutes)
                                    {
                                        Position posStart = route.GetLoc1();
                                        Position posEnd = route.GetLoc2();
                                        if (posStart.PosX == pos.PosX && posStart.PosY == pos.PosY)
                                        {
                                            //add to combined route ('Add' because it's a single route, not a collection)
                                            listOfAlternateRoutes.Add(route);
                                            //get destination route to Capital
                                            locID = Game.map.GetMapInfo(MapLayer.LocID, posEnd.PosX, posEnd.PosY);
                                            if (dictLocations.TryGetValue(locID, out loc))
                                            {
                                                //add to combined route
                                                listOfAlternateRoutes.AddRange(loc.GetRouteToCapital());
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Capital NOT Involved ---
            else
            {
                //get branch directions from capital
                originBranch = originLoc.GetCapitalRouteDirection();
                destinationBranch = destinationLoc.GetCapitalRouteDirection();
                //Both on same branch
                if(originBranch == destinationBranch)
                {
                    //default straight shot along branch
                    listOfDirectRoutes.AddRange(GetRouteOnSameBranch(posOrigin, posDestination));
                    listOfDirectRoutes.Reverse();
                    //check alternative connector route options for origin to capital -
                    distanceToCapital = originLoc.DistanceToCapital;
                    distanceToConnector = originLoc.DistanceToConnector;
                    distanceToDestination = destinationLoc.DistanceToCapital;
                    //connector exists?
                    originBranch = originLoc.GetCapitalRouteDirection();
                    if (ArrayOfConnectors[originBranch, 0] > 0)
                    {
                        //get Position
                        Position pos = new Position();
                        pos.PosX = ArrayOfConnectors[originBranch, 4];
                        pos.PosY = ArrayOfConnectors[originBranch, 5];
                        //get distance to traverse connector
                        distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                        //find loc in Dictionary
                        Location loc = new Location();
                        locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                        if (dictLocations.TryGetValue(locID, out loc))
                        {
                            //get distance to Capital
                            distanceAlternate = loc.DistanceToCapital;
                            //check combined distance vs. Origin -> Connector -> Across Connector -> Connector Destination to Capital -> Capital to Destination
                            if (distanceToCapital > (distanceToConnector + distanceAcrossConnector + distanceAlternate + distanceToDestination))
                            {
                                //compile combined route (destination - connector - destination - capital)
                                listOfAlternateRoutes.AddRange(originLoc.GetRouteToConnector());
                                //find connector route
                                foreach (Route route in ListOfConnectorRoutes)
                                {
                                    Position posEnd = route.GetLoc2();
                                    if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                    {
                                        //add to combined route ('Add' because it's a single route, not a collection)
                                        listOfAlternateRoutes.Add(route);
                                        break;
                                    }
                                }
                                //add to combined route
                                listOfAlternateRoutes.AddRange(loc.GetRouteToCapital());
                                //add leg from capital back to destination
                                listOfAlternateRoutes.AddRange(destinationLoc.GetRouteFromCapital());
                            }
                        }
                    }
                }
                //Different branches
                else
                {
                    listOfDirectRoutes.AddRange(originToCapitalRoutes);
                    //add capital to destination routes (reverse order)
                    /*for(int i = destinationToCapitalRoutes.Count - 1; i >= 0; i--)
                    {
                        Route route = destinationFromCapitalRoutes[i];
                        listOfDirectRoutes.Add(route);
                    }*/
                    listOfDirectRoutes.AddRange(destinationFromCapitalRoutes);

                    //check alternative routes ---

                    distanceToCapital = originLoc.DistanceToCapital;
                    distanceToConnector = originLoc.DistanceToConnector;
                    //connector exists?
                    if (ArrayOfConnectors[originBranch, 0] > 0 || ArrayOfConnectors[destinationBranch, 0] > 0)
                    {
                        //connector to destination branch
                        if(ArrayOfConnectors[originBranch,0] == destinationBranch)
                        {
                            distanceToConnector = originLoc.DistanceToConnector;
                            distanceToCapital = originLoc.DistanceToCapital;
                            //get Position of Connector destination (other end of connector)
                            Position pos = new Position();
                            pos.PosX = ArrayOfConnectors[originBranch, 4];
                            pos.PosY = ArrayOfConnectors[originBranch, 5];
                            //get distance to traverse connector
                            distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                            //find loc in Dictionary
                            Location loc = new Location();
                            locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                            if (dictLocations.TryGetValue(locID, out loc))
                            {
                                distanceAlternate = destinationLoc.DistanceToConnector;
                                distanceToDestination = destinationLoc.DistanceToCapital;
                                //distance Origin->Connector + Connector + ConnectorDestination->Destination
                                if( (distanceToCapital + distanceToDestination) > (distanceToConnector + distanceAcrossConnector + distanceAlternate))
                                {
                                    //route via connector is the quickest - compile combined route
                                    listOfAlternateRoutes.AddRange(originLoc.GetRouteToConnector());
                                    //find connector route
                                    foreach (Route route in ListOfConnectorRoutes)
                                    {
                                        Position posEnd = route.GetLoc2();
                                        if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                        {
                                            //add to combined route ('Add' because it's a single route, not a collection)
                                            listOfAlternateRoutes.Add(route);
                                            break;
                                        }
                                    }
                                    //add to combined route
                                    listOfAlternateRoutes.AddRange(destinationLoc.GetRouteFromConnector());
                                }
                            }
                        }
                        //NO connector to destination branch ---
                        else
                        {
                            //is there a connector to the Destination branch? (if not then no alternative route possible - ignore)
                            //DESTINATION branch only
                            if(ArrayOfConnectors[destinationBranch, 0] > 0 && ArrayOfConnectors[originBranch, 0] < 1)
                            {
                                alternateBranch = ArrayOfConnectors[destinationBranch, 0];
                                distanceToConnector = destinationLoc.DistanceToConnector;
                                distanceToDestination = destinationLoc.DistanceToCapital;
                                //get Position of Connector origin (on alternate branch)
                                Position pos = new Position();
                                pos.PosX = ArrayOfConnectors[destinationBranch, 4];
                                pos.PosY = ArrayOfConnectors[destinationBranch, 5];
                                //get distance to traverse connector
                                distanceAcrossConnector = ArrayOfConnectors[destinationBranch, 1];
                                //find loc in Dictionary
                                Location loc = new Location();
                                locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                                if (dictLocations.TryGetValue(locID, out loc))
                                {
                                    distanceAlternate = loc.DistanceToCapital;
                                    //distance origin->CAP + CAP->Connector + Connector + ConnectorDestination->Destination
                                    if ((distanceToCapital + distanceToDestination) > (distanceToCapital + distanceAlternate + distanceAcrossConnector + distanceToConnector))
                                    {
                                        //build up composite route
                                        listOfAlternateRoutes.AddRange(originLoc.GetRouteToCapital());
                                        listOfAlternateRoutes.AddRange(loc.GetRouteFromCapital());
                                        //find connector route
                                        foreach (Route route in ListOfConnectorRoutes)
                                        {
                                            Position posEnd = route.GetLoc2();
                                            if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                            {
                                                //add to combined route ('Add' because it's a single route, not a collection)
                                                listOfAlternateRoutes.Add(route);
                                                break;
                                            }
                                        }
                                        //add to combined route
                                        listOfAlternateRoutes.AddRange(destinationLoc.GetRouteToConnector() );
                                    }
                                }
                            }
                            //ORIGIN branch only
                            else if (ArrayOfConnectors[destinationBranch, 0] < 1 && ArrayOfConnectors[originBranch, 0] > 0)
                            {
                                distanceToConnector = originLoc.DistanceToConnector;
                                distanceToDestination = destinationLoc.DistanceToCapital;
                                //get Position of Connector origin (on alternate branch)
                                Position pos = new Position();
                                pos.PosX = ArrayOfConnectors[originBranch, 4];
                                pos.PosY = ArrayOfConnectors[originBranch, 5];
                                //get distance to traverse connector
                                distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                                //find loc in Dictionary
                                Location loc = new Location();
                                locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                                if (dictLocations.TryGetValue(locID, out loc))
                                {
                                    distanceAlternate = loc.DistanceToCapital;
                                    //distance origin->Connector + Connector + ConnectorDestination -> Capital + Capital->Destination
                                    if ((distanceToCapital + distanceToDestination) > (distanceToConnector + distanceAcrossConnector + distanceAlternate + distanceToDestination))
                                    {
                                        //build up composite route
                                        listOfAlternateRoutes.AddRange(originLoc.GetRouteToConnector());
                                        //find connector route
                                        foreach (Route route in ListOfConnectorRoutes)
                                        {
                                            Position posEnd = route.GetLoc2();
                                            if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                            {
                                                //add to combined route ('Add' because it's a single route, not a collection)
                                                listOfAlternateRoutes.Add(route);
                                                break;
                                            }
                                        }
                                        //add to combined route
                                        listOfAlternateRoutes.AddRange(loc.GetRouteToCapital());
                                        listOfAlternateRoutes.AddRange(destinationLoc.GetRouteFromCapital());
                                    }
                                }
                            }
                            //DESTINATION & ORIGIN but different branches (route checks either side of capital as has to transit through capital)
                            //use listOfAlternateRoutes for both straight shots and alternate, connector, routes for all
                            else
                            {
                                //check options for ORIGIN -> Capital -
                                distanceToCapital = originLoc.DistanceToCapital;
                                distanceToConnector = originLoc.DistanceToConnector;
                                //connector exists?
                                originBranch = originLoc.GetCapitalRouteDirection();
                                //get Position
                                Position pos = new Position();
                                pos.PosX = ArrayOfConnectors[originBranch, 4];
                                pos.PosY = ArrayOfConnectors[originBranch, 5];
                                //get distance to traverse connector
                                distanceAcrossConnector = ArrayOfConnectors[originBranch, 1];
                                //find loc in Dictionary
                                Location loc = new Location();
                                locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                                if (dictLocations.TryGetValue(locID, out loc))
                                {
                                    //get distance to Capital
                                    distanceAlternate = loc.DistanceToCapital;
                                    //straight shot to capital > origin -> Connector -> Capital?
                                    if (distanceToCapital > (distanceAlternate + distanceToConnector + distanceAcrossConnector))
                                    {
                                        //compile combined route (origin -> connector -> capital)
                                        listOfAlternateRoutes.AddRange(originLoc.GetRouteToConnector());
                                        //find connector route
                                        foreach (Route route in ListOfConnectorRoutes)
                                        {
                                            Position posEnd = route.GetLoc2();
                                            if (posEnd.PosX == pos.PosX && posEnd.PosY == pos.PosY)
                                            {
                                                //add to combined route ('Add' because it's a single route, not a collection)
                                                listOfAlternateRoutes.Add(route);
                                                break;
                                            }
                                        }
                                        //add to combined route
                                        listOfAlternateRoutes.AddRange(loc.GetRouteToCapital());
                                    }
                                    else
                                    {
                                        //default straight shot out to Capital along branch
                                        listOfAlternateRoutes.AddRange(originToCapitalRoutes);
                                    }
                                }

                                //next check options for Capital -> DESTINATION - use quickest -
                                distanceToCapital = destinationLoc.DistanceToCapital;
                                distanceToConnector = destinationLoc.DistanceToConnector;
                                //get Position
                                Position posConn = new Position();
                                posConn.PosX = ArrayOfConnectors[destinationBranch, 4];
                                posConn.PosY = ArrayOfConnectors[destinationBranch, 5];
                                //get distance to traverse connector
                                distanceAcrossConnector = ArrayOfConnectors[destinationBranch, 1];
                                //find loc in Dictionary
                                Location locConn = new Location();
                                locID = Game.map.GetMapInfo(MapLayer.LocID, posConn.PosX, posConn.PosY);
                                if (dictLocations.TryGetValue(locID, out locConn))
                                {
                                    //get distance to Capital
                                    distanceAlternate = locConn.DistanceToCapital;
                                    //check combined distance < Capital -> Connector -> Destination
                                    if (distanceToCapital > (distanceAlternate + distanceToConnector + distanceAcrossConnector))
                                    {
                                        //compile combined route (capital -> connector -> destination)
                                        listOfAlternateRoutes.AddRange(locConn.GetRouteFromCapital());
                                        //find connector route
                                        foreach (Route route in ListOfConnectorRoutes)
                                        {
                                            Position posEnd = route.GetLoc1();
                                            if (posEnd.PosX == posConn.PosX && posEnd.PosY == posConn.PosY)
                                            {
                                                //add to combined route ('Add' because it's a single route, not a collection)
                                                listOfAlternateRoutes.Add(route);
                                                break;
                                            }
                                        }
                                        //add to combined route
                                        listOfAlternateRoutes.AddRange(destinationLoc.GetRouteToConnector());
                                    }
                                    else
                                    {
                                        //default straight shot out from Capital along branch
                                        listOfAlternateRoutes.AddRange(destinationFromCapitalRoutes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //which List of Routes to use? 
            List<Route> returnRoutes = new List<Route>();
            if (listOfAlternateRoutes.Count > 0)
            { returnRoutes = listOfAlternateRoutes; }
            else
            { returnRoutes = listOfDirectRoutes; }
            //return & display info line
            /*int distance = GetDistance(returnRoutes);
            string originName = GetLocationName(posOrigin);
            string destinationName = GetLocationName(posDestination);
            Console.WriteLine();
            Console.WriteLine("Route from {0} to {1} distance {2}", originName, destinationName, distance);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();*/
            return returnRoutes;
        }



        //prints highlighted text for GetRouteOnSameBranch, eg. POP in blue to better show what's going on in the debug trace
        private void ColourText(string text, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "  ");
            //revert back to normal
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //find all immediate neighbours for each location
        private void InitialiseNeighbours()
        {
            //loop dictionary of locations
            Position pos = new Position();
            Location loc = new Location();
            foreach(KeyValuePair<int,Location> pair in dictLocations)
            {
                //key value pair
                pos = pair.Value.GetPosition();
                loc = pair.Value;
                //loop list of routes
                foreach(Route route in ListOfRoutes)
                {
                    Position posLoc1 = route.GetLoc1();
                    Position posLoc2 = route.GetLoc2();
                    Position posNeighbour = new Position();
                    //match location with route origin?
                    if(pos.PosX == posLoc1.PosX && pos.PosY == posLoc1.PosY)
                    {
                        posNeighbour.PosX = posLoc2.PosX;
                        posNeighbour.PosY = posLoc2.PosY;
                        posNeighbour.Distance = route.GetDistance();
                        loc.AddNeighbour(posNeighbour);
                    }
                    //match location with route destination?
                    else if (pos.PosX == posLoc2.PosX && pos.PosY == posLoc2.PosY)
                    {
                        posNeighbour.PosX = posLoc1.PosX;
                        posNeighbour.PosY = posLoc1.PosY;
                        posNeighbour.Distance = route.GetDistance();
                        loc.AddNeighbour(posNeighbour);
                    }
                    //if a valid route found, add details to Location object's list of neighbours
                    //if (posNeighbour != null)
                    //{ loc.AddNeighbour(posNeighbour); }
                }
            }
        }

        //general input method
        public void NormalInput(string input)
        {
            Location loc = new Location();
            Position pos = GetLocationInput("Location");

            //Location display
            if (input.Equals("L"))
            {
                int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
                if (dictLocations.ContainsKey(locID))
                { loc = dictLocations[locID]; loc.ShowStatus(); }
                else
                { Console.WriteLine("Debug: Location {0}:{1} doesn't exist in the dictLocations", pos.PosX, pos.PosY); }
            }
        }

        //returns a list of Routes based on input
        private List<Route> RouteInput(string input)
        {
            List<Route> listOfRoutes = new List<Route>();
            //get location
            Position pos1 = GetLocationInput("ORIGIN");
            //Return route of location to Capital
            if (input.Equals("C"))
            { listOfRoutes = RouteToCapital(pos1); }
            else
            {
                //Double Position Input (from A to B)
                Position pos2 = GetLocationInput("DESTINATION");
                //check that the two coords aren't identical
                if ((pos1 != null && pos2 != null) && (pos1.PosX != pos2.PosX || pos1.PosY != pos2.PosY))
                {
                    //Return routes in debug mode (show draw order on map)
                    if (input.Equals("D"))
                    { listOfRoutes = GetRouteAnywhere(pos1, pos2); }
                    //Return routes between locations anywhere on map
                    else if (input.Equals("G"))
                    { listOfRoutes = GetRouteAnywhere(pos1, pos2); }
                }
                else
                { Game.SetError(new Error(6, "identical Origin and Destination or the inputs are Not valid Loc's")); }
            }
                return listOfRoutes;
        }


        /// <summary>
        /// Get a location from Dictionary of Locations via locID, returns empty location (locID = 0) if not found
        /// </summary>
        /// <param name="locID"></param>
        /// <returns></returns>
        internal Location GetLocation(int locID)
        {
            Location loc = new Location();
            if (dictLocations.ContainsKey(locID))
            { loc = dictLocations[locID]; }
            else
            { Game.SetError(new Error(149, "locID not found in dictionary")); }
            return loc;
        }

        internal Dictionary<int, Location> GetLocations()
        { return dictLocations; }

        internal List<Location> GetLocationsList()
        { return ListOfLocations; }

        public int GetNumLocations()
        { return ListOfLocations.Count(); }

        public int GetNumSpecialLocations()
        { return numSpecialLocations; }

        private Position GetLocationInput(string name)
        {
            Console.WriteLine("Input {0} Coordinates in format X,Y", name);
            string coords = Console.ReadLine();
            string[] tokens = coords.Split(',');
            //Input Position
            Position pos = new Position();
            pos.PosX = Convert.ToInt32(tokens[0]);
            pos.PosY = Convert.ToInt32(tokens[1]);
            int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
            //check a valid location, return null if not
            if (dictLocations.ContainsKey(locID) == false)
            { pos = null; }
            return pos;
        }


        //test function to provide route to capital from input location, returns null if not found
        private List<Route> RouteToCapital(Position pos)
        {
            List<Route> listOfRoutes = null;
            Location loc = new Location();
            int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
            if (dictLocations.ContainsKey(locID) == true)
            {
                loc = dictLocations[locID];
                listOfRoutes = loc.GetRouteToCapital();
            }
            else
            { Console.WriteLine("Debug: Location {0}:{1} doesn't exist in the dictLocations", pos.PosX, pos.PosY); }
            return listOfRoutes;
        }

        public void PrintNetworkStatus()
        {
            Console.WriteLine("Debug: Network Summary ---");
            Console.WriteLine("Debug: dictLocations has {0} records", dictLocations.Count);
            Console.WriteLine("Debug: listOfLocationNames has {0} records", listOfLocationNames.Count);
        }


        //returns random location name from locations.txt list
        private string GetRandomLocationName()
        {
            int index;
            string name;
            index = rnd.Next(0, listOfLocationNames.Count);
            //get name
            name = listOfLocationNames[index];
            //delete record in list to prevent duplicate names
            try
            { listOfLocationNames.RemoveAt(index); }
            catch (Exception e)
            { Game.SetError(new Error(61, e.Message)); }
            return name;
        }

        //returns Location name for a given Position
        internal string GetLocationName(Position pos)
        {
            string locName = null;
            Location loc = new Location();
            int locID = Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
            if (dictLocations.TryGetValue(locID, out loc))
            { locName = loc.LocName; }
            return locName;
        }

        //returns distance for a given list of routes
        internal int GetDistance(List<Route> listOfRoutes)
        {
            int distance = 0;
            foreach(Route route in listOfRoutes)
            { distance += route.GetDistance(); }
            return distance;
        }

        /// <summary>
        /// Shows route details (returns a formated string)
        /// </summary>
        /// <param name="listOfRoutes"></param>
        /// <returns></returns>
        internal string ShowRouteDetails(List<Route> listOfRoutes)
        {
            string returnString = "Error: ShowRouteDetails";
            //return & display info line
            int distance = GetDistance(listOfRoutes);
            Position posOrigin = listOfRoutes[0].GetRouteStart();
            Position posDestination = listOfRoutes[listOfRoutes.Count - 1].GetRouteEnd();
            string originName = GetLocationName(posOrigin);
            string destinationName = GetLocationName(posDestination);
            //Console.WriteLine("Route from {0} to {1} distance {2}", originName, destinationName, distance);
            returnString = string.Format("Route from {0} to {1} distance {2}", originName, destinationName, distance);
            return returnString;
        }

        /// <summary>
        /// Debug method, console output
        /// </summary>
        public void ShowNetworkAnalysis()
        {
            Console.WriteLine();
            Console.WriteLine("--- Network Analysis");
            for (int i = 0; i <= arrayOfNetworkAnalysis.GetUpperBound(0); i++)
            {
                int locConsole = arrayOfNetworkAnalysis[i, (int)NetGrid.Locations];
                int connConsole = arrayOfNetworkAnalysis[i, (int)NetGrid.Connections];
                Console.WriteLine($"Dir {i, -5} {locConsole, 5} Locations {connConsole, 5} Connections");
            }
        }


        /// <summary>
        /// returns a list of groups of locations for houses and specials
        /// </summary>
        /// <param name="maxHouses">max. number of houses there could be in the land</param>
        /// <param name="idealLocs">ideal number of locations per house, used as a divisor</param>
        public void InitialiseHouseLocations(int maxHouses, int idealLocs)
        {
            int numBranches = 5; //1 to 4 loop counter and array sizer
            int totalLocs = 0;
            for (int i = 1; i < numBranches; i++)
            {
                //tally up number of locations (exclude capital)
                totalLocs += arrayOfNetworkAnalysis[i, (int)NetGrid.Locations];
                //copy across location numbers to [i, 3] (used for calcs, keep original for reference)
                arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] = arrayOfNetworkAnalysis[i, (int)NetGrid.Locations];
            }
            //work out number of houses based on how many Locations
            int numHouses = totalLocs / idealLocs;
            //don't exceed cap
            numHouses = Math.Min(numHouses, maxHouses);
            int housesTally = numHouses;
            int totalSpecials = 0;
            if( numHouses > 2 )
            {
                for (int i = 1; i < numBranches; i++)
                {
                    //check for special locations first and subtract from location totals in [,3] (store # special locations in [i, 4]
                    if(arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] == 1)
                    { arrayOfNetworkAnalysis[i, (int)NetGrid.Specials]++; arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs]--; totalSpecials++; }
                    //first loc from capital has 3 or 4 connections and >= 6 locs on branch => inn
                    if(arrayOfNetworkAnalysis[i, (int)NetGrid.Connections] >= 3 && arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] >= 6)
                    { arrayOfNetworkAnalysis[i, (int)NetGrid.Specials]++; arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs]--; totalSpecials++; }
                    //allocate house - special case to accommodate branches with a small number of locations (they end up with a default house regardless)
                    if (arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] >= 2)
                    {
                        // [,2] is used to keep a tally of houses in each branch
                        arrayOfNetworkAnalysis[i, (int)NetGrid.Houses]++;
                        housesTally--;
                    }
                }
            }
            //check at least one special
            if( totalSpecials < 1)
            {
                //allocate first link in first branch that has > 3 locations as a special, regardless of connections.
                for(int i = 1; i < numBranches; i++)
                {
                    if (arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] > 3)
                    { arrayOfNetworkAnalysis[i, (int)NetGrid.Specials]++; arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs]--; totalSpecials++;  break; }
                }
            }
            numSpecialLocations = totalSpecials;
            //allocate houses - the bulk of them
            bool changeFlag = false;
            do
            {
                changeFlag = false;
                for(int i = 1; i < numBranches; i++)
                {
                    //enough locs in branch to support a house?
                    int branchHouses = arrayOfNetworkAnalysis[i, (int)NetGrid.Houses];
                    if(arrayOfNetworkAnalysis[i, (int)NetGrid.WorkingLocs] >= idealLocs * (branchHouses + 1) && housesTally > 0)
                    {
                        arrayOfNetworkAnalysis[i, (int)NetGrid.Houses]++;
                        housesTally--;
                        changeFlag = true;
                    }
                }
                //to prevent being stuck in an endless loop (no branch meets criteria)
                if(changeFlag == false)
                { break; }
            }
            while (housesTally > 0);
           
            //debug output
            Console.WriteLine();
            Console.WriteLine("--- InitialiseHouseLocations");
            Console.WriteLine("Total Locations (excluding Capital) {0}", totalLocs);
            Console.WriteLine("Number of Houses {0}, Max Cap on House Numbers {1}", numHouses, maxHouses);
            Console.WriteLine();
            for(int i = 1; i < numBranches; i++)
            { Console.WriteLine("Branch {0} has {1} Houses allocated, Special Locations {2}", i, arrayOfNetworkAnalysis[i, (int)NetGrid.Houses], arrayOfNetworkAnalysis[i, (int)NetGrid.Specials]); }
            Console.WriteLine();
            Console.WriteLine("Total Houses Allocated {0} out of {1}", numHouses - housesTally, numHouses);

            List<Location> branchList = new List<Location>();
            List<int> locConnections = new List<int>();
            int totalHouseTally = numHouses - housesTally;
            int houseCounter = 1; //used to assign house ID's (1 to ...)
            //tracks finished result (houses)
            int[][] masterStatus = new int[numBranches][];
            int[][] masterLocID = new int[numBranches][];
            arrayOfCapitals = new int[0]; //locID of all house capitals
            //Initialise Capital as House 1 (Royal Household)
            //Game.map.SetMapInfo(MapLayer.Houses, capitalX, capitalY, 1);
            //
            //loop through each branch ---
            //
            for (int branch = 1; branch < numBranches; branch++)
            {
                switch (branch)
                {
                    case 1:
                        //no need to clear branch for first use
                        branchList.AddRange(listNorthBranch);
                        break;
                    case 2:
                        branchList.Clear();
                        branchList.AddRange(listEastBranch);
                        break;
                    case 3:
                        branchList.Clear();
                        branchList.AddRange(listSouthBranch);
                        break;
                    case 4:
                        branchList.Clear();
                        branchList.AddRange(listWestBranch);
                        break;
                }
                int countLocs = branchList.Count;
                //create array to mark which loc's have been used (0 - unassigned, 99 - special, 1+ house ID)
                int[] arrayStatus = new int[countLocs];
                int[] arrayLocID = new int[countLocs]; //duplicate to hold locId to speed up 'fill in the blanks' at the end
                int branchHouseTally;
                int houseLocTally;
                int locID = 0;
                bool innerStatus = false; //do-while exit flags
                bool outerStatus = false;
                bool newNode = false; //new node found
                //bool finishLoop = false;
                bool assignedLoc = false;
                //first check for special location (will automatically be the first location in the sorted List)
                if (arrayOfNetworkAnalysis[branch, (int)NetGrid.Specials] > 0)
                { arrayStatus[0] = 99; }
                //skip house allocation for a branch if 1, or 0, locations (1 would be automatically a special location)
                if (countLocs > 1)
                {
                    //how many houses assigned to branch
                    branchHouseTally = arrayOfNetworkAnalysis[branch, (int)NetGrid.Houses];
                    //number of loc's assigned to a house
                    houseLocTally = 0;
                    bool foundFlag = false;
                    //start at end branch and work in
                    do
                    {
                        //search list for first end of branch location
                        Location loc_1 = new Location();
                        //loop branch list to find first unassigned end of branch location
                        for (int i = 0; i < countLocs; i++)
                        {
                            foundFlag = false;
                            //is location unassigned?
                            if (arrayStatus[i] == 0)
                            {
                                loc_1 = branchList[i];
                                //end of branch?
                                if (loc_1.Connections == 1)
                                {
                                    /*Console.WriteLine("-- New House: Loc {0}:{1} {2} TotalHouseTally: {3} Connections {4} houseCounter", 
                                        loc_1.GetPosX(), loc_1.GetPosY(), loc_1.LocName, totalHouseTally, loc_1.Connections, houseCounter);*/
                                    //assign to highest number house
                                    arrayStatus[i] = houseCounter;
                                    houseLocTally = 1;
                                    //Console.WriteLine("Before loop: houseLocTally: {0}", houseLocTally);
                                    //update tally of locs assigned to house
                                    foundFlag = true;
                                    innerStatus = false;
                                    //keep going with same house number until a limit is reached
                                    do
                                    {
                                        locID = 0;
                                        newNode = false;
                                        //find immediate neighbour (same house)
                                        locConnections.Clear();
                                        locConnections.AddRange(loc_1.GetNeighboursLocID());
                                        int connectionsCount = locConnections.Count;
                                        //Single connection
                                        if (connectionsCount <= 2)
                                        {
                                            //get next node
                                            locID = locConnections[0];
                                            newNode = true;
                                            //check # loc's per house not exceeded, if so then exit
                                            if (houseLocTally == idealLocs - 1)
                                            { innerStatus = true;  }
                                            else
                                            { innerStatus = false; }
                                        }
                                        else if (connectionsCount > 2)
                                        {
                                            //loop list of neighbours looking for all single conection nodes (automatically assign regardless of houseLocCount)
                                            for (int index = 0; index < connectionsCount; index++)
                                            {
                                                int multiID = locConnections[index];
                                                //find location in list
                                                for (int p = 0; p < countLocs; p++)
                                                {
                                                    if (branchList[p].LocationID == multiID)
                                                    {
                                                        Location loc_2 = branchList[p];
                                                        //assign to same house, IF free
                                                        //Console.WriteLine("MULTI: Loc {0}:{1} Connections: {2}  arrayStatus: {3}", loc_2.GetPosX(), loc_2.GetPosY(), loc_2.Connections, arrayStatus[p]);
                                                        if (loc_2.Connections <= 2 && arrayStatus[p] == 0)
                                                        { arrayStatus[p] = houseCounter; }
                                                    }
                                                }
                                            }
                                            //exit
                                            innerStatus = true;
                                        }

                                        /*Console.WriteLine("Same House: Loc {0}:{1}  newNode: {2}, innerStatus: {3}, totalHouseTally: {4}, ID: {5}, houseCounter {6}", 
                                            loc_1.GetPosX(), loc_1.GetPosY(), newNode, innerStatus, totalHouseTally, locID, houseCounter);*/

                                        //new node found?
                                        if (newNode == true)
                                        {
                                            assignedLoc = false;
                                            int branchIndex = 0;
                                            //assign node - find in List of branchLocs
                                            for (int p = 0; p < countLocs; p++)
                                            {
                                                if (branchList[p].LocationID == locID && arrayStatus[p] == 0)
                                                {
                                                    //assign to same house, IF free
                                                    arrayStatus[p] = houseCounter;
                                                    houseLocTally++;
                                                    //Console.WriteLine("newMode == true: houseLocTally: {0}", houseLocTally);
                                                    assignedLoc = true;
                                                    branchIndex = p;
                                                    break;
                                                }
                                            }
                                            //if single connector and that node isn't viable, exit.
                                            if (assignedLoc == false)
                                            {
                                                if (connectionsCount <= 2)
                                                { innerStatus = true; }
                                            }
                                            //give value of new node to loc_1
                                            else if (assignedLoc == true && innerStatus == false)
                                            {
                                                //roll over to next location
                                                loc_1 = branchList[branchIndex];
                                            }
                                        }
                                    }
                                    while (innerStatus == false);

                                    //update house count
                                    if (foundFlag == true)
                                    { branchHouseTally--; totalHouseTally--; houseCounter++; }
                                    else if (foundFlag == false)
                                    { break; }
                                    //no more houses?
                                    if (branchHouseTally < 1)
                                    { outerStatus = true; break; }
                                }
                            }
                        }
                        //didn't find any suitable nodes after a full loop through?
                        if (foundFlag == false)
                        { outerStatus = true; }
                    }
                    while (outerStatus == false && totalHouseTally > 0);
                }
                //Console.WriteLine("--- Set HouseID's");

                //use arrayStatus to update House ID's on house MapGrid layer
                for (int i = 0; i < branchList.Count; i++)
                {
                    Location loc = branchList[i];
                    Game.map.SetMapInfo(MapLayer.HouseID, loc.GetPosX(), loc.GetPosY(), arrayStatus[i]);
                    loc.HouseID = arrayStatus[i];
                    //Console.WriteLine("Loc {0}:{1} arrayStatus: {2} ID: {3}", loc.GetPosX(), loc.GetPosY(), arrayStatus[i], loc.LocationID);
                    arrayLocID[i] = loc.LocationID;
                    
                }

                //fill in the blanks (if adjacent node has a house number, use that
                bool updateFlag = false;
                do
                {
                    updateFlag = false;
                    int neighbourID = 0;
                    int houseID = 0;
                    for (int i = 0; i < arrayStatus.Length; i++)
                    {
                        if (arrayStatus[i] == 0)
                        {
                            {
                                //location on branchList, get list of neighbours
                                Location loc_3 = branchList[i];
                                List<int> listOfNeighbours = loc_3.GetNeighboursLocID();
                                //loop list of neighbours looking for any with a house # assigned
                                for (int n = 0; n < listOfNeighbours.Count; n++)
                                {
                                    neighbourID = listOfNeighbours[n];
                                    //find index to use
                                    for (int index = 0; index < arrayLocID.Length; index++)
                                    {
                                        if (arrayLocID[index] == neighbourID)
                                        {
                                            //get house ID
                                            houseID = arrayStatus[index];
                                            if (houseID > 0 && houseID != 99)
                                            {
                                                //house number - assign to current node and break;
                                                arrayStatus[i] = houseID;
                                                Game.map.SetMapInfo(MapLayer.HouseID, loc_3.GetPosX(), loc_3.GetPosY(), houseID);
                                                loc_3.HouseID = arrayStatus[i];
                                                updateFlag = true;
                                                break;
                                            }
                                        }
                                    }
                                    //back out of search if already got a hit - one per iteration
                                    if (updateFlag == true)
                                    { break; }
                                }
                            }
                        }
                    }
                }
                while (updateFlag == true);
                
                //check for orphaned routes (a sub branch leading out from a special location)
                bool unassignedFlag = false;
                bool rerunNeeded = false;
                int specialIndex = 0;
                int newHouseID = 0;

                if (arrayOfNetworkAnalysis[branch, (int)NetGrid.Specials] > 0)
                {
                    //loop through arrayStatus and find special index and if there are any locations without an assigned houseID
                    for (int i = 0; i < arrayStatus.Length; i++)
                    {
                        if (arrayStatus[i] == 99)
                        { specialIndex = i; }
                        else if (arrayStatus[i] == 0)
                        { unassignedFlag = true; }
                    }
                }
                //any unassigned locations?
                if (unassignedFlag == true)
                {
                    //get neighbour list for Special location
                    Location loc_4 = branchList[specialIndex];
                    List<int> specialNeighbours = loc_4.GetNeighboursLocID();
                    //loop list of neighbours looking for a valid house # (first one you find)
                    for(int index = 0; index < specialNeighbours.Count; index++)
                    {
                        newHouseID = 0;
                        int nearID = specialNeighbours[index];
                        //does ID have a houseID?
                        for(int k = 0; k < arrayLocID.Length; k++)
                        {
                            if (arrayLocID[k] == nearID)
                            {
                                newHouseID = arrayStatus[k];
                                if (newHouseID > 0)
                                { break; }
                            }
                        }
                        //exit again
                        if (newHouseID > 0)
                        { break; }
                    }
                    //assign to neighbouring loc with no houseID
                    if (newHouseID > 0)
                    {
                        //loop list of special location neighbours looking for an unassigned location
                        for (int index = 0; index < specialNeighbours.Count; index++)
                        {
                            int nearID = specialNeighbours[index];
                            //find index
                            for (int k = 0; k < arrayLocID.Length; k++)
                            {
                                if (arrayLocID[k] == nearID && arrayStatus[k] == 0)
                                {
                                    //don't break, loop through and assign to ALL unassigned locations.
                                    arrayStatus[k] = newHouseID;
                                    Location loc_5 = branchList[k];
                                    Game.map.SetMapInfo(MapLayer.HouseID, loc_5.GetPosX(), loc_5.GetPosY(), newHouseID);
                                    loc_5.HouseID = arrayStatus[k];
                                    rerunNeeded = true;
                                } 
                            }
                        }
                    }
                }
                //Have any neighbours of the special location been assigned a like houseID?
                if (rerunNeeded == true)
                {
                    //need to rerun base 'fill in the blanks' functionality to allocate all unassigned locations (which will key off the loc adjacent to the special, above)
                    do
                    {
                        updateFlag = false;
                        int neighbourID = 0;
                        int houseID = 0;
                        for (int i = 0; i < arrayStatus.Length; i++)
                        {
                            if (arrayStatus[i] == 0)
                            {
                                {
                                    //location on branchList, get list of neighbours
                                    Location loc_3 = branchList[i];
                                    List<int> listOfNeighbours = loc_3.GetNeighboursLocID();
                                    //loop list of neighbours looking for any with a house # assigned
                                    for (int n = 0; n < listOfNeighbours.Count; n++)
                                    {
                                        neighbourID = listOfNeighbours[n];
                                        //find index to use
                                        for (int index = 0; index < arrayLocID.Length; index++)
                                        {
                                            if (arrayLocID[index] == neighbourID)
                                            {
                                                //get house ID
                                                houseID = arrayStatus[index];
                                                if (houseID > 0 && houseID != 99)
                                                {
                                                    //house number - assign to current node and break;
                                                    arrayStatus[i] = houseID;
                                                    Game.map.SetMapInfo(MapLayer.HouseID, loc_3.GetPosX(), loc_3.GetPosY(), houseID);
                                                    loc_3.HouseID = arrayStatus[i];
                                                    updateFlag = true;
                                                    break;
                                                }
                                            }
                                        }
                                        //back out of search if already got a hit - one per iteration
                                        if (updateFlag == true)
                                        { break; }
                                    }
                                }
                            }
                        }
                    }
                    while (updateFlag == true);
                }
                //update master Arrays (data for all branches)
                masterStatus[branch] = new int[arrayStatus.Length];
                masterLocID[branch] = new int[arrayLocID.Length];
                //copy data across
                for(int i = 0; i < arrayStatus.Length; i++)
                {
                    masterStatus[branch][i] = arrayStatus[i];
                    masterLocID[branch][i] = arrayLocID[i];
                }
            }
            //
            //create analysis data structures ---
            //
            Console.WriteLine();
            Console.WriteLine("--- House Analysis");
            //loop through MasterList and populate Lists
            uniqueHouses = 0;
            for(int outer = 1; outer < numBranches; outer++)
            {
                int arrayLength = masterStatus[outer].GetUpperBound(0);
                //Console.WriteLine("- Branch {0} upperBound {1}", outer, arrayLength);
                //only if branch isn't empty
                if (arrayLength > -1)
                {
                    List<int> subListUnique = new List<int>();
                    int houseValue = 0;
                    for (int inner = 0; inner < arrayLength + 1; inner++)
                    {
                        houseValue = masterStatus[outer][inner];
                        //Console.WriteLine("masterStatus[{0}][{1}] = {2}", outer, inner, houseValue);
                        //populate a list with unique houses for each branch
                        if(subListUnique.Contains(houseValue) == false && houseValue != 99)
                        { subListUnique.Add(houseValue); uniqueHouses++; }
                    }
                    //add to master list
                    listUniqueHousesByBranch.Add(subListUnique);
                }
            }
            //group LocID's by houses
            if (uniqueHouses > 0)
            {
                //Console.WriteLine();
                //Console.WriteLine("Unique Houses: {0}", uniqueHouses);
                //Set up subLists, one for each unique house, access by house #
                for (int i = 0; i < uniqueHouses + 1; i++)
                {
                    List<int> sublist = new List<int>();
                    listIndividualHouseLocID.Add(sublist);
                }
                //initialise array holding capital locID's
                arrayOfCapitals = new int[uniqueHouses + 1];
                //loop through MasterList and populate Lists
                for (int outer = 1; outer < numBranches; outer++)
                {
                    int arrayLength = masterStatus[outer].GetUpperBound(0);
                    if (arrayLength > -1)
                    {
                        for (int inner = 0; inner < arrayLength + 1; inner++)
                        {
                            int houseID = masterStatus[outer][inner];
                            int locID = masterLocID[outer][inner];
                            if (houseID != 99)
                            {
                                //populate array with locID's grouped by houses
                                listIndividualHouseLocID[houseID].Add(locID);
                            }
                        }
                    }
                }
                //determine House Capitals
                for(int outer = 0; outer < listIndividualHouseLocID.Count; outer++)
                {
                    int numLocs = listIndividualHouseLocID[outer].Count;
                    int locID = 0;
                    //create 2 arrays: first for # connections that Loc has, 2nd # routes to Capital
                    int[] tempArrayConnections = new int[numLocs];
                    int[] tempArrayRoutes = new int[numLocs];
                    //Console.WriteLine("- House {0}", outer);
                    for(int inner = 0; inner < numLocs; inner++)
                    {
                        locID = listIndividualHouseLocID[outer][inner];
                        Location tempLoc = GetLocation(locID);
                        if (locID > 0)
                        {
                            tempArrayConnections[inner] = tempLoc.Connections;
                            tempArrayRoutes[inner] = tempLoc.GetNumRoutesToCapital();
                            //Console.WriteLine("LocID {0} has {1} connections and is {2} routes from the Capital", locID, tempLoc.Connections, tempLoc.GetNumRoutesToCapital());
                        }
                    }
                    //if only a single loc then it must be the house capital
                    if(numLocs == 1)
                    {
                        //only loc must automatically be the capital
                        locID = listIndividualHouseLocID[outer][0];
                        Location loc = GetLocation(locID);
                        Game.map.SetMapInfo(MapLayer.Capitals, loc.GetPosX(), loc.GetPosY(), outer);
                        arrayOfCapitals[outer] = locID;
                    }
                    //which location will be capital? (highest # connections first, if equal then loc furtherst from Capital)
                    else if (numLocs > 1)
                    {
                        int maxValue = tempArrayConnections.Max();
                        int indexValue = 0;
                        int routeValue = 0;
                        //# connections
                        for(int i = 0; i < tempArrayConnections.Length; i++)
                        {
                            if(tempArrayConnections[i] == maxValue)
                            {
                                //duplicate connection values, does it have a longer route from capital?
                                if (tempArrayRoutes[i] > routeValue)
                                { routeValue = tempArrayRoutes[i]; indexValue = i; }
                            }
                        }
                        //capital is index value in List
                        locID = listIndividualHouseLocID[outer][indexValue];
                        Location loc = GetLocation(locID);
                        Game.map.SetMapInfo(MapLayer.Capitals, loc.GetPosX(), loc.GetPosY(), outer);
                        arrayOfCapitals[outer] = locID;
                    }
                }
            }
            
            //debug list contents
            Console.WriteLine();
            Console.WriteLine("--- listOfAllHouses");
            for (int i = 0; i < listUniqueHousesByBranch.Count; i++)
            { Console.WriteLine("Branch {0} has {1} records", i + 1, listUniqueHousesByBranch[i].Count); }
            Console.WriteLine();
            Console.WriteLine("--- listIndividualHouseLocID");
            for (int i = 0; i < listIndividualHouseLocID.Count; i++)
            { Console.WriteLine("House {0} has {1} records", i, listIndividualHouseLocID[i].Count); }
            Console.WriteLine();
            Console.WriteLine("--- House Capitals");
            for(int i = 0; i < arrayOfCapitals.Length; i++)
            { Console.WriteLine("House {0} has ID {1} as it's capital", i, arrayOfCapitals[i]); }
            
        }

        /// <summary>
        /// Returns the actual number of Houses required for the map
        /// </summary>
        /// <returns></returns>
        public int GetNumUniqueHouses()
        { return uniqueHouses; }


        /// <summary>
        /// Takes basic house objects from History and assigns them to HouseID's
        /// </summary>
        /// <param name="listOfHouses"></param>
        internal void UpdateHouses(List<MajorHouse> listOfHouses)
        {
            //List<House> returnListOfHouses = listOfHouses;
            Console.WriteLine();
            //set up a quick list for randomly assigning houses to houseID's
            List<int> randomList = new List<int>();
            for(int i = 0; i < listOfHouses.Count; i++)
            { randomList.Add(i + 1); }
            int randomIndex = 0;
            //Assign Capitals
            for(int i = 1; i < arrayOfCapitals.Length; i++)
            {
                House house = new House();
                house = listOfHouses[i - 1];
                house.LocID = arrayOfCapitals[i];
                //assign a random house ID (from available) to house
                randomIndex = rnd.Next(1, randomList.Count + 1);
                house.HouseID = randomList[randomIndex - 1];
                try
                { randomList.RemoveAt(randomIndex - 1); }
                catch (Exception e)
                { Game.SetError(new Error(61, e.Message)); }
                Console.WriteLine("House {0} has LocID {1} and HouseID {2}", house.Name, house.LocID, house.HouseID);
            }
            //loop houses and update data
            int houseID;
            int capitalLocID;
            int locID;
            int minorHouseID;
            //Console.WriteLine();
            for(int i = 0; i < listOfHouses.Count; i++)
            {
                MajorHouse house = new MajorHouse();
                house = listOfHouses[i];
                //MajorHouse majorHouse = house as MajorHouse;
                houseID = house.HouseID;
                //change name of Location to house name
                capitalLocID = arrayOfCapitals[houseID];
                Location loc = GetLocation(capitalLocID);
                loc.LocName = house.LocName;
                loc.RefID = house.RefID;
                //update capital Loc ID & branch
                house.LocID = capitalLocID;
                house.Branch = loc.GetCapitalRouteDirection();
                //update all house locations for house
                for(int k = 0; k < listIndividualHouseLocID[houseID].Count; k++)
                {
                    locID = listIndividualHouseLocID[houseID][k];
                    //if not capital, add to house list
                    if( locID != capitalLocID && locID > 0)
                    {
                        house.AddBannerLordLocation(locID);
                        //assign a Minor House (bannerlord)
                        Game.history.InitialiseMinorHouse(locID, houseID);
                    }
                }
                //Work out unique Loc's from house capital to kingdom capital
                List<Route> tempListOfRoutes = loc.GetRouteToCapital();
                //Console.WriteLine("--- House {0}", houseID);
                foreach(Route route in tempListOfRoutes)
                {
                    Position pos = route.GetLoc1();
                    minorHouseID = Game.map.GetMapInfo(MapLayer.HouseID, pos.PosX, pos.PosY);
                    //called method checks for locID = 0 & duplicate houseID's
                    if (minorHouseID != houseID && minorHouseID != 99)
                    { house.AddHousesToCapital(minorHouseID); }
                    //Console.WriteLine("House {0}", minorHouseID);
                }
            }
        }


        /// <summary>
        /// used to randomly place player characters at start (debug) -> Excludes Capital
        /// </summary>
        /// <returns>LocID</returns>
        internal int GetRandomLocation()
        {
            List<int> listOfKeys = new List<int>(dictLocations.Keys);
            int randomKey = rnd.Next(1, listOfKeys.Count);
            return listOfKeys[randomKey];
        }

        /// <summary>
        /// returns the # of Branches (1 to 4) by checking for any branch with zero locations
        /// </summary>
        /// <returns></returns>
        internal int GetNumBranches()
        {
            int branches = 4;
            for (int i = 1; i <= arrayOfNetworkAnalysis.GetUpperBound(0); i++)
            { if (arrayOfNetworkAnalysis[i, 0] == 0) { branches--; } }
            return branches;
        }

        /// <summary>
        /// returns total # of Loc's on a specified branch
        /// </summary>
        /// <param name="branch"></param>
        /// <returns></returns>
        internal int GetNumLocsByBranch(int branch)
        {
            if (branch > 0 && branch < 5)
            { return arrayOfNetworkAnalysis[branch, 0]; }
            else { Game.SetError(new Error(164, "Invalid branch input (outside of 1 to 4 range)")); }
            return 0;
        }

        /// <summary>
        /// returns true if branch has a connector, false otherwise
        /// </summary>
        /// <returns></returns>
        internal bool GetBranchConnectorStatus(int branch)
        {
            if (branch > 0 && branch < 5)
            { if (ArrayOfConnectors[branch, 1] > 0) { return true; } }
            else { Game.SetError(new Error(166, "Invalid branch input (outside of 1 to 4 range)")); }
            return false;
        }

        //methods above here
    }
}