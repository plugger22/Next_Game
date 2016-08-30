using Next_Game.Cartographic;
using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum ActorStatus {AtLocation, Travelling, Dead};
    //lord and lady are children of Lords, Heir is first in line to inherit. Note: keep Knight immediately after Bannerlord with Advisor and Special behind Knight.
    public enum ActorType {None, Ursuper, Loyal_Follower, Lord, Lady, Prince, Princess, Heir, lord, lady, BannerLord, Knight, Advisor, Special }; 
    public enum ActorOffice {None, King, Queen, Hand_of_the_King, Commander_of_Kings_Guard, Commander_of_City_Watch, Lord_of_the_North, Lord_of_the_East, Lord_of_the_South, Lord_of_the_West }
    public enum ActorRealm {None, Head_of_Noble_House, Head_of_House}
    public enum AdvisorRoyal {None, Master_of_Coin, Master_of_Whisperers, Master_of_Laws, Master_of_Ships}
    public enum AdvisorNoble {None, Maester, Castellan}
    public enum AdvisorReligious {None, Septon, Priest, Priestess}
    public enum ActorSex {None, Male, Female, Count};
    public enum ActorParents {Normal, Bastard, Adopted};
    public enum ActorDied {None, Childbirth} //how died?
    public enum WifeStatus {None, First_Wife, Second_Wife, Third_Wife, Fourth_Wife, Fifth_Wife, Sixth_Wife, Seventh_Wife}
    public enum ActorRelation {None, Wife, Husband, Son, Daughter, Father, Mother, Brother, Sister, Half_Brother, Half_Sister}

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
        public ActorType Type { get; set; } = 0;
        public ActorOffice Office { get; set; } = 0; //official title, if any
        public ActorRealm Realm { get; set; } = 0; //local house title, if any
        public ActorSex Sex { get; set; }
        public ActorParents Parents { get; set; } = 0; //normal family, bastard or adopted
        public ActorDied ReasonDied { get; set; } = 0;
        public string Handle { get; set; } = null; //eg. Nickname
        //stats 
        public int Combat { get; set; } = 3;
        public int Wits { get; set; } = 3;
        public int Charm { get; set; } = 3;
        public int Treachery { get; set; } = 3;
        public int Leadership { get; set; } = 3;
        public int[] arrayOfTraitID { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public int[,] arrayOfTraitEffects { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public string[] arrayOfTraitNames { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        //secrets
        private List<int> listOfSecrets;


        //default constructor 
        public Actor()
        {
            Name = null;
            Status = ActorStatus.AtLocation;
            actorPos = new Position();
            Description = "standard human";
            Age = 30;
            Type = ActorType.None;
            Sex = ActorSex.Male;
            arrayOfTraitID = new int[(int)TraitType.Count];
            arrayOfTraitEffects = new int[(int)TraitAge.Count, (int)TraitType.Count];
            arrayOfTraitNames = new string[(int)TraitType.Count];
            listOfSecrets = new List<int>();
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="name"></param>
        public Actor(string name, ActorType type, ActorSex sex = ActorSex.Male)
        {
            Status = ActorStatus.AtLocation;
            this.Name = name;
            actorPos = new Position();
            Description = "standard human";
            Age = 30;
            this.Type = type;
            this.Sex = sex;
            ActID = characterIndex++;
            arrayOfTraitID = new int[(int)TraitType.Count];
            arrayOfTraitEffects = new int[(int)TraitAge.Count, (int)TraitType.Count];
            arrayOfTraitNames = new string[(int)TraitType.Count];
            listOfSecrets = new List<int>();
        }

        public void SetActorPosition(Position posLoc)
        { actorPos = new Position(posLoc); }

        public Position GetActorPosition()
        { return actorPos; }

        public void AddSecret(int secretID)
        {
            if (secretID > 0)
            { listOfSecrets.Add(secretID); }
            else
            { Game.SetError(new Error(7, "invalid Secret ID")); }
        }

        public List<int> GetSecrets()
        { return listOfSecrets; }

    }


    //
    //Active actors - player controlled ---
    //

    class Active : Actor
    {
        public Active()
        { }

        public Active(string name, ActorType type, ActorSex sex = ActorSex.Male) : base (name, type, sex)
        { }
    }

    //Player avatar
    class Player : Active
    {
        public Player(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }

    //Player controlled Minions
    class Follower : Active
    {
        public Follower(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
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

        public Passive()
        { }

        public Passive(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }

        /*public Passive(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        {
            dictFamily = new SortedDictionary<int, ActorRelation>();
        }*/
    }


    //Great House Family members
    class Noble : Passive
    {
        public int GenID { get; set; } = Game.gameGeneration; //generation (1st, 2nd etc.
        public int InLine { get; set; } = 0; //number in line to inherit (males only)
        public int Married { get; set; } = 0; //year married, 0 if not
        public int Lordship { get; set; } = 0; //year made lord (Great House)
        public bool Fertile { get; set; } = false; //females - can have children?
        public string MaidenName { get; set; } = null; //used to store a wife's maiden name prior to marriage
        public WifeStatus WifeNumber { get; set; } = WifeStatus.None;
        private SortedDictionary<int, ActorRelation> dictFamily; //stores list of all relations (keyed off actorID)

        public Noble (string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { dictFamily = new SortedDictionary<int, ActorRelation>(); }

        /// <summary>
        /// add relative to sorted Dictionary of same
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="relation">enum Actor.cs</param>
        public void AddRelation(int actorID, ActorRelation relation)
        {
            if (actorID > 0 && relation != ActorRelation.None)
            { dictFamily.Add(actorID, relation); }
        }

        /// <summary>
        /// returns sorted dict of Family members (sorted by enum Actor.cs Relation order)
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<int, ActorRelation> GetFamily()
        { return dictFamily; }
    }


    //BannerLords
    class BannerLord : Passive
    {
        public int Lordship { get; set; } = 0; //year made lord (Bannerlord)

        public BannerLord (string name, ActorType type = ActorType.BannerLord, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }


    //Knights - attached to a noble house
    class Knight : Passive
    {
        public int Knighthood { get; set; } //year knighted

        public Knight(string name, ActorType type = ActorType.Knight, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }


    //Advisors - either Royal (King's Council) or Noble (Great House). Both can have Religious advisors.
    class Advisor : Passive
    {
        public AdvisorRoyal advisorRoyal { get; set; } = AdvisorRoyal.None;
        public AdvisorNoble advisorNoble { get; set; } = AdvisorNoble.None;
        public AdvisorReligious advisorReligious { get; set; } = AdvisorReligious.None;

        public Advisor (string name, ActorType type = ActorType.Advisor, int locID = 1, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }

    //Special NPC's
    class Special : Passive
    {
        public Special (string name, ActorType type = ActorType.Special, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }
}