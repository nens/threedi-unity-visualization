using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GISTech.GISTerrainLoader;
public class GenerateSimpleTerrain : MonoBehaviour
{
    private string TerrainFilePath;

    private RuntimeTerrainGenerator RuntimeGenerator;

    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;

    void Start()
    {
        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_SHP/Cuenca.tif";

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        Prefs = RuntimePrefs.Prefs;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;
 
        StartCoroutine(GenerateTerrain(TerrainFilePath));
    }
    private IEnumerator GenerateTerrain(string TerrainPath)
    {
        yield return new WaitForSeconds(2f);
 
            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {

            InitializingRuntimePrefs(TerrainPath);
            StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
        }else
        {
            if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
            {
                InitializingRuntimePrefs(TerrainPath);

                StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
            }
            else
            {
                Debug.LogError("Terrain file null or not supported.. Try againe");
                yield return null;
            }
        }

    }
    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.enabled = true;
        Prefs.TerrainFilePath = TerrainPath;
        Prefs.RemovePrvTerrain =  OptionEnabDisab.Enable;

        //Load Real Terrain elevation values
        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        Prefs.heightmapResolution = 65;
        Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
        Prefs.terrainMaterialMode = TerrainMaterialMode.Standard;

        Prefs.vectorType = VectorType.OpenStreetMap;
        Prefs.EnableRoadGeneration = OptionEnabDisab.Enable;
        Prefs.EnableBuildingGeneration = OptionEnabDisab.Enable;
        Prefs.EnableTreeGeneration = OptionEnabDisab.Enable;
    }

 
}
