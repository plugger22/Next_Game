using System;
using System.Collections.Generic;
using Next_Game;

namespace Next_Game.Cartographic
{

    public class Position
    {
        public int PosX { get; set;}
        public int PosY { get; set; }
        public float Distance { get; set; } //multi-purpose
        public int CapitalRoute { get; set; } //on which route from the capital is the position? (0 - Capital, 1 - North, 2 - East, 3 - South, 4 - West)

        public Position()
        { }

        public Position(int x, int y)
        { PosX = x;  PosY = y; }

        public Position(Position pos)
        { PosX = pos.PosX; PosY = pos.PosY; }
    }

    //enables dictionary key comparisons for the Position class
    class PositionEqualityComparer : IEqualityComparer<Position>
    {
        public bool Equals(Position pos1, Position pos2)
        {
            //identical x & Y coords is a match
            if(pos1.PosX == pos2.PosX && pos1.PosY == pos2.PosY)
            { return true; }
            //both null is a match
            else if(pos1 == null && pos2 == null)
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
        public bool Capital { get; set; } //is capital?
        public bool Connector { get; set; } //has a connector to a different branch?
        public int Connections { get; set; } //number of connections to neighbouring nodes
        public int DistanceToCapital { get; set; } 
        public int DistanceToConnector { get; set; }
        public int LocationID { get; }
        private List<Position> listOfNeighboursPos = new List<Position>(); //list of immediate neighbours, by Position
        private List<int> listOfNeighboursLocID = new List<int>(); //list of immediate neighbours, by LocID
        private readonly List<Route> routeToCapital = new List<Route>(); //Loc -> Capital
        private readonly List<Route> routeToConnector = new List<Route>(); //Loc -> Connector
        private readonly List<Route> routeFromCapital = new List<Route>(); //Capital -> Loc
        private readonly List<Route> routeFromConnector = new List<Route>(); //Connector -> Loc
        private List<int> characterList = new List<int>(); //list of characters (charID's) currently at Location

        public Location()
        { LocName = "testville"; Capital = false; locPos = new Position(); LocationID = locationIndex++; }

        //normal double constructor for a location
        public Location(Position pos)
        {
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY;
            Capital = false;
            Connector = false;
            LocationID = locationIndex++;
        }

        //triple constructor to specify capital
        public Location(Position pos, bool capital)
        {
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY;
            Capital = capital;
            Connector = false;
            LocationID = locationIndex++;
        }

        //quad constructor to include CapitalRoute
        public Location(Position pos, int dir,  bool capital)
        {
            LocName = "testville";
            locPos = new Position();
            locPos.PosX = pos.PosX; locPos.PosY = pos.PosY; locPos.CapitalRoute = dir;
            Capital = capital;
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
            int locID = Next_Game.Game.map.GetLocationID(pos.PosX, pos.PosY);
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
            foreach(Route route in routeFromCapital)
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

        //get CapitalRoute direction
        public int GetCapitalRouteDirection()
        { return locPos.CapitalRoute; }

        //get Position of Location
        public Position GetPosition()
        { return locPos; }

        public List<int> GetCharacterList()
        { return characterList; }
       

        //set Connector status (connector node to another branch?)
        public void SetConnector(bool status)
        { this.Connector = status; }

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
            foreach(Route route in listOfRoutes)
            { route.ShowPath(routeName); }
        }

        //add character to location
        public void AddCharacter(int charID)
        { characterList.Add(charID); }

        //remove character from location
        public void RemoveCharacter(int charID)
        { characterList.Remove(charID); }


        //prints details of array
        public void ShowStatus()
        {
            Console.WriteLine();
            Console.WriteLine("--- Location (ID {0})", LocationID);
            if(Capital == true)
            { Console.WriteLine("CAPITAL"); }
            if (Connector == true)
            { Console.WriteLine("CONNECTOR NODE"); }
            Console.WriteLine(LocName);
            
            //Console.WriteLine("Location at {0}:{1} on route {2}", locPos.PosX, locPos.PosY, locPos.CapitalRoute);
            //Console.WriteLine("Connections: {0}, Routes TO Capital: {1}, Distance to Capital: {2}", Connections, routeToCapital.Count, DistanceToCapital);
            //ShowRoute(routeToCapital, "To Capital");
            //Console.WriteLine("Connections: {0}, Routes TO Connector: {1}, Distance to Connector: {2}", Connections, routeToConnector.Count, DistanceToConnector);
            //ShowRoute(routeToConnector, "To Connector");
            //Console.WriteLine("Connections: {0}, Routes FROM Capital: {1}, Distance from Capital: {2}", Connections, routeFromCapital.Count, DistanceToCapital);
            //ShowRoute(routeFromCapital, "From Capital");
            //Console.WriteLine("Connections: {0}, Routes FROM Connector: {1}, Distance from Connector: {2}", Connections, routeFromConnector.Count, DistanceToConnector);
            //ShowRoute(routeFromConnector, "From Connector");
            //foreach (Position pos in listOfNeighbours)
            //{ Console.WriteLine("Neigbour at {0}:{1}, distance {2}", pos.PosX, pos.PosY, pos.Distance); }
        }

    }
}