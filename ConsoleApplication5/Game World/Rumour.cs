using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum RumourScope { None, Local, Global }
    public enum RumourType { None, Terrain, Road, Skill, Secret, Item, Disguise, HouseRel,Friends, Desire } //Corresponds to Rumour subclasses, set in subclass
    public enum RumourGlobal { None, North, East, South, West, All }

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
        public Rumour(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true)
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
        public RumourSkill(string text, int strength, int actorID, SkillType skill, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
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
        public RumourSecret(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
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
        public RumourItem(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
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
        public RumourDisguise(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Disguise;
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
        public RumourHouseRel(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
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
        /// RumourItem constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="strength"></param>
        /// <param name="skill"></param>
        /// <param name="scope"></param>
        /// <param name="turnCreated">If '0' then defaults to current game turn</param>
        /// <param name="global"></param>
        /// <param name="isActive"></param>
        public RumourFriends(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Friends;
        }
    }

    /// <summary>
    /// Rumour about number of friends and enemies at a House (one rumour per house provided that there are at least some Friends and/or Enemies -> Global.Branch)
    /// </summary>
    class RumourDesire : Rumour
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
        public RumourDesire(string text, int strength, RumourScope scope, int turnCreated = 0, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, turnCreated, global, isActive)
        {
            Type = RumourType.Desire;
        }
    }

    //add new class instances above here
}
