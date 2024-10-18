/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if RoadCreatorPro
using RoadCreatorPro;
#endif

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderUnityRoadCreatorPro 
    {

#if RoadCreatorPro

        private static List<GISTerrainLoaderSO_Road> RoadsPrefab;
        private static GameObject highwaysContainer;
        private static GISTerrainContainer container;
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;
        private static RoadSystem RoadSystemGenerator;
        private static GISTerrainLoaderPrefs Prefs;
        public static void CreateHighways(GISTerrainContainer m_container, List<GISTerrainLoaderLineGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {
            Prefs = prefs;
            container = m_container;
            VectorParameters = Prefs.VectorParameters_SO;
            RoadsPrefab = Prefs.RoadPrefabs;

            if(m_container.transform.Find("Roads"))
            {
                highwaysContainer = m_container.transform.Find("Roads").gameObject;
            }else
            {
                highwaysContainer = new GameObject("Roads");
                highwaysContainer.transform.parent = m_container.transform;
            }

            if(RoadSystemGenerator==null)
            {
                if (highwaysContainer.transform.Find("RoadCreatorPro"))
                {
                    RoadSystemGenerator = highwaysContainer.transform.Find("RoadCreatorPro").gameObject.GetComponent<RoadSystem>();
                }
                else
                {
                    GameObject roadCreatorPro = new GameObject("RoadCreatorPro");
                    RoadSystemGenerator = roadCreatorPro.AddComponent<RoadSystem>();
                    roadCreatorPro.transform.SetParent(highwaysContainer.transform);
                }
            }

            GenerateRoades(RoadSystemGenerator, GeoData);
 
        }
 
        private static void GenerateRoades(RoadSystem RoadSystemGenerator,List<GISTerrainLoaderLineGeoData> GeoData)
        {
            foreach (var Road in GeoData)
            {
                GameObject road = new GameObject("Road_"+ Road.Tag+"_"+ Road.ID);
                var roadcreator = road.AddComponent<RoadCreator>();
                road.transform.parent = RoadSystemGenerator.transform;
                roadcreator.InitializeSystem();
                roadcreator.deformMeshToTerrain = true;

                CreateHighway(roadcreator,Road, container, highwaysContainer.transform);

            }
        }
        public static void CreateHighway(RoadCreator roadCreator ,GISTerrainLoaderLineGeoData Road, GISTerrainContainer container, Transform parent)
        {
            List<Vector3> linePoints = new List<Vector3>();

            GameObject pointsContainer = new GameObject("Points");
            pointsContainer.transform.parent = roadCreator.transform;

            var roadtype = Road.Tag;

            var defaultRoad = RoadsPrefab[0];

            foreach (var so_road in RoadsPrefab)
            {
                if (so_road.name == roadtype)
                {
                    defaultRoad = so_road;
                }
            }


            foreach (var p in Road.GeoPoints)
            {
                var latlon = new DVector2(p.GeoPoint.x, p.GeoPoint.y); 
                var SpacePos = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, latlon);
                linePoints.Add(SpacePos);

                roadCreator.CreatePoint(null, false, false, SpacePos);
            }

            roadCreator.AddLanes(defaultRoad.LanesCount);

            foreach(var lanes in roadCreator.lanes)
            {
                lanes.width = AnimationCurve.Constant(0, 1 * container.Scale.y, defaultRoad.RoadWidth * container.Scale.y);

                lanes.materials = new List<Material>(0);

                lanes.materials.Add(defaultRoad.Roadmaterial);

                lanes.textureTilingMultiplier = defaultRoad.TextureTilingMultiplier* container.Scale.y;
            }
            if(Application.isEditor)
            GameObject.DestroyImmediate(pointsContainer);
            else
                GameObject.Destroy(pointsContainer);
            
            roadCreator.Regenerate();

            if (VectorParameters.AddVectorDataBaseToGameObjects && roadCreator)
                GISTerrainLoaderGeoVectorData.AddVectordDataToObject(roadCreator.gameObject, VectorObjectType.Line, null, Road);


        }
#endif

    }
}

