﻿using System;
using System.Collections.Generic;

namespace Next_Game.Cartographic
{

    public enum Cluster {Sea, Mountain, Forest} //NOTE: don't change order as ties in with data from map.InitialiseTerrain
    public enum GeoType {Large_Mtn, Medium_Mtn, Small_Mtn, Large_Forest, Medium_Forest, Small_Forest, Large_Sea, Medium_Sea, Small_Sea, Count}

    /// <summary>
    /// Clusters of geo objects such as sea, mountain or forest zones (orthagonol clusters)
    /// </summary>
    class GeoCluster
    {
        public string Name { get; set; } = "Unknown";
        public string Description { get; set; } = "No description provided";
        public int Size { get; }
        public int GeoID { get; }
        public Cluster Terrain { get; }
        public GeoType Type { get; }
        private List<int> listOfSecrets;

        public GeoCluster()
        { listOfSecrets = new List<int>(); }

        //default constructor
        public GeoCluster(int geoID, int type, int size)
        {
            listOfSecrets = new List<int>();
            this.GeoID = geoID;
            Terrain = (Cluster)type;
            this.Size = size;
            //determine type
            int small = Game.constant.GetValue(Global.TERRAIN_SMALL);
            int seaLarge = Game.constant.GetValue(Global.SEA_LARGE);
            int mountainLarge = Game.constant.GetValue(Global.MOUNTAIN_LARGE);
            int forestLarge = Game.constant.GetValue(Global.FOREST_LARGE);
            switch (Terrain)
            {
                case Cluster.Sea:
                    if (size <= small) { Type = GeoType.Small_Sea; }
                    else if (size >= seaLarge) { Type = GeoType.Large_Sea; }
                    else { Type = GeoType.Medium_Sea; }
                    break;
                case Cluster.Mountain:
                    if (size <= small) { Type = GeoType.Small_Mtn; }
                    else if (size >= mountainLarge) { Type = GeoType.Large_Mtn; }
                    else { Type = GeoType.Medium_Mtn; }
                    break;
                case Cluster.Forest:
                    if (size <= small) { Type = GeoType.Small_Forest; }
                    else if (size >= forestLarge) { Type = GeoType.Large_Forest; }
                    else { Type = GeoType.Medium_Forest; }
                    break;
            }
        }
    }
}
