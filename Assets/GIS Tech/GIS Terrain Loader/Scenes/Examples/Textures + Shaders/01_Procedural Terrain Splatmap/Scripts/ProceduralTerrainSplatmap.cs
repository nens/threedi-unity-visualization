/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace GISTech.GISTerrainLoader
{
    // In this demo example we will load/Generate terrain from
    // StreamingAsset folder, and we will set some splatmaps to terrain at Runtime 

    public class ProceduralTerrainSplatmap : MonoBehaviour
    {
 
        private string TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/PNG Demo/ASTER30m.png";

        private RuntimeTerrainGenerator RuntimeGenerator;

        private GISTerrainLoaderPrefs Prefs;
        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        void Start()
        {

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
            Prefs = RuntimePrefs.Prefs;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;
 
            StartCoroutine(GenerateTerrain(TerrainFilePath));
        }
 
        private IEnumerator GenerateTerrain(string TerrainPath)
        {
            yield return new WaitForSeconds(2f);

            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                if (string.IsNullOrEmpty(TerrainPath) || !System.IO.File.Exists(TerrainPath))
                {
                    Debug.LogError("Terrain file null or not supported.. Try againe");
                    yield break;
                }
            }

            InitializingRuntimePrefs(TerrainPath);

            StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));

        }
        private void InitializingRuntimePrefs(string TerrainPath)
        {
            RuntimeGenerator.enabled = true;
            Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;
            Prefs.TerrainFilePath = TerrainFilePath;
            //Load Real Terrain elevation values
            Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;

            //Note that GTL Can not Detect Real PNG dimensions so we need to set them manually
            Prefs.terrainDimensionMode = TerrainDimensionsMode.Manual;
            Prefs.TerrainDimensions = new DVector2(2, 2);

            Prefs.heightmapResolution = 513;
            Prefs.textureMode = TextureMode.Splatmapping;
            //Set the number of terrain tiles
            Prefs.terrainCount = new Vector2Int(1, 1);


            //Splatmap parameters 
            Prefs.Slope = 0.1f;
            Prefs.MergeRadius = 20;
            Prefs.MergingFactor = 2;
 
            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
               StartCoroutine(AddSplatmapsToTerrainWebData(TerrainPath));
            }else
            {
                AddSplatmapsToTerrain(TerrainPath);
            }


        }
        private void AddSplatmapsToTerrain(string TerrainPath)
        {
            //Directory so Splatmaps folder
            var terrainName = Path.GetFileName(TerrainPath).Split('.')[0];
            //Get SplatMap folder
            var splatmappsFolder = Path.GetDirectoryName(TerrainPath) + "/"+terrainName+"_Splatmap";
 
            var BaseTerrainTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/Sand.jpg", TextureWrapMode.Repeat);
            var BaseTerrainTexture_NormalMap = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/SandNormal.png", TextureWrapMode.Repeat);
            var BaseTerrainTexture_Size = new Vector2Int(100, 100);
            var baseLayer = new GISTerrainLoaderTerrainLayer(BaseTerrainTexture_Diffuse, BaseTerrainTexture_NormalMap, BaseTerrainTexture_Size);


            var GrassTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/Grass.png", TextureWrapMode.Repeat);
            var GrassTexture_NormalMap = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/GrassNormal.png", TextureWrapMode.Repeat);
            var GrassTerrainTexture_Size = new Vector2Int(100, 100);
            var GrassLayer = new GISTerrainLoaderTerrainLayer(GrassTexture_Diffuse, GrassTexture_NormalMap, GrassTerrainTexture_Size);


            var CliffATexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/CliffA.jpg", TextureWrapMode.Repeat);
            var CliffATerrainTexture_Size = new Vector2Int(300, 300);
            var CliffALayer = new GISTerrainLoaderTerrainLayer(CliffATexture_Diffuse, null, CliffATerrainTexture_Size);

            var CliffBTexture_Diffuse = GISTerrainLoaderTextureGenerator.LoadedTextureTile(splatmappsFolder + "/CliffB.jpg", TextureWrapMode.Repeat);
            var CliffBTerrainTexture_Size = new Vector2Int(300, 300);
            var CliffBLayer = new GISTerrainLoaderTerrainLayer(CliffBTexture_Diffuse, null, CliffBTerrainTexture_Size);


            Prefs.BaseTerrainLayers = baseLayer;
            Prefs.TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
            Prefs.TerrainLayers.Add(GrassLayer);
            Prefs.TerrainLayers.Add(CliffALayer);
            Prefs.TerrainLayers.Add(CliffBLayer);

            GISTerrainLoaderSplatMapping.DistributingHeights(Prefs.TerrainLayers);
            Prefs.TerrainLayers[0].X_Height = 0.1f;

        }
        private IEnumerator AddSplatmapsToTerrainWebData(string TerrainPath)
        {
            //Directory so Splatmaps folder
            var terrainName = Path.GetFileName(TerrainPath).Split('.')[0];
             //Get SplatMap folder
            var splatmappsFolder = Path.GetDirectoryName(TerrainPath) + "/" + terrainName + "_Splatmap";
 
            var BaseTerrainTextureDiffuse_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/Sand.jpg");
            Texture2D BaseTerrainTexture_Diffuse = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(BaseTerrainTextureDiffuse_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                BaseTerrainTexture_Diffuse = m_texture;
            }));

            var BaseTerrainTextureNormalMap_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/SandNormal.jpg");
            Texture2D BaseTerrainTexture_NormalMap = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(BaseTerrainTextureNormalMap_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                BaseTerrainTexture_NormalMap = m_texture;
            }));

            var BaseTerrainTexture_Size = new Vector2Int(100, 100);
            var baseLayer = new GISTerrainLoaderTerrainLayer(BaseTerrainTexture_Diffuse, BaseTerrainTexture_NormalMap, BaseTerrainTexture_Size);


            var GrassTexture_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/Grass.png");
            Texture2D GrassTexture_Diffuse = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(GrassTexture_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                GrassTexture_Diffuse = m_texture;
            }));

            var GrassTextureNormalMap_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/GrassNormal.png");
            Texture2D GrassTexture_NormalMap = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(GrassTextureNormalMap_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                GrassTexture_NormalMap = m_texture;
            }));


            var GrassTerrainTexture_Size = new Vector2Int(100, 100);
            var GrassLayer = new GISTerrainLoaderTerrainLayer(GrassTexture_Diffuse, GrassTexture_NormalMap, GrassTerrainTexture_Size);

            var CliffATextureDiffuse_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/CliffA.jpg");
            Texture2D CliffATexture_Diffuse = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(CliffATextureDiffuse_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                CliffATexture_Diffuse = m_texture;
            }));

            var CliffATerrainTexture_Size = new Vector2Int(300, 300);
            var CliffALayer = new GISTerrainLoaderTerrainLayer(CliffATexture_Diffuse, null, CliffATerrainTexture_Size);


            var CliffBTextureDiffuse_Path = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, Path.GetDirectoryName(TerrainPath), terrainName + "_Splatmap", "/CliffB.jpg");
            Texture2D CliffBTexture_Diffuse = null;
            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadTexture(CliffBTextureDiffuse_Path, (m_texture) =>
            {
                m_texture.wrapMode = TextureWrapMode.Repeat;
                CliffBTexture_Diffuse = m_texture;
            }));

            var CliffBTerrainTexture_Size = new Vector2Int(300, 300);
            var CliffBLayer = new GISTerrainLoaderTerrainLayer(CliffBTexture_Diffuse, null, CliffBTerrainTexture_Size);


            Prefs.BaseTerrainLayers = baseLayer;
            Prefs.TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
            Prefs.TerrainLayers.Add(GrassLayer);
            Prefs.TerrainLayers.Add(CliffALayer);
            Prefs.TerrainLayers.Add(CliffBLayer);

            GISTerrainLoaderSplatMapping.DistributingHeights(Prefs.TerrainLayers);
            Prefs.TerrainLayers[0].X_Height = 0.1f;

            yield return null;
        }
    }
}