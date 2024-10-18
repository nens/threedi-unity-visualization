/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace GISTech.GISTerrainLoader
{
    // Simple Script to learn how get distance in m or km between two gameobjects
    // in this case (Airplane - WayPoint)
    public class DistanceCalculation : MonoBehaviour
    {
        public AirplaneDemo Airplane;
        private SimpleTerrainGenerator MyTerrainGenerator;
        public Text UIDistance;

        void Start()
        {
            MyTerrainGenerator = this.GetComponent<SimpleTerrainGenerator>();
        }

        void Update()
        {
            if (Airplane == null) return;
            if (MyTerrainGenerator == null) return;

            if (MyTerrainGenerator.TerrainGenerated)
            {
                var AirplaneLatLonPos = Airplane.GetAirPlaneLatLonElevation().ToDVector2();

                var CurrentWayPointLatLon = Airplane.GetWayPointLatLon();

                var Distance =Math.Round(GISTerrainLoaderGeoConversion.CalDistance(AirplaneLatLonPos, CurrentWayPointLatLon),2);

                if (Distance>1)
                UIDistance.text = "Distance to the next waypoint : " + Distance + " [Km]";
                else
                    UIDistance.text = "Distance to the next waypoint : " + Distance*1000 + " [m]";
            }
         }
    }
}
