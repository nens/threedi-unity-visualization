using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GIS Terrain Loader Pro
/// This tutorial shows how to use the MultiTexture option for a terrain that has many raster data Ex: (Imagery,Topo, Terrain ... etc)
/// </summary>
public class SelectTextureLayer : MonoBehaviour
{
    public Dropdown UILayers;
    private string TerrainFilePath;

    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;
    private RuntimeTerrainGenerator RuntimeGenerator;

    void Start()
    {
        //Path to the DEM File
        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/MultiTextures/TIFF.tif";

        //Ref to RuntimeTerrainGenerator.cs Script
        RuntimeGenerator = RuntimeTerrainGenerator.Get;
        //Ref to GISTerrainLoaderRuntimePrefs.cs Script
        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

        //Set GISTerrainLoaderPrefs to GISTerrainLoaderRuntimePrefs script
        Prefs = RuntimePrefs.Prefs;

        UILayers.onValueChanged.AddListener(OnUILayerValueChanged);
        SetUILayers();

        //Coroutine To Start Generating terrains
        StartCoroutine(GenerateTerrain(TerrainFilePath));
    }
    private IEnumerator GenerateTerrain(string TerrainPath)
    {
        yield return new WaitForSeconds(0.5f);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (string.IsNullOrEmpty(TerrainPath) || !System.IO.File.Exists(TerrainPath))
            {
                Debug.LogError("Terrain file null or not supported.. Try againe");
                yield break;
            }
        }

        InitializingRuntimePrefs();

        StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));

    }
    private void InitializingRuntimePrefs()
    {
        //Enable RuntimeGenerator if it's not enabled
        RuntimeGenerator.enabled = true;
        //Set the path of the DEM file to GISTerrainLoaderPrefs 
        Prefs.TerrainFilePath = TerrainFilePath;
        //Enable it to Remove any GIS terrains already generated 
        Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;
        //We are loading a GeoTiff DEM File and we need a to read a real Terrain elevation values
        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        //We are loading a GeoTiff DEM File and we need a to read the real world terrain dimensions
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        //Set the scale to default Vector3(1, 1, 1)
        Prefs.terrainScale = new Vector3(1, 1, 1);
        //The DEM file Projected in Geographic WGS84 EPSG= 4326 so we can set the projection mode to geographic or AutoDetection
        Prefs.projectionMode = ProjectionMode.Geographic;
        //Terrain Tiles Heightmap resolution
        Prefs.heightmapResolution = 257;
 
        //Enable this to add textures to terrain
        Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
        //Set Terrain Texture to Mutli-Texture Mode
        Prefs.textureMode = TextureMode.MultiTexture;
    }
    private void SetUILayers()
    {
        //Read the number of Raster layers add it to UI Dropdown
        List<string> Layers = GISTerrainLoaderTextureGenerator.GetFullTextureFolders(TerrainFilePath);
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        if(Layers.Count>0)
        {
            for (int i=0; i< Layers.Count;i++)
            {
                options.Add(new Dropdown.OptionData("Layer_" + i));
            }
            UILayers.ClearOptions();
            UILayers.AddOptions(options);
        }

    }
    private void OnUILayerValueChanged(int value)
    {
        Prefs.TextureFolderIndex = value;

        StartCoroutine(RuntimeGenerator.GeneratedContainer.GenerateTextures(Prefs, true));
    }
    private void OnDisable()
    {
        UILayers.onValueChanged.RemoveListener(OnUILayerValueChanged);
    }
}
