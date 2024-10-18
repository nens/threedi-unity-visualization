/*     Unity GIS Tech 2020-2023      */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGeoPointGenerator
    {
        private static List<GISTerrainLoaderSO_GeoPoint> PointsPrefab;
        public static GISTerrainLoaderPrefs Prefs;
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;
 
        public static void GenerateGeoPoint(GISTerrainContainer container, List<GISTerrainLoaderPointGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {

            Prefs = prefs;
            PointsPrefab = Prefs.GeoPointPrefabs;

            VectorParameters = Prefs.VectorParameters_SO;
            GameObject highways = null;
            if (container.transform.Find("GeoPoints") == null)
            {
                highways = new GameObject();
                highways.name = "GeoPoints";
                highways.transform.parent = container.transform;
            }
            else
                highways = container.transform.Find("GeoPoints").gameObject;


            for (int i = 0; i < GeoData.Count; i++)
            {
                var P = GeoData[i];
                var prefab = GetPointPrefab(P.Tag);
                var point = P.GeoPoint;

                if (prefab != null)
                {
                    if(prefab.Prefab)
                    {
                        var GeoPoint = GameObject.Instantiate(prefab.Prefab, highways.transform);

                        if (GeoPoint)
                        {
                            GeoPoint.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container,point);
                            GeoPoint.name = "GeoPoint_" + P.Name;

                            if (VectorParameters.AddVectorDataBaseToGameObjects)
                                GISTerrainLoaderGeoVectorData.AddVectordDataToObject(GeoPoint, VectorObjectType.Point, P,null);

                            if (GeoPoint.GetComponent<GISTerrainLoaderGeoPoint>())
                                GeoPoint.GetComponent<GISTerrainLoaderGeoPoint>().SetName(P.Name);
                        }
                    }
                }
            }

        }
        private static GISTerrainLoaderSO_GeoPoint GetPointPrefab(string pointtype)
        {
            GISTerrainLoaderSO_GeoPoint point = null;
            foreach (var prefab in PointsPrefab)
            {
                if (prefab != null)
                {
                    if (prefab.GeoPointType == pointtype)
                        point = prefab;

                }
            }
            return point;
        }
        #region GPX

        public static void GenerateGeoPoint(GISTerrainLoaderGPXFileData m_GPXFileData, GISTerrainContainer container, GameObject GeoPointPrefab)
        {
            GameObject highways = null;

            if (container.transform.Find("GeoPoints") == null)
            {
                highways = new GameObject();
                highways.name = "GeoPoints";
                highways.transform.parent = container.transform;
            }
            else
                highways = container.transform.Find("GeoPoints").gameObject;


            for (int i = 0; i < m_GPXFileData.WayPoints.Count; i++)
            {
                var point = m_GPXFileData.WayPoints[i];

                if (GeoPointPrefab != null)
                {
                    var GeoPoint = GameObject.Instantiate(GeoPointPrefab, highways.transform).GetComponent<GISTerrainLoaderGeoPoint>();

                    if (GeoPoint)
                    {
                        GeoPoint.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container,new DVector2(point.Longitude, point.Latitude));
                        GeoPoint.name = "GeoPoint_" + point.Name;
                        GeoPoint.SetName(point.Name);
                    }
                }

            }

        }

        #endregion
    }
}

 