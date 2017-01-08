using System;
using System.Collections.Generic;
using Next_Game.Cartographic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum RevoltReason {None, Stupid_OldKing, Treacherous_NewKing, Incapacited_OldKing, Dead_OldKing, Internal_Dispute, External_Event}
    public enum Popularity {Hated_by, Disliked_by, Tolerated_by, Liked_by, Loved_by}

    /// <summary>
    /// Keeps track of all key game lore (backstory) DO NOT USE METHODS FOR ANY OTHER USE
    /// </summary>
    public class Lore
    {
        static Random rnd;
        //House RefID's
        public int RoyalRefIDOld { get; set; } // refID
        public int RoyalRefIDNew { get; set; } // refID
        public int RoyalRefIDCurrent { get; set; } // refID
        public int TurnCoat { get; set; } //actID of Turncoat Bannerlord who takes over lands of old king
        public int OldHouseID { get; set; } //HouseID of Old King (needed 'cause it's  removed from the house dictionaries once the new king takes over)
        public string OldHouseName { get; set; } //House name of old King
        //Royal & Rebel Family Nobles
        private List<Passive> listOfOldRoyals; //at time of revolt
        private List<Passive> listOfNewRoyals; //at time of taking power
        public Noble OldKing { get; set; }
        public Noble OldQueen { get; set; }
        public Noble OldHeir { get; set; }
        public Noble NewKing { get; set; }
        public Noble NewQueen { get; set; }
        public Noble NewHeir { get; set; }
        //reasons
        public RevoltReason WhyRevolt { get; set; } = RevoltReason.None;
        public Popularity OldKing_Popularity { get; set; } //charm
        public Popularity NewKing_Popularity { get; set; }
        public Popularity OldQueen_Popularity { get; set; }
        public Popularity NewQueen_Popularity { get; set; }

        //each list a series of sentences that make a paragraph about the topic
        private List<string> listOldKingRule;
        private List<string> listRevoltBackStory;
        private List<string> listUprising;
        private List<string> listRoyalFamilyFate;
        private List<string> listNewKingRule;
        private List<string> listChanges; //location's, bannerlords etc.
        private List<string> listOfUprisingBattles;

        public Lore(int seed)
        {
            rnd = new Random(seed);
            listOfOldRoyals = new List<Passive>();
            listOfNewRoyals = new List<Passive>();
            listOldKingRule = new List<string>();
            listRevoltBackStory = new List<string>();
            listUprising = new List<string>();
            listRoyalFamilyFate = new List<string>();
            listNewKingRule = new List<string>();
            listChanges = new List<string>();
            listOfUprisingBattles = new List<string>();
        }

        internal void SetListOfOldRoyals(List<Passive> listRoyals)
        { listOfOldRoyals?.AddRange(listRoyals); }

        internal void SetListOfNewRoyals(List<Passive> listRebels)
        { listOfNewRoyals?.AddRange(listRebels); }

        internal List<string> GetUprising()
        { return listUprising; }

        internal List<string> GetOldKingRule()
        { return listOldKingRule; }

        internal List<string> GetRevoltBackStory()
        { return listRevoltBackStory; }

        internal List<string> GetRoyalFamilyFate()
        { return listRoyalFamilyFate; }

        internal List<string> GetNewKingRule()
        { return listNewKingRule; }

        /// <summary>
        /// generates reason and populates lore lists
        /// </summary>
        internal void CreateOldKingBackStory(List<MajorHouse> listOfRoyalists, List<MajorHouse> listOfRebels, List<string> listOfWounds)
        {
            //list of possible reasons - weighted entries, one chosen at completion
            List<RevoltReason> listWhyPool = new List<RevoltReason>();
            List<int> listOfTempRoyals = new List<int>();
            List<int> listOfTempRebels = new List<int>();
            List<int> listOfTempKnights = Game.world.GetKnights();
            List<int> listOfCapturedActors = new List<int>();

            //check how smart old king was (takes into account wife's possible influence)
            int oldKing_Wits;
            int influencer = OldKing.Influencer;
            if (influencer > 0 && Game.world.CheckActorPresent(influencer, 1) && OldKing.CheckSkillInfluenced(SkillType.Wits))
            { oldKing_Wits = OldKing.GetSkill(SkillType.Wits, SkillAge.Fifteen, true); }
            else { oldKing_Wits = OldKing.GetSkill(SkillType.Wits); }
            //dumb king (2 pool entries if wits 2 stars and 5 entries if wits 1 star)
            if (oldKing_Wits == 2) { for (int i = 0; i < 2; i++) { listWhyPool.Add(RevoltReason.Stupid_OldKing); } } 
            else if (oldKing_Wits == 1) { for (int i = 0; i < 5; i++) { listWhyPool.Add(RevoltReason.Stupid_OldKing); } }

            //check new king treachery
            int newKing_Treachery;
            influencer = NewKing.Influencer;
            if (influencer > 0 && Game.world.CheckActorPresent(influencer, 1) && NewKing.CheckSkillInfluenced(SkillType.Treachery))
            { newKing_Treachery = NewKing.GetSkill(SkillType.Treachery, SkillAge.Fifteen, true); }
            else { newKing_Treachery = NewKing.GetSkill(SkillType.Treachery); }
            //treacherous new king grabs power (2 pool entries if 4 starts, 5 entries if treachery 5 stars)
            if (newKing_Treachery == 4) { for (int i = 0; i < 2; i++) { listWhyPool.Add(RevoltReason.Treacherous_NewKing); } }
            else if (newKing_Treachery == 5) { for (int i = 0; i < 5; i++) { listWhyPool.Add(RevoltReason.Treacherous_NewKing); } }

            //both kings leadership
            int oldKing_Leadership = OldKing.GetSkill(SkillType.Leadership);
            int newKing_Leadership = NewKing.GetSkill(SkillType.Leadership);

            //3 entries for old king being incapacitated
            for (int i = 0; i < 3; i++) { listWhyPool.Add(RevoltReason.Incapacited_OldKing); }
            //2 entries for old king dying
            for (int i = 0; i < 2; i++) { listWhyPool.Add(RevoltReason.Dead_OldKing); }
            //3 entries for an internal dispute
            for (int i = 0; i < 3; i++) { listWhyPool.Add(RevoltReason.Internal_Dispute); }
            //4 entries for an external event
            for (int i = 0; i < 3; i++) { listWhyPool.Add(RevoltReason.External_Event); }

            //choose a random reason from the pool
            WhyRevolt = listWhyPool[rnd.Next(0, listWhyPool.Count)];

            Console.WriteLine(Environment.NewLine + "--- Create BackStory");
            Console.WriteLine("Old King Wits {0} Aid {1}, {2}", oldKing_Wits, OldKing.ActID, OldKing.Name);
            Console.WriteLine("New King Treachery {0} Aid {1}, {2}", newKing_Treachery, NewKing.ActID, NewKing.Name);
            Console.WriteLine("WhyRevolt: {0}", WhyRevolt);

            //get old & new king and Queen's charm (can't be influenced)
            int oldKing_Charm = OldKing.GetSkill(SkillType.Charm);
            int newKing_Charm = NewKing.GetSkill(SkillType.Charm);
            int oldQueen_Charm = OldQueen.GetSkill(SkillType.Charm);
            int newQueen_Charm = NewQueen.GetSkill(SkillType.Charm);
            OldKing_Popularity = (Popularity)oldKing_Charm;
            NewKing_Popularity = (Popularity)newKing_Charm;
            OldQueen_Popularity = (Popularity)oldQueen_Charm;
            NewQueen_Popularity = (Popularity)newQueen_Charm;

            Console.WriteLine("King {0} was {1} by his people", OldKing.Name, OldKing_Popularity);
            Console.WriteLine("His Queen, {0}, was {1} with the common folk", OldQueen.Name, OldQueen_Popularity);
            Console.WriteLine(Environment.NewLine + "In {0} there was a great Revolt", Game.gameRevolt);

            //loyalties of Kings
            OldKing.Loyalty_Current = KingLoyalty.Old_King;
            NewKing.Loyalty_Current = KingLoyalty.New_King;

            //How many men could the Old king field?
            int royalArmy = 0;
            Console.WriteLine(Environment.NewLine + "--- Royalist Army");
            foreach (MajorHouse house in listOfRoyalists)
            {
                //Great houses
                royalArmy += GetMenAtArms(house);
                listOfTempRoyals.Add(house.LordID);
                //bannerLords
                List<int> bannerLords = house.GetBannerLords();
                if (bannerLords.Count > 0)
                {
                    foreach (int minorRefID in bannerLords)
                    {
                        House minorHouse = Game.world.GetHouse(minorRefID);
                        royalArmy += GetMenAtArms(minorHouse);
                        listOfTempRoyals.Add(minorHouse.LordID);
                    }
                }
            }
            Console.WriteLine(Environment.NewLine + "The Royalists fielded {0:N0} Men At Arms", royalArmy);
            //How many men could the New king field?
            int rebelArmy = 0;
            Console.WriteLine(Environment.NewLine + "--- Rebel Army");
            foreach (MajorHouse house in listOfRebels)
            {
                //Great houses
                rebelArmy += GetMenAtArms(house);
                listOfTempRebels.Add(house.LordID);
                //bannerLords
                List<int> bannerLords = house.GetBannerLords();
                if (bannerLords.Count > 0)
                {
                    foreach (int minorRefID in bannerLords)
                    {
                        House minorHouse = Game.world.GetHouse(minorRefID);
                        rebelArmy += GetMenAtArms(minorHouse);
                        listOfTempRebels.Add(minorHouse.LordID);
                    }
                }
            }
            Console.WriteLine(Environment.NewLine + "The Rebels fielded {0:N0} Men At Arms", rebelArmy);
            Console.WriteLine(Environment.NewLine + "King {0} was {1} his people", NewKing.Name, NewKing_Popularity);
            Console.WriteLine("His Queen, {0}, was {1} the common folk", NewQueen.Name, NewQueen_Popularity);

            //Battle Resolution module - multiple battles, sieges, to and fro

            //get list of royalist Locations
            List<Location> royalistLocs = new List<Location>();
            //add kingskeep as first record
            royalistLocs.Add(Game.network.GetLocation(1));
            foreach(MajorHouse house in listOfRoyalists)
            {
                royalistLocs.Add(Game.network.GetLocation(house.LocID));
                List<int> bannerLordLocs = house.GetBannerLordLocations();
                foreach(int locID in bannerLordLocs)
                { royalistLocs.Add(Game.network.GetLocation(locID)); }
            }
            //order by distance from capital
            List<Location> orderedLocs = new List<Location>();
            IEnumerable<Location> tempLocs =
                from loc in royalistLocs
                orderby loc.DistanceToCapital
                select loc;
            orderedLocs = tempLocs.ToList();
            //debug
            Console.WriteLine(Environment.NewLine + "--- Royalist Locations (Battle site pool");
            foreach(Location loc in orderedLocs)
            {
                string houseName = Game.world.GetGreatHouseName(loc.HouseID);
                string locName = loc.LocName;
                Console.WriteLine("{0} -> {1}, distance {2}", houseName, locName, loc.DistanceToCapital);
            }
            Console.WriteLine(Environment.NewLine + "--- Battles");
            //fixed # of battles -> first, last and one inbetween
            int numBattles = 3;
            HistKingdomIncident kingdomEvent;
            string descriptor = null;
            int index;
            int year = Game.gameRevolt - 1; //all conflict assumed to be one year prior to start of the new King's rule

            //hard coded data sets
            string[] array_LeadershipNew = new string[] { "None", "woeful", "dubious", "unexpectional", "strong", "commanding" };
            string[] array_LeadershipOld = new string[] { "None", "blundering", "insipid and uninspiring", "unremarkable", "firm", "decisive" };
            string[] array_Battle = new string[] { "met", "surprised", "marched on", "launched themselves at", "unexpectedly encountered", "threw themselves at" };
            string[] array_Siege = new string[] { "invested", "surrounded", "encircled", "cut-off", "isolated" };
            string[] array_OutcomeBattleBad = new string[] { "was unable to rally his men and suffered heavy losses", "and poor preperation led to a humilitating defeat", "lost control of the battle and suffered grevious losses",
            "and disregard of advice from his Lords led to a lost cause on the field of battle", "succeeded only in issuing a succession of fumbled orders that added to the confusion and the inevitable defeat" };
            string[] array_OutcomeBattleGood = new string[] { "wasn't enough to hold the line but he managed to retreat in good order", "failed to overcome the royalist's poor position but kept losses to a minimum",
            "enabled a strong counter attack that halted the rebels sufficiently for the bulk of the royalist forces to escape", "fended off a succession of rebel assaults and withdrew the royalist forces largely intact"};
            string[] array_OutcomeBattleNeutral = new string[] { "defeated","given a bloody nose","forced to retreat from the field of battle", "unable to hold their ground", "pushed back against their will" };
            string[] array_OutcomeSiegeBad = new string[] { "led to the subsequent slaughter of most within the stronghold", "goaded the rebels into burning every flammable structure in the stronghold",
            "turned an impregnable stronghold into an open invitation for destruction of all within", "resulted in a useless sortie that served only to leave the gates open for the enemy"};
            string[] array_OutcomeSiegeGood = new string[] { "saw the stronghold stand firm and the rebels repulsed", "led to a great victory over the besieging forces",
            "inflicted heavy losses on the besieging rebels", "threw back continual attempts by the rebels to breach the walls"};
            string[] array_OutcomeSiegeNeutral = new string[] { "loss of the stronghold", "pillaging of the stronghold", "surrender of the garrison", "destruction of the stronghold" };
            string[] array_FinalBattle = new string[] { "loss of the stronghold", "pillaging of the stronghold", "surrender of the garrison", "destruction of the stronghold" };
            string[] array_FinalSiege = new string[] { "loss of the stronghold", "pillaging of the stronghold", "surrender of the garrison", "destruction of the stronghold" };
            string[] array_LastStand = new string[] { "with his back to the wall", "desperate and dangerous", "with a failing kingdom before him", "with an air of desperation",
            "with nothing left to lose", "with naught but hope remaining", "displaying an hitherto unknown reservoir of fortitude"};
            string[] array_LastAssault = new string[] { "a vengeful wraith", "an unstoppable force of change", "a rabid, howling hyena", "a roaring, blood-crazed, demon", "the hand of the Devil", "voice of doom",
                "an evil angel of fire", "thunder roaring down from the sky", "a lightning apostole from above", "the hell raised Son of Satan", "a fiery dragon", "the hand of destiny"};
            string[] array_LastAction = new string[] { "threw", "launched", "hurled", "surged", "thrust", "flung", "stormed", "pounded" };
            string[] array_Fought = new string[] { "ferociously", "tenanciously", " savagely", "with unaccustomed ferocity", "like a man possessed"};
            string[] array_LostCause = new string[] { "to no avail", "a lost cause", "a vain attempt", "a wasted effort", "an exercise in futility", "a dark day of infamy", "inexorably ground down" };
            string[] array_Kingskeep = new string[] { "slaughtering all those he deemed unworthy", "beheading Royalists wherever he found them", "burning at the stake hundreds of royal sympathisers",
            "drowning multitudes of loyal royalists like bagged rats thrown into the river", "impaling anyone who refused to swear fealty to him"};
            string[] array_Inbetween = new string[] { "a day best forgotten by the Royalists", "an ignomious defeat of the King's forces", "resulted in an inconclusive outcome with heavy losses on both sides",
                " a hard fought draw but at a heavy cost", "a turning point with the defeat of the Royalists", "a defeat of the King forces that extinguished any hope of a victory" };
            string[] array_TurnOfTide = new string[] { "was thrown onto the defensive", "would never more dream of taking the fight to the Rebels", "could only hope to survive", "foreswore all chance of victory",
                "girded himself for the inevitable", "could only wait and pray" };
            string[] array_AbsentLeadership = new string[] { "after a time was nowhere to be seen", "was absent when needed", " inexplicably stood off and refused to participate", "went missing, along with his men,"};
            string[] array_EnemyLeadership = new string[] { "was captured and forced to fight for the enemy", "had his family held hostage and was forced to change sides", "was compelled to swap sides at the point of a spear"};
            string[] array_GoodLeadership = new string[] { "led his men with valour and distinction", "shored up a crumbling line with his presence", "exhibited outstanding leadership", "led from the front and set a fine example",
            "bravely held an outnumbered flank until help arrived", "managed to stand firm with his men and hold the line", "charged with his men in a ferocious counterattack", "stiffened the resolve of all around him with his bravery"};
            string[] array_HeroicDeeds = new string[] {"slew so many enemies that nobody dared come near him", "singlehandedly halted an enemy assault with cold steel", "recklessly risked his life to save others",
            "fought with great honour and heroism", "slaughtered untold enemies in a frenzied attack", "stood firm when all those around him had fled"};
            string[] array_BadDeeds = new string[] {"failed to rise to the occassion when under pressure", "disappointed those around him with his lacklustre swordsmanship", "Reigned in his horse and shied away from combat",
            "was seen to turn and gallop away from the enemy", "managed to soil his armour and loose his sword", "forgot that he was a knight and refused to engage with the enemy"};

            //find a turncoat bannerlord right at start -> take him out to avoid any battle events prior to his use
            if (listOfTempRoyals.Count > 1)
            {
                int count = 0;
                int limit = listOfTempRoyals.Count;
                Passive turncoatActor = null;
                do
                {
                    int tempIndex = rnd.Next(1, listOfTempRoyals.Count);
                    int turncoatID = listOfTempRoyals[tempIndex];
                    Passive actor = Game.world.GetPassiveActor(turncoatID);
                    if (actor.Type == ActorType.BannerLord)
                    { turncoatActor = actor; TurnCoat = actor.ActID; listOfTempRoyals.RemoveAt(tempIndex); }
                    else
                    {
                        //prevent an endless loop
                        count++;
                        if (count >= limit)
                        { Game.SetError(new Error(44, "Endless loop")); break; }
                    }
                }
                while (turncoatActor == null);
            }
            else { Game.SetError(new Error(32, "No data in listOfTempRoyals")); }

            //Conflict Loop ---

            string Friends, Enemies, eventText, secretText;
            for (int i = 0; i < numBattles; i++)
            {
                //choose from the closest to the Capital half of the royalist locations (battle of KingsKeep is always the last)
                if (i == numBattles - 1) { index = 0; }
                else { index = rnd.Next(1, orderedLocs.Count / 2); }
                Location loc = orderedLocs[index];
                orderedLocs.RemoveAt(index);
                //40% chance of a siege, otherwise a battle
                if (rnd.Next(100) < 40)
                { kingdomEvent = HistKingdomIncident.Siege; descriptor = string.Format("The Siege of {0}", loc.LocName); }
                else
                { kingdomEvent = HistKingdomIncident.Battle; descriptor = string.Format("The Battle of {0}", loc.LocName); }
                //create record
                string details = string.Format("{0} {1}", descriptor, Game.world.ShowLocationCoords(loc.LocationID));
                Record record = new Record(details, loc.LocationID, loc.RefID, year, kingdomEvent);
                Game.world.SetRecord(record);
                //add to list of battles to enable actor events to be fleshed out
                listOfUprisingBattles.Add(descriptor);
                //debug
                Console.WriteLine("{0} {1} {2}", year, descriptor, Game.world.ShowLocationCoords(loc.LocationID));

                //Battle descriptions - first (rebels always prevail, battle is smallest, rebel forces larger than royalists)
                string text_1 = null;
                string text_2 = null;
                if (i == 0)
                {
                    int rebelForces = rebelArmy / 2;
                    int royalForces = royalArmy / rnd.Next(3, 5);
                    text_1 = string.Format("Under the {0} leadership of the Rebel {1} {2} his {3:N0} men at arms", array_LeadershipNew[newKing_Leadership], NewKing.Type, NewKing.Name, rebelForces);
                    //first
                    if (kingdomEvent == HistKingdomIncident.Battle)
                    { text_1 += string.Format(" {0} a Royalist force of {1:N0} at {2}. ", array_Battle[rnd.Next(0, array_Battle.Length)], royalForces, descriptor); }
                    else
                    { text_1 += string.Format(" {0} a Royalist stronghold at {1}. ", array_Siege[rnd.Next(0, array_Siege.Length)], descriptor); }
                    //outcome text
                    if (kingdomEvent == HistKingdomIncident.Battle)
                    {
                        //battle
                        if (oldKing_Leadership > 3)
                        { text_2 = string.Format("King {0}'s {1} direction {2}.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeBattleGood[rnd.Next(0, array_OutcomeBattleGood.Length)]); }
                        else if (oldKing_Leadership < 3)
                        { text_2 = string.Format("King {0}'s {1} direction {2}.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeBattleBad[rnd.Next(0, array_OutcomeBattleBad.Length)]); }
                        else
                        { text_2 = string.Format("King {0}'s {1} direction resulted in his royalist forces being {2}.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeBattleNeutral[rnd.Next(0, array_OutcomeBattleNeutral.Length)]); }
                    }
                    else
                    {
                        //siege
                        if (oldKing_Leadership > 3)
                        { text_2 = string.Format("King {0}'s {1} direction {2}.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeSiegeGood[rnd.Next(0, array_OutcomeSiegeGood.Length)]); }
                        else if (oldKing_Leadership < 3)
                        { text_2 = string.Format("King {0}'s {1} direction {2}.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeSiegeBad[rnd.Next(0, array_OutcomeSiegeBad.Length)]); }
                        else
                        { text_2 = string.Format("King {0}'s {1} direction resulted in the {2} although the King and his guard managed to flee.", OldKing.Name, array_LeadershipOld[oldKing_Leadership], array_OutcomeSiegeNeutral[rnd.Next(0, array_OutcomeSiegeNeutral.Length)]); }
                    }
                }

                //final climatic battle
                else if (i == numBattles - 1)
                {
                    int rebelForces = rebelArmy;
                    int royalForces = royalArmy / rnd.Next(2, 6);
                    text_1 = string.Format("King {0}, {1}, made a last stand with his remaining {2:N0} loyal men at arms at {3}. ", OldKing.Name,
                        array_LastStand[rnd.Next(0, array_LastStand.Length)], royalForces, descriptor);
                    text_1 += string.Format("Like {0}, Lord {1} {2} a Royalist force {3:N0} strong straight at them. ", array_LastAssault[rnd.Next(0, array_LastAssault.Length)],
                        NewKing.Name, array_LastAction[rnd.Next(0, array_LastAction.Length)], rebelForces);
                    //outcome text
                    text_2 = string.Format("The King fought {0} but it was {1}. ", array_Fought[rnd.Next(0, array_Fought.Length)], array_LostCause[rnd.Next(0, array_LostCause.Length)]);
                    text_2 += string.Format("The rebel Lord {0} entered Kingskeep, {1}, before proclaiming himself King of the Iron Throne and ruler of the Seven Kingdoms. Long live the King!",
                        NewKing.Name, array_Kingskeep[rnd.Next(0, array_Kingskeep.Length)]);
                }

                //inbetween battles
                else
                {
                    text_1 = string.Format("{0}, {1}. ", descriptor, array_Inbetween[rnd.Next(0, array_Inbetween.Length)]);
                    text_2 = string.Format("King {0} {1} from this day forward.", OldKing.Name, array_TurnOfTide[rnd.Next(0, array_TurnOfTide.Length)]);
                }
                //debug
                Console.WriteLine(Environment.NewLine + text_1);
                Console.WriteLine(Environment.NewLine + text_2 + Environment.NewLine);

                //add to text list (run through word wrap first)
                listUprising.Add("");
                string textToWrap = text_1 + text_2;
                listUprising.AddRange(Game.utility.WordWrap(textToWrap, 120));

                

                //events ---

                for (int k = 0; k < Game.constant.GetValue(Global.BATTLE_EVENTS); k++)
                {

                    //knights
                    if (listOfTempKnights.Count > 0)
                    {
                        int rndNum = rnd.Next(10);
                        int listIndex = rnd.Next(0, listOfTempKnights.Count);
                        int knightID = listOfTempKnights[listIndex];
                        Passive knight = Game.world.GetPassiveActor(knightID);
                        Friends = "Royalists";
                        Enemies = "Rebels";
                        Record record_knight = null;
                        int knightHouse = knight.HouseID;
                        if (knight.Loyalty_Current == KingLoyalty.New_King) { Friends = "Rebels"; Enemies = "Royalists"; }
                        //what happened?
                        switch (rndNum)
                        {
                            case 0:
                            case 1:
                            case 2:
                                //knight captured
                                listOfCapturedActors.Add(knightID);
                                listOfTempKnights.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was captured by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor);
                                Console.WriteLine(string.Format("Knight {0}, Aid {1}, was captured by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor));
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorIncident.Captured);
                                break;
                            case 3:
                            case 4:
                                //knight killed
                                listOfTempKnights.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", knight.Name, knight.ActID, descriptor, Friends, knight.Age);
                                Console.WriteLine(string.Format("Knight {0}, Aid {1}, was killed by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor));
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorIncident.Died);
                                Game.history.RemoveActor(knight, year, ActorGone.Battle);
                                break;
                            case 5:
                            case 6:
                            case 7:
                                //knight wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", knight.Name, knight.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("Knight {0}, Aid {1}, was wounded by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor));
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorIncident.Wounded);
                                //60% chance of wound causing an ongoing issue -> Secret (random strength 1 to 4)
                                if (rnd.Next(100) < 60)
                                {
                                    string wound = listOfWounds[rnd.Next(0, listOfWounds.Count)];
                                    secretText = string.Format("{0}, Aid {1}, {2}", knight.Name, knight.ActID, wound);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Wound, year, secretText, rnd.Next(1, 5), knight.ActID);
                                    Game.history.SetSecret(secret);
                                    knight.AddSecret(secret.SecretID);
                                }
                                break;
                            case 8:
                            case 9:
                                int knightCombat = knight.GetSkill(SkillType.Combat);
                                int knightTreachery = knight.GetSkill(SkillType.Treachery);
                                bool trueEvent = true;
                                //special deed, good or bad
                                if (rnd.Next(0, 6) < knightCombat)
                                {
                                    //heroic deed
                                    index = rnd.Next(0, array_HeroicDeeds.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", knight.Name, knight.ActID, array_HeroicDeeds[index], descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} during {3}", knight.Name, knight.ActID, array_HeroicDeeds[index], descriptor);
                                    //did he take somebody else's glory? -> secret
                                    if (rnd.Next(1, 6) < knightTreachery)
                                    {
                                        secretText = string.Format("{0}, Aid {1}, {2} {3}", knight.Name, knight.ActID, "falsely claimed heroic deeds that belonged to another", descriptor);
                                        Secret_Actor secret = new Secret_Actor(SecretType.Glory, year, secretText, knightTreachery, knight.ActID);
                                        Game.history.SetSecret(secret);
                                        knight.AddSecret(secret.SecretID);
                                        trueEvent = false;
                                    }
                                }
                                else
                                {
                                    //bad deed
                                    index = rnd.Next(0, array_BadDeeds.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", knight.Name, knight.ActID, array_BadDeeds[index], descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} during {3}", knight.Name, knight.ActID, array_BadDeeds[index], descriptor);
                                    //was falsely accused by another? -> secret
                                    if (rnd.Next(1, 6) > knightTreachery)
                                    {
                                        secretText = string.Format("{0}, Aid {1}, {2} {3}", knight.Name, knight.ActID, "was falsely accused by another of something he didn't do", descriptor);
                                        Secret_Actor secret = new Secret_Actor(SecretType.Glory, year, secretText, knightTreachery, knight.ActID);
                                        Game.history.SetSecret(secret);
                                        knight.AddSecret(secret.SecretID);
                                        trueEvent = false;
                                    }
                                }
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorIncident.Heroic_Deed, trueEvent);
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_knight != null) { record_knight.AddActorIncident(HistActorIncident.Conflict); Game.world.SetRecord(record_knight); }
                    }

                    //Royalist Lords
                    if (listOfTempRoyals.Count > 1)
                    {
                        

                        int rndNum = rnd.Next(10);
                        int listIndex = rnd.Next(1, listOfTempRoyals.Count); //don't pick King (index 0)
                        int royalID = listOfTempRoyals[listIndex];
                        Passive royal = Game.world.GetPassiveActor(royalID);
                        Friends = "Royalists";
                        Enemies = "Rebels";
                        Record record_royal = null;
                        int royalHouse = royal.HouseID;
                        if (royal.Loyalty_Current == KingLoyalty.New_King) { Friends = "Rebels"; Enemies = "Royalists"; }
                        bool truth = true;
                        //what happened?
                        switch (rndNum)
                        {
                            case 0:
                            case 1:
                                //captured
                                listOfCapturedActors.Add(royalID);
                                listOfTempRoyals.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was captured by the {2} during {3}", royal.Name, royal.ActID, Enemies, descriptor);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was captured by the {3} during {4}", royal.Type, royal.Name, royal.ActID, Enemies, descriptor));
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorIncident.Captured);
                                break;
                            case 2:
                                //killed
                                listOfTempRoyals.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", royal.Name, royal.ActID, descriptor, Friends, royal.Age);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was killed by the {3} during {4}", royal.Type, royal.Name, royal.ActID, Enemies, descriptor));
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorIncident.Died);
                                //replace Bannerlord
                                if (royal is BannerLord) { ReplaceBannerLord((BannerLord)royal); }
                                else if (royal is Noble) { ReplaceNobleLord((Noble)royal); }
                                Game.history.RemoveActor(royal, year, ActorGone.Battle);
                                break;
                            case 3:
                            case 4:
                            case 5:
                                //wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", royal.Name, royal.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was wounded by the {3} during {4}", royal.Type, royal.Name, royal.ActID, Enemies, descriptor));
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorIncident.Wounded);
                                //60% chance of wound causing an ongoing issue -> Secret (random strength 1 to 4)
                                if (rnd.Next(100) < 60)
                                {
                                    string wound = listOfWounds[rnd.Next(0, listOfWounds.Count)];
                                    secretText = string.Format("{0}, Aid {1}, {2}", royal.Name, royal.ActID, wound);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Wound, year, secretText, rnd.Next(1, 5), royal.ActID);
                                    Game.history.SetSecret(secret);
                                    royal.AddSecret(secret.SecretID);
                                }
                                break;
                            case 6:
                            case 7:
                                //Bad leadership
                                bool changedSides = false;
                                //absent leadership, possible traitor
                                if (rnd.Next(100) < 60)
                                {
                                    index = rnd.Next(0, array_AbsentLeadership.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_AbsentLeadership[index], descriptor);
                                    Console.WriteLine(string.Format("{0} {1}, Aid {2}, {3} during {4}", royal.Type, royal.Name, royal.ActID, array_AbsentLeadership[index], descriptor));
                                }
                                else
                                {
                                    //changed sides against their will
                                    index = rnd.Next(0, array_EnemyLeadership.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_EnemyLeadership[index], descriptor);
                                    Console.WriteLine(string.Format("{0} {1}, Aid {2}, {3} during {4}", royal.Type, royal.Name, royal.ActID, array_EnemyLeadership[index], descriptor));
                                    //change loyalty and swap lord from royalist to rebel list for any future battles
                                    royal.Loyalty_Current = KingLoyalty.New_King;
                                    listOfTempRoyals.RemoveAt(listIndex);
                                    listOfTempRebels.Add(royalID);
                                    changedSides = true;
                                }
                                //was it because they were a traitor? -> secret
                                int lordTreachery = royal.GetSkill(SkillType.Treachery);
                                if (rnd.Next(0,6) < lordTreachery)
                                {
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "was a traitor who deliberately changed sides during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordTreachery, royal.ActID);
                                    Game.history.SetSecret(secret);
                                    royal.AddSecret(secret.SecretID);
                                    truth = false;
                                    if (changedSides == false)
                                    {
                                        //change loyalty and swap lord from royalist to rebel list for any future battles
                                        royal.Loyalty_Current = KingLoyalty.New_King;
                                        listOfTempRoyals.RemoveAt(listIndex);
                                        listOfTempRebels.Add(royalID);
                                        Console.WriteLine("Moved to Rebel List");
                                    }
                                }
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorIncident.Leadership, truth);
                                break;
                            case 8:
                            case 9:
                                //Good leadership
                                index = rnd.Next(0, array_GoodLeadership.Length);
                                eventText = string.Format("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_GoodLeadership[index], descriptor);
                                Console.WriteLine("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_GoodLeadership[index], descriptor);
                                int lordLeadership = royal.GetSkill(SkillType.Leadership);
                                if (rnd.Next(1, 6) > lordLeadership)
                                {
                                    //poor leadership, took somebody else's glory -> secret
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordLeadership, royal.ActID);
                                    Game.history.SetSecret(secret);
                                    royal.AddSecret(secret.SecretID);
                                    truth = false;
                                }
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorIncident.Leadership);
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_royal != null) { record_royal.AddActorIncident(HistActorIncident.Conflict); Game.world.SetRecord(record_royal); }
                    }

                    //Rebel Lords
                    if (listOfTempRebels.Count > 0)
                    {
                        int rndNum = rnd.Next(10);
                        int listIndex = rnd.Next(1, listOfTempRebels.Count); //don't pick King (index 0)
                        int rebelID = listOfTempRebels[listIndex];
                        Passive rebel = Game.world.GetPassiveActor(rebelID);
                        Friends = "Royalists";
                        Enemies = "Rebels";
                        Record record_rebel = null;
                        int rebelHouse = rebel.HouseID;
                        if (rebel.Loyalty_Current == KingLoyalty.New_King) { Friends = "Rebels"; Enemies = "Royalists"; }
                        bool truth = true;
                        //what happened?
                        switch (rndNum)
                        {
                            case 0:
                            case 1:
                                //captured
                                listOfCapturedActors.Add(rebelID);
                                listOfTempRebels.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was captured by the {2} during {3}", rebel.Name, rebel.ActID, Enemies, descriptor);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was captured by the {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, Enemies, descriptor));
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorIncident.Captured);
                                break;
                            case 2:
                                //killed
                                listOfTempRebels.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", rebel.Name, rebel.ActID, descriptor, Friends, rebel.Age);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was killed by the {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, Enemies, descriptor));
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorIncident.Died);
                                //replace Bannerlord
                                if (rebel is BannerLord ) { ReplaceBannerLord((BannerLord)rebel); }
                                else if (rebel is Noble) { ReplaceNobleLord((Noble)rebel); }
                                Game.history.RemoveActor(rebel, year, ActorGone.Battle);
                                break;
                            case 3:
                            case 4:
                            case 5:
                                //wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", rebel.Name, rebel.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was wounded by the {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, Enemies, descriptor));
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorIncident.Wounded);
                                //60% chance of wound causing an ongoing issue -> Secret (random strength 1 to 4)
                                if (rnd.Next(100) < 60)
                                {
                                    string wound = listOfWounds[rnd.Next(0, listOfWounds.Count)];
                                    secretText = string.Format("{0}, Aid {1}, {2}", rebel.Name, rebel.ActID, wound);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Wound, year, secretText, rnd.Next(1, 5), rebel.ActID);
                                    Game.history.SetSecret(secret);
                                    rebel.AddSecret(secret.SecretID);
                                }
                                break;
                            case 6:
                                //Bad leadership
                                bool changedSides = false;
                                //absent leadership, possible traitor
                                if (rnd.Next(100) < 60)
                                {
                                    index = rnd.Next(0, array_AbsentLeadership.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_AbsentLeadership[index], descriptor);
                                    Console.WriteLine(string.Format("{0} {1}, Aid {2}, {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, array_AbsentLeadership[index], descriptor));
                                }
                                else
                                {
                                    //changed sides against their will
                                    index = rnd.Next(0, array_EnemyLeadership.Length);
                                    eventText = string.Format("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_EnemyLeadership[index], descriptor);
                                    Console.WriteLine(string.Format("{0} {1}, Aid {2}, {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, array_EnemyLeadership[index], descriptor));
                                    //change loyalty and swap lord from Rebelist to royalist list for any future battles
                                    rebel.Loyalty_Current = KingLoyalty.Old_King;
                                    listOfTempRebels.RemoveAt(listIndex);
                                    listOfTempRoyals.Add(rebelID);
                                    changedSides = true;
                                }
                                
                                //was it because they were a traitor? -> secret
                                int lordTreachery = rebel.GetSkill(SkillType.Treachery);
                                if (rnd.Next(0, 6) < lordTreachery)
                                {
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "was a traitor who deliberately changed sides during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordTreachery, rebel.ActID);
                                    Game.history.SetSecret(secret);
                                    rebel.AddSecret(secret.SecretID);
                                    truth = false;
                                    if (changedSides == false)
                                    {
                                        //change loyalty and swap lord from rebel list to royalist list for any future battles
                                        rebel.Loyalty_Current = KingLoyalty.Old_King;
                                        listOfTempRebels.RemoveAt(listIndex);
                                        listOfTempRoyals.Add(rebelID);
                                        Console.WriteLine("Moved to Rebel List");
                                    }
                                }
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorIncident.Leadership, truth);
                                break;
                            case 7:
                            case 8:
                            case 9:
                                //Good leadership
                                index = rnd.Next(0, array_GoodLeadership.Length);
                                eventText = string.Format("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_GoodLeadership[index], descriptor);
                                Console.WriteLine("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_GoodLeadership[index], descriptor);
                                int lordLeadership = rebel.GetSkill(SkillType.Leadership);
                                if (rnd.Next(1, 6) > lordLeadership)
                                {
                                    //poor leadership, took somebody else's glory -> secret
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordLeadership, rebel.ActID);
                                    Game.history.SetSecret(secret);
                                    rebel.AddSecret(secret.SecretID);
                                    truth = false;
                                }
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorIncident.Leadership, truth);
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_rebel != null) { record_rebel.AddActorIncident(HistActorIncident.Conflict); Game.world.SetRecord(record_rebel); }
                    }
                }
            }

            //work out what happened to Captured actors
            foreach (int actorID in listOfCapturedActors)
            {
                int rndNum = rnd.Next(10);
                Passive actor = Game.world.GetPassiveActor(actorID);
                eventText = string.Format("{0}, Aid {1}, ", actor.Name, actor.ActID);
                HistActorIncident actorEvent = HistActorIncident.None;
                Friends = "Royalist";
                Enemies = "Rebel";
                bool truth = true;
                if (actor.Loyalty_Current == KingLoyalty.New_King) { Friends = "Rebel"; Enemies = "Royalist"; }
                switch (rndNum)
                {
                    case 0:
                    case 1:
                        eventText += string.Format("was ransomed and released by his {0} captors", Enemies);
                        break;
                    case 2:
                    case 3:
                        eventText += string.Format("was released unharmed by his {0} captors", Enemies);
                        break;
                    case 4:
                    case 5:
                        //tortured -> secret
                        eventText += string.Format("was released unharmed by his {0} captors", Enemies);
                        secretText = string.Format("{0}, Aid {1}, was tortured by the {2}s during captivity (mentally scarred)", actor.Name, actor.ActID, Enemies);
                        Secret_Actor secret = new Secret_Actor(SecretType.Torture, year, secretText, 2, actor.ActID);
                        Game.history.SetSecret(secret);
                        actor.AddSecret(secret.SecretID);
                        truth = false;
                        break;
                    case 6:  
                    case 7:
                        //eventText += string.Format("allowed to slowly rot to his death in the {0} dungeons", Enemies);
                        eventText += string.Format("died of his wounds while held in the {0} dungeons", Enemies);
                        actorEvent = HistActorIncident.Died;
                        //replace Bannerlord
                        if (actor is BannerLord) { ReplaceBannerLord((BannerLord)actor); }
                        else if (actor is Noble) { ReplaceNobleLord((Noble)actor); }
                        Game.history.RemoveActor(actor, Game.gameRevolt, ActorGone.Injuries);
                        //50% chance he was tortured to death -> secret
                        if (rnd.Next(100) < 50)
                        {
                            secretText = string.Format("{0}, Aid {1}, was tortured and died in the {2} dungeons", actor.Name, actor.ActID, Enemies);
                            Secret_Actor secret_1 = new Secret_Actor(SecretType.Murder, year, secretText, 2, actor.ActID);
                            Game.history.SetSecret(secret_1);
                            actor.AddSecret(secret_1.SecretID);
                            truth = false;
                        }
                        break;
                    case 8:
                    case 9:
                        //executed
                        eventText += string.Format("was summarily executed by the {0}s after a period of captivity", Enemies);
                        actorEvent = HistActorIncident.Died;
                        //replace Bannerlord
                        if (actor is BannerLord) { ReplaceBannerLord((BannerLord)actor); }
                        else if (actor is Noble) { ReplaceNobleLord((Noble)actor); }
                        Game.history.RemoveActor(actor, Game.gameRevolt, ActorGone.Executed);
                        break;
                    default:
                        Game.SetError(new Error(31, "Invalid case"));
                        break;
                }
                //store record
                Record record_actor = new Record(eventText, actor.ActID, 0, actor.RefID, Game.gameRevolt, HistActorIncident.Captured, truth);
                if (actorEvent == HistActorIncident.Died)
                { record_actor.AddActorIncident(HistActorIncident.Died); }
                Game.world.SetRecord(record_actor);
            }
        }

        /// <summary>
        /// creates text description of fate of royal family
        /// </summary>
        internal void CreateRoyalFamilyFate()
        {
            listRoyalFamilyFate.Add("");
            string text_1 = string.Format("King {0} publicly paraded and beheaded the traitor {1} and his wife, {2}. ", NewKing.Name, OldKing.Name, OldQueen.Name);
            string text_2 = "Their children were ruthlessly hunted down and killed. Blood shall not linger.";
            string textToWrap = text_1 + text_2;
            listRoyalFamilyFate.AddRange(Game.utility.WordWrap(textToWrap, 120));
            string text_3 = string.Format("Unfortunately the heir, {0}, managed to flee across the ocean. He shall not be forgotten...", OldHeir.Name);
            listRoyalFamilyFate.Add("");
            listRoyalFamilyFate.Add(text_3);
        }

        /// <summary>
        /// Determines how many men the Lord of House will put into the field when the call to arms comes (varies with treachery)
        /// </summary>
        /// <param name="house"></param>
        /// <returns></returns>
        internal int GetMenAtArms(House house)
        {
            float menAtArms = 0;
            //get Lord's treachery (adjusts number fielded) low treachery -> many, high treachery -> few
            int lordID = house.LordID;
            Passive lord = Game.world.GetPassiveActor(lordID);
            menAtArms = (float)(6 - lord.GetSkill(SkillType.Treachery, SkillAge.Fifteen, true)) / 5 * (float)house.MenAtArms;
            Console.WriteLine("Aid {0}, {1} has provided {2} men", lord.ActID, lord.Name, menAtArms);
            return Convert.ToInt32(menAtArms);
        }

        /// <summary>
        /// determines fate of Advisors (Royal and Noble) when required
        /// </summary>
        /// <param name="advisor"></param>
        /// <returns>true if Advisor dismissed</returns>
        internal bool FateOfAdvisor(Advisor advisor, Passive liege)
        {
            bool advisorDied = false;
            string[] evilFate = new string[] { "buried alive", "fed to the swine", "burnt at the stake","eaten alive by rats", "drowned in the well", "choked on excrement" };
            if (advisor.advisorNoble > AdvisorNoble.None)
            {
                //Noble advisor
            }
            else if (advisor.advisorRoyal > AdvisorRoyal.None)
            {
                //Royal advisor
                Record record = null;
                Secret_Actor secret = null;
                string secretText;
                string eventText;
                int range = 100;
                //the kings guard and the hand are automatically dismissed
                if (advisor.advisorRoyal == AdvisorRoyal.Commander_of_Kings_Guard || advisor.advisorRoyal == AdvisorRoyal.Hand_of_the_King) { range = 1; }
                if (rnd.Next(range) < Game.constant.GetValue(Global.ADVISOR_REFUSAL))
                {
                    //advisor dismissed
                    advisorDied = true;
                    advisor.Loyalty_Current = KingLoyalty.Old_King;
                    eventText = string.Format("{0} {1}, Aid {2}, refused to swear allegiance to King {3} and was dismissed", advisor.advisorRoyal, advisor.Name, advisor.ActID, liege.Name);
                    record = new Record(eventText, advisor.ActID, 1, 9999, Game.gameRevolt, HistActorIncident.Service);
                    Game.history.RemoveActor(advisor, Game.gameRevolt, ActorGone.Missing);
                    //chance of advisor being killed as a result -> secret (depends on New Kings treachery)
                    int liegeTreachery = liege.GetSkill(SkillType.Treachery);
                    string fate = evilFate[rnd.Next(0, evilFate.Length)];
                    if (rnd.Next(0, 6) < liegeTreachery )
                    {
                        secretText = string.Format("{0} {1}, Aid {2}, defied King {3} and was {4}", advisor.advisorRoyal, advisor.Name, advisor.ActID, liege.Name, fate);
                        secret = new Secret_Actor(SecretType.Murder, Game.gameRevolt, secretText, liegeTreachery, advisor.ActID);
                        advisor.AddSecret(secret.SecretID); liege.AddSecret(secret.SecretID);
                    }
                }
                else
                {
                    //advisor retained
                    advisor.Loyalty_Current = KingLoyalty.New_King;
                    eventText = string.Format("{0} {1}, Aid {2}, swears allegiance to King {3}", advisor.advisorRoyal, advisor.Name, advisor.ActID, liege.Name);
                    record = new Record(eventText, advisor.ActID, 1, 9999, Game.gameRevolt, HistActorIncident.Service);
                    //chance of advisor being a secret sympathiser to the Old King, a fifth Column -> secret (depends on Advisors treachery)
                    int advisorTreachery = liege.GetSkill(SkillType.Treachery);
                    string fate = evilFate[rnd.Next(0, evilFate.Length)];
                    if (rnd.Next(100) < (advisorTreachery * 5))
                    {
                        secretText = string.Format("{0} {1}, Aid {2}, is a fifth column who secretly despises King {3}", advisor.advisorRoyal, advisor.Name, advisor.ActID, liege.Name);
                        secret = new Secret_Actor(SecretType.Murder, Game.gameRevolt, secretText, advisorTreachery, advisor.ActID);
                        advisor.AddSecret(secret.SecretID);
                    }
                }
                //save record & secret if applicable
                Game.world.SetRecord(record);
                Game.history.SetSecret(secret);
            }
            return advisorDied;
        }

        /// <summary>
        /// swaps Old King's major house for a promoted bannerlord from his stable (turned traitor and is being rewarded)
        /// </summary>
        internal void CreateNewMajorHouse(List<HouseStruct> listUnusedMinorHouses, List<Advisor> listOfRoyalAdvisors)
        {
            Console.WriteLine(Environment.NewLine + "--- CreateNewMajorHouse");
            Passive oldBannerLord = Game.world.GetPassiveActor(TurnCoat);
            Console.WriteLine("turncoatActor {0}, {1}, ActID: {2} RefID: {3} Loc: {4}", oldBannerLord.Name, oldBannerLord.Handle, oldBannerLord.ActID, oldBannerLord.RefID, 
                Game.world.GetLocationName(oldBannerLord.LocID));
           
            if (oldBannerLord != null && oldBannerLord.Status == ActorStatus.AtLocation)
            {
                string descriptor;
                //turncoat details
                int oldBannerLordRefID = oldBannerLord.RefID;
                House turncoatHouse = Game.world.GetHouse(oldBannerLordRefID);
                //old king's house
                House tempHouse = Game.world.GetHouse(RoyalRefIDOld);
                MajorHouse oldkingHouse = tempHouse as MajorHouse;
                //new MajorHouse
                MajorHouse newMajorhouse = new MajorHouse();
                int houseID = Game.network.GetNumUniqueHouses() + 1;
                int amt;
                newMajorhouse.HouseID = houseID;
                Console.WriteLine("Old King House {0}, {1}", oldkingHouse.Name, oldkingHouse.Motto);
                //Ref ID is 99 (all relevant datapoints are changed to this, not previous bannerlord refID as this is > 100 and causes issues with code as it thinks it's still a minor house)
                int refID = 99;
                //set up road mapLayer
                Game.map.InitialiseRoadLayer(oldkingHouse.LocID);
                //set up new house
                newMajorhouse.Name = turncoatHouse.Name;
                newMajorhouse.Motto = turncoatHouse.Motto;
                newMajorhouse.Banner = turncoatHouse.Banner;
                newMajorhouse.ArcID = turncoatHouse.ArcID;
                //newMajorhouse.RefID = turncoatHouse.RefID;
                newMajorhouse.RefID = refID;
                newMajorhouse.LocName = oldkingHouse.LocName;
                newMajorhouse.MenAtArms = oldkingHouse.MenAtArms;
                newMajorhouse.CastleWalls = oldkingHouse.CastleWalls;
                newMajorhouse.Branch = oldkingHouse.Branch;
                newMajorhouse.LordID = oldBannerLord.ActID;
                newMajorhouse.LocID = oldkingHouse.LocID;
                newMajorhouse.Loyalty_Current = KingLoyalty.New_King;
                newMajorhouse.Loyalty_AtStart = KingLoyalty.Old_King;
                newMajorhouse.SetBannerLords(oldkingHouse.GetBannerLords());
                newMajorhouse.SetLordLocations(oldkingHouse.GetBannerLordLocations());
                newMajorhouse.SetHousesToCapital(oldkingHouse.GetHousesToCapital());
                newMajorhouse.SetHousesToConnector(oldkingHouse.GetHousesToConnector());
                newMajorhouse.SetSecrets(turncoatHouse.GetSecrets());
                newMajorhouse.Resources = oldkingHouse.Resources;

                //update Map with refID and houseID for loc
                Location locLord = Game.network.GetLocation(newMajorhouse.LocID);
                Game.map.SetMapInfo(MapLayer.HouseID, locLord.GetPosX(), locLord.GetPosY(), houseID);
                Game.map.SetMapInfo(MapLayer.RefID, locLord.GetPosX(), locLord.GetPosY(), refID);
                Console.WriteLine("loc {0}:{1}, houseID: {2}, refID: {3}", locLord.GetPosX(), locLord.GetPosY(), houseID, refID);
                //update Loc details
                locLord.HouseID = houseID;
                locLord.RefID = refID;

                //new MinorHouse
                MinorHouse newMinorHouse = new MinorHouse();
                //update bannerlord House details for promoted guy (House record stays, just updated)
                int index = rnd.Next(0, listUnusedMinorHouses.Count);
                HouseStruct minorStruct = listUnusedMinorHouses[index];
                newMinorHouse.Name = minorStruct.Name;
                newMinorHouse.RefID = minorStruct.RefID;
                newMinorHouse.Motto = minorStruct.Motto;
                newMinorHouse.Banner = minorStruct.Banner;
                newMinorHouse.LocName = minorStruct.Capital;
                newMinorHouse.ArcID = minorStruct.ArcID;
                newMinorHouse.Loyalty_AtStart = KingLoyalty.Old_King;
                newMinorHouse.Loyalty_Current = KingLoyalty.New_King;
                newMinorHouse.LocID = turncoatHouse.LocID;
                newMinorHouse.MenAtArms = turncoatHouse.MenAtArms;
                newMinorHouse.Branch = turncoatHouse.Branch;
                newMinorHouse.CastleWalls = turncoatHouse.CastleWalls;
                newMinorHouse.Resources = turncoatHouse.Resources;
                //remove from list to prevent future use
                listUnusedMinorHouses.RemoveAt(index);
                Console.WriteLine("{0}, {1}, RefID: {2}", minorStruct.Name, minorStruct.Motto, minorStruct.RefID);

                //need a new Bannerlord Actor
                int bannerLocID = newMinorHouse.LocID;
                Location locBannerLord = Game.network.GetLocation(bannerLocID); 
                Console.WriteLine("bannerlord comes from {0}, LocID: {1} ({2}:{3})", Game.world.GetLocationName(bannerLocID), bannerLocID, locBannerLord.GetPosX(), locBannerLord.GetPosY());
                Position pos = locBannerLord.GetPosition();
                BannerLord newBannerLord = Game.history.CreateBannerLord(minorStruct.Name, pos, bannerLocID, minorStruct.RefID, houseID);
                Game.world.SetPassiveActor(newBannerLord);
                Console.WriteLine("new Bannerlord {0}, ActID: {1}", newBannerLord.Name, newBannerLord.ActID);
                newBannerLord.Loyalty_AtStart = KingLoyalty.New_King;
                newBannerLord.Loyalty_Current = KingLoyalty.New_King;
                newBannerLord.Lordship = Game.gameRevolt;
                newBannerLord.AddRelEventPlyr(new Relation("Grateful to the New King for their Title", "Supports New King", -45));
                newMinorHouse.LordID = newBannerLord.ActID;
                
                //need to update house.ListOfBannerLords (remove old refID, add new)
                List<int> tempLords = new List<int>();
                MajorHouse liegeLordHouse = null;
                if (oldBannerLord.HouseID == oldkingHouse.HouseID)
                { liegeLordHouse = newMajorhouse;  }
                else
                { liegeLordHouse = Game.world.GetGreatHouse(oldBannerLord.HouseID); }
                Console.WriteLine("Liege Lord House {0}", liegeLordHouse.Name);
                tempLords.AddRange(liegeLordHouse.GetBannerLords());
                index = tempLords.FindIndex(a => a == oldBannerLordRefID);
                if (index > -1)
                {
                    tempLords.RemoveAt(index);
                    tempLords.Add(minorStruct.RefID);
                    liegeLordHouse.SetBannerLords(tempLords);
                    Console.WriteLine("Remove index: {0}, Add RefID: {1}", index, minorStruct.RefID);
                }
                else { Console.WriteLine("Not found in listOfBannerLords"); }

                //loop through turncoat bannerlords and update loc details
                List<int> tempList = newMajorhouse.GetBannerLordLocations();
                foreach( int locID in tempList)
                {
                    Location locTemp = Game.network.GetLocation(locID);
                    Game.map.SetMapInfo(MapLayer.HouseID, locTemp.GetPosX(), locTemp.GetPosY(), houseID);
                    Console.WriteLine("BannerLord loc {0}:{1}, houseID: {2}", locTemp.GetPosX(), locTemp.GetPosY(), houseID);
                    //update Loc details
                    locTemp.HouseID = houseID;
                }

                //need to create a new Noble actor with details of old BannerLord (must have same ActID)
                Noble newLord = new Noble();

                //need to update original Bannerlord actor (now Lord) details
                newLord.ActID = oldBannerLord.ActID;
                //newLord.RefID = oldBannerLord.RefID;
                newLord.RefID = refID;
                newLord.HouseID = oldBannerLord.HouseID;
                newLord.Name = oldBannerLord.Name;
                newLord.Handle = oldBannerLord.Handle;
                newLord.arrayOfSkillID = oldBannerLord.arrayOfSkillID;
                newLord.arrayOfTraitEffects = oldBannerLord.arrayOfTraitEffects;
                newLord.arrayOfSkillInfluences = oldBannerLord.arrayOfSkillInfluences;
                newLord.arrayOfTraitNames = oldBannerLord.arrayOfTraitNames;
                newLord.Type = ActorType.Lord;
                newLord.Realm = ActorRealm.Head_of_Noble_House;
                newLord.LocID = locLord.LocationID;
                newLord.Status = ActorStatus.AtLocation;
                newLord.HouseID = houseID;
                newLord.Loyalty_Current = KingLoyalty.New_King;
                newLord.Loyalty_AtStart = KingLoyalty.Old_King;
                newLord.GenID = 1;
                newLord.Lordship = Game.gameRevolt;
                newLord.Age = oldBannerLord.Age;
                newLord.Born = oldBannerLord.Born;
                newLord.BornRefID = oldBannerLord.BornRefID;
                newLord.Sex = oldBannerLord.Sex;
                newLord.SetSecrets(oldBannerLord.GetSecrets());
                newLord.Resources = oldBannerLord.Resources;
                newLord.AddRelEventPlyr(new Relation("Grateful to the New King for their Title", "Supports New King", -45));

                //remove oldBannerLord from dictionaries and add newLord
                Game.world.RemovePassiveActor(oldBannerLord.ActID);
                Game.world.AddPassiveActor(newLord);

                //update actors for different locations
                locLord.AddActor(newLord.ActID);
                locBannerLord.RemoveActor(oldBannerLord.ActID);

                //remove old Bannerlord house from dictAllHouses
                Game.world.RemoveMinorHouse(oldBannerLordRefID);

                //remove oldking House from relevant dictionaries
                Game.world.RemoveMajorHouse(oldkingHouse);
                //remove old bannerlord house, add new from dictAllHouses
                //Game.world.RemoveMinorHouse(oldBannerLordRefID);
                Game.world.AddOtherHouse(newMinorHouse);
                //add house to world dictionaries (do after turncoatHouse update otherwise two identical houses in world.dictAllHouses)
                Game.world.AddMajorHouse(newMajorhouse);
                //sort list Of GreatHouses by Power
                Game.world.SortMajorHouses();

                //update bannerlords of Great House 
                List<int> tempBannerLords = newMajorhouse.GetBannerLords();
                foreach (int lordRefID in tempBannerLords)
                {
                    House bannerHouse = (MinorHouse)Game.world.GetHouse(lordRefID);
                    //MinorHouse bannerHouse = tempbannerHouse as MinorHouse;
                    bannerHouse.HouseID = newMajorhouse.HouseID;
                }

                //update Map with refID and houseID for loc
                Game.map.SetMapInfo(MapLayer.RefID, locBannerLord.GetPosX(), locBannerLord.GetPosY(), newMinorHouse.RefID);
                Console.WriteLine("Updating MapLayer -> loc {0}:{1}, refID: {2}", locBannerLord.GetPosX(), locBannerLord.GetPosY(), newMinorHouse.RefID);
                //update Loc details
                locBannerLord.RefID = newMinorHouse.RefID;
                
                //advisors - castellan, in oldkings house need replacing
                foreach( Advisor advisor in listOfRoyalAdvisors)
                {
                    string fate = "dismissed";
                    if (rnd.Next(100) < 40)
                    {
                        fate = "beheaded";
                        Game.history.RemoveActor(advisor, Game.gameRevolt, ActorGone.Executed);
                        //record - fate of old king advisor
                        descriptor = string.Format("{0} {1}, Aid {2} was {3} on orders of the new Lord {4}", advisor.advisorNoble, advisor.Name, advisor.ActID, fate, newLord.Name);
                        Record record_6 = new Record(descriptor, advisor.ActID, advisor.LocID, advisor.RefID, Game.gameRevolt, HistActorIncident.Service);
                        record_6.AddActorIncident(HistActorIncident.Died);
                        Game.world.SetRecord(record_6);
                    }
                    else
                    {
                        Game.history.RemoveActor(advisor, Game.gameRevolt, ActorGone.Missing);
                        //record - fate of old king advisor
                        descriptor = string.Format("{0} {1}, Aid {2} was {3} on orders of the new Lord {4}", advisor.advisorNoble, advisor.Name, advisor.ActID, fate, newLord.Name);
                        Record record_6 = new Record(descriptor, advisor.ActID, advisor.LocID, advisor.RefID, Game.gameRevolt, HistActorIncident.Service);
                        Game.world.SetRecord(record_6);
                    }
                }
                //create castellan
                Advisor castellan = Game.history.CreateAdvisor(locLord.GetPosition(), newMajorhouse.LocID, newMajorhouse.RefID, houseID, ActorSex.Male, AdvisorNoble.Castellan, AdvisorRoyal.None);
                castellan.Loyalty_AtStart = newMajorhouse.Loyalty_AtStart; castellan.Loyalty_Current = newMajorhouse.Loyalty_Current;
                amt = rnd.Next(40) * -1;
                castellan.AddRelEventPlyr(new Relation("Grateful to the New King for their position", "Supports New King", amt));
                Game.world.SetPassiveActor(castellan);
                //create maester
                Advisor maester = Game.history.CreateAdvisor(locLord.GetPosition(), newMajorhouse.LocID, newMajorhouse.RefID, houseID, ActorSex.Male, AdvisorNoble.Maester, AdvisorRoyal.None);
                maester.Loyalty_AtStart = newMajorhouse.Loyalty_AtStart; maester.Loyalty_Current = newMajorhouse.Loyalty_Current;
                amt = rnd.Next(40) * -1;
                maester.AddRelEventPlyr(new Relation("Grateful to the New King for their position", "Supports New King", amt));
                Game.world.SetPassiveActor(maester);
                //create septon
                Advisor septon = Game.history.CreateAdvisor(locLord.GetPosition(), newMajorhouse.LocID, newMajorhouse.RefID, houseID, ActorSex.Male, AdvisorNoble.Septon, AdvisorRoyal.None);
                septon.Loyalty_AtStart = newMajorhouse.Loyalty_AtStart; septon.Loyalty_Current = newMajorhouse.Loyalty_Current;
                amt = rnd.Next(40) * -1;
                septon.AddRelEventPlyr(new Relation("Grateful to the New King for their position", "Supports New King", amt));
                Game.world.SetPassiveActor(septon);
                //create new Knight
                Knight knight = Game.history.CreateKnight(locLord.GetPosition(), newMajorhouse.LocID, newMajorhouse.RefID, houseID);
                knight.Loyalty_AtStart = newMajorhouse.Loyalty_AtStart; knight.Loyalty_Current = newMajorhouse.Loyalty_Current;
                amt = rnd.Next(40) * -1;
                knight.AddRelEventPlyr(new Relation("Grateful to the New King for their position", "Supports New King", amt));
                Game.world.SetPassiveActor(knight);
                //create new wife
                Noble wife = (Noble)Game.history.CreateStartingHouseActor(newMajorhouse.Name, ActorType.Lady, locLord.GetPosition(), locLord.LocationID, newMajorhouse.RefID, houseID, ActorSex.Female, WifeStatus.First_Wife);
                wife.Loyalty_AtStart = newMajorhouse.Loyalty_AtStart; wife.Loyalty_Current = newMajorhouse.Loyalty_Current;
                amt = rnd.Next(40) * -1;
                wife.AddRelEventPlyr(new Relation("Grateful to the New King for their position", "Supports New King", amt));
                Game.world.SetPassiveActor(wife);
                //create family
                Game.history.CreateFamily(newLord, wife, newMinorHouse.LocName);

                //record - new bannerlord
                descriptor = string.Format("{0}, Aid {1}, assumes Lordship, BannerLord of House {2}, age {3}", newBannerLord.Name, newBannerLord.ActID, newMinorHouse.Name, newBannerLord.Age);
                Record record_0 = new Record(descriptor, newBannerLord.ActID, newBannerLord.LocID, newBannerLord.RefID, newBannerLord.Lordship, HistActorIncident.Lordship);
                Game.world.SetRecord(record_0);
                //record - old bannerlord does the dirty (public knowledge, hence no secret)
                descriptor = string.Format("{0}, Aid {1}, dramatically changed sides during {2} and captured Old King {3} ", oldBannerLord.Name, oldBannerLord.ActID, listOfUprisingBattles[listOfUprisingBattles.Count - 1], OldKing.Name);
                Record record_5 = new Record(descriptor, oldBannerLord.ActID, oldBannerLord.LocID, oldBannerLord.RefID, Game.gameRevolt, HistActorIncident.Lordship);
                Game.world.SetRecord(record_5);
                //record - old bannerlord promoted
                descriptor = string.Format("{0}, Aid {1}, has been elevated to a Noble Lord by decree of King {2}", oldBannerLord.Name, oldBannerLord.ActID, NewKing.Name);
                Record record_1 = new Record(descriptor, oldBannerLord.ActID, oldBannerLord.LocID, oldBannerLord.RefID, Game.gameRevolt, HistActorIncident.Lordship);
                Game.world.SetRecord(record_1);
                //House record - new House created
                descriptor = string.Format("King {0} has gifted all property and lands belonging to House {1} to House {2}", NewKing.Name, oldkingHouse.Name, newMajorhouse.Name);
                Record record_4 = new Record(descriptor, newMajorhouse.LocID, newMajorhouse.RefID, Game.gameRevolt, HistHouseIncident.Ownership);
                Game.world.SetRecord(record_4);
                //record - new GreatLord
                descriptor = string.Format("{0}, Aid {1}, assumes Lordship of House {2}, age {3}", oldBannerLord.Name, oldBannerLord.ActID, newMajorhouse.Name, oldBannerLord.Age);
                Record record_2 = new Record(descriptor, oldBannerLord.ActID, oldBannerLord.LocID, oldBannerLord.RefID, Game.gameRevolt, HistActorIncident.Lordship);
                Game.world.SetRecord(record_2);
                //House record - old King's house stolen
                descriptor = string.Format("The false King {0} has stolen all property and lands belonging to House {1}", NewKing.Name, oldkingHouse.Name);
                Record record_3 = new Record(descriptor, OldKing.LocID, OldKing.RefID, Game.gameRevolt, HistHouseIncident.Ownership);
                Game.world.SetRecord(record_3);

                //resentment of other new king loyal noble lords at promotion of bannerlord

            }
            else
            { Game.SetError(new Error(33, "Invalid TurnCoat.ActorID")); }
        }

        /// <summary>
        /// Replace a Bannerlord that has died (use this instead as it calls the history method)
        /// </summary>
        /// <param name="deadLord"></param>
        /// <param name="surname"></param>
        internal void ReplaceBannerLord(BannerLord deadLord, string surname = null)
        {
            try
            {
                House house = Game.world.GetHouse(deadLord.RefID);
                //if no surname provided then a brother of the same house steps forward
                if (surname == null)
                { surname = house.Name; }
                Location loc = Game.network.GetLocation(deadLord.LocID);
                BannerLord newLord = Game.history.CreateBannerLord(surname, loc.GetPosition(), deadLord.LocID, deadLord.RefID, deadLord.HouseID);
                newLord.Loyalty_Current = KingLoyalty.New_King;
                newLord.Loyalty_AtStart = deadLord.Loyalty_AtStart;
                newLord.AddRelEventPlyr(new Relation("Loyal to the New King", "Supports New King", -25));
                newLord.Lordship = Game.gameRevolt - 1;
                Game.world.SetPassiveActor(newLord);
                //update house
                house.LordID = newLord.ActID;

                //record of lordship & taking over
                string descriptor = string.Format("{0}, Aid {1}, brother of {2}, assumes Lordship of House {3}, age {4}", newLord.Name, newLord.ActID, deadLord.Name, house.Name, newLord.Age);
                Record record_0 = new Record(descriptor, newLord.ActID, newLord.LocID, newLord.RefID, newLord.Lordship, HistActorIncident.Lordship);
                Game.world.SetRecord(record_0);

                //debug
                Console.WriteLine("New Bannerlord for House {0} -> {1}, Aid {2}", house.Name, newLord.Name, newLord.ActID);
            }
            catch (Exception e)
            { Game.SetError(new Error(39, e.Message)); }
        }

        /// <summary>
        /// Replace a Great Lord with heir (plus Regent if needed) if he has died during the Uprising
        /// </summary>
        /// <param name="deadLord"></param>
        internal void ReplaceNobleLord(Noble deadLord)
        {
            int amt;
            string descriptor;
            try
            { 
                House house = Game.world.GetHouse(deadLord.RefID);
                //find Heir
                SortedDictionary<int, ActorRelation> dictFamily = deadLord.GetFamily();
                Noble heir = null;
                Noble wife = null;
                int year = Game.gameRevolt;
                //loop family of dead lord
                foreach(KeyValuePair<int, ActorRelation> kvp in dictFamily)
                {
                    if (kvp.Value == ActorRelation.Son)
                    {
                        Noble son = (Noble)Game.world.GetPassiveActor(kvp.Key);
                        if (son.InLine == 1)
                        { heir = son; }
                    }
                    if (kvp.Value == ActorRelation.Wife)
                    { wife = (Noble)Game.world.GetPassiveActor(kvp.Key); }
                }
                //son of Age?
                if (heir != null)
                {
                    //heir assumes Lordship
                    heir.Type = ActorType.Lord;
                    heir.Realm = ActorRealm.Head_of_House;
                    heir.Lordship = year;
                    amt = rnd.Next(40);
                    if (deadLord.Loyalty_Current == KingLoyalty.New_King) { amt *= -1; heir.AddRelEventPlyr(new Relation("Loyal to New King", "Supports New King", amt)); }
                    else { heir.AddRelEventPlyr(new Relation("Loyal to Old King", "Supports Old King", amt)); }
                    house.LordID = heir.ActID;
                    //heir is off age to take control
                    if (heir.Age >= 15)
                    {
                        //record
                        descriptor = string.Format("{0}, Aid {1}, son of {2}, assumes Lordship of House {3}, age {4}", heir.Name, heir.ActID, deadLord.Name, house.Name, heir.Age);
                        Record record_0 = new Record(descriptor, heir.ActID, heir.LocID, heir.RefID, heir.Lordship, HistActorIncident.Lordship);
                        Game.world.SetRecord(record_0);
                    }
                    //underage heir, his mother becomes Regent
                    else if (wife != null)
                    {
                        //record
                        descriptor = string.Format("{0}, Aid {1}, assumes Lordship (with a Regent) of House {2}, age {3}", heir.Name, heir.ActID, house.Name, heir.Age);
                        Record record_1 = new Record(descriptor, heir.ActID, heir.LocID, heir.RefID, heir.Lordship, HistActorIncident.Lordship);
                        Game.world.SetRecord(record_1);
                        //wife as regent
                        wife.Realm = ActorRealm.Regent;
                        Game.history.SetInfluence(heir, wife, SkillType.Combat);
                        Game.history.SetInfluence(heir, wife, SkillType.Wits);
                        Game.history.SetInfluence(heir, wife, SkillType.Charm);
                        Game.history.SetInfluence(heir, wife, SkillType.Leadership);
                        Game.history.SetInfluence(heir, wife, SkillType.Treachery);
                        //record
                        descriptor = string.Format("{0}, Aid {1}, wife of {2}, assumes Regency of House {3}, age {4}", wife.Name, wife.ActID, deadLord.Name, house.Name, wife.Age);
                        Record record_2 = new Record(descriptor, heir.ActID, heir.LocID, heir.RefID, heir.Lordship, HistActorIncident.Lordship);
                        record_2.AddActor(wife.ActID);
                        Game.world.SetRecord(record_2);
                    }
                    //deadlord's brother steps in, as wife is dead, to become regent
                    else if (wife == null)
                    {
                        Location loc = Game.network.GetLocation(house.LocID);
                        Position pos = loc.GetPosition();
                        //create new lord
                        Noble brother = (Noble)Game.history.CreateRegent(house.Name, pos, house.LocID, house.RefID, house.HouseID);
                        amt = rnd.Next(40);
                        if (deadLord.Loyalty_Current == KingLoyalty.New_King) { amt *= -1;  brother.AddRelEventPlyr(new Relation("Loyal to New King", "Supports New King", amt)); }
                        else { brother.AddRelEventPlyr(new Relation("Loyal to the Old King", "Supports Old King", amt)); }
                        Game.history.SetInfluence(heir, brother, SkillType.Combat);
                        Game.history.SetInfluence(heir, brother, SkillType.Wits);
                        Game.history.SetInfluence(heir, brother, SkillType.Charm);
                        Game.history.SetInfluence(heir, brother, SkillType.Leadership);
                        Game.history.SetInfluence(heir, brother, SkillType.Treachery);
                        //record
                        descriptor = string.Format("{0}, Aid {1}, brother of {2}, assumes Regency of House {3}, age {4}", brother.Name, brother.ActID, deadLord.Name, house.Name, brother.Age);
                        Record record_3 = new Record(descriptor, heir.ActID, heir.LocID, heir.RefID, heir.Lordship, HistActorIncident.Lordship);
                        record_3.AddActor(brother.ActID);
                        Game.world.SetRecord(record_3);
                    }
                }
                else
                { Game.SetError(new Error(41, "First-in-Line to inherit isn't present")); }
            }
            catch (Exception e)
            { Game.SetError(new Error(40, e.Message)); }
        }


        //methods above here
    }
}
