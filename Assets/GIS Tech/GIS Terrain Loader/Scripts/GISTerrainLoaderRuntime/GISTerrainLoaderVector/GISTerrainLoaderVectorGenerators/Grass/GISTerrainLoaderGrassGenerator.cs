/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections.Generic;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGrassGenerator
    {
        private static List<DetailPrototype> DetailPrototypes;
        private static List<GISTerrainLoaderSO_GrassObject> GrassPrefabs;
        private static GISTerrainContainer container;

        private static float DetailDistance;
        private static float GrassScaleFactor;

        private static IndexedDetails[,] Totaldetails;

        private static Dictionary<string, int> Grassprototypes_Ind = new Dictionary<string, int>();
        private static int totalGrassCount = 0;
        private static int detailResolution;

        public static void GenerateGrass(GISTerrainContainer container, List<GISTerrainLoaderPolygonGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {
            var GrassPrefabs_str = new List<string>();

            foreach (var p in GrassPrefabs)
            {
                if (p)
                    GrassPrefabs_str.Add(p.grassType);
            }

            if (GeoData.Count == 0)
                return;

            GISTerrainLoaderSO_GrassObject grass_SO = null;

            string grasstype = "forest";

            for (int i = 0; i < GeoData.Count; i++)
            {
                GISTerrainLoaderPolygonGeoData Poly = GeoData[i];

                grasstype = Poly.Tag;
 
                grass_SO = GetGrassPrefab(grasstype);

                if (grass_SO != null)
                {
                    var SpacePoints = Poly.GeoPointsToSpacePoints(container);

                    List<Vector3> Grasspoints = new List<Vector3>();

                    if(prefs.GrassDistribution == PointDistribution.uniformly)
                    {
                        for (int j = 0; j < SpacePoints.Count; j++)
                        {
                            var SubPoly = SpacePoints[j];
                            if (SubPoly.Count == 0)
                                return;

                            var density = (100 - (grass_SO.GrassDensity-1.1f)) * container.Scale.y*0.1f;
                            var RegularPoints = GISTerrainLoaderVectorExtensions.GenerateRegular2DPointsInsidePolygon(SubPoly, density);
                            Grasspoints.AddRange(RegularPoints);
                        }

                    }else
                    {
 

                        for (int j = 0; j < SpacePoints.Count; j++)
                        {
                            var SubPoly = SpacePoints[j];
                            if (SubPoly.Count == 0)
                                return;
 
                            var density = (100 - (grass_SO.GrassDensity - 1.1f)) * container.Scale.y * 0.1f;
                            var RandomPoints = GISTerrainLoaderVectorExtensions.GenerateRandomPointInsidePolygon(SubPoly, density);
                            Grasspoints.AddRange(RandomPoints);
    

                        }

                    }

                    if (Grasspoints.Count < 3) return;

                    foreach (var p in Grasspoints)
                    {
                        var terrainObj = container.GetTerrainTile(p);
                        


                        if (terrainObj != null)
                            SetGrass(grass_SO, terrainObj.Number, p, 3);
                    }
                }
            }

            foreach (var det in Totaldetails)
                det.SetTerraindetails();
        }

        private static void SetGrass(GISTerrainLoaderSO_GrassObject grass_SO, Vector2Int t_index, Vector3 position, float radius)
        {

            int Prefab_index = UnityEngine.Random.Range(0, grass_SO.GrassPrefab.Count);
            var grassModel = grass_SO.GrassPrefab[Prefab_index];
            int m_prototypeIndex = GetGrassPrototypeIndex(grassModel);

            var det = Totaldetails[t_index.x, t_index.y];

            var map = det.details[m_prototypeIndex];

            int TerrainDetailMapSize = det.terrain.terrainData.detailResolution;

            Vector3 terrainPos = Vector3.zero;
            terrainPos.x = (position.x - det.terrain.transform.position.x) / det.terrain.terrainData.size.x;
            terrainPos.y = 0;
            terrainPos.z = (position.z - det.terrain.transform.position.z) / det.terrain.terrainData.size.z;
            
            Vector3 terrainDetailPos = Vector3.zero;
            terrainDetailPos.x = Mathf.FloorToInt(terrainPos.x * TerrainDetailMapSize);
            terrainDetailPos.z = Mathf.FloorToInt(terrainPos.z * TerrainDetailMapSize);

            Vector3 TexturePoint3D = terrainDetailPos;
 
            float[] xymaxmin = new float[4];
            xymaxmin[0] = TexturePoint3D.z + radius;
            xymaxmin[1] = TexturePoint3D.z - radius;
            xymaxmin[2] = TexturePoint3D.x + radius;
            xymaxmin[3] = TexturePoint3D.x - radius;

            for (int y = 0; y < detailResolution; y++)
            {
                if (xymaxmin[2] > y && xymaxmin[3] < y)
                {
                    for (int x = 0; x < detailResolution; x++)
                    {
                        if (xymaxmin[0] > x && xymaxmin[1] < x)
                            map[x, y] = 10* (int)grass_SO.GrassDensity;
                    }
                }
            }

        }
        public static void AddDetailsLayersToTerrains(GISTerrainContainer m_container, GISTerrainLoaderPrefs prefs)
        {
            prefs.LoadAllGrassPrefabs();

            GrassPrefabs = prefs.GrassPrefabs;
            container = m_container;
            DetailDistance = prefs.DetailDistance;
            GrassScaleFactor = prefs.GrassScaleFactor * container.Scale.x;

            int c = 0;
            List<GISTerrainLoaderSO_Grass> objects = new List<GISTerrainLoaderSO_Grass>();
            List<string> objects_type = new List<string>();
            Grassprototypes_Ind = new Dictionary<string, int>();
            totalGrassCount = 0;

            foreach (var element in prefs.GrassPrefabs)
            {
                if (element != null)
                {
                    foreach (var prefab in element.GrassPrefab)
                    {
                        if (prefab != null)
                        {
                            objects.Add(prefab);
                            objects_type.Add(element.grassType);
                            c++;
                        }

                    }
                    if (!Grassprototypes_Ind.ContainsKey(element.grassType))
                        Grassprototypes_Ind.Add(element.grassType, c);
                    c = 0;

                }

            }

            DetailPrototypes = new List<DetailPrototype>(objects.Count);

            for (int i = 0; i < objects.Count; i++)
            {
                var prefab = objects[i];
                DetailPrototypes.Add((CopyDetailPrototype(m_container, prefab)));

            }

            foreach (var SO_prefab in GrassPrefabs)
            {
                if (SO_prefab != null)
                {
                    foreach (var prefab in SO_prefab.GrassPrefab)
                    {
                        totalGrassCount++;
                    }
                }
                else
                    Debug.LogError("Grass Prefab is null ");

            }

            TerrainData tdata = container.terrains[0, 0].terrainData;
            detailResolution = tdata.detailResolution;
            Totaldetails = new IndexedDetails[container.TerrainCount.x, container.TerrainCount.y];

            foreach (var t in container.terrains)
            {
                var IndexedDetails = new IndexedDetails(t.terrain, new Vector2Int(detailResolution, detailResolution), totalGrassCount);
                Totaldetails[t.Number.x, t.Number.y] = IndexedDetails;

            }

            foreach (var terrain in container.terrains)
            {
                terrain.terrainData.detailPrototypes = DetailPrototypes.ToArray();
                terrain.terrain.detailObjectDistance = DetailDistance;
            }



        }
        private static DetailPrototype CopyDetailPrototype(GISTerrainContainer m_container, GISTerrainLoaderSO_Grass Source_item)
        {
            var detailPrototype = new DetailPrototype();

            detailPrototype.renderMode = DetailRenderMode.GrassBillboard;
            detailPrototype.prototypeTexture = Source_item.DetailTexture;
            detailPrototype.minWidth = Source_item.MinWidth;
            detailPrototype.maxWidth = Source_item.MaxWidth * GrassScaleFactor * m_container.Scale.x;
            detailPrototype.minHeight = Source_item.MinHeight;
            detailPrototype.maxHeight = Source_item.MaxHeight * GrassScaleFactor * m_container.Scale.x;
            detailPrototype.noiseSpread = Source_item.Noise;
            detailPrototype.healthyColor = Source_item.HealthyColor;
            detailPrototype.dryColor = Source_item.DryColor;


            if (Source_item.BillBoard)
                detailPrototype.renderMode = DetailRenderMode.GrassBillboard;
            else detailPrototype.renderMode = DetailRenderMode.Grass;

            return detailPrototype;
        }
        private static int GetGrassPrototypeIndex(GISTerrainLoaderSO_Grass SO_Grass)
        {
            int Index = 0;

            for (int j = 0; j < DetailPrototypes.Count; j++)
            {
                var Details = DetailPrototypes[j];

                if (SO_Grass.DetailTexture.name == Details.prototypeTexture.name)
                {
                    Index = DetailPrototypes.IndexOf(Details);
                    continue;
                }


            }

            return Index;
        }
        private static GISTerrainLoaderSO_GrassObject GetGrassPrefab(string grassType)
        {
            GISTerrainLoaderSO_GrassObject grass = null;
            foreach (var prefab in GrassPrefabs)
            {
                if (prefab != null)
                {
                    if (prefab.grassType == grassType)
                    {
                        grass = prefab;

                    }


                }
            }
            return grass;
        }

    }
    public class IndexedDetails
    {
        public List<int[,]> details = new List<int[,]>();
        public Terrain terrain;
        public IndexedDetails(Terrain m_terrain, Vector2Int dim,int totalGrassCount)
        {
            terrain = m_terrain;

            for (int i = 0; i < totalGrassCount; i++)
            {
                details.Add(new int[dim.x, dim.y]);
            }
        }

        public void SetTerraindetails()
        {
            for (var x = 0; x < details.Count; x++)
            {
                terrain.terrainData.SetDetailLayer(0, 0, x, details[x]);
            }
           
        }
    }

}
