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
            listStats.Add(new Snippet($"{Game.network.GetNumPorts()} Ports"));
            listStats.Add(new Snippet($"{Game.map.KingsRoadLength} Length of King's Road"));
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
            string goodsText = string.Format("Goods: Iron x {0}, Timber x {1}, Wool x {2}, Fur x {3}, Oil x {4}, Wine x {5}, Gold x {6}", arrayTradeData[(int)Goods.Iron], arrayTradeData[(int)Goods.Timber],
                arrayTradeData[(int)Goods.Wool], arrayTradeData[(int)Goods.Furs], arrayTradeData[(int)Goods.Oil], arrayTradeData[(int)Goods.Wine], arrayTradeData[(int)Goods.Gold]);
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
                int kingID = Game.lore.NewKing.ActID;
                if (dictMajorHouses != null)
                {
                    listDisplay.Add(new Snippet("--- Major Houses", RLColor.Yellow, RLColor.Black));
                    foreach (var house in dictMajorHouses)
                    {
                        Passive lord = Game.world.GetPassiveActor(house.Value.LordID);
                        if (lord != null)
                        {
                            //exclude King's own house
                            if (lord.ActID != kingID)
                            {
                                relLvl = 100 - lord.GetRelPlyr();
                                stars = relLvl / 20 + 1;
                                stars = Math.Min(5, stars);
                                if (stars <= 2) { starColor = RLColor.LightRed; } else { starColor = RLColor.Yellow; }
                                listDisplay.Add(new Snippet($"{"House " + house.Value.Name,-25}", false));
                                listDisplay.Add(new Snippet($"{GetStars(stars),-15}", starColor, RLColor.Black, false));
                                listDisplay.Add(new Snippet($"Lord {lord.Name}, \"{ lord.Handle }\""));
                            }
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
                    //relLvl = capital.GetLenderRelations((Finance)i);
                    relLvl = capital.GetFinanceInfo(Account.Lender, i, FinArray.Data);
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


        public void ShowKingCouncilRL()
        {
            List<Snippet> listDisplay = new List<Snippet>();
            Dictionary<int, Passive> dictRoyalCourt = Game.world.GetRoyalCourt();
            RLColor goodColor = Color._godMode;
            RLColor badColor = RLColor.LightRed;
            RLColor goodTrait = Color._goodTrait;
            RLColor badTrait = Color._badTrait;
            RLColor displayColor;
            listDisplay.Add(new Snippet("--- Royal Council", RLColor.Yellow, RLColor.Black));
            int wits, treachery, loyalty, stars;
            string trait;
            bool newLine;
            foreach (var advisor in dictRoyalCourt)
            {
                trait = "";
                listDisplay.Add(new Snippet(""));
                listDisplay.Add(new Snippet($"{advisor.Value.Title} {advisor.Value.Name}, \"{advisor.Value.Handle}\", ActID {advisor.Value.ActID}", RLColor.Yellow, RLColor.Black));
                //Wits -> Ability
                wits = advisor.Value.GetSkill(SkillType.Wits);
                if (wits != 3) { trait = advisor.Value.GetTraitName(SkillType.Wits); newLine = false; } else { newLine = true; }
                if (wits > 2) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Ability (Wits)",-30}", false));
                listDisplay.Add(new Snippet($"{GetStars(wits),-15}", displayColor, RLColor.Black, newLine));
                if (newLine == false)
                {
                    if (wits > 3) { displayColor = goodTrait; } else { displayColor = badTrait; }
                    listDisplay.Add(new Snippet($"{trait,-20}", displayColor, RLColor.Black));
                }
                //Treachery -> Corruption
                treachery = advisor.Value.GetSkill(SkillType.Treachery);
                if (treachery != 3) { trait = advisor.Value.GetTraitName(SkillType.Treachery); newLine = false; } else { newLine = true; }
                if (treachery <= 3) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Corruption (Treachery)", -30}", false));
                listDisplay.Add(new Snippet($"{GetStars(treachery),-15}", displayColor, RLColor.Black, newLine));
                if (newLine == false)
                {
                    if (treachery < 3) { displayColor = goodTrait; } else { displayColor = badTrait; }
                    listDisplay.Add(new Snippet($"{trait,-20}", displayColor, RLColor.Black));
                }
                //Relationship with the King
                loyalty = advisor.Value.GetRelPlyr();
                if (Game.gameAct == Act.One) { loyalty = 100 - loyalty; }
                stars = (loyalty / 20) + 1;
                stars = Math.Min(5, stars);
                if (stars > 2) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Loyalty (Rel)", -30}", false));
                listDisplay.Add(new Snippet($"{GetStars(stars),-15}", displayColor, RLColor.Black, false));
                if (loyalty >= 50) { displayColor = goodTrait; } else { displayColor = badTrait; }
                listDisplay.Add(new Snippet($"{"Rel " + loyalty + "%",-20}", displayColor, RLColor.Black));
            }
            //display data
            Game.infoChannel.SetInfoList(listDisplay, ConsoleDisplay.Multi);
        }

        /// <summary>
        /// Display Kingdom finances
        /// </summary>
        public void ShowKingFinancesRL()
        {
            List<Snippet> listDisplay = new List<Snippet>();
            CapitalHouse capital = Game.world.GetCapital();
            RLColor goodColor = Color._goodTrait;
            RLColor badColor = Color._badTrait;
            RLColor activeColor = RLColor.White;
            RLColor inactiveColor = RLColor.LightGray;
            RLColor goodStar = RLColor.Yellow;
            RLColor badStar = RLColor.LightRed;
            RLColor displayColor;
            int amount, balance, accounts, rate, cashflow;
            int spacer = 2; //number of blank lines between data groups
            bool newLine;
            if (capital != null)
            {
                //Header
                listDisplay.Add(new Snippet("--- Ye Olde Financial Records", RLColor.Yellow, RLColor.Black));
                accounts = Game.variable.GetValue(GameVar.Account_Timer);
                listDisplay.Add(new Snippet(string.Format("Next tally available in {0} day{1}", accounts, accounts != 1 ? "s" : "")));
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Income
                balance = 0;
                listDisplay.Add(new Snippet("--- Income", RLColor.Yellow, RLColor.Black));
                for (int i = 1; i < (int)Income.Count; i++)
                {
                    amount = capital.GetFinanceInfo(Account.Income, i, FinArray.Data);
                    balance += amount;
                    if (amount != 0) { newLine = false; } else { newLine = true; }
                    if (capital.GetFinanceInfo(Account.Income, i, FinArray.Status) > 0 ) { displayColor = activeColor; } else { displayColor = inactiveColor; }
                    listDisplay.Add(new Snippet($"{"Taxes on " + (Income)i,-30}{amount,-12:N0}", displayColor, RLColor.Black, newLine));
                    if (newLine == false)
                    {
                        rate = capital.GetFinanceInfo(Account.Income, i, FinArray.Rate);
                        if (rate > 3) { displayColor = badStar; } else { displayColor = goodStar; }
                        listDisplay.Add(new Snippet($"{GetStars(rate),-15}", displayColor, RLColor.Black));
                    }
                }
                if (balance > 0) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Balance",-30}{balance, -12:N0}gold coins", displayColor, RLColor.Black));
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Expenses
                balance = 0;
                listDisplay.Add(new Snippet("--- Expenses", RLColor.Yellow, RLColor.Black));
                for (int i = 1; i < (int)Expense.Count; i++)
                {
                    amount = capital.GetFinanceInfo(Account.Expense, i, FinArray.Data);
                    balance += amount;
                    if (amount != 0) { newLine = false; } else { newLine = true; }
                    if (capital.GetFinanceInfo(Account.Expense, i, FinArray.Status) > 0) { displayColor = activeColor; } else { displayColor = inactiveColor; }
                    listDisplay.Add(new Snippet($"{"Cost of " + (Expense)i,-30}{amount,-12:N0}", displayColor, RLColor.Black, newLine));
                    if (newLine == false)
                    {
                        rate = capital.GetFinanceInfo(Account.Expense, i, FinArray.Rate);
                        if (rate > 3) { displayColor = badStar; } else { displayColor = goodStar; }
                        listDisplay.Add(new Snippet($"{GetStars(rate),-15}", displayColor, RLColor.Black));
                    }
                }
                if (balance > 0) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Balance",-30}{balance, -12:N0}gold coins", displayColor, RLColor.Black));
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Summary
                listDisplay.Add(new Snippet("--- Summary", RLColor.Yellow, RLColor.Black));
                //lump sums
                for (int i = 1; i < (int)LumpSum.Count; i++)
                {
                    amount = capital.GetFinanceInfo(Account.LumpSum, i, FinArray.Data);
                    if (capital.GetFinanceInfo(Account.LumpSum, i, FinArray.Status) > 0) { displayColor = activeColor; } else { displayColor = inactiveColor; amount = 0; }
                    listDisplay.Add(new Snippet($"{(LumpSum)i, -30}{amount, -12:N0}", displayColor, RLColor.Black));
                }
                //cashflow (Income - Expenses)
                cashflow = capital.GetFinanceInfo(Account.FinSummary, (int)FinSummary.CashFlow, FinArray.Data);
                if (cashflow > 0) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Net Cashflow",-30}{cashflow:N0}", displayColor, RLColor.Black));
                //balance
                balance = capital.GetFinanceInfo(Account.FinSummary, (int)FinSummary.Balance, FinArray.Data);
                if (balance > 0) { displayColor = goodColor; } else { displayColor = badColor; }
                listDisplay.Add(new Snippet($"{"Balance", -30}{balance, -12:N0}gold coins", displayColor, RLColor.Black));
                //spacer
                for (int i = 0; i < spacer; i++)
                { listDisplay.Add(new Snippet("")); }
                //Outstanding Loans
                listDisplay.Add(new Snippet("--- Loans", RLColor.Yellow, RLColor.Black));
                if (capital.GetNumOfLoans() > 0)
                {
                    amount = Game.constant.GetValue(Global.LOAN_AMOUNT);
                    List<Finance> listOfLoans = capital.GetLoans();
                    for (int i = 0; i < listOfLoans.Count; i++)
                    { listDisplay.Add(new Snippet($"Loan with the {listOfLoans[i]} for {amount:N0} Gold")); }
                }
                else { listDisplay.Add(new Snippet("No Loans are currently outstanding")); }
                //display data
                Game.infoChannel.SetInfoList(listDisplay, ConsoleDisplay.Multi);
            }
            else { Game.SetError(new Error(309, "Invalid Capital (null) -> no rel's shown")); }
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
            bool royalAdvisor = false;
            string actorType;
            if (dictAllActors.TryGetValue(actorID, out person))
            {
                int locID = person.LocID;
                int refID = Game.world.ConvertLocToRef(locID);
                House house = Game.world.GetHouse(refID);
                //Set up people types
                Player player = null;
                Active active = null;
                Advisor advisor = null;
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
                if (person is Advisor)
                {
                    advisor = person as Advisor;
                    actorType = Game.world.GetAdvisorType((Advisor)person);
                    if (advisor.advisorRoyal > AdvisorRoyal.None) { royalAdvisor = true; }
                }
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
                            Position pos = person.GetPosition();
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
                    if (royalAdvisor == false && person.Type != ActorType.Lord && person is Passive || person is Inquisitor && person.Status != ActorStatus.Gone)
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
                    //with Lord (exclude royal advisors)
                    if (royalAdvisor == false)
                    {
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
            }
            else
            { listToDisplay.Add(new Snippet(string.Format("No Character with ID {0} exists", actorID), RLColor.LightRed, RLColor.Black)); }
            return listToDisplay;
        }


        /// <summary>
        /// Method to switch info call 
        /// </summary>
        /// <param name="act"></param>
        public List<Snippet> ShowPlayerMinionsRL(Act act)
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            if (act == Act.One) { listToDisplay.AddRange(ShowActiveActorsRL()); }
            else if (act == Act.Two) { listToDisplay.AddRange(ShowInquisitorsRL()); }
            return listToDisplay;
        }

        /// <summary>
        /// Returns a list of characters in string format to pass to InfoChannel to display in multi-Console
        /// </summary>
        /// <returns>List with info on each character a single, sequential, entry in the list</returns>
        /// <param name="locationsOnly">If true only show those at Locations, default is show all</param>
        public List<Snippet> ShowActiveActorsRL(bool locationsOnly = false)
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            Dictionary<int, Active> dictActiveActors = Game.world.GetAllActiveActors();
            listToDisplay.Add(new Snippet("--- Player Characters", RLColor.Yellow, RLColor.Black));
            int chance, locID;
            ActorStatus status;
            string locName, concealText, coordinates, distText, crowText;
            string locStatus = "who knows?";
            string charString; //overall string
            RLColor textColor = RLColor.White;
            //loop actors
            foreach (var actor in dictActiveActors)
            {
                status = actor.Value.Status;
                concealText = "";
                //ignore dead actors
                if (status != ActorStatus.Gone)
                {
                    locID = actor.Value.LocID;
                    locName = Game.world.GetLocationName(locID);
                    switch (status)
                    {
                        case ActorStatus.AtLocation:
                            locStatus = $"At {locName}";
                            if (actor.Value is Player)
                            {
                                Player player = actor.Value as Player;
                                if (player.Conceal == ActorConceal.SafeHouse)
                                { concealText = string.Format("At SafeHouse ({0} star{1})", player.ConcealLevel, player.ConcealLevel != 1 ? "s" : ""); }
                                else if (player.Conceal == ActorConceal.Disguise)
                                { concealText = string.Format("In Disguise ({0} star{1})", player.ConcealLevel, player.ConcealLevel != 1 ? "s" : ""); }
                            }
                            break;
                        case ActorStatus.Travelling:
                            locStatus = string.Format("{0} to {1}", actor.Value.Travel == TravelMode.Mounted ? "Riding" : "Walking", locName);
                            if (actor.Value is Player)
                            {
                                Player player = actor.Value as Player;
                                if (player.Conceal == ActorConceal.Disguise)
                                { concealText = string.Format("In Disguise ({0} star{1})", player.ConcealLevel, player.ConcealLevel != 1 ? "s" : ""); }
                                else if (player.Travel == TravelMode.Mounted)
                                { concealText = Game.display.GetStars(player.HorseHealth); }
                            }
                            break;
                        case ActorStatus.AtSea:
                            locStatus = string.Format("On a ship to {0}, arriving in {1} day{2}", locName, actor.Value.VoyageTimer, actor.Value.VoyageTimer != 1 ? "s" : "");
                            break;
                        case ActorStatus.Adrift:
                            locStatus = string.Format("Adrift in {0}. Survival time {1} day{2}", actor.Value.SeaName, actor.Value.DeathTimer, actor.Value.DeathTimer != 1 ? "s" : "");
                            break;
                        case ActorStatus.Captured:
                            locStatus = string.Format("Held at {0}. Survival time {1} day{2}", locName, actor.Value.DeathTimer, actor.Value.DeathTimer != 1 ? "s" : "");
                            break;
                    }
                    //only show chosen characters (at Location or not depending on parameter)
                    if (locationsOnly == true && status == ActorStatus.AtLocation || !locationsOnly)
                    {
                        Position pos = actor.Value.GetPosition();
                        coordinates = string.Format("(Loc {0}:{1})", pos.PosX, pos.PosY);
                        if (actor.Value is Player)
                        {
                            //player (no distance display)
                            textColor = Color._player;
                            switch (actor.Value.Status)
                            {
                                case ActorStatus.AtLocation:
                                case ActorStatus.Travelling:
                                    charString = string.Format("Aid {0,-2} {1,-18} {2,-30}{3,-15} {4,-11}", actor.Key, actor.Value.Name, locStatus, coordinates, concealText);
                                    break;
                                case ActorStatus.AtSea:
                                case ActorStatus.Adrift:
                                case ActorStatus.Captured:
                                    charString = string.Format("Aid {0,-2} {1,-18} {2,-30}", actor.Key, actor.Value.Name, locStatus);
                                    break;
                                default:
                                    charString = "Unknown Actor.Value.Status";
                                    break;
                            }
                        }
                        else
                        {
                            if (actor.Value.Delay == 0)
                            {
                                if (actor.Value.Activated == true) { textColor = Color._active; }
                                else { textColor = RLColor.White; }
                            }
                            else { textColor = RLColor.LightGray; }
                            //distance = Game.utility.GetDistance(posPlayer.PosX, posPlayer.PosY, pos.PosX, pos.PosY);
                            distText = string.Format("{0} {1}", "dist:", actor.Value.CrowDistance);
                            chance = actor.Value.CrowChance + actor.Value.CrowBonus;
                            chance = Math.Min(100, chance);
                            crowText = string.Format("{0} {1}{2}", "crow:", chance, "%");
                            charString = string.Format("Aid {0,-2} {1,-18} {2,-30}{3,-15} {4,-11} {5,-12} {6,-12}", actor.Key, actor.Value.Name, locStatus, coordinates, distText, crowText,
                                actor.Value.Known == true ? "Known" : "Unknown");
                        }
                        listToDisplay.Add(new Snippet(charString, textColor, RLColor.Black));
                    }
                }
            }
            return listToDisplay;
        }

        /// <summary>
        /// Display Inquisitors (Act Two)
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowInquisitorsRL()
        {
            List<Snippet> listToDisplay = new List<Snippet>();
            Dictionary<int, Enemy> dictEnemyActors = Game.world.GetAllEnemyActors();
            listToDisplay.Add(new Snippet("--- Inquisitors", RLColor.Yellow, RLColor.Black));
            int locID;
            ActorStatus status;
            string locName, coordinates/*concealText, distText, ,  crowText*/;
            string locStatus = "Unknown";
            string charString; //overall string
            RLColor textColor = RLColor.White;
            foreach (var enemy in dictEnemyActors)
            {
                if (enemy.Value.GoodEnemy == true)
                {
                    status = enemy.Value.Status;
                    //ignore dead actors
                    if (status != ActorStatus.Gone)
                    {
                        locID = enemy.Value.LocID;
                        locName = Game.world.GetLocationName(locID);
                        switch (status)
                        {
                            case ActorStatus.AtLocation:
                                locStatus = $"At {locName}";
                                break;
                            case ActorStatus.Travelling:
                                locStatus = string.Format("Riding to {0}", locName);
                                break;
                        }
                        Position pos = enemy.Value.GetPosition();
                        coordinates = string.Format("(Loc {0}:{1})", pos.PosX, pos.PosY);
                        /*distText = string.Format("{0} {1}", "dist:", enemy.Value.CrowDistance);
                        chance = enemy.Value.CrowChance + enemy.Value.CrowBonus;
                        chance = Math.Min(100, chance);
                        crowText = string.Format("{0} {1}{2}", "crow:", chance, "%");
                        charString = string.Format("Aid {0,-2} {1,-18} {2,-30}{3,-15} {4,-11} {5,-12} {6,-12}", enemy.Key, enemy.Value.Name, locStatus, coordinates, distText, crowText,
                            enemy.Value.Known == true ? "Known" : "Unknown");*/
                        charString = string.Format("Aid {0,-2} {1,-25} {2,-30}{3,-15}", enemy.Key, enemy.Value.Name, locStatus, coordinates);
                        listToDisplay.Add(new Snippet(charString, textColor, RLColor.Black));
                    }
                }
            }
            return listToDisplay;
        }

        /// <summary>
        /// Display a list of all enemies
        /// </summary>
        /// <param name="debugMode">If true shows actual data, if false shows only what the Player knows</param>
        /// <returns></returns>
        public List<Snippet> ShowEnemiesRL(bool debugMode = false)
        {
            Dictionary<int, Enemy> dictEnemyActors = Game.world.GetAllEnemyActors();
            ActorStatus status;
            int locID;
            int expire = Game.constant.GetValue(Global.KNOWN_REVERT);
            string locName = "Unknown";
            string coordinates = "Unknown";
            string charString = "Unknown";
            string locStatus = "Unknown";
            List<Snippet> listInquistors = new List<Snippet>();
            List<Snippet> listOthers = new List<Snippet>(); //all other enemies
            List<Snippet> listToDisplay = new List<Snippet>();
            //loop dict of enemies
            foreach (var enemy in dictEnemyActors)
            {
                status = enemy.Value.Status;
                locID = enemy.Value.LocID;
                locName = Game.world.GetLocationName(locID);
                //known
                if (enemy.Value.Known == true || debugMode == true)
                {
                    if (status == ActorStatus.AtLocation)
                    {
                        string activity = "?";
                        switch (enemy.Value.Goal)
                        {
                            case ActorAIGoal.Hide:
                                activity = "Hiding";
                                break;
                            case ActorAIGoal.Search:
                                activity = "Searching";
                                break;
                            case ActorAIGoal.Wait:
                                activity = "Waiting";
                                break;
                        }
                        locStatus = activity + " at " + locName;
                    }
                    else if (status == ActorStatus.Travelling)
                    { locStatus = "Moving to " + locName; }
                }
                //unknown
                else
                {
                    locID = enemy.Value.LastKnownLocID;
                    locName = Game.world.GetLocationName(locID);
                    if (enemy.Value.LastKnownGoal == ActorAIGoal.Move)
                    { locStatus = "Moving to " + locName; }
                    else
                    {
                        string activity = "?";
                        switch (enemy.Value.LastKnownGoal)
                        {
                            case ActorAIGoal.Hide:
                                activity = "Hiding";
                                break;
                            case ActorAIGoal.Search:
                                activity = "Searching";
                                break;
                            case ActorAIGoal.Wait:
                                activity = "Waiting";
                                break;
                        }
                        locStatus = activity + " at " + locName;
                    }
                }
                //get location coords
                Location loc = Game.network.GetLocation(locID);
                if (loc != null) { coordinates = string.Format("(Pos {0}:{1})", loc.GetPosX(), loc.GetPosY()); }
                else { Game.SetError(new Error(184, "Invalid Loc (null) default Cordinate text used")); }
                //split enemies into two lists for display purposes
                if (enemy.Value is Inquisitor)
                {
                    //Inquisitor
                    if (enemy.Value.Known == true || debugMode == true)
                    {
                        //known status
                        if (debugMode == true)
                        {
                            charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15} {4,-12}  Goal -> {5,-8} Branch -> {6}", enemy.Key, enemy.Value.Name, locStatus, coordinates,
                              enemy.Value.Known == true ? "Known" : "Unknown", enemy.Value.Goal, enemy.Value.AssignedBranch);
                        }
                        else { charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15}", enemy.Key, enemy.Value.Name, locStatus, coordinates); }
                        listInquistors.Add(new Snippet(charString, RLColor.White, RLColor.Black));
                    }
                    else
                    {
                        if (enemy.Value.TurnsUnknown <= expire)
                        {
                            //unknown status and info is 'x' turns or less old
                            charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15} {4} day{5} old information", enemy.Key, enemy.Value.Name, locStatus, coordinates, enemy.Value.TurnsUnknown,
                                enemy.Value.TurnsUnknown == 1 ? "" : "s");
                            listInquistors.Add(new Snippet(charString, RLColor.LightRed, RLColor.Black));
                        }
                        else
                        {
                            //unknown status and beyond the time horizon
                            charString = string.Format("Aid {0,-3} {1,-23} Whereabouts unknown", enemy.Key, enemy.Value.Name);
                            listInquistors.Add(new Snippet(charString, RLColor.LightGray, RLColor.Black));
                        }
                    }
                }
                else
                {
                    //All other Enemies
                    if (enemy.Value.Known == true || debugMode == true)
                    {
                        //known status
                        if (debugMode == true)
                        {
                            charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15} {4,-12}  Goal -> {5,-8} Branch -> {6}", enemy.Key, enemy.Value.Name, locStatus, coordinates,
                              enemy.Value.Known == true ? "Known" : "Unknown", enemy.Value.Goal, enemy.Value.AssignedBranch);
                        }
                        else
                        { charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15}", enemy.Key, enemy.Value.Name, locStatus, coordinates); }
                        listOthers.Add(new Snippet(charString, RLColor.White, RLColor.Black));
                    }
                    else
                    {
                        if (enemy.Value.TurnsUnknown <= expire)
                        {
                            //unknown status and info is 'x' turns or less old
                            charString = string.Format("Aid {0,-3} {1,-23} {2,-35}{3,-15} {4} day{5} old information", enemy.Key, enemy.Value.Name, locStatus, coordinates, enemy.Value.TurnsUnknown,
                                enemy.Value.TurnsUnknown == 1 ? "" : "s");
                            listOthers.Add(new Snippet(charString, RLColor.LightRed, RLColor.Black));
                        }
                        else
                        {
                            //unknown status and beyond the time horizon
                            charString = string.Format("Aid {0,-3} {1,-23} Whereabouts unknown", enemy.Key, enemy.Value.Name);
                            listOthers.Add(new Snippet(charString, RLColor.LightGray, RLColor.Black));
                        }
                    }
                }
            }
            //set up display list
            if (listInquistors.Count > 0)
            {
                listToDisplay.Add(new Snippet("--- Inquisitors", RLColor.Yellow, RLColor.Black));
                for (int i = 0; i < listInquistors.Count; i++)
                { listToDisplay.Add(listInquistors[i]); }
            }
            if (listOthers.Count > 0)
            {
                listToDisplay.Add(new Snippet(""));
                listToDisplay.Add(new Snippet("--- Other Enemies", RLColor.Yellow, RLColor.Black));
                for (int i = 0; i < listOthers.Count; i++)
                { listToDisplay.Add(listOthers[i]); }
            }
            return listToDisplay;
        }


        /// <summary>
        /// used to display character data when first selected by a # key in main menu
        /// </summary>
        /// <param name="inputConsole"></param>
        /// <param name="consoleDisplay"></param>
        /// <param name="charID"></param>
        public Snippet ShowSelectedActor(int charID)
        {
            Dictionary<int, Active> dictActiveActors = Game.world.GetAllActiveActors();
            string returnText = "Character NOT KNOWN";
            //find in dictionary
            if (dictActiveActors.ContainsKey(charID))
            {
                Actor person = dictActiveActors[charID];
                Position pos = person.GetPosition();
                returnText = $"{person.Name} at {Game.world.GetLocationName(person.LocID)} ({pos.PosX}:{pos.PosY}) has been selected";
            }
            return new Snippet(returnText);
        }


        /// <summary>
        /// click on a location to get info
        /// </summary>
        /// <param name="pos"></param>
        internal List<Snippet> ShowLocationRL(int locID, int mouseX, int mouseY)
        {
            int relFriends = Game.constant.GetValue(Global.FRIEND_THRESHOLD);
            int relEnemies = Game.constant.GetValue(Global.ENEMY_THRESHOLD);
            int numFriends = 0; int numEnemies = 0; int relPlyr = 0;
            Dictionary<int, Actor> dictAllActors = Game.world.GetAllActors();
            List<Snippet> locList = new List<Snippet>();
            //Location display
            if (locID > 0)
            {
                string description = "House";
                RLColor highlightColor = RLColor.Cyan;
                RLColor unknownColor = RLColor.LightGray;
                RLColor knownColor = RLColor.White;
                RLColor displayColor;
                bool houseCapital = false;
                Location loc = Game.network.GetLocation(locID);
                if (loc != null)
                {
                    House house = Game.world.GetHouse(loc.RefID);
                    if (!(house is CapitalHouse))
                    {
                        //ignore the capital and special locations for the moment until they are included in dictAllHouses
                        if (house != null)
                        {
                            int eventCount = house.GetNumFollowerEvents();
                            if (house.Special == HouseSpecial.None && loc.HouseID != 99)
                            {
                                int resources = house.Resources;
                                //normal houses - major / minor / capital 
                                locList.Add(new Snippet(string.Format("House {0} of {1}, Lid {2}, Rid {3}, Hid {4}, Branch {5}", house.Name, loc.LocName, loc.LocationID, loc.RefID,
                                    loc.HouseID, loc.GetBranch()), highlightColor, RLColor.Black));
                                locList.Add(new Snippet(string.Format("Motto \"{0}\"", house.Motto)));
                                locList.Add(new Snippet(string.Format("Banner \"{0}\"", house.Banner)));
                                locList.Add(new Snippet(string.Format("Seated at {0} {1}", house.LocName, Game.world.GetLocationCoords(locID))));
                                RLColor loyaltyColor = Color._goodTrait;
                                if (house.Loyalty_Current == KingLoyalty.New_King) { loyaltyColor = Color._badTrait; }
                                locList.Add(new Snippet(string.Format("Loyal to the {0}", house.Loyalty_Current), loyaltyColor, RLColor.Black));

                                if (house is MajorHouse)
                                {
                                    MajorHouse majorHouse = house as MajorHouse;
                                    locList.Add(new Snippet($"House {majorHouse.Name} has {majorHouse.GetNumBannerLords()} BannerLords"));
                                }
                                //population and food inof
                                string foodInfo = $"Population: {house.Population:N0} Harvest: {house.FoodCapacity:N0} Food Balance: {house.FoodCapacity - house.Population:N0} Granary: {house.FoodStockpile:N0}";
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.Food) == true) { displayColor = knownColor; }
                                locList.Add(new Snippet(foodInfo, displayColor, RLColor.Black));
                                //safehouse info
                                if (house.SafeHouse > 0)
                                {
                                    displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.SafeHouse) == true) { displayColor = knownColor; }
                                    locList.Add(new Snippet($"Safe House available ", displayColor, RLColor.Black, false));
                                    displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.SafeHouse) == true) { displayColor = RLColor.LightRed; }
                                    locList.Add(new Snippet(string.Format("{0}", Game.display.GetStars(house.SafeHouse)), displayColor, RLColor.Black));
                                }
                                //castle wall info
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.CastleWalls) == true) { displayColor = knownColor; }
                                locList.Add(new Snippet(string.Format("Strength of Castle Walls ({0}) ", (CastleDefences)house.CastleWalls), displayColor, RLColor.Black, false));
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.CastleWalls) == true) { displayColor = RLColor.LightRed; }
                                locList.Add(new Snippet(string.Format("{0}", Game.display.GetStars((int)house.CastleWalls)), displayColor, RLColor.Black));
                                //resources info
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.Resources) == true) { displayColor = knownColor; }
                                locList.Add(new Snippet(string.Format("House Resources ({0}) ", (ResourceLevel)resources), displayColor, RLColor.Black, false));
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.Resources) == true) { displayColor = RLColor.LightRed; }
                                locList.Add(new Snippet(string.Format("{0}", Game.display.GetStars((int)resources)), displayColor, RLColor.Black));
                                //Men At Arms info
                                displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.Military) == true) { displayColor = knownColor; }
                                locList.Add(new Snippet($"Men At Arms {house.MenAtArms:N0}", displayColor, RLColor.Black));
                                //archetype info
                                if (eventCount > 0)
                                {
                                    locList.Add(new Snippet(string.Format("Archetype \"{0}\" with {1} events", Game.director.GetArchetypeName(house.ArcID), eventCount),
                                        RLColor.LightGray, RLColor.Black));
                                }
                            }
                            else if (house.Special == HouseSpecial.Inn)
                            {
                                //special Inn
                                locList.Add(new Snippet(string.Format("{0} Inn, LocID {1}, RefID {2}, Branch {3}", house.Name, loc.LocationID, loc.RefID, loc.GetBranch()), highlightColor, RLColor.Black));
                                locList.Add(new Snippet(string.Format("Motto \"{0}\"", house.Motto)));
                                locList.Add(new Snippet(string.Format("Signage \"{0}\"", house.Banner)));
                                locList.Add(new Snippet(string.Format("Found at {0}", Game.world.GetLocationCoords(locID))));
                                if (eventCount > 0)
                                {
                                    locList.Add(new Snippet(string.Format("Archetype \"{0}\" with {1} events", Game.director.GetArchetypeName(house.ArcID), eventCount),
                                      RLColor.LightGray, RLColor.Black));
                                }
                            }
                        }
                        //correct location description
                        if (loc.HouseID == 99)
                        { description = "A homely Inn"; }
                        else if (loc.LocationID == 1)
                        { description = loc.LocName + ": the Home of the King"; }
                        else if (Game.map.GetMapInfo(MapLayer.Capitals, loc.GetPosX(), loc.GetPosY()) == 0)
                        { description = "BannerLord of House"; }
                        //bannerlord details if applicable
                        if (houseCapital == false)
                        {
                            string locDetails = string.Format("{0} {1}", description, Game.world.GetMajorHouseName(loc.HouseID));
                            locList.Add(new Snippet(locDetails));
                        }
                    }
                    if (loc.isCapital == true)
                    {
                        CapitalHouse capital = house as CapitalHouse;
                        locList.Add(new Snippet("KINGDOM CAPITAL", RLColor.Yellow, RLColor.Black));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.CastleWalls) == true) { displayColor = knownColor; }
                        locList.Add(new Snippet($"Strength of Castle Walls {(CastleDefences)capital.CastleWalls} ", displayColor, RLColor.Black, false));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.CastleWalls) == true) { displayColor = RLColor.LightRed; }
                        locList.Add(new Snippet($"{Game.display.GetStars(capital.CastleWalls)}", displayColor, RLColor.Black));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.Resources) == true) { displayColor = knownColor; }
                        locList.Add(new Snippet($"House Resources {(ResourceLevel)capital.Resources} ", displayColor, RLColor.Black, false));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.Resources) == true) { displayColor = RLColor.LightRed; }
                        locList.Add(new Snippet($"{Game.display.GetStars(capital.Resources)}", displayColor, RLColor.Black));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.Military) == true) { displayColor = knownColor; }
                        locList.Add(new Snippet($"Men At Arms {capital.MenAtArms:N0}", displayColor, RLColor.Black));
                        displayColor = unknownColor; if (capital.GetInfoStatus(HouseInfo.Food) == true) { displayColor = knownColor; }
                        description = $"Population: {house.Population:N0} Harvest: {house.FoodCapacity:N0} Food Balance: {house.FoodCapacity - house.Population:N0} Granary: {house.FoodStockpile:N0}";
                        locList.Add(new Snippet(description, displayColor, RLColor.Black));
                    }
                    //imports & exports
                    if (house.GetNumImports() > 0 || house.GetNumExports() > 0)
                    {
                        int upper, sumGoods;
                        bool newLine;
                        //Imports
                        int numGoods = house.GetNumImports();
                        if (numGoods > 0)
                        {
                            sumGoods = 0;
                            locList.Add(new Snippet("Imports -> ", false));
                            int[,] tempImports = house.GetImports();
                            upper = tempImports.GetUpperBound(0);
                            for (int i = 0; i <= upper; i++)
                            {
                                //at least one present and is known
                                if (tempImports[i, 0] > 0)
                                {
                                    sumGoods++;
                                    displayColor = unknownColor;
                                    if (tempImports[i, 1] > 0) { displayColor = knownColor; }
                                    newLine = true;
                                    if (sumGoods < numGoods) { newLine = false; }
                                    locList.Add(new Snippet($"{(Goods)i} x {tempImports[i, 0]} ", displayColor, RLColor.Black, newLine));
                                }
                            }
                        }
                        //Exports
                        numGoods = house.GetNumExports();
                        if (numGoods > 0)
                        {
                            sumGoods = 0;
                            locList.Add(new Snippet("Exports -> ", false));
                            int[,] tempExports = house.GetExports();
                            upper = tempExports.GetUpperBound(0);
                            for (int i = 0; i <= upper; i++)
                            {
                                //at least one present and is known
                                if (tempExports[i, 0] > 0)
                                {
                                    sumGoods++;
                                    displayColor = unknownColor;
                                    if (tempExports[i, 1] > 0) { displayColor = knownColor; }
                                    newLine = true;
                                    if (sumGoods < numGoods) { newLine = false; }
                                    locList.Add(new Snippet($"{(Goods)i} x {tempExports[i, 0]} ", displayColor, RLColor.Black, newLine));
                                }
                            }
                        }
                    }
                    if (loc.Connector == true)
                    { locList.Add(new Snippet("CONNECTOR", RLColor.Red, RLColor.Black)); }
                    if (loc.isPort == true)
                    {
                        int numPorts = loc.GetNumConnectedPorts();
                        locList.Add(new Snippet(string.Format("PORT, connected to {0} other port{1}. Sea Passages available.", numPorts, numPorts != 1 ? "s" : ""), RLColor.LightRed, RLColor.Black));
                    }
                    //characters at location
                    List<int> charList = loc.GetActorList();
                    List<string> listAtLocation = new List<string>();
                    List<string> listInDungeon = new List<string>();
                    charList.Sort();
                    if (charList.Count > 0)
                    {
                        RLColor textColor = RLColor.White;
                        int row = 3;
                        
                        string actorDetails = "";
                        string actorType;
                        foreach (int charID in charList)
                        {
                            row++;
                            if (dictAllActors.ContainsKey(charID))
                            {
                                textColor = RLColor.White;
                                //Actor person = new Actor();
                                Actor person = dictAllActors[charID];
                                //advisors can be one of three different categories
                                if (person is Advisor) { actorType = Game.world.GetAdvisorType((Advisor)person); }
                                else { actorType = Convert.ToString(person.Type); }
                                if ((int)person.Office > 0)
                                { actorType = Convert.ToString(person.Office); }
                                actorDetails = string.Format("Aid {0} {1} {2}, age {3}", person.ActID, actorType, person.Name, person.Age);
                                //player controlled (change color of text)?
                                if (person is Active)
                                {
                                    if (person is Player)
                                    { textColor = Color._player; }
                                    else
                                    { textColor = Color._active; }
                                }
                                //tally friends and enemies
                                if (person.ActID > 1)
                                {
                                    relPlyr = person.GetRelPlyr();
                                    if (relPlyr >= relFriends) { numFriends++; }
                                    else if (relPlyr <= relEnemies) { numEnemies++; }
                                }
                                if (person.Status == ActorStatus.AtLocation) { listAtLocation.Add(actorDetails); }
                                else if (person.Status == ActorStatus.Captured) { listInDungeon.Add(actorDetails); }
                                else { Game.SetError(new Error(187, $"Invalid person.Status \"{person.Status}\"")); }
                            }
                            else { Game.SetError(new Error(187, $"Invalid ActorID \"{charID}\"")); }
                        }
                            //At Location
                            locList.Add(new Snippet(string.Format("Characters at {0}", loc.LocName), RLColor.Brown, RLColor.Black));
                            if (listAtLocation.Count > 0)
                            {
                                for (int i = 0; i < listAtLocation.Count; i++)
                                { locList.Add(new Snippet(listAtLocation[i], textColor, RLColor.Black)); }
                            }
                            else { locList.Add(new Snippet("None present", textColor, RLColor.Black)); }
                            //In Dungeon
                            if (listInDungeon.Count > 0)
                            {
                                locList.Add(new Snippet(string.Format("Characters in the {0} Dungeons", loc.LocName), RLColor.LightRed, RLColor.Black));
                                for (int i = 0; i < listInDungeon.Count; i++)
                                { locList.Add(new Snippet(listInDungeon[i], textColor, RLColor.Black)); }
                            }
                        //display friends and enemies
                        if (numFriends > 0 || numEnemies > 0)
                        {
                            displayColor = unknownColor; if (house.GetInfoStatus(HouseInfo.FriendsEnemies) == true) { displayColor = knownColor; }
                            locList.Add(new Snippet(string.Format("Current Standing at {0}", loc.LocName), RLColor.Brown, RLColor.Black));
                            locList.Add(new Snippet(string.Format("You have {0} Friend{1} and {2} Enem{3} here", numFriends, numFriends != 1 ? "s" : "", numEnemies,
                                numEnemies != 1 ? "ies" : "y"), displayColor, RLColor.Black));
                        }
                    }
                }
                else { Game.SetError(new Error(187, "Invalid Loc (null)")); }
            }
            else if (locID == 0)
            {
                //Non-Location -> Terrain
                int geoID = Game.map.GetMapInfo(MapLayer.GeoID, mouseX, mouseY, true);
                int numEvents;
                //geo sea zone or terrain cluster present?
                if (geoID > 0)
                {
                    GeoCluster cluster = Game.world.GetGeoCluster(geoID);
                    if (cluster != null)
                    {
                        locList.Add(new Snippet(string.Format("{0}, geoID {1}", cluster.Name, cluster.GeoID), RLColor.Yellow, RLColor.Black));
                        locList.Add(new Snippet(cluster.Description));
                        locList.Add(new Snippet(string.Format("Size {0}, Terrain {1}, Type {2}", cluster.Size, cluster.Terrain, cluster.Type)));
                        numEvents = cluster.GetNumFollowerEvents() + cluster.GetNumPlayerEvents();
                        if (numEvents > 0)
                        { locList.Add(new Snippet(string.Format("Archetype \"{0}\" with {1} Events", Game.director.GetArchetypeName(cluster.Archetype), numEvents))); }
                    }
                    else
                    { locList.Add(new Snippet(string.Format("ERROR: GeoCluster couldn't be found for geoID {0}", geoID), RLColor.Red, RLColor.Black)); }
                }
                //nothing there apart from plains
                else
                { locList.Add(new Snippet("ERROR: There is no Location present here", RLColor.Red, RLColor.Black)); }

            }
            else
            { locList.Add(new Snippet("ERROR: Please click on the map", RLColor.Red, RLColor.Black)); }
            return locList;
        }


        /// <summary>
        /// Generate a list of All Messages
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowMessagesRL()
        {
            List<Snippet> tempList = new List<Snippet>();
            RLColor color = RLColor.White;
            Dictionary<int, Message> dictMessages = Game.world.GetMessages();
            foreach (var message in dictMessages)
            {
                if (message.Value.Type == MessageType.Activation) { color = Color._active; }
                else { color = RLColor.White; }
                tempList.Add(new Snippet(string.Format("Day {0}, {1}, {2}", message.Value.Day, message.Value.Year, message.Value.Text), color, RLColor.Black));
            }
            return tempList;
        }

        /// <summary>
        /// return 8 most recent messages to display at bottom right console window
        /// </summary>
        /// <returns></returns>
        public List<Snippet> ShowRecentMessagesRL()
        {
            List<Snippet> tempList = new List<Snippet>();
            Dictionary<int, Message> dictMessages = Game.world.GetMessages();
            tempList.Add(new Snippet("--- Message Log Recent", RLColor.Yellow, RLColor.Black));
            tempList.AddRange(Game.world.GetMessageQueue());
            return tempList;
        }


        public List<Snippet> ShowFoodRL(FoodInfo mode)
        {
            List<Snippet> tempList = new List<Snippet>();
            if (mode != FoodInfo.None)
            {
                List<String> listOfFoodInfo = Game.world.GetFoodInfo(mode);
                tempList.Add(new Snippet($"--- {mode} Food Info", RLColor.Yellow, RLColor.Black));
                if (listOfFoodInfo.Count > 0)
                {
                    foreach (var text in listOfFoodInfo)
                    { tempList.Add(new Snippet(text)); }
                }
                else { tempList.Add(new Snippet($"No records have been returned for FoodInfo mode \"{mode}\"", RLColor.LightRed, RLColor.Black)); }
            }
            else { tempList.Add(new Snippet("ERROR -> Invalid FoodInfo mode provided", RLColor.LightRed, RLColor.Black)); }
            return tempList;
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
            num = Math.Max(0, num);
            for (int i = 0; i < num; i++)
            { stars += "o "; }
            return stars;
        }
        //methods above here
    }
}
