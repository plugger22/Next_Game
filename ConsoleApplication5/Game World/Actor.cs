using Next_Game.Cartographic;
using System;
using System.Collections.Generic;

namespace Next_Game
{
    public enum ActorStatus {AtLocation, Travelling, Gone}; //'Gone' encompasses dead and missing
    //lord and lady are children of Lords, Heir is first in line to inherit. Note: keep Knight immediately after Bannerlord with Advisor and Special behind Knight.
    public enum ActorType {None, Ursuper, Loyal_Follower, Lord, Lady, Prince, Princess, Heir, lord, lady, BannerLord, Knight, Advisor, Special }; 
    public enum ActorOffice {None, King, Queen, Lord_of_the_North, Lord_of_the_East, Lord_of_the_South, Lord_of_the_West }
    public enum ActorRealm {None, Head_of_Noble_House, Head_of_House, Regent}
    public enum AdvisorRoyal {None, Master_of_Coin, Master_of_Whisperers, Master_of_Laws, Master_of_Ships, High_Septon, Hand_of_the_King, Commander_of_Kings_Guard, Commander_of_City_Watch }
    public enum AdvisorNoble {None, Maester, Castellan, Septon}
    public enum ActorSex {None, Male, Female, Count};
    public enum ActorParents {Normal, Bastard, Adopted};
    public enum ActorGone {None, Missing, Childbirth, Battle, Executed, Murdered, Accident, Disease, Injuries} //how died (or gone missing)?
    public enum WifeStatus {None, First_Wife, Second_Wife, Third_Wife, Fourth_Wife, Fifth_Wife, Sixth_Wife, Seventh_Wife}
    public enum ActorRelation {None, Wife, Husband, Son, Daughter, Father, Mother, Brother, Sister, Half_Brother, Half_Sister}

    public class Actor
    {
        private static int characterIndex = 1; //provides  a unique ID to each character (first characters should always be the player controlled ones with ID's < 10)
        private Position actorPos;
        public string Name { get; set; }
        public int LocID { get; set; } //current location (if travelling then destination) -> if dead then '0'
        public int Speed { get; set; } = Game.constant.GetValue(Global.MOVE_SPEED); //speed of travel throughout the world
        public int ActID { get; set; } //set in constructor except in special circumstances (eg, copying actors over in Lore.cs)
        public int Age { get; set; }
        public int Born { get; set; } //year born
        public int Gone { get; set; } = 0; //year died or missing
        public string Description { get; set; }
        public int Delay { get; set; } = 0; // if > 0 then character delayed for some reason and unavailable
        public string DelayReason { get; set; }
        public ActorStatus Status { get; set; } = 0;
        public ActorType Type { get; set; } = 0;
        public ActorOffice Office { get; set; } = 0; //official title, if any
        public ActorRealm Realm { get; set; } = 0; //local house title, if any
        public ActorSex Sex { get; set; }
        public ActorParents Parents { get; set; } = 0; //normal family, bastard or adopted
        public ActorGone ReasonGone { get; set; } = 0; //reason gone or missing
        public KingLoyalty Loyalty_AtStart { get; set; } = KingLoyalty.None; //loyalty (prior to uprising)
        public KingLoyalty Loyalty_Current { get; set; } = KingLoyalty.None; //current loyalty
        public string Handle { get; set; } = null; //eg. Nickname
        //stats 
        public int Combat { get; set; } = 3;
        public int Wits { get; set; } = 3;
        public int Charm { get; set; } = 3;
        public int Treachery { get; set; } = 3;
        public int Leadership { get; set; } = 3;
        public int Touched { get; set; } = 0;
        public int Influencer { get; set; } = 0; //ActorID of person who is influencing traits (can only be one)
        public int[] arrayOfTraitID { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public int[,] arrayOfTraitEffects { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public string[] arrayOfTraitNames { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public int[] arrayOfTraitInfluences { get; set; } //effects due to person influencing (default 0)
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
            arrayOfTraitInfluences = new int[(int)TraitType.Count];
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
            arrayOfTraitInfluences = new int[(int)TraitType.Count];
            listOfSecrets = new List<int>();
        }

        public void SetActorPosition(Position posLoc)
        { actorPos = new Position(posLoc); }

        public Position GetActorPosition()
        { return actorPos; }

        /// <summary>
        /// checks whether particular trait has any influence effect, True if so.
        /// </summary>
        /// <param name="trait"></param>
        /// <returns></returns>
        public bool CheckTraitInfluenced(TraitType trait)
        {
            if (arrayOfTraitInfluences[(int)trait] != 0)
            { return true; }
            return false;
        }

        /// <summary>
        /// returns net value of a specified trait (with/without influence) Need to check if influence applies first
        /// </summary>
        /// <param name="trait"></param>
        /// <param name="influenceEffect">True if you want the influenced trait</param>
        /// <returns></returns>
        public int GetTrait(TraitType trait, TraitAge age = TraitAge.Fifteen, bool influenceEffect = false)
        {
            int traitValue = 3 + arrayOfTraitEffects[(int)age, (int)trait];;
            if (influenceEffect == true)
            { traitValue += arrayOfTraitInfluences[(int)trait]; }
            return traitValue;
        }

        /// <summary>
        /// returns string showing trait effect, eg. '(+2)' or '(-1)' or null if no effect
        /// </summary>
        /// <param name="age"></param>
        /// <param name="trait"></param>
        /// <param name="influenceEffect">True if you want the influenced trait</param>
        /// <returns></returns>
        public string GetTraitEffectText(TraitType trait, TraitAge age = TraitAge.Fifteen, bool influenceEffect = false)
        {
            string text = null;
            string plus = null;
            int effect = 0;
            if (influenceEffect == true)
            { effect = arrayOfTraitInfluences[(int)trait]; }
            else { effect = arrayOfTraitEffects[(int)age, (int)trait]; }
            if (effect > 0) { plus = "+"; }
            else if (effect == 0) { return text; }
            text = "(" + plus + Convert.ToString(effect) + ")";
            return text;
        }

        public void AddSecret(int secretID)
        {
            if (secretID > 0)
            { listOfSecrets.Add(secretID); }
            else
            { Game.SetError(new Error(7, "invalid Secret ID")); }
        }

        public List<int> GetSecrets()
        { return listOfSecrets; }

        public void SetSecrets(List<int> secrets)
        { if (secrets != null) { listOfSecrets.Clear(); listOfSecrets.AddRange(secrets); } }
    }


    //
    //Active actors - player controlled ---
    //

    public class Active : Actor
    {
        public int ArcID { get; set; } //archetype ID
        public int CrowChance { get; set; } //chance of crow getting through
        public int CrowDistance { get; set; } //distance between player and follower
        public int CrowBonus { get; set; } //carry over bonus to CrowChance from previous turn
        public int Resources { get; set; } //abstracted money, equipment and influence
        public bool Activated { get; set; } //can move/be given orders this turn, or not
        private List<string> crowTooltip { get; set; } //explanation of factors influencing crow chance

        public Active()
        { crowTooltip = new List<string>(); }

        public Active(string name, ActorType type, ActorSex sex = ActorSex.Male) : base (name, type, sex)
        { crowTooltip = new List<string>(); }

        public void AddCrowTooltip(string tooltip)
        { if (tooltip != null) { crowTooltip.Add(tooltip); } }

        public List<string> GetCrowTooltips()
        { return crowTooltip; }

        public void ClearCrowTooltips()
        { crowTooltip.Clear(); }
    }

    //Player avatar
    public class Player : Active
    {
        public int CrowsNumber { get; set; }

        public Player(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { Activated = true; }
    }

    //Player controlled Minions
    public class Follower : Active
    {
        public int FollowerID { get; set; } //FID
        public string Role { get; set; } //one or two word description of who they represent, eg. Beggar, Assasssin, etc.
        public int Loyalty_Player { get; set; } //loyalty to the player (1 to 5 stars)

        public Follower(string name, ActorType type, int followerID, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        {
            Activated = false;
            FollowerID = followerID;
        }
    }


    //
    //Passive actors - all NPC's ---
    //

    public class Passive : Actor
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
    public class Noble : Passive
    {
        public int GenID { get; set; } = Game.gameGeneration; //generation (1st, 2nd etc.
        public int InLine { get; set; } = 0; //number in line to inherit (males only)
        public int Married { get; set; } = 0; //year married, 0 if not
        public int Lordship { get; set; } = 0; //year made lord (Great House)
        public bool Fertile { get; set; } = false; //females - can have children?
        public string MaidenName { get; set; } = null; //used to store a wife's maiden name prior to marriage
        public WifeStatus WifeNumber { get; set; } = WifeStatus.None;
        private SortedDictionary<int, ActorRelation> dictFamily; //stores list of all relations (keyed off actorID)

        public Noble()
        { dictFamily = new SortedDictionary<int, ActorRelation>(); }

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
    public class BannerLord : Passive
    {
        public int Lordship { get; set; } = 0; //year made lord (Bannerlord)

        public BannerLord (string name, ActorType type = ActorType.BannerLord, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }


    //Knights - attached to a noble house
    public class Knight : Passive
    {
        public int Knighthood { get; set; } //year knighted

        public Knight(string name, ActorType type = ActorType.Knight, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }


    //Advisors - either Royal (King's Council) or Noble (Great House). Both can have Religious advisors.
    public class Advisor : Passive
    {
        public AdvisorRoyal advisorRoyal { get; set; } = AdvisorRoyal.None;
        public AdvisorNoble advisorNoble { get; set; } = AdvisorNoble.None;
        public int CommenceService { get; set; } //year commenced service with the Great House

        public Advisor (string name, ActorType type = ActorType.Advisor, int locID = 1, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }

    //Special NPC's
    public class Special : Passive
    {
        public Special (string name, ActorType type = ActorType.Special, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }
}