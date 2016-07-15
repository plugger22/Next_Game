using System.Collections.Generic;
using System.Linq;
using System;
using RLNET;

namespace Next_Game
{
    public class MessageLog
    {
        private readonly Queue<string> messageQueue; //short term queue to display recent messages
        private readonly List<string> messageList; //long term list of all messages
        int margin;

        public MessageLog()
        {
            margin = 2;
            messageQueue = new Queue<string>();
            messageList = new List<string>();
            messageList.Add("Message Log Full ---");
        }

        public void Add(string message, int turn)
        {
            //add a turn time stamp
            message = Convert.ToString(turn + 1200) + " " + message;
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
            string[] messageArray = messageQueue.ToArray();
            consoleDisplay.Print(margin, 1, "Message Log Recent ---", RLColor.White);
            for (int i = 0; i < messageArray.Count(); i++)
            { consoleDisplay.Print(margin, (i + 1) * 2 + 1, messageArray[i], RLColor.White); }
        }

        public List<string> GetMessageList()
        { return messageList; }

    }
}