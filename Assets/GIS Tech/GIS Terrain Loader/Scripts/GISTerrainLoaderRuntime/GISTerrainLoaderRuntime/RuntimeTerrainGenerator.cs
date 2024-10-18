/*     Unity GIS Tech 2020-2024      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

 
namespace GISTech.GISTerrainLoader
{
    
    public class RuntimeTerrainGenerator : GISTerrainLoaderMonoSingleton<RuntimeTerrainGenerator>
    {
        public static event TerrainProgression OnProgress;

        public static event RuntimeTerrainGeneratorEvents OnFinish;

        private GISTerrainLoaderElevationInfo ElevationInfo;

        [HideInInspector]
        public GISTerrainLoaderPrefs Prefs;
 
        public GISTerrainTile[,] terrains;

        private List<GISTerrainTile> ListTerrainObjects;

        [HideInInspector]
        public GISTerrainContainer GeneratedContainer;
 

        private string LoadedFileExtension = "";

        private GeneratorState Generatorstate = GeneratorState.idle;

        private GISTerrainLoaderWebData WebData = new GISTerrainLoaderWebData();
 

        void OnEnable()
        {
            GISTerrainLoaderFloatReader.OnReadError += OnError;
            GISTerrainLoaderTIFFLoader.OnReadError += OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError += OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError += OnError;
            GISTerrainLoaderRawLoader.OnReadError += OnError;
            GISTerrainLoaderASCILoader.OnReadError += OnError;
            GISTerrainLoaderBILReader.OnReadError += OnError;
            GISTerrainLoaderHGTLoader.OnReadError += OnError;
#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError += OnError;
#endif
 
        }

        public IEnumerator StartGenerating(GISTerrainLoaderPrefs prefs)
        {
            Prefs = prefs;

            Prefs.LoadSettings();

            if (string.IsNullOrEmpty(Prefs.TerrainFilePath))
            {
                OnError("Prefs Terrain File Path is Is Null Or Empty ...");
                yield break;
            }

            if (Generatorstate != GeneratorState.Generating)
            {
                Generatorstate = GeneratorState.Generating;
 
                yield return Try(FreeUpMemory());
                yield return Try(CheckFileConfig());
                yield return Try(LoadElevationFile(Prefs.TerrainFilePath));
                yield return Try(GenerateContainer());
                yield return Try(GenerateHeightmap());
                yield return Try(RepareTerrains());
                yield return Try(GenerateTextures());
                yield return Try(GenerateVectorData());
                yield return Try(GenerateBackground());
                yield return Try(Finish());
            }
        }
        private IEnumerator FreeUpMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Resources.UnloadUnusedAssets();
            yield return null;
        }
        private IEnumerator CheckFileConfig()
        {
            var WebDataPath = GISTerrainLoaderPlatformHelper.GetGISWebDataPath(Application.platform, Prefs.TerrainFilePath);

            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                WebData = new GISTerrainLoaderWebData();

                yield return Try(GISTerrainLoaderPlatformHelper.LoadFileWebData(WebDataPath, (data) =>
                {
                    WebData = data;

                    Prefs.terrainCount = WebData.Tiles_count;

                }));
             }
            else
            {
                if (Prefs.Settings_SO.IsValidTerrainFile(Path.GetExtension(Prefs.TerrainFilePath)))
                {
                    CheckTerrainTextures();
                }
                else
                {
                    OnError("Can't Load this File or not exist..");
                }
                yield return null;
            }

        }
        private IEnumerator LoadElevationFile(string filepath)
        {
            LoadedFileExtension = Path.GetExtension(filepath);

            ElevationInfo = null;

            if (LoadedFileExtension == "tiff") LoadedFileExtension = "tif";
 
            switch (LoadedFileExtension)
            {
                case ".tif":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var TiffReader = new GISTerrainLoaderTIFFLoader(Prefs);
 
                        byte[] WebData = new byte[0];

                        if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            yield return Try(GISTerrainLoaderPlatformHelper.LoadFileBytes(filepath, (data) =>
                            {
                                WebData = data;
                            }));

                            TiffReader.WebData = WebData;
                        }
 
                        TiffReader.LoadFile();
 
                        yield return new WaitUntil(() => TiffReader.LoadComplet == true);

                        ElevationInfo.GetData(TiffReader.data);

                        CheckForDimensionAndTiles(true);
                    }
                    break;
                case ".las":
                    {
#if GISTerrainLoaderPdal

                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var lasReader = new GISTerrainLoaderLASLoader();

                        if (!lasReader.LoadComplet)
                            lasReader.LoadLasFile(filepath);

                        ElevationInfo.GetData(lasReader.data);

                        yield return new WaitUntil(() => lasReader.LoadComplet == true);

                        yield return new WaitUntil(() => File.Exists(filepath) == true);

                        if (File.Exists(filepath))
                        {
                            Prefs.TerrainFilePath = lasReader.GeneratedFilePath;
                            yield return new WaitForSeconds(1f);

                            var TiffReader = new GISTerrainLoaderTIFFLoader(Prefs);
 
                                TiffReader.LoadFile(ElevationInfo.data);
  
                            yield return new WaitUntil(() => TiffReader.LoadComplet == true);

                            ElevationInfo.GetData(TiffReader.data);
                            CheckForDimensionAndTiles(true);
                            Generatorstate = GeneratorState.idle;
                            lasReader.LoadComplet = false;
                        }
                        else
                            Debug.LogError("File Not exsiting " + filepath);
#endif
                    }
                    break;
                case ".flt":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var floatReader = new GISTerrainLoaderFloatReader(Prefs);
                        floatReader.LoadFile(Prefs.TerrainFilePath);
                        yield return new WaitUntil(() => floatReader.LoadComplet == true);

                        ElevationInfo.GetData(floatReader.data);

                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".bin":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var binReader = new GISTerrainLoaderBinLoader(Prefs);
                        binReader.LoadFile(Prefs.TerrainFilePath);

                        yield return new WaitUntil(() => binReader.LoadComplet == true);

                        ElevationInfo.GetData(binReader.data);

                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".bil":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();
 
                        var BILReader = new GISTerrainLoaderBILReader(Prefs);
                        BILReader.LoadFile(Prefs.TerrainFilePath);

                        yield return new WaitUntil(() => BILReader.LoadComplet == true);
                        ElevationInfo.GetData(BILReader.data);
                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".asc":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var ASCIReader = new GISTerrainLoaderASCILoader(Prefs);
                        ASCIReader.LoadFile(Prefs.TerrainFilePath);

                        yield return new WaitUntil(() => ASCIReader.LoadComplet == true);

                        ElevationInfo.GetData(ASCIReader.data);
                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".hgt":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var HGTReader = new GISTerrainLoaderHGTLoader(Prefs);
                        HGTReader.LoadFile(Prefs.TerrainFilePath);

                        yield return new WaitUntil(() => HGTReader.LoadComplet == true);
                        ElevationInfo.GetData(HGTReader.data);
                        CheckForDimensionAndTiles(true);


                    }
                    break;
                case ".raw":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var RawReader = new GISTerrainLoaderRawLoader(Prefs);
                        RawReader.LoadFile(Prefs.TerrainFilePath);
 
                        yield return new WaitUntil(() => RawReader.LoadComplet == true);
                        ElevationInfo.GetData(RawReader.data);
                        CheckForDimensionAndTiles(false);

                    }
                    break;
                case ".png":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var PngReader = new GISTerrainLoaderDEMPngLoader(Prefs);

                        byte[] WebData = new byte[0];

                        if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            yield return Try(GISTerrainLoaderPlatformHelper.LoadFileBytes(filepath, (data) =>
                            {
                                WebData = data;
                            }));

                            PngReader.WebData = WebData;

                        }
 
                        PngReader.LoadFile(Prefs.TerrainFilePath);

                        yield return new WaitUntil(() => PngReader.LoadComplet == true);

                        ElevationInfo.GetData(PngReader.data);

                        CheckForDimensionAndTiles(false);


                    }
                    break;
                case ".ter":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var TerReader = new GISTerrainLoaderTerraGenLoade(Prefs);
                        TerReader.LoadFile(filepath);

                        yield return new WaitUntil(() => TerReader.LoadComplet == true);

                        ElevationInfo.GetData(TerReader.data);
                        CheckForDimensionAndTiles(false);
                    }
                    break;
            }
        }
        private IEnumerator GenerateContainer()
        {
            if (ElevationInfo == null || ElevationInfo.data == null)
            {
                StopAllCoroutines();
                OnError(" DEM not loaded correctly .. !");
                yield break;
            }

            if(Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection && (ElevationInfo.data.Dimensions.x == 0 || ElevationInfo.data.Dimensions.y == 0))
            {
                StopAllCoroutines();
                OnError("Terrain Dimension is null ...");
                yield break;
            }


            ListTerrainObjects = new List<GISTerrainTile>();

            string containerName = "Terrains";

            //Destroy prv created terrain
            if (Prefs.RemovePrvTerrain == OptionEnabDisab.Enable)
            {
                Destroy(GameObject.Find(containerName));
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                int index = 1;
                while (GameObject.Find(containerName) != null)
                {
                    containerName = containerName + " " + index.ToString();
                    index++;
                }
            }

            var container = new GameObject(containerName);

            container.transform.position = new Vector3(0, 0, 0);

            Vector2Int tCount = new Vector2Int((int)Prefs.terrainCount.x, (int)Prefs.terrainCount.y);

            float maxElevation = ElevationInfo.data.MinMaxElevation.y;
            float minElevation = ElevationInfo.data.MinMaxElevation.x;
            float ElevationRange = maxElevation - minElevation;

            if (Prefs.UnderWater == OptionEnabDisab.Enable)
            {
                if (minElevation <= 0 && maxElevation <= 0)
                    ElevationRange = Math.Abs(minElevation) - Math.Abs(maxElevation);
                else
                    if (maxElevation >= 0 && minElevation < 0)
                    ElevationRange = maxElevation + Math.Abs(minElevation);

            }

            var sizeX = Mathf.Floor((float)Prefs.TerrainDimensions.x * Prefs.terrainScale.x * Prefs.ScaleFactor) / Prefs.terrainCount.x;
            var sizeZ = Mathf.Floor((float)Prefs.TerrainDimensions.y * Prefs.terrainScale.z * Prefs.ScaleFactor) / Prefs.terrainCount.y;
            var sizeY = (ElevationRange) / Prefs.ElevationScaleValue * Prefs.TerrainExaggeration * 100 * Prefs.terrainScale.y * 10;

            Vector3 size;

            if (!Prefs.Settings_SO.IsGeoFile(LoadedFileExtension))
            {
                if (Prefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = ((162)) * Prefs.terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = 300 * Prefs.TerrainExaggeration * Prefs.terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }

            }
            else
            {
                if (Prefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = (ElevationRange / Prefs.ElevationScaleValue) * 1000 * Prefs.terrainScale.y;

                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = sizeY * 10;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
            }
 
            terrains = new GISTerrainTile[tCount.x, tCount.y];

            GeneratedContainer = container.AddComponent<GISTerrainContainer>();

            GeneratedContainer.data = new GISTerrainLoaderFileData();

            GeneratedContainer.data = ElevationInfo.data;

            GeneratedContainer.TerrainCount = Prefs.terrainCount;

            GeneratedContainer.TerrainFilePath = Prefs.TerrainFilePath;

            GeneratedContainer.Scale = Prefs.terrainScale;

            GeneratedContainer.ContainerSize = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);

            GeneratedContainer.SubTerrainSize = size;

            if (Prefs.Settings_SO.IsGeoFile(LoadedFileExtension))
                GeneratedContainer.data.Dimensions = ElevationInfo.data.Dimensions;
            else
                GeneratedContainer.data.Dimensions = Prefs.TerrainDimensions;

            GeneratedContainer.data.MinMaxElevation = new Vector2((float)ElevationInfo.data.MinMaxElevation.x, (float)ElevationInfo.data.MinMaxElevation.y);

            //Terrain Size Bounds 
            var centre = new Vector3(GeneratedContainer.ContainerSize.x / 2, 0, GeneratedContainer.ContainerSize.z / 2);
            GeneratedContainer.GlobalTerrainBounds = new Bounds(centre, new Vector3(centre.x + GeneratedContainer.ContainerSize.x / 2, 0, centre.z + GeneratedContainer.ContainerSize.z / 2));


            if (!GeneratedContainer.IsValidContainer(GeneratedContainer))
            {
                StopAllCoroutines();
                Destroy(GameObject.Find(containerName));
                GC.Collect();
                GC.WaitForPendingFinalizers();
                OnError("Error While loading file : Tiff not loaded correctly, Elevation out of Range");
                yield break;
             }

            CheckTerrainMaterials();


            for (int x = 0; x < tCount.x; x++)
            {
                for (int y = 0; y < tCount.y; y++)
                {
                    if(container)
                    {
                        terrains[x, y] = CreateTerrain(container.transform, x, y, size, Prefs.terrainScale);
                        terrains[x, y].container = GeneratedContainer;
                        ListTerrainObjects.Add(terrains[x, y]);
                    }
                }
            }

            GeneratedContainer.data = ElevationInfo.data;
            GeneratedContainer.terrains = terrains;


            if (Prefs.EPSGCode==0)
            Prefs.ProjectionsData_SO.MainFileProjection = new RuntimeProjection(GISTerrainLoaderEPSG.GetEPSGName(4326), 4326);
            else
                Prefs.ProjectionsData_SO.MainFileProjection = new RuntimeProjection(GISTerrainLoaderEPSG.GetEPSGName(GeneratedContainer.data.EPSG), GeneratedContainer.data.EPSG);
 
            yield return null;

        }
        private IEnumerator GenerateHeightmap()
        {
            int index = 0;

            foreach (var Tile in ListTerrainObjects)
            {
                if (index >= terrains.Length - 1)
                {
                    yield return null;
                }

                float prog = ((index * 100) / (ListTerrainObjects.Count));

                ElevationInfo.RuntimeGenerateHeightMap(Prefs, Tile);

                yield return new WaitUntil(() => terrains[Tile.Number.x, Tile.Number.y].ElevationState == ElevationState.Loaded);

                if (OnProgress != null)
                    OnProgress("Generating Heightmap", (float)prog);

                yield return new WaitUntil(() => terrains[Tile.Number.x, Tile.Number.y].ElevationState == ElevationState.Loaded);

                index++;
            }

        }
        private IEnumerator RepareTerrains()
        {
            if (Prefs.UseTerrainHeightSmoother == OptionEnabDisab.Enable)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainHeights(ListTerrainObjects, 1 - Prefs.TerrainHeightSmoothFactor);

            if (Prefs.UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainSurface(ListTerrainObjects, Prefs.TerrainSurfaceSmoothFactor);

            if (Prefs.UseTerrainHeightSmoother == OptionEnabDisab.Enable || Prefs.UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
                GISTerrainLoaderBlendTerrainEdge.StitchTerrain(ListTerrainObjects, 50f, 20);
          
            if (Prefs.TerrainBaseboards == OptionEnabDisab.Enable)
                GISTerrainLoaderBaseboardsGenerator.GenerateTerrainBaseboards(GeneratedContainer, Prefs.BorderHigh, Prefs.terrainBorderMaterial);

            yield return null;

        }
        private IEnumerator GenerateTextures()
        {
            yield return Try(GeneratedContainer.GenerateTextures(Prefs,false,OnProgress));
        }
        private IEnumerator GenerateVectorData()
        {

            yield return Try(GeneratedContainer.GenerateVectorData(Prefs));
        }
        private IEnumerator GenerateBackground()
        {
            yield return Try(GeneratedContainer.GenerateBackground(Prefs,ElevationInfo));
        }
        
        private IEnumerator Finish()
        {
            foreach (GISTerrainTile item in terrains)
                item.terrain.Flush();



            Generatorstate = GeneratorState.idle;

            AddGISVirtualTexture();

            StopAllCoroutines();

            if (OnFinish != null)
                OnFinish(GeneratedContainer);

            if (OnProgress != null)
                OnProgress("Finalization", 100);

            if (GeneratedContainer != null)
            {
                Debug.Log("<color=green><size=14> Terrain Generated Successfully</size></color>");
                Debug.Log("<color=gray><size=14> Bounds : Upper Left " + GeneratedContainer.data.TLPoint_LatLon + ", Lower Right " + GeneratedContainer.data.DRPoint_LatLon + ", Lenght / Width  " + Math.Round(GeneratedContainer.data.Dimensions.x, 2) + " X " + Math.Round(GeneratedContainer.data.Dimensions.y, 2) + " [Km] " + "</size></color>");
            }

            yield return null;
        }
        private void CheckTerrainMaterials()
        {

            if (Prefs.terrainMaterialMode == TerrainMaterialMode.Standard)
            {
                Prefs.terrainMaterial = new Material((Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material)));

                if (Prefs.terrainMaterial == null)
                    Debug.LogError("Standard terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
            }

            if (Prefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRamp)
            {
                if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
                    Prefs.textureMode = TextureMode.WithoutTexture;

                if (Prefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    Prefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                Prefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapColorRamp", typeof(Material)));

                if (Prefs.terrainMaterial)
                {
                    Prefs.terrainMaterial.SetFloat("_TerrainHeight", GeneratedContainer.ContainerSize.y);
                    Prefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                    Prefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
                }
                else
                    Debug.LogError("HeightmapColorRamp terrain material not found in 'Resources/Materials/TerrainShaders/HeightmapColorRamp' ");
            }

            if (Prefs.terrainMaterialMode == TerrainMaterialMode.ElevationGrayScaleGradient)
            {
                if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
                    Prefs.textureMode = TextureMode.WithoutTexture;

                if (Prefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    Prefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                Prefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/ElevationGrayScaleGradient", typeof(Material)));
                Prefs.terrainMaterial.SetFloat("_TerrainHeight", GeneratedContainer.ContainerSize.y);
                Prefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                Prefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);

                if (Prefs.terrainMaterial == null)
                    Debug.LogError("ElevationGrayScaleGradient terrain material not found in 'Resources/Materials/TerrainShaders/ElevationGrayScaleGradient' ");
            }

            if (Prefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines)
            {
                if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
                    Prefs.textureMode = TextureMode.WithoutTexture;

                if (Prefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    Prefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                Prefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapContourLines", typeof(Material)));

                if (Prefs.terrainMaterial)
                {
                    Prefs.terrainMaterial.SetFloat("_ContourInterval", Prefs.ContourInterval);
                    Prefs.terrainMaterial.SetFloat("_TerrainHeight", GeneratedContainer.ContainerSize.y);
                    Prefs.terrainMaterial.SetFloat("_LineWidth", GISTerrainLoaderConstants.LineWidth);
                    Prefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                    Prefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
                }
                else
                    Debug.LogError("HeightmapContourLines terrain material not found in 'Resources/Materials/TerrainShaders/HeightmapContourLines' ");

            }

#if GISVirtualTexture
                    if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.GISVirtualTexture)
                    {
                        RuntimePrefs.terrainMaterial = (Material)Resources.Load("Materials/GISVirtualTexture", typeof(Material));

                        if (RuntimePrefs.terrainMaterial == null)
                            Debug.LogError("GIS Virtual Texture material not found in 'GIS Virtual Texture/Resources/Materials/GISVirtualTexture' ");
                    }
#endif
        }
        private void CheckTerrainTextures()
        {
            if (Prefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRamp||  Prefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines || Prefs.terrainMaterialMode == TerrainMaterialMode.ElevationGrayScaleGradient)
                Prefs.textureMode = TextureMode.WithoutTexture;

            if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
            {
                if (Prefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                {
                    var c_count = GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(Prefs.TerrainFilePath);

                    Prefs.terrainCount = new Vector2Int((int)c_count.x, (int)c_count.y);

                    if (c_count == Vector2.zero)
                    {
                        Prefs.terrainCount = new Vector2Int(1, 1);
                    }

                }
            }
        }
        public void StopGeneration()
        {
            StopAllCoroutines();
        }
        public void SetGeneratedTerrain(GISTerrainContainer container)
        {
            GeneratedContainer = container;
        }
        void OnError(string errorMsg)
        {
            if(!string.IsNullOrEmpty(errorMsg))
            Debug.LogError(errorMsg);

            Generatorstate = GeneratorState.idle;
            StopAllCoroutines();
        }
        private void AddGISVirtualTexture()
        {

#if GISVirtualTexture

            if (Prefs.terrainMaterialMode == TerrainMaterialMode.GISVirtualTexture)
            {
                if (GeneratedContainer)
                {
                    Prefs.terrainMaterial = (Material)Resources.Load("Materials/GISVirtualTexture", typeof(Material));

                    if (Prefs.terrainMaterial)
                    {
                        if (!GameObject.FindObjectOfType<GISTech.GISVirtualTexture.GISVirtualTextureRuntimePrefs>())
                        {
                            GeneratedContainer.gameObject.AddComponent<GISTech.GISVirtualTexture.GISVirtualTextureRuntimePrefs>();
                        }
                        else
                        {
                            Debug.LogError("Runtime GIS Virtual Texture already exists in your scene");
                        }
                    }
                    else
                        Debug.LogError("GIS Virtual Texture material not found ('GIS Virtual Texture/Resources/Materials/GISVirtualTexture') ");
                }

            }
#endif
        }
        void OnDisable()
        {
            GISTerrainLoaderFloatReader.OnReadError -= OnError;
            GISTerrainLoaderTIFFLoader.OnReadError -= OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError -= OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError -= OnError;
            GISTerrainLoaderRawLoader.OnReadError -= OnError;
            GISTerrainLoaderASCILoader.OnReadError -= OnError;
            GISTerrainLoaderBILReader.OnReadError -= OnError;
            GISTerrainLoaderHGTLoader.OnReadError -= OnError;
 
#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError -= OnError;
#endif
        }
        private void CheckForDimensionAndTiles(bool AutoDim)
        {
            if (Prefs.terrainCount == new Vector2Int(0, 0))
                Prefs.terrainCount = new Vector2Int(1, 1);

            if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                if (AutoDim)
                {
                    if (ElevationInfo.data.Dimensions.x == 0 || ElevationInfo.data.Dimensions.y == 0)
                    {
                        Debug.LogError("Can't detecte terrain dimension (Check your file projection) and try againe ");
                        StopAllCoroutines();
                        return;
                    }
                    else
                    if (ElevationInfo.data.Dimensions != new DVector2(0, 0))
                    {
                        Prefs.TerrainDimensions =  ElevationInfo.data.Dimensions;
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        Prefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }
                    else
                    {
                        StopAllCoroutines();
                    }
                }
                else
                {
                    if (Prefs.TerrainDimensions.x == 0 || Prefs.TerrainDimensions.y == 0)
                    {
                        OnError("Reset Terrain dimensions ... try again  ");
                        StopAllCoroutines();
                        return;
                    }
                    else
        if (Prefs.TerrainDimensions != new DVector2(0, 0))
                    {
                        //RuntimePrefs.TerrainDimensions = new Vector2(RuntimePrefs.TerrainDimensions.x, RuntimePrefs.TerrainDimensions.y);
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        Prefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }
                    else
                    {
                        if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
                         OnError("Can't detecte terrain textures folder ... try again");
                    }

                }
            }
            else
            {
                if (Prefs.TerrainDimensions.x == 0 || Prefs.TerrainDimensions.y == 0)
                {
                    OnError("Can't detecte terrain textures folder ... try again");
                    StopAllCoroutines();
                    return;
                }
                else
    if (Prefs.TerrainDimensions != new DVector2(0, 0))
                {
                    //RuntimePrefs.TerrainDimensions = new Vector2((float)RuntimePrefs.TerrainDimensions.x, RuntimePrefs.TerrainDimensions.y);
                }

                if (ElevationInfo.data.Tiles != Vector2.zero)
                {
                    Prefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                }
                else
                {
                    ElevationInfo.data.Tiles = Prefs.terrainCount;
                    if (Prefs.textureMode == TextureMode.WithTexture || Prefs.textureMode == TextureMode.MultiTexture || Prefs.textureMode == TextureMode.MultiLayers)
                        OnError("Texture Folder Not Found...");

                    StopAllCoroutines();
                }
            }

        }
        private GISTerrainTile CreateTerrain(Transform parent, int x, int y, Vector3 size, Vector3 scale)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.heightmapResolution = Prefs.heightmapResolution;
            tdata.baseMapResolution = Prefs.baseMapResolution;
            tdata.SetDetailResolution(Prefs.detailResolution, Prefs.resolutionPerPatch);
            tdata.size = size;

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("{0}-{1}", x, y);
            GO.transform.parent = parent;
            GO.transform.position = new Vector3(size.x * x, 0, size.z * y);


            GISTerrainTile item = GO.AddComponent<GISTerrainTile>();
            item.Number = new Vector2Int(x, y);
            item.size = size;
            item.ElevationFilePath = Prefs.TerrainFilePath;
 
            var t = GO.GetComponent<Terrain>();
            item.terrain = t;
            item.terrainData = t.terrainData;

            item.terrain.heightmapPixelError = Prefs.PixelError;
            item.terrain.basemapDistance = Prefs.BaseMapDistance;
            item.terrain.materialTemplate = Prefs.terrainMaterial;


            if (Prefs.TerrainLayerSet == OptionEnabDisab.Enable)
                item.terrain.gameObject.layer = Prefs.TerrainLayer;


            item.TextureState = TextureState.Wait;
            item.ElevationState = ElevationState.Wait;

            float prog = ((terrains.GetLength(0) * terrains.GetLength(1) * 100f) / (Prefs.terrainCount.x * Prefs.terrainCount.y)) / 100f;

            if (OnProgress != null)
            {
                OnProgress("Generating Terrains", prog);
            }


            return item;
        }
        public IEnumerator Try(IEnumerator enumerator)
        {
            while (true)
            {
                object current;
                try
                {
                    if (enumerator.MoveNext() == false)
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    OnError("Error while generating terrain : " + ex.Message);
                    yield break;
                }
                yield return current;
            }
        }

    }
}