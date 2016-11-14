using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{
    /// <summary>
    /// Card Conflict Situations
    /// </summary>
    class Situation
    {
        public string Description { get; set; }
        public int Effect { get; set; } //eg. 1X, 2X
        public int Level { get; set; } //How many cards (random weighted determination)
        public CardType TypeAttack { get; set; } //how situation is treated if player chooses an Attack strategy
        public CardType TypeBalanced { get; set; } //how situation is treated if player chooses a balanced strategy
        public CardType TypeDefend { get; set; } //how situation is treated if player chooses a defend strategy

        //default constructor
        public Situation(string description, CardType attack, CardType balanced, CardType defend, int level = 0)
        {
            this.Description = description;
            TypeAttack = attack;
            TypeBalanced = balanced;
            TypeDefend = defend;
            //default effect of 1X
            Effect = 1;
            if (level == 0)
            {
                //randomly determine effect (70% - 1, 16% - 2, 8% - 3, 4% - 4, 2% - 5)
               
            }
            else { this.Level = level; }
        }

    }
}
