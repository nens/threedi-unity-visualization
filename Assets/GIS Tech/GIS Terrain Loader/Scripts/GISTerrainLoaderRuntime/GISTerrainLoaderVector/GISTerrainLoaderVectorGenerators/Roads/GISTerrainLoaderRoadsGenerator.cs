/*     Unity GIS Tech 2020-2023      */

using UnityEngine;
using System.Collections.Generic;


#if EASYROADS3D || EASYROADS3D_PRO
using EasyRoads3Dv3;
#endif


#if RoadCreatorPro
using RoadCreatorPro;
#endif

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRoadsGenerator
    {
        private static RoadGeneratorType roadGenerator;

        public static bool EnableRoadName;

        private static List<GISTerrainLoaderSO_Road> RoadsPrefab;

        private static GISTerrainLoaderPrefs Prefs;
        public static void GenerateTerrainRoades(GISTerrainContainer container, List<GISTerrainLoaderLineGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {
            Prefs = prefs;
            RoadsPrefab = Prefs.RoadPrefabs;
            roadGenerator = Prefs.RoadGenerator;
            EnableRoadName = Prefs.EnableRoadName;

            if (GeoData.Count > 0)
            {
                switch (roadGenerator)
                {
                    case RoadGeneratorType.Line:

                        GISTerrainLoaderLineGenerator.CreateHighways(container, GeoData, Prefs.VectorParameters_SO, Prefs.RoadPrefabs);
                        break;
                    case RoadGeneratorType.RoadCreatorPro:
#if RoadCreatorPro
                            GISTerrainLoaderUnityRoadCreatorPro.CreateHighways(container, GeoData, Prefs);
#endif
                        break;
                    case RoadGeneratorType.EasyRoad3D:

#if EASYROADS3D || EASYROADS3D_PRO
                    GISTerrainLoaderEasyRoadGenerator.ES3_BuildTerrain = Prefs.BuildTerrains;
                    GISTerrainLoaderEasyRoadGenerator.ES3_RiseOffset = Prefs.RoadRaiseOffset;
                    GISTerrainLoaderEasyRoadGenerator.CreateHighways(container, GeoData,Prefs);
#endif
                        break;

                }
            }
   

            GISTerrainLoaderGeoConversion.terrain = null;
        }
        private static Vector3[] FindMaxDistance(Vector3[] linePoints)
        {
            Vector3[] result = new Vector3[2];
            float max = 0;
            for (int i = 1; i < linePoints.Length; i++)
            {
                float dis = Vector3.Distance(linePoints[i - 1], linePoints[i]);
                if (dis > max)
                {
                    max = dis;
                    result[0] = linePoints[i - 1];
                    result[1] = linePoints[i];
                }

            }

            return result;
        }
        public static void CreateRoadNameLabel(Vector3[] linePoints, string roadName, Transform parent, GISTerrainContainer container)
        {
            if (linePoints.Length > 1)
            {
                int b = linePoints.Length / 2;
                int a = b - 1;
                Vector3 pointA = new Vector3(linePoints[a].x, linePoints[a].y, linePoints[a].z);
                Vector3 pointB = new Vector3(linePoints[b].x, linePoints[b].y, linePoints[b].z);

                if (Vector3.Distance(pointA, pointB) > roadName.Length * RoadLableConstants.roadNameStringSizeMultipler)
                {
                    CreateRoadNameLabel(pointA, pointB, roadName, parent, container);
                }
                else
                {
                    Vector3[] maxDis = FindMaxDistance(linePoints);

                    pointA = maxDis[0];
                    pointB = maxDis[1];

                    if (Vector3.Distance(pointA, pointB) > roadName.Length * RoadLableConstants.roadNameStringSizeMultipler)
                    {
                        CreateRoadNameLabel(pointA, pointB, roadName, parent, container);
                    }
                }
            }
        }
        public static void CreateRoadNameLabel(Vector3 pointA, Vector3 pointB, string roadName, Transform parent, GISTerrainContainer container)
        {
            GameObject text = new GameObject();
            text.transform.parent = parent.transform;
            text.name = "Road name";
            TextMesh textMesh = text.AddComponent<TextMesh>();
            textMesh.text = roadName;
            textMesh.transform.Rotate(90, 90, 0);
            textMesh.fontSize = 100;
            textMesh.characterSize = RoadLableConstants.roadNameLabelSize * container.LableScaleOverage();
            textMesh.color = RoadLableConstants.roadNameLabelColor;

            if (pointA.z < pointB.z)
            {
                var elevation = GISTerrainLoaderGeoConversion.GetHeight(text.transform.position) + 0.7f;
                text.transform.position = new Vector3(pointB.x, elevation, pointB.z);
                text.transform.LookAt(pointA);
                text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
            }
            else if (pointA.z > pointB.z)
            {
                var elevation = GISTerrainLoaderGeoConversion.GetHeight(text.transform.position) + 0.7f;
                text.transform.position = new Vector3(pointA.x, elevation, pointA.z);
                text.transform.LookAt(pointB);
                text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
            }
            else
            {
                if (pointA.x < pointB.x)
                {
                    var elevation = GISTerrainLoaderGeoConversion.GetHeight(text.transform.position) + 0.7f;
                    text.transform.position = new Vector3(pointA.x, elevation, pointA.z);
                    text.transform.LookAt(pointB);
                    text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
                }
                else
                {
                    var elevation = GISTerrainLoaderGeoConversion.GetHeight(text.transform.position) + 0.7f;
                    text.transform.position = new Vector3(pointB.x, elevation, pointB.z);
                    text.transform.LookAt(pointA);
                    text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
                }
            }
        }
    }
}