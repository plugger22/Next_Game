using Next_Game.Cartographic;

namespace Next_Game
{
    public enum CharStatus {Location, Travelling};

    internal class Character
    {
        private static int characterIndex = 1; //provides  a unique ID to each character (first characters should always be the player controlled ones with ID's < 10)
        private Position charPos;
        private string charName;
        private CharStatus charStatus;
        public int CharLocationID { get; set; } //current location (if travelling then destination)
        public int CharSpeed { get; set; } = 2; //speed of travel throughout the world
        public int charID; //Can only have a max of 9 characters (including player) due to map draw limitations (mapMarker based on lowest Party ID)

        //default constructor 
        public Character()
        {
            charName = "Ser_Nobody";
            charStatus = CharStatus.Location;
            charPos = new Position();
            charID = characterIndex++;
        }

        //main use constructor
        public Character(string name)
        {
            charStatus = CharStatus.Location;
            charName = name;
            this.charID = characterIndex++;
            charPos = new Position();
        }

        public void SetCharacterPosition(Position posLoc)
        { charPos = new Position(posLoc); }

        public void SetCharacterStatus(CharStatus status)
        { charStatus = status; }

        public Position GetCharacterPosition()
        { return charPos; }

        public string GetCharacterName()
        { return charName; }     

        public int GetCharacterStatus()
        { return (int) charStatus; }     

        public int GetCharacterID()
        { return charID; }
    }
}