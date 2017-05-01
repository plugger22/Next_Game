using System.Collections.Generic;
using System.Linq;
using System;
using RLNET;

namespace Next_Game
{
    /*public class MessageLog
    {
        private readonly Queue<Snippet> messageQueue; //short term queue to display recent messages
        private readonly List<Snippet> messageList; //long term list of all messages
        int margin;

        public MessageLog()
        {
            margin = 2;
            messageQueue = new Queue<Snippet>();
            messageList = new List<Snippet>();
            //messageList.Add(new Snippet("--- Message Log Full"));
        }

        /// <summary>
        /// add Message log, use default turn 0 to auto get Game.gameTurn
        /// </summary>
        /// <param name="message"></param>
        /// <param name="turn"></param>
        public void Add(Snippet message, int turn = 0)
        {
            if (turn == 0) { turn = Game.gameTurn + 1; }
            //add a turn time stamp
            string snippetText = message.GetText();
            snippetText = "Day " + turn + " " + snippetText;
            message.SetText(snippetText);
            messageQueue.Enqueue(message);
            //max 8 entries in queue at any one time
            if (messageQueue.Count > 8)
            { messageQueue.Dequeue(); }
            //add to list
            messageList.Add(message);
        }

        public void DrawMessageQueue(RLConsole consoleDisplay)
        {
            consoleDisplay.Clear();
            Snippet[] messageArray = messageQueue.ToArray();
            consoleDisplay.Print(margin, 1, "--- Message Log Recent", RLColor.Yellow, RLColor.Black);
            int lineCounter = 0;
            for (int i = 0; i < messageArray.Count(); i++)
            {
                Snippet snippet = messageArray[i];
                consoleDisplay.Print(margin, (lineCounter + 1) * 2 + 1, snippet.GetText(), snippet.GetForeColor(), snippet.GetBackColor());
                //new line, or an existing line
                if (snippet.GetNewLine() == true)
                { lineCounter++; }
            }
        }

        public List<Snippet> GetMessageList()
        { return messageList; }

    }*/
}