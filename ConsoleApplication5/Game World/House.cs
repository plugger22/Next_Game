using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using System.Linq;


namespace Next_Game
{
    public enum KingLoyalty { None, Old_King, New_King }
    public enum HouseSpecial { None, Inn }
    public enum CastleDefences { None, Minimal, Weak, Average, Strong, Formidable }
    public enum HouseInfo { None, Resources, CastleWalls, FriendsEnemies, Food, SafeHouse, History, Military, Count} //toggles information display on/off depending on known status
    public enum FinArray { Data, Status, Rate, Reference, Constant, Count } //second index for CapitalHouse finance arrays
    public enum Account { None, Lender, Income, Expense, LumpSum, FinSummary} //which CapitalHouse financial array are we referring to?

    //
    // Base class
    //
    class House
    {
        public string Name { get; set; }
        public string Motto { get; set; }
        public string Banner { get; set; }
        public string LocName { get; set; }
        public int HouseID { get; set; } = 0; //unique to Great House (allocated by Network.UpdateHouses)
        public int LocID { get; set; } //unique to location
        public int RefID { get; set; } //unique to house (great or minor)
        public int ArcID { get; set; } //unique archetype for house (specified in House import files)
        public int Branch { get; set; } //branch # House on -> 0 is Capital, 1 is North, 2 is East, 3 is South, 4 is West
        public int MenAtArms { get; set; }
        public int Population { get; set; }
        public int CastleWalls { get; set; } //strength of castle walls (1 to 5)
        public int LordID { get; set; } //actorID of noble Lord currently in charge of house
        public int Resources { get; set; } //same as actor resources (0 to 5). Head of house has access to this level of resources.
        public int SafeHouse { get; set; } //Safe House present if > 0. Number represents how many stars (max 5)
        public int FoodCapacity { get; set; } //the amount of food that the house can harvest from the surrounding terrain at the end of the growing season
        public int FoodStockpile { get; set; } //the amount of food that is stockpiled at the house loc for winter
        public bool ObserveFlag { get; set; } //True if the player has already used the "observe" action at this location
        public LocType Type { get; set; }
        public KingLoyalty Loyalty_AtStart { get; set; }
        public KingLoyalty Loyalty_Current { get; set; }
        public HouseSpecial Special { get; set; } //used for quick acess to special locations, eg. Inns
        private int[,] arrayOfInfoVis; //toggles variousinformation elements (enum HouseInfo) display on/off -> [0,] (int)HouseInfo [1] Unknown '0', Known '1'
        private int[,] arrayOfExports; //toggles visibility of export goods (enum Goods) and tracks #'s -> [0] # of each good, [1] Unknown '0', Known '1'
        private int[,] arrayOfImports; //as per exports
        private List<int> listOfFirstNames; //contains ID #'s (listOfMaleFirstNames index) of all first names used by males within the house (eg. 'Eddard Stark II')
        private List<int> listOfSecrets;
        private List<int> listOfFollowerEvents;
        private List<int> listOfPlayerEvents;
        private List<int> listOfRumours;
        private List<Relation> listOfRelations; //relationships (records) with other houses (can have multiple relations with another house)
        private Dictionary<int, int> dictCurrentRelations; //current Relationship levels, key is RefID, value is current Rel lvl

        /// <summary>
        /// default constructor
        /// </summary>
        public House()
        {
            listOfFirstNames = new List<int>();
            listOfSecrets = new List<int>();
            listOfRumours = new List<int>();
            listOfFollowerEvents = new List<int>();
            listOfPlayerEvents = new List<int>();
            listOfRelations = new List<Relation>();
            dictCurrentRelations = new Dictionary<int, int>();
            arrayOfInfoVis = new int[(int)HouseInfo.Count, 2];
            arrayOfExports = new int[(int)Goods.Count, 2];
            arrayOfImports = new int[(int)Goods.Count, 2];
            ObserveFlag = false;
        }



        /// <summary>
        /// adds ID to list of names and returns # of like names in list
        /// </summary>
        /// <param name="nameID"></param>
        /// <returns></returns>
        public int AddName(int nameID)
        {
            int numOfLikeNames = 1;
            listOfFirstNames.Add(nameID);
            numOfLikeNames = listOfFirstNames.Count(i => i.Equals(nameID));
            return numOfLikeNames;
        }

        /// <summary>
        /// returns Known (true) or Unknown (false) status for HouseInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool GetInfoStatus(HouseInfo info)
        {
            if (arrayOfInfoVis[(int)info, 1] > 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// add an Export good to the house
        /// </summary>
        /// <param name="good"></param>
        public void AddExport(Goods good)
        { arrayOfExports[(int)good, 0]++; }

        /// <summary>
        /// toggles known/unknown status to on/off for Export Goods
        /// </summary>
        /// <param name="good"></param>
        /// <param name="isKnown"></param>
        public void SetExportStatus(Goods good, bool isKnown = true)
        {
            if (isKnown == true)
            { arrayOfExports[(int)good, 1] = 1; }
            else { arrayOfExports[(int)good, 1] = 0; }
        }

        /// <summary>
        /// returns Known (true) or Unknown (false) status for Export Goods
        /// </summary>
        /// <param name="good"></param>
        /// <returns></returns>
        public bool GetExportStatus(Goods good)
        {
            if (arrayOfExports[(int)good, 1] > 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// returns number of unique Exports (ignores 3 of one type -> treats as one)
        /// </summary>
        /// <returns></returns>
        public int GetNumExports()
        {
            int sumOfExports = 0;
            for (int i = 1; i <= arrayOfExports.GetUpperBound(0); i++)
            { if (arrayOfExports[i, 0] > 0) { sumOfExports++; } }
            return sumOfExports;
        }

        public int[,] GetExports()
        { return arrayOfExports; }

        /// <summary>
        /// add an Import good to the house
        /// </summary>
        /// <param name="good"></param>
        public void AddImport(Goods good)
        { arrayOfImports[(int)good, 0]++; }

        /// <summary>
        /// toggles known/unknown status to on/off for Import Goods
        /// </summary>
        /// <param name="good"></param>
        /// <param name="isKnown"></param>
        public void SetImportStatus(Goods good, bool isKnown = true)
        {
            if (isKnown == true)
            { arrayOfImports[(int)good, 1] = 1; }
            else { arrayOfImports[(int)good, 1] = 0; }
        }

        /// <summary>
        /// returns Known (true) or Unknown (false) status for Import Goods
        /// </summary>
        /// <param name="good"></param>
        /// <returns></returns>
        public bool GetImportStatus(Goods good)
        {
            if (arrayOfImports[(int)good, 1] > 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// returns number of unique Imports (ignores 3 of one type -> treats as one)
        /// </summary>
        /// <returns></returns>
        public int GetNumImports()
        {
            int sumOfImports = 0;
            for (int i = 1; i <= arrayOfImports.GetUpperBound(0); i++)
            { if (arrayOfImports[i, 0] > 0) { sumOfImports++; } }
            return sumOfImports;
        }

        public int[,] GetImports()
        { return arrayOfImports; }

        /// <summary>
        /// toggles known/unknown status to on/off for HouseInfo
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isKnown"></param>
        public void SetInfoStatus(HouseInfo info, bool isKnown = true)
        {
            if (isKnown == true)
            { arrayOfInfoVis[(int)info, 1] = 1; }
            else { arrayOfInfoVis[(int)info, 1] = 0; }
        }

        internal void SetFollowerEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfFollowerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(56, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetFollowerEvents()
        { return listOfFollowerEvents; }

        internal int GetNumFollowerEvents()
        { return listOfFollowerEvents.Count; }

        internal void SetPlayerEvents(List<int> listEvents)
        {
            if (listEvents != null)
            { listOfPlayerEvents.AddRange(listEvents); }
            else
            { Game.SetError(new Error(56, "Invalid list of Events input (null)")); }
        }

        internal List<int> GetPlayerEvents()
        { return listOfPlayerEvents; }

        internal int GetNumPlayerEvents()
        { return listOfPlayerEvents.Count; }

        internal List<int> GetSecrets()
        { return listOfSecrets; }

        internal void SetSecrets(List<int> tempSecrets)
        {
            if (tempSecrets != null)
            { listOfSecrets.Clear(); listOfSecrets.AddRange(tempSecrets); }
            else
            { Game.SetError(new Error(57, "Invalid list of Secrets input (null)")); }
        }

        internal List<int> GetRumours()
        { return listOfRumours; }

        internal void AddRumour(int rumourID)
        {
            if (rumourID > 0)
            { listOfRumours.Add(rumourID); }
            else { Game.SetError(new Error(267, "Invalid rumourID (zero, or less)")); }
        }

        /// <summary>
        /// remove a rumour from the list, returns true if successful, false otherwise
        /// </summary>
        /// <param name="rumourID"></param>
        /// <returns></returns>
        internal bool RemoveRumour(int rumourID)
        { return listOfRumours.Remove(rumourID); }

        /// <summary>
        /// if neg then a food deficit, otherwise a surplus
        /// </summary>
        /// <returns></returns>
        public int GetFoodBalance()
        { return (FoodCapacity - Population); }

        /// <summary>
        /// import a list of Relations and add to House Relations List (auto  updates current Rel dict)
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetRelations(List<Relation> tempList)
        {
            if (tempList != null && tempList.Count > 0)
            {
                listOfRelations.AddRange(tempList);
                //update current rel dictionary
                foreach(var relation in tempList)
                {  UpdateRelations(relation.RefID, relation.Change);  }
            }
            else { Game.SetError(new Error(132, "Invalid List of Relations Input (null or empty)")); }
        }

        /// <summary>
        /// Add a relationship record (auto  updates current Rel dict)
        /// </summary>
        /// <param name="relation"></param>
        internal void AddRelations(Relation relation)
        {
            if (relation != null)
            {
                listOfRelations.Add(relation);
                //update current Rel dict
                UpdateRelations(relation.RefID, relation.Change);
            }
            else { Game.SetError(new Error(135, "Invalid Relation (null)")); }
        }

        internal List<Relation> GetRelations()
        { return listOfRelations; }

        /// <summary>
        /// Return a sorted (recent -> distant) list of Relations that apply to the House with the input refID. Returns list, (count 0), if none
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        internal List<Relation> GetSpecificRelations(int refID) //NOT SURE IF THIS method WORKS
        {
            List<Relation> tempList = new List<Relation>();
            if (listOfRelations.Count > 0)
            {
                IEnumerable<Relation> houseRels =
                    from relation in listOfRelations
                    where relation.RefID == refID
                    orderby relation.TrackerID descending
                    select relation;
                tempList = houseRels.ToList();
            }
            return tempList;
        }

        /// <summary>
        /// updates current rel dict with latest data, creates new entry if refID doesn't exist. All relationships assumed to start at a base of zero. Auto called whenever a new Relationship record is added
        /// </summary>
        /// <param name="refID"></param>
        /// <param name="change"></param>
        private void UpdateRelations(int refID, int change)
        {
            int relLevel = 0;
            if (dictCurrentRelations.ContainsKey(refID))
            {
                relLevel = dictCurrentRelations[refID];
                //update with change amount to current Rel level
                dictCurrentRelations[refID] = relLevel + change;
            }
            else
            {
                //no entry found, create new one
                try
                { dictCurrentRelations.Add(refID, change); }
                catch (ArgumentException)
                { Game.SetError(new Error(241, $"Invalid RefID \"{refID}\" (duplicate record) -> Relationship not updated")); }
            }
        }

        /// <summary>
        /// looks up house in Dictionary and returns current rel level, '0' if not found (neutral)
        /// </summary>
        /// <param name="refID"></param>
        /// <returns></returns>
        internal int GetCurrentRelationship(int refID)
        {
            int relLevel = 0;
            if (dictCurrentRelations.ContainsKey(refID))
            { relLevel = dictCurrentRelations[refID];  }
            return relLevel;
        }

    }
    
    /// <summary>
    /// Capital House (Kingskeep) -> holds all info for King related matters
    /// </summary>
    /// 
    class CapitalHouse : House
    {
        public int CashFlow { get; set; } //net financial balance (income - expenses)
        //public int Balance { get; set; } //total gold available in treasury

        //collections
        private List<Finance> listOfLoans; //who has lent money to the King
        private int[] arrayOfGroups; //relationship levels (0 - 100)  with the different WorldGroups (enum) within the Capital, Kingskeep
        private int[,] arrayOfLenders; //relationship levels (0 - 100) with the different Lenders who you've obtained a loan from
        //all the following use the FinArray as their second index - 0 Amount, 1 Active Item, 2 Rate, 3 Reference, 4 constant value
        private int[,] arrayOfIncome; // Director.cs enum Income (2 Rate is TaxRate)
        private int[,] arrayOfExpenses; // Director.cs enum Expense (2 Rate is budget cost)
        private int[,] arrayOfLumpSums; // Director.cs enum LumpSums
        private int[,] arrayOfFinSummary; //Director.cs enum FinSummary (3 Reference holds previous account period's updated data for Corruption/Appropriations and Loans)

        /// <summary>
        /// default constructor
        /// </summary>
        public CapitalHouse()
        {
            
            listOfLoans = new List<Finance>();
            arrayOfGroups = new int[(int)WorldGroup.Count];
            arrayOfLenders = new int[(int)Finance.Count, (int)FinArray.Count];
            arrayOfIncome = new int[(int)Income.Count, (int)FinArray.Count];
            arrayOfExpenses = new int[(int)Expense.Count, (int)FinArray.Count];
            arrayOfLumpSums = new int[(int)LumpSum.Count, (int)FinArray.Count];
            arrayOfFinSummary = new int[(int)FinSummary.Count, (int)FinArray.Count];
        }

        public void AddLoan(Finance loan)
        {
            if (loan != Finance.None)
            { listOfLoans.Add(loan); }
        }

        public int GetNumOfLoans()
        { return listOfLoans.Count; }

        public List<Finance> GetLoans()
        { return listOfLoans; }

        public void SetGroupRelations(WorldGroup group, int newRelLvl)
        { arrayOfGroups[(int)group] = newRelLvl; }

        /// <summary>
        /// Change by adding an amount to a group relationship
        /// </summary>
        /// <param name="group"></param>
        /// <param name="changeAmt">Adds this amount to existing group relationship level</param>
        public void ChangeGroupRelations(WorldGroup group, int changeAmt)
        {
            arrayOfGroups[(int)group] += changeAmt;
            Game.logTurn?.Write($"[ChangeGroupRelations] {group} relations with King changed by {changeAmt}, now {arrayOfGroups[(int)group]}");
        }

        /// <summary>
        /// returns relationship level of a particular group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int GetGroupRelations(WorldGroup group)
        { return arrayOfGroups[(int)group]; }

        /// <summary>
        /// Set FinArray.Data in any finance array (all indentically sized)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        /// <param name="data"></param>
        public void SetFinanceData(Account account, int index, int data)
        {
            string item = "Unknown";
            switch (account)
            {
                case Account.Lender:
                    arrayOfLenders[index, (int)FinArray.Data] = data;
                    item = Convert.ToString((Finance)index);
                    break;
                case Account.Income:
                    arrayOfIncome[index, (int)FinArray.Data] = data;
                    item = Convert.ToString((Income)index);
                    break;
                case Account.Expense:
                    arrayOfExpenses[index, (int)FinArray.Data] = data;
                    item = Convert.ToString((Expense)index);
                    break;
                case Account.LumpSum:
                    arrayOfLumpSums[index, (int)FinArray.Data] = data;
                    item = Convert.ToString((LumpSum)index);
                    break;
                case Account.FinSummary:
                    arrayOfFinSummary[index, (int)FinArray.Data] = data;
                    item = Convert.ToString((FinSummary)index);
                    break;
                default:
                    Game.SetError(new Error(312, $"Invalid account option \"{account}\" -> data not added to array"));
                    break;
            }
            Game.logTurn?.Write($"[SetFinanceData] {account} {item}, new data -> {data}");
        }

        /// <summary>
        /// Set FinArray.Status in any finance array (all indentically sized)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        /// <param name="status"></param>
        public void SetFinanceStatus(Account account, int index, bool status)
        {
            string item = "Unknown";
            switch (account)
            {
                case Account.Lender:
                    if (status == true) { arrayOfLenders[index, (int)FinArray.Status] = 1; }
                    else { arrayOfLenders[index, (int)FinArray.Status] = 0; }
                    item = Convert.ToString((Finance)index);
                    break;
                case Account.Income:
                    if (status == true) { arrayOfIncome[index, (int)FinArray.Status] = 1; }
                    else { arrayOfIncome[index, (int)FinArray.Status] = 0; }
                    item = Convert.ToString((Income)index);
                    break;
                case Account.Expense:
                    if (status == true) { arrayOfExpenses[index, (int)FinArray.Status] = 1; }
                    else { arrayOfExpenses[index, (int)FinArray.Status] = 0; }
                    item = Convert.ToString((Expense)index);
                    break;
                case Account.LumpSum:
                    if (status == true) { arrayOfLumpSums[index, (int)FinArray.Status] = 1; }
                    else { arrayOfLumpSums[index, (int)FinArray.Status] = 0; };
                    item = Convert.ToString((LumpSum)index);
                    break;
                case Account.FinSummary:
                    if (status == true) { arrayOfFinSummary[index, (int)FinArray.Status] = 1; }
                    else { arrayOfFinSummary[index, (int)FinArray.Status] = 0; };
                    item = Convert.ToString((FinSummary)index);
                    break;
                default:
                    Game.SetError(new Error(313, $"Invalid account option \"{account}\" -> status not updated in array"));
                    break;
            }
            Game.logTurn?.Write($"[SetFinanceStatus] {account} {item}, new status -> {status}");
        }


        /// <summary>
        /// Set FinArray.Rate in any finance array (all indentically sized)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        /// <param name="rate"></param>
        public void SetFinanceRate(Account account, int index, int rate)
        {
            string item = "Unknown";
            switch (account)
            {
                case Account.Lender:
                    arrayOfLenders[index, (int)FinArray.Rate] = rate;
                    item = Convert.ToString((Finance)index);
                    break;
                case Account.Income:
                    arrayOfIncome[index, (int)FinArray.Rate] = rate;
                    item = Convert.ToString((Income)index);
                    break;
                case Account.Expense:
                    arrayOfExpenses[index, (int)FinArray.Rate] = rate;
                    item = Convert.ToString((Expense)index);
                    //handle updates of GameVars where applicable
                    switch ((Expense)index)
                    {
                        case Expense.Inquisitors:
                            //Inquisitor budget allocation (used for AI search routines)
                            Game.variable.SetValue(GameVar.Inquisitor_Budget, rate);
                            break;
                        case Expense.Royal_Lifestyle:
                            //Lifestyle budget allocation (used to adjust King's legend every account period)
                            Game.variable.SetValue(GameVar.Lifestyle_Budget, rate);
                            break;
                        case Expense.Officials_Wages:
                            //Official Wages budget allocation (chance of them extorting money and dropping group relations)
                            Game.variable.SetValue(GameVar.Official_Budget, rate);
                            break;
                    }
                    break;
                case Account.LumpSum:
                    arrayOfLumpSums[index, (int)FinArray.Rate] = rate;
                    item = Convert.ToString((LumpSum)index);
                    break;
                default:
                    Game.SetError(new Error(314, $"Invalid account option \"{account}\" -> rate not updated in array"));
                    break;
            }
            Game.logTurn?.Write($"[SetFinanceRate] {account} {item}, new rate -> {(Rate)rate}");
        }

        /// <summary>
        /// Set FinArray.Reference in any finance array (all indentically sized)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        /// <param name="refData"></param>
        public void SetFinanceReference(Account account, int index, int refData)
        {
            string item = "Unknown";
            switch (account)
            {
                case Account.Lender:
                    arrayOfLenders[index, (int)FinArray.Reference] = refData;
                    item = Convert.ToString((Finance)index);
                    break;
                case Account.Income:
                    arrayOfIncome[index, (int)FinArray.Reference] = refData;
                    item = Convert.ToString((Income)index);
                    break;
                case Account.Expense:
                    arrayOfExpenses[index, (int)FinArray.Reference] = refData;
                    item = Convert.ToString((Expense)index);
                    break;
                case Account.LumpSum:
                    arrayOfLumpSums[index, (int)FinArray.Reference] = refData;
                    item = Convert.ToString((LumpSum)index);
                    break;
                case Account.FinSummary:
                    arrayOfFinSummary[index, (int)FinArray.Reference] = refData;
                    item = Convert.ToString((FinSummary)index);
                    break;
                default:
                    Game.SetError(new Error(315, $"Invalid account option \"{account}\" -> reference not updated in array"));
                    break;
            }
            Game.logTurn?.Write($"[SetFinanceReference] {account} {item}, new reference -> {refData}");
        }

        /// <summary>
        /// Set FinArray.Constant in any finance array (all indentically sized)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        /// <param name="constant"></param>
        public void SetFinanceConstant(Account account, int index, int constant)
        {
            string item = "Unknown";
            switch (account)
            {
                case Account.Lender:
                    arrayOfLenders[index, (int)FinArray.Constant] = constant;
                    item = Convert.ToString((Finance)index);
                    break;
                case Account.Income:
                    arrayOfIncome[index, (int)FinArray.Constant] = constant;
                    item = Convert.ToString((Income)index);
                    break;
                case Account.Expense:
                    arrayOfExpenses[index, (int)FinArray.Constant] = constant;
                    item = Convert.ToString((Expense)index);
                    break;
                case Account.LumpSum:
                    arrayOfLumpSums[index, (int)FinArray.Constant] = constant;
                    item = Convert.ToString((LumpSum)index);
                    break;
                default:
                    Game.SetError(new Error(316, $"Invalid account option \"{account}\" -> constant not updated in array"));
                    break;
            }
            Game.logTurn?.Write($"[SetFinanceConstant] {account} {item}, new constant -> {constant}");
        }

        /// <summary>
        /// Get any FinArray in any finance array (all indentically sized). If Status will return 1 for true, 0 for false
        /// </summary>
        /// <param name="account"></param>
        /// <param name="index">Make sure you use the appropriate enum for this!</param>
        public int GetFinanceInfo(Account account, int index, FinArray type)
        {
            int data = 0;
            switch (account)
            {
                case Account.Lender:
                    data = arrayOfLenders[index, (int)type];
                    break;
                case Account.Income:
                    data = arrayOfIncome[index, (int)type];
                    break;
                case Account.Expense:
                    data = arrayOfExpenses[index, (int)type];
                    break;
                case Account.LumpSum:
                    data = arrayOfLumpSums[index, (int)type];
                    break;
                case Account.FinSummary:
                    data = arrayOfFinSummary[index, (int)type];
                    break;
                default:
                    Game.SetError(new Error(317, $"Invalid account option \"{account}\" -> default info value of Zero returned"));
                    break;
            }
            return data;
        }



        //public void SetLenderRelations(Finance lender, int newRelLvl)
        //{ arrayOfLenders[(int)lender, (int)FinArray.Data] = newRelLvl; }

        /// <summary>
        /// Change by adding an amount to a lender relationship
        /// </summary>
        /// <param name="lender"></param>
        /// <param name="changeAmt">Adds this amount to existing lender relationship level</param>
        /*public void ChangeLenderRelations(Finance lender, int changeAmt)
        { arrayOfLenders[(int)lender, (int)FinArray.Data] += changeAmt; }

        /// <summary>
        /// returns relationship level of a particular lender
        /// </summary>
        /// <param name="lender"></param>
        /// <returns></returns>
        public int GetLenderRelations(Finance lender)
        { return arrayOfLenders[(int)lender, (int)FinArray.Data]; }


        //public void SetIncome(Income income, int amount)
        //{ arrayOfIncome[(int)income, (int)FinArray.Data] = amount; }

        /// <summary>
        /// Adds an amount to the exisitng income
        /// </summary>
        /// <param name="income"></param>
        /// <param name="amount">Amount to Add (pass a negative number to subtract)</param>
       /* public void ChangeIncome(Income income, int amount)
        { arrayOfIncome[(int)income, (int)FinArray.Data] += amount; }

        public int GetIncome(Income income)
        { return arrayOfIncome[(int)income, (int)FinArray.Data]; }

        /// <summary>
        /// Returns true if income item is Active
        /// </summary>
        /// <param name="income"></param>
        /// <returns></returns>
        public bool GetIncomeStatus(Income income)
        { if (arrayOfIncome[(int)income, (int)FinArray.Status] > 0) { return true; } return false; }

        /// <summary>
        /// Set Active status (true/false) for an income item
        /// </summary>
        /// <param name="income"></param>
        /// <param name="status"></param>
        /*public void SetIncomeStatus(Income income, bool status)
        {
            if (status == true) { arrayOfIncome[(int)income, (int)FinArray.Status] = 1; }
            else { arrayOfIncome[(int)income, (int)FinArray.Status] = 0; }
        }

        public void SetIncomeTax(Income income, TaxRate tax)
        { arrayOfIncome[(int)income, (int)FinArray.Rate] = (int)tax; }*/

        /// <summary>
        /// returns TaxRate for income item
        /// </summary>
        /// <param name="income"></param>
        /// <returns></returns>
        /*public Rate GetIncomeTax(Income income)
        { return (Rate)arrayOfIncome[(int)income, (int)FinArray.Rate]; }

        //public void SetIncomeReference(Income income, int refData)
        //{ arrayOfIncome[(int)income, (int)FinArray.Reference] = refData; }

        public int GetIncomeReference(Income income)
        { return arrayOfIncome[(int)income, (int)FinArray.Reference]; }

        //public void SetIncomeConstant(Income income, int conData)
        //{ arrayOfIncome[(int)income, (int)FinArray.Constant] = conData; }

        public int GetIncomeConstant(Income income)
        { return arrayOfIncome[(int)income, (int)FinArray.Constant]; }

        //public void SetExpense(Expense expense, int amount)
        //{ arrayOfExpenses[(int)expense, (int)FinArray.Data] = amount; }

        /// <summary>
        /// Adds an amount to the exisitng expense
        /// </summary>
        /// <param name="income"></param>
        /// <param name="amount">Amount to Add (pass a negative number to subtract)</param>
        public void ChangeExpense(Expense expense, int amount)
        { arrayOfExpenses[(int)expense, (int)FinArray.Data] += amount; }

        public int GetExpense(Expense expense)
        { return arrayOfExpenses[(int)expense, (int)FinArray.Data]; }

        /// <summary>
        /// Returns true if Expense item is Active
        /// </summary>
        /// <param name="expense"></param>
        /// <returns></returns>
        public bool GetExpenseStatus(Expense expense)
        { if (arrayOfExpenses[(int)expense, (int)FinArray.Status] > 0) { return true; } return false; }*/

        /// <summary>
        /// Set Active status (true/false) for an expense item
        /// </summary>
        /// <param name="expense"></param>
        /// <param name="status"></param>
        /*public void SetExpenseStatus(Expense expense, bool status)
        {
            if (status == true) { arrayOfExpenses[(int)expense, (int)FinArray.Status] = 1; }
            else { arrayOfExpenses[(int)expense, (int)FinArray.Status] = 0; }
        }

        public void SetLumpSum(LumpSum lump, int amount)
        { arrayOfLumpSums[(int)lump, (int)FinArray.Data] = amount; }*/

        /// <summary>
        /// Adds an amount to the exisitng LumpSum
        /// </summary>
        /// <param name="income"></param>
        /// <param name="amount">Amount to Add (pass a negative number to subtract)</param>
        /*public void ChangeLumpSum(LumpSum lump, int amount)
        { arrayOfLumpSums[(int)lump, (int)FinArray.Data] += amount; }

        public int GetLumpSum(LumpSum lump)
        { return arrayOfLumpSums[(int)lump, (int)FinArray.Data]; }

        /// <summary>
        /// Returns true if LumpSum item is Active
        /// </summary>
        /// <param name="lump"></param>
        /// <returns></returns>
        public bool GetLumpSumStatus(LumpSum lump)
        { if (arrayOfLumpSums[(int)lump, (int)FinArray.Status] > 0) { return true; } return false; }

        /// <summary>
        /// Set Active status (true/false) for a LumpSum item
        /// </summary>
        /// <param name="lump"></param>
        /// <param name="status"></param>
        /*public void SetLumpSumStatus(LumpSum lump, bool status)
        {
            if (status == true) { arrayOfLumpSums[(int)lump, (int)FinArray.Status] = 1; }
            else { arrayOfLumpSums[(int)lump, (int)FinArray.Status] = 0; }
        }*/

        


        //end Capital Methods
    }

    //
    // Major (Great) house ---
    //
    class MajorHouse : House
    {
        private List<int> listLordLocations; //locID of all bannerlords
        private List<int> listBannerLords; //refID of all bannerlords
        //private List<int> listHousesToCapital; //unique houses (HID), ignoring special locations -> NOT hooked up
        //private List<int> listHousesToConnector; //unique houses (HID), ignoring special locations -> NOT hooked up
        

        public MajorHouse()
        {
            listLordLocations = new List<int>();
            //listHousesToCapital = new List<int>();  NOT hooked up
            //listHousesToConnector = new List<int>(); NOT hooked up
            listBannerLords = new List<int>();
        }


        /// <summary>
        /// add a location to list of house controlled locations
        /// </summary>
        /// <param name="locID"></param>
        public void AddBannerLordLocation(int locID)
        { listLordLocations.Add(locID); }

        public void AddBannerLord(int refID)
        { listBannerLords.Add(refID); }

        /// <summary>
        /// add a house ID to list of unique houses to capital
        /// </summary>
        /// <param name="houseID"></param>
        /*public void AddHousesToCapital(int houseID)
        {
            //check houseID isn't already in list (only unique HouseID's are stored)
            if (houseID > 0)
            {
                if (listHousesToCapital.Contains(houseID) == false)
                { listHousesToCapital.Add(houseID); }
            }
        }*/

        

        /// <summary>
        /// returns list of Lords (subsidary bannerlord locations)
        /// </summary>
        /// <returns></returns>
        public List<int> GetBannerLordLocations()
        { return listLordLocations; }

        public List<int> GetBannerLords()
        { return listBannerLords; }

        public int GetNumBannerLords()
        { return listLordLocations.Count; }

        /*public List<int> GetHousesToCapital()
        { return listHousesToCapital; }

        public List<int> GetHousesToConnector()
        { return listHousesToConnector; }

        public void SetHousesToCapital(List<int> tempList)
        { listHousesToCapital.Clear(); listHousesToCapital.AddRange(tempList); }

        public void SetHousesToConnector(List<int> tempList)
        { listHousesToConnector.Clear(); listHousesToConnector.AddRange(tempList); }*/

        public void SetLordLocations(List<int> tempList)
        { listLordLocations.Clear(); listLordLocations.AddRange(tempList); }

        public void SetBannerLords(List<int> tempList)
        { listBannerLords.Clear();  listBannerLords.AddRange(tempList); }


    }

   /// <summary>
   /// Bannerlords
   /// </summary>
    class MinorHouse : House
    {
        public MinorHouse()
        { }

    }

    /// <summary>
    /// Special House class 
    /// </summary>
    class SpecialHouse : House
    {
        public SpecialHouse()
        { }
    }

    /// <summary>
    /// Inn's
    /// </summary>
    class InnHouse : SpecialHouse
    {
        private List<int> listOfFollowers; //ActID of followers in the Inn, available for recruitment.

        public InnHouse()
        {
            listOfFollowers = new List<int>();
            Special = HouseSpecial.Inn;
            CastleWalls = 0;
        }

        /// <summary>
        /// adds a follower to the listOfFollowers available for recruitment
        /// </summary>
        /// <param name="actID"></param>
        public void AddFollower(int actID)
        {
            if (actID > 0 && actID < 10)
            {
                listOfFollowers.Add(actID);
                Game.logStart?.Write($"Inn \"{Name}\" -> Follower ActID {actID} added");
            }
        }

        public List<int> GetFollowers()
        { return listOfFollowers; }

        public int GetNumFollowers()
        { return listOfFollowers.Count(); }


        /*/// <summary>
        /// used to update list (passed to world.InitialiseActiveActors, entries possibly deleted, passed back by reference as newly updated version)
        /// </summary>
        /// <param name="listOfUpdatedFollowers"></param>
        public void UpdateFollowers(List<int> listOfUpdatedFollowers)
        {
            if (listOfUpdatedFollowers != null)
            {
                if (listOfUpdatedFollowers.Count > 0)
                {
                    listOfFollowers.Clear();
                    listOfFollowers.AddRange(listOfUpdatedFollowers);
                }
                else { Game.SetError(new Error(226, "Invalid listOfUpdatedFollowers input (no records)")); }
            }
            else { Game.SetError(new Error(226, "Invalid listOfUpdatedFollowers input (null)")); }
        }*/

        /// <summary>
        /// Removes a follower from the list I'cause they've been recruited)
        /// </summary>
        /// <param name="actID"></param>
        public void RemoveFollower(int actID)
        {
            if (actID > 1 && actID < 10)
            {
                bool removed = false;
                for(int i = 0; i < listOfFollowers.Count; i++)
                {
                    if (listOfFollowers[i] == actID)
                    {
                        listOfFollowers.RemoveAt(i);
                        Game.logTurn?.Write($"Follower ActID {actID} has been removed from listOfFollowers at \"{Name}\"");
                        removed = true;
                        break;
                    }
                }
                if (removed == false)
                { Game.SetError(new Error(226, $"Follower, ActID {actID} wasn't found in {Name}'s listOfFollowers -> Not removed")); }
            }
            else { Game.SetError(new Error(226, $"Invalid actID \"{actID}\"")); }
        }
    }
}
