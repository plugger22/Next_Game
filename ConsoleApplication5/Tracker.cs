using System;

namespace Next_Game
{
    //event categories
    public enum TrackCat
    { Player_Movement }
        
    /// <summary>
    /// Used for tracking game events for Characters. Objects stored in List in World object
    /// </summary>
    internal class Tracker
    {
        int turn; //turn object occured
        TrackCat trackCat; //category
        int charID; //character ID
        string eventText;
      
        //main constructor
        public Tracker(int turn, TrackCat category, int charID, string eventText)
        {
            this.turn = turn;
            trackCat = category;
            this.charID = charID;
            this.eventText = eventText;
        }

        //display Tracker Object
        public void ShowEvent()
        { Console.WriteLine("T{0,-4} {1,-25} ID {2,-3} {3}", Convert.ToString(turn), trackCat, charID, eventText); }
    }

}