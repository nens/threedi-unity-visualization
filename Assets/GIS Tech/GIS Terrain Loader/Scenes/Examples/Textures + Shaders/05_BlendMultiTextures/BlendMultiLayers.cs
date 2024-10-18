using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GISTech.GISTerrainLoader;
using UnityEngine.UI;
using System.Linq;
public class BlendMultiLayers : MonoBehaviour
{
    public Dropdown UILayers;
    public Slider UIOpacitySlider;
    private string TerrainFilePath;

    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;
    private RuntimeTerrainGenerator RuntimeGenerator;

    private float Opacity;
    private float[,,] splat;
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

        UIOpacitySlider.onValueChanged.AddListener(UIOpacitySliderValueChanged);


        //Coroutine To Start Generating terrains
        StartCoroutine(GenerateTerrain(TerrainFilePath));
    }
    private IEnumerator GenerateTerrain(string TerrainPath)
    {
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
        Prefs.textureMode = TextureMode.MultiLayers;
    }
    private void SetUILayers()
    {
        List<string> Layers = GISTerrainLoaderTextureGenerator.GetFullTextureFolders(TerrainFilePath);

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        if (Layers.Count > 0)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                options.Add(new Dropdown.OptionData("Layer_" + i));
            }
            UILayers.ClearOptions();
            UILayers.AddOptions(options);
        }

    }
    private void OnUILayerValueChanged(int value)
    {
        UIOpacitySlider.value = 0;
        Prefs.TextureFolderIndex = value;
    }
    private void UIOpacitySliderValueChanged(float value)
    {
        Opacity = value;

        foreach (var terrain in RuntimeGenerator.GeneratedContainer.terrains)
        {
            var targetTerrain = terrain.terrain;

            GetTerrainCoordinates(targetTerrain);
        }
    }
    private void OnDisable()
    {
        UILayers.onValueChanged.RemoveListener(OnUILayerValueChanged);
        UIOpacitySlider.onValueChanged.RemoveListener(UIOpacitySliderValueChanged);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var terrain in RuntimeGenerator.GeneratedContainer.terrains)
            {
               var targetTerrain = terrain.terrain;

                GetTerrainCoordinates(targetTerrain);
            }
        }
    }
 
 
    public int areaOfEffectSize = 100; // size of the brush
 
 
    private void GetTerrainCoordinates(Terrain targetTerrain )
    {
        splat = targetTerrain.terrainData.GetAlphamaps(0, 0, targetTerrain.terrainData.alphamapWidth, targetTerrain.terrainData.alphamapHeight);

        for (int xx = 0; xx < targetTerrain.terrainData.alphamapWidth; xx++)
        {
            for (int yy = 0; yy < targetTerrain.terrainData.alphamapHeight; yy++)
            {
                float[] weights = new float[targetTerrain.terrainData.alphamapLayers];

                for (int zz = 0; zz < splat.GetLength(2); zz++)
                {

                    weights[zz] = splat[xx, yy, zz];
                }
                weights[Prefs.TextureFolderIndex] += Opacity * 2; 
                                                
                float sum = weights.Sum();

                for (int ww = 0; ww < weights.Length; ww++)
                {
                    weights[ww] /= sum;
                    splat[xx, yy, ww] = weights[ww];
                }
            }
        }
 
        targetTerrain.terrainData.SetAlphamaps(0, 0, splat);
         targetTerrain.Flush();
    }
 
   
  
 
  
  
}
