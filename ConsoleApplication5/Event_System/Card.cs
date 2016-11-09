using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game.Event_System
{

    public enum CardCategory { None, Conflict, Item, Secret, Companion, Disguise }
    public enum ConflictType { None, Skill, Trait, Situation, Companion, Item, Supporter }
    public enum CardType { None, Good, Bad, Neutral}

    /// <summary>
    /// Base class
    /// </summary>
    class Card
    {
        public CardType Type { get; set; }
        public CardCategory Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public Card(CardType type, string title, string description)
        {
            this.Type = type;
            this.Title = title;
            this.Description = description;
        }
    }

    /// <summary>
    /// Conflict Cards
    /// </summary>
    class CardConflict : Card
    {
        public int Effect { get; set; } //eg. 1X, 2X, etc.
        public string Special { get; set; }
        public ConflictType Conflict_Type { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CardConflict(ConflictType conflictType, CardType type, string title, string description, int effect = 1) : base (type, title, description)
        {
            Category = CardCategory.Conflict;
            Conflict_Type = conflictType;
            this.Effect = effect;
        }

    }


}
