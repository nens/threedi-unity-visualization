/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class AirplaneDemo : MonoBehaviour
    {
        public RealWorldElevation ElevationMode = RealWorldElevation.Altitude;

        //Position where the airplane will appear when the terrain is generated 
        public DVector2 StartPositionLatLon;
        public float StartElevation = 1500; 

        public float Movingspeed;
        public float RotationSpeed;
        // Real World Waypoints In Lat-Lon-Elevation
        public WayPoints waypoints;

        [HideInInspector]
        public Vector3 TargetPoint = new Vector3(0,0,0);
        private Quaternion rotation;
        private int wayIndex = 0;

        [HideInInspector]
        public bool EnableFlying;

        public GISTerrainContainer container;
        public void OnTerrainGeneratingCompleted()
        {
            this.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(RuntimeTerrainGenerator.Get.GeneratedContainer, StartPositionLatLon, StartElevation, SetElevationMode.RelativeToSeaLevel);
            if(container== null)
            container = FindObjectOfType<GISTerrainContainer>(true);

            if (waypoints.UnityWorldSpacePoints.Count>0)
            TargetPoint = waypoints.UnityWorldSpacePoints[0];
        }
        private void FixedUpdate()
        {
            if(TargetPoint != Vector3.zero)
            {
                AirPlaneGuidance(TargetPoint);

                var dis = Vector3.Distance(transform.position, TargetPoint);

                if (dis < 10f)
                {
                    wayIndex++;
                    if (wayIndex > waypoints.UnityWorldSpacePoints.Count - 1)
                        wayIndex = 0;
                    TargetPoint = waypoints.UnityWorldSpacePoints[wayIndex];

                    return;
                }

            }


        }
        private void AirPlaneGuidance(Vector3 target)
        {
            Vector3 relPos = target - transform.position;
            relPos = target - transform.position;
            rotation = Quaternion.LookRotation(relPos, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotationSpeed * Time.fixedDeltaTime);
            transform.Translate(transform.forward * Movingspeed * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Get the real world Elevation of the airplane 
        /// </summary>
        /// <returns></returns>
        public DVector3 GetAirPlaneLatLonElevation()
        {
            return GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(this.transform.position, container,true, ElevationMode);
        }
        /// <summary>
        /// Get the the Current WayPoint LatLon
        /// </summary>
        /// <returns></returns>
        public DVector2 GetWayPointLatLon()
        {
            return GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(TargetPoint, container).ToDVector2();
 
        }
    }
}
