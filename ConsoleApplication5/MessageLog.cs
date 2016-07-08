using System.Collections.Generic;
using System.Linq;
using System;
using RLNET;

namespace Next_Game
{
    public class MessageLog
    {
        private readonly Queue<string> messageQueue;
        int margin;

        public MessageLog()
        {
            margin = 2;
            messageQueue = new Queue<string>();
        }

        public void Add(string message, int turn)
        {
            //add a turn time stamp
            message = Convert.ToString(turn + 1200) + " " + message;
            messageQueue.Enqueue(message);
            //max 8 entries in queue at any one time
            if( messageQueue.Count > 8 )
            { messageQueue.Dequeue(); }
        }

        public void DrawMessageLog( RLConsole consoleDisplay)
        {
            consoleDisplay.Clear();
            string[] messageArray = messageQueue.ToArray();
            consoleDisplay.Print(margin, 1, "Message Log ---", RLColor.White);
            for(int i = 0; i < messageArray.Count(); i++)
            { consoleDisplay.Print(margin, (i + 1) * 2 + 1, messageArray[i], RLColor.White); }
        }
    }
}