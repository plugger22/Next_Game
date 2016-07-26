using System;
using System.Collections.Generic;
using RLNET;

namespace Next_Game
{
    //Handles display of information to the multi / input / status / spare Consoles
    public class InfoChannel
    {
        int multiMargin; //margin offset from left of page and top for info display on console
        int inputMargin;
        int statusMargin;
        private List<Snippet> multiList; //list of strings to display in the multi Console
        private List<Snippet> inputList; //list of strings to display in the input Console
        private List<Snippet> statusList; //list of strings to display in the Status Console


        public InfoChannel()
        {
            multiList = new List<Snippet>();
            inputList = new List<Snippet>();
            statusList = new List<Snippet>();
            multiMargin = 2;
            inputMargin = 2;
            statusMargin = 1;
        }

        /// <summary>
        /// Create a list of snippets to display
        /// </summary>
        /// <param name="listToDisplay"></param>
        /// <param name="consoleDisplay"></param>
        public void SetInfoList(List<Snippet> listToDisplay, ConsoleDisplay consoleDisplay)
        {
            switch(consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Clear();
                    inputList.AddRange(listToDisplay);
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Clear();
                    multiList.AddRange(listToDisplay);
                    break;
                case ConsoleDisplay.Status:
                    statusList.Clear();
                    statusList.AddRange(listToDisplay);
                    break;
            }
        }


        /// <summary>
        /// append a snippet to the list to display
        /// </summary>
        /// <param name="appendSnippet"></param>
        /// <param name="consoleDisplay"></param>
        public void AppendInfoList(Snippet appendSnippet, ConsoleDisplay consoleDisplay)
        {
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Add(appendSnippet);
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Add(appendSnippet);
                    break;
                case ConsoleDisplay.Status:
                    statusList.Add(appendSnippet);
                    break;
            }
        }


        public void DrawInfoConsole(RLConsole infoConsole, ConsoleDisplay consoleDisplay, bool clearDisplay = true)
        {
            List<Snippet> displayList = new List<Snippet>();
            int margin = 2;
            int maxLength = 10;
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    displayList = inputList;
                    margin = inputMargin;
                    break;
                case ConsoleDisplay.Multi:
                    displayList = multiList;
                    margin = multiMargin;
                    maxLength = 40;
                    break;
                case ConsoleDisplay.Status:
                    displayList = statusList;
                    margin = statusMargin;
                    break;
            }
            //max number of lines
            maxLength = Math.Min(maxLength, displayList.Count);

            if (clearDisplay)
            { infoConsole.Clear(); }
            int lineCounter = 0;
            for (int i = 0; i < maxLength; i++)
            {
                Snippet snippet = displayList[i];
                infoConsole.Print(margin, lineCounter * 2 + margin, snippet.GetText(), snippet.GetForeColor(), snippet.GetBackColor());
                //new line or on existing line?
                if (snippet.GetNewLine() == true)
                { lineCounter++; }
            }
        }

        /*public void SetInfoList(List<string> listToDisplay, ConsoleDisplay consoleDisplay)
        {
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Clear();
                    inputList.AddRange(listToDisplay);
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Clear();
                    multiList.AddRange(listToDisplay);
                    break;
                case ConsoleDisplay.Status:
                    statusList.Clear();
                    statusList.AddRange(listToDisplay);
                    break;
            }
        }

        public void AppendInfoList(string appendString, ConsoleDisplay consoleDisplay)
        {
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Add(appendString);
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Add(appendString);
                    break;
                case ConsoleDisplay.Status:
                    statusList.Add(appendString);
                    break;
            }
        }


        public void DrawInfoConsole(RLConsole infoConsole, ConsoleDisplay consoleDisplay, bool clearDisplay = true)
        {
            List<string> displayList = new List<string>();
            int margin = 2;
            int maxLength = 10;
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    displayList = inputList;
                    margin = inputMargin;
                    break;
                case ConsoleDisplay.Multi:
                    displayList = multiList;
                    margin = multiMargin;
                    maxLength = 40;
                    break;
                case ConsoleDisplay.Status:
                    displayList = statusList;
                    margin = statusMargin;
                    break;
            }
            //max number of lines
            maxLength = Math.Min(maxLength, displayList.Count);

            if (clearDisplay)
            { infoConsole.Clear(); }
            for (int i = 0; i < maxLength; i++)
            { infoConsole.Print(margin, i * 2 + margin, displayList[i], RLColor.White, RLColor.Black); }
        }*/

    }
}