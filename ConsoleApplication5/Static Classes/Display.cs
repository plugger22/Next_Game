using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using System.Linq;
using RLNET;


namespace Next_Game
{
    /// <summary>
    /// handles all display methods (separates output code from world.cs)
    /// </summary>
    public class Display
    {

        public Display()
        { }

        /// <summary>
        /// places a message in info panel detailing all relevant data for a single generation
        /// </summary>
        public void ShowGeneratorStatsRL()
        {
            List<Snippet> listStats = new List<Snippet>();
            RLColor houseColor;
            Dictionary<int, MajorHouse> dictMajorHouses = Game.world.GetMajorHouses();
            Dictionary<int, House> dictAllHouses = Game.world.GetAllHouses();
            Dictionary<int, Actor> dictAllActors = Game.world.GetAllActors();
            Dictionary<int, int> dictHousePower = Game.world.GetHousePower();
            int[] arrayTradeData = Game.world.GetTradeData();
            //calcs
            int numLocs = Game.network.GetNumLocations();
            int numGreatHouses = dictMajorHouses.Count;
            int numSpecialLocs = Game.network.GetNumSpecialLocations();
            int numBannerLords = dictAllHouses.Count - numGreatHouses - 1 - numSpecialLocs;
            int numActors = dictAllActors.Count;
            int numChildren = numActors - (numGreatHouses * 2) - numBannerLords;
            int numSecrets = Game.world.GetPossessionsCount(PossessionType.Secret);
            int numRumours = Game.world.GetRumoursNormalCount();
            int numTimedRumours = Game.world.GetRumoursTimedCount();
            //checksum
            if (numLocs != numGreatHouses + numSpecialLocs + numBannerLords)
            { Game.SetError(new Error(25, "Locations don't tally")); }
            int numErrors = Game.GetErrorCount();
            //data
            listStats.Add(new Snippet("--- Generation Statistics", RLColor.Yellow, RLColor.Black));
            listStats.Add(new Snippet(string.Format("{0} Locations", numLocs)));
            listStats.Add(new Snippet(string.Format("{0} Great Houses", numGreatHouses)));
            listStats.Add(new Snippet(string.Format("{0} BannerLords", numBannerLords)));
            listStats.Add(new Snippet(string.Format("{0} Special Locations", numSpecialLocs)));
            listStats.Add(new Snippet("1 Capital"));
            listStats.Add(new Snippet(string.Format("{0} Actors ({1} Children)", numActors, numChildren)));
            listStats.Add(new Snippet(string.Format("{0} Secrets", numSecrets)));
            listStats.Add(new Snippet(string.Format("{0} Total Rumours  ({1} Normal, {2} Timed)", numRumours + numTimedRumours, numRumours, numTimedRumours)));
            if (numErrors > 0) { listStats.Add(new Snippet(string.Format("{0} Errors", numErrors), RLColor.LightRed, RLColor.Black)); }
            //Total population and food capacity
            int food = 0; int population = 0;
            foreach (var house in dictAllHouses)
            {
                food += house.Value.FoodCapacity;
                population += house.Value.Population;
            }
            listStats.Add(new Snippet($"Total Population {population:N0}, Total Food Capacity {food:N0} Surplus/Shortfall {food - population:N0}"));
            string tradeText = string.Format("Total Net World Wealth {0}{1}", arrayTradeData[0] > 0 ? "+" : "", arrayTradeData[0]);
            listStats.Add(new Snippet(tradeText));
            string goodsText = string.Format("Goods: Iron x {0}, Timber x {1}, Gold x {2}, Wine x {3}, Oil x {4}, Wool x {5}, Furs x {6}", arrayTradeData[(int)Goods.Iron], arrayTradeData[(int)Goods.Timber],
                arrayTradeData[(int)Goods.Gold], arrayTradeData[(int)Goods.Wine], arrayTradeData[(int)Goods.Oil], arrayTradeData[(int)Goods.Wool], arrayTradeData[(int)Goods.Furs]);
            listStats.Add(new Snippet(goodsText));
            //list of all Greathouses by power
            listStats.Add(new Snippet("Great Houses", RLColor.Yellow, RLColor.Black));
            string housePower;
            foreach (var power in dictHousePower)
            {
                MajorHouse house = Game.world.GetMajorHouse(power.Key);
                housePower = string.Format("Hid {0} House {1} has {2} BannerLords  {3}, Loyal to the {4} (orig {5})", house.HouseID, house.Name, house.GetNumBannerLords(),
                    Game.world.GetLocationCoords(house.LocID), house.Loyalty_Current, house.Loyalty_AtStart);
                //highlight great houses still loyal to the old king
                if (house.Loyalty_Current == KingLoyalty.New_King) { houseColor = RLColor.White; }
                else { houseColor = Color._goodTrait; }
                listStats.Add(new Snippet(housePower, houseColor, RLColor.Black));
            }

            //if start of game also show Errors
            if (Game.gameTurn == 0)
            {
                List<Snippet> tempList = Game.ShowErrorsRL();
                if (tempList.Count > 0)
                {
                    listStats.Add(new Snippet(""));
                    listStats.Add(new Snippet(""));
                    listStats.Add(new Snippet(""));
                    listStats.Add(new Snippet("--- Errors ALL", RLColor.LightRed, RLColor.Black));
                    listStats.AddRange(tempList);
                }
            }
            //display data
            Game.infoChannel.SetInfoList(listStats, ConsoleDisplay.Multi);
        }

        /// <summary>
        /// Show all key relations with King
        /// </summary>
        public void ShowKingRelationsRL()
        {
            List<Snippet> listDisplay = new List<Snippet>();
            CapitalHouse capital = Game.world.GetCapital();
            RLColor starColor;
            int relLvl, stars;
            int spacer = 2; //number of blank lines between data groups
            if (capital != null)
            {
                //World Groups
                listDisplay.Add(new Snippet("--- KingsKeep Groups", RLColor.Yellow, RLColor.Black));
                for (int i = 1; i < (int)WorldGroup.Count; i++)
                {
                    relLvl = capital.GetGroupRelations((WorldGroup)i);
                    stars = relLvl / 20 + 1;
                    stars = Math.Min(5, stars);
                    if (stars <= 2) { starColor = RLColor.LightRed; } else { starColor = RLColor.Yellow; }
                    listDisplay.Add(new Snippet($"{(WorldGroup)i,-25}", false));
                    listDisplay.Add(new Snippet($"{GetStars(stars),-15}", starColor, RLColor.Black, false));
                    listDisplay.Add(new Snippet($"Rel Lvl {relLvl}%", RLColor.LightGray, RLColor.Black));
                }
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Lords
                Dictionary<int, MajorHouse> dictMajorHouses = Game.world.GetMajorHouses();
                if (dictMajorHouses != null)
                {
                    listDisplay.Add(new Snippet("--- Major Houses", RLColor.Yellow, RLColor.Black));
                    foreach (var house in dictMajorHouses)
                    {
                        Passive lord = Game.world.GetPassiveActor(house.Value.LordID);
                        if (lord != null)
                        {
                            relLvl = 100 - lord.GetRelPlyr();
                            stars = relLvl / 20 + 1;
                            stars = Math.Min(5, stars);
                            if (stars <= 2) { starColor = RLColor.LightRed; } else { starColor = RLColor.Yellow; }
                            listDisplay.Add(new Snippet($"{"House " + house.Value.Name,-25}", false));
                            listDisplay.Add(new Snippet($"{GetStars(stars),-15}", starColor, RLColor.Black, false));
                            listDisplay.Add(new Snippet($"Lord {lord.Name}, \"{ lord.Handle }\""));
                        }
                        else { Game.SetError(new Error(308, $"Invalid Lord (null) from house.Value.LordID {house.Value.LordID}")); }
                    }
                }
                else { Game.SetError(new Error(308, "Invalid dictMajorHouses (null) -> Lord rel's not shown")); }
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Lenders
                listDisplay.Add(new Snippet("--- Lenders", RLColor.Yellow, RLColor.Black));
                for (int i = 1; i < (int)Finance.Count; i++)
                {
                    relLvl = capital.GetLenderRelations((Finance)i);
                    stars = relLvl / 20 + 1;
                    stars = Math.Min(5, stars);
                    if (stars <= 2) { starColor = RLColor.LightRed; } else { starColor = RLColor.Yellow; }
                    listDisplay.Add(new Snippet($"{(Finance)i,-25}", false));
                    listDisplay.Add(new Snippet($"{GetStars(stars),-15}", starColor, RLColor.Black, false));
                    listDisplay.Add(new Snippet($"Rel Lvl {relLvl}%", RLColor.LightGray, RLColor.Black));
                }

                //display data
                Game.infoChannel.SetInfoList(listDisplay, ConsoleDisplay.Multi);
            }
            else { Game.SetError(new Error(308, "Invalid Capital (null) -> no rel's shown")); }
        }

        /// <summary>
        /// Generate a list of All Known Rumours
        /// <param name="displayMode">Which filtered set do you wish to display?</param>
        /// <param name="data">A multipurpose data point that can be used to further filter rumours, eg. RefID for House Rumours, has a default of '0'</param>
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowRumoursRL(RumourDisplay displayMode, int data = 0)
        {
            List<Snippet> listData = new List<Snippet>();
            Dictionary<int, Rumour> dictRumoursKnown = Game.world.GetRumoursKnown();
            string description;
            int age;
            bool selectRumour;
            foreach (var rumour in dictRumoursKnown)
            {
                selectRumour = false;
                switch (displayMode)
                {
                    case RumourDisplay.All:
                        selectRumour = true;
                        break;
                    case RumourDisplay.Enemies:
                        if (rumour.Value is RumourEnemy)
                        { selectRumour = true; }
                        break;
                }
                //add to list if meets the filter criteria
                if (selectRumour == true)
                {
                    age = Game.gameTurn - rumour.Value.TurnCreated;
                    description = string.Format("RID {0}, \"{1}\", {2} day{3} old", rumour.Key, rumour.Value.Text, age, age != 1 ? "s" : "");
                    listData.Add(new Snippet(description));
                }
            }
            return listData;
        }

        /// <summary>
        ///Generate a list with all details of a specific rumour (Known or Unknown)
        /// </summary>
        /// <param name="rumourID"></param>
        /// <returns></returns>
        public List<Snippet> ShowRumourRL(int rumourID, bool knownRumour = true)
        {
            List<Snippet> listData = new List<Snippet>();
            Rumour rumour = null;
            //get rumour
            if (knownRumour == true)
            { rumour = Game.world.GetRumourKnown(rumourID); }
            else { rumour = Game.world.GetRumour(rumourID); }
            //valid rumour?
            if (rumour != null)
            {
                int age = Game.gameTurn - rumour.TurnCreated;
                //basic description for all rumours
                listData.Add(new Snippet($"--- RumourID {rumour.RumourID}", RLColor.Yellow, RLColor.Black));
                listData.Add(new Snippet(string.Format("{0} day{1} old", age, age != 1 ? "s" : "")));
                listData.Add(new Snippet($"\"{rumour.Text}\""));
                Active active = Game.world.GetActiveActor(rumour.WhoHeard);
                if (active != null && rumour.RefID > 0)
                {
                    age = Game.gameTurn - rumour.TurnRevealed;
                    string description = string.Format("First heard by {0}, ActID {1}, {2} day{3} ago at {4}", active.Name, active.ActID, age, age != 0 ? "s" : "",
                        Game.world.GetLocationName(Game.world.ConvertRefToLoc(rumour.RefID)));
                    listData.Add(new Snippet(description));
                }

                //rumour specific text
                switch (rumour.Type)
                {
                    case RumourType.Skill:
                    case RumourType.HouseRel:
                    case RumourType.Item:
                    case RumourType.Road:
                    case RumourType.Secret:
                    case RumourType.Terrain:
                    case RumourType.Disguise:
                    case RumourType.Desire:
                    case RumourType.Friends:
                    case RumourType.Enemy:
                    case RumourType.Relationship:
                    case RumourType.Event:
                    case RumourType.Goods:
                    case RumourType.HouseHistory:
                    case RumourType.Military:
                    case RumourType.Loan:
                    case RumourType.Lender:
                        break;
                    default:
                        Game.SetError(new Error(275, $"Invalid Rumour.Type \"{rumour.Type}\""));
                        listData.Add(new Snippet("Invalid Rumour Type", RLColor.LightRed, RLColor.Black));
                        break;
                }
            }
            else
            {
                listData.Add(new Snippet($"RumourID {rumourID} doesn't exist. Try again.", RLColor.LightRed, RLColor.Black));
                //Game.SetError(new Error(275, $"Invalid Rumour for rumourID {rumourID}"));
            }
            return listData;
        }

        /// <summary>
        /// Generate a list of all the horses the Player has had
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowHorsesRL()
        {
            List<Snippet> listData = new List<Snippet>();
            List<HorseRecord> listHorses = Game.world.GetHorses();
            string description;
            for (int i = 0; i < listHorses.Count; i++)
            {
                HorseRecord record = listHorses[i];
                description = string.Format("\"{0}\", a {1}, was {2} {3} on turn {4} after being owned for {5} day{6}", record.Name, record.Type, record.Gone, record.LocText, record.Turn,
                    record.Days, record.Days != 1 ? "s" : "");
                listData.Add(new Snippet(description));
            }
            return listData;
        }


        /// <summary>
        /// Show game state as well as the date as the default input display (green if Good has increased, red if Bad)
        /// </summary>
        public void ShowGameStateRL()
        {
            int data, good, bad, change;
            List<Snippet> listStats = new List<Snippet>();
            RLColor increase = Color._increase;
            RLColor decrease = Color._decrease;
            RLColor foreground;
            //Date
            listStats.AddRange(Game.utility.ShowDate());
            //justice
            data = Game.director.CheckGameState(GameState.Justice);
            good = Game.director.GetGameState(GameState.Justice, DataState.Good);
            bad = Game.director.GetGameState(GameState.Justice, DataState.Bad);
            change = Game.director.CheckGameStateChange(GameState.Justice);
            foreground = RLColor.White;
            if (change > 0) { foreground = increase; }
            else if (change < 0) { foreground = decrease; }
            listStats.Add(new Snippet(string.Format("{0, -18} {1} %  (good {2} bad {3})", "Justice (Cause)", data, good, bad), foreground, RLColor.Black));
            //legend_usurper
            data = Game.director.CheckGameState(GameState.Legend_Usurper);
            good = Game.director.GetGameState(GameState.Legend_Usurper, DataState.Good);
            bad = Game.director.GetGameState(GameState.Legend_Usurper, DataState.Bad);
            change = Game.director.CheckGameStateChange(GameState.Legend_Usurper);
            foreground = RLColor.White;
            if (change > 0) { foreground = increase; }
            else if (change < 0) { foreground = decrease; }
            listStats.Add(new Snippet(string.Format("{0, -18} {1} %  (good {2} bad {3})", "Legend (You)", data, good, bad), foreground, RLColor.Black));
            //legend_king
            data = Game.director.CheckGameState(GameState.Legend_King);
            good = Game.director.GetGameState(GameState.Legend_King, DataState.Good);
            bad = Game.director.GetGameState(GameState.Legend_King, DataState.Bad);
            change = Game.director.CheckGameStateChange(GameState.Legend_King);
            foreground = RLColor.White;
            if (change > 0) { foreground = increase; }
            else if (change < 0) { foreground = decrease; }
            listStats.Add(new Snippet(string.Format("{0, -18} {1} %  (good {2} bad {3})", "Legend (King)", data, good, bad), foreground, RLColor.Black));
            //honour_usurper
            data = Game.director.CheckGameState(GameState.Honour_Usurper);
            good = Game.director.GetGameState(GameState.Honour_Usurper, DataState.Good);
            bad = Game.director.GetGameState(GameState.Honour_Usurper, DataState.Bad);
            change = Game.director.CheckGameStateChange(GameState.Honour_Usurper);
            foreground = RLColor.White;
            if (change > 0) { foreground = increase; }
            else if (change < 0) { foreground = decrease; }
            listStats.Add(new Snippet(string.Format("{0, -18} {1} %  (good {2} bad {3})", "Honour (You)", data, good, bad), foreground, RLColor.Black));
            //honour_king
            data = Game.director.CheckGameState(GameState.Honour_King);
            good = Game.director.GetGameState(GameState.Honour_King, DataState.Good);
            bad = Game.director.GetGameState(GameState.Honour_King, DataState.Bad);
            change = Game.director.CheckGameStateChange(GameState.Honour_King);
            foreground = RLColor.White;
            if (change > 0) { foreground = increase; }
            else if (change < 0) { foreground = decrease; }
            listStats.Add(new Snippet(string.Format("{0, -18} {1} %  (good {2} bad {3})", "Honour (King)", data, good, bad), foreground, RLColor.Black));

            //show Visibility status
            int knownStatus = Game.world.GetActiveActorKnownStatus(1);
            if (knownStatus > 0)
            { listStats.Add(new Snippet(string.Format("Known, reverts in {0} day{1}", knownStatus, knownStatus == 1 ? "" : "s"), Color._badTrait, RLColor.Black)); }
            else { listStats.Add(new Snippet("Unknown (the Inquisitors don't know your location)", Color._goodTrait, RLColor.Black)); }

            //display data
            Game.infoChannel.SetInfoList(listStats, ConsoleDisplay.Input);
        }

        /// <summary>
        /// Display a list of all GameStats and their current values
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowGameStatsRL()
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            GameStatistic[] tempGameStats = Game.statistic.GetArrayOfGameStats();
            int[] tempStatistics = Game.statistic.GetArrayOfStatistics();
            listToDisplay.Add(new Snippet("--- GameStats with Current Values", RLColor.Yellow, RLColor.Black));
            if (tempGameStats.Length > 0 && tempGameStats.Length == tempStatistics.Length)
            {
                string description;
                for (int i = 1; i < tempGameStats.Length; i++)
                {
                    description = $"{i,-3}:  {tempGameStats[i],-20} -> {tempStatistics[i],4}";
                    listToDisplay.Add(new Snippet(description));
                }
            }
            else
            {
                string errorText = $"arrayOfGameStats.Length \"{tempGameStats.Length}\" doesn't match arrayOfStatistics.Length \"{tempStatistics.Length}\"";
                Game.SetError(new Error(260, errorText));
                listToDisplay.Add(new Snippet(errorText, RLColor.LightRed, RLColor.Black));
            }
            return listToDisplay;
        }


        /// <summary>
        /// Display a list of all GameVars and their current values
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowGameVarsRL()
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            GameVar[] tempGameVars = Game.variable.GetArrayOfGameVars();
            int[] tempVariables = Game.variable.GetArrayOfVariables();
            listToDisplay.Add(new Snippet("--- GameVars with Current Values", RLColor.Yellow, RLColor.Black));
            if (tempGameVars.Length > 0 && tempGameVars.Length == tempVariables.Length)
            {
                string description;
                for (int i = 1; i < tempGameVars.Length; i++)
                {
                    description = $"{i,-3}:  {tempGameVars[i],-20} -> {tempVariables[i],4}";
                    listToDisplay.Add(new Snippet(description));
                }
            }
            else
            {
                string errorText = $"arrayOfGameVars.Length \"{tempGameVars.Length}\" doesn't match arrayOfVariables.Length \"{tempVariables.Length}\"";
                Game.SetError(new Error(233, errorText));
                listToDisplay.Add(new Snippet(errorText, RLColor.LightRed, RLColor.Black));
            }
            return listToDisplay;
        }


        /// <summary>
        /// Display a single Actor in detail
        /// </summary>
        /// <param name="ActID"></param>
        /// <returns></returns>
        public List<Snippet> ShowActorRL(int actorID)
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            //collections
            Dictionary<int, Actor> dictAllActors = Game.world.GetAllActors();
            Actor person = new Actor();
            RLColor foreColor, tagColor;
            RLColor unknownColor = RLColor.LightGray;
            string actorType;
            if (dictAllActors.TryGetValue(actorID, out person))
            {
                int locID = person.LocID;
                int refID = Game.world.ConvertLocToRef(locID);
                House house = Game.world.GetHouse(refID);
                //Set up people types
                Player player = null;
                Active active = null;
                Disguise disguise = null;
                //player
                if (person is Player)
                {
                    player = person as Player;
                    //disguise
                    if (player.ConcealDisguise > 0)
                    {
                        Possession possession = Game.world.GetPossession(player.ConcealDisguise);
                        if (possession != null && possession is Disguise)
                        { disguise = possession as Disguise; }
                    }
                }
                if (person is Active)
                { active = person as Active; }
                //advisors can be one of three different categories
                if (person is Advisor) { actorType = Game.world.GetAdvisorType((Advisor)person); }
                else { actorType = person.Title; }
                if ((int)person.Office > 0)
                { actorType = Convert.ToString(person.Office); }
                string name = string.Format("{0} {1}", actorType, person is Follower ? "\"" + person.Name + "\"" : person.Name);
                string handle = null;
                bool newLine = true;
                //nickname
                if (person.Age >= 15)
                {
                    newLine = false;
                    if (String.IsNullOrEmpty(person.Handle) == true) { handle = ""; }
                    else { handle = string.Format(" \"{0}\"", person.Handle); }
                }
                RLColor color = RLColor.White;
                RLColor locColor = RLColor.White;
                string locString = "?";
                listToDisplay.Add(new Snippet(name, RLColor.Yellow, RLColor.Black, false));
                //nickname (show as White)?
                if (handle != null)
                { listToDisplay.Add(new Snippet(handle, Color._star, RLColor.Black, false)); }
                //actorID
                listToDisplay.Add(new Snippet(string.Format(", Aid {0}", actorID), RLColor.Yellow, RLColor.Black));
                //realm
                if ((int)person.Realm > 0)
                { listToDisplay.Add(new Snippet(string.Format("Realm Title: {0}", person.Realm))); }
                //office
                if ((int)person.Office > 0)
                { listToDisplay.Add(new Snippet(string.Format("Office: {0}", person.Office), RLColor.Yellow, RLColor.Black)); }
                //location (if special replace with description)
                if (!(person is Special))
                {
                    //location descriptor
                    switch (person.Status)
                    {
                        case ActorStatus.AtLocation:
                            locString = string.Format("Located at {0} {1}, Lid {2}, Rid {3}", Game.world.GetLocationName(locID), Game.world.GetLocationCoords(locID), locID, refID);
                            break;
                        case ActorStatus.Travelling:
                            Position pos = person.GetActorPosition();
                            locString = string.Format("Currently at {0}:{1}, {2} towards {3} {4}, Lid {5}, Rid {6}", pos.PosX, pos.PosY, person.Travel == TravelMode.Mounted ? "Riding" : "Walking",
                                Game.world.GetLocationName(locID), Game.world.GetLocationCoords(locID), locID, refID);
                            break;
                        case ActorStatus.AtSea:
                            if (person is Player)
                            {
                                locString = string.Format("At Sea onboard the S.S \"{0}\" bound for {1}, arriving in {2} more day{3}", player.ShipName, Game.world.GetLocationName(locID),
                                    player.VoyageTimer, player.VoyageTimer != 1 ? "s" : "");
                            }
                            break;
                        case ActorStatus.Captured:
                            locString = string.Format("Incarcerated in the bowels of the {0} dungeons. Survival time {1} more day{2}", Game.world.GetLocationName(locID), player.DeathTimer,
                                player.DeathTimer != 1 ? "s" : "");
                            break;
                        case ActorStatus.Adrift:
                            if (person is Player)
                            { locString = string.Format("Adrift in {0}. Survival time {1} day{2}", player.SeaName, player.DeathTimer, player.DeathTimer != 1 ? "s" : ""); }
                            break;
                        case ActorStatus.Gone:
                            locString = string.Format("Passed away ({0}) in {1}", person.ReasonGone, person.Gone);
                            locColor = RLColor.Red;
                            break;
                    }
                    listToDisplay.Add(new Snippet(locString, locColor, RLColor.Black));
                    //Concealed
                    if (player?.Conceal > ActorConceal.None)
                    {
                        switch (player.Conceal)
                        {
                            case ActorConceal.SafeHouse:
                                listToDisplay.Add(new Snippet(string.Format("{0, -16}", "At SafeHouse"), RLColor.Yellow, RLColor.Black, false));
                                listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(player.ConcealLevel)), RLColor.LightRed, RLColor.Black));
                                break;
                            case ActorConceal.Disguise:
                                listToDisplay.Add(new Snippet(string.Format("{0, -16}", "In Disguise"), RLColor.Yellow, RLColor.Black, false));
                                listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(player.ConcealLevel)), RLColor.LightRed, RLColor.Black, false));
                                listToDisplay.Add(new Snippet($"{disguise?.Description}", unknownColor, RLColor.Black));
                                break;
                        }
                    }
                }
                else { listToDisplay.Add(new Snippet(person.Description)); }
                //Delayed
                if (person.Delay > 0)
                { listToDisplay.Add(new Snippet(string.Format("Delayed (\"{0}\") for {1} {2}", person.DelayReason, person.Delay, person.Delay == 1 ? "turn" : "turns"), RLColor.LightRed, RLColor.Black)); }
                if (person is Knight)
                {
                    Knight knight = person as Knight;
                    string houseName;
                    if (knight.HouseID == Game.lore.OldHouseID) { houseName = Game.lore.OldHouseName; }
                    //deals with case of knight belonging to old King (he's been deleted from dictMajorHouses)
                    else { houseName = Game.world.GetMajorHouseName(knight.HouseID); }
                    listToDisplay.Add(new Snippet(string.Format("Has sworn allegiance to House {0}", houseName)));
                }
                //Loyalty
                if (person is Passive && !(person is Special))
                { listToDisplay.Add(new Snippet(string.Format("Loyal to the {0} (originally {1})", person.Loyalty_Current, person.Loyalty_AtStart))); }
                listToDisplay.Add(new Snippet(string.Format("{0} y.o {1}, born {2}", person.Age, person.Sex, person.Born)));
                //stats - natural ---
                string effectText = null;
                int abilityStars;
                RLColor traitColor;
                RLColor starColor;
                RLColor skillColor;

                SkillType trait;
                //age of actor
                SkillAge age = SkillAge.Fifteen;
                if (person.Age >= 5 && person.Age < 15)
                { age = SkillAge.Five; }
                //only show abilities if age >= 5 & not dead
                if (person.Age >= 5)
                {
                    //header
                    listToDisplay.Add(new Snippet("Abilities (some at 5 y.o, all at 15 y.o)", RLColor.Brown, RLColor.Black));
                    //combat
                    trait = SkillType.Combat;
                    effectText = person.GetTraitEffectText(trait, age);
                    abilityStars = person.GetSkill(trait, age);
                    if (abilityStars < 3) { traitColor = Color._badTrait; }
                    else if (abilityStars == 3) { traitColor = Color._star; }
                    else { traitColor = Color._goodTrait; }
                    //Combat skill known?
                    if (person.GetSkillKnown(SkillType.Combat) == true) { starColor = Color._star; skillColor = RLColor.White; }
                    else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                    //display
                    newLine = true;
                    if (abilityStars != 3)
                    { newLine = false; }
                    if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Combat"), skillColor, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                        if (abilityStars != 3)
                        { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                    }
                    //Wits
                    trait = SkillType.Wits;
                    effectText = person.GetTraitEffectText(trait, age);
                    abilityStars = person.GetSkill(trait, age);
                    if (abilityStars < 3) { traitColor = Color._badTrait; }
                    else if (abilityStars == 3) { traitColor = Color._star; }
                    else { traitColor = Color._goodTrait; }
                    //Wits skill known?
                    if (person.GetSkillKnown(SkillType.Wits) == true) { starColor = Color._star; skillColor = RLColor.White; }
                    else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                    //display
                    newLine = true;
                    if (abilityStars != 3)
                    { newLine = false; }
                    if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Wits"), skillColor, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                        if (abilityStars != 3)
                        { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                    }
                    //charm
                    trait = SkillType.Charm;
                    effectText = person.GetTraitEffectText(trait, age);
                    abilityStars = person.GetSkill(trait, age);
                    if (abilityStars < 3) { traitColor = Color._badTrait; }
                    else if (abilityStars == 3) { traitColor = Color._star; }
                    else { traitColor = Color._goodTrait; }
                    //Charm skill known?
                    if (person.GetSkillKnown(SkillType.Charm) == true) { starColor = Color._star; skillColor = RLColor.White; }
                    else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                    //display
                    newLine = true;
                    if (abilityStars != 3)
                    { newLine = false; }
                    if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Charm"), skillColor, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                        if (abilityStars != 3)
                        { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                    }
                    //Treachery
                    trait = SkillType.Treachery;
                    effectText = person.GetTraitEffectText(trait, age);
                    abilityStars = person.GetSkill(trait, age);
                    if (abilityStars < 3) { traitColor = Color._badTrait; }
                    else if (abilityStars == 3) { traitColor = Color._star; }
                    else { traitColor = Color._goodTrait; }
                    //Treachery skill known?
                    if (person.GetSkillKnown(SkillType.Treachery) == true) { starColor = Color._star; skillColor = RLColor.White; }
                    else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                    //display
                    newLine = true;
                    if (abilityStars != 3)
                    { newLine = false; }
                    if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Treachery"), skillColor, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                        if (abilityStars != 3)
                        { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                    }
                    //Leadership
                    trait = SkillType.Leadership;
                    effectText = person.GetTraitEffectText(trait, age);
                    abilityStars = person.GetSkill(trait, age);
                    if (abilityStars < 3) { traitColor = Color._badTrait; }
                    else if (abilityStars == 3) { traitColor = Color._star; }
                    else { traitColor = Color._goodTrait; }
                    //Leadership skill known?
                    if (person.GetSkillKnown(SkillType.Leadership) == true) { starColor = Color._star; skillColor = RLColor.White; }
                    else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                    //display
                    newLine = true;
                    if (abilityStars != 3)
                    { newLine = false; }
                    if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Leadership"), skillColor, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                        if (abilityStars != 3)
                        { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                    }
                    //Touched
                    if (person.Touched > 0)
                    {
                        trait = SkillType.Touched;
                        effectText = person.GetTraitEffectText(trait, age);
                        abilityStars = person.GetSkill(trait, age);
                        if (abilityStars < 3) { traitColor = Color._badTrait; }
                        else if (abilityStars == 3) { traitColor = Color._star; }
                        else { traitColor = Color._goodTrait; }
                        //Touched skill known?
                        if (person.GetSkillKnown(SkillType.Touched) == true) { starColor = Color._star; skillColor = RLColor.White; }
                        else { skillColor = unknownColor; traitColor = unknownColor; starColor = unknownColor; }
                        //display
                        newLine = true;
                        if (abilityStars != 3)
                        { newLine = false; }
                        if ((age == SkillAge.Five && abilityStars != 3) || age == SkillAge.Fifteen)
                        {
                            listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Touched"), skillColor, RLColor.Black, false));
                            listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(abilityStars)), starColor, RLColor.Black, newLine));
                            if (abilityStars != 3)
                            { listToDisplay.Add(new Snippet(string.Format("{0} {1}", person.arrayOfTraitNames[(int)trait], effectText), traitColor, RLColor.Black)); }
                        }
                    }
                }
                //Conditions
                if (person.CheckConditions() == true)
                {
                    List<Condition> tempConditions = person.GetConditions();
                    listToDisplay.Add(new Snippet("Conditions (additional Skill modifiers)", RLColor.Brown, RLColor.Black));
                    string tempCondition_0, tempCondition_1;
                    RLColor tempColor = RLColor.White;
                    foreach (Condition condition in tempConditions)
                    {
                        if (condition.Timer != 999)
                        { tempCondition_0 = string.Format("\"{0}\", {1} day{2}", condition.Text, condition.Timer, condition.Timer == 1 ? "" : "s"); }
                        else { tempCondition_0 = string.Format("\"{0}\"", condition.Text); }
                        tempCondition_1 = string.Format("{0} ({1}{2})", condition.Skill, condition.Effect > 0 ? "+" : "", condition.Effect);
                        if (condition.Effect > 0) { tempColor = Color._goodTrait; }
                        else { tempColor = Color._badTrait; }
                        listToDisplay.Add(new Snippet(string.Format("{0, -28}", tempCondition_0), false));
                        listToDisplay.Add(new Snippet(string.Format("{0}", tempCondition_1), tempColor, RLColor.Black));
                    }
                }
                //Crow explanation for loyal followers
                if (person is Follower)
                {
                    Active tempPerson = person as Active;
                    List<string> tempList = tempPerson.GetCrowTooltips();
                    if (tempList.Count > 0)
                    {
                        listToDisplay.Add(new Snippet("Crow Explanation", RLColor.Brown, RLColor.Black));
                        foreach (string tip in tempList)
                        { listToDisplay.Add(new Snippet(tip)); }
                        if (tempPerson.Activated == true)
                        { listToDisplay.Add(new Snippet("Activated", RLColor.Yellow, RLColor.Black)); }
                        else
                        { listToDisplay.Add(new Snippet("Not activated", RLColor.Red, RLColor.Black)); }
                    }
                }
                //relationships (ignore for dead actors)
                if (!(person is Player || person is Special) && person.Status != ActorStatus.Gone)
                {
                    listToDisplay.Add(new Snippet("Relationships", RLColor.Brown, RLColor.Black));

                    //with Player
                    int relStars = person.GetRelPlyrStars();
                    listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Player"), false));
                    if (person.RelKnown == true) { foreColor = RLColor.LightRed; } else { foreColor = unknownColor; }
                    listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(relStars)), foreColor, RLColor.Black, false));
                    int change = person.GetPlayerChange();
                    int relPlyr = person.GetRelPlyr();
                    string tagText = string.Format("(Change {0}{1})", change > 0 ? "+" : "", change);
                    if (change == 0) { tagText = ""; }
                    if (person.RelKnown == true)
                    {
                        tagColor = Color._badTrait;
                        if (relPlyr >= 50) { tagColor = Color._goodTrait; }
                    }
                    else { tagColor = unknownColor; }
                    listToDisplay.Add(new Snippet(string.Format("{0}, Rel {1}, {2}", person.GetPlayerTag(), relPlyr, tagText),
                        tagColor, RLColor.Black, true));
                    //with Lord
                    if (person.Type != ActorType.Lord && person is Passive || person is Inquisitor && person.Status != ActorStatus.Gone)
                    {
                        relStars = person.GetRelLordStars();
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Lord"), false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(relStars)), foreColor, RLColor.Black, false));
                        change = person.GetLordChange();
                        int relLord = person.GetRelLord();
                        tagText = string.Format("(Change {0}{1})", change > 0 ? "+" : "", change);
                        if (change == 0) { tagText = ""; }
                        if (person.RelKnown == true)
                        {
                            tagColor = Color._badTrait;
                            if (relLord >= 50) { tagColor = Color._goodTrait; }
                        }
                        else { tagColor = unknownColor; }
                        listToDisplay.Add(new Snippet(string.Format("{0}, Rel {1}, {2}", person.GetLordTag(), relLord, tagText),
                            tagColor, RLColor.Black, true));
                    }
                }
                //Possessions -> active followers
                if (person is Active)
                {
                    //Resources
                    int resources = active.Resources;
                    resources = Math.Min(5, resources);
                    listToDisplay.Add(new Snippet("Possessions", RLColor.Brown, RLColor.Black));
                    listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Resources"), false));
                    listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(resources)), RLColor.LightRed, RLColor.Black, false));
                    listToDisplay.Add(new Snippet(string.Format("{0}", (ResourceLevel)resources), true));
                    //Disguise
                    if (player?.ConcealDisguise > 0 && disguise != null)
                    {
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Disguise"), false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(disguise.Strength)), RLColor.LightRed, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(string.Format("{0}", disguise.Description), true));
                    }
                }
                //Possessions -> Lords and BannerLords
                else if (person.Type == ActorType.Lord || person.Type == ActorType.BannerLord)
                {
                    int resources = person.Resources;
                    resources = Math.Min(5, resources);
                    listToDisplay.Add(new Snippet("Possessions", RLColor.Brown, RLColor.Black));
                    listToDisplay.Add(new Snippet(string.Format("{0, -16}", "Resources"), false));
                    listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(resources)), RLColor.LightRed, RLColor.Black, false));
                    listToDisplay.Add(new Snippet(string.Format("{0}", (ResourceLevel)resources), true));
                }
                else if (person is Advisor)
                {
                    //Show any Disguises available to give to the Player
                    Advisor advisor = person as Advisor;
                    if (advisor.CheckDisguises() == true)
                    {
                        listToDisplay.Add(new Snippet("Disguises", RLColor.Brown, RLColor.Black));
                        List<int> listOfDisguises = advisor.GetDisguises();
                        if (listOfDisguises != null)
                        {
                            foreach (var costume in listOfDisguises)
                            {
                                Possession possession = Game.world.GetPossession(costume);
                                if (possession != null)
                                {
                                    if (possession is Disguise)
                                    {
                                        Disguise tempDisguise = possession as Disguise;
                                        listToDisplay.Add(new Snippet($"Disguise \"{tempDisguise.Description}\", PossID {tempDisguise.PossID} {Game.display.GetStars(tempDisguise.Strength)}", unknownColor, RLColor.Black));
                                    }
                                    else { Game.SetError(new Error(245, $"Invalid Possession (not a Disguise) for PossID {possession.PossID}")); }
                                }
                                else { Game.SetError(new Error(245, "Invalid Possession (null) for Advisor Disguises")); }
                            }
                        }
                        else { Game.SetError(new Error(245, "Invalid listOfDisguises (null) for Advisors")); }
                    }
                }
                //Possessions -> Items
                List<int> listItems = person.GetItems();
                if (listItems.Count > 0)
                {
                    foreach (int possID in listItems)
                    {
                        Item item = Game.world.GetItem(possID);
                        //add possession label for all non-Lords who wouldn't normally have one
                        if (person is Passive && (person.Type != ActorType.Lord && person.Type != ActorType.BannerLord))
                        { listToDisplay.Add(new Snippet("Possessions", RLColor.Brown, RLColor.Black)); }
                        //debug -> display item in grey if it isn't known (eg. a secret)
                        if (item.Active == true)
                        { listToDisplay.Add(new Snippet(string.Format("Item ID {0}, \"{1}\", {2}", item.ItemID, item.Description, item.ItemType))); }
                        else { listToDisplay.Add(new Snippet(string.Format("Item ID {0}, \"{1}\", {2}", item.ItemID, item.Description, item.ItemType), unknownColor, RLColor.Black)); }
                    }
                }
                //Horse
                if (person is Player)
                {
                    listToDisplay.Add(new Snippet("Horse", RLColor.Brown, RLColor.Black));
                    if (player.horseStatus != HorseStatus.Gone)
                    {
                        string horseText = string.Format("A {0}, {1}, owned for {2} day{3}", player.HorseType, player.horseStatus, player.HorseDays, player.HorseDays != 1 ? "s" : "");
                        string horseName = $"\"{player.HorseName}\"";
                        listToDisplay.Add(new Snippet(string.Format("{0, -16}", horseName), false));
                        listToDisplay.Add(new Snippet(string.Format("{0, -12}", Game.display.GetStars(player.HorseHealth)), RLColor.LightRed, RLColor.Black, false));
                        listToDisplay.Add(new Snippet(horseText, true));
                    }
                    else { listToDisplay.Add(new Snippet("None", RLColor.White, RLColor.Black)); }
                }
                //Desires 
                if (person is Passive)
                {
                    Passive passivePerson = person as Passive;
                    if (passivePerson.Desire > PossPromiseType.None)
                    {
                        listToDisplay.Add(new Snippet("Desire", RLColor.Brown, RLColor.Black));
                        if (passivePerson.DesireKnown == true)
                        { listToDisplay.Add(new Snippet($"Wants {passivePerson.DesireText}", RLColor.White, RLColor.Black)); } //visible if known
                        else { listToDisplay.Add(new Snippet($"Wants {passivePerson.DesireText}", unknownColor, RLColor.Black)); } //hidden if not
                    }
                }
                //family
                SortedDictionary<int, ActorRelation> dictTempFamily = null;
                if (person is Noble) { Noble tempPerson = person as Noble; dictTempFamily = tempPerson.GetFamily(); }
                else if (person is Player) { Player tempPerson = person as Player; dictTempFamily = tempPerson.GetFamily(); }
                if (dictTempFamily != null)
                {
                    //SortedDictionary<int, ActorRelation> dictTempFamily = tempFamilyPerson.GetFamily();
                    if (dictTempFamily?.Count > 0)
                    {
                        listToDisplay.Add(new Snippet("Family", RLColor.Brown, RLColor.Black));
                        string maidenName;
                        foreach (KeyValuePair<int, ActorRelation> kvp in dictTempFamily)
                        {
                            Noble relPerson = (Noble)Game.world.GetPassiveActor(kvp.Key);
                            RLColor familyColor = RLColor.White;
                            if (relPerson.Status == ActorStatus.Gone)
                            { familyColor = RLColor.LightGray; }
                            maidenName = "";
                            if (relPerson.MaidenName != null)
                            { maidenName = string.Format(" (nee {0})", relPerson.MaidenName); }
                            int relAge = Game.gameStart - relPerson.Born;
                            string houseName;
                            //needed 'cause old King's house has been removed from the dictionaries
                            if (relPerson.HouseID == Game.lore.OldHouseID)
                            { houseName = Game.lore.OldHouseName; }
                            else { houseName = Game.world.GetMajorHouseName(relPerson.HouseID); }
                            string text = string.Format("{0} Aid {1}: {2} {3}{4} of House {5}, Age {6}",
                              kvp.Value, relPerson.ActID, relPerson.Type, relPerson.Name, maidenName, houseName, relAge);

                            listToDisplay.Add(new Snippet(text, familyColor, RLColor.Black));
                        }
                    }
                }
                //secrets
                List<int> listOfSecrets = person.GetSecrets();
                if (listOfSecrets.Count > 0)
                {
                    listToDisplay.Add(new Snippet("Secrets", RLColor.Brown, RLColor.Black));
                    foreach (int possessionID in listOfSecrets)
                    {
                        Secret secret = (Secret)Game.world.GetPossession(possessionID);
                        if (secret != null)
                        {
                            listToDisplay.Add(new Snippet(string.Format("{0} {1} ", secret.Year, secret.Description), false));
                            listToDisplay.Add(new Snippet(string.Format("{0}", Game.display.GetStars(secret.Strength)), RLColor.LightRed, RLColor.Black));
                        }
                    }
                }
                //player specific Soft possessions - Favours & Introductions
                if (person is Player)
                {
                    //favours (Player only)
                    List<int> listOfFavours = player.GetFavours();
                    if (listOfFavours.Count > 0)
                    {
                        listToDisplay.Add(new Snippet("Favours", RLColor.Brown, RLColor.Black));
                        foreach (int possessionID in listOfFavours)
                        {
                            Favour favour = (Favour)Game.world.GetPossession(possessionID);
                            if (favour != null)
                            {
                                listToDisplay.Add(new Snippet(string.Format("{0} {1} ", favour.Year, favour.Description), false));
                                listToDisplay.Add(new Snippet(string.Format("{0}", Game.display.GetStars(favour.Strength)), RLColor.LightRed, RLColor.Black));
                            }
                        }
                    }
                    //Introductions (Player only)
                    Dictionary<int, int> dictOfIntroductions = player.GetIntroductions();
                    if (dictOfIntroductions != null)
                    {
                        if (dictOfIntroductions.Count > 0)
                        {
                            listToDisplay.Add(new Snippet("Introductions", RLColor.Brown, RLColor.Black));
                            foreach (var intro in dictOfIntroductions)
                            {
                                if (intro.Value > 0)
                                { listToDisplay.Add(new Snippet(string.Format("You have {0} introduction{1} to House \"{2}\"", intro.Value, intro.Value != 1 ? "s" : "", Game.world.GetHouseName(intro.Key)))); }
                            }
                        }
                    }
                    else { Game.SetError(new Error(245, "Invalid dictOfIntroductions (null)")); }
                }
                //Promises
                List<int> listOfPromises = person.GetPromises();
                if (listOfPromises.Count > 0)
                {
                    listToDisplay.Add(new Snippet("Promises", RLColor.Brown, RLColor.Black));
                    foreach (int possessionID in listOfPromises)
                    {
                        Promise promise = (Promise)Game.world.GetPossession(possessionID);
                        if (promise != null)
                        {
                            listToDisplay.Add(new Snippet(string.Format("{0} {1}", promise.Year, promise.Description), false));
                            listToDisplay.Add(new Snippet(string.Format("    {0}", Game.display.GetStars(promise.Strength)), RLColor.LightRed, RLColor.Black));
                        }
                    }
                }
                //personal history
                List<string> actorHistory = new List<string>();
                if (person is Player)
                {
                    //Player -> get original (pre-game start history)
                    actorHistory = Game.world.GetActorHistoricalRecords(player.HistoryID);
                }
                actorHistory.AddRange(Game.world.GetActorHistoricalRecords(person.ActID));
                if (actorHistory.Count > 0)
                {
                    listToDisplay.Add(new Snippet("Personal History", RLColor.Brown, RLColor.Black));
                    foreach (string text in actorHistory)
                    { listToDisplay.Add(new Snippet(text)); }
                }
                //Current events
                List<string> actorCurrent = new List<string>();
                if (person is Player)
                { actorCurrent.AddRange(Game.world.GetPlayerCurrentRecords(person.ActID)); }
                else
                { actorCurrent.AddRange(Game.world.GetActorCurrentRecords(person.ActID)); }
                if (actorCurrent.Count > 0)
                {
                    listToDisplay.Add(new Snippet("Recent Events", RLColor.Brown, RLColor.Black));
                    foreach (string text in actorCurrent)
                    { listToDisplay.Add(new Snippet(text)); }
                }
                //Relationship records
                if (!(person is Player) && person.Status != ActorStatus.Gone)
                {
                    if (person.RelKnown == true) { foreColor = RLColor.White; } else { foreColor = unknownColor; }
                    //with Player
                    List<Relation> playerRelations = person.GetRelEventPlyr();
                    if (playerRelations.Count > 0)
                    {
                        listToDisplay.Add(new Snippet("Relationship History with Player", RLColor.Brown, RLColor.Black));
                        foreach (Relation relationship in playerRelations)
                        { listToDisplay.Add(new Snippet(relationship.GetRelationText(), foreColor, RLColor.Black)); }
                    }
                    //with Lord
                    List<Relation> lordRelations = person.GetRelEventLord();
                    lordRelations = person.GetRelEventLord();
                    if (lordRelations.Count > 0)
                    {
                        listToDisplay.Add(new Snippet("Relationship History with Lord", RLColor.Brown, RLColor.Black));
                        foreach (Relation relationship in lordRelations)
                        { listToDisplay.Add(new Snippet(relationship.GetRelationText(), foreColor, RLColor.Black)); }
                    }
                }
            }
            else
            { listToDisplay.Add(new Snippet(string.Format("No Character with ID {0} exists", actorID), RLColor.LightRed, RLColor.Black)); }
            return listToDisplay;
        }




        /// <summary>
        /// creates a string showing the number of stars for traits, secrets, etc. (1 to 5 stars)
        /// </summary>
        /// <param name="num">number of stars</param>
        /// <returns></returns>
        internal string GetStars(int num)
        {
            string stars = null;
            num = Math.Min(5, num);
            num = Math.Max(1, num);
            for (int i = 0; i < num; i++)
            { stars += "o "; }
            return stars;
        }
        //methods above here
    }
}
