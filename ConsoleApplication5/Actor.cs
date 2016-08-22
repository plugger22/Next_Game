using Next_Game.Cartographic;
using System.Collections.Generic;

namespace Next_Game
{
    public enum ActorStatus {AtLocation, Travelling, Dead};
    public enum ActorTitle {None, Ursuper, Loyal_Follower, Lord, Lady, BannerLord, Prince, Princess, Heir, lord, lady }; //lord and lady are children of Lords, Heir is first in line to inherit
    public enum ActorOffice {None, King, Queen, Hand_of_the_King, Commander_of_Kings_Guard, Commander_of_City_Watch, Master_of_Coin, Master_of_Whisperers, Master_of_Laws,
                             Master_of_Ships, Warden_of_the_North, Warden_of_the_East, Warden_of_the_South, Warden_of_the_West }
    public enum ActorRealm {None, Head_of_Noble_House, Head_of_House}
    public enum ActorSex {Male, Female, Count};
    public enum ActorDied {None, Childbirth} //how died?
    public enum WifeStatus {None, First_Wife, Second_Wife, Third_Wife, Fourth_Wife, Fifth_Wife, Sixth_Wife, Seventh_Wife}
    public enum Relation {None, Wife, Husband, Son, Daughter, Bastard, Father, Mother, Brother, Sister, Half_Brother, Half_Sister}

    internal class Actor
    {
        private static int characterIndex = 1; //provides  a unique ID to each character (first characters should always be the player controlled ones with ID's < 10)
        private Position actorPos;
        
        public string Name { get; set; }
        public int LocID { get; set; } //current location (if travelling then destination)
        public int Speed { get; set; } = 2; //speed of travel throughout the world
        public int ActID { get; } //set in constructor
        public int Age { get; set; }
        public int Born { get; set; } //year born
        public int Died { get; set; } //year died
        public string Description { get; set; }
        public ActorStatus Status { get; set; } = 0;
        public ActorTitle Title { get; set; } = 0;
        public ActorOffice Office { get; set; } = 0; //official title, if any
        public ActorRealm Realm { get; set; } = 0; //local house title, if any
        public ActorSex Sex { get; set; }
        public ActorDied ReasonDied { get; set; } = 0;
        public string Handle { get; set; } = null; //eg. Nickname
        //stats 
        public int Combat { get; set; } = 3;
        public int Wits { get; set; } = 3;
        public int Charm { get; set; } = 3;
        public int Treachery { get; set; } = 3;
        public int Administration { get; set; } = 3;
        public int Leadership { get; set; } = 3;
        public int[] arrayOfTraitID { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public int[,] arrayOfTraitEffects { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public string[] arrayOfTraitNames { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1


        //default constructor 
        public Actor()
        {
            Name = null;
            Status = ActorStatus.AtLocation;
            actorPos = new Position();
            Description = "standard human";
            Age = 30;
            Title = ActorTitle.None;
            Sex = ActorSex.Male;
            arrayOfTraitID = new int[(int)TraitType.Count];
            arrayOfTraitEffects = new int[(int)TraitAge.Count, (int)TraitType.Count];
            arrayOfTraitNames = new string[(int)TraitType.Count];
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="name"></param>
        public Actor(string name, ActorTitle title, ActorSex sex = ActorSex.Male)
        {
            Status = ActorStatus.AtLocation;
            Name = name;
            actorPos = new Position();
            Description = "standard human";
            Age = 30;
            Title = title;
            Sex = sex;
            ActID = characterIndex++;
            arrayOfTraitID = new int[(int)TraitType.Count];
            arrayOfTraitEffects = new int[(int)TraitAge.Count, (int)TraitType.Count];
            arrayOfTraitNames = new string[(int)TraitType.Count];
        }

        public void SetActorPosition(Position posLoc)
        { actorPos = new Position(posLoc); }

        public Position GetActorPosition()
        { return actorPos; }

        /*public int GetActorID()
        { return ActID; }*/
    }


    //
    //Active actors - player controlled ---
    //

    class Active : Actor
    {
        public Active()
        { }

        public Active(string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base (name, title, sex)
        { }
    }

    //Player avatar
    class Player : Active
    {
        public Player(string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }

    //Player controlled Minions
    class Minion : Active
    {
        public Minion(string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }


    //
    //Passive actors - all NPC's ---
    //

    class Passive : Actor
    {
        public int RefID { get; set; } = 0; //house assignment, eg. Lannister (not HouseID).
        public int HouseID { get; set; } = 0; //dynamically assigned great house alignment 
        public int BornRefID { get; set; } = 0; //house born in (eg. wife married into another house), if 0 then ignore
        public int GenID { get; set; } = 1; //generation (1st, 2nd etc.
        public int InLine { get; set; } = 0; //number in line to inherit (males only)
        public int Married { get; set; } = 0; //year married, 0 if not
        public int Lordship { get; set; } = 0; //year made lord (Great House/Bannerlord)
        public bool Fertile { get; set; } = false; //females - can have children?
        public string MaidenName { get; set; } = null; //used to store a wife's maiden name prior to marriage
        public WifeStatus WifeNumber { get; set; } = WifeStatus.None;
        private SortedDictionary<int, Relation> dictFamily; //stores list of all relations (keyed off actorID)

        public Passive()
        {
            dictFamily = new SortedDictionary<int, Relation>();
        }

        public Passive(string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        {
            dictFamily = new SortedDictionary<int, Relation>();
        }

        /// <summary>
        /// add relative to sorted Dictionary of same
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="relation">enum Actor.cs</param>
        public void AddRelation(int actorID, Relation relation)
        {
            if (actorID > 0 && relation != Relation.None)
            { dictFamily.Add(actorID, relation); }
        }

        /// <summary>
        /// returns sorted dict of Family members (sorted by enum Actor.cs Relation order)
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<int, Relation> GetFamily()
        { return dictFamily; }
    }

    //Great House Family members
    class Family : Passive
    {
        public Family (string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }

    //BannerLords
    class BannerLord : Passive
    {
        public BannerLord (string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }

    //Kings Council Members
    class Advisor : Passive
    {
        public Advisor (string name, ActorTitle title, int locID = 1, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }

    //Special NPC's
    class Special : Passive
    {
        public Special (string name, ActorTitle title, ActorSex sex = ActorSex.Male) : base(name, title, sex)
        { }
    }
}