using System;
using System.Collections.Generic;
using RLNET;

namespace Next_Game
{

    public enum BoxType {None, Dynamic, Card} //dynamic is for events and it's a dynamically sizing box in the vertical dimension

    //Handles display of information to the multi / input / status / spare Consoles
    public class InfoChannel
    {
        int multiMargin; //margin offset from left of page and top for info display on console
        int inputMargin;
        int statusMargin;
        int messageMargin;
        int boxWidth;
        private List<Snippet> multiList; //list of strings to display in the multi Console
        private List<Snippet> inputList; //list of strings to display in the input Console
        private List<Snippet> statusList; //list of strings to display in the Status Console
        private List<Snippet> messageList; //list of strings to display in the Message Console
        private List<Snippet> eventList; //list of strings to display in an Event box
        //special multiConsole display objects
        private Box dynamicBox; //automatically adjusts it's height to text -> box initialised at time of drawing because of this
        private Box cardBox; //standard card


        public InfoChannel()
        {
            multiList = new List<Snippet>();
            inputList = new List<Snippet>();
            statusList = new List<Snippet>();
            messageList = new List<Snippet>();
            eventList = new List<Snippet>();
            multiMargin = 2;
            inputMargin = 2;
            statusMargin = 2;
            messageMargin = 2;
            boxWidth = 100;
            RLColor backColor = Color._background1;
            //dynamicBox = new Box(boxWidth, 10, 10, backColor, RLColor.Black);
            cardBox = new Box(45, 45, 10, backColor, RLColor.Black);
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
                case ConsoleDisplay.Message:
                    messageList.Clear();
                    messageList.AddRange(listToDisplay);
                    break;
                case ConsoleDisplay.Event:
                    eventList.Clear();
                    //add blank line to start
                    eventList.Add(new Snippet(""));
                    //add snippets individually, word wrapping when needed
                    for (int i = 0; i < listToDisplay.Count; i++)
                    {
                        Snippet tempSnippet = listToDisplay[i];
                        string tempString = tempSnippet.GetText();
                        int maxWidth = boxWidth - 4;
                        //length O.K
                        if (tempString.Length < maxWidth)
                        { eventList.Add(tempSnippet); }
                        else
                        {
                            //overlength -> word wrap
                            List<string> tempList = Game.utility.WordWrap(tempString, maxWidth);
                            string text;
                            for (int k = 0; k < tempList.Count; k++)
                            {
                                text = tempList[k];
                                eventList.Add(new Snippet(text, tempSnippet.GetForeColor(), tempSnippet.GetBackColor()));
                                //spacer line if required
                                if ( k < tempList.Count - 1)
                                { eventList.Add(new Snippet("")); }
                            }
                        }
                    }
                    //add blank line to end
                    eventList.Add(new Snippet(""));
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
                case ConsoleDisplay.Message:
                    messageList.Add(appendSnippet);
                    break;
                case ConsoleDisplay.Event:
                    eventList.Add(appendSnippet);

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
                case ConsoleDisplay.Message:
                    messageList.Clear();
                    break;
                case ConsoleDisplay.Event:
                    eventList.Clear();
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
                case ConsoleDisplay.Message:
                    messageList.Insert(0, snippet);
                    break;
                case ConsoleDisplay.Event:
                    eventList.Insert(0, snippet);
                    break;
            }
        }

        /// <summary>
        /// Main Draw console function
        /// </summary>
        /// <param name="infoConsole"></param>
        /// <param name="consoleDisplay"></param>
        /// <param name="clearDisplay"></param>
        /// <param name="mode">Specify here if it's a multiConsole special mode requiring other than a straight text display</param>
        public void DrawInfoConsole(RLConsole infoConsole, ConsoleDisplay consoleDisplay, SpecialMode mode = SpecialMode.None, bool clearDisplay = true)
        {
            if (mode > 0)
            { DrawSpecial(infoConsole, mode); }
            else
            {
                //Straight text display
                List<Snippet> displayList = new List<Snippet>();
                int margin = 2;
                int startIndex = 0;
                int dataLength = 10; //max lines of data allowed in console
                switch (consoleDisplay)
                {
                    case ConsoleDisplay.Input:
                        //default game state display if nothing present
                        if (inputList.Count == 0)
                        {
                            Game.world.ShowGameStateRL();
                            //inputList.Add(new Snippet(Game.utility.ShowDate(), RLColor.Yellow, RLColor.Black)); 
                        }
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
                    case ConsoleDisplay.Message:
                        displayList = messageList;
                        margin = messageMargin;
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
        }

        /// <summary>
        /// change background color of a box
        /// </summary>
        /// <param name="color"></param>
        /// <param name="type"></param>
        public void ChangeBoxColor(RLColor color, BoxType type)
        {
            try
            {
                switch (type)
                {
                    case BoxType.Dynamic:
                        dynamicBox?.SetBackColor(color);
                        break;
                    case BoxType.Card:
                        cardBox?.SetBackColor(color);
                        break;
                }
            }
            catch (Exception e)
            { Game.SetError(new Error(30, e.Message)); }
        }


        /// <summary>
        /// Draw special MultiConsole mode
        /// </summary>
        /// <param name="multiConsole"></param>
        /// <param name="mode"></param>
        private void DrawSpecial(RLConsole multiConsole, SpecialMode mode )
        {
            //test data
                    List<Snippet> listOfSnippets = new List<Snippet>();
                    listOfSnippets.Add(new Snippet("This is the most amazing house I've lived in", RLColor.Black, RLColor.Black));
                    listOfSnippets.Add(new Snippet());
                    listOfSnippets.Add(new Snippet("Hey, I'm not so sure about that?", RLColor.Black, RLColor.Black));
                    listOfSnippets.Add(new Snippet());
                    listOfSnippets.Add(new Snippet("Yes I am!", RLColor.Black, RLColor.Black));
                    listOfSnippets.Add(new Snippet());
                    listOfSnippets.Add(new Snippet());
                    listOfSnippets.Add(new Snippet("Choose [F1] Event or [F2] Color", RLColor.Red, RLColor.Black));

            switch (mode)
            {
                case SpecialMode.PlayerEvent:
                case SpecialMode.FollowerEvent:
                case SpecialMode.Outcome:
                    //ignore if nothing to display
                    if (eventList.Count > 0)
                    {
                        dynamicBox = new Box(boxWidth, eventList.Count +2, 10, Color._background1, RLColor.Black);
                        dynamicBox.SetText(eventList);
                        //draw box
                        dynamicBox.Draw(multiConsole);
                    }
                    break;
                case SpecialMode.Conflict:
                    switch (Game._conflictMode)
                    {
                        case ConflictMode.Strategy:
                            Game.layout.DrawStrategy(multiConsole);
                            break;
                        case ConflictMode.Cards:
                            Game.layout.DrawCards(multiConsole);
                            break;
                        case ConflictMode.Confirm:
                            Game.layout.DrawConfirm(multiConsole, Game.layout.GetTestDataConfirm());
                            break;
                        case ConflictMode.AutoResolve:
                            Game.layout.DrawAutoResolve(multiConsole, Game.layout.GetTestDataAutoResolve());
                            break;
                        case ConflictMode.Outcome:
                            Game.layout.DrawOutcome(multiConsole);
                            break;
                    }
                                       
                    break;
            }
        }
    }
}