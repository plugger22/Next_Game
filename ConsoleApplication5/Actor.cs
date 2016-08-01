using Next_Game.Cartographic;

namespace Next_Game
{
    public enum ActorStatus {AtLocation, Travelling};

    internal class Actor
    {
        private static int characterIndex = 1; //provides  a unique ID to each character (first characters should always be the player controlled ones with ID's < 10)
        private Position actorPos;
        private ActorStatus status;
        public string Name { get; set; }
        public int LocID { get; set; } //current location (if travelling then destination)
        public int Speed { get; set; } = 2; //speed of travel throughout the world
        public int ActID; //Can only have a max of 9 characters (including player) due to map draw limitations (mapMarker based on lowest Party ID)

        //default constructor 
        public Actor()
        {
            Name = "Ser_Nobody";
            status = ActorStatus.AtLocation;
            actorPos = new Position();
            ActID = characterIndex++;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="name"></param>
        public Actor(string name)
        {
            status = ActorStatus.AtLocation;
            Name = name;
            this.ActID = characterIndex++;
            actorPos = new Position();
        }

        public void SetActorPosition(Position posLoc)
        { actorPos = new Position(posLoc); }

        public void SetActorStatus(ActorStatus status)
        { this.status = status; }

        public Position GetActorPosition()
        { return actorPos; }

        public int GetActorStatus()
        { return (int) status; }     

        public int GetActorID()
        { return ActID; }
    }
}