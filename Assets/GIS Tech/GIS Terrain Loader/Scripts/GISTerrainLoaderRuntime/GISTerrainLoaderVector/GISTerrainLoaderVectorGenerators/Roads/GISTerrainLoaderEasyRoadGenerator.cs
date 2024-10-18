/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if EASYROADS3D || EASYROADS3D_PRO
using EasyRoads3Dv3;
#endif
namespace GISTech.GISTerrainLoader
{

#if EASYROADS3D || EASYROADS3D_PRO

    using EasyRoads3Dv3;

    

    public class GISTerrainLoaderEasyRoadGenerator
    {

        public static ERRoad Eroad;

        private static GISTerrainContainer container;

        private static List<GISTerrainLoaderSO_Road> RoadsPrefab;
            
        public static ERRoadNetwork roadNetwork;

        private static GameObject roadNetworkGO = null;

        private static Transform roadContiner;

        public static OptionEnabDisab ES3_BuildTerrain;

        public static float ES3_RiseOffset;

        private static bool IsRuntime = false;

        public static OptionEnabDisab ES3_AddRoadType;

        public static ERModularBase ErModularBase;

        private static GISTerrainLoaderPrefs Prefs;
        public static void CreateHighways(GISTerrainContainer m_container, List<GISTerrainLoaderLineGeoData> GeoData, GISTerrainLoaderPrefs Prefs)
        {
            if (Application.isEditor) IsRuntime = false;
            if (Application.isPlaying) IsRuntime = true;

            if (EasyRoadBaseModelExist())
            {
                container = m_container;

                RoadsPrefab = Prefs.RoadPrefabs;

                roadContiner = null;

                var ERNet_01 = Resources.Load("ERRoadNetwork") as GameObject;
                var ERNet_02 = Resources.Load("ER Road Network") as GameObject;

                if (ERNet_01)
                {
                    roadNetworkGO = UnityEngine.Object.Instantiate(ERNet_01);
                }
                else
                {
                    if (ERNet_02)
                        roadNetworkGO = UnityEngine.Object.Instantiate(ERNet_02);
                }

                if (roadNetworkGO)
                {
                    roadNetworkGO.transform.parent = container.transform;
                    roadNetworkGO.name = "Road Network";
                    roadNetworkGO.transform.position = Vector3.zero;
                    roadContiner = roadNetworkGO.transform.Find("Road Objects");
                }
 
                roadNetwork = new ERRoadNetwork();
                roadNetwork.roadNetwork.importSideObjectsAlert = false;
                roadNetwork.roadNetwork.importRoadPresetsAlert = false;
                roadNetwork.roadNetwork.importCrossingPresetsAlert = false;
                roadNetwork.roadNetwork.importSidewalkPresetsAlert = false;

                ErModularBase = UnityEngine.Object.FindObjectOfType<ERModularBase>();
                AddRoadTypes();


                GenerateRoades(GeoData);
                ErModularBase.roadTypes.Clear();
            }
            else
            {
                Debug.Log("EasyRoad3D asset not exists or the main ES resources not added to the current project.. ");
                Debug.Log("Please Import EasyRoad and add 'EASYROADS3D || EASYROADS3D_PRO'  to the scripting Define Symbols from Player Settings plane .");
            }
        }
        private static void GenerateRoades(List<GISTerrainLoaderLineGeoData> GeoData)
        {
            if (GeoData.Count != 0)
            {
                foreach (var Road in GeoData)
                    CreateHighway(Road);
            }

            Finilize(ES3_BuildTerrain, ES3_RiseOffset);

        }
        public static GameObject CreateHighway(GISTerrainLoaderLineGeoData GeoRoad)
        {
            TerrainData tdata = container.terrains[0, 0].terrainData;

            int detailResolution = tdata.detailResolution;

            Vector3[] linePoints = new Vector3[GeoRoad.GeoPoints.Count];

            for (int i = 0; i < GeoRoad.GeoPoints.Count; i++)
            {
                var latlon = new DVector2(GeoRoad.GeoPoints[i].GeoPoint.x, GeoRoad.GeoPoints[i].GeoPoint.y);
 
                linePoints[i] = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container,latlon);
            }

            var Georoadtype = GeoRoad.Tag;

            var so_defaultRoad = RoadsPrefab[0];

            foreach (var so_road in RoadsPrefab)
            {
                if (so_road.name == Georoadtype)
                {
                    so_defaultRoad = so_road;
                }
            }

            var GTLRoad = new GISTerrainLoaderRoad(so_defaultRoad, container);
            GTLRoad.Points = linePoints;
            GTLRoad.RoadTypeName = so_defaultRoad.RoadType;
            GTLRoad.GTLSO_Road = so_defaultRoad;

                GameObject m_roadGO = null;

            if (EasyRoadBaseModelExist())
            {
                m_roadGO = CreateRoad(GTLRoad, container, IsRuntime);

                var Modular = UnityEngine.Object.FindObjectOfType<EasyRoads3Dv3.ERModularBase>();

                Modular.OnBuildModeEnter();

            }

            if (m_roadGO != null)
            {
                m_roadGO.name = GeoRoad.Name + "_" + GeoRoad.Tag;

                if (GISTerrainLoaderRoadsGenerator.EnableRoadName)
                    GISTerrainLoaderRoadsGenerator.CreateRoadNameLabel(linePoints, m_roadGO.name, m_roadGO.transform, container);

            }

            return m_roadGO;
        }

        public static GameObject CreateRoad(GISTerrainLoaderRoad m_GTLRoad, GISTerrainContainer m_container, bool m_Runtime=false)
        {
            container = m_container;

            GameObject GO_Road = null;

            if (!m_Runtime)
                GO_Road = EditorCreateRoad(m_GTLRoad);
            else
                GO_Road = RuntimeCreateRoad(m_GTLRoad);

            return GO_Road;
        }
        public static GameObject EditorCreateRoad(GISTerrainLoaderRoad m_road)
        {
            ERRoadType roadType = new ERRoadType();
 
            roadType.roadWidth = m_road.width;
            roadType.roadMaterial = m_road.material;
            roadType.roadTypeName = m_road.RoadTypeName;

            Eroad = new ERRoad();

           
            Eroad = roadNetwork.CreateRoad(roadType.roadTypeName, roadType, m_road.Points);
            Eroad.roadScript.roadType = roadType.id;
            Eroad.SetRoadType(roadType);

            Eroad.SnapToTerrain(true);
            Eroad.gameObject.isStatic = false;

            if (roadContiner != null)
                Eroad.gameObject.transform.parent = roadContiner;
 
            return Eroad.gameObject;
        }
        public static GameObject RuntimeCreateRoad(GISTerrainLoaderRoad m_road)
        {
            if (Object.FindObjectOfType<ERModularBase>() == null)
            {
                GameObject roadNetworkGO = null;
                var ERNet_01 = Resources.Load("ER Road Network") as GameObject;
                var ERNet_02 = Resources.Load("ERRoadNetwork") as GameObject;

                if (ERNet_01)
                {
                    roadNetworkGO = Object.Instantiate(ERNet_01);
                }
                else
                {
                    if (ERNet_02)
                        roadNetworkGO = Object.Instantiate(ERNet_02);
                }

                if (roadNetworkGO != null)
                {
                    roadNetworkGO.name = "Road Network";
                    roadNetworkGO.transform.position = Vector3.zero;
                    roadContiner = roadNetworkGO.transform.Find("Road Objects");
                }

            }

            roadNetwork = new ERRoadNetwork();
            roadNetwork.roadNetwork.importSideObjectsAlert = false;
            roadNetwork.roadNetwork.importRoadPresetsAlert = false;
            roadNetwork.roadNetwork.importCrossingPresetsAlert = false;
            roadNetwork.roadNetwork.importSidewalkPresetsAlert = false;

            ERRoadType roadType = new ERRoadType();

            roadType.roadWidth = m_road.width;
            roadType.roadMaterial = m_road.material;

            Eroad = roadNetwork.CreateRoad(m_road.ToString(), roadType, m_road.Points);
            Eroad.SnapToTerrain(true);
            Eroad.gameObject.isStatic = false;

            Eroad.gameObject.transform.parent = container.transform;

            return Eroad.gameObject;
        }
        public static void Finilize(OptionEnabDisab BuildRoad, float RaiseOffset)
        {
            if (roadNetwork != null)
            {
                roadNetwork.roadNetwork.BuildTerrainRoutine(roadNetwork);

                if (BuildRoad == OptionEnabDisab.Enable)
                {
                    roadNetwork.SetRaiseOffset(RaiseOffset);
                    roadNetwork.BuildRoadNetwork();
                }
               
            }
        }
        private static bool EasyRoadBaseModelExist()
        {
            bool exist = false;

            var ERNet_01 = Resources.Load("ERRoadNetwork") as GameObject;
            var ERNet_02 = Resources.Load("ER Road Network") as GameObject;


            if (ERNet_01 == null)
                exist = true;

            if (ERNet_02 == null)
                exist = true;

            return exist;
        }

        private static void AddRoadTypes()
        {
           var ObjectsLog = Resources.Load("ERSideObjectsLog") as GameObject;
            if(ObjectsLog == null)
                ObjectsLog = Resources.Load("ERProjectLog") as GameObject;

            if(ObjectsLog!=null)
            {
                var SideObjectsLog = ObjectsLog.GetComponent<ERSideObjectLog>();
                SideObjectsLog.roadPresets = new List<QDQDOOQQDQODD>();
                ErModularBase.roadTypes.Clear();

                foreach (var SO_Road in RoadsPrefab)
                {
                    int index = RoadsPrefab.IndexOf(SO_Road) + 1;

                    QDQDOOQQDQODD ESRoad = new QDQDOOQQDQODD(index);
                    ESRoad.roadTypeName = SO_Road.RoadType;
                    ESRoad.roadWidth = SO_Road.RoadWidth;
                    ESRoad.roadMaterial = SO_Road.Roadmaterial;
                    ESRoad.id = index;
                    ESRoad.type = ERRoadWayType.Primary;
                    if (!SideObjectsLog.roadPresets.Contains(ESRoad))
                        SideObjectsLog.roadPresets.Add(ESRoad);
                }
            }
        }
    }
#endif
}