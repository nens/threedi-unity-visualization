using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWebGLTerrain : MonoBehaviour
{
    private string TerrainFilePath;

 
    private RuntimeTerrainGenerator RuntimeGenerator ;
    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;

    // Start is called before the first frame update
    void Start()
    {

        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Coordinates/Coordinates.tif";

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        Prefs = RuntimePrefs.Prefs;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;

        GenerateTerrain(TerrainFilePath);
    }

    void Update()
    {
 
    }
    private void GenerateTerrain(string TerrainPath)
    {
        InitializingRuntimePrefs(TerrainPath);
        StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));

    }
    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.enabled = true;
        Prefs.TerrainFilePath = TerrainPath;
        Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;

        //Load Real Terrain elevation values
        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;

        Prefs.heightmapResolution = 1025;
        Prefs.textureMode = TextureMode.WithTexture;
    }
}
