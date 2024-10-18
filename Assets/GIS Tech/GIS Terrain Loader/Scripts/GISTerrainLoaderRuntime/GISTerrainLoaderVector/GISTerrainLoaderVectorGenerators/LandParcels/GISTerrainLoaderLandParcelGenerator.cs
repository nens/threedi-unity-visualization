/*     Unity GIS Tech 2020-2024      */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderLandParcelGenerator : GISTerrainLoaderVectorGenerator
    {
        public override void Generate(GISTerrainLoaderPrefs m_prefs, GISTerrainLoaderGeoVectorData m_GeoData, GISTerrainContainer m_container)
        {
            Prefs = m_prefs;
            container = m_container;
            GeoData = m_GeoData;

            GameObject LandParcelContainer = new GameObject();
            LandParcelContainer.name = "LandParcel";
            LandParcelContainer.transform.parent = container.transform;

            GeneratePlane(Prefs, container, LandParcelContainer.transform);
 
        }
        private void GeneratePlane(GISTerrainLoaderPrefs m_prefs, GISTerrainContainer m_container, Transform parent)
        {
            if (GeoData.GeoPolygons.Count == 0)
                return;

            for (int p = 0; p < GeoData.GeoPolygons.Count; p++)
            {
                var PolyTag = GeoData.GeoPolygons[p].Tag;

                var LandParcel_SO = GetPrefab(m_prefs, PolyTag);

                var Poly = GeoData.GeoPolygons[p];

                var LandParcelData = new GISTerrainLoaderLandParcelData(Prefs, Poly, container,true);

                if (LandParcelData.LandParcel_SO != null)
                {
                    if (LandParcelData.SpacePoints.Count > 0)
                    {
                        var meshData = new GISTerrainLoaderMeshData();
                        meshData.GetCentrePoint(LandParcelData.SpacePoints, LandParcelData.MinMaxHeight);

                        GISTerrainLoaderVectorEntity VE = new GISTerrainLoaderVectorEntity(Poly.ID + "_" + Poly.Tag, "Mesh");

                        GISTerrainLoaderPolygonMeshGenerator MeshGenerator = new GISTerrainLoaderPolygonMeshGenerator(LandParcelData.SpacePoints, meshData);
                        MeshGenerator.Run();
 
                        var Yoffset = m_prefs.LandParcelOffsetY*1.5f * container.Scale.y;
                        var UVScale = 1f * container.Scale.y;
                        VE.Apply(meshData, m_container.transform, Yoffset, UVScale);

                        if (Prefs.LandParcelElevationMode == VectorElevationMode.AdaptedToTerrainElevation)
                        {
                            int[] subdivision = new int[] { 0, 2, 3, 4, 6, 8, 9, 12, 16, 18, 24 };
 
                            var mesh = VE.MeshFilter.sharedMesh;

                            GISTerrainLoaderMeshHelper.Subdivide(mesh, subdivision[Prefs.LandParcelPolygonCount]);

                            VE.MeshFilter.mesh = mesh;

                            Vector3[] newVertices = new Vector3[mesh.vertices.Length];

                            for (int i = 0; i < mesh.vertices.Length; i++)
                            {
                                var vertice = mesh.vertices[i];
                                var pos = vertice;
                                var ele = GISTerrainLoaderGeoConversion.GetHeight(pos + VE.GameObject.transform.position);
                                pos = new Vector3(vertice.x, ele + Prefs.LandParcelOffsetY, vertice.z);
                                newVertices[i] = pos;
                            }
                            VE.Mesh.vertices = newVertices;
                            mesh.vertices = newVertices;
                        }
                        VE.Transform.parent = parent;

                        List<Material> Materials = new List<Material>();

                        Material LPMaterial = LandParcel_SO.material;

                        if (LandParcel_SO.material == null)
                            LPMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultLandParcelMaterial, typeof(Material)) as Material;

                        Materials.Add(new Material(LPMaterial));

                        VE.SetMaterial(Materials);

                        if (Prefs.VectorParameters_SO.AddVectorDataBaseToGameObjects)
                            GISTerrainLoaderGeoVectorData.AddVectordDataToObject(VE.GameObject, VectorObjectType.Polygon, null, null, Poly);

                    }

                }
            }
        }
        private static GISTerrainLoaderSO_LandParcel GetPrefab(GISTerrainLoaderPrefs m_prefs, string landParcelTag)
        {
            GISTerrainLoaderSO_LandParcel landParcel = null;

            foreach (var prefab in m_prefs.LandParcelPrefabs)
            {
                if (prefab != null)
                {
                    if (prefab.Tag == landParcelTag)
                        landParcel = prefab;
                }
            }
            return landParcel;
        }
    }
    public class GISTerrainLoaderLandParcelData
    {
        public float width = 5;
        public float CEMT = 5;
        public string draft = "";
        public string tidal = "";
        public float maxwidth = 5;
        public float maxheight = 0;
        public float maxlength = 1;
        public float maxspeed = 1;

        public GISTerrainLoaderSO_LandParcel LandParcel_SO;
        public GISTerrainLoaderPolygonGeoData PolyData;

        public List<List<Vector3>> SpacePoints = new List<List<Vector3>>();

        public Vector2 MinMaxHeight = new Vector2();

        public GISTerrainLoaderLandParcelData(GISTerrainLoaderPrefs Prefs, GISTerrainLoaderPolygonGeoData m_PolyData, GISTerrainContainer container, bool complexPoly=false)
        {
            PolyData = m_PolyData;

            List<Role> Rols = new List<Role>();

            SpacePoints = new List<List<Vector3>>();

            List<DVector2> TerrainBounds = container.GetBoundsCoordinatesAsPointGeoData().ToDVector2List();

            List<List<DVector2>> NewPolyIntersection = new List<List<DVector2>>();

            for (int i = 0; i < PolyData.GeoPoints.Count; i++)
            {
                var SubPolyPoints = PolyData.GeoPoints[i].ToDVector2List();
                Rols.Add(PolyData.Roles[i]);

                var PolyIntersx = GISTerrainLoaderPolygonMathUtility.ClipPolygons(TerrainBounds, SubPolyPoints, BooleanOperation.Intersection);

                if (PolyIntersx.Count == 0 && GISTerrainLoaderPolygonMathUtility.IsPolygonInsidePolygon(SubPolyPoints, TerrainBounds))
                {

                    NewPolyIntersection.Add(SubPolyPoints);
                }
                if (PolyIntersx.Count > 0)
                {
                    NewPolyIntersection.Add(PolyIntersx[0]);

                }


            }

            PolyData.GeoPoints = NewPolyIntersection.ToMultiGeoPointList();
            PolyData.Roles = Rols;

            SpacePoints = GetPolyPoints(container, PolyData, 0, PointDistribution.uniformly, 100);
 
            MinMaxHeight = PolyData.GetMinMaxHeight(SpacePoints);
            LandParcel_SO = GetLandParcelPrefab(Prefs.LandParcelPrefabs, PolyData.Tag);

        }
        private static GISTerrainLoaderSO_LandParcel GetLandParcelPrefab(List<GISTerrainLoaderSO_LandParcel> WaterPrefabs, string waterTag)
        {
            GISTerrainLoaderSO_LandParcel LandParcel = null;

            foreach (var prefab in WaterPrefabs)
            {
                if (prefab != null && !string.IsNullOrEmpty(prefab.Tag) && !string.IsNullOrEmpty(waterTag))
                {
                    if (prefab.Tag.Equals(waterTag))
                        LandParcel = prefab;
                }
            }

            if (LandParcel == null)
            {
                LandParcel = Resources.Load(GISTerrainLoaderConstants.DefaultLandParcelSO, typeof(GISTerrainLoaderSO_LandParcel)) as GISTerrainLoaderSO_LandParcel;
                LandParcel.LoadDefaultValues();
            }
            return LandParcel;
        }
 
        public List<List<Vector3>> GetPolyPoints(GISTerrainContainer container, GISTerrainLoaderPolygonGeoData Poly, float ElevationOffset, PointDistribution PointDistribution, float threshold)
        {
            int Counter = Poly.GeoPoints.Count;

            List<List<Vector3>> points = new List<List<Vector3>>(Counter);

            var SpacePoints = Poly.GeoPointsToSpacePoints(container);

            for (int i = 0; i < SpacePoints.Count; i++)
            {
                var SubPoly =  SpacePoints[i];

                List<Vector3> Newpoints = new List<Vector3>();

                if (SubPoly.Count > 0)
                    Newpoints = GISTerrainLoaderVectorExtensions.GenerateRegular3DPointsInsidePolygon(container, SubPoly, 500);
  
                bool ClockWise = Poly.IsClockwise(Newpoints);

                if (Poly.Roles[i] == Role.Outer)
                {
                    if (!ClockWise)
                        Newpoints = Poly.OrderToClockwise(Newpoints.ToArray()).ToList();
                }
                else
                {
                    if (ClockWise)
                        Newpoints = Poly.CounterOrderToClockwise(Newpoints.ToArray()).ToList();
                }
 
                points.Add(Newpoints);
            }
            return points;

        }

    }

}