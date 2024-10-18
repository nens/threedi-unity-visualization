/*     Unity GIS Tech 2020-2023      */

using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderBuildingGenerator
    {
        private static GISTerrainContainer container;
        private static GISTerrainLoaderPrefs Prefs;
        public static void GenerateBuildings(GISTerrainContainer m_container, List<GISTerrainLoaderPolygonGeoData> GeoData, GISTerrainLoaderPrefs prefs)
        {
            Prefs = prefs;

            if (GeoData.Count == 0)
                return;

            container = m_container;
 
            GameObject buildingContainer = new GameObject();
            buildingContainer.name = "buildings";
            buildingContainer.transform.parent = container.transform;

            for (int p = 0; p < GeoData.Count; p++)
            {
                var Poly = GeoData[p];
 
                var buildingData = new GISTerrainLoaderBuildingData(container, Poly, Prefs.BuildingPrefabs, Prefs.VectorParameters_SO, Prefs.GenerateBuildingBase);

                if (buildingData.building_SO != null)
                {
                    if(buildingData.SpacePoints.Count > 0)
                    {
                        var meshData = new GISTerrainLoaderMeshData(Prefs.GenerateBuildingBase);
                        meshData.GetCentrePoint(buildingData.SpacePoints, buildingData.MinMaxHeight);

                        GISTerrainLoaderVectorEntity VEBuilding = new GISTerrainLoaderVectorEntity(Poly.ID + "_" + Poly.Tag, "BuildingMesh");

                        GISTerrainLoaderPolygonMeshGenerator BuildingGenerator = new GISTerrainLoaderPolygonMeshGenerator(buildingData.SpacePoints,meshData,buildingData.building_SO.TextureRect);
                        BuildingGenerator.Run();

                        GISTerrainLoaderWallGenerator WallGenerator = new GISTerrainLoaderWallGenerator(buildingData, container, VEBuilding);
                        WallGenerator.Run(meshData, container);

                        VEBuilding.Apply(meshData, buildingContainer.transform);

                        List<Material> Materials = new List<Material>();
                        Material RoofMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultRoofMaterial, typeof(Material)) as Material;
                        Material WallMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultWallMaterial, typeof(Material)) as Material;
                        RoofMaterial.SetTexture("_MainTex", buildingData.building_SO.RoofTexture);
                        WallMaterial.SetTexture("_MainTex", buildingData.building_SO.WallTexture);
                        Materials.Add(new Material(RoofMaterial));
                        Materials.Add(new Material(WallMaterial));

                        VEBuilding.SetMaterial(Materials);

                        if (Prefs.GenerateBuildingBase == OptionEnabDisab.Enable)
                        {
                            var BasementMeshData = new GISTerrainLoaderMeshData(Prefs.GenerateBuildingBase);
                            BasementMeshData.CentrePosition = meshData.CentrePosition;

                            GISTerrainLoaderVectorEntity VEBasement = new GISTerrainLoaderVectorEntity(Poly.ID + "*_*" + Poly.Tag, "BasementMesh");
 
                            GISTerrainLoaderPolygonMeshGenerator BaseGenerator = new GISTerrainLoaderPolygonMeshGenerator(buildingData.SpacePoints, BasementMeshData, WallGenerator.height);
                            BaseGenerator.Run();

                            var WallGenerator2 = new GISTerrainLoaderWallGenerator(buildingData, container, VEBasement, (buildingData.MinMaxHeight.y - buildingData.MinMaxHeight.x));
                            WallGenerator2.Run(BasementMeshData, container);

                            VEBasement.Apply(BasementMeshData, VEBuilding.Transform);

                            Materials = new List<Material>();
                            Material BasementMaterial = Resources.Load(GISTerrainLoaderConstants.DefaultBasementMaterial, typeof(Material)) as Material;
                            BasementMaterial.SetTexture("_MainTex", buildingData.building_SO.BasementTexture);
                            Materials.Add(new Material(BasementMaterial)); Materials.Add(new Material(BasementMaterial));

                            VEBasement.SetMaterial(Materials);
                        }

                        if (Prefs.VectorParameters_SO.AddVectorDataBaseToGameObjects)
                            GISTerrainLoaderGeoVectorData.AddVectordDataToObject(VEBuilding.GameObject, VectorObjectType.Polygon, null, null, Poly);

                    }

                }
            }

        }

    }

}