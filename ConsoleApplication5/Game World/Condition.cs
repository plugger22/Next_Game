using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// all characters (active and passive) can have Conditions that affect their skills. Conditions are dynamic whereas Skills are fixed.
    /// </summary>
    class Condition
    {
        public SkillType Skill { get; set; } //eg. Strength, Charm, etc.
        public int Effect { get; set; } //+/- 1 or 2 max.
        public string Text { get; set; } //descriptor
        public int Timer { get; set; } //'999' indicates a permanent condition. Any other value > 0 will decrement each turn and the condition will be removed once at 0

        /// <summary>
        /// Standard constructor with a default timer of 999 indicating a permanent condition.
        /// </summary>
        /// <param name="skill">can't be SkillType.None</param>
        /// <param name="effect"></param>
        /// <param name="text"></param>
        /// <param name="timer"></param>
        public Condition(SkillType skill, int effect, string text, int timer = 999)
        {
            if (skill > SkillType.None)
            { this.Skill = skill; }
            else { Game.SetError(new Error(128, "Invalid Skill input (SkillType None)")); Skill = SkillType.None; }
            if (effect != 0 && Math.Abs(effect) < 3)
            { this.Effect = effect; }
            else { Game.SetError(new Error(128, "Invalid Effect input (value zero or > 2)")); Effect = 0; }
            if (String.IsNullOrEmpty(text) == false)
            { this.Text = text; }
            else { Game.SetError(new Error(128, "Invalid Text input (Null or Empty string)")); Text = "Unknown"; }
            if (timer > 0 && timer < 1000)
            { this.Timer = timer; }
            else { Game.SetError(new Error(128, "Invalid Timer input (value zero, or less OR value > 999)")); Timer = 999; }
        }
    }
}
