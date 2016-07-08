using System;
using System.Collections.Generic;

namespace Next_Game.Cartographic
{
    class Route
    {
        private Position loc1;  //closest to capital (start of route)
        private Position loc2;  //furthest from capital (end of route)
        private int routeDistance;
        private readonly List<Position> path = new List<Position>();
        

        public Route()
        {}

        //main constructor - pass the path list and all the rest is taken care of
        public Route(List<Position> newPath)
        {
            path.AddRange(newPath);
            routeDistance = path.Count;
            loc1 = path[0]; //first record is furthest location to capital
            loc2 = path[routeDistance - 1]; //last closest is furthest location from capital
        }

        //Get Location Coords
        public Position GetLoc1() { return loc1; }
        public Position GetLoc2() { return loc2; }

        //Get Route distance (length of route - 1 to allow for originating point)
        public int GetDistance()
        { return path.Count - 1; }

        public List<Position> GetPath()
        { return path; }

        public void SetPath(List<Position> newPath)
        {
            path.Clear();
            path.AddRange(newPath);
        }

        public void ReversePath()
        { path.Reverse(); }

        //a debug function to display path in sequential order
        public void ShowPath(string nameRoute)
        {
            int num = 1;
            Console.WriteLine(nameRoute + " ---");
            foreach(Position pos in path)
            { Console.WriteLine("{0} -> {1}:{2}", num, pos.PosX, pos.PosY); num++; }
        }

        //print class (debug)
        public void PrintRoute()
        { Console.WriteLine("Debug: Route from {0}:{1} to {2}:{3}, distance {4}", loc1.PosX, loc1.PosY, loc2.PosX, loc2.PosY, routeDistance); }

        //print details of a route (debug)
        public void PrintRouteDetails()
        {
            Console.WriteLine("Debug: PrintRouteDetails ---");
            foreach (Position pos in path)
            { Console.WriteLine("Debug: Route coords {0}:{1}", pos.PosX, pos.PosY); }
        }
    }
}