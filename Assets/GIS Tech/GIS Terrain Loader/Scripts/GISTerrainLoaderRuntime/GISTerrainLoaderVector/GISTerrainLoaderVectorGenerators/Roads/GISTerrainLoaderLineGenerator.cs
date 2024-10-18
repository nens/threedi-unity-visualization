/*     Unity GIS Tech 2020-2023      */


using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderLineGenerator
    {
        private static List<GISTerrainLoaderSO_Road> RoadsPrefab;
        private static GameObject highwaysContainer;
        private static GISTerrainContainer container;
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;
        public static void CreateHighways(GISTerrainContainer m_container, List<GISTerrainLoaderLineGeoData> GeoData, GISTerrainLoaderVectorParameters_SO m_VectorParameters, List<GISTerrainLoaderSO_Road> m_RoadsPrefab)
        {

            RoadsPrefab = m_RoadsPrefab;
            highwaysContainer = new GameObject();
            highwaysContainer.name = "Roads";
            highwaysContainer.transform.parent = m_container.transform;
            container = m_container;
            VectorParameters = m_VectorParameters;
            GenerateRoades(GeoData);
        }
        private static void GenerateRoades(List<GISTerrainLoaderLineGeoData> GeoData)
        {
            if (GeoData.Count != 0)
            {
                foreach (var Road in GeoData)
                    CreateHighway(Road, container, highwaysContainer.transform);
            }
        }

        public static GameObject CreateHighway(GISTerrainLoaderLineGeoData Road, GISTerrainContainer container, Transform parent)
        {
            TerrainData tdata = container.terrains[0, 0].terrainData;

            int detailResolution = tdata.detailResolution;

            Vector3[] linePoints = new Vector3[Road.GeoPoints.Count];

            for (int i = 0; i < Road.GeoPoints.Count; i++)
            {
                var latlon = new DVector2(Road.GeoPoints[i].GeoPoint.x, Road.GeoPoints[i].GeoPoint.y);
               
                linePoints[i] = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container,latlon);


            }

            var roadtype = Road.Tag;

            var defaultRoad = RoadsPrefab[0];

            foreach (var so_road in RoadsPrefab)
            {
                if (so_road.name == roadtype)
                {
                    defaultRoad = so_road;
                }
            }

            var road = new GISTerrainLoaderRoad(defaultRoad, container);

            road.Points = linePoints;

            GameObject m_road = null;

            m_road = CreateLine(road);

            m_road.transform.parent = parent;

            m_road.name = roadtype;

            if (m_road != null)
            {
                m_road.name = Road.Name + "_" + Road.Tag;

                if (GISTerrainLoaderRoadsGenerator.EnableRoadName)
                    GISTerrainLoaderRoadsGenerator.CreateRoadNameLabel(linePoints, m_road.name, m_road.transform, container);

                if(VectorParameters.AddVectorDataBaseToGameObjects)
                GISTerrainLoaderGeoVectorData.AddVectordDataToObject(m_road, VectorObjectType.Line,null,Road);
            }

            return m_road;
        }
 
        public static GameObject CreateLine(GISTerrainLoaderRoad m_road )
        {
            LineRenderer lineRender = RLine(m_road.Points);

            lineRender.alignment = LineAlignment.TransformZ;

            lineRender.material = m_road.material;


            lineRender.startWidth = m_road.width;
            lineRender.endWidth = m_road.width;

            lineRender.startColor = m_road.color;
            lineRender.endColor = m_road.color;



            return lineRender.gameObject;

        }
        public static LineRenderer RLine(Vector3[] linePoints)
        {
            GameObject result = new GameObject();

            result.transform.Rotate(new Vector3(90, 0, 0));

            LineRenderer lineRender = result.AddComponent<LineRenderer>();
            lineRender.positionCount = linePoints.Length;
            lineRender.SetPositions(linePoints);

            return lineRender;

        }
    }
}
