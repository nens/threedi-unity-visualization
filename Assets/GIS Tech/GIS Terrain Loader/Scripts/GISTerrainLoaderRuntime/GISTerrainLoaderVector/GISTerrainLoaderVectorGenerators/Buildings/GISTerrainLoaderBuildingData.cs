/*     Unity GIS Tech 2020-2023     */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
   public class GISTerrainLoaderBuildingData
    {
        public float Scale = 5;
        public float levels = 1;
        public float Minlevel = 1;
        public float height = 0;
        public float MinHeight = 0;
        public float MaxHeight = 0;

        public GISTerrainLoaderSO_Building building_SO;
        public GISTerrainLoaderPolygonGeoData PolyData;
 
        public List<List<Vector3>> SpacePoints = new List<List<Vector3>>();

        public Vector2 MinMaxHeight = new Vector2();

        public GISTerrainLoaderBuildingData(GISTerrainContainer container, GISTerrainLoaderPolygonGeoData m_PolyData, List<GISTerrainLoaderSO_Building> buildingPrefabs, GISTerrainLoaderVectorParameters_SO VectorParameters, OptionEnabDisab GenerateBuildingBase)
        {
            Scale = VectorParameters.DefaultHeightScale * container.Scale.y;
            PolyData = m_PolyData;
            SpacePoints = new List<List<Vector3>>();
 

            var Value = "";
 
            if (PolyData.TryGetValue(VectorParameters.building_Levels_Tga, out Value))
            {
                if (!string.IsNullOrEmpty(Value)) levels = (float)(GISTerrainLoaderExtensions.GetDouble(Value));
            }


            Value = "";
            if (PolyData.TryGetValue(VectorParameters.building_Height_Tga, out Value))
            {
                if (!string.IsNullOrEmpty(Value)) height = (float)(GISTerrainLoaderExtensions.GetDouble(Value));
            }

            Value = "";
            if (PolyData.TryGetValue(VectorParameters.building_MinHeight_Tga, out Value))
            {
                if (!string.IsNullOrEmpty(Value)) MinHeight = (float)(GISTerrainLoaderExtensions.GetDouble(Value));
            }

            Value = "";
            if (PolyData.TryGetValue(VectorParameters.building_MaxHeight_Tga, out Value))
            {
                if (!string.IsNullOrEmpty(Value)) MaxHeight = (float)(GISTerrainLoaderExtensions.GetDouble(Value));
            }

            Value = "";
            if (PolyData.TryGetValue(VectorParameters.building_MinLevel_Tga, out Value))
            {
                if (!string.IsNullOrEmpty(Value)) Minlevel = (float)(GISTerrainLoaderExtensions.GetDouble(Value));
            }

            if (GenerateBuildingBase == OptionEnabDisab.Disable)
                SpacePoints = GetPolyPoints(container, PolyData);
            else
                SpacePoints = GetPolySpaceBasePoints(container, PolyData);


            building_SO = GetBuildingPrefab(buildingPrefabs, PolyData.Tag);

            if (building_SO)
            {
                if (levels == 0) levels = 1;
                if (building_SO.Defaultheight == 0) building_SO.Defaultheight = 3 * Scale;
                if (height == 0 && MaxHeight == 0) height = MaxHeight = building_SO.Defaultheight * levels * Scale;
                if (height == 0 && MaxHeight != 0) height = MaxHeight * Scale;
                if (height != 0 && MaxHeight == 0) MaxHeight = height * Scale;
                if (MaxHeight == 0) MaxHeight = height * Scale;

                building_SO.CalculateParameters();
            }
            else
            {
                levels = 1;
                height = 3 * Scale;
                MaxHeight = 3 * Scale;
             }

  
        }
        private static GISTerrainLoaderSO_Building GetBuildingPrefab(List<GISTerrainLoaderSO_Building> buildingPrefabs, string buildingTag)
        {
            GISTerrainLoaderSO_Building building = null;

            foreach (var prefab in buildingPrefabs)
            {
                if (prefab != null && !string.IsNullOrEmpty(prefab.buildingTag) && !string.IsNullOrEmpty(buildingTag))
                {
                    if (prefab.buildingTag.Equals(buildingTag))
                        building = prefab;
                }
            }

            if (building == null)
            {
                building = Resources.Load(GISTerrainLoaderConstants.DefaultBuildingSO, typeof(GISTerrainLoaderSO_Building)) as GISTerrainLoaderSO_Building;
                building.LoadDefaultValues();
            }
            return building;
        }

        public List<List<Vector3>> GetPolyPoints(GISTerrainContainer container, GISTerrainLoaderPolygonGeoData Poly, float ElevationOffset = 0f)
        {
            var buildingpoints = Poly.GeoPoints;

            int Counter = buildingpoints.Count;

            List<List<Vector3>> points = new List<List<Vector3>>(Counter);

            for (int i = 0; i < Counter; i++)
            {
                var sublist = buildingpoints[i];
 
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
                var sublist =  buildingpoints[i];
 
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