﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum RumourScope { None, Local, Global }
    public enum RumourType { None, Terrain, Road, Skill }
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
        /// <param name="type"></param>
        /// <param name="topic"></param>
        /// <param name="global"></param>
        public Rumour(string text, int strength, RumourScope scope, RumourGlobal global = RumourGlobal.None, bool isActive = true)
        {
            RumourID = rumourIndex++;
            Text = text;
            if (strength > 0 && strength < 6) { this.Strength = strength; }
            else { Game.SetError(new Error(261, $"Invalid Rumour Strength (\"{strength}\") for \"{text}\" -> assigned default Strength of 3")); Strength = 3; }
            Scope = scope;
            Global = global;
            Active = isActive;
            TurnCreated = Game.gameTurn;
        }
    }

    /// <summary>
    /// Rumour about an NPC's skill
    /// </summary>
    class RumourSkill : Rumour
    {
        public int ActorID { get; set; } //who the rumour refers to
        public SkillType Skill { get; set; } //which skill the rumour refers to

        public RumourSkill(string text, int strength, int actorID, SkillType skill, RumourScope scope, RumourGlobal global = RumourGlobal.None, bool isActive = true) : base(text, strength, scope, global, isActive)
        {
            ActorID = actorID;
            Skill = skill;
            Type = RumourType.Skill;
        }

    }
    //add new class instances above here
}
