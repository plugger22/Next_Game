using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    ///NOTE -> CURRENTLY NOT USED

    /// <summary>
    /// Card Conflict Situations
    /// </summary>
    class Situation
    {
        public string Description { get; set; }
        public int Effect { get; set; } //eg. 1X, 2X
        public int Level { get; set; } //How many cards (random weighted determination)
        public CardType TypeAttack { get; set; } //how situation is treated if defender chooses an Attack strategy
        public CardType TypeBalanced { get; set; } //how situation is treated if defender chooses a balanced strategy
        public CardType TypeDefend { get; set; } //how situation is treated if defender chooses a defend strategy

        //default constructor
        public Situation(string description, CardType attack, CardType balanced, CardType defend, int level = 0)
        {
            this.Description = description;
            TypeAttack = attack;
            TypeBalanced = balanced;
            TypeDefend = defend;
            //default effect of 1X
            Effect = 1;
            this.Level = level;
        }

    }
}
