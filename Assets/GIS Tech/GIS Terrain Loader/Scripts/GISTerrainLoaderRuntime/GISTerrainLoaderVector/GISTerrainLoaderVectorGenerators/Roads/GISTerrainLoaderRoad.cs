/*     Unity GIS Tech 2020-2023      */


using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRoad
    {
        public string RoadTypeName;
        public Material material;
        public float width;
        public Color color;
        public Vector3[] Points;

        public GISTerrainLoaderSO_Road GTLSO_Road;

        public GISTerrainLoaderRoad(GISTerrainLoaderSO_Road so_road, GISTerrainContainer m_container)
        {
            width = so_road.RoadWidth* m_container.Scale.y;
            color = so_road.RoadColor;
            material = so_road.Roadmaterial;
        }
 
    }

    public class RoadConstants
    {
        public const float Scale = 1f;
    }
    public class RoadLableConstants
    {
        public const float roadNameLabelSize = 0.1f;
        public const float roadNameLabelPosY = 0.2f * 10;
        public const float roadNameStringSizeMultipler = 0.35f * 10f;
        public static Color roadNameLabelColor = Color.yellow;
    }
}
