/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GISTech.GISTerrainLoader
{
    [Serializable]
    public class GISTerrainLoaderPrefs
    {
        public GISTerrainLoaderPrefs()
        {
           
        }
        #region Generator
        public ReadingMode readingMode = ReadingMode.Full;
        /// <summary>
        /// Main DEM File Path
        /// </summary>
        public string TerrainFilePath;
        /// <summary>
        /// Conatins All Projection system needed (Editable in edit/play mode) as ScriptablObject
        /// </summary>
        public GISTerrainLoaderRuntimeProjection_SO ProjectionsData_SO;
        /// <summary>
        /// ScriptablObject Contains All GIS Terrain Loader Setting 
        /// </summary>
        public GISTerrainLoaderSettings_SO Settings_SO;
        /// <summary>
        /// ScriptablObject Contains All GIS Terrain Loader Vector Loader Parameters   
        /// </summary>
        public GISTerrainLoaderVectorParameters_SO VectorParameters_SO;

        public bool ShowCoordinates;
        public DVector2 SubRegionUpperLeftCoordiante;
        public DVector2 SubRegionDownRightCoordiante;


        #endregion

        #region Projections 
        public ProjectionMode projectionMode = ProjectionMode.Geographic;
        public DisplayFormat CoordinatesDisplayFormat = DisplayFormat.Decimale;
        public int EPSGCode;

        #endregion

        #region TerrainGenerator

        public TerrainDimensionsMode terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        public bool TerrainHasDimensions;
        public OptionEnabDisab TerrainBaseboards = OptionEnabDisab.Disable;
        public int BorderHigh = 0;
        public OptionEnabDisab TerrainLayerSet = OptionEnabDisab.Disable;

 
        public Vector2Int terrainCount = Vector2Int.one;
        public Vector3 terrainScale = Vector3.one;
        public DVector2 TerrainDimensions;

        public OptionEnabDisab TerrainBackground = OptionEnabDisab.Disable;

        public int TerrainBackgroundHeightmapResolution = 128;
        public int TerrainBackgroundHeightmapResolution_index = 2;
        public int[] TerrainBackgroundHeightmapResolutions = { 33, 65, 129, 257, 513, 1025};
        public string[] TerrainBackgroundHeightmapResolutionsSrt = new string[] { "33", "65", "129", "257", "513", "1025"};
 
        public int TerrainBackgroundTextureResolution = 1024;
        public int TerrainBackgroundTextureResolution_index = 2;
        public int[] TerrainBackgroundTextureResolutions = { 33, 65, 129, 257, 513, 1025 };
        public string[] TerrainBackgroundTextureResolutionsSrt = new string[] { "32", "64", "128", "256", "512", "1024"};


        public int TerrainLayer;
        public int TerrainLayer_index = 2;
        public string[] TerrainLayerSrt = new string[] {};
 
        #endregion

        #region ElevationLoader
        public TerrainElevation TerrainElevation = TerrainElevation.RealWorldElevation;
        public TiffElevationSource tiffElevationSource = TiffElevationSource.DEM;
        public float ElevationScaleValue = 1112.0f;
        public float ScaleFactor = 1000;
        public int BandsIndex;
        public OptionEnabDisab UnderWater = OptionEnabDisab.Disable;
        public FixOption TerrainFixOption = FixOption.Disable;
        public EmptyPoints ElevationForNullPoints = EmptyPoints.Average;
        public float ElevationValueForNullPoints = 0;
        public Vector2 TerrainMaxMinElevation = new Vector2(0, 0);
        public float TerrainExaggeration;
        #endregion

        #region TerrainTextures
        public TextureMode textureMode = TextureMode.WithTexture;
        public int TextureFolderIndex = 0;
        public OptionEnabDisab UseTerrainEmptyColor = OptionEnabDisab.Disable;
        public TexturesLoadingMode textureloadingMode = TexturesLoadingMode.AutoDetection;
        public OptionEnabDisab UnderWaterShader = OptionEnabDisab.Disable;
        public ShaderType TerrainShaderType = ShaderType.ColorRamp;
        public OptionEnabDisab SaveShaderTextures = OptionEnabDisab.Disable;

        [SerializeField]
        public GISTerrainLoaderTerrainLayer BaseTerrainLayers = new GISTerrainLoaderTerrainLayer();
        [SerializeField]
        public List<GISTerrainLoaderTerrainLayer> TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
        public float Slope = 0.1f;
        public int MergeRadius = 1;
        public int MergingFactor = 1;

        #endregion

        #region TerrainDetails
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 1024;
        public int heightmapResolution = 128;

        public int heightmapResolution_index = 2;
        public int detailResolution_index = 5;
        public int resolutionPerPatch_index = 1;
        public int baseMapResolution_index = 5;
        public int[] heightmapResolutions = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        public string[] heightmapResolutionsSrt = new string[] { "33", "65", "129", "257", "513", "1025", "2049", "4097" };


        public int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4097" };

        public int[] availableHeightsResolutionPrePec = { 4, 8, 16, 32 };
        public string[] availableHeightsResolutionPrePectSrt = new string[] { "4", "8", "16", "32" };
 
        public float PixelError;
        public float BaseMapDistance = 2000;
        #endregion

        #region TerrainMaterials
        public TerrainMaterialMode terrainMaterialMode = TerrainMaterialMode.Standard;
        public Material terrainMaterial = null;
        public Material terrainBorderMaterial = null;
        public Color TerrainEmptyColor = Color.white;
        public float ContourInterval = 50;
        #endregion

        #region TerrainSmoothing
        public OptionEnabDisab UseTerrainSurfaceSmoother = OptionEnabDisab.Disable;
        public int TerrainSurfaceSmoothFactor = 4;
        public OptionEnabDisab UseTerrainHeightSmoother = OptionEnabDisab.Disable;
        public float TerrainHeightSmoothFactor = 0.05f;
        #endregion

        #region VectorGenerator

        public VectorType vectorType = VectorType.OpenStreetMap;
 
        public OptionEnabDisab EnableGeoPointGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableRoadGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableTreeGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableGrassGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableBuildingGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableWaterGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableLandParcelGeneration = OptionEnabDisab.Disable;
        

        public float TreeScaleFactor = 2f;
        public float TreeDistance = 4000f;
        public float BillBoardStartDistance = 300;

        public PointDistribution TreeDistribution = PointDistribution.Randomly;

        public float GrassScaleFactor = 1.5f;
        public float DetailDistance = 400;
        public PointDistribution GrassDistribution = PointDistribution.Randomly;

        [SerializeField]
        public List<GISTerrainLoaderSO_GeoPoint> GeoPointPrefabs = new List<GISTerrainLoaderSO_GeoPoint>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Building> BuildingPrefabs = new List<GISTerrainLoaderSO_Building>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Road> RoadPrefabs = new List<GISTerrainLoaderSO_Road>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Tree> TreePrefabs = new List<GISTerrainLoaderSO_Tree>();
        [SerializeField]
        public List<GISTerrainLoaderSO_GrassObject> GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Water> WaterPrefabs = new List<GISTerrainLoaderSO_Water>();
        [SerializeField]
        public List<GISTerrainLoaderSO_LandParcel> LandParcelPrefabs = new List<GISTerrainLoaderSO_LandParcel>();

        public OptionEnabDisab BuildTerrains = OptionEnabDisab.Disable;
        public OptionEnabDisab GenerateBuildingBase = OptionEnabDisab.Disable;

        public RoadGeneratorType RoadGenerator = RoadGeneratorType.Line;
        public bool EnableRoadName;
        public float RoadRaiseOffset = 0.1f;

        public WaterSource WaterDataSource = WaterSource.DefaultPlane;
        public float WaterOffsetY = 2f;

        public VectorElevationMode LandParcelElevationMode = VectorElevationMode.Default2DPlane;
        public float LandParcelOffsetY = 2f;
        public int LandParcelPolygonCount = 2;

        public GameObject GeoPointPrefab;
        public GISTerrainLoaderSO_Road PathPrefab;

        #endregion

#if UNITY_EDITOR

        public int lastTab = 0;
#endif

        //Raw File Parameters
        public RawDepth Raw_Depth = RawDepth.Bit16;
        public RawByteOrder Raw_ByteOrder = RawByteOrder.Windows;

        public OptionEnabDisab RemovePrvTerrain;


        public void ResetPrefs()
        {
            readingMode = ReadingMode.Full;
            ShowCoordinates = false;
            SubRegionDownRightCoordiante = new DVector2(0, 0);
            SubRegionUpperLeftCoordiante = new DVector2(0, 0);
            ////////////////////////////////////////////////////////////////////////////////

            TerrainElevation = TerrainElevation.RealWorldElevation;
            TerrainExaggeration = 0.27f;
            terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
            TerrainDimensions = new DVector2(10, 10);
            terrainScale = new Vector3(1, 1, 1);
            UnderWater = OptionEnabDisab.Disable;

            ////////////////////////////////////////////////////////////////////////////////

            heightmapResolution_index = 2;
            heightmapResolution = heightmapResolutions[heightmapResolution_index];

            detailResolution_index = 4;
            detailResolution = availableHeights[detailResolution_index];

            resolutionPerPatch_index = 1;
            resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];

            baseMapResolution_index = 4;
            baseMapResolution = availableHeights[baseMapResolution_index];

            PixelError = 1;
            BaseMapDistance = 1000;

            terrainMaterialMode = TerrainMaterialMode.Standard;

            ////////////////////////////////////////////////////////////////////////////////

            textureMode = TextureMode.WithTexture;
            TerrainEmptyColor = Color.white;
            UseTerrainEmptyColor = OptionEnabDisab.Disable;

            ////////////////////////////////////////////////////////////////////////////////

            UseTerrainHeightSmoother = OptionEnabDisab.Disable;
            TerrainHeightSmoothFactor = 0.05f;
            UseTerrainSurfaceSmoother = OptionEnabDisab.Disable;
            TerrainSurfaceSmoothFactor = 4;

            ////////////////////////////////////////////////////////////////////////////////

            EnableTreeGeneration = OptionEnabDisab.Disable;
            TreeDistance = 4000;
            BillBoardStartDistance = 300;
            TreePrefabs = new List<GISTerrainLoaderSO_Tree>();

            EnableGrassGeneration = OptionEnabDisab.Disable;
            GrassScaleFactor = 2.5f;
            DetailDistance = 400;
            GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();

            EnableRoadGeneration = OptionEnabDisab.Disable;
            RoadGenerator = RoadGeneratorType.Line;

        }
 
        public bool IsVectorGenerationEnabled(string fileExtension)
        {
            var isGeoFile = Settings_SO.GeoFile.Contains(fileExtension);

            bool val;

            bool vectorEnable = (EnableTreeGeneration == OptionEnabDisab.Enable || EnableGrassGeneration == OptionEnabDisab.Enable || EnableRoadGeneration == OptionEnabDisab.Enable || EnableBuildingGeneration == OptionEnabDisab.Enable || EnableGeoPointGeneration == OptionEnabDisab.Enable || EnableWaterGeneration == OptionEnabDisab.Enable || EnableLandParcelGeneration == OptionEnabDisab.Enable);

            if (isGeoFile && (vectorEnable))
                val = true;
            else
                val = false;


            return val;
        }

        #region Prefabs
        public void LoadAllGrassPrefabs()
        {
            var prefabs = Resources.LoadAll(GISTerrainLoaderConstants.GrassPrefabFolder, typeof(GISTerrainLoaderSO_GrassObject));

            if (prefabs.Length > 0)
            {
                GrassPrefabs.Clear();

                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                        GrassPrefabs.Add(prefab as GISTerrainLoaderSO_GrassObject);
                }

            }
            else
                Debug.Log("Not tree prefabs detected in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass'");
        }
        public void LoadAllTreePrefabs()
        {
            var prefabs = Resources.LoadAll(GISTerrainLoaderConstants.TreesPrefabFolder, typeof(GISTerrainLoaderSO_Tree));

            if (prefabs.Length > 0)
            {
                TreePrefabs.Clear();

                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                        TreePrefabs.Add(prefab as GISTerrainLoaderSO_Tree);
                }

            }
            else
                Debug.Log("Not tree prefabs detected in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees'");
        }
        public void LoadAllRoadPrefabs(RoadGeneratorType roadType)
        {
            var roadsPrefab = Resources.LoadAll(GISTerrainLoaderConstants.RoadsPrefabFolder, typeof(GISTerrainLoaderSO_Road));

            List<GISTerrainLoaderSO_Road> prefabs = new List<GISTerrainLoaderSO_Road>();

            foreach (var road in roadsPrefab)
            {
                var r = (GISTerrainLoaderSO_Road)(road as GISTerrainLoaderSO_Road);
                Material mat = null;

                if (r.MaterialType == MaterialSet.Auto)
                {

                    if (roadType == RoadGeneratorType.EasyRoad3D)
                        mat = Resources.Load("Environment/Roads/Materials/ForEasyRoad3D/" + road.name, typeof(Material)) as Material;

                    if (roadType == RoadGeneratorType.Line)
                    {
                        mat = Resources.Load("Environment/Roads/Materials/StandardLineRender/" + road.name, typeof(Material)) as Material;
                        if (mat) mat.SetColor("_Color", r.RoadColor);
                    }
                    if (roadType == RoadGeneratorType.RoadCreatorPro)
                    {
                        mat = Resources.Load("Environment/Roads/Materials/RoadCreatorPro/" + road.name, typeof(Material)) as Material;
                        if (mat) mat.SetColor("_Color", r.RoadColor);
                    }


                }

                if (r.Roadmaterial == null || mat == null)
                    mat = Resources.Load("Environment/Roads/Materials/Standard", typeof(Material)) as Material;



                r.Roadmaterial = mat;
                prefabs.Add(r);

            }

            RoadPrefabs = prefabs;
        }
        public void LoadAllPointPrefabs()
        {
            var PointsPrefab = Resources.LoadAll(GISTerrainLoaderConstants.GeoPointsPrefabFolder, typeof(GISTerrainLoaderSO_GeoPoint));

            List<GISTerrainLoaderSO_GeoPoint> prefabs = new List<GISTerrainLoaderSO_GeoPoint>();

            foreach (var point in PointsPrefab)
            {
                var r = (GISTerrainLoaderSO_GeoPoint)(point as GISTerrainLoaderSO_GeoPoint);
                prefabs.Add(r);
            }
            GeoPointPrefabs =  prefabs;
        }
        public void LoadAllBuildingPrefabs()
        {
            var buildingPrefab = Resources.LoadAll(GISTerrainLoaderConstants.BuildingPrefabFolder, typeof(GISTerrainLoaderSO_Building));

            List<GISTerrainLoaderSO_Building> prefabs = new List<GISTerrainLoaderSO_Building>();

            foreach (var building in buildingPrefab)
            {
                var r = building as GISTerrainLoaderSO_Building;

                prefabs.Add(r);

            }

            BuildingPrefabs = prefabs;
        }
        public void LoadAllWaterPrefabs()
        {
            var WaterPrefab = Resources.LoadAll(GISTerrainLoaderConstants.WaterPrefabFolder, typeof(GISTerrainLoaderSO_Water));

            List<GISTerrainLoaderSO_Water> prefabs = new List<GISTerrainLoaderSO_Water>();

            foreach (var Water in WaterPrefab)
            {
                var r = Water as GISTerrainLoaderSO_Water;
                prefabs.Add(r);

            }

            WaterPrefabs = prefabs;
        }
        public void LoadAllLandParcelPrefabs()
        {
            var LandParcelPrefab = Resources.LoadAll(GISTerrainLoaderConstants.LandParcelPrefabFolder, typeof(GISTerrainLoaderSO_LandParcel));

            List<GISTerrainLoaderSO_LandParcel> prefabs = new List<GISTerrainLoaderSO_LandParcel>();

            foreach (var landParcel in LandParcelPrefab)
            {
                var r = landParcel as GISTerrainLoaderSO_LandParcel;

                prefabs.Add(r);

            }

            LandParcelPrefabs = prefabs;
        }
        
        #endregion


        #region GISTerrainLoaderScriptableObject

        /// <summary>
        /// Load GIS Terrain Loader Settings Vector-Projection-GeneraleSettings
        /// </summary>
        public void LoadSettings()
        {
            LoadGISTerrainLoaderProjectionsData();
            LoadGISTerrainLoaderSettings();
            LoadGISTerrainLoaderVectorSettings();
        }
        private void LoadGISTerrainLoaderProjectionsData()
        {
            ProjectionsData_SO = (GISTerrainLoaderRuntimeProjection_SO)Resources.Load("Settings/RuntimeProjections", typeof(GISTerrainLoaderRuntimeProjection_SO));
        }
        private void LoadGISTerrainLoaderSettings()
        {
            Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));
        }
        private void LoadGISTerrainLoaderVectorSettings()
        {
            VectorParameters_SO = GISTerrainLoaderVectorParameters_SO.LoadParameters();
 
            if (!VectorParameters_SO)
                Debug.LogError("Vector Parameters ScriptableObject File Not found ...");
        }
        #endregion
    }
}