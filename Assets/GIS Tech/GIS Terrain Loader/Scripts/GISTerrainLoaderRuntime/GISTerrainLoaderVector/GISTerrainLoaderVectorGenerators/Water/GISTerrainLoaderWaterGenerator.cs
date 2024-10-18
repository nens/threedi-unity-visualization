/*     Unity GIS Tech 2020-2023      */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderWaterGenerator : GISTerrainLoaderVectorGenerator
    {
        public override void Generate(GISTerrainLoaderPrefs m_prefs, GISTerrainLoaderGeoVectorData m_GeoData, GISTerrainContainer m_container)
        {
            Prefs = m_prefs;
            container = m_container;
            GeoData = m_GeoData;
 
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            GameObject WaterContainer = new GameObject();
            WaterContainer.name = "Water";
            WaterContainer.transform.parent = container.transform;

            if (Prefs.WaterDataSource == WaterSource.DefaultPlane)
            {
                GeneratePlane(Prefs, container, WaterContainer.transform);
            }
            else
            {
                if (GeoData.GeoPolygons.Count == 0)
                    return;
 
                for (int p = 0; p < GeoData.GeoPolygons.Count; p++)
                {
                    var Poly = GeoData.GeoPolygons[p];

                    var WaterData = new GISTerrainLoaderWaterData(Prefs, Poly, container);

                    if (WaterData.water_SO != null)
                    {
                        if (WaterData.SpacePoints.Count > 0)
                        {

                            var meshData = new GISTerrainLoaderMeshData();
                            meshData.GetCentrePoint(WaterData.SpacePoints, WaterData.MinMaxHeight);

                            GISTerrainLoaderVectorEntity VEWater = new GISTerrainLoaderVectorEntity(Poly.ID + "_" + Poly.Tag, "WaterMesh");

                            GISTerrainLoaderPolygonMeshGenerator MeshGenerator = new GISTerrainLoaderPolygonMeshGenerator(WaterData.SpacePoints, meshData);
                            MeshGenerator.Run();

                            var Yoffset = 1.5f * container.Scale.y;
                            var UVScale = 1f * container.Scale.y; 
                            VEWater.Apply(meshData, WaterContainer.transform, Yoffset, UVScale);

                            List<Material> Materials = new List<Material>();
                            Material WaterMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultWaterMaterial, typeof(Material)) as Material;
                            Materials.Add(new Material(WaterMaterial));

                            VEWater.SetMaterial(Materials);

                            if (Prefs.VectorParameters_SO.AddVectorDataBaseToGameObjects)
                                GISTerrainLoaderGeoVectorData.AddVectordDataToObject(VEWater.GameObject, VectorObjectType.Polygon, null, null, Poly);

                        }

                    }
                }
            }
  
        }
        private void GeneratePlane(GISTerrainLoaderPrefs m_prefs,GISTerrainContainer m_container,Transform parent)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            
            plane.transform.parent = parent;
            plane.transform.position += new Vector3(m_container.ContainerSize.x / 2, m_prefs.WaterOffsetY, m_container.ContainerSize.z / 2);
            plane.transform.localScale = m_container.ContainerSize/10;
            plane.transform.Rotate(new Vector3(0, 1, 0), 180);

            Material WaterMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultWaterMaterial, typeof(Material)) as Material;

            float[,] data = m_container.data.GetNormlizedHeightmap(m_prefs.heightmapResolution, m_container.terrains);
            var FoamText = GISTerrainLoaderTerrainShader.GetShadedTexture(m_prefs, m_container, data, ShadedTextureType.Foam);
            WaterMaterial.SetTexture("_FoamDistanceMap", FoamText);
            var waterText = GISTerrainLoaderTerrainShader.GetShadedTexture(m_prefs, m_container, data, ShadedTextureType.Water);
            WaterMaterial.SetTexture("_WaterTexture", waterText);
            WaterMaterial.SetFloat("_TerrainScale", m_container.Scale.y);
            plane.GetComponent<MeshRenderer>().material = new Material(WaterMaterial) ;
 
 
        }

    }
    public class GISTerrainLoaderWaterData
    {
        public float width = 5;
        public float CEMT = 5;
        public string draft = "";
        public string tidal = "";
        public float maxwidth = 5;
        public float maxheight = 0;
        public float maxlength = 1;
        public float maxspeed = 1;
 
        public GISTerrainLoaderSO_Water water_SO;
        public GISTerrainLoaderPolygonGeoData PolyData;

        public List<List<Vector3>> SpacePoints = new List<List<Vector3>>();

        public Vector2 MinMaxHeight = new Vector2();

        public GISTerrainLoaderWaterData(GISTerrainLoaderPrefs Prefs, GISTerrainLoaderPolygonGeoData m_PolyData, GISTerrainContainer container)
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
            SpacePoints = GetPolyPoints(container, PolyData);
            MinMaxHeight = PolyData.GetMinMaxHeight(SpacePoints);
            water_SO = GetWaterPrefab(Prefs.WaterPrefabs, PolyData.Tag);
 
        }
        private static GISTerrainLoaderSO_Water GetWaterPrefab(List<GISTerrainLoaderSO_Water> WaterPrefabs, string waterTag)
        {
            GISTerrainLoaderSO_Water Water = null;

            foreach (var prefab in WaterPrefabs)
            {
                if (prefab != null && !string.IsNullOrEmpty(prefab.WaterTag) && !string.IsNullOrEmpty(waterTag))
                {
                    if (prefab.WaterTag.Equals(waterTag))
                        Water = prefab;
                }
            }

            if (Water == null)
            {
                Water = Resources.Load(GISTerrainLoaderConstants.DefaultWaterSO, typeof(GISTerrainLoaderSO_Water)) as GISTerrainLoaderSO_Water;
                Water.LoadDefaultValues();
            }
            return Water;
        }
        public List<List<Vector3>> GetPolyPoints(GISTerrainContainer container, GISTerrainLoaderPolygonGeoData Poly, float ElevationOffset = 0f)
        {
            int Counter = Poly.GeoPoints.Count;

            List<List<Vector3>> points = new List<List<Vector3>>(Counter);

            for (int i = 0; i < Counter; i++)
            {
                var sublist = Poly.GeoPoints[i];

                List<Vector3> SubSpace = new List<Vector3>();

                for (int s = 0; s < sublist.Count; s++)
                {
                    var PointA_LatLon = new DVector2(sublist[s].GeoPoint.x, sublist[s].GeoPoint.y);
                    var pointA = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, PointA_LatLon);
                    pointA.y = pointA.y - ElevationOffset;
                    SubSpace.Add(pointA);
                }

                bool ClockWise = Poly.IsClockwise(SubSpace);

                if (Poly.Roles[i] == Role.Outer)
                {
                    if (!ClockWise)
                        SubSpace = Poly.OrderToClockwise(SubSpace.ToArray()).ToList();
                }
                else
                {
                    if (ClockWise)
                        SubSpace = Poly.CounterOrderToClockwise(SubSpace.ToArray()).ToList();
                }

                points.Add(SubSpace);
            }
            return points;

        }
        public List<List<Vector3>> GetPolySpaceBasePoints(GISTerrainContainer container, GISTerrainLoaderPolygonGeoData Poly)
        {
            var buildingpoints = Poly.GeoPoints;

            int Counter = Poly.GeoPoints.Count;

            List<List<Vector3>> SpacePoints = new List<List<Vector3>>(Counter);

            float SpaceMaxElevation = float.MinValue;

            for (int i = 0; i < Counter; i++)
            {
                var sublist = buildingpoints[i];

                List<Vector3> SubSpace = new List<Vector3>();

                for (int s = 0; s < sublist.Count; s++)
                {

                    var PointA_LatLon = new DVector2(sublist[s].GeoPoint.x, sublist[s].GeoPoint.y);

                    if (!container.IncludeRealWorldPoint(PointA_LatLon))
                        return new List<List<Vector3>>(Counter);

                    var point = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, PointA_LatLon);

                    if (point.y > SpaceMaxElevation)
                        SpaceMaxElevation = point.y;
                    SubSpace.Add(point);

                }

                bool ClockWise = Poly.IsClockwise(SubSpace);

                if (Poly.Roles[i] == Role.Outer)
                {
                    if (!ClockWise)
                        SubSpace = Poly.OrderToClockwise(SubSpace.ToArray()).ToList();
                }
                else
                {
                    if (ClockWise)
                        SubSpace = Poly.CounterOrderToClockwise(SubSpace.ToArray()).ToList();
                }

                SpacePoints.Add(SubSpace);
            }

            MinMaxHeight = Poly.GetMinMaxHeight(SpacePoints);

            return SpacePoints;

        }
 
    }
 
}