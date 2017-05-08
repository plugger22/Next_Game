using System;
using System.Collections.Generic;
using Next_Game;

namespace Next_Game.Cartographic
{

    public class Position
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public float Distance { get; set; } //multi-purpose
        public int Branch { get; set; } //on which route from the capital is the position? (0 - Capital, 1 - North, 2 - East, 3 - South, 4 - West)

        public Position()
        { }

        public Position(int x, int y)
        { PosX = x; PosY = y; }

        public Position(Position pos)
        { PosX = pos.PosX; PosY = pos.PosY; }

    }

    //enables dictionary key comparisons for the Position class
    class PositionEqualityComparer : IEqualityComparer<Position>
    {
        public bool Equals(Position pos1, Position pos2)
        {
            //identical x & Y coords is a match
            if (pos1.PosX == pos2.PosX && pos1.PosY == pos2.PosY)
            { return true; }
            //both null is a match
            else if (pos1 == null && pos2 == null)
            { return true; }
            else
            { return false; }
        }

        public int GetHashCode(Position pos)
        {
            int hCode = pos.PosX ^ pos.PosY;
            return hCode.GetHashCode();
        }
    }

    //Position pair class for pathfinding
    class PositionPair : Position
    {
        public int Pos2X { get; set; }
        public int Pos2Y { get; set; }
    }

    //Location class
    //sortable on distance to capital (closest to furthest)
    internal class Location : IComparable<Location>
    {
        private static int locationIndex = 1;
        public string LocName { get; set; }
        private Position locPos;
        public int LocationID { get; }
        public bool Capital { get; set; } //true if Capital
        public bool Port { get; set; } //true if a port (orthoganlly adjacent to a sea cluster with at least one other port present
        public bool Connector { get; set; } //has a connector to a different branch?
        public int Connections { get; set; } //number of connections to neighbouring nodes
        public int ConnectorID { get; set; } = 0; //ID of location at the other end of the connection (if one exists)
        public int DistanceToCapital { get; set; }
        public int DistanceToConnector { get; set; }
        public int HouseID { get; set; } //location house ID if applicable
        public int RefID { get; set; } //location ref ID if applicable
        public int ArcID { get; set; } //location archetype ID if applicable        
        private List<Position> listOfNeighboursPos; //list of immediate neighbours, by Position
        private List<int> listOfNeighboursLocID; //list of immediate neighbours, by LocID
        private readonly List<Route> routeToCapital; //Loc -> Capital
        private readonly List<Route> routeToConnector; //Loc -> Connector
        private readonly List<Route> routeFromCapital; //Capital -> Loc
        private readonly List<Route> routeFromConnector; //Connector -> Loc
        private List<int> listOfActors; //list of characters (actorID's) currently at Location
        private List<int> listOfSecrets;
        private List<int> listOfFollowerEvents;
        private List<int> listOfPlayerEvents;
        private Dictionary<int, int> dictSeaDistances; //locID of destination port (key) and distance (value)

        public Location()
        { LocName = "testville"; Capital = false; locPos = new Position(); LocationID = locationIndex++; }

        //normal double constructor for a location
        public Location(Position pos)
        {
            listOfActors = new List<int>();
            listOfSecrets = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listOfNeighboursPos = new List<Position>();
            listOfNeighboursLocID = new List<int>();
            dictSeaDistances = new Dictionary<int, int>();
            routeToCapital = new List<Route>();
            routeToConnector = new List<Route>();
            routeFromCapital = new List<Route>();
            routeFromConnector = new List<Route>();
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY;
            Capital = false;
            Port = false;
            Connector = false;
            LocationID = locationIndex++;
        }

        //triple constructor to specify capital
        public Location(Position pos, bool capital)
        {
            listOfActors = new List<int>();
            listOfSecrets = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listOfNeighboursPos = new List<Position>();
            listOfNeighboursLocID = new List<int>();
            dictSeaDistances = new Dictionary<int, int>();
            routeToCapital = new List<Route>();
            routeToConnector = new List<Route>();
            routeFromCapital = new List<Route>();
            routeFromConnector = new List<Route>();
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY;
            Capital = capital;
            Port = false;
            Connector = false;
            LocationID = locationIndex++;
        }

        //quad constructor to include CapitalRoute
        public Location(Position pos, int dir, bool capital)
        {
            listOfActors = new List<int>();
            listOfSecrets = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listOfNeighboursPos = new List<Position>();
            listOfNeighboursLocID = new List<int>();
            dictSeaDistances = new Dictionary<int, int>();
            routeToCapital = new List<Route>();
            routeToConnector = new List<Route>();
            routeFromCapital = new List<Route>();
            routeFromConnector = new List<Route>();
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY; locPos.Branch = dir;
            Capital = capital;
            Port = false;
            Connector = false;
            LocationID = locationIndex++;
        }

        /// <summary>
        /// Default comparer for Location (based on distance to Capital)
        /// </summary>
        /// <param name="pos"></param>
        public int CompareTo(Location compareLoc)
        {
            if (compareLoc == null)
            { return 1; }
            else
            { return this.DistanceToCapital.CompareTo(compareLoc.DistanceToCapital); }
        }

        //add Neighbour data
        public void AddNeighbour(Position pos)
        {
            listOfNeighboursPos.Add(pos);
            int locID = Next_Game.Game.map.GetMapInfo(MapLayer.LocID, pos.PosX, pos.PosY);
            listOfNeighboursLocID.Add(locID);
        }

        //input list of routes to Capital
        public void SetRoutesToCapital(List<Route> listOfRoutes)
        {
            routeToCapital.Clear();
            routeToCapital.AddRange(listOfRoutes);
        }

        public void SetRoutesFromCapital(List<Route> listOfRoutes)
        {
            routeFromCapital.Clear();
            routeFromCapital.AddRange(listOfRoutes);
        }

        public void SetRoutesFromConnector(List<Route> listOfRoutes)
        {
            routeFromConnector.Clear();
            routeFromConnector.AddRange(listOfRoutes);
        }

        public void SetRoutesToConnector(List<Route> listOfRoutes)
        {
            routeToConnector.Clear();
            routeToConnector.AddRange(listOfRoutes);
        }

        //reverses routeFromCapital and stores it in routeToCapital
        public void InitialiseRouteToCapital()
        {
            //new list of Routes
            List<Route> reversedRoutes = new List<Route>();
            //loop through each existing route
            foreach (Route route in routeFromCapital)
            {
                //copy Path to a new instance (independent of original)
                List<Position> tempPath = new List<Position>(route.GetPath());
                //create a new route using the path in the constructor (otherwise route won't have the new path)
                Route tempRoute = new Route(tempPath);
                //reverse the path
                tempRoute.ReversePath();
                //add route to the new list of routes
                reversedRoutes.Add(tempRoute);
            }
            //reverse entire route
            reversedRoutes.Reverse();
            //input new route list with reversed paths and routes for the reciprocal route
            SetRoutesToCapital(reversedRoutes);
        }

        //reverses routeFromConnector and stores it in routeToConnector
        public void InitialiseRouteToConnector()
        {
            //new list of Routes
            List<Route> reversedRoutes = new List<Route>();
            //loop through each existing route
            foreach (Route route in routeFromConnector)
            {
                //copy Path to a new instance (independent of original)
                List<Position> tempPath = new List<Position>(route.GetPath());
                //create a new route using the path in the constructor (otherwise route won't have the new path)
                Route tempRoute = new Route(tempPath);
                //reverse the path
                tempRoute.ReversePath();
                //add route to the new list of routes
                reversedRoutes.Add(tempRoute);
            }
            //reverse entire route
            reversedRoutes.Reverse();
            //input new route list with reversed paths and routes for the reciprocal route
            SetRoutesToConnector(reversedRoutes);
        }

        //return list of Neighbours
        public List<Position> GetNeighboursPos()
        { return listOfNeighboursPos; }

        public List<int> GetNeighboursLocID()
        { return listOfNeighboursLocID; }

        //get Position coord 
        public int GetPosX()
        { return locPos.PosX; }

        //get Position coord 
        public int GetPosY()
        { return locPos.PosY; }

        //get number of Routes to Capital
        public int GetNumRoutesToCapital()
        { return routeToCapital.Count; }

        //get list of Routes to Capital
        public List<Route> GetRouteToCapital()
        { return routeToCapital; }

        //get list of Routes From Capital
        public List<Route> GetRouteFromCapital()
        { return routeFromCapital; }

        //avoid passing a reference - debug
        private List<Route> GetRouteFromCapitalCopy()
        {
            List<Route> listRoutesCopy = new List<Route>(routeFromCapital);
            return listRoutesCopy;
        }

        //get list of Routes to Connector
        public List<Route> GetRouteToConnector()
        { return routeToConnector; }

        //get list of Routes From Connector
        public List<Route> GetRouteFromConnector()
        { return routeFromConnector; }

        //get Branch direction
        public int GetBranch()
        { return locPos.Branch; }

        //get Position of Location
        public Position GetPosition()
        { return locPos; }

        public List<int> GetActorList()
        { return listOfActors; }

        /// <summary>
        /// returns true if the actor is present at the location
        /// </summary>
        /// <param name="actorID"></param>
        /// <returns></returns>
        public bool CheckActorStatus(int actorID)
        {
            //if (listOfActors.Find(x => x == actorID) > 0)
            if (listOfActors.Contains(actorID) == true)
            { return true; }
            return false;
        }

        //returns true if capital
        public bool IsCapital()
        {
            if (Capital == true)
            { return true; }
            else { return false; }
        }

        //prints details of a route, cell by cell
        private void ShowRoute(List<Route> listOfRoutes, string routeName)
        {
            foreach (Route route in listOfRoutes)
            { route.ShowPath(routeName); }
        }

        //add character to location (goes with RemoveActor)
        public void AddActor(int actorID)
        {
            try
            { listOfActors.Add(actorID); }
            catch (Exception e)
            { Game.SetError(new Error(60, e.Message)); }
        }


        ///remove character from location (goes with AddActor)
        public void RemoveActor(int actorID)
        {
            try
            { listOfActors.Remove(actorID); }
            catch (Exception e)
            { Game.SetError(new Error(61, e.Message)); }
        }


        //prints details of array
        public void ShowStatus()
        {
            Console.WriteLine();
            Console.WriteLine("--- Location (ID {0})", LocationID);
            if (Capital == true)
            { Console.WriteLine("CAPITAL"); }
            if (Connector == true)
            { Console.WriteLine("CONNECTOR NODE"); }
            Console.WriteLine(LocName);
        }

        internal void SetEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfFollowerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(58, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetFollowerEvents()
        { return listOfFollowerEvents; }

        internal List<int> GetPlayerEvents()
        { return listOfPlayerEvents; }

        /// <summary>
        /// add the distance (at sea) to the specified location
        /// </summary>
        /// <param name="locID"></param>
        /// <param name="distance"></param>
        public void AddSeaDistance(int locID, int distance)
        {
            try
            {
                dictSeaDistances.Add(locID, distance);
                Game.logStart?.Write($"[Notification] LocID {locID} distance {distance} added to dictSeaDistance for Port LocID {LocationID}");
            }
            catch (ArgumentNullException)
            { Game.SetError(new Error(216, "Invalid input (null)")); }
            catch (ArgumentException)
            {
                Game.SetError(new Error(216, $"Invalid locID {locID} (duplicate exists)"));
            }
        }

        public Dictionary<int, int> GetSeaDistances()
        { return dictSeaDistances; }

        public int GetNumConnectedPorts()
        { return dictSeaDistances.Count; }
    }
}