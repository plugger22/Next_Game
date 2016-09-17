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
    /// Keeps track of all key game lore (backstory)
    /// </summary>
    public class Lore
    {
        static Random rnd;
        //HouseID's
        public int RoyalHouseOld { get; set; }
        public int RoyalHouseNew { get; set; }
        public int RoyalHouseCurrent { get; set; }
        //Royal & Rebel Family and retainers
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
            if (influencer > 0 && Game.world.CheckActorPresent(influencer, 1) && OldKing.CheckTraitInfluenced(TraitType.Wits))
            { oldKing_Wits = OldKing.GetTrait(TraitType.Wits, TraitAge.Fifteen, true); }
            else { oldKing_Wits = OldKing.GetTrait(TraitType.Wits); }
            //dumb king (2 pool entries if wits 2 stars and 5 entries if wits 1 star)
            if (oldKing_Wits == 2) { for (int i = 0; i < 2; i++) { listWhyPool.Add(RevoltReason.Stupid_OldKing); } } 
            else if (oldKing_Wits == 1) { for (int i = 0; i < 5; i++) { listWhyPool.Add(RevoltReason.Stupid_OldKing); } }

            //check new king treachery
            int newKing_Treachery;
            influencer = NewKing.Influencer;
            if (influencer > 0 && Game.world.CheckActorPresent(influencer, 1) && NewKing.CheckTraitInfluenced(TraitType.Treachery))
            { newKing_Treachery = NewKing.GetTrait(TraitType.Treachery, TraitAge.Fifteen, true); }
            else { newKing_Treachery = NewKing.GetTrait(TraitType.Treachery); }
            //treacherous new king grabs power (2 pool entries if 4 starts, 5 entries if treachery 5 stars)
            if (newKing_Treachery == 4) { for (int i = 0; i < 2; i++) { listWhyPool.Add(RevoltReason.Treacherous_NewKing); } }
            else if (newKing_Treachery == 5) { for (int i = 0; i < 5; i++) { listWhyPool.Add(RevoltReason.Treacherous_NewKing); } }

            //both kings leadership
            int oldKing_Leadership = OldKing.GetTrait(TraitType.Leadership);
            int newKing_Leadership = NewKing.GetTrait(TraitType.Leadership);

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
            int oldKing_Charm = OldKing.GetTrait(TraitType.Charm);
            int newKing_Charm = NewKing.GetTrait(TraitType.Charm);
            int oldQueen_Charm = OldQueen.GetTrait(TraitType.Charm);
            int newQueen_Charm = NewQueen.GetTrait(TraitType.Charm);
            OldKing_Popularity = (Popularity)oldKing_Charm;
            NewKing_Popularity = (Popularity)newKing_Charm;
            OldQueen_Popularity = (Popularity)oldQueen_Charm;
            NewQueen_Popularity = (Popularity)newQueen_Charm;

            Console.WriteLine("King {0} was {1} by his people", OldKing.Name, OldKing_Popularity);
            Console.WriteLine("His Queen, {0}, was {1} with the common folk", OldQueen.Name, OldQueen_Popularity);
            Console.WriteLine(Environment.NewLine + "In {0} there was a great Revolt", Game.gameYear);

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
            HistKingdomEvent kingdomEvent;
            string descriptor = null;
            int index;
            int year = Game.gameStart - 1; //all conflict assumed to be one year prior to start of the new King's rule
            List<string> listOfBattles = new List<string>();

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
                { kingdomEvent = HistKingdomEvent.Siege; descriptor = string.Format("The Siege of {0}", loc.LocName); }
                else
                { kingdomEvent = HistKingdomEvent.Battle; descriptor = string.Format("The Battle of {0}", loc.LocName); }
                //create record
                string details = string.Format("{0} {1}", descriptor, Game.world.ShowLocationCoords(loc.LocationID));
                Record record = new Record(details, loc.LocationID, loc.HouseRefID, year, kingdomEvent);
                Game.world.SetRecord(record);
                //add to list of battles to enable actor events to be fleshed out
                listOfBattles.Add(descriptor);
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
                    if (kingdomEvent == HistKingdomEvent.Battle)
                    { text_1 += string.Format(" {0} a Royalist force of {1:N0} at {2}. ", array_Battle[rnd.Next(0, array_Battle.Length)], royalForces, descriptor); }
                    else
                    { text_1 += string.Format(" {0} a Royalist stronghold at {1}. ", array_Siege[rnd.Next(0, array_Siege.Length)], descriptor); }
                    //outcome text
                    if (kingdomEvent == HistKingdomEvent.Battle)
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
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorEvent.Captured);
                                break;
                            case 3:
                            case 4:
                                //knight killed
                                listOfTempKnights.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", knight.Name, knight.ActID, descriptor, Friends, knight.Age);
                                Console.WriteLine(string.Format("Knight {0}, Aid {1}, was killed by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor));
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorEvent.Died);
                                Game.history.RemoveDeadActor(knight, year, ActorDied.Battle);
                                break;
                            case 5:
                            case 6:
                            case 7:
                                //knight wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", knight.Name, knight.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("Knight {0}, Aid {1}, was wounded by the {2} during {3}", knight.Name, knight.ActID, Enemies, descriptor));
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorEvent.Wounded);
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
                                int knightCombat = knight.GetTrait(TraitType.Combat);
                                int knightTreachery = knight.GetTrait(TraitType.Treachery);
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
                                    }
                                }
                                record_knight = new Record(eventText, knight.ActID, loc.LocationID, knight.RefID, year, HistActorEvent.Deed);
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_knight != null) { record_knight.AddActorEvent(HistActorEvent.Conflict); Game.world.SetRecord(record_knight); }
                    }

                    //Royalist Lords
                    if (listOfTempRoyals.Count > 0)
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
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorEvent.Captured);
                                break;
                            case 2:
                                //killed
                                listOfTempRoyals.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", royal.Name, royal.ActID, descriptor, Friends, royal.Age);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was killed by the {3} during {4}", royal.Type, royal.Name, royal.ActID, Enemies, descriptor));
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorEvent.Died);
                                Game.history.RemoveDeadActor(royal, year, ActorDied.Battle);
                                break;
                            case 3:
                            case 4:
                            case 5:
                                //wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", royal.Name, royal.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was wounded by the {3} during {4}", royal.Type, royal.Name, royal.ActID, Enemies, descriptor));
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorEvent.Wounded);
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
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorEvent.Leadership);
                                //was it because they were a traitor? -> secret
                                int lordTreachery = royal.GetTrait(TraitType.Treachery);
                                if (rnd.Next(0,6) < lordTreachery)
                                {
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "was a traitor who deliberately changed sides during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordTreachery, royal.ActID);
                                    Game.history.SetSecret(secret);
                                    royal.AddSecret(secret.SecretID);
                                    if (changedSides == false)
                                    {
                                        //change loyalty and swap lord from royalist to rebel list for any future battles
                                        royal.Loyalty_Current = KingLoyalty.New_King;
                                        listOfTempRoyals.RemoveAt(listIndex);
                                        listOfTempRebels.Add(royalID);
                                        Console.WriteLine("Moved to Rebel List");
                                    }
                                }
                                break;
                            case 8:
                            case 9:
                                //Good leadership
                                index = rnd.Next(0, array_GoodLeadership.Length);
                                eventText = string.Format("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_GoodLeadership[index], descriptor);
                                Console.WriteLine("{0}, Aid {1}, {2} during {3}", royal.Name, royal.ActID, array_GoodLeadership[index], descriptor);
                                record_royal = new Record(eventText, royal.ActID, loc.LocationID, royal.RefID, year, HistActorEvent.Leadership);
                                int lordLeadership = royal.GetTrait(TraitType.Leadership);
                                if (rnd.Next(1, 6) > lordLeadership)
                                {
                                    //poor leadership, took somebody else's glory -> secret
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} {3}", royal.Name, royal.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordLeadership, royal.ActID);
                                    Game.history.SetSecret(secret);
                                    royal.AddSecret(secret.SecretID);
                                }
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_royal != null) { record_royal.AddActorEvent(HistActorEvent.Conflict); Game.world.SetRecord(record_royal); }
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
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorEvent.Captured);
                                break;
                            case 2:
                                //killed
                                listOfTempRebels.RemoveAt(listIndex);
                                eventText = string.Format("{0}, Aid {1}, was killed during {2} while fighting for the {3}, age {4}", rebel.Name, rebel.ActID, descriptor, Friends, rebel.Age);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was killed by the {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, Enemies, descriptor));
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorEvent.Died);
                                Game.history.RemoveDeadActor(rebel, year, ActorDied.Battle);
                                break;
                            case 3:
                            case 4:
                            case 5:
                                //wounded
                                eventText = string.Format("{0}, Aid {1}, was wounded during {2} while fighting for the {3}", rebel.Name, rebel.ActID, descriptor, Friends);
                                Console.WriteLine(string.Format("{0} {1}, Aid {2}, was wounded by the {3} during {4}", rebel.Type, rebel.Name, rebel.ActID, Enemies, descriptor));
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorEvent.Wounded);
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
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorEvent.Leadership);
                                //was it because they were a traitor? -> secret
                                int lordTreachery = rebel.GetTrait(TraitType.Treachery);
                                if (rnd.Next(0, 6) < lordTreachery)
                                {
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "was a traitor who deliberately changed sides during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordTreachery, rebel.ActID);
                                    Game.history.SetSecret(secret);
                                    rebel.AddSecret(secret.SecretID);
                                    if (changedSides == false)
                                    {
                                        //change loyalty and swap lord from rebel list to royalist list for any future battles
                                        rebel.Loyalty_Current = KingLoyalty.Old_King;
                                        listOfTempRebels.RemoveAt(listIndex);
                                        listOfTempRoyals.Add(rebelID);
                                        Console.WriteLine("Moved to Rebel List");
                                    }
                                }
                                break;
                            case 7:
                            case 8:
                            case 9:
                                //Good leadership
                                index = rnd.Next(0, array_GoodLeadership.Length);
                                eventText = string.Format("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_GoodLeadership[index], descriptor);
                                Console.WriteLine("{0}, Aid {1}, {2} during {3}", rebel.Name, rebel.ActID, array_GoodLeadership[index], descriptor);
                                record_rebel = new Record(eventText, rebel.ActID, loc.LocationID, rebel.RefID, year, HistActorEvent.Leadership);
                                int lordLeadership = rebel.GetTrait(TraitType.Leadership);
                                if (rnd.Next(1, 6) > lordLeadership)
                                {
                                    //poor leadership, took somebody else's glory -> secret
                                    secretText = string.Format("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Console.WriteLine("{0}, Aid {1}, {2} {3}", rebel.Name, rebel.ActID, "cowardly stole somebody else's glory during", descriptor);
                                    Secret_Actor secret = new Secret_Actor(SecretType.Loyalty, year, secretText, lordLeadership, rebel.ActID);
                                    Game.history.SetSecret(secret);
                                    rebel.AddSecret(secret.SecretID);
                                }
                                break;
                            default:
                                Game.SetError(new Error(31, "Invalid case"));
                                break;
                        }
                        if (record_rebel != null) { record_rebel.AddActorEvent(HistActorEvent.Conflict); Game.world.SetRecord(record_rebel); }
                    }
                }
            }

            //work out what happened to Captured actors
            foreach (int actorID in listOfCapturedActors)
            {
                int rndNum = rnd.Next(10);
                Passive actor = Game.world.GetPassiveActor(actorID);
                eventText = string.Format("{0}, Aid {1}, was ", actor.Name, actor.ActID);
                HistActorEvent actorEvent = HistActorEvent.None;
                Friends = "Royalist";
                Enemies = "Rebel";
                bool truth = true;
                if (actor.Loyalty_Current == KingLoyalty.New_King) { Friends = "Rebel"; Enemies = "Royalist"; }
                switch (rndNum)
                {
                    case 0:
                    case 1:
                        eventText += string.Format("ransomed and released by his {0} captors", Enemies);
                        break;
                    case 2:
                    case 3:
                        eventText += string.Format("released unharmed by his {0} captors", Enemies);
                        break;
                    case 4:
                    case 5:
                        //tortured -> secret
                        eventText += string.Format("released unharmed by his {0} captors", Enemies);
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
                        actorEvent = HistActorEvent.Died;
                        Game.history.RemoveDeadActor(actor, Game.gameStart, ActorDied.Injuries);
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
                        eventText += string.Format("summarily executed by the {0}s after a period of captivity", Enemies);
                        actorEvent = HistActorEvent.Died;
                        Game.history.RemoveDeadActor(actor, Game.gameStart, ActorDied.Executed);
                        break;
                    default:
                        Game.SetError(new Error(31, "Invalid case"));
                        break;
                }
                //store record
                Record record_actor = new Record(eventText, actor.ActID, 0, actor.RefID, Game.gameStart, HistActorEvent.Captured, truth);
                if (actorEvent == HistActorEvent.Died)
                { record_actor.AddActorEvent(HistActorEvent.Died); }
                Game.world.SetRecord(record_actor);
            }
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
            menAtArms = (float)(6 - lord.GetTrait(TraitType.Treachery, TraitAge.Fifteen, true)) / 5 * (float)house.MenAtArms;
            Console.WriteLine("Aid {0}, {1} has provided {2} men", lord.ActID, lord.Name, menAtArms);
            return Convert.ToInt32(menAtArms);
        }
    }
}
