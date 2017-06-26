using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Cartographic;
using Next_Game.Event_System;

namespace Next_Game.Event_System
{
    /// <summary>
    /// Contains all Act One events
    /// </summary>
    public class ActOne
    {
        static Random rnd;

        public ActOne(int seed)
        {
            rnd = new Random(seed);
        }


        /// <summary>
        /// create a dynamic auto player location event for Act one - assumed to be at player's current location
        /// <param name="filter">Which group of people should the event focus on (from pool of people present at the location)</param>
        /// </summary>
        internal void CreateAutoLocEvent(EventAutoFilter filter, int actorID = 0)
        {
            //get player
            Player player = (Player)Game.world.GetPlayer();
            if (player != null)
            {
                Game.logTurn?.Write("- CreateAutoLocEvent (Director.cs)");
                List<Actor> listActors = new List<Actor>();
                List<Passive> listCourt = new List<Passive>();
                List<Passive> listAdvisors = new List<Passive>();
                List<Passive> listVisitors = new List<Passive>();
                List<Follower> listFollowers = new List<Follower>();
                List<Trigger> listTriggers = new List<Trigger>();
                int limit; //loop counter, prevents overshooting the # of available function keys
                int locID = player.LocID;
                int rndLocID, voyageDistance;
                int locType = 0; //1 - capital, 2 - MajorHouse, 3 - MinorHouse, 4 - Inn
                int talkRel = Game.constant.GetValue(Global.TALK_THRESHOLD);
                int speed = Game.constant.GetValue(Global.SEA_SPEED);
                int chance, voyageTime;
                string actorText = "unknown"; string optionText = "unknown"; string locName = "unknown";
                Location loc = Game.network.GetLocation(locID);
                if (loc != null)
                { locName = Game.display.GetLocationName(locID); }
                else { Game.SetError(new Error(73, "Invalid Loc (null)")); }
                int houseID = Game.map.GetMapInfo(MapLayer.HouseID, loc.GetPosX(), loc.GetPosY());
                int refID = Game.map.GetMapInfo(MapLayer.RefID, loc.GetPosX(), loc.GetPosY());
                House house = Game.world.GetHouse(refID);
                if (house == null && refID != 9999) { Game.SetError(new Error(118, "Invalid house (null)")); }
                string houseName = "Unknown";
                string tempText;
                if (refID > 0) { houseName = Game.world.GetHouseName(refID); }
                int testRefID; //which refID (loc) to use when checking who's present
                //what type of location?
                switch (loc.Type)
                {
                    case LocType.Capital: locType = 1; break;
                    case LocType.MajorHouse: locType = 2; break;
                    case LocType.MinorHouse: locType = 3; break;
                    case LocType.Inn:
                        {
                            locType = 4;
                            //can't be locals present at an Inn, only Visitors and Followers
                            if (filter == EventAutoFilter.Court) { filter = EventAutoFilter.Visitors; Game.SetError(new Error(118, "Invalid filter (Locals when at an Inn)")); }
                        }
                        break;
                    default:
                        Game.SetError(new Error(118, $"Invalid loc.Type \"{loc.Type}\""));
                        break;
                }
                //Get actors present at location
                List<int> actorIDList = loc.GetActorList();
                if (actorIDList.Count > 0)
                {
                    //get actual actors
                    for (int i = 0; i < actorIDList.Count; i++)
                    {
                        Actor tempActor = Game.world.GetAnyActor(actorIDList[i]);
                        if (tempActor != null)
                        {   //exclude player from list (they are always present) & you
                            if (tempActor.ActID != 1)
                            { listActors.Add(tempActor); Game.logTurn?.Write(string.Format(" [AutoEvent -> ActorList] \"{0}\", ID {1} added to list of Actors", tempActor.Name, tempActor.ActID)); }
                        }
                        else { Game.SetError(new Error(118, string.Format("Invalid tempActor ID {0} (Null)", actorIDList[i]))); }
                    }
                    //filter actors accordingly
                    for (int i = 0; i < listActors.Count; i++)
                    {
                        Actor actor = listActors[i];
                        if (actor is Passive)
                        {
                            Passive tempPassive = actor as Passive;
                            testRefID = refID;
                            if (locType == 1) { testRefID = Game.lore.RoyalRefIDNew; }
                            if (tempPassive.RefID == testRefID && !(actor is Advisor))
                            {
                                if (tempPassive.Type == ActorType.Lord || tempPassive.Age >= 15)
                                {
                                    listCourt.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEvent -> LocalList] \"{0}\", ID {1} added to list of Locals",
                                      tempPassive.Name, tempPassive.ActID));
                                }
                            }
                            else if (actor is Advisor)
                            {
                                listAdvisors.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEvent -> AdvisorList] \"{0}\", ID {1} added to list of Advisors",
                                  tempPassive.Name, tempPassive.ActID));
                            }
                            else
                            {
                                if (tempPassive.Age >= 15)
                                {
                                    listVisitors.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEvent -> VisitorList] \"{0}\", ID {1} added to list of Visitors",
                                      tempPassive.Name, tempPassive.ActID));
                                }
                            }
                        }
                        else if (actor is Follower)
                        {
                            Follower tempFollower = actor as Follower;
                            listFollowers.Add(tempFollower);
                            Game.logTurn?.Write(string.Format(" [AutoEvent -> FollowerList] \"{0}\", ID {1} added to list of Followers", tempFollower.Name, tempFollower.ActID));
                        }
                    }
                    //new event (auto location events always have eventPID of '1000' -> old version in Player dict is deleted before new one added)
                    EventPlayer eventObject = new EventPlayer(1000, "What to do?", EventFrequency.Low) { Category = EventCategory.AutoCreate, Status = EventStatus.Active, Type = ArcType.Location };
                    tempText = "";
                    switch (filter)
                    {
                        case EventAutoFilter.None:
                            eventObject.Text = string.Format("You are at {0}. How will you fill your day?", locName);

                            //option -> audience with local House member
                            if (listCourt.Count() > 0)
                            {
                                OptionInteractive option = null;
                                if (locType != 1)
                                {
                                    option = new OptionInteractive(string.Format("Seek an Audience with a member of House {0} ({1} present)", houseName, listCourt.Count));
                                    option.ReplyGood = string.Format("House {0} has agreed to allow the Ursurper to enter the Court", houseName);
                                }
                                else
                                {
                                    //capital
                                    option = new OptionInteractive(string.Format("Seek an Audience with a member of the Royal Household ({0} present)", listCourt.Count));
                                    option.ReplyGood = string.Format("The Royal Clerk has advised that the Ursurper has permission to enter the Court");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Court);
                                option.SetGoodOutcome(outcome);
                                listTriggers.Clear();
                                listTriggers.Add(new Trigger(TriggerCheck.Known, 0, 1, EventCalc.Equals, false));
                                listTriggers.Add(new Trigger(TriggerCheck.Introduction, 0, 1, EventCalc.Equals, false));
                                option.SetTriggers(listTriggers);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Advisor
                            if (listAdvisors.Count() > 0)
                            {
                                OptionInteractive option = null;
                                if (locType != 1)
                                {
                                    option = new OptionInteractive(string.Format("Seek an Audience with an Advisor to House {0} ({1} present)", houseName, listAdvisors.Count));
                                    option.ReplyGood = string.Format("House {0} is willing to let you talk to whoever you wish", houseName);
                                }
                                else
                                {
                                    //capital
                                    option = new OptionInteractive(string.Format("Seek an Audience with a Royal Advisor ({0} present)", listAdvisors.Count));
                                    option.ReplyGood = string.Format("The Royal Clerk has advised that the household is willing to consider the matter");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Advisors);
                                option.SetGoodOutcome(outcome);
                                listTriggers.Clear();
                                listTriggers.Add(new Trigger(TriggerCheck.Known, 0, 1, EventCalc.Equals, false));
                                listTriggers.Add(new Trigger(TriggerCheck.Introduction, 0, 1, EventCalc.Equals, false));
                                option.SetTriggers(listTriggers);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Visitor
                            if (listVisitors.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Seek an Audience with a Visitor to House {0} ({1} present)", houseName, listVisitors.Count));
                                option.ReplyGood = string.Format("House {0} is willing to let you talk to whoever you wish", houseName);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Visitors);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Follower
                            if (listFollowers.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Talk to one of your Loyal Followers ({0} present)", listFollowers.Count));
                                option.ReplyGood = "A conversation may well be possible";
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Followers);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }

                            if (player.Known == false)
                            {
                                if (player.IntroPresented == false)
                                {
                                    //option -> Make yourself Known
                                    OptionInteractive optionKnown = new OptionInteractive("Make yourself Known");
                                    optionKnown.ReplyGood = "You reveal your identity and gain access to the Court";
                                    OutKnown outKnown = new OutKnown(eventObject.EventPID, -1);
                                    OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.None);
                                    optionKnown.SetGoodOutcome(outKnown);
                                    optionKnown.SetGoodOutcome(outcome);
                                    eventObject.SetOption(optionKnown);
                                }
                                //option -> Use an introduction
                                int numIntros = player.GetNumOfIntroductions(refID);
                                if (numIntros > 0)
                                {
                                    OptionInteractive optionIntro = new OptionInteractive($"Use an Introduction (you have {numIntros})");
                                    optionIntro.ReplyGood = $"You present your written Introduction to House \"{houseName}\"";
                                    OutIntroduction outIntro = new OutIntroduction(eventObject.EventPID, refID);
                                    OutEventChain outEvent = new OutEventChain(1000, EventAutoFilter.None);
                                    optionIntro.SetGoodOutcome(outIntro);
                                    optionIntro.SetGoodOutcome(outEvent);
                                    eventObject.SetOption(optionIntro);
                                }
                            }
                            //option -> recruit follower (only at Inns)
                            if (locType == 4)
                            {
                                //you haven't yet reached the max. number of Followers allowed?
                                int numFollowers = Game.world.GetNumFollowers();
                                int maxFollowers = Game.constant.GetValue(Global.MAX_FOLLOWERS);
                                if (numFollowers >= maxFollowers)
                                { Game.logTurn?.Write(" Trigger: Player has max. allowed number of Followers already -> Recruit option cancelled"); }
                                else
                                {
                                    //there is at least one remaining follower available to recruit
                                    if (Game.history.GetNumRemainingFollowers() > 0)
                                    {
                                        //option has a random chance of appearing
                                        if (rnd.Next(100) <= Game.constant.GetValue(Global.RECRUIT_FOLLOWERS))
                                        {
                                            //are there any followers available to recruit in the inn?
                                            //House tempHouse = Game.world.GetHouse(refID);
                                            if (house != null)
                                            {
                                                if (house is InnHouse)
                                                {
                                                    InnHouse inn = house as InnHouse;
                                                    int numAvailable = inn.GetNumFollowers();
                                                    if (numAvailable > 0)
                                                    {
                                                        OptionInteractive option = new OptionInteractive(string.Format("Recruit a Follower ({0} present)", numAvailable));
                                                        option.ReplyGood = "You are always on the lookout for loyal followers";
                                                        OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Recruit);
                                                        option.SetGoodOutcome(outcome);
                                                        eventObject.SetOption(option);
                                                    }
                                                    else { Game.logStart?.Write($"[Notification] No followers available for recruiting at \"{inn.Name}\""); }
                                                }
                                                else { Game.SetError(new Error(118, "House isn't a valid Inn")); }
                                            }
                                            else { Game.SetError(new Error(118, "Invalid House (null)")); }
                                        }
                                        else { Game.logStart?.Write($"[Notification] Random roll didn't succeed for recruiting followers option at LocID {locID}"); }
                                    }
                                    else { Game.logStart?.Write($"[Notification] No followers remaining in listActiveActors -> recruitment option cancelled"); }
                                }
                            }

                            //option -> seek information (if not known)
                            if (player.Known == false)
                            {
                                OptionInteractive option = new OptionInteractive("Ask around for Information");
                                option.ReplyGood = "You make some discreet enquiries";
                                OutRumour outRumour = new OutRumour(eventObject.EventPID);
                                option.SetGoodOutcome(outRumour);
                                eventObject.SetOption(option);
                            }
                            //option -> Observe (have a look around) -> Can't do so at Inns (nothing to see)
                            if (house != null)
                            {
                                if (house.ObserveFlag == false && house.Special == HouseSpecial.None)
                                {
                                    OptionInteractive option = new OptionInteractive("Observe");
                                    option.ReplyGood = "You walk around, taking note of everything";
                                    OutObserve outObserve = new OutObserve(eventObject.EventPID);
                                    option.SetGoodOutcome(outObserve);
                                    eventObject.SetOption(option);
                                }
                            }
                            else { Game.SetError(new Error(118, "Invalid house (null) for Observe option")); }
                            //option -> seek passage to another port (if applicable)
                            if (loc.isPort == true)
                            {
                                OptionInteractive option = new OptionInteractive("Seek Sea Passage to another Port");
                                option.ReplyGood = "You head to the harbour and search for a suitable ship";
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Docks);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> Lay low (only if not known)
                            if (house != null)
                            {
                                if (player.Known == true && house.SafeHouse > 0 && player.Conceal != ActorConceal.SafeHouse && (house?.GetInfoStatus(HouseInfo.SafeHouse) == true))
                                {
                                    OptionInteractive option = new OptionInteractive($"Lay Low ({house.SafeHouse} stars)");
                                    option.ReplyGood = $"You seek refuge at a Safe House ({house.SafeHouse} stars). You are immune from discovery while ever the place of refuge retains at least one star";
                                    OutKnown outKnown = new OutKnown(eventObject.EventPID, 1);
                                    OutSafeHouse outSafe = new OutSafeHouse(eventObject.EventPID, 1);
                                    option.SetGoodOutcome(outKnown);
                                    option.SetGoodOutcome(outSafe);
                                    eventObject.SetOption(option);
                                }
                            }
                            else { Game.SetError(new Error(118, "Invalid house (null) for Lay low option")); }
                            //option -> Leave
                            OptionInteractive option_L = new OptionInteractive("Leave");
                            if (player.Known == true) { option_L.ReplyGood = "You depart, head held high, shoulders back, meeting the eye of everyone you pass"; }
                            else { option_L.ReplyGood = "You quietly depart, moving quietly through the mottled shadows"; }
                            OutNone outcome_L = new OutNone(eventObject.EventPID);
                            option_L.SetGoodOutcome(outcome_L);
                            eventObject.SetOption(option_L);
                            break;
                        case EventAutoFilter.SafeHouse:
                            eventObject.Name = $"Safe House at {house.LocName} ({house.SafeHouse} stars)";
                            eventObject.Text = "What do you wish to do, Sire?";
                            //option -> Remain in the safe house (default)
                            OptionInteractive optionStay = new OptionInteractive("Continue Laying Low");
                            optionStay.ReplyGood = "You choose to remain in your refuge, safe from the prying eyes of your enemies";
                            OutNone outcomeStay = new OutNone(eventObject.EventPID);
                            optionStay.SetGoodOutcome(outcomeStay);
                            eventObject.SetOption(optionStay);
                            //option -> Leave the safe house
                            OptionInteractive optionLeave = new OptionInteractive("Leave the Safe House");
                            optionLeave.ReplyGood = "Carefully, checking the lane, you leave the security of your safe house and venture back out into the world";
                            OutSafeHouse outcomeLeave = new OutSafeHouse(eventObject.EventPID, -1);
                            optionLeave.SetGoodOutcome(outcomeLeave);
                            eventObject.SetOption(optionLeave);
                            break;
                        case EventAutoFilter.Court:
                            eventObject.Name = "Talk to members of the Court";
                            eventObject.Text = string.Format("Which members of House {0} do you wish to talk to?", houseName);
                            //options -> one for each member present
                            limit = listCourt.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive local = listCourt[i];
                                if (local.Office > ActorOffice.None)
                                { actorText = string.Format("{0} {1}", local.Office, local.Name); }
                                else { actorText = string.Format("{0} {1}", local.Type, local.Name); }
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventAutoFilter.Advisors:
                            eventObject.Name = "Talk to Advisors";
                            eventObject.Text = string.Format("Which Advisor do you wish to talk to?");
                            //options -> one for each member present
                            limit = listAdvisors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive local = listAdvisors[i];
                                actorText = string.Format("{0} {1}", local.Title, local.Name);
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                listTriggers.Clear();
                                listTriggers.Add(new Trigger(TriggerCheck.RelPlyr, local.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                option.SetTriggers(listTriggers);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventAutoFilter.Visitors:
                            eventObject.Name = "Talk to Visitors";
                            eventObject.Text = string.Format("You are at {0}. Which visitor do you wish to talk to?", locName);
                            //options -> one for each member present
                            limit = listVisitors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive visitor = listVisitors[i];
                                actorText = string.Format("{0} {1}", visitor.Title, visitor.Name);
                                //actorText = string.Format("{0} {1}", visitor.Type, visitor.Name);
                                optionText = string.Format("Seek an audience with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = visitor.ActID };
                                option.ReplyGood = string.Format("{0} has agreed to meet with you", actorText);
                                listTriggers.Clear();
                                listTriggers.Add(new Trigger(TriggerCheck.RelPlyr, visitor.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                option.SetTriggers(listTriggers);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventAutoFilter.Followers:
                            eventObject.Name = "Talk to Followers";
                            eventObject.Text = string.Format("You are at {0}. Which follower do you wish to talk to?", locName);
                            //options -> one for each member present
                            limit = listFollowers.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Follower follower = listFollowers[i];
                                actorText = string.Format("{0} {1}", follower.Type, follower.Name);
                                optionText = string.Format("Find time to talk with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = follower.ActID };
                                option.ReplyGood = string.Format("{0} is happy to sit down for a chat", actorText);
                                //OutNone outcome = new OutNone(eventObject.EventPID);
                                OutEventChain outcome = new OutEventChain(1000, EventAutoFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;

                        case EventAutoFilter.Recruit:
                            eventObject.Name = "Recruit a Follower";
                            eventObject.Text = string.Format("You are at {0}. Which follower do you wish to Recruit?", locName);
                            //options -> one for each recruit present
                            if (house != null)
                            {
                                InnHouse inn = house as InnHouse;
                                List<int> listRecruits = inn.GetFollowers();
                                limit = listRecruits.Count();
                                limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                                List<Active> listRemainingFollowers = Game.history.GetActiveActors();
                                if (listRemainingFollowers.Count > 0)
                                {
                                    if (limit > 0)
                                    {
                                        //default option
                                        OptionInteractive option = new OptionInteractive("You've changed your mind");
                                        option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                        OutNone outcome = new OutNone(eventObject.EventPID);
                                        option.SetGoodOutcome(outcome);
                                        eventObject.SetOption(option);
                                    }
                                    for (int i = 0; i < limit; i++)
                                    {
                                        //get follower
                                        Follower follower = null;
                                        foreach (Active actor in listRemainingFollowers)
                                        {
                                            if (actor.ActID == listRecruits[i])
                                            {
                                                follower = (Follower)actor;
                                                break;
                                            }
                                        }
                                        //follower found
                                        if (follower != null)
                                        {
                                            optionText = $"\"{follower.Name}\" offers to help";
                                            OptionInteractive option = new OptionInteractive(optionText) { ActorID = follower.ActID };
                                            option.ReplyGood = $"{follower.Name} agrees to help you win back your throne";
                                            OutFollower outcome = new OutFollower(eventObject.EventPID);
                                            option.SetGoodOutcome(outcome);
                                            eventObject.SetOption(option);
                                        }
                                    }
                                }
                                else { Game.SetError(new Error(73, "Invalid listRemainingFollowers (no records)")); }
                            }
                            break;

                        case EventAutoFilter.Docks:
                            //visit the docks and assess your options
                            eventObject.Name = "Seek Passage to another Port";
                            eventObject.Text = $"You are at {locName}'s Docks. Squawk! How do you wish to proceed?";
                            //Option -> Leave
                            OptionInteractive option_d0 = new OptionInteractive("You've changed your mind.");
                            option_d0.ReplyGood = string.Format("You leave the smelly docks of {0} behind", Game.world.GetHouseName(refID));
                            OutNone outcome_d0 = new OutNone(eventObject.EventPID);
                            option_d0.SetGoodOutcome(outcome_d0);
                            eventObject.SetOption(option_d0);
                            //Option -> Look for a ship
                            OptionInteractive option_d1 = new OptionInteractive("Look for a suitable ship (Possible success)");
                            option_d1.ReplyGood = "A ship might be found, but where to?";
                            OutEventChain outcome_d1 = new OutEventChain(eventObject.EventPID, EventAutoFilter.FindShip);
                            option_d1.SetGoodOutcome(outcome_d1);
                            eventObject.SetOption(option_d1);
                            //Option -> Bribe a Captain
                            OptionInteractive option_d2 = new OptionInteractive("Bribe a Captain to take you (Guaranteed success)");
                            option_d2.ReplyGood = "There is always a ship for those prepared to pay";
                            OutEventChain outcome_d2 = new OutEventChain(eventObject.EventPID, EventAutoFilter.BribeCaptain);
                            option_d2.SetGoodOutcome(outcome_d2);
                            eventObject.SetOption(option_d2);
                            List<Trigger> listDockTriggers = new List<Trigger>();
                            listDockTriggers.Add(new Trigger(TriggerCheck.ResourcePlyr, 0, 2, EventCalc.GreaterThanOrEqual));
                            option_d2.SetTriggers(listDockTriggers);
                            break;
                        case EventAutoFilter.FindShip:
                            //look for a suitable ship -> may find one, may not
                            eventObject.Name = "Look for a suitable ship";
                            eventObject.Text = $"You are at {locName}. Which Port do you wish to travel to?";
                            Dictionary<int, int> dictSeaDistances_0 = loc.GetSeaDistances();
                            //options -> one for each possible port
                            limit = dictSeaDistances_0.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind.");
                                option.ReplyGood = string.Format("You leave the raucous docks of {0} behind", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            foreach (var passage in dictSeaDistances_0)
                            {
                                Location locDestination = Game.network.GetLocation(passage.Key);
                                if (locDestination != null)
                                {
                                    //estimate voyage time, min 1 day
                                    voyageTime = passage.Value / speed;
                                    voyageTime = Math.Max(1, voyageTime);
                                    chance = (rnd.Next(1, 10) - 1) * 10;
                                    chance = Math.Max(10, chance);
                                    optionText = string.Format("Obtain passage to {0} {1}, a voyage of {2} day{3}. {4}% chance of success.", locDestination.LocName,
                                        Game.display.GetLocationCoords(locDestination.LocationID), voyageTime, voyageTime != 1 ? "s" : "", chance);
                                    OptionInteractive option = new OptionInteractive(optionText) { LocID = locDestination.LocationID, Test = chance };
                                    option.ReplyGood = "A suitable ship is available. You board immediately";
                                    option.ReplyBad = $"No ship is available to take you to {locDestination.LocName} today";
                                    OutPassage outSuccess = new OutPassage(eventObject.EventPID, locDestination.LocationID, voyageTime);
                                    string failText = string.Format("You depart the {0} docks after failing to find a suitable ship bound for {1}", locName, locDestination.LocName);
                                    OutNone outFail = new OutNone(eventObject.EventPID, failText, loc.LocationID);
                                    option.SetGoodOutcome(outSuccess);
                                    option.SetBadOutcome(outFail);
                                    eventObject.SetOption(option);
                                }
                                else { Game.SetError(new Error(73, "invalid locDestination (null) -> Passage option not created")); }
                            }
                            //desperado option (must be at least 2 port options available)
                            if (limit > 1)
                            {
                                optionText = "Find the first available ship, who cares where it's going? (Guaranteed but Risky)";
                                List<int> tempList = new List<int>(dictSeaDistances_0.Keys);
                                rndLocID = tempList[rnd.Next(tempList.Count)];
                                voyageDistance = dictSeaDistances_0[rndLocID];
                                voyageTime = voyageDistance / speed;
                                voyageTime = Math.Max(1, voyageTime);
                                Location locRandom = Game.network.GetLocation(rndLocID);
                                if (locRandom != null)
                                {
                                    OptionInteractive optionRandom = new OptionInteractive(optionText) { LocID = rndLocID };
                                    optionRandom.ReplyGood = "A nearby ship is casting off it's mooring lines, about to leave. You jump onboard";
                                    OutPassage outRandom = new OutPassage(eventObject.EventPID, rndLocID, voyageTime, false);
                                    optionRandom.SetGoodOutcome(outRandom);
                                    eventObject.SetOption(optionRandom);
                                }
                                else { Game.SetError(new Error(73, "Invalid locRandom (null)")); }
                            }
                            break;
                        case EventAutoFilter.BribeCaptain:
                            //Bribe a Captain -> Guaranteed
                            eventObject.Name = "Look for a suitable ship";
                            eventObject.Text = $"You are at {locName}. Which Port do you wish to travel to?";
                            Dictionary<int, int> dictSeaDistances_1 = loc.GetSeaDistances();
                            //options -> one for each possible port
                            limit = dictSeaDistances_1.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind.");
                                option.ReplyGood = string.Format("You leave the raucous docks of {0} behind", Game.world.GetHouseName(refID));
                                OutNone outcome = new OutNone(eventObject.EventPID);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            int fastSpeed = speed + 1; //bribed passages are faster
                            foreach (var passage in dictSeaDistances_1)
                            {
                                Location locDestination = Game.network.GetLocation(passage.Key);
                                if (locDestination != null)
                                {
                                    //estimate voyage time, min 1 day
                                    voyageTime = passage.Value / fastSpeed;
                                    voyageTime = Math.Max(1, voyageTime);
                                    optionText = string.Format("Buy a fast passage to {0} {1}, a voyage of {2} day{3} (costs a Resource)", locDestination.LocName,
                                        Game.display.GetLocationCoords(locDestination.LocationID), voyageTime, voyageTime != 1 ? "s" : "");
                                    OptionInteractive option = new OptionInteractive(optionText) { LocID = locDestination.LocationID };
                                    option.ReplyGood = "Money talks. The Captain pockets the gold and bids you come aboard";
                                    OutResource outResource = new OutResource(eventObject.EventPID, true, 1, EventCalc.Subtract);
                                    OutPassage outPassage = new OutPassage(eventObject.EventPID, locDestination.LocationID, voyageTime);
                                    //OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                    option.SetGoodOutcome(outResource);
                                    option.SetGoodOutcome(outPassage);
                                    eventObject.SetOption(option);
                                }
                                else { Game.SetError(new Error(73, "invalid locDestination (null) -> Passage option not created")); }
                            }
                            //desperado option (must be at least 2 port options available)
                            if (limit > 1)
                            {
                                optionText = "Find the first available ship, who cares where it's going? (Guaranteed but Risky)";
                                List<int> bribeList = new List<int>(dictSeaDistances_1.Keys);
                                rndLocID = bribeList[rnd.Next(bribeList.Count)];
                                voyageDistance = dictSeaDistances_1[rndLocID];
                                voyageTime = voyageDistance / speed;
                                voyageTime = Math.Max(1, voyageTime);
                                Location locRandom = Game.network.GetLocation(rndLocID);
                                if (locRandom != null)
                                {
                                    OptionInteractive optionRandom = new OptionInteractive(optionText) { LocID = rndLocID };
                                    optionRandom.ReplyGood = "A nearby ship is casting off it's mooring lines, about to leave. You jump onboard";
                                    OutPassage outRandom = new OutPassage(eventObject.EventPID, rndLocID, voyageTime, false);
                                    optionRandom.SetGoodOutcome(outRandom);
                                    eventObject.SetOption(optionRandom);
                                }
                                else { Game.SetError(new Error(73, "Invalid locRandom (null)")); }
                            }
                            break;
                        case EventAutoFilter.Interact:
                            //inteact with the selected individual
                            if (actorID > 1 && Game.world.CheckActorPresent(actorID, locID) == true)
                            {
                                Actor person = Game.world.GetAnyActor(actorID);
                                if (person != null)
                                {
                                    //admin for when Player meets character
                                    if (player.Conceal == ActorConceal.None) { Game.director.AddCourtVisit(refID); } //add to list of Court's visited (not if in disguise)
                                    person.SetRelationshipStatus(true); //reveal relationship levels and history
                                    //How to refer to them?
                                    if (person is Advisor) { actorText = $"{Game.display.GetAdvisorType((Advisor)person)} {person.Name}"; }
                                    else { actorText = string.Format("{0} {1}", person.Type, person.Name); }
                                    actorText = $"{person.Title} {person.Name}";
                                    eventObject.Name = "Interact";
                                    eventObject.Text = string.Format("How would you like to interact with {0}?", actorText);
                                    tempText = string.Format("You are granted an audience with {0} {1} \"{2}\", ActID {3}, at {4}", person.Title, person.Name, person.Handle, person.ActID, loc.LocName);
                                    //default -> flip back to court or advisor options
                                    OptionInteractive option_0 = new OptionInteractive("Excuse Yourself") { ActorID = actorID };
                                    option_0.ReplyGood = $"{actorText} stares at you with narrowed eyes";
                                    if (person is Advisor) { OutEventChain outcome_0 = new OutEventChain(1000, EventAutoFilter.Advisors); option_0.SetGoodOutcome(outcome_0); }
                                    else { OutEventChain outcome_0 = new OutEventChain(1000, EventAutoFilter.Court); option_0.SetGoodOutcome(outcome_0); }
                                    eventObject.SetOption(option_0);
                                    //improve relationship (befriend)
                                    OptionInteractive option_1 = new OptionInteractive("Befriend") { ActorID = actorID };
                                    option_1.ReplyGood = string.Format("{0} looks at you expectantly", actorText);
                                    List<Trigger> listTriggers_1 = new List<Trigger>();
                                    listTriggers_1.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), Game.constant.GetValue(Global.IMPROVE_THRESHOLD), EventCalc.GreaterThanOrEqual));
                                    option_1.SetTriggers(listTriggers_1);
                                    OutConflict outcome_1 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Befriend, SubType = ConflictSubType.Befriend };
                                    option_1.SetGoodOutcome(outcome_1);
                                    eventObject.SetOption(option_1);
                                    person.SetAllSkillsKnownStatus(true);
                                    house.SetInfoStatus(HouseInfo.FriendsEnemies);
                                    //blackmail
                                    OptionInteractive option_2 = new OptionInteractive("Blackmail") { ActorID = actorID };
                                    option_2.ReplyGood = string.Format("{0} frowns, their expression darkens", actorText);
                                    List<Trigger> listTriggers_2 = new List<Trigger>();
                                    listTriggers_2.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), Game.constant.GetValue(Global.BLACKMAIL_THRESHOLD), EventCalc.GreaterThanOrEqual));
                                    option_2.SetTriggers(listTriggers_2);
                                    OutConflict outcome_2 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Blackmail, SubType = ConflictSubType.Blackmail };
                                    option_2.SetGoodOutcome(outcome_2);
                                    eventObject.SetOption(option_2);
                                    person.SetAllSkillsKnownStatus(true);
                                    house.SetInfoStatus(HouseInfo.FriendsEnemies);
                                    //seduce
                                    OptionInteractive option_3 = new OptionInteractive("Seduce") { ActorID = actorID };
                                    option_3.ReplyGood = string.Format("{0} flutters their eyelids at you", actorText);
                                    List<Trigger> listTriggers_3 = new List<Trigger>();
                                    listTriggers_3.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), Game.constant.GetValue(Global.SEDUCE_THRESHOLD), EventCalc.GreaterThanOrEqual));
                                    listTriggers_3.Add(new Trigger(TriggerCheck.Sex, 0, (int)person.Sex, EventCalc.NotEqual)); //must be opposite sex
                                    option_3.SetTriggers(listTriggers_3);
                                    OutConflict outcome_3 = new OutConflict(eventObject.EventPID, actorID, ConflictType.Social) { Social_Type = ConflictSocial.Seduce, SubType = ConflictSubType.Seduce };
                                    option_3.SetGoodOutcome(outcome_3);
                                    eventObject.SetOption(option_3);
                                    person.SetAllSkillsKnownStatus(true);
                                    house.SetInfoStatus(HouseInfo.FriendsEnemies);
                                    //You want Something from them
                                    OptionInteractive option_5 = new OptionInteractive("You want something") { ActorID = actorID };
                                    option_5.ReplyGood = $"{actorText} sits back and cautiously agrees to discuss your needs";
                                    OutEventChain outcome_5 = new OutEventChain(eventObject.EventPID, EventAutoFilter.YouWant);
                                    option_5.SetGoodOutcome(outcome_5);
                                    List<Trigger> listTriggers_5 = new List<Trigger>();
                                    listTriggers_5.Add(new Trigger(TriggerCheck.RelPlyr, person.GetRelPlyr(), talkRel, EventCalc.GreaterThanOrEqual));
                                    option_5.SetTriggers(listTriggers_5);
                                    eventObject.SetOption(option_5);
                                    person.SetAllSkillsKnownStatus(true);
                                    //Desire (NPC wants something from you)
                                    OptionInteractive option_6 = new OptionInteractive("They want something") { ActorID = actorID };
                                    option_6.ReplyGood = string.Format("{0} leans forward enthusiastically to discuss {1} needs with you", actorText, person.Sex == ActorSex.Male ? "his" : "her");
                                    List<Trigger> listTriggers_6 = new List<Trigger>();
                                    listTriggers_6.Add(new Trigger(TriggerCheck.Desire, 0, 0, EventCalc.None));
                                    listTriggers_6.Add(new Trigger(TriggerCheck.Promise, 0, 0, EventCalc.None));
                                    option_6.SetTriggers(listTriggers_6);
                                    OutEventChain outcome_6 = new OutEventChain(eventObject.EventPID, EventAutoFilter.TheyWant);
                                    //OutNone outcome_6 = new OutNone(eventObject.EventPID);
                                    option_6.SetGoodOutcome(outcome_6);
                                    eventObject.SetOption(option_6);
                                    person.SetAllSkillsKnownStatus(true);
                                }
                                else { Game.SetError(new Error(73, "Invalid actorID from AutoCreateEvent (null from dict)")); }
                            }
                            break;
                        case EventAutoFilter.TheyWant:
                            //Character has a desire that you can meet in return for a relationship boost
                            Actor personWant = Game.world.GetAnyActor(actorID);
                            if (personWant != null)
                            {
                                /*if (personWant is Advisor)
                                {
                                    Advisor advisor = personWant as Advisor;
                                    if (advisor.advisorRoyal > AdvisorRoyal.None) { actorText = string.Format("{0} {1}", advisor.advisorRoyal, advisor.Name); }
                                    else { actorText = string.Format("{0} {1}", advisor.advisorNoble, advisor.Name); }
                                }
                                else if (personWant.Office > ActorOffice.None)
                                { actorText = string.Format("{0} {1}", personWant.Office, personWant.Name); }
                                else { actorText = string.Format("{0} {1}", personWant.Type, personWant.Name); }*/
                                if (personWant is Advisor) { actorText = $"{Game.display.GetAdvisorType((Advisor)personWant)} {personWant.Name}"; }
                                else { actorText = string.Format("{0} {1}", personWant.Type, personWant.Name); }
                                if (personWant is Passive)
                                {
                                    //You've spoken to the character so you know their desire (if you didn't previously)
                                    Passive tempPassive = personWant as Passive;
                                    tempPassive.SetDesireKnown(true);
                                    //set up promises
                                    int strength; // strength of promise, 1 to 5
                                    int baseValue = Game.constant.GetValue(Global.PROMISES_BASE);
                                    //if too many promises have been handed out, effect is halved
                                    int numPromises = Game.variable.GetValue(GameVar.Promises_Num);
                                    int numHalved = Game.constant.GetValue(Global.PROMISES_HALVED);
                                    if (numPromises >= numHalved)
                                    { baseValue /= 2; Game.logTurn?.Write($"[Promises] {numPromises} handed out is >= {numHalved}, relationship baseValue halved to {baseValue}"); }
                                    Passive passive = personWant as Passive;
                                    eventObject.Name = "They Want Something";
                                    eventObject.Text = $"{passive.Title} {passive.Name}, ActID {passive.ActID} has a desire for {passive.DesireText}.";
                                    tempText = string.Format("You sit down and discuss what you can do for {0} {1} \"{2}\", ActID {3}, at {4}", passive.Title, passive.Name,
                                        passive.Handle, passive.ActID, loc.LocName);
                                    //default
                                    OptionInteractive option_w0 = new OptionInteractive("Sorry, you can't help") { ActorID = actorID };
                                    option_w0.ReplyGood = $"{actorText} shrugs their shoulders";
                                    OutEventChain outcome_w0 = new OutEventChain(eventObject.EventPID, EventAutoFilter.Interact);
                                    option_w0.SetGoodOutcome(outcome_w0);
                                    eventObject.SetOption(option_w0);
                                    //Give it some thought
                                    OptionInteractive option_w1 = new OptionInteractive("You promise to give it some thought") { ActorID = actorID };
                                    option_w1.ReplyGood = $"{actorText} nods in agreement";
                                    strength = 1;
                                    OutPromise outcome_w1_0 = new OutPromise(eventObject.EventPID, passive.Desire, strength);
                                    OutRelPlyr outcome_w1_1 = new OutRelPlyr(eventObject.EventPID, baseValue * strength, EventCalc.Add, $"Ursurper Promises to think about {passive.DesireText}", "Promise");
                                    OutFavour outcome_w1_2 = new OutFavour(eventObject.EventPID, strength);
                                    option_w1.SetGoodOutcome(outcome_w1_0);
                                    option_w1.SetGoodOutcome(outcome_w1_1);
                                    option_w1.SetGoodOutcome(outcome_w1_2);
                                    eventObject.SetOption(option_w1);
                                    //Promise to Take Care of it
                                    OptionInteractive option_w2 = new OptionInteractive("You promise to take care of it") { ActorID = actorID };
                                    option_w2.ReplyGood = $"{actorText} nods in agreement";
                                    strength = 3;
                                    OutPromise outcome_w2_0 = new OutPromise(eventObject.EventPID, passive.Desire, strength);
                                    OutRelPlyr outcome_w2_1 = new OutRelPlyr(eventObject.EventPID, baseValue * strength, EventCalc.Add, $"Ursurper Promises to take care off {passive.DesireText}", "Promise");
                                    OutFavour outcome_w2_2 = new OutFavour(eventObject.EventPID, strength);
                                    option_w2.SetGoodOutcome(outcome_w2_0);
                                    option_w2.SetGoodOutcome(outcome_w2_1);
                                    option_w2.SetGoodOutcome(outcome_w2_2);
                                    eventObject.SetOption(option_w2);
                                    //Swear on your Father's Grave
                                    OptionInteractive option_w3 = new OptionInteractive("You swear on your father's grave that you'll fix it") { ActorID = actorID };
                                    option_w3.ReplyGood = $"{actorText} nods in agreement";
                                    strength = 5;
                                    OutPromise outcome_w3_0 = new OutPromise(eventObject.EventPID, passive.Desire, strength);
                                    OutRelPlyr outcome_w3_1 = new OutRelPlyr(eventObject.EventPID, baseValue * strength, EventCalc.Add,
                                        $"Ursurper Swears on their Father's grave to deal with {passive.DesireText}", "Promise");
                                    OutFavour outcome_w3_2 = new OutFavour(eventObject.EventPID, strength);
                                    option_w3.SetGoodOutcome(outcome_w3_0);
                                    option_w3.SetGoodOutcome(outcome_w3_1);
                                    option_w3.SetGoodOutcome(outcome_w3_2);
                                    eventObject.SetOption(option_w3);
                                }
                            }
                            break;
                        case EventAutoFilter.YouWant:
                            //You want something from the NPC character
                            Actor personNeed = Game.world.GetAnyActor(actorID);
                            if (personNeed != null)
                            {
                                /*if (personNeed is Advisor)
                                {
                                    Advisor advisor = personNeed as Advisor;
                                    if (advisor.advisorRoyal > AdvisorRoyal.None) { actorText = string.Format("{0} {1}", advisor.advisorRoyal, advisor.Name); }
                                    else { actorText = string.Format("{0} {1}", advisor.advisorNoble, advisor.Name); }
                                }
                                else if (personNeed.Office > ActorOffice.None)
                                {
                                    actorText = string.Format("{0} {1}", personNeed.Office, personNeed.Name);
                                }
                                else { actorText = string.Format("{0} {1}", personNeed.Type, personNeed.Name); }*/
                                if (personNeed is Advisor) { actorText = $"{Game.display.GetAdvisorType((Advisor)personNeed)} {personNeed.Name}"; }
                                else { actorText = string.Format("{0} {1}", personNeed.Type, personNeed.Name); }
                                if (personNeed is Passive)
                                {
                                    eventObject.Name = "You Want Something";
                                    eventObject.Text = string.Format("How would you like to interact with {0}?", actorText);
                                    tempText = string.Format("You sit down and discuss your needs with {0} {1} \"{2}\", ActID {3}, at {4}", personNeed.Title, personNeed.Name,
                                        personNeed.Handle, personNeed.ActID, loc.LocName);
                                    //default -> flip back to advisor options
                                    OptionInteractive option_n0 = new OptionInteractive("Excuse Yourself") { ActorID = actorID };
                                    option_n0.ReplyGood = $"{actorText} stares at you with narrowed eyes";
                                    OutEventChain outcome_n0 = new OutEventChain(1000, EventAutoFilter.Advisors);
                                    option_n0.SetGoodOutcome(outcome_n0);
                                    eventObject.SetOption(option_n0);

                                    //Cash in a Favour
                                    OptionInteractive option_n1 = new OptionInteractive("Cash in a Favour") { ActorID = actorID };
                                    option_n1.ReplyGood = $"{actorText} squirms in their seat. It doesn't appear comfortable";
                                    OutNone outcome_n1 = new OutNone(eventObject.EventPID);
                                    option_n1.SetGoodOutcome(outcome_n1);
                                    eventObject.SetOption(option_n1);

                                    //Hand over a disguise (Player can't already have a disguise in his inventory)
                                    if (personNeed is Advisor)
                                    {
                                        Advisor advisor = personNeed as Advisor;
                                        if (advisor.CheckDisguises() == true)
                                        {
                                            OptionInteractive option_n2 = new OptionInteractive("Ask for a Disguise") { ActorID = actorID };
                                            option_n2.ReplyGood = $"{actorText} nods solmenly and reaches for a nearby sack";
                                            OutDisguise outcome_n2 = new OutDisguise(eventObject.EventPID);
                                            option_n2.SetGoodOutcome(outcome_n2);
                                            List<Trigger> listTriggers_n2 = new List<Trigger>();
                                            listTriggers_n2.Add(new Trigger(TriggerCheck.Disguise, 0, 0, EventCalc.None));
                                            option_n2.SetTriggers(listTriggers_n2);
                                            eventObject.SetOption(option_n2);
                                        }
                                    }

                                    //Become an informant
                                    OptionInteractive option_n3 = new OptionInteractive("Ask to keep me Informed") { ActorID = actorID };
                                    option_n3.ReplyGood = $"{actorText} agrees to keep their ear to the ground and lete you know if anything interesting occurs";
                                    OutNone outcome_n3 = new OutNone(eventObject.EventPID);
                                    option_n3.SetGoodOutcome(outcome_n3);
                                    eventObject.SetOption(option_n3);
                                    //swear allegiance
                                    OptionInteractive option_4 = new OptionInteractive("Ask for their Allegiance") { ActorID = actorID };
                                    option_4.ReplyGood = string.Format("{0} kneels at your feet", actorText);
                                    List<Trigger> listTriggers_4 = new List<Trigger>();
                                    listTriggers_4.Add(new Trigger(TriggerCheck.RelPlyr, personNeed.GetRelPlyr(), Game.constant.GetValue(Global.ALLEGIANCE_THRESHOLD), EventCalc.GreaterThanOrEqual));
                                    listTriggers_4.Add(new Trigger(TriggerCheck.ActorType, (int)personNeed.Type, (int)ActorType.Lord, EventCalc.Equals)); //must be a Lord
                                    option_4.SetTriggers(listTriggers_4);
                                    OutNone outcome_4 = new OutNone(eventObject.EventPID);
                                    option_4.SetGoodOutcome(outcome_4);
                                    eventObject.SetOption(option_4);
                                }
                            }
                            break;
                        default:
                            Game.SetError(new Error(118, string.Format("Invalid EventFilter (\"{0}\")", filter)));
                            break;
                    }
                    //Create & Add Event Package
                    EventPackage package = new EventPackage() { Person = player, EventObject = eventObject, Done = false };
                    Game.director.AddPlyrCurrentEvent(package);
                    //if more than the current event present the original one (autocreated) needs to be deleted
                    if (Game.director.GetPlyrCurrentEventsCount() > 1) { Game.director.RemoveAtPlyrCurrentEvents(0); }
                    //add to Player dictionary (ResolveOutcome looks for it there) -> check not an instance present already
                    Game.director.RemovePlayerEvent(1000);
                    Game.director.AddPlayerEvent(eventObject);
                    /*if (dictPlayerEvents.ContainsKey(1000)) { dictPlayerEvents.Remove(1000); }
                    dictPlayerEvents.Add(1000, eventObject);*/
                    //message
                    if (tempText.Length > 0)
                    {
                        Game.world.SetMessage(new Message(tempText, MessageType.Event));
                        Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, CurrentActorEvent.Event));
                    }
                }
                else { Game.SetError(new Error(118, "Invalid List of Actors (Zero present at Location")); }
            }
            else { Game.SetError(new Error(118, "Invalid Player (returns Null)")); }
        }

        //new methods above here
    }
}
