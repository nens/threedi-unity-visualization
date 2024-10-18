/*     Unity GIS Tech 2020-2021     */

using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainTile : MonoBehaviour
    {
        [HideInInspector]
        public string ElevationFilePath;
        [HideInInspector]
        public GISTerrainContainer container;
        [HideInInspector]
        public Vector3 size;
        [HideInInspector]
        public Vector2Int Number;

        public Terrain terrain;

        public TerrainData terrainData;
 
        [HideInInspector]
        public ElevationState ElevationState;
        [HideInInspector]
        public TextureState TextureState;

        /// <summary>
        /// Check if point intersect with terrain bounds
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public bool IncludePoint(Vector3 Position)
        {
            bool Include = false;
 
            var MinLat = terrain.terrainData.bounds.min.y;
            var MinLon = terrain.terrainData.bounds.min.x;
            var MaxLat = terrain.terrainData.bounds.max.y;
            var MaxLon = terrain.terrainData.bounds.max.x;

            if (Position.x >= MinLon && Position.x <= MaxLon && Position.y >= MinLat && Position.y <= MaxLat)
                Include = true;
            return Include;
        }
    }

}