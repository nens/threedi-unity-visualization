using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderBaseboardsGenerator
    {
        public static float BorderHigh = 0;
        private static int heightmapResolution;
 
        public static float TextureScaleX = 0;
        public static float TextureScaleY = 0;
 
        private static Vector3[] vertices;
        private static Vector2[] uvs;
        private static Vector3[] normals;
        private static int[] triangles;
        private static GameObject BorderContainer;
        public static void GenerateTerrainBaseboards(GISTerrainContainer Container,int m_borderHigh, Material Bordermaterial=null)
        {
            if (Container == null) return;

            if (Bordermaterial == null)
            {
                Bordermaterial = (Material)Resources.Load("Materials/Default-Border-Standard", typeof(Material));

                if (Bordermaterial == null)
                    Debug.LogError("Border terrain material null or not found in 'Resources/Materials/Default-Border-Standard' ");
            }

            TextureScaleX = Bordermaterial.mainTextureScale.x;
            TextureScaleY = Bordermaterial.mainTextureScale.y;

            BorderHigh = m_borderHigh* Container.Scale.y;
            heightmapResolution = Container.terrains[0, 0].terrainData.heightmapResolution;

            int countVertices = heightmapResolution * 2;
            vertices = new Vector3[countVertices];
            normals = new Vector3[countVertices];
            uvs = new Vector2[countVertices];
            triangles = new int[(heightmapResolution - 1) * 6];
 
            BorderContainer = new GameObject("Borders");
            BorderContainer.transform.parent = Container.transform;

            for (int x = 0; x < Container.TerrainCount.x; x++)
            {
                bool left = x == 0;
                bool right = x == Container.TerrainCount.x - 1;

                for (int z = 0; z < Container.TerrainCount.y; z++)
                {
                    bool top = z == 0;
                    bool bottom = z == Container.TerrainCount.y - 1;
 
                    var Tile = Container.terrains[x, z];

                    CreateTerrainBorder(BorderContainer,Tile, Bordermaterial, left, right, top, bottom);
                }
            }
        }

        private static void CreateTerrainBorder(GameObject BorderContainer,GISTerrainTile TerrainTile, Material mat, bool left, bool right, bool top, bool bottom)
        {
            TerrainData terrainData = TerrainTile.terrainData;

            float heightScale = terrainData.size.y;
            float maxHeight = float.MinValue;
 
            if (left)
            {
                float[,] heights = terrainData.GetHeights(0, 0, 1, heightmapResolution);
                float scale = terrainData.size.z / (heightmapResolution - 1);
                float x = 0;

                for (int i = 0; i < heightmapResolution; i++)
                {
                    float h = heights[i, 0] * heightScale;
                    if (h > maxHeight) maxHeight = h;

                    vertices[i] = new Vector3(x, h, i * scale);
                }

                CreateBorderMesh(TerrainTile, mat, Vector3.left, maxHeight, TextureScaleX, false);
            }

            if (right)
            {
                float[,] heights = terrainData.GetHeights(heightmapResolution - 1, 0, 1, heightmapResolution);
                float scale = terrainData.size.z / (heightmapResolution - 1);
                float x = terrainData.size.x;

                for (int i = 0; i < heightmapResolution; i++)
                {
                    float h = heights[i, 0] * heightScale;
                    if (h > maxHeight) maxHeight = h;

                    vertices[i] = new Vector3(x, h, i * scale);
                }

                CreateBorderMesh(TerrainTile, mat, Vector3.right, maxHeight, TextureScaleX, true);
            }

            if (top)
            {
                float[,] heights = terrainData.GetHeights(0, 0, heightmapResolution, 1);
                float scale = terrainData.size.x / (heightmapResolution - 1);
                float z = 0;

                for (int i = 0; i < heightmapResolution; i++)
                {
                    float h = heights[0, i] * heightScale;
                    if (h > maxHeight) maxHeight = h;

                    vertices[i] = new Vector3(i * scale, h, z);
                }

                CreateBorderMesh(TerrainTile, mat, Vector3.forward, maxHeight, TextureScaleY, true);
            }

            if (bottom)
            {
                float[,] heights = terrainData.GetHeights(0, heightmapResolution - 1, heightmapResolution, 1);
                float scale = terrainData.size.x / (heightmapResolution - 1);
                float z = terrainData.size.z;

                for (int i = 0; i < heightmapResolution; i++)
                {
                    float h = heights[0, i] * heightScale;
                    if (h > maxHeight) maxHeight = h;

                    vertices[i] = new Vector3(i * scale, h, z);
                }

                CreateBorderMesh(TerrainTile, mat, Vector3.back, maxHeight, TextureScaleY, false);
            }
        }

        private static void CreateBorderMesh(GISTerrainTile TerrainTile, Material mat, Vector3 normalDirection, float maxHeight, float UVFactor, bool forwardTriangleOrder)
        {
            int ti = 0;
            for (int i = 0; i < heightmapResolution; i++)
            {
                int i2 = i + heightmapResolution;

                Vector3 v = vertices[i];
                float h = v.y;
                vertices[i2] = v;
                vertices[i2].y = BorderHigh;

                float uvx = i / (float)heightmapResolution * UVFactor;
                uvs[i] = new Vector2(uvx, (h - BorderHigh) / (maxHeight - BorderHigh));
                uvs[i2] = new Vector2(uvx, 0);

                normals[i] = normalDirection;
                normals[i2] = normalDirection;

                if (i < heightmapResolution - 1)
                {
                    triangles[ti++] = i;
                    if (forwardTriangleOrder)
                    {
                        triangles[ti++] = i + 1;
                        triangles[ti++] = i2 + 1;
                    }
                    else
                    {
                        triangles[ti++] = i2 + 1;
                        triangles[ti++] = i + 1;
                    }

                    triangles[ti++] = i;

                    if (forwardTriangleOrder)
                    {
                        triangles[ti++] = i2 + 1;
                        triangles[ti++] = i2;
                    }
                    else
                    {
                        triangles[ti++] = i2;
                        triangles[ti++] = i2 + 1;
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            GameObject go = new GameObject("Border");
            go.transform.parent = BorderContainer.transform;
            go.transform.localPosition = TerrainTile.transform.position;
            
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }

    }
}