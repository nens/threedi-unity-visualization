/*     Unity GIS Tech 2020-2023      */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTreeGenerator
    {
        private static List<PrototypeTagIndex> Treeprototypes;
        private static Dictionary<string,int> Treeprototypes_Ind= new Dictionary<string, int>();
 
        private static GISTerrainContainer container;
        private static float TreeDistance;
        private static float BillBoardStartDistance;
         private static GISTerrainLoaderPrefs Prefs;
        public static void GenerateTrees(GISTerrainContainer container, List<GISTerrainLoaderPolygonGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {
            Prefs = prefs;
 
            if (GeoData.Count == 0)
                return;
 
            GISTerrainLoaderSO_Tree tree_SO = null;

            string treetype = "forest";

            for (int i = 0; i < GeoData.Count; i++)
            {
                GISTerrainLoaderPolygonGeoData Poly = GeoData[i];
 
                treetype = Poly.Tag;

                tree_SO = GetTreePrefab(treetype);

                if (tree_SO != null)
                {
                    if(Poly.GeoPoints.Count>0)
                    {
                        var SpacePoints = Poly.GeoPointsToSpacePoints(container);

                        if (SpacePoints.Count == 0)
                            return;
         
                        List<Vector3> Treepoints = new List<Vector3>();

                        if (prefs.TreeDistribution == PointDistribution.uniformly)
                        {
                            for (int j = 0; j < SpacePoints.Count; j++)
                            {
                                var SubPoly = SpacePoints[j];

                                if (SubPoly.Count == 0)
                                    return;

                                var density = (100 - (tree_SO.TreeDensity - 1.1f)) * container.Scale.y * 0.5f;
                                var RegularPoints = GISTerrainLoaderVectorExtensions.GenerateRegular2DPointsInsidePolygon(SubPoly, density);
                                Treepoints.AddRange(RegularPoints);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < SpacePoints.Count; j++)
                            {
                                var SubPoly = SpacePoints[j];

                                if (SubPoly.Count == 0)
                                    return;
                                var density = (100 - (tree_SO.TreeDensity - 1.1f)) * container.Scale.y;
                                var RandomPoints = GISTerrainLoaderVectorExtensions.GenerateRandomPointInsidePolygon(SubPoly, density);
                                Treepoints.AddRange(RandomPoints);
      
                            }
                        }

                        for (int j = 0; j < Treepoints.Count; j++)
                        {

                            CreateTree(tree_SO, container, Treepoints[j]);

                        }
                    }

                }

            }
        }        
        private static void CreateTree(GISTerrainLoaderSO_Tree tree,GISTerrainContainer TerrainContainer, Vector3 pos)
        {

            float TreeScaleFactor = tree.TreeScaleFactor * TerrainContainer.Scale.y*0.5f;
            float RandomScaleFactor = tree.TreeRandomScaleFactor * TerrainContainer.Scale.y * 0.5f;


            var m_prototypeIndex = GetTreePrototype(tree.m_treeType, Treeprototypes);
 
            for (int x = 0; x < TerrainContainer.TerrainCount.x; x++)
            {
                for (int y = 0; y < TerrainContainer.TerrainCount.y; y++)
                {

                    GISTerrainTile item = TerrainContainer.terrains[x, y];
                    Terrain terrain = item.terrain;
                    terrain.treeBillboardDistance = BillBoardStartDistance;
                    terrain.treeDistance = TreeDistance;
                    TerrainData tData = terrain.terrainData;
                    Vector3 terPos = terrain.transform.position;
                    Vector3 localPos = pos - terPos;
                    float heightmapWidth = (tData.heightmapResolution - 1) * tData.heightmapScale.x;
                    float heightmapHeight = (tData.heightmapResolution - 1) * tData.heightmapScale.z;

                    if (localPos.x > 0 && localPos.z > 0 && localPos.x < heightmapWidth && localPos.z < heightmapHeight)
                    {
                        terrain.AddTreeInstance(new TreeInstance
                        {
                            color = Color.white,
                            heightScale = TreeScaleFactor* Prefs.TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor),
                            lightmapColor = Color.white,
                            position = new Vector3(localPos.x / heightmapWidth, 0, localPos.z / heightmapHeight),
                            prototypeIndex = UnityEngine.Random.Range(m_prototypeIndex.x, m_prototypeIndex.y),
                            widthScale = TreeScaleFactor * Prefs.TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor)
                        });
                        break;
                    }
                }
            }
        }
        public static void AddTreePrefabsToTerrains(GISTerrainContainer m_container, GISTerrainLoaderPrefs m_prefs)
        {
            m_prefs.LoadAllTreePrefabs();

            TreeDistance = m_prefs.TreeDistance;
            BillBoardStartDistance = m_prefs.BillBoardStartDistance;
            container = m_container;

            int c = 0;
            List<object> objects = new List<object>();
            List<string> objects_type = new List<string>();
            Treeprototypes_Ind = new Dictionary<string, int>();

            foreach ( var prefab in m_prefs.TreePrefabs)
            {

                if(prefab!=null)
                {
                    foreach(var t in prefab.TreePrefab)
                    {
                        if (t != null)
                        {
                            objects.Add(t);
                            objects_type.Add(prefab.m_treeType);
                            c++;
                        }
                    }
                    if(!Treeprototypes_Ind.ContainsKey(prefab.m_treeType))
                    Treeprototypes_Ind.Add(prefab.m_treeType, c);
                     c = 0;
                }
            }

            TreePrototype[] prototypes = new TreePrototype[objects.Count];

            Treeprototypes = new List<PrototypeTagIndex>();

            for (int i = 0; i < prototypes.Length; i++)
            {
                prototypes[i] = new TreePrototype
                {
                    prefab = (GameObject)objects[i] as GameObject
                };

                Treeprototypes.Add(new PrototypeTagIndex(prototypes[i], objects_type[i]));

            }

            foreach (var item in container.terrains)
            {
                item.terrainData.treePrototypes = prototypes;
                item.terrainData.treeInstances = new TreeInstance[0];
            }
        }
        public static List<Vector3> GetGlobalPointsFromWay(GISTerrainLoaderOSMWay way, Dictionary<long, GISTerrainLoaderOSMNode> _nodes)
        {
            List<Vector3> points = new List<Vector3>();

            if (way.Nodes.Count == 0) return points;

            foreach (var node in way.Nodes)
            {
                if (node != null)
                    points.Add(new Vector3((float)node.Lon, 0, (float)node.Lat));
            }
            return points;
        }
        private static Vector2Int GetTreePrototype(string treetype, List<PrototypeTagIndex> Treeprototypes)
        {
            Vector2Int Index = new Vector2Int(0, 0);

            var l = Treeprototypes_Ind.ToList();
            int t_value = 0;

            foreach (var tree in l)
            {
                if (tree.Key == treetype)
                {
                    Index = new Vector2Int(t_value, (t_value + tree.Value));
                }
                t_value += tree.Value;
            }
            return Index;
        }
        private static GISTerrainLoaderSO_Tree GetTreePrefab(string treetype)
        {
            GISTerrainLoaderSO_Tree tree = null;
            foreach (var prefab in Prefs.TreePrefabs)
            {
                if (prefab != null)
                {
                    if (prefab.m_treeType == treetype)
                        tree= prefab;

                }
            }
            return tree;
        }
    }
    public class PrototypeTagIndex
    {
        public TreePrototype protoType;
        public string treeType;

        public PrototypeTagIndex(TreePrototype m_protoType, string m_treeType)
        {
            protoType = m_protoType;
            treeType = m_treeType;
        }
        
    }
}