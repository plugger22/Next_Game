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
        private Box eventBox;


        public InfoChannel()
        {
            multiList = new List<Snippet>();
            inputList = new List<Snippet>();
            statusList = new List<Snippet>();
            multiMargin = 2;
            inputMargin = 2;
            statusMargin = 1;
            RLColor backColor = Color._background1;
            eventBox = new Box(110, 40, 10, 10, backColor, RLColor.Black);
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


        /// <summary>
        /// clear out a console
        /// </summary>
        /// <param name="consoleDisplay"></param>
        public void ClearConsole(ConsoleDisplay consoleDisplay)
        {
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Clear();
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Clear();
                    break;
                case ConsoleDisplay.Status:
                    statusList.Clear();
                    break;
            }
        }


        /// <summary>
        /// Inserts a snippet to the head of a list to be displayed - do so AFTER calling AppendInfoList as it clears the list
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="consoleDisplay"></param>
        public void InsertHeader(Snippet snippet, ConsoleDisplay consoleDisplay)
        {
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    inputList.Insert(0, snippet);
                    break;
                case ConsoleDisplay.Multi:
                    multiList.Insert(0, snippet);
                    break;
                case ConsoleDisplay.Status:
                    statusList.Insert(0, snippet);
                    break;
            }
        }


        public void DrawInfoConsole(RLConsole infoConsole, ConsoleDisplay consoleDisplay, bool clearDisplay = true)
        {
            List<Snippet> displayList = new List<Snippet>();
            int margin = 2;
            int startIndex = 0;
            int dataLength = 10; //max lines of data allowed in console
            switch (consoleDisplay)
            {
                case ConsoleDisplay.Input:
                    //default date display if nothing present
                    if (inputList.Count == 0)
                    { inputList.Add(new Snippet(Game.ShowDate(), RLColor.Yellow, RLColor.Black)); }
                    displayList = inputList;
                    margin = inputMargin;
                    break;
                case ConsoleDisplay.Multi:
                    displayList = multiList;
                    margin = multiMargin;
                    dataLength = Game._multiConsoleLength;
                    break;
                case ConsoleDisplay.Status:
                    displayList = statusList;
                    margin = statusMargin;
                    break;
            }
            //max number of lines
            int maxLength = Math.Min(dataLength, displayList.Count);
            if (clearDisplay)
            { infoConsole.Clear(); }
            int lineCounter = 0;
            int listLength = displayList.Count;
            //keep _scrollIndex (set in Game.ScrollingKeyInput) within bounds
            if (consoleDisplay == ConsoleDisplay.Multi && Game._scrollIndex > 0) 
            {
                startIndex = Math.Min(Game._scrollIndex, displayList.Count - Game._multiConsoleLength / 2);
                startIndex = Math.Max(startIndex, 0);
                maxLength = Math.Min(dataLength + Game._scrollIndex, displayList.Count);
                //prevent _scrollIndex from escalating
                if (startIndex < Game._scrollIndex)
                {
                    Game._scrollIndex = startIndex - Game._multiConsoleLength / 2;
                    Game._scrollIndex = Math.Max(Game._scrollIndex, 0);
                }
            }
            //Display data
            int length = 0; //allows for sequential snippets on the same line
            for (int i = startIndex; i < maxLength; i++)
            {
                Snippet snippet = displayList[i];
                infoConsole.Print(margin + length, lineCounter * 2 + margin, snippet.GetText(), snippet.GetForeColor(), snippet.GetBackColor());
                //new line
                if (snippet.GetNewLine() == true)
                { lineCounter++; length = 0; }
                else
                { length += snippet.GetText().Length; }

            }
            //multi console interface texts at bottom
            if (consoleDisplay == ConsoleDisplay.Multi && lineCounter == dataLength)
            {
                Snippet blank = new Snippet("");
                infoConsole.Print(margin, lineCounter * 2 + margin, blank.GetText(), blank.GetForeColor(), blank.GetBackColor());
                lineCounter++;
                Snippet instructions = new Snippet("[PGDN] and [PGUP] to scroll, [ESC] to exit", RLColor.Magenta, RLColor.Black);
                infoConsole.Print(margin, lineCounter * 2 + margin, instructions.GetText(), instructions.GetForeColor(), instructions.GetBackColor());
                //set global var to trigger Scrolling mode
                Game._fullConsole = true;
            }
        }

        public void DrawSpecial(RLConsole multiConsole)
        {
            //text data
            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("This is the most amazing house I've lived in", RLColor.Black, RLColor.Black));
            listOfSnippets.Add(new Snippet());
            listOfSnippets.Add(new Snippet("Hey, I'm not so sure about that?", RLColor.Black, RLColor.Black));
            listOfSnippets.Add(new Snippet());
            listOfSnippets.Add(new Snippet("Yes I am!", RLColor.Black, RLColor.Black));
            listOfSnippets.Add(new Snippet());
            listOfSnippets.Add(new Snippet());
            listOfSnippets.Add(new Snippet("Choose [Y] Yes or {N] No", RLColor.Red, RLColor.Black));
            eventBox.SetText(listOfSnippets);
            //draw box
            eventBox.Draw(multiConsole);
        }
    }
}