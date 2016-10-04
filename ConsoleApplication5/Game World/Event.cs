using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// Base Event class
    /// </summary>
    class Event
    {
        private static int eventIndex = 1;
        public int EventID { get; }
        public string Name { get; set; }
        public int TempID { get; set; }


        public Event(int tempID, string name)
        {
            EventID = eventIndex++;
            this.TempID = tempID;
            this.Name = name;
            //debug
            Console.WriteLine("EventID {0], {1}, TempID {2}", EventID, Name, TempID);
        }
    }
}
