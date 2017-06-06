using System;
using System.Collections.Generic;
using Next_Game.Cartographic;

namespace Next_Game
{

    /// <summary>
    /// static class to hold all general utility methods
    /// </summary>
    public class Utility
    {

        /// <summary>
        /// utility function
        /// </summary>
        /// <returns></returns>
        public string ShowDate()
        {
            string dateReturn = "Unknown";
            int moonDay = (Game.gameTurn % 30) + 1;
            int moonCycle = (Game.gameTurn / 30) + 1;
            string moonSuffix = "th";
            if (moonCycle == 1)
            { moonSuffix = "st"; }
            else if (moonCycle == 2)
            { moonSuffix = "nd"; }
            else if (moonCycle == 3)
            { moonSuffix = "rd"; }
            string harvestText = string.Format("Harvest in {0} day{1}", Game.HarvestTimer, Game.HarvestTimer != 1 ? "s" : "");
            string winterText = string.Format("Winter in {0} day{1}", Game.WinterTimer, Game.WinterTimer != 1 ? "s" : "");
            dateReturn = string.Format("Day {0} of the {1}{2} Moon in the Year of our Gods {3}  (Turn {4}) {5} {6}", moonDay, moonCycle, moonSuffix, 
                Game.gameYear, Game.gameTurn + 1, Game.HarvestTimer > 0 ? harvestText : "", Game.WinterTimer > 0 ? winterText : "");
            return dateReturn;
        }

        /// <summary>
        /// word wrap a long sentence
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxCharacters"></param>
        /// <returns></returns>
        public List<string> WordWrap(string input, int maxCharacters)
        {
            List<string> lines = new List<string>();
            if (String.IsNullOrEmpty(input) == false)
            {
                
                if (!input.Contains(" "))
                {
                    int start = 0;
                    while (start < input.Length)
                    {
                        lines.Add(input.Substring(start, Math.Min(maxCharacters, input.Length - start)));
                        start += maxCharacters;
                    }
                }
                else
                {
                    string[] words = input.Split(' ');
                    string line = "";
                    foreach (string word in words)
                    {
                        if ((line + word).Length > maxCharacters)
                        {
                            lines.Add(line.Trim());
                            line = "";
                        }
                        line += string.Format("{0} ", word);
                    }
                    if (line.Length > 0)
                    { lines.Add(line.Trim()); }
                }
            }
            else { Game.SetError(new Error(195, "Invalid string input (null or empty) -> WordWrap Cancelled")); }
            return lines;
        }

        /// <summary>
        /// Gives 2D distance between two coordinates
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public int GetDistance(int x1, int y1, int x2, int y2)
        {
            double dX = x1 - x2;
            double dY = y1 - y2;
            double multi = dX * dX + dY * dY;
            double distance = Math.Sqrt(multi);
            return (int)distance;
        }

        /// <summary>
        /// Checks a string for actor related text tags and swaps them over for correct texts (conflict card texts)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public string CheckTagsActor(string text, Actor opponent)
        {
            string checkedText = text;
            if (String.IsNullOrEmpty(text) == false)
            {

                string tag, replaceText;
                int tagStart, tagFinish, length; //indexes
                if (opponent != null)
                {
                    Player player = Game.world.GetPlayer();
                    if (player != null)
                    {
                        //loop whilever tags are present
                        while (checkedText.Contains("<") == true)
                        {
                            tagStart = checkedText.IndexOf("<");
                            tagFinish = checkedText.IndexOf(">");
                            length = tagFinish - tagStart;
                            tag = checkedText.Substring(tagStart + 1, length - 1);
                            //strip brackets
                            replaceText = null;
                            switch (tag)
                            {
                                case "men":
                                    replaceText = string.Format("{0} {1}'s Men-At-Arms", opponent.Title, opponent.Name);
                                    break;
                                case "name":
                                    replaceText = string.Format("{0} {1}", opponent.Title, opponent.Name);
                                    break;
                                case "beast":
                                    replaceText = $"{opponent.Name}";
                                    break;
                                case "him":
                                    replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "him" : "her");
                                    break;
                                case "she":
                                case "he":
                                    replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "he" : "she");
                                    break;
                                case "She":
                                case "He":
                                    replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "He" : "She");
                                    break;
                                case "his":
                                    replaceText = string.Format("{0}", opponent.Sex == ActorSex.Male ? "his" : "her");
                                    break;
                                case "geocluster":
                                    Position pos = player.GetActorPosition();
                                    if (pos != null)
                                    {
                                        int data = Game.map.GetMapInfo(MapLayer.GeoID, pos.PosX, pos.PosY);
                                        if (data > 0)
                                        {
                                            GeoCluster geocluster = Game.world.GetGeoCluster(data);
                                            if (geocluster != null)
                                            { replaceText = geocluster.Name; }
                                            else { Game.SetError(new Error(283, "Invalid geocluster (null) for forest/mountain/sea")); }
                                        }
                                    }
                                    else { Game.SetError(new Error(283, "Invalid position (null) for forest/mountain/sea")); }
                                    break;
                                default:
                                    replaceText = "";
                                    Game.SetError(new Error(101, string.Format("Invalid tag (\"{0}\")", tag)));
                                    break;
                            }
                            if (replaceText != null)
                            {
                                //swap tag for text
                                checkedText = checkedText.Remove(tagStart, length + 1);
                                checkedText = checkedText.Insert(tagStart, replaceText);
                            }
                        }
                    }
                    else { Game.SetError(new Error(101, "Invalid Player (null)")); }
                }
                else { Game.SetError(new Error(101, "Invalid opponent (null) in CheckTagsActor")); }
            }
            else
            { Game.SetError(new Error(101, "Invalid Input (null or empty)")); }
               
            return checkedText;
        }

        /// <summary>
        /// selection of tags used by AutoReact events
        /// </summary>
        /// <param name="text"></param>
        /// <param name="refID"></param>
        /// <returns></returns>
        internal string CheckTagsAuto(string text, string swap_1 = null, Actor opponent = null, GeoCluster cluster = null )
        {
            string checkedText = text;
            if (String.IsNullOrEmpty(text) == false)
            {

                string tag, replaceText;
                int tagStart, tagFinish, length; //indexes

                //loop whilever tags are present
                while (checkedText.Contains("<") == true)
                {
                    tagStart = checkedText.IndexOf("<");
                    tagFinish = checkedText.IndexOf(">");
                    length = tagFinish - tagStart;
                    tag = checkedText.Substring(tagStart + 1, length - 1);
                    //strip brackets
                    replaceText = null;
                    switch (tag)
                    {
                        case "person":
                            if (opponent != null)
                            { replaceText = string.Format("{0} {1}", opponent.Title, opponent.Name);}
                            else { Game.SetError(new Error(124, "Invalid opponent (null) in CheckTagsAuto")); }
                            break;
                        case "terrain":
                            if (cluster != null) { replaceText = cluster.Name; }
                            else { Game.SetError(new Error(124, "Invalid cluster (null) in CheckTagsAuto")); }
                            break;
                        case "text_1":
                            if (String.IsNullOrEmpty(swap_1) == false) { replaceText = swap_1; }
                            else { Game.SetError(new Error(124, "Invalid Text_1 (null or empty) in CheckTagsAuto")); }
                            break;
                        default:
                            replaceText = "";
                            Game.SetError(new Error(124, string.Format("Invalid tag (\"{0}\") in CheckTagsAuto", tag)));
                            break;
                    }
                    if (replaceText != null)
                    {
                        //swap tag for text
                        checkedText = checkedText.Remove(tagStart, length + 1);
                        checkedText = checkedText.Insert(tagStart, replaceText);
                    }
                }
            }
            else
            { Game.SetError(new Error(101, "Invalid Input (null or empty)")); }

            return checkedText;
        }

        /// <summary>
        /// selection of tags used for the 'View from the Market' (Director.cs -> GetMarketView)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public string CheckTagsView(string text, Player player)
        {
            string checkedText = text;
            if (String.IsNullOrEmpty(text) == false)
            {

                string tag, replaceText;
                int tagStart, tagFinish, length, data; //indexes
                if (player != null)
                {
                    //loop whilever tags are present
                    while (checkedText.Contains("<") == true)
                    {
                        tagStart = checkedText.IndexOf("<");
                        tagFinish = checkedText.IndexOf(">");
                        length = tagFinish - tagStart;
                        tag = checkedText.Substring(tagStart + 1, length - 1);
                        //strip brackets
                        replaceText = null;
                        switch (tag)
                        {
                            case "player":
                                replaceText = player.Name;
                                break;
                            case "newKing":
                                replaceText = Game.lore.NewKing.Name;
                                break;
                            case "oldKing":
                                replaceText = Game.lore.OldKing.Name;
                                break;
                            case "playerHandle":
                                replaceText = player.Handle;
                                break;
                            case "newKingHandle":
                                replaceText = Game.lore.NewKing.Handle;
                                break;
                            case "oldKingHandle":
                                replaceText = Game.lore.OldKing.Handle;
                                break;
                            case "him":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "him" : "her");
                                break;
                            case "he":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "he" : "she");
                                break;
                            case "He":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "He" : "She");
                                break;
                            case "his":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "his" : "her");
                                break;
                            case "His":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "His" : "Her");
                                break;
                            case "man":
                                replaceText = string.Format("{0}", player.Sex == ActorSex.Male ? "man" : "woman");
                                break;
                            case "locRecent":
                                data = Game.director.GetRecentlyVisited();
                                if (data <= 0)
                                {
                                    //gets a random locID as no viable one found above
                                    data = Game.network.GetRandomLocation();
                                    Game.logTurn?.Write("[Alert -> LocRecent] No viable locID returned from director.GetRecentlyVisited -> default Network.GetRandomLocation");
                                }
                                replaceText = Game.world.GetLocationName(data);
                                break;
                            case "locRandom":
                                data = Game.director.GetRandomVisited();
                                if (data <= 0)
                                {
                                    //gets a random locID as no viable one found above
                                    data = Game.network.GetRandomLocation();
                                    Game.logTurn?.Write("[Alert -> LocRandom] No viable locID returned from director.GetRandomVisited -> default Network.GetRandomLocation");
                                }
                                replaceText = Game.world.GetLocationName(data);
                                break;
                            case "roadTo":
                            case "locName":
                                data = player.LocID;
                                replaceText = Game.world.GetLocationName(data);
                                break;
                            case "court":
                                data = Game.director.GetRandomCourt();
                                if (data <= 0)
                                {
                                    //get a random court name as no viable name found normally
                                    replaceText = Game.world.GetRandomMajorHouseName();
                                    Game.logTurn?.Write("[Alert -> Court] No viable House name returned from Director.GetRandomCourt -> default World.GetRandomMajorHouseName");
                                }
                                else { replaceText = Game.world.GetHouseName(data); }
                                break;
                            case "randomMale":
                                replaceText = Game.history.GetFirstName(ActorSex.Male);
                                break;
                            case "randomFemale":
                                replaceText = Game.history.GetFirstName(ActorSex.Female);
                                break;
                            case "geocluster":
                                Position pos = player.GetActorPosition();
                                if (pos != null)
                                {
                                    data = Game.map.GetMapInfo(MapLayer.GeoID, pos.PosX, pos.PosY);
                                    GeoCluster geocluster = Game.world.GetGeoCluster(data);
                                    if (geocluster != null)
                                    { replaceText = geocluster.Name; }
                                    else { Game.SetError(new Error(283, "Invalid geocluster (null) for forest/mountain")); }
                                }
                                else { Game.SetError(new Error(283, "Invalid position (null) for forest/mountain")); }
                                break;
                            case "sea":
                                replaceText = player.SeaName;
                                break;
                            case "ship":
                                replaceText = player.ShipName;
                                break;
                            case "curse":
                                replaceText = Game.director.GetAssortedRandom(Assorted.Curse);
                                break;
                            case "animalBig":
                                replaceText = Game.director.GetAssortedRandom(Assorted.AnimalBig);
                                break;
                            default:
                                replaceText = "";
                                Game.SetError(new Error(283, string.Format("Invalid tag (\"{0}\")", tag)));
                                break;
                        }
                        if (replaceText != null)
                        {
                            //swap tag for text
                            checkedText = checkedText.Remove(tagStart, length + 1);
                            checkedText = checkedText.Insert(tagStart, replaceText);
                        }
                    }
                }
                else { Game.SetError(new Error(283, "Invalid player (null) in CheckTagsView")); }
            }
            else
            { Game.SetError(new Error(283, "Invalid Input (null or empty)")); }

            return checkedText;
        }

        /// <summary>
        /// selection of tags used for Rumours (events), Returns null in special circumstances ('name' tag and no descriptor provided)
        /// </summary>
        /// <param name="text">Rumour text that may contain tags</param>
        /// <param name="descriptor">Multipurpose descriptor, could be Forest name, locName, etc. </param>
        /// <returns></returns>
        public string CheckTagsRumour(string text, string descriptor = "")
        {
            string checkedText = text;
            bool abortFlag = false;
            if (String.IsNullOrEmpty(text) == false)
            {
                string tag, replaceText;
                int tagStart, tagFinish, length; //indexes

                //loop while ever tags are present
                while (checkedText.Contains("<") == true)
                {
                    tagStart = checkedText.IndexOf("<");
                    tagFinish = checkedText.IndexOf(">");
                    length = tagFinish - tagStart;
                    tag = checkedText.Substring(tagStart + 1, length - 1);

                    //strip brackets
                    replaceText = null;
                    switch (tag)
                    {
                        case "newKing":
                            replaceText = Game.lore.NewKing.Name;
                            break;
                        case "oldKing":
                            replaceText = Game.lore.OldKing.Name;
                            break;
                        case "newKingHandle":
                            replaceText = Game.lore.NewKing.Handle;
                            break;
                        case "oldKingHandle":
                            replaceText = Game.lore.OldKing.Handle;
                            break;
                        case "name":
                            //multipurpose, Initialise Archetypes determines this
                            if (String.IsNullOrEmpty(descriptor) == false)
                            { replaceText = descriptor; }
                            else
                            {
                                replaceText = "";
                                abortFlag = true;
                                Game.logTurn?.Write($"[Alert -> CheckTagsRumour] <name> has no descriptor, Rumour \"{text}\" -> cancelled"); }
                            break;
                        default:
                            replaceText = "";
                            Game.SetError(new Error(283, string.Format("Invalid tag (\"{0}\")", tag)));
                            break;
                    }
                    if (replaceText != null)
                    {
                        //swap tag for text
                        checkedText = checkedText.Remove(tagStart, length + 1);
                        checkedText = checkedText.Insert(tagStart, replaceText);
                    }
                }
            }
            else
            { Game.SetError(new Error(292, "Invalid Input (null or empty)")); }
            //check for special case of a <name> tag and no descriptor provided
            if (abortFlag == true)
            { checkedText = null; }
            return checkedText;
        }

        /// <summary>
        /// Capitalises the first letter of a word, eg. cat -> Cat
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string Capitalise(string s)
        {
            if (String.IsNullOrEmpty(s))
            { return s; }
            if (Char.IsUpper(s[1]) == true)
            { return s; }
            else
            {
                if (s.Length == 1)
                { return s.ToUpper(); }
                return s.Remove(1).ToUpper() + s.Substring(1);
            }
        }

        //methods above here
    }
}
