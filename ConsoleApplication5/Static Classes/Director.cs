using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    public enum StoryAI {None, Benevolent, Balanced, Evil, Tricky}
    
    /// <summary>
    /// Director that manages the game world according to a Story AI personality
    /// </summary>
    public class Director
    {
        Story story;
        List<int> listOfActiveGeoClusters; //clusters that have a road through them
        List<int> listGenEventsForest; //generic events for forests
        List<int> listGenEventsMountain;
        List<int> listGenEventsSea;
        List<int> listGenEventsNormal;
        List<int> listGenEventsRoyal;
        List<int> listGenEventsConnector;
        List<int> listGenEventsCapital;
        List<int> listGenEventsMajor;
        List<int> listGenEventsMinor;
        List<int> listGenEventsInn;
        Dictionary<int, Event> dictEvents;

        public Director()
        {
            listOfActiveGeoClusters = new List<int>();
            listGenEventsForest = new List<int>();
            listGenEventsMountain = new List<int>();
            listGenEventsSea = new List<int>();
            listGenEventsNormal = new List<int>(); //note that Normal road generic events also apply to all types of Roads (Royal generics -> Royal + Normal, for example)
            listGenEventsRoyal = new List<int>();
            listGenEventsConnector = new List<int>();
            listGenEventsCapital = new List<int>();
            listGenEventsMajor = new List<int>();
            listGenEventsMinor = new List<int>();
            listGenEventsInn = new List<int>();

            dictEvents = new Dictionary<int, Event>();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void InitialiseDirector()
        {
            listOfActiveGeoClusters.AddRange(Game.map.GetActiveGeoClusters());
            Console.WriteLine(Environment.NewLine + "--- Import Events");
            dictEvents = Game.file.GetEvents("Events.txt");
            GetGenericEvents();
        }

        /// <summary>
        /// loop all events and place generic eventID's in their approrpriate lists
        /// </summary>
        private void GetGenericEvents()
        {
            int eventID;
            foreach(var eventObject in dictEvents)
            {
                if (eventObject.Value.Category == ArcCat.Generic)
                {
                    eventID = eventObject.Value.EventID;
                    switch(eventObject.Value.AppliesTo)
                    {
                        case ArcType.GeoCluster:
                            switch(eventObject.Value.GeoType)
                            {
                                case ArcGeo.Forest:
                                    listGenEventsForest.Add(eventID);
                                    break;
                                case ArcGeo.Mountain:
                                    listGenEventsMountain.Add(eventID);
                                    break;
                                case ArcGeo.Sea:
                                    listGenEventsSea.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcGeo"));
                                    break;
                            }
                            break;
                        case ArcType.Location:
                            switch(eventObject.Value.LocType)
                            {
                                case ArcLoc.Capital:
                                    listGenEventsCapital.Add(eventID);
                                    break;
                                case ArcLoc.Major:
                                    listGenEventsMajor.Add(eventID);
                                    break;
                                case ArcLoc.Minor:
                                    listGenEventsMinor.Add(eventID);
                                    break;
                                case ArcLoc.Inn:
                                    listGenEventsInn.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcLoc"));
                                    break;
                            }
                            break;
                        case ArcType.Road:
                            switch(eventObject.Value.RoadType)
                            {
                                case ArcRoad.Normal:
                                    listGenEventsNormal.Add(eventID);
                                    break;
                                case ArcRoad.Royal:
                                    listGenEventsRoyal.Add(eventID);
                                    break;
                                case ArcRoad.Connector:
                                    listGenEventsConnector.Add(eventID);
                                    break;
                                default:
                                    Game.SetError(new Error(50, "Invalid Type, ArcRoad"));
                                    break;
                            }
                            break;
                    }
                }
            }
        }
    }
}
