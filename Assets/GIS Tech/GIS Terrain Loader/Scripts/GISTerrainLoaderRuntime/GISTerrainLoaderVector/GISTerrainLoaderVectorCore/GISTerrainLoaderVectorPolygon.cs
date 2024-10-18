/*     Unity GIS Tech 2020-2024      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderVectorPolygon : MonoBehaviour
    {
        public VectorDataPolySource PolyDataSource;
        public GISTerrainLoaderPolygonGeoData PolygonGeoData;

        public void UpdateGeoDataFrom(GISTerrainContainer Container, CoordinatesSource coordinatesSource = CoordinatesSource.FromGeoDataScript, bool StoreElevationValue = false, RealWorldElevation ElevationMode = RealWorldElevation.Elevation)
        {
            switch (coordinatesSource)
            {

                case CoordinatesSource.FromTerrain:

                    switch (PolyDataSource)
                    {
                        case VectorDataPolySource.MeshFilter:
                            //Get the Component Which Conatins Point in Unity Space World
                            var filter = GetComponent<MeshFilter>();

                            if (filter != null)
                            {
                                var SpacePoints = GISTerrainLoaderPointsFromMesh.GetPointsFromMesh(filter);

                                PolygonGeoData.GeoPoints.Clear();

                                for (int i = 0; i < SpacePoints.Count; i++)
                                {
                                    var sub = SpacePoints[i];

                                    List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();

                                    for (int j = 0; j < sub.Count; j++)
                                    {

                                        DVector3 RWPosition = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(sub[j] + this.transform.position, Container, StoreElevationValue, ElevationMode);

                                        GISTerrainLoaderPointGeoData GeoPoint = new GISTerrainLoaderPointGeoData();
                                        GeoPoint.GeoPoint = RWPosition.ToDVector2();

                                        
                                        if (StoreElevationValue)
                                            GeoPoint.Elevation = (float)RWPosition.z;
                                        

                                        SubGeoPoints.Add(GeoPoint);
                                    }

                                    PolygonGeoData.GeoPoints.Add(SubGeoPoints);
                                }

                            }
                            break;
                        case VectorDataPolySource.Custom:
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
 