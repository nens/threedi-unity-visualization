/*     Unity GIS Tech 2020-2023      */
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainContainer : MonoBehaviour
    {

#if UNITY_EDITOR
        public int lastTab = 0;
#endif
        public ProjectionMode m_ContainerProjection = ProjectionMode.Geographic;

        public GISTerrainLoaderFileData data;

        public Vector3 Scale;
        public Vector3 ContainerSize;
        public Vector3 SubTerrainSize;
        public Vector2Int TerrainCount;

        public Bounds GlobalTerrainBounds;
        public string TerrainFilePath;

        private GISTerrainTile[,] _terrains;
        public GISTerrainTile[,] terrains
        {
            get
            {
                if (_terrains == null)
                {
                    _terrains = new GISTerrainTile[TerrainCount.x, TerrainCount.y];
                    GISTerrainTile[] items = GetComponentsInChildren<GISTerrainTile>();
                    foreach (GISTerrainTile item in items) _terrains[item.Number.x, item.Number.y] = item;
                }
                return _terrains;
            }
            set
            {
                _terrains = value;
            }
        }


        /// <summary>
        /// Get Terrain Tile which intersect with the Position x
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public GISTerrainTile GetTerrainTile(Vector3 Position)
        {
            GISTerrainTile Tile = null;
            _terrains = terrains;
            for (int i = 0; i < TerrainCount.x; i++)
            {
                for (int j = 0; j < TerrainCount.y; j++)
                {
                    var tile = _terrains[i, j];

                    if (tile.IncludePoint(Position))
                    {
                        Tile = tile;
        
                    }
                       

                }
            }
            return Tile;

        }

        /// <summary>
        /// Check If the Real World Coordinates belongs to Terrain Real World Bounds 
        /// </summary>
        /// <returns></returns>
        public bool IncludeRealWorldPoint(DVector2 RealWorldCoor)
        {
            bool PointInculded = false;

            if (data.EPSG != 0)
            {
                if (IncludeEPSGPoint(RealWorldCoor))
                    PointInculded = true;

            }
            else
            {
                if (IncludePoint(RealWorldCoor))
                    PointInculded = true;
            }

            return PointInculded;
        }
        public bool IncludeEPSGPoint(DVector2 Coor)
        {
            bool Include = false;

            var MinLat = data.DROriginal_Coor.y;
            var MinLon = data.TLOriginal_Coor.x;
            var MaxLat = data.TLOriginal_Coor.y;
            var MaxLon = data.DROriginal_Coor.x;

            if (Coor.x > MinLon && Coor.x < MaxLon && Coor.y > MinLat && Coor.y < MaxLat)
                Include = true;
            return Include;
        }
        public bool IncludePoint(DVector2 LatLon)
        {
            bool Include = false;

            var MinLat = data.DLPoint_LatLon.y;
            var MinLon = data.TLPoint_LatLon.x;
            var MaxLat = data.TLPoint_LatLon.y;
            var MaxLon = data.DRPoint_LatLon.x;

            if (LatLon.x >= MinLon && LatLon.x <= MaxLon && LatLon.y >= MinLat && LatLon.y <= MaxLat)
                Include = true;
            return Include;
        }


        public bool IncludeSpacePosition(Vector3 Position)
        {
            bool Include = false;

            if (Position.x >= 0 && Position.x <= ContainerSize.x && Position.z >= 0 && Position.z < ContainerSize.z)
                Include = true;

            return Include;
        }
        /// <summary>
        /// Check if the container has a correct parameters
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public bool IsValidContainer(GISTerrainContainer container)
        {
            bool isValid = true;

            if (float.IsNaN(container.ContainerSize.x) || float.IsNaN(container.ContainerSize.y) || float.IsNaN(container.ContainerSize.z))
                isValid = false;

            if (float.IsInfinity(container.ContainerSize.x) || float.IsInfinity(container.ContainerSize.y) || float.IsInfinity(container.ContainerSize.z))
                isValid = false;

            if (Math.Abs(container.ContainerSize.x) < float.Epsilon|| Math.Abs(container.ContainerSize.y) < float.Epsilon || Math.Abs(container.ContainerSize.z) < float.Epsilon)
                isValid = false;

            return isValid;

        }
        public float RouteScaleOverage()
        {

            float value = 1;

#if UNITY_EDITOR
            if (Application.isPlaying || EditorApplication.isPlaying)
                value = ((Scale.x + Scale.y + Scale.z) / 3) / 6;

            if (Application.isEditor && !EditorApplication.isPlaying)
                value = ((Scale.x + Scale.y + Scale.z) / 3) * 1.3f;

#else           
            value = ((Scale.x + Scale.y + Scale.z) / 3) / 6;
#endif

            return value;
        }
        public float LableScaleOverage()
        {
            float value = 1;

#if UNITY_EDITOR
            if (Application.isPlaying || EditorApplication.isPlaying)
                value = ((Scale.x + Scale.y + Scale.z) / 3) * 2.5f;

            if (Application.isEditor && !EditorApplication.isPlaying)
                value = ((Scale.x + Scale.y + Scale.z) / 3) * 10;
#else           
            value = ((Scale.x + Scale.y + Scale.z) / 3)  * 2.5f;
#endif

            return value;
        }
 

        #region Export Data
        public void ExportVectorData(ExportVectorType VectorType, string m_FilePath, GISTerrainLoaderGeoVectorData m_GeoData, bool m_StoreElevationData = false)
        {
            switch (VectorType)
            {
                case ExportVectorType.Shapfile:
                    GISTerrainLoaderShapeFileExporter Exporter = new GISTerrainLoaderShapeFileExporter(m_FilePath, m_GeoData, m_StoreElevationData);
                    Exporter.Export();
                    break;
            }
        }
        #endregion

        #region TotatlTerrains Prefs
        private int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        private int[] availableHeightsResolutionPrePec = { 4, 8, 16, 32 };

        public float m_PixelErro;
        public float PixelErro
        {
            get { return m_PixelErro; }
            set
            {
                if (m_PixelErro != value)
                {
                    m_PixelErro = value;
                    OnPixelErroValueChanged(value);

                }
            }
        }

        public float m_BaseMapDistance = 1000;
        public float BaseMapDistance
        {
            get { return m_BaseMapDistance; }
            set
            {
                if (m_BaseMapDistance != value)
                {
                    m_BaseMapDistance = value;
                    OnBaseMapDistanceValueChanged(value);

                }
            }
        }

        public float m_DetailDistance = 100;
        public float DetailDistance
        {
            get { return m_DetailDistance; }
            set
            {
                if (m_DetailDistance != value)
                {
                    m_DetailDistance = value;
                    OnDetailDistanceValueChanged(value);

                }
            }
        }

        public float m_DetailDensity = 100;
        public float DetailDensity
        {
            get { return m_DetailDensity; }
            set
            {
                if (m_DetailDensity != value)
                {
                    m_DetailDensity = value;
                    OnDetailDensityValueChanged(value);

                }
            }
        }

        public float m_TreeDistance = 4000;
        public float TreeDistance
        {
            get { return m_TreeDistance; }
            set
            {
                if (m_TreeDistance != value)
                {
                    m_TreeDistance = value;
                    OnTreeDistanceValueChanged(value);

                }
            }
        }

        public float m_BillBoardStartDistance = 500;
        public float BillBoardStartDistance
        {
            get { return m_BillBoardStartDistance; }
            set
            {
                if (m_BillBoardStartDistance != value)
                {
                    m_BillBoardStartDistance = value;
                    OnBillBoardStartDistanceValueChanged(value);

                }
            }
        }

        private float m_FadeLength = 10;
        public float FadeLength
        {
            get { return m_FadeLength; }
            set
            {
                if (m_FadeLength != value)
                {
                    m_FadeLength = value;
                    OnFadeLengthValueChanged(value);

                }
            }
        }

        private int m_DetailResolution_index = 5;
        public int DetailResolution_index
        {
            get { return m_DetailResolution_index; }
            set
            {
                if (m_DetailResolution_index != value)
                {
                    m_DetailResolution_index = value;
                    OnDetailResolutionValueChanged(value);

                }
            }
        }
        private int m_ResolutionPerPatch_index = 1;
        public int ResolutionPerPatch_index
        {
            get { return m_ResolutionPerPatch_index; }
            set
            {
                if (m_ResolutionPerPatch_index != value)
                {
                    m_ResolutionPerPatch_index = value;
                    OnResolutionPerPatchValueChanged(value);

                }
            }
        }

        private int m_BaseMapResolution_index = 5;
        public int BaseMapResolution_index
        {
            get { return m_BaseMapResolution_index; }
            set
            {
                if (m_BaseMapResolution_index != value)
                {
                    m_BaseMapResolution_index = value;
                    OnBaseMapResolutionValueChanged(value);

                }
            }
        }

        public void OnPixelErroValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.heightmapPixelError = value;
        }
        public void OnBaseMapDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.basemapDistance = value;
        }
        public void OnDetailDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.detailObjectDistance = value;
        }
        public void OnDetailDensityValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.detailObjectDensity = value;
        }

        public void OnTreeDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeDistance = value;
        }
        public void OnBillBoardStartDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeBillboardDistance = value;
        }
        public void OnFadeLengthValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeCrossFadeLength = value;
        }

        public void OnDetailResolutionValueChanged(int value)
        {
            var detailResolution = availableHeights[value];
            foreach (var t in terrains)
            {
                var resolutionPerPatch = t.terrain.terrainData.detailResolutionPerPatch;
                t.terrain.terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);
            }
        }
        public void OnResolutionPerPatchValueChanged(int value)
        {
            var resolutionPerPatch = availableHeightsResolutionPrePec[value];

            foreach (var t in terrains)
            {
                var detailResolution = t.terrain.terrainData.detailResolution;
                t.terrain.terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);
            }

        }
        public void OnBaseMapResolutionValueChanged(int value)
        {
            var baseMapResolution = availableHeights[value];

            foreach (var t in terrains)
                t.terrain.terrainData.baseMapResolution = baseMapResolution;
        }


        #endregion

        #region Runtime Loader 

        #region Runtime Raster Loader
        private GISTerrainLoaderWebData WebData = new GISTerrainLoaderWebData();
        private List<GISTerrainTile> ListTerrainObjects = new List<GISTerrainTile>();
        public IEnumerator GenerateTextures(GISTerrainLoaderPrefs Prefs, bool ClearLayers=false, TerrainProgression OnProgress=null)
        {
            yield return StartCoroutine(CheckFileConfig(Prefs));

            int index = 0;
 
            switch (Prefs.textureMode)
            {
                case TextureMode.WithTexture:

                    if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                    {

                        if (WebData.TextureFolderExist == 1)
                        {
                            foreach (var tile in WebData.textures)
                            {
                                var numbers = Regex.Matches(tile.Split('.')[0], @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
                                int x = numbers[0]; int y = numbers[1];

                                GISTerrainTile terrain = null;

                                foreach (var t in terrains)
                                {
                                    if (t.Number.x == x && t.Number.y == y)
                                        terrain = t;

                                }

                                var texturePath = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, WebData.MainPath, Prefs.Settings_SO.TextureFolderName, tile);

                                Texture2D texture = null;

                                yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(texturePath, (m_texture) =>
                                {
                                    texture = m_texture;
                                }));

                                if (texture == null)
                                {
                                    texture = (Texture2D)Resources.Load("Textures/NullTexture");
                                }
                                else
                                    GISTerrainLoaderTextureGenerator.RuntimePlatformeAddTexturesToTerrain(texture, terrain, ClearLayers);


                            }
                        }
                    }
                    else
                    {
                        TextureSourceFormat TextureSourceformat = null;

                        bool TextureFolderExist = GISTerrainLoaderTextureGenerator.CheckForTerrainTextureFolder(Prefs.TerrainFilePath, out TextureSourceformat);

                        if (Prefs.textureloadingMode == TexturesLoadingMode.Manual)
                        {
                            var FolderTiles_count = GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(Prefs.TerrainFilePath); ;

                            if (Prefs.terrainCount != FolderTiles_count)
                            {
                                if (FolderTiles_count == Vector2.one)
                                {
                                    GISTerrainLoaderTextureGenerator.SplitTex(Prefs.TerrainFilePath, Prefs.terrainCount).Wait();
                                }
                                else
                                {
                                    if (FolderTiles_count.x > 1 || FolderTiles_count.y > 1)
                                    {
                                        GISTerrainLoaderTextureGenerator.CombienTerrainTextures(Prefs.TerrainFilePath);

                                        GISTerrainLoaderTextureGenerator.SplitTex(Prefs.TerrainFilePath, Prefs.terrainCount).Wait();

                                        Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
                                    }

                                }

                            }
                            else
                                Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;

                        }

                        if (Prefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                        {
                            if (TextureFolderExist)
                            {
                                var TextureFolderPath = Path.Combine(Path.GetDirectoryName(Prefs.TerrainFilePath), Path.GetFileNameWithoutExtension(Prefs.TerrainFilePath) + Prefs.Settings_SO.TextureFolderName);

                                string[] Tiles = GISTerrainLoaderTextureGenerator.GetTextureTiles(TextureFolderPath, Prefs.Settings_SO);


                                if (Tiles.Length > 0)
                                {
                                    TextureSourceFormat TextureSourceSoft = null;

                                    bool IsCorrectFormat = GISTerrainLoaderTextureGenerator.TextureSourceName(Tiles, Prefs.Settings_SO, out TextureSourceSoft);

                                    if (IsCorrectFormat)
                                    {

                                        foreach (var Tile in ListTerrainObjects)
                                        {
                                            if (index >= ListTerrainObjects.Count)
                                            {
                                                yield return null;
                                            }

                                            float prog = ((index * 100) / (ListTerrainObjects.Count));

                                            if (OnProgress != null)
                                                OnProgress("Generate Textures", prog);

                                            GISTerrainLoaderTextureGenerator.RuntimeAddTexturesToTerrain(TextureFolderPath, Prefs.Settings_SO, TextureSourceSoft, Tile, ClearLayers);

                                            yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                                            index++;
                                        }

                                    }

                                }

                            }
                            else
                                Debug.LogError("Texture Folder Not Found...");

                        }
                    }
                    break;
                case TextureMode.MultiTexture:

                    if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        if (WebData.TextureFolderExist == 1)
                        {
                            foreach (var tile in WebData.textures)
                            {
                                var numbers = Regex.Matches(tile.Split('.')[0], @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
                                int x = numbers[0]; int y = numbers[1];

                                GISTerrainTile terrain = null;

                                foreach (var t in terrains)
                                {
                                    if (t.Number.x == x && t.Number.y == y)
                                        terrain = t;

                                }

                                var texturePath = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, WebData.MainPath, Prefs.Settings_SO.TextureFolderName, tile);

                                Texture2D texture = null;

                                yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(texturePath, (m_texture) =>
                                {
                                    texture = m_texture;
                                }));

                                if (texture == null)
                                {
                                    texture = (Texture2D)Resources.Load("Textures/NullTexture");
                                }
                                else
                                    GISTerrainLoaderTextureGenerator.RuntimePlatformeAddTexturesToTerrain(texture, terrain, ClearLayers);


                            }
                        }
                    }
                    else
                    {
                        TextureSourceFormat TextureSourceformat = null;

                        bool MainTextureFolderExist = GISTerrainLoaderTextureGenerator.CheckForTerrainTextureFolder(Prefs.TerrainFilePath, out TextureSourceformat);

                        if (Prefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                        {
                            if (MainTextureFolderExist)
                            {
                                var TextureFolderPath = Path.Combine(Path.GetDirectoryName(Prefs.TerrainFilePath), Path.GetFileNameWithoutExtension(Prefs.TerrainFilePath) + Prefs.Settings_SO.TextureFolderName);

                                List<string> FullTexturesFolders = GISTerrainLoaderTextureGenerator.GetFullTextureFolders(Prefs.TerrainFilePath);

                                if (FullTexturesFolders.Count > 0)
                                {
                                    string FolderPath = FullTexturesFolders[Prefs.TextureFolderIndex];

                                    string[] Tiles = GISTerrainLoaderTextureGenerator.GetTextureTiles(FolderPath, Prefs.Settings_SO);
                                     
                                    if (Tiles.Length > 0)
                                    {
                                        TextureSourceFormat TextureSourceSoft = null;

                                        bool IsCorrectFormat = GISTerrainLoaderTextureGenerator.TextureSourceName(Tiles, Prefs.Settings_SO, out TextureSourceSoft);

                                        if (IsCorrectFormat)
                                        {
                                            foreach (var Tile in ListTerrainObjects)
                                            {
                                                if (index >= ListTerrainObjects.Count)
                                                {
                                                    yield return null;
                                                }

                                                float prog = ((index * 100) / (ListTerrainObjects.Count));

                                                if (OnProgress != null)
                                                    OnProgress("Generate Textures", prog);

                                                GISTerrainLoaderTextureGenerator.RuntimeAddMultiTextureToTerrain(FolderPath, Prefs.Settings_SO, TextureSourceSoft, Tile, ClearLayers);

                                                yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                                                index++;
                                            }

                                        }

                                    }
 
                                }


                            }
                            else
                                Debug.LogError("Texture Folder Not Found...");

                        }
                    }
                    break;
                case TextureMode.MultiLayers:

                    if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        if (WebData.TextureFolderExist == 1)
                        {
                            foreach (var tile in WebData.textures)
                            {
                                var numbers = Regex.Matches(tile.Split('.')[0], @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
                                int x = numbers[0]; int y = numbers[1];

                                GISTerrainTile terrain = null;

                                foreach (var t in terrains)
                                {
                                    if (t.Number.x == x && t.Number.y == y)
                                        terrain = t;

                                }

                                var texturePath = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, WebData.MainPath, Prefs.Settings_SO.TextureFolderName, tile);

                                Texture2D texture = null;

                                yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(texturePath, (m_texture) =>
                                {
                                    texture = m_texture;
                                }));

                                if (texture == null)
                                {
                                    texture = (Texture2D)Resources.Load("Textures/NullTexture");
                                }
                                else
                                    GISTerrainLoaderTextureGenerator.RuntimePlatformeAddTexturesToTerrain(texture, terrain, ClearLayers);


                            }
                        }
                    }
                    else
                    {
                        TextureSourceFormat TextureSourceformat = null;

                        bool MainTextureFolderExist = GISTerrainLoaderTextureGenerator.CheckForTerrainTextureFolder(Prefs.TerrainFilePath, out TextureSourceformat);

                        if (Prefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                        {
                            if (MainTextureFolderExist)
                            {
                                var TextureFolderPath = Path.Combine(Path.GetDirectoryName(Prefs.TerrainFilePath), Path.GetFileNameWithoutExtension(Prefs.TerrainFilePath) + Prefs.Settings_SO.TextureFolderName);
                                
                                List<string> FullTexturesFolders = GISTerrainLoaderTextureGenerator.GetFullTextureFolders(Prefs.TerrainFilePath);

                                if (FullTexturesFolders.Count > 0)
                                {
                                      foreach (var Folder in FullTexturesFolders)
                                    {

                                        string[] Tiles = GISTerrainLoaderTextureGenerator.GetTextureTiles(Folder, Prefs.Settings_SO);

                                        if (Tiles.Length > 0)
                                        {
                                            TextureSourceFormat TextureSourceSoft = null;

                                            bool IsCorrectFormat = GISTerrainLoaderTextureGenerator.TextureSourceName(Tiles, Prefs.Settings_SO, out TextureSourceSoft);

                                            if (IsCorrectFormat)
                                            {
                                                foreach (var Tile in ListTerrainObjects)
                                                {
                                                    if (index >= ListTerrainObjects.Count)
                                                    {
                                                        yield return null;
                                                    }

                                                    float prog = ((index * 100) / (ListTerrainObjects.Count));

                                                    if (OnProgress != null)
                                                        OnProgress("Generate Textures", prog);

                                                    GISTerrainLoaderTextureGenerator.RuntimeAddMultiTextureToTerrain(Folder, Prefs.Settings_SO, TextureSourceSoft, Tile, ClearLayers);

                                                    yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                                                    index++;
                                                }

                                            }

                                        }
                                    }
                                }


                            }
                            else
                                Debug.LogError("Texture Folder Not Found...");

                        }
                    }
                    break;

                case TextureMode.WithoutTexture:

                    if (Prefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    {
                        Material mat = new Material(Shader.Find("Standard"));

                        mat.SetColor("_Color", Prefs.TerrainEmptyColor);

                        foreach (var Tile in ListTerrainObjects)
                        {
                            float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                            Tile.terrain.materialTemplate = mat;

                            index++;

                            if (index >= terrains.Length)
                            {
                                yield return null;
                            }

                            if (OnProgress != null)
                                OnProgress("Generating Terrain Colors", prog);

                        }

                    }

                    yield return null;

                    break;

                case TextureMode.ShadedRelief:

                    foreach (var Tile in ListTerrainObjects)
                    {
                        if (index >= ListTerrainObjects.Count)
                        {
                            yield return null;
                        }

                        float prog = ((index * 100) / (ListTerrainObjects.Count));

                        if (OnProgress != null)
                            OnProgress("Generating Terrain Shader", prog);
 
                        GISTerrainLoaderTerrainShader.GenerateShadedTextureRuntime(Prefs, Tile, ClearLayers);

                        yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                        index++;

                    }

                    break;
                case TextureMode.Splatmapping:

                    foreach (var Tile in ListTerrainObjects)
                    {
                        float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                        GISTerrainLoaderSplatMapping.SetTerrainSpaltMap(Prefs, Tile);

                        index++;

                        if (OnProgress != null)
                            OnProgress("Generating Splatmaps ", prog);

                        if (index >= terrains.Length)
                        {
                            yield return null;
                        }
                    }

                    break;
            }

            FreeUpMemory();

            yield return null;
        }
        private IEnumerator CheckFileConfig(GISTerrainLoaderPrefs Prefs)
        {
            ListTerrainObjects = new List<GISTerrainTile>();

            Prefs.LoadSettings();

            for (int x = 0; x < TerrainCount.x; x++)
            {
                for (int y = 0; y < TerrainCount.y; y++)
                {
                    ListTerrainObjects.Add(terrains[x, y]);
                }
            }

            WebData = new GISTerrainLoaderWebData();

            var WebDataPath = GISTerrainLoaderPlatformHelper.GetGISWebDataPath(Application.platform, Prefs.TerrainFilePath);

            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileWebData(WebDataPath, (data) =>
                {
                    WebData = data;
                    Prefs.terrainCount = WebData.Tiles_count;
                }));
            }
            else
            {
                if (Prefs.Settings_SO.IsValidTerrainFile(Path.GetExtension(Prefs.TerrainFilePath)))
                {
                    CheckTerrainTextures(Prefs);
                }
                else
                {
                    Debug.LogError("Can't Load this File or not exist..");
                }
                yield return null;
            }

            CheckTerrainMaterials(Prefs);
        }

        #endregion

        #region RuntimeMaterial

        public void UpdateTerrainMaterial(TerrainMaterialMode terrainMaterialMode, float ContourInterval = 50, float ContourLineWidth = 0.017f)
        {
            foreach (var terrain in terrains)
            {
                Material terrainMaterial = terrain.terrain.materialTemplate;

                if (terrainMaterialMode == TerrainMaterialMode.Standard)
                {
                    terrainMaterial = new Material((Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material)));

                    if (terrainMaterial == null)
                        Debug.LogError("Standard terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
                }

                if (terrainMaterialMode == TerrainMaterialMode.HeightmapColorRamp)
                {
                    terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapColorRamp", typeof(Material)));

                    if (terrainMaterial)
                    {
                        terrainMaterial.SetFloat("_TerrainHeight", ContainerSize.y);
                        terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                        terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
                    }
                    else
                        Debug.LogError("HeightmapColorRamp terrain material not found in 'Resources/Materials/TerrainShaders/HeightmapColorRamp' ");
                }

                if (terrainMaterialMode == TerrainMaterialMode.ElevationGrayScaleGradient)
                {

                    terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/ElevationGrayScaleGradient", typeof(Material)));
                    terrainMaterial.SetFloat("_TerrainHeight", ContainerSize.y);
                    terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                    terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);

                    if (terrainMaterial == null)
                        Debug.LogError("ElevationGrayScaleGradient terrain material not found in 'Resources/Materials/TerrainShaders/ElevationGrayScaleGradient' ");
                }

                if (terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines)
                {
                    terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapContourLines", typeof(Material)));

                    if (terrainMaterial)
                    {
                        terrainMaterial.SetFloat("_ContourInterval", ContourInterval);
                        terrainMaterial.SetFloat("_TerrainHeight", ContainerSize.y);
                        terrainMaterial.SetFloat("_LineWidth", ContourLineWidth);
                        terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                        terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
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
                terrain.terrain.materialTemplate = terrainMaterial;
            }




        }
        #endregion

        #region Runtime Vector Loader
        public IEnumerator GenerateVectorData(GISTerrainLoaderPrefs Prefs)
        {
            GISTerrainContainer GeneratedContainer = this;

            var LoadedFileExtension = Path.GetExtension(Prefs.TerrainFilePath);

            var isGeoFile = Prefs.IsVectorGenerationEnabled(LoadedFileExtension);

            if (isGeoFile)
            {
                if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {
                    if (Prefs.EnableGeoPointGeneration == OptionEnabDisab.Enable)
                        Prefs.LoadAllPointPrefabs();

                    if (Prefs.EnableRoadGeneration == OptionEnabDisab.Enable)
                        Prefs.LoadAllRoadPrefabs(Prefs.RoadGenerator);

                    if (Prefs.EnableTreeGeneration == OptionEnabDisab.Enable)
                        GISTerrainLoaderTreeGenerator.AddTreePrefabsToTerrains(GeneratedContainer, Prefs);

                    if (Prefs.EnableGrassGeneration == OptionEnabDisab.Enable)
                        GISTerrainLoaderGrassGenerator.AddDetailsLayersToTerrains(GeneratedContainer, Prefs);

                    if (Prefs.EnableBuildingGeneration == OptionEnabDisab.Enable)
                        Prefs.LoadAllBuildingPrefabs();

                    if (Prefs.EnableWaterGeneration == OptionEnabDisab.Enable)
                        Prefs.LoadAllWaterPrefabs();

                    if (Prefs.EnableLandParcelGeneration == OptionEnabDisab.Enable)
                        Prefs.LoadAllLandParcelPrefabs();

                    
                    List<GISTerrainLoaderGeoVectorData> LoadedGeoData = new List<GISTerrainLoaderGeoVectorData>();

                    if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        var AllVectorData = GISTerrainLoaderVectorExtensions.GetAllVectorFilesFromWebData(WebData, Prefs.vectorType);
                       
                        if (AllVectorData.Count > 0)
                        {
                            foreach (var vectorfile in AllVectorData)
                            {
                                string filePath = vectorfile.Key;

                                VectorType fileType = vectorfile.Value;

                                GISTerrainLoaderGeoVectorData GeoData = new GISTerrainLoaderGeoVectorData();

                                switch (fileType)
                                {
                                    case VectorType.OpenStreetMap:
 
                                        var filedata = new byte[0];

                                        yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(filePath, (data) =>
                                        {
                                            filedata = data;

                                        }));

                                        GISTerrainLoaderOSMFileLoader osmloader = new GISTerrainLoaderOSMFileLoader(filedata);

                                        GeoData = osmloader.GetGeoFiltredData("", GeneratedContainer);

                                        if (!GeoData.IsEmptyGeoData() && (Prefs.vectorType == fileType))
                                            LoadedGeoData.Add(GeoData);
                                        break;

                                    case VectorType.ShapeFile:
 
                                        var ShpData = new byte[0];
                                        var ProjData = new byte[0];
                                        var DBFData = new byte[0];

                                        string dbfpath = Path.ChangeExtension(filePath, ".dbf");
                                        string ProjPath = Path.ChangeExtension(filePath, ".prj");

                                        yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(filePath, (data) =>
                                        {
                                            ShpData = data;

                                        }));

                                        yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(dbfpath, (data) =>
                                        {
                                            DBFData = data;
                                        }));

                                        yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(ProjPath, (data) =>
                                        {
                                            ProjData = data;
                                        }));

                                        GISTerrainLoaderShpFileHeader shape = GISTerrainLoaderShapeReader.LoadFile(ShpData, filePath) as GISTerrainLoaderShpFileHeader;

                                        var shpFileLoader = new GISTerrainLoaderShapeFileLoader(shape, DBFData, ProjData);

                                        GeoData = shpFileLoader.GetGeoFiltredData("", GeneratedContainer);

                                        if (!GeoData.IsEmptyGeoData() && (Prefs.vectorType == fileType))
                                            LoadedGeoData.Add(GeoData);
  
                                        break;

                                    case VectorType.GPX:

                                        var GpxData = new byte[0];

                                        yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(filePath, (data) =>
                                        {
                                            GpxData = data;

                                        }));

                                        GISTerrainLoaderGPXLoader LoadGPXFile = new GISTerrainLoaderGPXLoader(filePath, GpxData);
                                        
                                        GeoData = LoadGPXFile.GetGeoFiltredData("", GeneratedContainer);

                                        if (!GeoData.IsEmptyGeoData() && (Prefs.vectorType == fileType))
                                            LoadedGeoData.Add(GeoData);
                                        
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                         LoadedGeoData = GISTerrainLoaderVectorParser.LoadVectorFiles(Prefs, GeneratedContainer);

                    }
 
                    foreach (var GeoData in LoadedGeoData)
                    {
                        if (Prefs.EnableGeoPointGeneration == OptionEnabDisab.Enable)
                        {
                            var GeoPoints = GISTerrainLoaderDataFilter.GetGeoVectorPointsData(GeoData, Prefs.VectorParameters_SO.Attributes_Points);
                            GISTerrainLoaderGeoPointGenerator.GenerateGeoPoint(GeneratedContainer, GeoPoints, Prefs);
                        }

                        if (Prefs.EnableRoadGeneration == OptionEnabDisab.Enable)
                        {
                            var GeoLines = GISTerrainLoaderDataFilter.GetGeoVectorLinesData(GeoData, Prefs.VectorParameters_SO.Attributes_Roads);
                            GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(GeneratedContainer, GeoLines, Prefs);
                        }

                        if (Prefs.EnableTreeGeneration == OptionEnabDisab.Enable)
                        {
                            if (Prefs.TreePrefabs.Count > 0)
                            {
                                var GeoPolygons = GISTerrainLoaderDataFilter.GetGeoVectorPolyData(GeoData, Prefs.VectorParameters_SO.Attributes_Trees);

                                GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, GeoPolygons, Prefs);
                            }
                            else
                                Debug.LogError("Error : Tree Prefabs List is empty ");
                        }


                        if (Prefs.EnableGrassGeneration == OptionEnabDisab.Enable)
                        {
                            if (Prefs.GrassPrefabs.Count > 0)
                            {
                                var GeoPolygons = GISTerrainLoaderDataFilter.GetGeoVectorPolyData(GeoData, Prefs.VectorParameters_SO.Attributes_Grass);

                                GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GeoPolygons,Prefs);
                            }
                            else
                                Debug.LogError("Error : Grass Prefabs List is empty ");

                        }

                        if (Prefs.EnableBuildingGeneration == OptionEnabDisab.Enable)
                        {
                            var GeoPolygons = GISTerrainLoaderDataFilter.GetGeoVectorPolyData(GeoData, Prefs.VectorParameters_SO.Attributes_Buildings);

                            GISTerrainLoaderBuildingGenerator.GenerateBuildings(GeneratedContainer, GeoPolygons, Prefs);
                        }

                    }

                    yield return null;
                }
                else
                {
                    Debug.LogError("Vector Data Available only with Real World DEM Data (Set Terrain Dimension Mode to Auto)");
                }

            }

            FreeUpMemory();
        }
#endregion
#endregion

        public void GetStoredHeightmap(bool debug = true)
        {
            TextAsset heightmap = (TextAsset)Resources.Load(("HeightmapData/" + data.SerializedFileName), typeof(TextAsset));
            TextAsset FileData = (TextAsset)Resources.Load(("HeightmapData/" + data.SerializedFileName + "_Data"), typeof(TextAsset));

            if (data != null && heightmap)
            {
                data.LoadFileData(FileData.text);
                data.floatheightData = GISTerrainLoaderHeightmapSerializer.DeserializeHeightMap(heightmap.bytes, new Vector2(data.mapSize_col_x, data.mapSize_row_y));
            }
            else
            {
                if (debug)
                    Debug.Log("Terrain data not found in GISTech/GIS Terrain Loader/Resources/HeightmapData Folder");
            }

        }
        private void CheckTerrainTextures(GISTerrainLoaderPrefs RuntimePrefs)
        {
            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRamp || RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines || RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.ElevationGrayScaleGradient)
                RuntimePrefs.textureMode = TextureMode.WithoutTexture;

            if (RuntimePrefs.textureMode == TextureMode.WithTexture || RuntimePrefs.textureMode == TextureMode.MultiTexture || RuntimePrefs.textureMode == TextureMode.MultiLayers)
            {
                if (RuntimePrefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                {
                    var c_count = GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(TerrainFilePath);

                    RuntimePrefs.terrainCount = new Vector2Int((int)c_count.x, (int)c_count.y);

                    if (c_count == Vector2.zero)
                    {
                        RuntimePrefs.terrainCount = new Vector2Int(1, 1);
                    }

                }
            }
        }
        private void CheckTerrainMaterials(GISTerrainLoaderPrefs RuntimePrefs)
        {

            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.Standard)
            {
                RuntimePrefs.terrainMaterial = new Material((Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material)));

                if (RuntimePrefs.terrainMaterial == null)
                    Debug.LogError("Standard terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
            }

            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRamp)
            {
                if (RuntimePrefs.textureMode == TextureMode.WithTexture || RuntimePrefs.textureMode == TextureMode.MultiTexture || RuntimePrefs.textureMode == TextureMode.MultiLayers)
                    RuntimePrefs.textureMode = TextureMode.WithoutTexture;

                if (RuntimePrefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    RuntimePrefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                RuntimePrefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapColorRamp", typeof(Material)));

                if (RuntimePrefs.terrainMaterial)
                {
                    RuntimePrefs.terrainMaterial.SetFloat("_TerrainHeight", this.ContainerSize.y);
                    RuntimePrefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                    RuntimePrefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
                }
                else
                    Debug.LogError("HeightmapColorRamp terrain material not found in 'Resources/Materials/TerrainShaders/HeightmapColorRamp' ");
            }

            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.ElevationGrayScaleGradient)
            {
                if (RuntimePrefs.textureMode == TextureMode.WithTexture || RuntimePrefs.textureMode == TextureMode.MultiTexture || RuntimePrefs.textureMode == TextureMode.MultiLayers)
                    RuntimePrefs.textureMode = TextureMode.WithoutTexture;

                if (RuntimePrefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    RuntimePrefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                RuntimePrefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/ElevationGrayScaleGradient", typeof(Material)));
                RuntimePrefs.terrainMaterial.SetFloat("_TerrainHeight", this.ContainerSize.y);
                RuntimePrefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                RuntimePrefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);

                if (RuntimePrefs.terrainMaterial == null)
                    Debug.LogError("ElevationGrayScaleGradient terrain material not found in 'Resources/Materials/TerrainShaders/ElevationGrayScaleGradient' ");
            }

            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines)
            {
                if (RuntimePrefs.textureMode == TextureMode.WithTexture || RuntimePrefs.textureMode == TextureMode.MultiTexture || RuntimePrefs.textureMode == TextureMode.MultiLayers)
                    RuntimePrefs.textureMode = TextureMode.WithoutTexture;

                if (RuntimePrefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    RuntimePrefs.UseTerrainEmptyColor = OptionEnabDisab.Disable;

                RuntimePrefs.terrainMaterial = new Material((Material)Resources.Load("Materials/TerrainShaders/HeightmapContourLines", typeof(Material)));

                if (RuntimePrefs.terrainMaterial)
                {
                    RuntimePrefs.terrainMaterial.SetFloat("_ContourInterval", RuntimePrefs.ContourInterval);
                    RuntimePrefs.terrainMaterial.SetFloat("_TerrainHeight", this.ContainerSize.y);
                    RuntimePrefs.terrainMaterial.SetFloat("_LineWidth", GISTerrainLoaderConstants.LineWidth);
                    RuntimePrefs.terrainMaterial.SetFloat("_Brightness", GISTerrainLoaderConstants.Brightness);
                    RuntimePrefs.terrainMaterial.SetFloat("_Contrast", GISTerrainLoaderConstants.Contrast);
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
        private void FreeUpMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Resources.UnloadUnusedAssets();
        }

        public List<DVector2> GetBoundsCoordinatesAsDVector()
        {
            List<DVector2> TerrainBounds = new List<DVector2>();

            TerrainBounds.Add(data.TLOriginal_Coor);
            TerrainBounds.Add(new DVector2(data.DROriginal_Coor.x, data.TLOriginal_Coor.y));
            TerrainBounds.Add(data.DROriginal_Coor);
            TerrainBounds.Add(new DVector2(data.TLOriginal_Coor.x, data.DROriginal_Coor.y));

            return TerrainBounds;
        }
        public List<GISTerrainLoaderPointGeoData> GetBoundsCoordinatesAsPointGeoData()
        {
            List<GISTerrainLoaderPointGeoData> TerrainBounds = new List<GISTerrainLoaderPointGeoData>();

            TerrainBounds.Add(new GISTerrainLoaderPointGeoData(this.data.TLOriginal_Coor));
            TerrainBounds.Add(new GISTerrainLoaderPointGeoData(new DVector2(this.data.DROriginal_Coor.x, this.data.TLOriginal_Coor.y)));
            TerrainBounds.Add(new GISTerrainLoaderPointGeoData(this.data.DROriginal_Coor));
            TerrainBounds.Add(new GISTerrainLoaderPointGeoData(new DVector2(this.data.TLOriginal_Coor.x, this.data.DROriginal_Coor.y)));

            return TerrainBounds;
        }

        /// <summary>
        /// Resize a terrain to a specific scale 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="Scale"></param>
        public void RescaleTerrain(Vector3 Scale)
        {
            this.SubTerrainSize.x = Scale.x / TerrainCount.x;
            this.SubTerrainSize.z = Scale.z / TerrainCount.y;

            for (int index = 0; index < this.terrains.Length; index++)
            {
                if (index >= terrains.Length)
                {
 
                    return;
                }

                int x = index % TerrainCount.x;
                int y = index / TerrainCount.x;

                var terrain = terrains[x, y];
                terrain.terrainData.size = new Vector3(SubTerrainSize.x, terrain.terrainData.size.y, SubTerrainSize.z);
                terrain.transform.position = new Vector3(SubTerrainSize.x * x, 0, SubTerrainSize.z * y);

                foreach (var layer in terrain.terrainData.terrainLayers)
                {
                    layer.tileSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
                }
            }
        }
        public GISTerrainTile CreateBackgroundTerrainTile(GISTerrainLoaderPrefs Prefs, int x, int y)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.heightmapResolution = Prefs.heightmapResolution;
            tdata.baseMapResolution = Prefs.baseMapResolution;
            tdata.SetDetailResolution(Prefs.detailResolution, Prefs.resolutionPerPatch);
            tdata.size = this.ContainerSize;


            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("BGTile__{0}__{1}", x, y);
            GO.transform.parent = this.gameObject.transform;
            GO.transform.position = new Vector3(this.ContainerSize.x * x, 0, this.ContainerSize.z * y);
            GO.isStatic = false;

            if (Prefs.TerrainLayerSet == OptionEnabDisab.Enable)
                GO.gameObject.layer = Prefs.TerrainLayer;

            GISTerrainTile item = GO.AddComponent<GISTerrainTile>();
            item.Number = new Vector2Int(x, y);
            item.size = this.ContainerSize;
            item.ElevationFilePath = Prefs.TerrainFilePath;

            item.terrain = GO.GetComponent<Terrain>();
            item.terrainData = item.terrain.terrainData;

            item.terrain.heightmapPixelError = Prefs.PixelError;
            item.terrain.basemapDistance = Prefs.BaseMapDistance;
            item.terrain.materialTemplate = Prefs.terrainMaterial;

#if UNITY_EDITOR

            if(!Application.isPlaying)
            {
                string filename = Path.Combine(this.TerrainFilePath, GO.name) + ".asset";

                AssetDatabase.CreateAsset(tdata, filename);

                AssetDatabase.SaveAssets();
            }

#endif
 
            return item;
        }

        public IEnumerator GenerateBackground(GISTerrainLoaderPrefs Prefs, GISTerrainLoaderElevationInfo ElevationInfo)
        {
            if (Prefs.TerrainBackground == OptionEnabDisab.Enable)
            {
                var GeneratedContainer = this;

                var MainTexture = GISTerrainLoaderTextureGenerator.CaptureContainerTexture(GeneratedContainer, Prefs);
                Color32[] SourcePix = MainTexture.GetPixels32();
                SourcePix = GISTerrainLoaderTextureGenerator.AdjustBrightnessContrast(SourcePix, 0.73f);

                GISTerrainLoaderPrefs m_Prefs = new GISTerrainLoaderPrefs();
                m_Prefs.terrainCount = new Vector2Int(1, 1);

                var terrain_Bottom = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, 0, -1);
                terrain_Bottom.container = GeneratedContainer;
                terrain_Bottom.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.Bottom, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_Bottom, false);
                yield return StartCoroutine(ElevationInfo.GenerateHeightMap(m_Prefs, terrain_Bottom, TerrainSide.Bottom).AsIEnumerator());
                yield return new WaitForSeconds(0.001f);

                var terrain_Right = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, 1, 0);
                terrain_Right.container = GeneratedContainer;
                terrain_Right.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.Right, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_Right, false);
                StartCoroutine(ElevationInfo.GenerateHeightMap(m_Prefs, terrain_Right, TerrainSide.Right).AsIEnumerator());
                yield return new WaitForSeconds(0.001f);

                var terrain_Top = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, 0, 1);
                terrain_Top.container = GeneratedContainer;
                terrain_Top.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.Top, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_Top, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_Top, TerrainSide.Top).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);

                //////Co
                var terrain_Left = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, -1, 0);
                terrain_Left.container = GeneratedContainer;
                terrain_Left.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.Left, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_Left, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_Left, TerrainSide.Left).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);

                var terrain_TopRight = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, 1, 1);
                terrain_TopRight.container = GeneratedContainer;
                terrain_TopRight.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.TopRight, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_TopRight, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_TopRight, TerrainSide.TopRight).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);

                var terrain_TopLeft = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, -1, 1);
                terrain_TopLeft.container = GeneratedContainer;
                terrain_TopLeft.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.TopLeft, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_TopLeft, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_TopLeft, TerrainSide.TopLeft).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);

                var terrain_BottomLeft = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, -1, -1);
                terrain_BottomLeft.container = GeneratedContainer;
                terrain_BottomLeft.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.BottomLeft, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_BottomLeft, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_BottomLeft, TerrainSide.BottomLeft).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);

                var terrain_BottomRight = GeneratedContainer.CreateBackgroundTerrainTile(Prefs, 1, -1);
                terrain_BottomRight.container = GeneratedContainer;
                terrain_BottomRight.Number = new Vector2Int(0, 0);
                GISTerrainLoaderTextureGenerator.AddTextureToTerrain(GISTerrainLoaderTextureGenerator.OrientedTexture(SourcePix, TerrainSide.BottomRight, Prefs.TerrainBackgroundTextureResolution, GeneratedContainer.TerrainFilePath), terrain_BottomRight, false);
                ElevationInfo.GenerateHeightMap(m_Prefs, terrain_BottomRight, TerrainSide.BottomRight).AsIEnumerator();
                yield return new WaitForSeconds(0.001f);
            }

            yield return null;
        }

    }

}