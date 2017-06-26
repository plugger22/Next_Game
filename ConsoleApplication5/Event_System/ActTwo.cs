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
    /// Contains all Act Two events
    /// </summary>
    public class ActTwo
    {
        static Random rnd;

        public ActTwo(int seed)
        {
            rnd = new Random(seed);
        }


        /// <summary>
        /// Auto Location Event for Act Two
        /// </summary>
        internal void CreateAutoEventTwo(EventFilter filter, int actorID = 0)
        {
            //get player
            Player player = (Player)Game.world.GetPlayer();
            if (player != null)
            {
                Game.logTurn?.Write("- CreateAutoEventTwo (ActTwo.cs)");
                List<Actor> listActors = new List<Actor>();
                List<Passive> listCourt = new List<Passive>();
                List<Passive> listAdvisors = new List<Passive>();
                List<Passive> listVisitors = new List<Passive>();
                List<Follower> listFollowers = new List<Follower>();
                List<Actor> listCaptured = new List<Actor>();
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
                else { Game.SetError(new Error(328, "Invalid Loc (null)")); }
                int houseID = Game.map.GetMapInfo(MapLayer.HouseID, loc.GetPosX(), loc.GetPosY());
                int refID = Game.map.GetMapInfo(MapLayer.RefID, loc.GetPosX(), loc.GetPosY());
                House house = Game.world.GetHouse(refID);
                if (house == null && refID != 9999) { Game.SetError(new Error(328, "Invalid house (null)")); }
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
                            if (filter == EventFilter.Court) { filter = EventFilter.Visitors; Game.SetError(new Error(118, "Invalid filter (Locals when at an Inn)")); }
                        }
                        break;
                    default:
                        Game.SetError(new Error(328, $"Invalid loc.Type \"{loc.Type}\""));
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
                            { listActors.Add(tempActor); Game.logTurn?.Write(string.Format(" [AutoEventTwo -> ActorList] \"{0}\", ID {1} added to list of Actors", tempActor.Name, tempActor.ActID)); }
                        }
                        else { Game.SetError(new Error(328, string.Format("Invalid tempActor ID {0} (Null)", actorIDList[i]))); }
                    }
                    //filter actors accordingly
                    for (int i = 0; i < listActors.Count; i++)
                    {
                        Actor actor = listActors[i];
                        if (actor.Status == ActorStatus.Captured)
                        { listCaptured.Add(actor); }
                        else if (actor is Passive)
                        {
                            Passive tempPassive = actor as Passive;
                            testRefID = refID;
                            if (locType == 1) { testRefID = Game.lore.RoyalRefIDNew; }
                            if (tempPassive.RefID == testRefID && !(actor is Advisor))
                            {
                                if (tempPassive.Type == ActorType.Lord || tempPassive.Age >= 15)
                                {
                                    listCourt.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEventTwo -> LocalList] \"{0}\", ID {1} added to list of Locals",
                                      tempPassive.Name, tempPassive.ActID));
                                }
                            }
                            else if (actor is Advisor)
                            {
                                listAdvisors.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEventTwo -> AdvisorList] \"{0}\", ID {1} added to list of Advisors",
                                  tempPassive.Name, tempPassive.ActID));
                            }
                            else
                            {
                                if (tempPassive.Age >= 15)
                                {
                                    listVisitors.Add(tempPassive); Game.logTurn?.Write(string.Format(" [AutoEventTwo -> VisitorList] \"{0}\", ID {1} added to list of Visitors",
                                      tempPassive.Name, tempPassive.ActID));
                                }
                            }
                        }
                        else if (actor is Follower)
                        {
                            Follower tempFollower = actor as Follower;
                            listFollowers.Add(tempFollower);
                            Game.logTurn?.Write(string.Format(" [AutoEventTwo -> FollowerList] \"{0}\", ID {1} added to list of Followers", tempFollower.Name, tempFollower.ActID));
                        }
                    }
                    //new event (auto location events always have eventPID of '1000' -> old version in Player dict is deleted before new one added)
                    EventPlayer eventObject = new EventPlayer(1000, "What to do?", EventFrequency.Low) { Category = EventCategory.AutoCreate, Status = EventStatus.Active, Type = ArcType.Location };
                    tempText = "";
                    //
                    // Resolution ---
                    //
                    switch (filter)
                    {
                        case EventFilter.None:
                            eventObject.Text = string.Format("You are at {0}. How will you fill your day, sire?", locName);
                            //option -> default Leave
                            OptionInteractive option_L = new OptionInteractive("Leave");
                            if (player.Known == true) { option_L.ReplyGood = "You realise you have matters to attend to elsewhere"; }
                            else { option_L.ReplyGood = "You depart, a retinue of minons scurrying in your wake"; }
                            OutNone outcome_L = new OutNone(eventObject.EventPID);
                            option_L.SetGoodOutcome(outcome_L);
                            eventObject.SetOption(option_L);
                            //option -> audience with local House member
                            if (listCourt.Count() > 0)
                            {
                                OptionInteractive option = null;
                                if (locType != 1)
                                {
                                    option = new OptionInteractive(string.Format("Seek an Audience with a member of House {0} ({1} present)", houseName, listCourt.Count));
                                    option.ReplyGood = string.Format("House {0} awaits your presence, sire", houseName);
                                }
                                else
                                {
                                    //capital
                                    option = new OptionInteractive(string.Format("Seek an Audience with the Royal Household ({0} present)", listCourt.Count));
                                    option.ReplyGood = string.Format("The Royal Usher has prepared for your arrival, sire");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Court);
                                option.SetGoodOutcome(outcome);
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
                                    option = new OptionInteractive(string.Format("Speak with a Royal Advisor ({0} present)", listAdvisors.Count));
                                    option.ReplyGood = string.Format("The Royal Clerk has advised that the Council awaits you, sire");
                                }
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Advisors);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Visitor
                            if (listVisitors.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Grant an Audience with a Visitor to House {0} ({1} present)", houseName, listVisitors.Count));
                                option.ReplyGood = string.Format("House {0} is willing to let you talk to whoever you wish", houseName);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Visitors);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            //option -> audience with Captured actor in Dungeons
                            if (listCaptured.Count() > 0)
                            {
                                OptionInteractive option = new OptionInteractive(string.Format("Visit a Prisoner ({0} present)", listCaptured.Count));
                                option.ReplyGood = "It isn't savoury down there in the Dungeons, sire, but if you insist...";
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Dungeon);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Court:
                            eventObject.Name = "Talk to members of the Court";
                            eventObject.Text = string.Format("Which members of House {0} do you wish to talk to?", houseName);
                            //options -> one for each member present
                            limit = listCourt.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} with a swirl of your cape", Game.world.GetHouseName(refID));
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.None);
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
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Advisors:
                            eventObject.Name = "Talk to the Royal Council";
                            eventObject.Text = string.Format("Which Advisor do you wish to consult?");
                            //options -> one for each member present
                            limit = listAdvisors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} with a swirl of your cape", Game.world.GetHouseName(refID));
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.None);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Passive local = listAdvisors[i];
                                actorText = string.Format("{0} {1}", local.Title, local.Name);
                                optionText = string.Format("Consult with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} bows and looks at your expectantly", actorText);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.SpeakCouncil);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Dungeon:
                            eventObject.Name = "Descend to the dark, putrid, bowels of the castle";
                            eventObject.Text = string.Format("Which Prisoner do you wish to speak with?");
                            //options -> one for each member present
                            limit = listCaptured.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} with a swirl of your cape", Game.world.GetHouseName(refID));
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.None);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            for (int i = 0; i < limit; i++)
                            {
                                Actor local = listCaptured[i];
                                actorText = string.Format("{0} {1}", local.Title, local.Name);
                                optionText = string.Format("Speak with {0}", actorText);
                                OptionInteractive option = new OptionInteractive(optionText) { ActorID = local.ActID };
                                option.ReplyGood = string.Format("{0} sprawls before you, filthy and rank", actorText);
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.SpeakPrisoner);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.Visitors:
                            eventObject.Name = "Grant an Audience to a Visitor";
                            eventObject.Text = string.Format("You are at {0}. Which visitor do you wish to talk to?", locName);
                            //options -> one for each member present
                            limit = listVisitors.Count();
                            limit = Math.Min(12, limit); //max 12 options possible (F1 - F12)
                            if (limit > 0)
                            {
                                //default option
                                OptionInteractive option = new OptionInteractive("You've changed your mind");
                                option.ReplyGood = string.Format("You depart {0} without further ado", Game.world.GetHouseName(refID));
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.None);
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
                                OutEventChain outcome = new OutEventChain(1000, EventFilter.Interact);
                                option.SetGoodOutcome(outcome);
                                eventObject.SetOption(option);
                            }
                            break;
                        case EventFilter.SpeakCouncil:
                            //Royal Council of Advisors -> inteact with the selected individual
                            if (actorID > 1 && Game.world.CheckActorPresent(actorID, locID) == true)
                            {
                                Actor person = Game.world.GetAnyActor(actorID);
                                if (person != null)
                                {
                                    //How to refer to them?
                                    if (person is Advisor) { actorText = $"{Game.display.GetAdvisorType((Advisor)person)} {person.Name}"; }
                                    else { actorText = string.Format("{0} {1}", person.Type, person.Name); }
                                    actorText = $"{person.Title} {person.Name}";
                                    eventObject.Name = "Interact";
                                    eventObject.Text = string.Format("How would you like to interact with {0}?", actorText);
                                    //default -> flip back to court or advisor options
                                    OptionInteractive option_0 = new OptionInteractive("Excuse Yourself") { ActorID = actorID };
                                    option_0.ReplyGood = $"{actorText} stares at you with narrowed eyes";
                                    if (person is Advisor) { OutEventChain outcome_0 = new OutEventChain(1000, EventFilter.Advisors); option_0.SetGoodOutcome(outcome_0); }
                                    else { OutEventChain outcome_0 = new OutEventChain(1000, EventFilter.None); option_0.SetGoodOutcome(outcome_0); }
                                    eventObject.SetOption(option_0);
                                    if (person is Advisor)
                                    {
                                        Advisor advisor = person as Advisor;
                                        switch (advisor.advisorRoyal)
                                        {
                                            case AdvisorRoyal.High_Septon:
                                                break;
                                            case AdvisorRoyal.Master_of_Coin:
                                                break;
                                            case AdvisorRoyal.Master_of_Laws:
                                                break;
                                            case AdvisorRoyal.Master_of_Whisperers:
                                                if (Game.variable.GetValue(GameVar.Inquisitor_AI) > 0)
                                                {
                                                    //assume direct control of Inquisitors
                                                }
                                                else
                                                {
                                                    //hand control of Inquistors back to Whisperer
                                                }
                                                break;
                                            case AdvisorRoyal.Hand_of_the_King:
                                                break;
                                            case AdvisorRoyal.Commander_of_City_Watch:
                                                break;
                                        }
                                    }

                                }
                                else { Game.SetError(new Error(328, $"Invalid actorID \"{actorID}\", from AutoCreateEvent (null from dict)")); }
                            }
                            break;
                        case EventFilter.SpeakPrisoner:
                            
                            break;
                        default:
                            Game.SetError(new Error(328, string.Format("Invalid EventFilter (\"{0}\")", filter)));
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
                    //message
                    if (tempText.Length > 0)
                    {
                        Game.world.SetMessage(new Message(tempText, MessageType.Event));
                        Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, CurrentActorEvent.Event));
                    }
                }
                else { Game.SetError(new Error(328, "Invalid List of Actors (Zero present at Location")); }
            }
            else { Game.SetError(new Error(328, "Invalid Player (null)")); }
        }
        //new methods above here
    }
}
