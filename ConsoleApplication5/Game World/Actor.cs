
using Next_Game.Cartographic;
using System;
using System.Collections.Generic;
using Next_Game.Event_System;

namespace Next_Game
{
    public enum ActorStatus {AtLocation, Travelling, Gone}; //'Gone' encompasses dead and missing
    //lord and lady are children of Lords, Heir is first in line to inherit. Note: keep Knight immediately after Bannerlord with Advisor and Special behind Knight.
    public enum ActorType {None, Usurper, Follower, Lord, Lady, Prince, Princess, Heir, lord, lady, BannerLord, Knight, Advisor, Special, Beast }; 
    public enum ActorOffice {None, Usurper, King, Queen, Lord_of_the_North, Lord_of_the_East, Lord_of_the_South, Lord_of_the_West }
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
        public int Resources { get; set; } //abstracted money, equipment and influence
        private int relPlyr; //relationship with Player (0 to 100), higher the better
        public string DelayReason { get; set; }
        public string Title { get; set; } //text description of whatever relevant title they have. Automatically set by constructors. Used for display purposes.
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
        public int[] arrayOfSkillID { get; set; } //array index corresponds to skill type in Skill.cs SkillType enum, eg. Combat = 1
        public int[,] arrayOfTraitEffects { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public string[] arrayOfTraitNames { get; set; } //array index corresponds to trait type in Trait.cs TraitType enum, eg. Combat = 1
        public int[] arrayOfSkillInfluences { get; set; } //effects due to person influencing (default 0)
        public int[] arrayOfConditions { get; set; } //net effect of any conditions
        //lists
        private List<int> listOfSecrets; //secrets have a PossID which can be referenced in the dictPossessions (world.cs)
        private List<int> listOfFollowerEvents;
        private List<int> listOfPlayerEvents;
        private List<Relation> listRelOther; //list of relation messages relating to all actors other than the Player
        private List<Relation> listRelPlyr; //list of relation messages relating to the Player
        private List<Condition> listConditions; //list of all active conditions affecting the Player


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
            relPlyr = 50; //nuetral
            arrayOfSkillID = new int[(int)SkillType.Count];
            arrayOfTraitEffects = new int[(int)SkillAge.Count, (int)SkillType.Count];
            arrayOfTraitNames = new string[(int)SkillType.Count];
            arrayOfSkillInfluences = new int[(int)SkillType.Count];
            arrayOfConditions = new int[(int)SkillType.Count];
            listOfSecrets = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listRelOther = new List<Relation>();
            listRelPlyr = new List<Relation>();
            listConditions = new List<Condition>();
            //set title but only if not already set by lower level constructor
            if (String.IsNullOrEmpty(Title) == true) { Title = string.Format("{0}", Type); }
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
            relPlyr = 50; //nuetral
            ActID = characterIndex++;
            arrayOfSkillID = new int[(int)SkillType.Count];
            arrayOfTraitEffects = new int[(int)SkillAge.Count, (int)SkillType.Count];
            arrayOfTraitNames = new string[(int)SkillType.Count];
            arrayOfSkillInfluences = new int[(int)SkillType.Count];
            arrayOfConditions = new int[(int)SkillType.Count];
            listOfSecrets = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listRelOther = new List<Relation>();
            listRelPlyr = new List<Relation>();
            listConditions = new List<Condition>();
            //set title but only if not already set by lower level constructor
            if (String.IsNullOrEmpty(Title) == true) { Title = string.Format("{0}", Type); }
        }

        public void SetActorPosition(Position posLoc)
        { actorPos = new Position(posLoc); }

        public Position GetActorPosition()
        { return actorPos; }

        internal void SetEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfFollowerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(65, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetFollowerEvents()
        { return listOfFollowerEvents; }

        internal int GetNumFollowerEvents()
        { return listOfFollowerEvents.Count; }

        internal List<int> GetPlayerEvents()
        { return listOfPlayerEvents; }

        internal int GetNumPlayerEvents()
        { return listOfPlayerEvents.Count; }

        //needed for sub classes (world.cs -> ShowActorRL)
        internal SortedDictionary<int, ActorRelation> GetFamily()
        { return null; }

        public void AddRelEventOther(Relation relMsg)
        { listRelOther.Add(relMsg); }

        /// <summary>
        /// adds event & updates relPlyr & figures out new value (Level) after changes
        /// </summary>
        /// <param name="relMsg"></param>
        public void AddRelEventPlyr(Relation relMsg)
        {
            ChangeRelPlyr(relMsg.Change);
            relMsg.Level = relPlyr;
            listRelPlyr.Add(relMsg);
        }

        public List<Relation> GetRelEventPlyr()
        { return listRelPlyr; }

        /// <summary>
        /// returns Tag of most recent Player event (used for ShowActorRL display in World.cs)
        /// </summary>
        /// <returns></returns>
        internal string GetPlayerTag()
        {
            if (listRelPlyr.Count > 0)
            {
                Relation relation = listRelPlyr[listRelPlyr.Count - 1];
                return relation.Tag;
            }
            else { return ""; }
        }

        /// <summary>
        /// returns Change amount of last Relationship effect (eg. the Tag)
        /// </summary>
        /// <returns></returns>
        internal int GetPlayerChange()
        {
            if (listRelPlyr.Count > 0)
            {
                Relation relation = listRelPlyr[listRelPlyr.Count - 1];
                return relation.Change;
            }
            else { return 0; }
        }

        public int GetRelPlyr()
        { return relPlyr; }

        /// <summary>
        /// set actor's relationship with player at start
        /// </summary>
        /// <param name="value"></param>
        public void SetRelPlyr(int value)
        { relPlyr = value; relPlyr = Math.Min(100, relPlyr); relPlyr = Math.Max(0, relPlyr); }

        /// <summary>
        /// updates actor's relationship with the Player 
        /// </summary>
        /// <param name="change"></param>
        private void ChangeRelPlyr(int change)
        {
            relPlyr += change;
            relPlyr = Math.Min(100, relPlyr);
            relPlyr = Math.Max(0, relPlyr);
        }

        /// <summary>
        /// checks whether particular skill has any influence effect, True if so.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool CheckSkillInfluenced(SkillType skill)
        {
            if (arrayOfSkillInfluences[(int)skill] != 0)
            { return true; }
            return false;
        }

        /// <summary>
        /// returns net value of a specified skill (with/without influence) Need to check if influence applies first
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="influenceEffect">True if you want the influenced skill</param>
        /// <returns></returns>
        public int GetSkill(SkillType skill, SkillAge age = SkillAge.Fifteen, bool influenceEffect = false)
        {
            int skillValue = 3 + arrayOfTraitEffects[(int)age, (int)skill];;
            if (influenceEffect == true)
            { skillValue += arrayOfSkillInfluences[(int)skill]; }
            return skillValue;
        }

        /// <summary>
        /// returns string showing trait effect, eg. '(+2)' or '(-1)' or null if no effect
        /// </summary>
        /// <param name="age"></param>
        /// <param name="skill"></param>
        /// <param name="influenceEffect">True if you want the influenced trait</param>
        /// <returns></returns>
        public string GetTraitEffectText(SkillType skill, SkillAge age = SkillAge.Fifteen, bool influenceEffect = false)
        {
            string text = null;
            string plus = null;
            int effect = 0;
            if (influenceEffect == true)
            { effect = arrayOfSkillInfluences[(int)skill]; }
            else { effect = arrayOfTraitEffects[(int)age, (int)skill]; }
            if (effect > 0) { plus = "+"; }
            else if (effect == 0) { return text; }
            text = "(" + plus + Convert.ToString(effect) + ")";
            return text;
        }

        /// <summary>
        /// returns straight trait text, eg. "Obnoxious"
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public string GetTraitName(SkillType skill)
        { return arrayOfTraitNames[(int)skill]; }

        /// <summary>
        /// returns relationship to Player as a integer between 1 and 5 (stars) by breaking down RelPlyr into chunks of 20
        /// </summary>
        /// <returns></returns>
        public int GetRelPlyrStars()
        {
            int rel = relPlyr;
            rel /= 20;
            rel += 1;
            rel = Math.Min(5, rel);
            return rel;
        }

        /// <summary>
        /// Secrets
        /// </summary>
        /// <param name="possID"></param>
        public void AddSecret(int possID)
        {
            if (possID > 0)
            { listOfSecrets.Add(possID); }
            else
            { Game.SetError(new Error(7, "invalid Possession (Secret) ID")); }
        }

        public List<int> GetSecrets()
        { return listOfSecrets; }

        public void SetSecrets(List<int> secrets)
        { if (secrets != null) { listOfSecrets.Clear(); listOfSecrets.AddRange(secrets); } }

        /// <summary>
        /// Conditions
        /// </summary>
        /// <param name="condition"></param>
        internal void AddCondition(Condition condition)
        {
            if (condition != null)
            {
                if (condition.Skill != SkillType.None)
                {
                    listConditions.Add(condition);
                    //add to array
                    arrayOfConditions[(int)condition.Skill] += condition.Effect;
                    Console.WriteLine("SYSTEM: {0} {1}, ID {2}, arrayOfConditions[{3}] was {4} now {5}", Title, Name, ActID, 
                        condition.Skill, arrayOfConditions[(int)condition.Skill] + condition.Effect, arrayOfConditions[(int)condition.Skill]);
                    //record event
                    string timerText = string.Format("{0}", condition.Timer == 999 ? "permanent effect" : string.Format("lasts for {0} days", condition.Timer));
                    string conditionText = string.Format("\"{0}\" condition acquired, {1} {2}{3}, {4}", condition.Text, condition.Skill, condition.Effect > 0 ? "+" : "", 
                        condition.Effect, timerText);
                    Record record = new Record(conditionText, ActID, LocID, 0, Game.gameYear, HistActorIncident.Condition);
                    Game.world.SetRecord(record);
                }
                else { Game.SetError(new Error(129, "Invalid Condition Input (Skill is SkillType.None)")); }
            }
            else { Game.SetError(new Error(129, "Invalid Condition Input (null)")); }
        }

       internal List<Condition> GetConditions()
        { return listConditions; }

        /// <summary>
        /// returns true if conditions present, false otherwise
        /// </summary>
        /// <returns></returns>
        internal bool CheckConditions()
        {
            if (listConditions.Count > 0) { return true; }
            return false;
        }

        /// <summary>
        /// polls all conditions, decrements any timers (< 999) and removes condition if timer reaches 0
        /// </summary>
        internal void UpdateConditionTimers()
        {
            //foreach(var condition in listConditions)
            //reverse loop through listConditions (may have to delete some conditions if timer = 0)
            for (int i = listConditions.Count - 1; i >= 0; i--)
            {
                Condition condition = listConditions[i];
                if (condition.Timer > 0 && condition.Timer < 999)
                {
                    condition.Timer--;
                    Console.WriteLine("{0} {1}, ID {2}, condition timer reduced from {3} to {4} (\"{5}\")", Title, Name, ActID, condition.Timer + 1, condition.Timer, condition.Text);
                    //timer = 0, remove condition
                    if (condition.Timer == 0)
                    {
                        //record event
                        string conditionText = string.Format("\"{0}\", {1} {2}{3}, condition removed ", condition.Text, condition.Skill, 
                            condition.Effect > 0 ? "+" : "", condition.Effect);
                        Record record = new Record(conditionText, ActID, LocID, 0, Game.gameYear, HistActorIncident.Condition);
                        Game.world.SetRecord(record);
                        //update array
                        arrayOfConditions[(int)condition.Skill] -= condition.Effect;
                        Console.WriteLine("SYSTEM: {0} {1}, ID {2}, arrayOfConditions[{3}] was {4} now {5}", Title, Name, ActID, condition.Skill, 
                            arrayOfConditions[(int)condition.Skill] + condition.Effect, arrayOfConditions[(int)condition.Skill]);
                        //remove condition
                        listConditions.RemoveAt(i);
                    }

                }
            }

        }

        /// <summary>
        /// Adjust Actor's Resource value up or down
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="calc">Can be Add / Subtract / Equals</param>
        public string ChangeResources(int amount, EventCalc calc)
        {
            string messageText = "";
            switch (calc)
            {
                case EventCalc.Add:
                    Resources += amount;
                    messageText = string.Format("{0} {1}'s Resources increased by {2} to {3} (max. 5)", Title, Name, amount, Resources);
                    break;
                case EventCalc.Subtract:
                    Resources -= amount;
                    messageText = string.Format("{0} {1}'s Resources reduced by {2} to {3} (min. 1)", Title, Name, amount, Resources);
                    break;
                case EventCalc.Equals:
                    Resources = amount;
                    messageText = string.Format("{0} {1}'s Resources changed from {2} to {3} (min. 0, max. 5)", Title, Name, amount, Resources);
                    break;
                default:
                    Game.SetError(new Error(127, string.Format("Invalid Calc (\"{0}\") in Actor.ChangeResources", calc)));
                    break;
            }
            if (String.IsNullOrEmpty(messageText) == false)
            {
                Message message = new Message(messageText, MessageType.Event);
                Game.world.SetMessage(message);
            }
            //keep within paramters
            Resources = Math.Min(5, Resources);
            Resources = Math.Max(1, Resources);
            return messageText;
        }
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
        public bool Activated { get; set; } //can move/be given orders this turn, or not
        private List<string> crowTooltip { get; set; } //explanation of factors influencing crow chance
        

        public Active()
        {
            crowTooltip = new List<string>();
            Title = string.Format("{0}", Type);
        }

        public Active(string name, ActorType type, ActorSex sex = ActorSex.Male) : base (name, type, sex)
        {
            crowTooltip = new List<string>();
            Title = string.Format("{0}", Type);
        }

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
        public int HistoryID { get; set; } //actorID of character who becomes the usurper
        private SortedDictionary<int, ActorRelation> dictFamily; //stores list of all relations (keyed off actorID)
        private List<int> listOfFavours; //stores possessionId of favours granted to Player
        private List<int> listOfIntroductions; //stores possessionId of introductions granted to the Player

        public Player(string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        {
            Activated = true;
            Title = string.Format("{0}", Type);
            listOfFavours = new List<int>();
            listOfIntroductions = new List<int>();
        }

        public void SetFamily (SortedDictionary<int, ActorRelation> dictFamily)
        {
            if (dictFamily != null)
            { this.dictFamily = dictFamily; }
        }

        public new SortedDictionary<int, ActorRelation> GetFamily()
        { return dictFamily; }

        public void AddFavour(int possID)
        { if (possID > 0) { listOfFavours.Add(possID); } }

        public List<int> GetFavours()
        { return listOfFavours; }

        public void AddIntroduction(int possID)
        { if (possID > 0) { listOfIntroductions.Add(possID); } }

        public List<int> GetIntroductions()
        { return listOfIntroductions; }
    }

    //Player controlled Minions
    public class Follower : Active
    {
        public int FollowerID { get; set; } //FID
        public string Role { get; set; } //one or two word description of who they represent, eg. Beggar, Assasssin, etc.
        //public int Loyalty_Player { get; set; } //loyalty to the player (1 to 5 stars) NOTE: superceded by RelPlyr instead

        public Follower(string name, ActorType type, int followerID, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        {
            Activated = false;
            FollowerID = followerID;
            Title = string.Format("{0}", Type);
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
        { dictFamily = new SortedDictionary<int, ActorRelation>(); Title = string.Format("{0}", Type); }

        public Noble (string name, ActorType type, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { dictFamily = new SortedDictionary<int, ActorRelation>(); Title = string.Format("{0}", Type); }

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
        public new SortedDictionary<int, ActorRelation> GetFamily()
        { return dictFamily; }
    }


    //BannerLords
    public class BannerLord : Passive
    {
        public int Lordship { get; set; } = 0; //year made lord (Bannerlord)

        public BannerLord (string name, ActorType type = ActorType.BannerLord, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { Title = string.Format("{0}", Type); }
    }


    //Knights - attached to a noble house
    public class Knight : Passive
    {
        public int Knighthood { get; set; } //year knighted

        public Knight(string name, ActorType type = ActorType.Knight, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { Title = string.Format("{0}", Type); }
    }


    //Advisors - either Royal (King's Council) or Noble (Great House). Both can have Religious advisors.
    public class Advisor : Passive
    {
        public AdvisorRoyal advisorRoyal { get; set; } = AdvisorRoyal.None;
        public AdvisorNoble advisorNoble { get; set; } = AdvisorNoble.None;
        public int CommenceService { get; set; } //year commenced service with the Great House

        public Advisor (string name, ActorType type = ActorType.Advisor, int locID = 1, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { } //Title set in history.CreateAdvisor
    }

    //Special NPC's
    public class Special : Passive
    {
        public Special (string name, ActorType type = ActorType.Special, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }


    //Beasts
    public class Beast : Actor
    {

        public Beast(string name, ActorType type = ActorType.Beast, ActorSex sex = ActorSex.Male) : base(name, type, sex)
        { }
    }
}