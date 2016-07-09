using System;
using System.Collections.Generic;
using RLNET;

namespace Next_Game.Cartographic
{

    public enum MapLayer { Base, Player, NPC, LocID, Debug, Count }

    //Main Map class (single instance, it's job is to set everything up at the start)
    public class Map
    {
        private int mapSize;
        private int[,,] mapGrid;
        private static int capitalX;
        private static int capitalY;
        private static Random rnd;
        private int[,] arrayOfConnectors = new int[5,6];
        private List<Route> listOfRoutes = new List<Route>(); //list of all routes (excludes Connectors)
        private List<Route> listOfConnectors = new List<Route>(); //list of all special branch Connector routes
        private List<Location> listOfLocations = new List<Location>(); //list of all locations
        //DrawMapRL coords (need to be class instances in order to convert mouse coords)
        private int margin; //spacing on all sides for whole map (used for top and left sides)
        private int offsetVertical; //number of rows to allow for header at top 
        private int offsetHorizontal; //number of rows to allow for indexes at left

        //default constructor assumes a square map/grid & random seed
        public Map(int mapSize, int seed)
        {
            this.mapSize = mapSize;
            mapGrid = new int[(int)MapLayer.Count + 1, mapSize, mapSize];
            rnd = new Random(seed);
            //DrawMapRL
            margin = 2;
            offsetVertical = margin + 2;
            offsetHorizontal = margin + 1;
        }
        
        /// <summary>
        /// initialise Map
        /// </summary>
        /// <param name="frequency" is the probability (1d100 < frequency) of a location in a cell></param>
        /// <param name="spacing" is the minimum distance apart they should be></param>
        public void InitialiseMap(int frequency, int spacing = 1)
        {
            //terrain first
            //InitialiseTerrain(3, 4, 4, 3);
            //loop through map and randomly assign locations
            for (int row = 0; row < mapSize; row++)
            {
                for (int column = 0; column < mapSize; column++)
                {
                    //random hit and no terrain in location
                    if (rnd.Next(0, 100) < frequency && mapGrid[(int)MapLayer.Base, column, row] == 0)
                    {
                        //check not adjacent (distance equal to spacing) to an existing location
                        int nearX = column;
                        int nearY = row;
                        int temp = 0;
                        bool neighbour = false;
                        //check adjacent left column
                        if (nearX > 0)
                        {
                            for (int i = 1; i <= spacing; i++)
                            {
                                temp = nearX - 1;
                                if (temp >= 0)
                                {
                                    if (mapGrid[(int)MapLayer.Base, temp, row] > 0)
                                    { neighbour = true; }
                                }
                            }
                        }
                        //check adjacent right column
                        if (nearX < mapSize)
                        {
                            for (int i = 1; i <= spacing; i++)
                            {
                                temp = nearX + 1;
                                if (temp < mapSize)
                                {
                                    if (mapGrid[(int)MapLayer.Base, temp, row] > 0)
                                    { neighbour = true; }
                                }
                            }
                        }
                        //check adjacent top row
                        if (nearY > 0)
                        {
                            for (int i = 1; i <= spacing; i++)
                            {
                                temp = nearY - 1;
                                if (temp >= 0)
                                {
                                    if (mapGrid[(int)MapLayer.Base, column, temp] > 0)
                                    { neighbour = true; }
                                }
                            }
                        }
                        //check adjacent bottom row
                        if (nearY < mapSize)
                        {
                            for (int i = 1; i <= spacing; i++)
                            {
                                temp = nearY + 1;
                                if (temp < mapSize)
                                {
                                    if( mapGrid[(int)MapLayer.Base, column, temp] > 0)
                                    { neighbour = true; }
                                }   
                            }
                        }

                        //can't be immediately diagonally adjacent, regardless of spacing
                        if (nearX > 0 && nearY > 0)
                        {
                            if (mapGrid[(int)MapLayer.Base, nearX - 1, nearY - 1] > 0)
                            { neighbour = true; }
                        }
                        if (nearX > 0 && nearY < mapSize - 1)
                        {
                            if (mapGrid[(int)MapLayer.Base, nearX - 1, nearY + 1] > 0)
                            { neighbour = true; }
                        }
                        if (nearX < mapSize - 1 && nearY > 0)
                        {
                            if (mapGrid[(int)MapLayer.Base, nearX + 1, nearY - 1] > 0)
                            { neighbour = true; }
                        }
                        if (nearX < mapSize - 1 && nearY < mapSize - 1)
                        {
                            if (mapGrid[(int)MapLayer.Base, nearX + 1, nearY + 1] > 0)
                            { neighbour = true; }
                        }

                        //valid location site
                        if (neighbour == false)
                        { mapGrid[(int)MapLayer.Base, column, row] = 1; }
                        else
                        { mapGrid[(int)MapLayer.Base, column, row] = 0; }
                    }
                }
            }
            FindCapital();
            InitialiseRoads(4);
        }

        //sets up terrain on the map. numMountains indicates the # of mountain ranges, sizeMountains is the size of the mountain range
        private void InitialiseTerrain(int numMountains, int sizeMountains, int numForests, int sizeForests)
        {
            //error check input
            if(sizeMountains > mapSize -2)
            { sizeMountains = mapSize - 2; }
            if (sizeForests > mapSize - 2)
            { sizeForests = mapSize - 2; }
            //Layout Mountains
            for (int i = 0; i < numMountains; i++)
            {
                //generate a random map location
                int ranX = rnd.Next(1, mapSize - sizeMountains - 2);
                int ranY = rnd.Next(1, mapSize - sizeMountains - 2);
                //determine direction
                if (rnd.Next(10) <= 4)
                {
                    //horizontal
                    for(int k = 0; k < sizeMountains; k++)
                    { mapGrid[(int)MapLayer.Base, k + ranX, ranY] = 30; }
                }
                else 
                {
                    //vertical
                    for (int k = 0; k < sizeMountains; k++)
                    { mapGrid[(int)MapLayer.Base, ranX, k + ranY] = 30; }
                }
            }
            //Layout Forest (doubled layout, eg. two lots side by side in appropriate direction
            for (int i = 0; i < numForests; i++)
            {
                //generate a random map location
                int ranX = rnd.Next(1, mapSize - sizeForests - 2);
                int ranY = rnd.Next(1, mapSize - sizeForests - 2);
                //determine direction
                if (rnd.Next(10) <= 4)
                {
                    //horizontal
                    for (int k = 0; k < sizeForests; k++)
                    { mapGrid[(int)MapLayer.Base, k + ranX, ranY] = 31; mapGrid[(int)MapLayer.Base, k + ranX, ranY + 1] = 31; }
                }
                else
                {
                    //vertical
                    for (int k = 0; k < sizeForests; k++)
                    { mapGrid[(int)MapLayer.Base, ranX, k + ranY] = 31; mapGrid[(int)MapLayer.Base, ranX + 1, k + ranY] = 31; }
                }
            }
        }

        /// <summary>
        /// Draw Map Console version
        /// </summary>
        /*public void DrawMap()
        {
            //write header to screen (two rows, vertical 2 digit, spaced 3 apart)
            Console.Write("   ");
            for (int j = 0; j < mapSize; j++)
            { Console.Write("{0,2} ", j / 10); }
            int i = 10;
            int counter = 0;
            int playerLayer = 0;
            int mainLayer = 0; //
            Console.WriteLine(); Console.Write("   ");
            do
            {
                Console.Write("{0,2} ", i % 10); counter++; i++;
                if(i > 19) { i = 10; }
            }
            while (counter < mapSize);
            Console.WriteLine(); Console.WriteLine();
            //write row & grid to screen
            Console.ForegroundColor = ConsoleColor.Gray;
            //for route debugging
            for (int row = 0; row < mapSize; row++)
            {
                Console.Write("{0,2} ", row);
                for (int column = 0; column < mapSize; column++)
                {
                    string marker = "  ";
                    //Check Player layer first (overides base layer)
                    mainLayer = mapGrid[(int)MapLayer.Base, column, row];
                    playerLayer = mapGrid[(int)MapLayer.Player, column, row];
                    if (playerLayer > 0)
                    {
                        //# represent group at location (static) or moving. Show yellow for capital, cyan for loc and green for enroute
                        marker = " " + Convert.ToString(playerLayer) + " ";
                        if(mainLayer == 2)
                        { Console.ForegroundColor = ConsoleColor.Yellow; } //capital
                        else if(mainLayer == 1)
                        { Console.ForegroundColor = ConsoleColor.Cyan; } //location
                        else
                        { Console.ForegroundColor = ConsoleColor.Green; } //in enroute
                    }
                    //draw base level
                    else
                    {
                        //ordinary location
                        switch (mainLayer)
                        {
                            //empty cell
                            case 0:
                                marker = " . "; Console.ForegroundColor = ConsoleColor.DarkGray;
                                break;
                            //location ---
                            case 1:
                                marker = " ■"; Console.ForegroundColor = ConsoleColor.Cyan;
                                break;
                            //kingdom capital
                            case 2:
                                marker = " ■"; Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            //road vertical ---
                            case 10:
                                marker = " │"; Console.ForegroundColor = ConsoleColor.White;
                                break;
                            //road lateral
                            case 11:
                                marker = "---"; Console.ForegroundColor = ConsoleColor.White;
                                break;
                            //road right up (end of dogleg) or road down left (start of dogleg)
                            case 12:
                                marker = "-┘ "; Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            //road right down (end of dogleg) or up left (start of dogleg)
                            case 13:
                                marker = "-┐ "; Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            //road left up (end of dogleg) or down right (start of dogleg)
                            case 14:
                                marker = " └-"; Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            //road left down (end of dogleg) up right (start of dogleg)
                            case 15:
                                marker = " ┌-"; Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            //road vertical route ---
                            case 20:
                                marker = " │"; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //road lateral route
                            case 21:
                                marker = "---"; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //road right up (end of dogleg) or road down left (start of dogleg) route
                            case 22:
                                marker = "-┘ "; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //road right down (end of dogleg) or up left (start of dogleg) route
                            case 23:
                                marker = "-┐ "; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //road left up (end of dogleg) or down right (start of dogleg) route
                            case 24:
                                marker = " └-"; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //road left down (end of dogleg) up right (start of dogleg) route
                            case 25:
                                marker = " ┌-"; Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            //terrain, mountains
                            case 30:
                                marker = " M "; Console.ForegroundColor = ConsoleColor.White;
                                break;
                            //terrain, forests
                            case 31:
                                marker = " ^ "; Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            //for debug routes
                            case 40:
                            case 41:
                            case 42:
                            case 43:
                            case 44:
                            case 45:
                                //keep numbers 0 to 9, different colours for each set of 10 = red -> yellow -> green -> magenta
                                int num = mapGrid[(int)MapLayer.Debug, column, row];
                                if (num < 10)
                                { marker = " " + Convert.ToString(num); Console.ForegroundColor = ConsoleColor.Red; }
                                else if (num >= 10 && num < 20)
                                { num %= 10; marker = " " + Convert.ToString(num); Console.ForegroundColor = ConsoleColor.Yellow; }
                                else if (num >= 20 && num < 30)
                                { num %= 10; marker = " " + Convert.ToString(num); Console.ForegroundColor = ConsoleColor.Green; }
                                else
                                { num %= 10; marker = " " + Convert.ToString(num); Console.ForegroundColor = ConsoleColor.Magenta; }
                                break;
                        }
                    } 
                    Console.Write("{0,-3}", marker);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("{0,3} ", row);
                Console.WriteLine();
            }
            //write footer to screen (two rows, vertical 2 digit, spaced 3 apart)
            Console.WriteLine(); 
            Console.Write("   ");
            for (int j = 0; j < mapSize; j++)
            { Console.Write("{0,2} ", j / 10); }
            i = 10;
            counter = 0;
            Console.WriteLine(); Console.Write("   ");
            do
            {
                Console.Write("{0,2} ", i % 10); counter++; i++;
                if (i > 19) { i = 10; }
            }
            while (counter < mapSize);
            Console.WriteLine();
        }*/

        /// <summary>
        /// Draw map RLNET version
        /// </summary>
        /// <param name="mapConsole"></param>
        public void DrawMapRL(RLConsole mapConsole)
        {
            mapConsole.Clear();
            RLColor foreground; //normal foreground color
            //RLColor forVertical; //color used for vertical above and below connections
            int playerLayer = 0;
            int mainLayer = 0;
            int horizontal1; //ascii character as an alt code - 3 characters per cell to enlarge map (horizontal plane)
            int horizontal2; 
            int horizontal3;
            int vertical1; //ascii character as an alt code (vertical plane above)
            int vertical2; //(vertical plane below)
            //
            //margin and the vertical & horizontal offsets are class instances
            //
            //write header to screen (two rows, vertical 2 digit, spaced 3 apart)
            //int mainUp; //cell immediately above the main one
            //int mainDown; //cell immediately below the main one
            foreground = RLColor.LightGray;
            for (int j = 0; j < mapSize; j++)
            {
                //top row (10's)
                mapConsole.Set(j * 3 + offsetHorizontal + margin, 0 + margin, foreground, RLColor.Black, 32);
                mapConsole.Set(j * 3 + 1 + offsetHorizontal + margin, 0 + margin, foreground, RLColor.Black, j / 10 + 48);
                mapConsole.Set(j * 3 + 2 + offsetHorizontal + margin, 0 + margin, foreground, RLColor.Black, 32);
                //bottom row (1's)
                mapConsole.Set(j * 3 + offsetHorizontal + margin, 1 + margin, foreground, RLColor.Black, 32);
                mapConsole.Set(j * 3 + 1 + offsetHorizontal + margin, 1 + margin, foreground, RLColor.Black, j % 10 + 48);
                mapConsole.Set(j * 3 + 2 + offsetHorizontal + margin, 1 + margin, foreground, RLColor.Black, 32);
            }
            //for route debugging
            for (int row = 0; row < mapSize; row++)
            {
                //Right hand side vertical index
                foreground = RLColor.LightGray;
                mapConsole.Set(mapSize * 3 + 1 + offsetHorizontal + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, row / 10 + 48);
                mapConsole.Set(mapSize * 3 + 2 + offsetHorizontal + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, row % 10 + 48);
                foreground = RLColor.Gray;
                //forVertical = RLColor.White;
                for (int column = 0; column < mapSize; column++)
                {
                    horizontal1 = 32; //default space
                    horizontal2 = 32;
                    horizontal3 = 32;
                    vertical1 = 32;
                    vertical2 = 32;
                    //Check Player layer first (overides base layer)
                    mainLayer = mapGrid[(int)MapLayer.Base, column, row];
                    playerLayer = mapGrid[(int)MapLayer.Player, column, row];
                    //get cells above and below
                    //if (row > 0) { mainUp = mapGrid[(int)MapLayer.Base, column, row - 1];} else { mainUp = 0; }
                    //if (row < mapSize - 1) { mainDown = mapGrid[(int)MapLayer.Base, column, row + 1]; } else { mainDown = 0; }
                    //Party Movement - Player
                    if (playerLayer > 0)
                    {
                        //# represent group at location (static) or moving. Show yellow for capital, cyan for loc and green for enroute
                        horizontal2 = playerLayer + 48;
                        if (mainLayer == 2)
                        { foreground = RLColor.Yellow; } //capital
                        else if (mainLayer == 1)
                        { foreground = RLColor.Cyan; } //location
                        else
                        { foreground = RLColor.Green; } //in enroute
                    }
                    //draw base level
                    else
                    {
                        //ordinary location
                        switch (mainLayer)
                        {
                            //empty cell - centred period
                            case 0:
                                horizontal2 = 7;  foreground = RLColor.Gray;
                                break;
                            //location -filled square
                            case 1:
                                horizontal2 = 219;
                                foreground = RLColor.Cyan;
                                //check cells above and below and draw vertical bars if appropriae
                                //if( mainUp == 10 || mainUp == 13 || mainUp == 15) { vertical1 = 179; }
                                //if( mainDown == 10 || mainDown == 12 || mainDown == 14) { vertical2 = 179; }
                                break;
                            //kingdom capital - filled square (large)
                            case 2:
                                horizontal2 = 219;
                                foreground = RLColor.Yellow;
                                //check cells above and below and draw vertical bars if appropriae
                                //if (mainUp == 10 || mainUp == 13 || mainUp == 15) { vertical1 = 179; }
                                //if (mainDown == 10 || mainDown == 12 || mainDown == 14) { vertical2 = 179; }
                                break;
                            //road vertical - vertical line (should be 179 but uses 196 for some reason)
                            case 10:
                                horizontal2 = 179;
                                vertical1 = 179;
                                vertical2 = 179;
                                foreground = RLColor.White; 
                                break;
                            //road lateral - dash (should be 196 but uses 179 for some reason)
                            case 11:
                                horizontal1 = 196;
                                horizontal2 = 196;
                                horizontal3 = 196;
                                foreground = RLColor.White;
                                break;
                            //road right up (end of dogleg) or road down left (start of dogleg)
                            case 12:
                                horizontal1 = 196;
                                horizontal2 = 217;
                                vertical1 = 179;
                                foreground = RLColor.White;
                                break;
                            //road right down (end of dogleg) or up left (start of dogleg) (should be 191 but uses 192)
                            case 13:
                                horizontal1 = 196;
                                horizontal2 = 191;
                                vertical2 = 179;
                                foreground = RLColor.White;
                                break;
                            //road left up (end of dogleg) or down right (start of dogleg)
                            case 14:
                                horizontal2 = 192;
                                horizontal3 = 196;
                                vertical1 = 179;
                                foreground = RLColor.White;
                                break;
                            //road left down (end of dogleg) up right (start of dogleg)
                            case 15:
                                horizontal2 = 218;
                                horizontal3 = 196;
                                vertical2 = 179;
                                foreground = RLColor.White;
                                break;
                            //road vertical route ---
                            case 20:
                                horizontal2 = 179;
                                vertical1 = 179;
                                vertical2 = 179;
                                foreground = RLColor.LightRed;
                                break;
                            //road lateral route
                            case 21:
                                horizontal1 = 196;
                                horizontal2 = 196;
                                horizontal3 = 196;
                                foreground = RLColor.LightRed;
                                break;
                            //road right up (end of dogleg) or road down left (start of dogleg) route
                            case 22:
                                horizontal1 = 196;
                                horizontal2 = 217;
                                vertical1 = 179;
                                foreground = RLColor.LightRed;
                                break;
                            //road right down (end of dogleg) or up left (start of dogleg) route
                            case 23:
                                horizontal1 = 196;
                                horizontal2 = 191;
                                vertical2 = 179;
                                foreground = RLColor.LightRed;
                                break;
                            //road left up (end of dogleg) or down right (start of dogleg) route
                            case 24:
                                horizontal2 = 192;
                                horizontal3 = 196;
                                vertical1 = 179;
                                foreground = RLColor.LightRed;
                                break;
                            //road left down (end of dogleg) up right (start of dogleg) route
                            case 25:
                                horizontal2 = 218;
                                horizontal3 = 196;
                                vertical2 = 179;
                                foreground = RLColor.LightRed;
                                break;
                            //terrain, mountains - 'M'
                            case 30:
                                horizontal2 = 77; foreground = RLColor.White;
                                break;
                            //terrain, forests '^'
                            case 31:
                                horizontal2 = 94; foreground = RLColor.Green;
                                break;
                            //for debug routes
                            case 40:
                            case 41:
                            case 42:
                            case 43:
                            case 44:
                            case 45:
                                //keep numbers 0 to 9, different colours for each set of 10 = red -> yellow -> green -> magenta
                                int num = mapGrid[(int)MapLayer.Debug, column, row];
                                if (num < 10)
                                { horizontal2 = num + 48; foreground = RLColor.Red; }
                                else if (num >= 10 && num < 20)
                                { num %= 10; horizontal2 = num + 48; foreground = RLColor.Yellow; }
                                else if (num >= 20 && num < 30)
                                { num %= 10; horizontal2 = num + 48; foreground = RLColor.Green; }
                                else
                                { num %= 10; horizontal2 = num + 48; foreground = RLColor.Magenta; }
                                break;
                        }
                    }
                    //horizontal
                    mapConsole.Set(column * 3 + offsetHorizontal + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, horizontal1);
                    mapConsole.Set(column * 3 + 1 + offsetHorizontal + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, horizontal2);
                    mapConsole.Set(column * 3 + 2 + offsetHorizontal + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, horizontal3);
                    //vertical
                    mapConsole.Set(column * 3 + 1 + offsetHorizontal + margin, row * 3 + offsetVertical, foreground, RLColor.Black, vertical1);
                    mapConsole.Set(column * 3 + 1 + offsetHorizontal + margin, row * 3 + 2 + offsetVertical, foreground, RLColor.Black, vertical2);
                }
                foreground = RLColor.LightGray;
                mapConsole.Set(0 + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, row/10 + 48);
                mapConsole.Set(1 + margin, row * 3 + 1 + offsetVertical, foreground, RLColor.Black, row % 10 + 48);
            }
            //write footer to screen (two rows, vertical 2 digit, spaced 3 apart)
            foreground = RLColor.LightGray;
            for (int j = 0; j < mapSize; j++)
            {
                mapConsole.Set(j * 3 + 1 + offsetHorizontal + margin, mapSize * 3 + 1 + offsetVertical, foreground, RLColor.Black, j / 10 + 48);
                mapConsole.Set(j * 3 + 1 + offsetHorizontal + margin, mapSize * 3 + 2 + offsetVertical, foreground, RLColor.Black, j % 10 + 48);
            }
        }

        //clears map and draws a specific route
        internal void DrawRouteRL(List<Route> listOfRoutes)
        {
            if(listOfRoutes.Count > 0)
            {
                List<Position> path = new List<Position>();
                //int routeDistance = 0;
                //reset routes back to default
                ShowRoutes(); ShowConnectorRoutes();
                //update map for route to show red
                foreach (Route route in listOfRoutes)
                {
                    //route.PrintRouteDetails();
                    path = route.GetPath();
                    //keep a tab of distance
                    //routeDistance += path.Count - 1;
                    //loop through path
                    foreach (Position pos in path)
                    {
                        //roads only, set all roads to red (20 - 25) to indicate a known route
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 10 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 15)
                            { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] += 10; }
                    }
                }
            }
        }

        //clears map and draws a specific route in Debug mode (routes are sequentially numbered with Loc's in blue)
        internal void DrawRouteDebug(List<Route> listOfRoutes)
        {
            if (listOfRoutes.Count > 0)
            {
                List<Position> path = new List<Position>();
                //reset routes back to default
                ShowRoutes(); ShowConnectorRoutes();
                int debugCount = 1;
                //update map for route to show red
                foreach (Route route in listOfRoutes)
                {
                    //route.PrintRouteDetails();
                    path = route.GetPath();
                    //loop through path
                    foreach (Position pos in path)
                    {
                        //roads only, set all roads to red #'s (in sequence drawn)
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 10 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 15)
                        {
                            mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] += 30;
                            mapGrid[(int)MapLayer.Debug, pos.PosX, pos.PosY] = debugCount;
                            debugCount++;
                        }
                    }
                }
            }
        }

        //calculates distance between two sets of 2D coordinates
        public float MapDistance(int x1, int y1, int x2, int y2)
        {
            //pythagoras formula
            return (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }


        //locates Capital City (home of King's throne)
        private void FindCapital()
        {
            //Find centre of map coords
            int centerX = mapSize / 2;
            int centerY = mapSize / 2;
            Position pos = new Position();
            float distance = mapSize;

            //Find the location closest to the map center
            pos = FindNearestLocation(centerX, centerY);
            //shortest distance coords will be stored in pos, set to Capital
            mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] = 2;
            //update capital coords
            capitalX = pos.PosX;
            capitalY = pos.PosY;
        }
        
        /// <summary>
        /// Returns capital as a Position
        /// </summary>
        /// <returns></returns>
        public Position GetCapital()
        {
            Position pos = new Position();
            pos.PosX = capitalX;
            pos.PosY = capitalY;
            return pos;
        }

        //loops mapGrid and returns location nearest to target X & Y as a Position object (x,y,distance)
        //minDistance (default to 0) allows you to ignore locations that are within a set minimum distance from target
        private Position FindNearestLocation(int targetX, int targetY, float minDistance = 0)
        {
            float distance = mapSize;
            float calcDistance;
            Position pos = new Position();
            //Loop all locations and calculate distance to map centre
            for (int row = 0; row < mapSize; row++)
            {
                for (int column = 0; column < mapSize; column++)
                {
                    //location present (no capital yet)
                    if (mapGrid[(int)MapLayer.Base, column, row] == 1)
                    {
                        //distance from loc to map center
                        calcDistance = MapDistance(column, row, targetX, targetY);
                        //find shortest distance (exclude case of own location, or locations within a minimum distance from target)
                        if (calcDistance > minDistance && calcDistance < distance)
                        { distance = calcDistance; pos.PosX = column; pos.PosY = row; pos.Distance = calcDistance; }
                    }
                }
            }
            return pos;
        }

        //set up road network
        //max connections can only be 4 if it's a square cell (N/E/S/W)
        private void InitialiseRoads(int maxConnections = 4)
        {
            //first take capital and attempt to connect to the maxConnections # of locs nearest.
            //Console.WriteLine("Debug: CapitalRoads ---");
            //can't have more than 4 connections
            if(maxConnections > 4) { maxConnections = 4; }
            //find nearest location to capital
            Position pos = new Position();
            pos = FindNearestLocation(capitalX, capitalY);
            if (CheckRouteOrthogonal(capitalX, capitalY, pos.PosX, pos.PosY) == false)
            { CheckRouteDogLeg(capitalX, capitalY, pos.PosX, pos.PosY); }
            //find second and third nearest locations and attempt to connect a route to them
            float minDistance;
            int roadCount = 0;
            //check further if last couple of roads aren't valid
            for(int i = 0; i < 6; i++)
            {
                minDistance = pos.Distance;
                pos = FindNearestLocation(capitalX, capitalY, minDistance);
                //Console.WriteLine("Debug: START loop ctr {0}, roadCount {1}", i, roadCount);
                if (CheckRouteOrthogonal(capitalX, capitalY, pos.PosX, pos.PosY) == true)
                { roadCount++; }
                else
                {
                    if (CheckRouteDogLeg(capitalX, capitalY, pos.PosX, pos.PosY) == true)
                    { roadCount++; }
                }
                //maximum of 3 roads
                //Console.WriteLine("Debug: END loop ctr {0}, roadCount {1}", i, roadCount);
                if (roadCount == maxConnections - 1)
                { break; }
            }
            //first round completed (capital connected to nearest loc's. 
            //then...
            //Recursively attempt to connect to all remaining loc's
            while (LocationRoads(mapSize / 3) == false) ;
            //sweeper method to find any unconnected locations and delete them
            LocationSweeper();
            //set up routes
            Position capital = new Position();
            capital.PosX = capitalX; capital.PosY = capitalY;
            InitialiseRoutes(capital);
            //Connectors must be AFTER Routes
            InitialiseConnectors();
            InitialiseMapLayers();
            //ShowArrayOfConnectors();
            //Console.ReadKey();
        }

        private void LocationSweeper()
        {
            //Console.WriteLine("Debug: LocationSweeper ---");
            int numConnections = 0;
            //Loop all locations
            for (int row = 0; row < mapSize; row++)
            {
                for (int column = 0; column < mapSize; column++)
                {
                    //location present (ignore capital)
                    if (mapGrid[(int)MapLayer.Base, column, row] == 1)
                    {
                        numConnections = CheckConnection(column, row);
                        if (numConnections == 0)
                        {
                            //delete location
                            mapGrid[(int)MapLayer.Base, column, row] = 0;      
                            //Console.WriteLine("Debug: Location DELETED {0}:{1}", column, row);
                        }
                    }
                }
            }
        }

        //LocationRoads (maxDistance is the max distance within grid that connections can be made, eg. mapSize/2)
        //returns false if there is still room for further connections, true if the end of the road (no connections made this time / no 0 or 1 connection loc's available)
        //recursive type function - searches for all 1 connection nodes (tip of the road branches - stores in a list) and attempts to connect them to 0 connection nodes 
        //by gradually testing each to see if there is a suitable loc to connect to within a limitDistance increments +1 each time the full list is tested. This ensures that the most
        //suitable, nearest connections are made and that some locations become 'hubs' with more than one connection present.
        public bool LocationRoads(float maxDistance)
        {
            //Console.WriteLine("Debug: Location Roads ---");
            bool finished = false; //return state (if true then no longer continue, end of the road)
            List<Position> nodes = new List<Position>(); //list of inner node locations
            int totalConnectionsZero = 0; //total of 0 connection loc's
            int totalConnectionsOne = 0; //total of 1 connection loc's
            int totalConnectionsMade = 0; //number of connections made this time
            //Loop all locations
            for (int row = 0; row < mapSize; row++)
            {
                for (int column = 0; column < mapSize; column++)
                {
                    int numConnections = 0;
                    //location present (ignore capital)
                    if (mapGrid[(int)MapLayer.Base, column, row] == 1)
                    {
                        numConnections = CheckConnection(column, row);
                        if (numConnections == 1)
                        {
                            totalConnectionsOne++;
                            //must be an inner node (only these and the Capital present) - add to list
                            Position pos = new Position();
                            pos.PosX = column; pos.PosY = row;
                            nodes.Add(pos);
                            //Console.WriteLine("Debug: Node added to List {0}:{1}", column, row);
                        }
                        else if (numConnections == 0)
                        { totalConnectionsZero++; }
                    }
                }
            }
            if (totalConnectionsZero > 0 && totalConnectionsOne > 0)
            {
                //Outer loop (gradually increasing limitDistance)
                float searchDistance = 2.0f; //look for 0 connection locations to connect to within this distance, increments each complete pass through list up to maxDistance .
                for (int d = (int)searchDistance; d < maxDistance; d++)
                {
                    //Console.WriteLine("Debug: Outer loop {0} ---", d);
                    //middle loop (nodes in list)
                    foreach (Position pos in nodes)
                    {
                        int nodeConnections = CheckConnection(pos.PosX, pos.PosY); // already has one from the capital
                        //Console.WriteLine("debug: {0} connections for node {1}:{2}", nodeConnections, pos.PosX, pos.PosY);
                        if (nodeConnections < 4)
                        {
                            //inner loop
                            for (int row = 0; row < mapSize; row++)
                            {
                                for (int column = 0; column < mapSize; column++)
                                {
                                    int locConnections = 0;
                                    //location present (ignore capital) & node hasn't maxxed out connections
                                    if (mapGrid[(int)MapLayer.Base, column, row] == 1 && nodeConnections < 4)
                                    {
                                        locConnections = CheckConnection(column, row);
                                        //if (locConnections > 1)
                                        //{ Console.WriteLine("Debug: Loc at {0}:{1} has {2} connections", column, row, locConnections); }
                                        if (locConnections == 0)
                                        {
                                            //check distance from node
                                            if (MapDistance(pos.PosX, pos.PosY, column, row) <= searchDistance)
                                            {
                                                //attempt to form a connection
                                                if (CheckRouteOrthogonal(pos.PosX, pos.PosY, column, row) == true)
                                                { nodeConnections++; totalConnectionsMade++; }
                                                else
                                                {
                                                    if (CheckRouteDogLeg(pos.PosX, pos.PosY, column, row) == true)
                                                    { nodeConnections++; totalConnectionsMade++; }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    searchDistance++;
                    //Console.WriteLine("Debug: limitDistance {0}", searchDistance);
                }
            }
            else
            { finished = true; } // run out of 1 or 0 connection locations
            if(totalConnectionsMade == 0)
            { finished = true; } //no connections made this run through, must be the end of the road
            return finished;
        }

        //check for a route between two locations (straight shot left or right, up or down)
        public bool CheckRouteOrthogonal(int x1, int y1, int x2, int y2)
        {
            bool route = true;
            //assumes same row
            if (y1 == y2)
            {
                //check route to the right
                if (x2 > x1)
                {
                    route = CheckRouteRight(x1, y1, x2, y2);
                    //if true, set route in mapGrid
                    if (route == true)
                    { SetRouteRight(x1, y1, x2, y2); }
                }
                //check route to the left
                else if (x1 > x2)
                {
                    route = CheckRouteLeft(x1, y1, x2, y2);
                    //if true, set route in mapGrid
                    if (route == true)
                    { SetRouteLeft(x1, y1, x2, y2); }
                }
            }
            //assumes same column
            else if (x1 == x2)
            {
                //check route Up
                if (y1 > y2)
                {
                    route = CheckRouteUp(x1, y1, x2, y2);
                    //if true, set route in mapGrid
                    if (route == true)
                    { SetRouteUp(x1, y1, x2, y2); }
                }
                //check route Down
                else if (y2 > y1)
                {
                    route = CheckRouteDown(x1, y1, x2, y2);
                    //if true, set route in mapGrid
                    if (route == true)
                    { SetRouteDown(x1, y1, x2, y2); }
                }
            }
            //no orthogonal route exists
            else
            { route = false; }
            return route; 
        }

        //Does route exist between points moving RIGHT?
        private bool CheckRouteRight(int x1, int y1, int x2, int y2)
        {
            bool route = true;
            int limit = x2;
            if (y1 == y2)
                { limit--; } //orthogonal
            for (int i = (x1 + 1); i <= limit; i++)
            {
                if (mapGrid[(int)MapLayer.Base, i, y1] > 0)
                { route = false; break; }
            }
            return route;
        }

        //Does route exist between points moving LEFT?
        private bool CheckRouteLeft(int x1, int y1, int x2, int y2)
        {
            bool route = true;
            int limit = x2;
            if (y1 == y2)
                { limit++; } //orthogonal
            for (int i = (x1 - 1); i >= limit; i--)
            {
                if (mapGrid[(int)MapLayer.Base, i, y1] > 0) { route = false; break; }
            }
            return route;
        }

        //Does route exist between points moving UP?
        private bool CheckRouteUp(int x1, int y1, int x2, int y2)
        {
            bool route = true;
            int limit = y2;
            if (x1 == x2)
                { limit++;  } //orthogonal
            for (int i = (y1 - 1); i >= limit; i--)
            {
                if (mapGrid[(int)MapLayer.Base, x1, i] > 0) { route = false; break; }
            }
            return route;
        }

        //Does route exist between points moving DOWN?
        private bool CheckRouteDown(int x1, int y1, int x2, int y2)
        {
            bool route = true;
            int limit = y2;
            if(x1 == x2)
                { limit--; } //orthogonal
            for (int i = (y1 + 1); i <= limit; i++)
            {
                if (mapGrid[(int)MapLayer.Base, x1, i] > 0) { route = false; break; }
            }
            return route;
        }

        //Set the route in mapGrid between points moving RIGHT (direction: 1  up tick, -1 down tick, 0 straight ahead)
        private void SetRouteRight(int x1, int y1, int x2, int y2, int direction = 0)
        {
            int limit = x2;
            if(y1 == y2)
                { limit--; } //orthogonal
            for (int i = x1 + 1; i <= limit; i++)
            {
                if (mapGrid[(int)MapLayer.Base, i, y1] == 0)
                {
                    mapGrid[(int)MapLayer.Base, i, y1] = 11;
                    if (i == x2)
                    {
                        if (direction == 1) { mapGrid[(int)MapLayer.Base, i, y1] = 12; } //dogleg Up
                        else if (direction == -1) { mapGrid[(int)MapLayer.Base, i, y1] = 13; } //dogleg Down
                    }
                }
                else { Console.WriteLine("Debug: ERROR in SET route to the RIGHT mapGrid[{0},{1}] = {2}", i , y1, mapGrid[(int)MapLayer.Base, i, y1]); }
            }
        }

        //Set the route in mapGrid between points moving LEFT (direction: 1  up tick, -1 down tick, 0 straight ahead)
        private void SetRouteLeft(int x1, int y1, int x2, int y2, int direction=0)
        {
            int limit = x2;
            if(y1 == y2)
                { limit++; } //orthogonal
            for (int i = x1 - 1; i >= limit; i--)
            {
                if (mapGrid[(int)MapLayer.Base, i, y1] == 0)
                {
                    mapGrid[(int)MapLayer.Base, i, y1] = 11;
                    if (i == x2)
                    {
                        if (direction == 1) { mapGrid[(int)MapLayer.Base, i, y1] = 14;} //dogleg Up
                        else if (direction == -1) { mapGrid[(int)MapLayer.Base, i, y1] = 15; } //dogleg Down
                    } 
                }
                else { Console.WriteLine("Debug: ERROR in SET route to the LEFT mapGrid[{0},{1}] = {2}", i, y1, mapGrid[(int)MapLayer.Base, i, y1]); }
            }
        }

        //Set the route in mapGrid between points moving UP (direction: 1  right tick, -1 left tick, 0 straight ahead)
        private void SetRouteUp(int x1, int y1, int x2, int y2, int direction = 0)
        {
            int limit = y2;
            if (x1 == x2)
            { limit++; } //orthogonal
            for (int i = y1 - 1; i >= limit; i--)
            {
                if (mapGrid[(int)MapLayer.Base, x1, i] == 0)
                {
                    mapGrid[(int)MapLayer.Base, x1, i] = 10;
                    if (i == y2)
                    {
                        if (direction == 1) { mapGrid[(int)MapLayer.Base, x1, i] = 15; } // dogleg up right
                        else if (direction == -1) { mapGrid[(int)MapLayer.Base, x1, i] = 13; } //dogleg up left
                    } 
                }
                else { Console.WriteLine("Debug: ERROR in SET route UP mapGrid[{0},{1}] = {2}, limit: {3}", x1, i, mapGrid[(int)MapLayer.Base, x1, i], limit); }
            }
        }

        //Set the route in mapGrid between points moving DOWN (direction: 1 right tick, -1 left tick, 0 straight ahead)
        private void SetRouteDown(int x1, int y1, int x2, int y2, int direction = 0)
        {
            int limit = y2;
            if (x1 == x2)
            { limit--; } //orthogonal
            for (int i = y1 + 1; i <= limit; i++)
            {
                if (mapGrid[(int)MapLayer.Base, x1, i] == 0)
                {
                    mapGrid[(int)MapLayer.Base, x1, i] = 10;
                    if (i == y2)
                    {
                        if (direction == 1) { mapGrid[(int)MapLayer.Base, x1, i] = 14; } // dogleg down right
                        else if (direction == -1) { mapGrid[(int)MapLayer.Base, x1, i] = 12; } //dogleg down left
                    }
                }
                else { Console.WriteLine("Debug: ERROR in SET Map route DOWN mapGrid[{0},{1}] = {2}, limit: {3}", x1, i, mapGrid[(int)MapLayer.Base, x1, i], limit); }
            }
        }

        //check for a route between two locations (dog leg combination, eg. knight's move)
        public bool CheckRouteDogLeg(int x1, int y1, int x2, int y2)
        {
            bool route = false;
            int diffColumns = x1 - x2; //difference in Columns (-ve RIGHT, +ve LEFT)
            int diffRows = y1 - y2; //difference in Rows (-ve DOWN, +ve UP)
            //check a dogleg exists
            if (diffColumns != 0 && diffRows != 0)
            {
                //route to the RIGHT--------------------
                if (diffColumns < 0)
                {
                    //route UP---
                    if (diffRows > 0)
                    {
                        //test RIGHT + UP
                        route = CheckRouteRight(x1, y1, x2, y2);
                        if (route == true)
                        //Right good, check Up
                        {
                            route = CheckRouteUp(x2, y1, x2, y2);
                            if (route == true)
                            { SetRouteRight(x1, y1, x2, y2, 1); SetRouteUp(x2, y1, x2, y2); }
                           
                        }
                        //test UP + RIGHT
                        else
                        {
                            route = CheckRouteUp(x1, y1, x2, y2);
                            if (route == true)
                            {
                                //Up good, check Right
                                route = CheckRouteRight(x1, y2, x2, y2);
                                if (route == true)
                                { SetRouteUp(x1, y1, x2, y2, 1); SetRouteRight(x1, y2, x2, y2, 1); }
                            }
                        }
                    }
                    //route DOWN---
                    else
                    {
                        //test RIGHT + DOWN
                        route = CheckRouteRight(x1, y1, x2, y2);
                        if (route == true)
                        //Right good, check Down
                        {
                            route = CheckRouteDown(x2, y1, x2, y2);
                            if (route == true)
                            { SetRouteRight(x1, y1, x2, y2, -1); SetRouteDown(x2, y1, x2, y2); }

                        }
                        //test DOWN + RIGHT
                        else
                        {
                            route = CheckRouteDown(x1, y1, x2, y2);
                            if (route == true)
                            {
                                //Down good, check Right
                                route = CheckRouteRight(x1, y2, x2, y2);
                                if (route == true)
                                { SetRouteDown(x1, y1, x2, y2, 1); SetRouteRight(x1, y2, x2, y2, -1); }
                            }
                        }
                    }
                }
                //route to the LEFT ----------------
                else
                {
                    //route UP---
                    if (diffRows > 0)
                    {
                        //test LEFT + UP
                        route = CheckRouteLeft(x1, y1, x2, y2);
                        if (route == true)
                        //Left good, check Up
                        {
                            route = CheckRouteUp(x2, y1, x2, y2);
                            if (route == true)
                            { SetRouteLeft(x1, y1, x2, y2, 1); SetRouteUp(x2, y1, x2, y2); }

                        }
                        //test UP + LEFT
                        else
                        {
                            route = CheckRouteUp(x1, y1, x2, y2);
                            if (route == true)
                            {
                                //Up good, check Left
                                route = CheckRouteLeft(x1, y2, x2, y2);
                                if (route == true)
                                { SetRouteUp(x1, y1, x2, y2, -1); SetRouteLeft(x1, y2, x2, y2, 1); }
                            }
                        }
                    }
                    //route DOWN---
                    else
                    {
                        //test LEFT + DOWN
                        route = CheckRouteLeft(x1, y1, x2, y2);
                        if (route == true)
                        //Left good, check Down
                        {
                            route = CheckRouteDown(x2, y1, x2, y2);
                            if (route == true)
                            { SetRouteLeft(x1, y1, x2, y2, -1); SetRouteDown(x2, y1, x2, y2); }

                        }
                        //test DOWN + LEFT
                        else
                        {
                            route = CheckRouteDown(x1, y1, x2, y2);
                            if (route == true)
                            {
                                //Down good, check Left
                                route = CheckRouteLeft(x1, y2, x2, y2);
                                if (route == true)
                                { SetRouteDown(x1, y1, x2, y2, -1); SetRouteLeft(x1, y2, x2, y2, -1); }
                            }
                        }
                    }
                }
                    
            }
            return route;
        }

        //checks if the Location has a connection by circling all orthogonal cells and looking for an appropriate road type
        //returns # of connections
        private int CheckConnection(int x, int y)
        {
            int connections = 0;
            int gridValue = 0;
            //check North
            if(y > 0)
            {
                gridValue = mapGrid[(int)MapLayer.Base, x, y - 1];
                if(gridValue > 0)
                {
                    if (gridValue == 10 || gridValue == 13 || gridValue == 15)
                    { connections++; } // Console.WriteLine("debug: connections North for {0}:{1}", x, y)
                }
            }
            //check East
            if(x < mapSize - 1)
            {
                gridValue = mapGrid[(int)MapLayer.Base, x + 1, y];
                if (gridValue > 0)
                {
                    if (gridValue == 11 || gridValue == 12 || gridValue == 13)
                    { connections++; } // Console.WriteLine("debug: connections East for {0}:{1}", x, y)
                }
            }
            //check South
            if(y < mapSize - 1)
            {
                gridValue = mapGrid[(int)MapLayer.Base, x, y + 1];
                if (gridValue > 0)
                {
                    if (gridValue == 10 || gridValue == 12 || gridValue == 14)
                    { connections++; } //Console.WriteLine("debug: connections South for {0}:{1}", x, y);
                }
            }
            //check West
            if(x > 0)
            {
                gridValue = mapGrid[(int)MapLayer.Base, x - 1, y];
                if (gridValue > 0)
                {
                    if (gridValue == 11 || gridValue == 14 || gridValue == 15)
                    { connections++; } //Console.WriteLine("debug: connections West for {0}:{1}", x, y);
                }
            }
            return connections;
        }

        //ROUTES =============

        //MASTER method to set up all routes & Locations. Input Capital coords.
        private void InitialiseRoutes(Position pos)
        {
            //Console.WriteLine("Debug: InitialiseRoutes ---");
            bool endStatus = false;
            bool startAtCapital; //used to set initial direction from captial
            List<Position> path = new List<Position>(); //sequential path of current route
            List<Position> pathPrevious = new List<Position>(); //used for reverting back to previous node
            Position newPos = new Position();
            Position otherPos = new Position();
            // create new Location for the capital and add to list
            Location locCapital = new Location(pos, 0, true);
            locCapital.Connections = CheckConnection(pos.PosX, pos.PosY);
            listOfLocations.Add(locCapital);


            //capital loop
            for (int capDirection = 1; capDirection < 5; capDirection++)
            {
                newPos = pos;
                startAtCapital = true;
                //main loop - keep backing up nodes until you return to the capital
                do
                {
                    //Console.WriteLine("Debug: Main Loop starts (capDirection {0} ---", capDirection);
                    //start at a node, check each direction in turn, if viable, go to next node
                    do
                    {
                        //Console.WriteLine("Debug: Outer Loop starts NEXT NODE IN ---");
                        for (int direction = 1; direction < 5; direction++)
                        {
                            //if starting out from capital, ensure correct direction
                            if (startAtCapital == true)
                            { direction = capDirection; startAtCapital = false; }
                            endStatus = true;
                            if (CheckRouteExists(newPos, direction))
                            {
                                //found a route
                                path = FollowRoute(newPos, direction);
                                newPos = path[path.Count - 1];
                                otherPos = path[0];
                                endStatus = EndOfRoad(otherPos, newPos);
                                
                                if (endStatus == false)
                                {
                                    //new route
                                    Route route = new Route(path);
                                    //add to list of all routes
                                    listOfRoutes.Add(route);
                                    //Console.WriteLine("Debug: NEW ROUTE ADDED - {0} records", listOfRoutes.Count);
                                    //print route for debugging
                                    //route.PrintRoute();
                                    // create new Location and add to list
                                    Location locNew = new Location(newPos, capDirection, false);
                                    locNew.Connections = CheckConnection(newPos.PosX, newPos.PosY);
                                    listOfLocations.Add(locNew);
                                    break;
                                }
                                else
                                {
                                    //route already exists, continue checking existing node for possible routes
                                    endStatus = true; //reset so to enable continuation
                                    newPos = path[0]; //reset origin point
                                    //Console.WriteLine("Debug: ROUTE EXISTS (direction {0}), continue checking )", direction);
                                }
                            }
                        }
                    }
                    while (endStatus == false);
                    //Console.WriteLine("Debug: BACK UP A NODE - Before: newPos {0}:{1}", newPos.PosX, newPos.PosY);
                    //O.K, back up a node - get last route in listOfRoutes and get starting pos
                    //if NOT at end of branch (but still backing up a node), the last route in the list isn't the one you want

                    pathPrevious = GetPreviousNode(newPos);
                    newPos = pathPrevious[0];
                    //otherPos = pathPrevious[pathPrevious.Count - 1];
                    //Console.WriteLine("Debug: BACK UP A NODE - After: newPos {0}:{1}", newPos.PosX, newPos.PosY);
                    //Console.WriteLine();
                }
                //check if at capital
                while (newPos.PosX != capitalX || newPos.PosY != capitalY);
                //Console.WriteLine("Debug: Map Summary ---");
                //Console.WriteLine("Debug: listOfRoutes {0} records", listOfRoutes.Count);
                //Console.WriteLine("Debug: listOfLocations {0} records", listOfLocations.Count);
            }
        }



        //method called when Initialise route needs to go back a node.
        //search listOfRoutes in reverse until pos != first record and pos = last record to find correct Route
        private List<Position> GetPreviousNode(Position pos)
        {
            Route route = new Route();
            List<Position> path = new List<Position>();
            Position posStart = new Position();
            Position posEnd = new Position();
            for (int i = listOfRoutes.Count - 1; i >= 0; i--)
            {
                route = listOfRoutes[i];
                path = route.GetPath();
                posStart = path[0];
                posEnd = path[path.Count - 1];
                //Console.WriteLine("Debug: GetPreviousNode - Route {0} of {1} - Start {2}:{3} End {4}:{5})", i, listOfRoutes.Count, posStart.PosX, posStart.PosY, posEnd.PosX, posEnd.PosY);
                if((pos.PosX != posStart.PosX || pos.PosY != posStart.PosY) && (pos.PosX == posEnd.PosX && pos.PosY == posEnd.PosY))
                { return path; }
            }
            //Console.WriteLine("Debug: ERROR in GetPreviousNode - no suitable route found");
            return path;
        }

        //change all found routes to red on map if true, default gray if false
        public void ShowRoutes(bool show = false)
        {
            List<Position> path = new List<Position>();
            foreach (Route route in listOfRoutes)
            {
                //route.PrintRouteDetails();
                path = route.GetPath();
                foreach (Position pos in path)
                {
                    if (show)
                    {
                        //roads only, set all roads to red (20 - 25) to indicate a known route
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 10 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 15)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] += 10; }
                        //for dEBUG mode - roads only, set all roads to red (20 - 25) to indicate a known route
                        else if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 40 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 45)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 20; }
                    }
                    else if (!show)
                    {
                        //roads only, set all roads to back to normal (10 - 15) to indicate a known route
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 20 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 25)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 10; }
                        //for DEBUG mode - roads only, set all roads to back to normal (10 - 15) to indicate a known route
                        else if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 40 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 45)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 30; }
                    }
                    //clear out debug grid regardless
                    mapGrid[(int)MapLayer.Debug, pos.PosX, pos.PosY] = 0;
                }
            }

        }

        //change all found connector routes to red on map if true, default gray if false
        public void ShowConnectorRoutes(bool show = false)
        {
            List<Position> path = new List<Position>();
            foreach (Route route in listOfConnectors)
            {
                //route.PrintRouteDetails();
                path = route.GetPath();
                foreach (Position pos in path)
                {
                    if (show)
                    {
                        //roads only, set all roads to red (20 - 25) to indicate a known route
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 10 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 15)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] += 10; }
                        //for dEBUG mode - roads only, set all roads to red (20 - 25) to indicate a known route
                        else if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 40 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 45)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 20; }
                    }
                    else if (!show)
                    {
                        //roads only, set all roads to back to normal (10 - 15) to indicate a known route
                        if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 20 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 25)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 10; }
                        //for DEBUG mode - roads only, set all roads to back to normal (10 - 15) to indicate a known route
                        else if (mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] >= 40 && mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] <= 45)
                        { mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY] -= 30; }
                    }
                    //clear out debug grid regardless
                    mapGrid[(int)MapLayer.Debug, pos.PosX, pos.PosY] = 0;
                }
            }
        }
        
        //check to see if route finder has reached the end of a branch (input start and end of path)
        //routes are always in the format closest of moving further away from the capital (in road distance)
        private bool EndOfRoad(Position posStart, Position posEnd)
        {
            bool endStatus = false;
            Position searchPosStart = new Position();
            Position searchPosEnd = new Position();
            foreach (Route tempRoute in listOfRoutes)
            {
                searchPosStart = tempRoute.GetLoc1();
                searchPosEnd = tempRoute.GetLoc2();
                //note: need to check for routes in both directions as once end of branch reached it'll turn around and attempt to go back up the route in reverse
                if (posStart.PosX == searchPosStart.PosX && posStart.PosY == searchPosStart.PosY || posEnd.PosX == searchPosStart.PosX && posEnd.PosY == searchPosStart.PosY)
                {
                    if (posStart.PosX == searchPosEnd.PosX && posStart.PosY == searchPosEnd.PosY || posEnd.PosX == searchPosEnd.PosX && posEnd.PosY == searchPosEnd.PosY)
                    {
                        //route already exists - end of road
                        //Console.WriteLine("Debug: EndOfRoad - ROUTE EXISTS");
                        endStatus = true; break;
                    }  
                }
            }
            return endStatus;
        }

        //takes a location and checks for a possible route (start of) in a given direction
        // direction of route (0 none, 1 North, 2 East, 3 South, 4 West)
        private bool CheckRouteExists( Position pos, int direction)
        {
            //Console.WriteLine("Debug: CheckRouteExists ---");
            //Console.WriteLine("Debug: direction {0}, Position {1}:{2}", direction, pos.PosX, pos.PosY);
            int cell = 0; //current cell contents in mapGrid
            bool viableRoute = false;
            //check relevant cell for presence of a route
            switch (direction)
                {
                    case 1:
                        //north
                        if (pos.PosY > 0)
                        {
                            cell = mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY - 1];
                            if (cell == 10 || cell == 13 || cell == 15)
                            { viableRoute = true; }
                        }
                        break;
                    case 2:
                        //east
                        if (pos.PosX < mapSize -1)
                        { 
                            cell = mapGrid[(int)MapLayer.Base, pos.PosX + 1, pos.PosY];
                            if (cell == 11 || cell == 12 || cell == 13)
                            { viableRoute = true; }
                        }
                        break;
                    case 3:
                        //south
                        if (pos.PosY < mapSize - 1)
                        {
                            cell = mapGrid[(int)MapLayer.Base, pos.PosX, pos.PosY + 1];
                            if (cell == 10 || cell == 12 || cell == 14)
                            { viableRoute = true; }
                        }
                        break;
                    case 4:
                        //west
                        if (pos.PosX > 0)
                        {
                            cell = mapGrid[(int)MapLayer.Base, pos.PosX - 1, pos.PosY];
                            if (cell == 11 || cell == 14 || cell == 15)
                            { viableRoute = true; }
                        }
                        break;
                    default:
                        //automatically exit if no viable startDirection input
                        return false;
                }
                //all directions checked, no route found
                return viableRoute;
        }

        //follows a route from the input loc position to it's terminal location
        //returns a list of Positions that detail the sequential cell by cell route
        private List<Position> FollowRoute(Position pos, int direction)
        {
            Position posMethod = new Position();
            posMethod.PosX = pos.PosX; posMethod.PosY = pos.PosY;
            List<Position> path = new List<Position>();
            //Console.WriteLine("Debug: FollowRoute ---");
            //Console.WriteLine("Debug: direction {0}, position {1}:{2}", direction, posMethod.PosX, posMethod.PosY);
            int currentX = posMethod.PosX; //starting location in mapGrid
            int currentY = posMethod.PosY;
            int newX = posMethod.PosX; //next cell along location in mapGrid
            int newY = posMethod.PosY;
            int cell = mapGrid[(int)MapLayer.Base, currentX, currentY];
            //add starting location to list
            if(cell > 0 && cell < 4)
            { path.Add(pos); }
            //continue until a location is reached
            do
            {
                //move to next cell depending on direction
                switch(direction)
                {
                    case 1:
                        //UP
                        newX = currentX;
                        newY = currentY - 1;
                        break;
                    case 2:
                        //RIGHT
                        newX = currentX + 1;
                        newY = currentY;
                        break;
                    case 3:
                        //DOWN
                        newX = currentX;
                        newY = currentY + 1;
                        break;
                    case 4:
                        //LEFT
                        newX = currentX - 1;
                        newY = currentY;
                        break;
                    default:
                        //error catcher: bad data
                        Console.WriteLine("Debug: ERROR 1 in FollowRoute");
                        break;
                }
                //get new cell contents
                cell = mapGrid[(int)MapLayer.Base, newX, newY];
                //update pos
                posMethod.PosX = newX; posMethod.PosY = newY;
                //need a new Position otherwise passed by reference
                Position posInput = new Position();
                posInput.PosX = newX; posInput.PosY = newY;
                //add current pos to list
                path.Add(posInput);
                //get new direction from new cell value - SWITCH
                switch(cell)
                {
                    case 10:
                        //vertical
                        if (currentY - newY > 0)
                        { direction = 1; } //UP
                        else
                        { direction = 3; } //DOWN
                        break;
                    case 11:
                        //horizontal
                        if (currentX - newX > 0)
                        { direction = 4; } //LEFT
                        else
                        { direction = 2; } //RIGHT
                        break;
                    case 12:
                        if (currentX - newX < 0)
                        { direction = 1; } //UP
                        else
                        { direction = 4; } //LEFT
                        break;
                    case 13:
                        if (currentX - newX < 0)
                        { direction = 3; } //DOWN
                        else
                        { direction = 4; } //LEFT
                        break;
                    case 14:
                        if(currentX - newX > 0)
                        { direction = 1; } //UP
                        else
                        { direction = 2; } //RIGHT
                        break;
                    case 15:
                        if(currentX - newX > 0)
                        { direction = 3; } //DOWN
                        else
                        { direction = 2; } //RIGHT
                        break;
                }
                //update current coords (AFTER determining direction above as previous current coords needed for this)
                //Console.WriteLine("Debug: Current {0}:{1}, New {2}:{3}", currentX, currentY, newX, newY);
                currentX = newX; currentY = newY;

            }
            while (cell > 3);
            return path;
        }

        //return list of routes
        internal List<Route> GetRoutes()
        { return listOfRoutes; }

        //return list of Connectors
        internal List<Route> GetConnectors()
        { return listOfConnectors; }

        //return array of Connections
        public int[,] GetArrayOfConnectors()
        { return arrayOfConnectors; }

        //return list of locations
        internal List<Location> GetLocations()
        { return listOfLocations; }

        //initialise connections between end of branch nodes once main map has been made & roads and routes set up.
        private void InitialiseConnectors()
        {
            List<Location> listOfSingleConnectionLocs = new List<Location>();
            int capitalDirection = 0;
            float distance = 0f;
            bool foundMatch = false;
            bool[] branches = new bool[] { false, false, false, false, false }; // used to indicate if branch has made a connection (max one connection per branch) branches 1 to 4 (ignore 0)
            int outer = 0;
            int inner = 0;
            int x1; int y1; int x2; int y2;
            //loop through entire process a number of times to ensure close connections are made before distant ones (making i larger enables smaller connections)
            for (int i = 8; i > 0; i--)
            {
                //clear list
                listOfSingleConnectionLocs.Clear();
                //loop all loc's and store those with a single connection (end of branch)
                foreach (Location loc in listOfLocations)
                {
                    if (loc.Connections == 1)
                    {
                        //add to list only if the locations branch is false (hasn't yet had a connector assigned)
                        int dir = loc.GetCapitalRouteDirection();
                        if (branches[dir] == false)
                        { listOfSingleConnectionLocs.Add(loc); }
                    }
                }
                //OUTER loop list of single Connectors (find an origin)
                for(outer = 0; outer < listOfSingleConnectionLocs.Count; outer++ )
                {
                    Location locOrigin = listOfSingleConnectionLocs[outer];
                    capitalDirection = locOrigin.GetCapitalRouteDirection();
                    //INNER loop list of single Connectors (find a destination)
                    for (inner = 0; inner < listOfSingleConnectionLocs.Count; inner++)
                    {
                        foundMatch = false;
                        Location locDestination = listOfSingleConnectionLocs[inner];
                        //not origin (outer) and origin (inner)
                        if(outer != inner)
                        {
                            //not on the same branch
                            if(capitalDirection != locDestination.GetCapitalRouteDirection())
                            {
                                //distance <= map size / i (gradually expanding distance search)
                                x1 = locOrigin.GetPosX();
                                y1 = locOrigin.GetPosY();
                                x2 = locDestination.GetPosX();
                                y2 = locDestination.GetPosY();
                                distance = MapDistance(x1, y1, x2, y2);
                                if( distance <= mapSize / i)
                                {
                                    //check to see if a route exists
                                    if (CheckRouteOrthogonal(x1, y1, x2, y2) == false)
                                    {
                                        if (CheckRouteDogLeg(x1, y1, x2, y2) == true)
                                        {
                                            //success
                                            foundMatch = true;
                                            break;
                                        }
                                    }
                                    //orthagonal route exist
                                    else
                                    {
                                        //success
                                        foundMatch = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //exit loop if match found
                    if (foundMatch == true)
                    { break; }
                }
                //match
                if (foundMatch == true)
                {
                    Location loc1 = listOfSingleConnectionLocs[outer];
                    Location loc2 = listOfSingleConnectionLocs[inner];
                    int direction1 = loc1.GetCapitalRouteDirection();
                    int direction2 = loc2.GetCapitalRouteDirection();
                    //flag branches array to indicate that these branches already have a connection (excluded from next list of Single Connectors)
                    branches[direction1] = true;
                    branches[direction2] = true;
                    //set connector status
                    loc1.SetConnector(true);
                    loc2.SetConnector(true);
                    //Base node positions
                    Position pos1 = loc1.GetPosition();
                    Position pos2 = loc2.GetPosition();
                    //loop loc1 looking for the connector route
                    for(int k = 1; k <= 4; k++ )
                    {
                        if(CheckRouteExists(pos1, k))
                        {
                            //follow route
                            List<Position> path = FollowRoute(pos1, k);
                            List<Position> pathReverse = FollowRoute(pos1, k);
                            pathReverse.Reverse();
                            //route to connector?
                            Position posCheck = path[path.Count - 1];
                            //found connector route (only other route must be to neighbour)
                            if(posCheck.PosX == pos2.PosX && posCheck.PosY == pos2.PosY )
                            {
                                //add route to list of Connector routes & exit
                                Route route1 = new Route(path);
                                listOfConnectors.Add(route1);
                                //reverse path & loc1 & loc2 and add reciprocal route
                                Route route2 = new Route(pathReverse);
                                //route2.ReverseRoute();
                                listOfConnectors.Add(route2);
                                //update arrayOfConnectors
                                arrayOfConnectors[direction1,0] = direction2;
                                arrayOfConnectors[direction1, 1] = path.Count - 1;
                                arrayOfConnectors[direction1, 2] = pos1.PosX;
                                arrayOfConnectors[direction1, 3] = pos1.PosY;
                                arrayOfConnectors[direction1, 4] = pos2.PosX;
                                arrayOfConnectors[direction1, 5] = pos2.PosY;
                                //do the same for the reciprocal route
                                arrayOfConnectors[direction2, 0] = direction1;
                                arrayOfConnectors[direction2, 1] = path.Count - 1;
                                arrayOfConnectors[direction2, 2] = pos2.PosX;
                                arrayOfConnectors[direction2, 3] = pos2.PosY;
                                arrayOfConnectors[direction2, 4] = pos1.PosX;
                                arrayOfConnectors[direction2, 5] = pos1.PosY;
                                
                                //exit
                                break;
                            }
                        }
                    }
                }
                //check to see if all connections have been made (max of 2 routes in a square cell set-up as only 1 unique connector per branch)
                int counter = 0;
                foreach(bool flag in branches)
                {
                    if (flag == true)
                    { counter++; }
                }
                //all four branch directions used? Exit search for possible Connectors
                if (counter > 3)
                { break; }
            }
        }

        //method (debug) to print out array data
        private void ShowArrayOfConnectors()
        {
            Console.WriteLine();
            Console.WriteLine("arrayOfConnectors ---");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("From Branch {6} To Branch {0} Distance {1} Origin {2}:{3} Destination {4}:{5}",
                  arrayOfConnectors[i, 0], arrayOfConnectors[i, 1], arrayOfConnectors[i, 2], arrayOfConnectors[i,3], arrayOfConnectors[i,4], arrayOfConnectors[i,5], i); }
            Console.WriteLine();
        }

        /// <summary>
        /// Sets up various map layers - LocID
        /// </summary>
        private void InitialiseMapLayers()
        {
            //create Location ID layer (places Loc ID at coords of every location on LocID layer.
            foreach(Location loc in listOfLocations)
            { mapGrid[(int)MapLayer.LocID, loc.GetPosX(), loc.GetPosY()] = loc.LocationID; }
        }

        /// <summary>
        /// Input X & Y coords and return LocID based on MapGrid Layer. Converts raw mouse input coords if needed.
        /// </summary>
        /// <param name="x_cord"></param>
        /// <param name="y_cord"></param>
        /// /// <param name="convert">Converts raw mouse coords if true, default false</param>
        /// <returns>'0' if location ID not valid</returns>
        public int GetLocationID(int x_cord, int y_cord, bool convert = false)
        {
            int locID = 0;
            //convert raw mouse input coords?
            if(convert == true)
            {
                x_cord= (x_cord - margin - offsetHorizontal) / 3;
                y_cord = (y_cord - margin - offsetVertical - 19) / 3;
            }
            return locID = mapGrid[(int)MapLayer.LocID, x_cord, y_cord];
        }

        /// <summary>
        /// Update MapGrid [Player] layer with a dictionary of current Player positions
        /// </summary>
        /// <param name="dictUpdatePlayers">Position coord and mapMarker for Player Move Object</param>
        public void UpdatePlayers(Dictionary<Position, int> dictUpdatePlayers)
        {
            //clear out the Player layer of the grid first
            for(int row = 0; row < mapSize; row++)
            {
                for (int column = 0; column < mapSize; column++)
                { mapGrid[(int)MapLayer.Player, row, column] = 0; }
            }
            //update with new data
            foreach(KeyValuePair<Position, int> entry in dictUpdatePlayers)
            { mapGrid[(int)MapLayer.Player, entry.Key.PosX, entry.Key.PosY] = entry.Value; }
        }

        public void UpdateMap(bool showRoutes = false, bool showConnectors = false)
        {
            ShowRoutes(showRoutes);
            ShowConnectorRoutes(showConnectors);
        }

        /// <summary>
        /// takes raw x, y mouse input and converts to a map Position
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public Position ConvertMouseCoords(int mouseX, int mouseY)
        {
            int convertX, convertY;
            convertX = (mouseX - margin - offsetHorizontal) / 3;
            convertY = (mouseY - margin - offsetVertical - 19) / 3;
            Position pos = new Position(convertX, convertY);
            return pos;
        }


        //--- place new method above---
    }
}