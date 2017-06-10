using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum RumourScope { None, Local, Global }
    public enum RumourType { None, Terrain, Road, Skill, Secret, Item, Disguise, HouseRel, Friends, Desire, Enemy, Relationship, Event, SafeHouse, Goods, HouseHistory } //Corresponds to Rumour subclasses, set in subclass
    public enum RumourGlobal { All, North, East, South, West }
    public enum RumourStatus { Normal, Timed, Inactive} //Normal -> dictRumoursNormal, Timed (TimerExpire > 0) -> dictRumoursTimed, Inactive (TimerStart > 0) -> dictRumoursInactive
    public enum RumourDisplay { All, Enemies} //used by Game.ShowRumoursRL to filter the required rumour set

    /// <summary>
    /// handles all rumours, eg. 'Ask around for information'
    /// </summary>
    class Rumour
    {
        private static int rumourIndex = 1; //provides a unique ID to each rumour
        public int RumourID { get; }
        public string Text { get; set; }
        public int Strength { get; set; } //strength of rumour -> 1 to 5 (highest)
        public bool Active { get; set; } //rumour only used if Active is true
        public int RefID { get; set; } //Optional -> location specific rumour, default '0'
        public int TurnCreated { get; set; } //Turn when rumour became active (used to show age of rumour)
        public int TurnRevealed { get; set; } //Turn when rumour was revealed, eg. became known
        public int TimerStart { get; set; } //Turns before rumour becomes active, default '0'
        public int TimerExpire { get; set; } //Turns before rumour becomes inactive (expires), default '0'
        public int WhoHeard { get; set; } //ActorID of character (player or follower) who first heard the rumour
        public RumourScope Scope { get; set; }
        public RumourType Type { get; set; } //Corresponds to Rumour subclasses, set in subclass
        public RumourGlobal Global { get; set; } //Optional -> only applies if Type is RumourType.Global
        public RumourStatus Status { get; set; } //Optional -> Default normal, automatically set by 'Director.cs -> AddRumour'


        public Rumour()
        { }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then equals current game turn</param>
        /// <param name="global"></param>
        public Rumour(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true)
        {
            RumourID = rumourIndex++;
            Text = text;
            if (strength > 0 && strength < 6) { this.Strength = strength; }
            else { Game.SetError(new Error(261, $"Invalid Rumour Strength (\"{strength}\") for \"{text}\" -> assigned default Strength of 3")); Strength = 3; }
            Scope = scope;
            Global = global;
            Active = isActive;
            //if '0' then defaults to current game turn
            if (turnCreated == 0) { TurnCreated = Game.gameTurn; }
            else
            {
                if (turnCreated > Game.gameTurn) { TurnCreated = Game.gameTurn; }
                else { TurnCreated = turnCreated; }
            }
            //Status -> default 
            Status = RumourStatus.Normal;
        }
    }

    /// <summary>
    /// Rumour about an NPC's skill -> Local
    /// </summary>
    class RumourSkill : Rumour
    {
        public int ActorID { get; set; } //who the rumour refers to
        public SkillType Skill { get; set; } //which skill the rumour refers to

        /// <summary>
        /// Skill constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="actorID"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourSkill(string text, int strength, int actorID, SkillType skill, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            ActorID = actorID;
            Skill = skill;
            Type = RumourType.Skill;
        }

    }

    /// <summary>
    /// Rumour about an NPC's secret (one rumour regardless of how many secrets they have -> Local)
    /// </summary>
    class RumourSecret : Rumour
    {

        /// <summary>
        /// RumourSecret constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourSecret(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Secret;
        }
    }


    /// <summary>
    /// Rumour about an NPC's item (one rumour per item -> Global.All)
    /// </summary>
    class RumourItem : Rumour
    {

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourItem(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Item;
        }
    }

    /// <summary>
    /// Rumour about an NPC's who have disguises (one rumour per NPC -> Global.All)
    /// </summary>
    class RumourDisguise : Rumour
    {

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourDisguise(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Disguise;
        }
    }

    /// <summary>
    /// Rumour about a Safe House being present in a location that supports the New King (those of the old King are assumed known at game start)
    /// </summary>
    class RumourSafeHouse : Rumour
    {
        //RefID holds house RefID so that ResolveRumour can set SafeHouse to known for that house

        public RumourSafeHouse(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.SafeHouse;
        }
    }

    /// <summary>
    /// Rumour about House Relationships (one rumour per relationship -> Global.Branch)
    /// </summary>
    class RumourHouseRel : Rumour
    {
        public int TrackerID { get; set; } //tracker ID of relationship (needed to find house rel & set to Known)
        //NOTE: Rumour.RefID is houseFrom.RefID
        public int HouseToRefID { get; set; } //refID of house the rumour refers to 

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourHouseRel(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.HouseRel;
        }
    }

    /// <summary>
    /// Rumour about number of friends and enemies at a House (one rumour per house provided that there are at least some Friends and/or Enemies -> Global.Branch)
    /// </summary>
    class RumourFriends : Rumour
    {

        /// <summary>
        /// Rumour Friends constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourFriends(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Friends;
        }
    }

    /// <summary>
    /// Rumour about a House's History -> Local)
    /// </summary>
    class RumourHouseHistory : Rumour
    {

        /// <summary>
        /// Rumour HouseHistory constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourHouseHistory(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.HouseHistory;
        }
    }


    /// <summary>
    /// Rumour about a House's import/export goods (one rumour for each different good that the house deals in) -> Local
    /// </summary>
    class RumourGoods : Rumour
    {
        public Goods Good { get; set; } //type of Good
        
        /// <summary>
        /// Rumour Goods constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="good"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated"></param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourGoods(string text, int strength, Goods good, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            this.Good = good;
            Type = RumourType.Goods;
        }
    }

    /// <summary>
    /// Rumour about number of friends and enemies at a House (one rumour per house provided that there are at least some Friends and/or Enemies -> Global.Branch)
    /// </summary>
    class RumourDesire : Rumour
    {
        public int Data { get; set; } //multipurpose data, could be actor, refID or possessionID

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="data">Multipurpose data point (eg. actorID, RefID, PossID) depending on actor.Desire -> PossPromiseType)</param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourDesire(string text, int strength, RumourScope scope, int data, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Data = data;
            Type = RumourType.Desire;
        }
    }


    //
    // Timed Rumours ---
    //

    /// <summary>
    /// Timed rumour (TimerExpire > 0) about location & activity of an Unknown enemy
    /// </summary>
    class RumourEnemy : Rumour
    {

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="TimerExpire">Should be > 0 (# of turns until rumour expires)</param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourEnemy(string text, int strength, RumourScope scope, int timerExpire, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            if (timerExpire <= 0)
            { Game.SetError(new Error(278, $"Invalid timerExpire \"{timerExpire}\" for RumourEnemy -> changed to default value of 3")); timerExpire = 3; }
            this.TimerExpire = timerExpire;
            Type = RumourType.Enemy;
        }
    }


    /// <summary>
    /// Timed rumour (TimerExpire > 0). Reveals NPC's relationship status with Lord and Player (changes Actor.RelKnown to true) -> Local (House) scope
    /// </summary>
    class RumourRelationship : Rumour
    {
        public int ActorID { get; set; } //who the rumour refers to

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="TimerExpire">Should be > 0 (# of turns until rumour expires)</param>
        /// <param name="actorID">actorID of the NPC being referred to</param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourRelationship(string text, int strength, RumourScope scope, int timerExpire, int actorID, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            if (timerExpire <= 0)
            { Game.SetError(new Error(285, $"Invalid timerExpire \"{timerExpire}\" for RumourRel-> changed to default value of 3")); timerExpire = 3; }
            this.TimerExpire = timerExpire;
            this.ActorID = actorID;
            Type = RumourType.Relationship;
        }
    }

    /// <summary>
    /// Event rumour generated by data in Events when the first become Active (single shot)
    /// </summary>
    class RumourEvent : Rumour
    {

        /// <summary>
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="TimerExpire">Should be > 0 (# of turns until rumour expires)</param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourEvent(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.All, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Event;
        }
    }

    //add new class instances above here
}
