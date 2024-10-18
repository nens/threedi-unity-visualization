/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderVectorLine : MonoBehaviour
    {
        public VectorDataLineSource LineDataSource;

        public GISTerrainLoaderLineGeoData LineGeoData;
 
        public void UpdateGeoDataFrom(GISTerrainContainer Container, CoordinatesSource coordinatesSource = CoordinatesSource.FromGeoDataScript, bool StoreElevationValue = false, RealWorldElevation ElevationMode = RealWorldElevation.Elevation)
        {
            switch (coordinatesSource)
            {

                case CoordinatesSource.FromTerrain:

                    switch (LineDataSource)
                    {
                        case VectorDataLineSource.LineRenderer:
                            //Get the Component Which Conatins Point in Unity Space World
                            LineRenderer lineRenderer = GetComponent<LineRenderer>();

                            if (lineRenderer != null)
                            {
                                LineGeoData.GeoPoints.Clear();

                                for (int i = 0; i < lineRenderer.positionCount; i++)
                                {
                                    var point = lineRenderer.GetPosition(i);
                                    var RWPosition = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(point, Container, StoreElevationValue, ElevationMode);

                                    GISTerrainLoaderPointGeoData GeoPoint = new GISTerrainLoaderPointGeoData();
                                    GeoPoint.GeoPoint = RWPosition.ToDVector2();

                                    if (StoreElevationValue)
                                        GeoPoint.Elevation = (float)RWPosition.z;

                                    LineGeoData.GeoPoints.Add(GeoPoint);

                                }

                            }
                            break;
                        case VectorDataLineSource.Custom:
                            //Add custom code  
                            break;
 
                    }
                     break;
                case CoordinatesSource.FromGeoDataScript:
                    //
                    break;
            }
        }
    }
}