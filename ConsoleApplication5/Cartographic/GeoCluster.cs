using System;


namespace Next_Game.Cartographic
{

    public enum Cluster {Sea, Mountain, Forest} //NOTE: don't change order as ties in with data from map.InitialiseTerrain

    /// <summary>
    /// Clusters of geo objects such as sea, mountain or forest zones (orthagonol clusters)
    /// </summary>
    class GeoCluster
    {
        public string Name { get; set; } = "Unknown";
        public string Description { get; set; } = "No description provided";
        public int Size { get; }
        public int GeoID { get; }
        public Cluster ClusterType { get; }

        public GeoCluster()
        { }

        //default constructor
        public GeoCluster(int geoID, int type, int size)
        {
            this.GeoID = geoID;
            ClusterType = (Cluster)type;
            this.Size = size;
        }
    }
}
