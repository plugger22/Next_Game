using System;
using System.Collections.Generic;
using System.IO;
using RLNET;

//TODO optimise route search routines by having four separate route lists, one for each branch

namespace Next_Game.Cartographic
{
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
        private int[,] arrayOfNetworkAnalysis; //analysises network [1 for each branch direction] [# locs] [# of connection of first loc out from capital][# of houses on branch]

        //default constructor with seed for random # generator
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
            arrayOfNetworkAnalysis = new int[5, 2]; //0 capital, 1-4 N,E,S,W


        }

        //set up Network collections
        public void InitialiseNetwork()
        {
            //read in location names
            string filePath = "c:/Users/cameron/documents/visual studio 2015/Projects/Next_Game/Data/locations.txt";
            string[] arrayOfLocationNames = File.ReadAllLines(filePath);
            //read location names from array into list
            for (int i = 0; i < arrayOfLocationNames.Length; i++)
            { listOfLocationNames.Add(arrayOfLocationNames[i]); }
            //set up locations dictionary
            //one entry for each location, keyed off it's ID
            if ( dictLocations != null)
            {
                foreach(Location loc in ListOfLocations)
                {
                    //name
                    loc.LocName = GetRandomLocationName();
                    //add to dictionary
                    dictLocations.Add(loc.LocationID, loc);
                    //tally up number of locations on each branch
                    int branch = loc.GetCapitalRouteDirection();
                    arrayOfNetworkAnalysis[branch, 0]++;
                }
                //create list of neighbours for each location
                InitialiseNeighbours();
                //create list of routes from locations back to the capital
                InitialiseRoutesToCapital();
                InitialiseRoutesToConnectors();
            }
        }

        //sorts out all locations on a branch
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
                    listOfNeighbours = loc.GetNeighbours();
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
                    int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
        }
        //sets up routes to connectors (if one exists for that branch)
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
            int locID = Game.map.GetLocationID(posOrigin.PosX, posOrigin.PosY);
            //position pair (route origin/destination)
            //lookup origin location in dictionary
            if (dictLocations.TryGetValue(locID, out baseLoc))
            {
                baseNeighbours = baseLoc.GetNeighbours();
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
                    originNeighbours.RemoveAt(0);
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
                                        originNeighbours.RemoveAt(0);
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
                                originNeighbours.RemoveAt(0);
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
                                Console.WriteLine("ERROR - originNeighbours EMPTY");
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
                        locID = Game.map.GetLocationID(posNew.PosX, posNew.PosY);
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
                            newNeighbours = newLoc.GetNeighbours();
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
                                locID = Game.map.GetLocationID(posBase.PosX, posBase.PosY);
                                if (dictLocations.TryGetValue(locID, out searchLoc))
                                { searchNeighbours = searchLoc.GetNeighbours(); }
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
                                    originNeighbours.RemoveAt(0);
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

        //Main Route Finding Algorithim from A to B anywhere on the map.
        //Returns List<Route> to enable a Map object to draw the route on the map
        //data validation of A & B positions is done by Network.RouteInput()
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
            int locID = Game.map.GetLocationID(posOrigin.PosX, posOrigin.PosY);
            if (dictLocations.TryGetValue(locID, out originLoc))
            {
                originToCapitalRoutes.AddRange(originLoc.GetRouteToCapital());
                originFromCapitalRoutes.AddRange(originLoc.GetRouteFromCapital());
            }
            //Get destination routes
            locID = Game.map.GetLocationID(posDestination.PosX, posDestination.PosY);
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
                        locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
                        if (dictLocations.TryGetValue(locID, out loc))
                        {
                            //get distance to Capital
                            distanceAlternate = loc.DistanceToCapital;
                            //check combined distance < Capital to Destination
                            if (distanceToCapital > (distanceAlternate + distanceToConnector + distanceAcrossConnector))
                            {
                                //compile combined route (destination -> connector -> destination -> capital)
                                listOfAlternateRoutes.AddRange(destinationLoc.GetRouteToConnector());
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
                        locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
                        if (dictLocations.TryGetValue(locID, out loc))
                        {
                            //get distance to Capital
                            distanceAlternate = loc.DistanceToCapital;
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
                                        locID = Game.map.GetLocationID(posEnd.PosX, posEnd.PosY);
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
                        locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
                            locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
                                locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
                                locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
                                locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
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
                                locID = Game.map.GetLocationID(posConn.PosX, posConn.PosY);
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
            Console.WriteLine("Route from {0} to {1} distance {2}", originName, destinationName, distance);)
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
                int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
                if (dictLocations.ContainsKey(locID))
                { loc = dictLocations[locID]; loc.PrintStatus(); }
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
                { Console.WriteLine("ERROR: You've input an identical Origin and Destination or the inputs are Not valid Locations"); }
            }
                return listOfRoutes;
        }


        /// <summary>
        /// Get a location from Dictionary of Locations via locID
        /// </summary>
        /// <param name="locID"></param>
        /// <returns></returns>
        internal Location GetLocation(int locID)
        {
            Location loc = new Location();
            if (dictLocations.ContainsKey(locID))
            { loc = dictLocations[locID]; }
            return loc;
        }

        private Position GetLocationInput(string name)
        {
            Console.WriteLine("Input {0} Coordinates in format X,Y", name);
            string coords = Console.ReadLine();
            string[] tokens = coords.Split(',');
            //Input Position
            Position pos = new Position();
            pos.PosX = Convert.ToInt32(tokens[0]);
            pos.PosY = Convert.ToInt32(tokens[1]);
            int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
            //check a valid location, return null if not
            if (!dictLocations.ContainsKey(locID))
            { pos = null; }
            return pos;
        }


        //test function to provide route to capital from input location
        private List<Route> RouteToCapital(Position pos)
        {
            List<Route> listOfRoutes = new List<Route>();
            listOfRoutes = null;
            Location loc = new Location();
            int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
            if (dictLocations.ContainsKey(locID))
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
            index = rnd.Next(0, listOfLocationNames.Count - 1);
            //get name
            name = listOfLocationNames[index];
            //delete record in list to prevent duplicate names
            listOfLocationNames.RemoveAt(index);
            return name;
        }

        //returns Location name for a given Position
        internal string GetLocationName(Position pos)
        {
            string locName = null;
            Location loc = new Location();
            int locID = Game.map.GetLocationID(pos.PosX, pos.PosY);
            if (dictLocations.TryGetValue(locID, out loc))
            { locName = loc.LocName; }
            return locName;
        }

        //returns distance for a given list of routes
        private int GetDistance(List<Route> listOfRoutes)
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

        //debug program
        public void ShowNetworkAnalysis()
        {
            Console.WriteLine();
            Console.WriteLine("--- Network Analysis");
            for (int i = 0; i < arrayOfNetworkAnalysis.Length; i++)
            {
                int direction = arrayOfNetworkAnalysis[i, 0];
                Console.WriteLine("Dir {0,-10} {1, -15) Locations {2, -15} Connections", );
            }

        }

        //methods above here
    }
}