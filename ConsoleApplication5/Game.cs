using System;
using System.Collections.Generic;
using RLNET;
using Next_Game.Cartographic;

namespace Next_Game
{


    public class Game
    {
        //game variables
        //int mapSize = 0;
        //int seed = 0;
        
        //main constructor
        /*public Game(int mapSize, int seed)
        {
            this.mapSize = mapSize;
            this.seed = seed;
        }*/


        public void ProcessTurn()
        {
            world.IncrementGameTurn();
            map.UpdatePlayers(world.MoveCharacters());
        }


        /// <summary>
        /// General location input routine
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns Location ID</returns>
        private int GetLocationIDInput(string name)
        {
            Console.WriteLine("Input {0} Coordinates in format X,Y", name);
            string coords = Console.ReadLine();
            string[] tokens = coords.Split(',');
            //Input Position
            int x_Cord = Convert.ToInt32(tokens[0]);
            int y_Cord = Convert.ToInt32(tokens[1]);
            int locID = map.GetLocationID(x_Cord, y_Cord);
            return locID;
        }

        /// <summary>
        /// General location input routine
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns Position</returns>
        private Position GetLocationPosInput(string name)
        {
            int locID = 0;
            Position pos = new Position();
            do
            {

                Console.WriteLine("Input {0} Coordinates in format X,Y", name);
                string coords = Console.ReadLine();
                string[] tokens = coords.Split(',');
                //Input Position
                int x_Cord = Convert.ToInt32(tokens[0]);
                int y_Cord = Convert.ToInt32(tokens[1]);
                locID = map.GetLocationID(x_Cord, y_Cord);

                if (locID > 0)
                { pos.PosX = x_Cord; pos.PosY = y_Cord; }
                else
                { Console.WriteLine("ERROR: A Location does not exist at that position (try again)"); }
            }
            while (locID == 0);
            return pos;
        }

        /// <summary>
        /// handles initial input for selection of character and returns pos of location (null if travelling)
        /// </summary>
        /// <param name="inputConsole"></param>
        /// <param name="consoleDisplay"></param>
        /// <param name="charID"></param>
        /// <returns></returns>
        public Position GetCharacterLocation(RLConsole inputConsole, ConsoleDisplay consoleDisplay, int charID)
        {
            Position pos = new Position();
            List<string> charList = new List<string>();
            charList.Add(world.GetCharacterRL(charID));
            pos = world.GetCharacterLocationByPos(charID);
            if (pos != null)
            { charList.Add("Click on the Destination location or press [ESC] to cancel" );}
            else
            { charList.Add("The character is not currently at your disposal"); }
            infoChannel.SetInfoList(charList, consoleDisplay);
            infoChannel.DrawInfoConsole(inputConsole, consoleDisplay);
            return pos;
        }

        public void MoveCharacters(Position posOrigin, Position posDestination, int charID, RLConsole inputConsole, ConsoleDisplay inputDisplay)
        {
            List<Position> pathToTravel = network.GetPathAnywhere(posOrigin, posDestination);
            string infoText = world.InitiateMoveCharacters(charID, posOrigin, posDestination, pathToTravel);
            Program.messageLog.Add(infoText, world.GetGameTurn());
            infoChannel.AppendInfoList(infoText, inputDisplay);
            infoChannel.DrawInfoConsole(inputConsole, inputDisplay, true);
        }

        /// <summary>
        /// when a character # is pressed in the main menu it switches to the character menu and this highlights which character you've chosen
        /// </summary>
        /// <param name="charID"></param>
        public void ShowSelectedCharacter(RLConsole inputConsole, ConsoleDisplay consoleDisplay, int charID)
        {
            List<string> infoList = new List<string>();
            infoList.Add(world.ShowSelectedCharacter(charID));
            infoChannel.SetInfoList(infoList, consoleDisplay);
            infoChannel.DrawInfoConsole(inputConsole, consoleDisplay, true);
        }


        /// <summary>
        /// Main map update routine (Draws map only, other options handle ShowRoute/ShowConnectorRoutes)
        /// </summary>
        public void DrawMap(RLConsole mapConsole)
        {
           mapConsole.Clear();
           map.DrawMapRL(mapConsole);
        }

        /// <summary>
        /// General purpose map update routine
        /// </summary>
        /// <param name="showRoutes"></param>
        /// <param name="showConnectors"></param>
        public void UpdateMap(bool showRoutes = false, bool showConnectors = false)
        {
            map.ShowRoutes(showRoutes);
            map.ShowConnectorRoutes(showConnectors);
        }

        /// <summary>
        /// Display player character status to multi-Console
        /// </summary>
        /// <param name="multiConsole"></param>
        /// <param name="locationsOnly">show characters at locations and NOT travelling if true (default false - show all)</param>
        public void ShowCharacters(RLConsole multiConsole, ConsoleDisplay consoleDisplay, bool locationsOnly = false)
        {
            //output to console
            world.ShowPlayerCharacters(false);
            //output to RL
            multiConsole.Clear();
            infoChannel.SetInfoList(world.ShowPlayerCharactersRL(locationsOnly), consoleDisplay);
            infoChannel.DrawInfoConsole(multiConsole, consoleDisplay);
        }

        /// <summary>
        /// Display Location based on Left Mouse Click
        /// </summary>
        /// <param name="multiConsole"></param>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        public void ShowLocationMulti(RLConsole multiConsole, int mouseX, int mouseY)
        {
            Position pos = new Position(map.ConvertMouseCoords(mouseX, mouseY));
            infoChannel.SetInfoList(world.ShowLocationRL(pos), ConsoleDisplay.Multi);
            infoChannel.DrawInfoConsole(multiConsole, ConsoleDisplay.Multi);
        }

        //returns true if a valid location found
        public bool ShowLocationInput(RLConsole inputConsole, int mouseX, int mouseY)
        {
            bool validLocation = false;
            Position pos = new Position(map.ConvertMouseCoords(mouseX, mouseY));
            string locName = world.GetLocationName(pos);
            if (locName != "unknown")
            {
                infoChannel.AppendInfoList(locName, ConsoleDisplay.Input);
                validLocation = true;
            }
            infoChannel.DrawInfoConsole(inputConsole, ConsoleDisplay.Input);
            return validLocation;
        }


        public void ShowRoute(RLConsole mapConsole, Position pos1, Position pos2)
        {
            //check that the two coords aren't identical
            if ((pos1 != null && pos2 != null) && (pos1.PosX != pos2.PosX || pos1.PosY != pos2.PosY))
            {
                List<Route> listOfRoutes = new List<Route>();
                listOfRoutes = network.GetRouteAnywhere(pos1, pos2);
                map.DrawRouteRL(listOfRoutes);
                map.DrawMapRL(mapConsole);
            }
        }

        public void ShowRouteDebug(RLConsole mapConsole, Position pos1, Position pos2)
        {
            //check that the two coords aren't identical
            if ((pos1 != null && pos2 != null) && (pos1.PosX != pos2.PosX || pos1.PosY != pos2.PosY))
            {
                List<Route> listOfRoutes = new List<Route>();
                listOfRoutes = network.GetRouteAnywhere(pos1, pos2);
                map.DrawRouteDebug(listOfRoutes);
                map.DrawMapRL(mapConsole);
            }
        }


        public void GetRouteOrigin(RLConsole inputConsole)
        {
            inputConsole.Clear();
            List<string> inputList = new List<string>();
            inputList.Add("--- Show the Route between two Locations");
            inputList.Add("Select ORIGIN Location by Mouse (press ESC to Exit)");
            infoChannel.SetInfoList(inputList, ConsoleDisplay.Input);
            infoChannel.DrawInfoConsole(inputConsole, ConsoleDisplay.Input);
        }

        public void GetRouteDestination(RLConsole inputConsole)
        {
            List<string> inputList = new List<string>();
            infoChannel.AppendInfoList("Select DESTINATION Location by Mouse (press ESC to Exit)", ConsoleDisplay.Input);
            infoChannel.DrawInfoConsole(inputConsole, ConsoleDisplay.Input);
        }

        /// <summary>
        /// Mouse output in form of Location and coords written to input-Console - No console output, take click, return pos only
        /// </summary>
        /// <param name="inputConsole"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// /// <returns>Returns position in map coordinates, returns 'null' if not a location</returns>
        public Position MouseInput(int x, int y)
        {
            //convert coords
            Position pos = new Position(map.ConvertMouseCoords(x, y));
            //check for a location at coords
            string locName = world.GetLocationName(pos);
            if( locName == "unknown")
            { pos = null; }
            return pos;
        }

        public void MouseInputError(RLConsole inputConsole, ConsoleDisplay consoleDisplay)
        {
            infoChannel.AppendInfoList("ERROR: Not a valid Location", consoleDisplay);
            infoChannel.DrawInfoConsole(inputConsole, consoleDisplay, true);
        }

    }
}