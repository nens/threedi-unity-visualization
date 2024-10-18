using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPX_Example : MonoBehaviour
{
    private string TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/GPXTerrain/GPXTerrain.tif";
    private RuntimeTerrainGenerator RuntimeGenerator;
    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;
    // Start is called before the first frame update
    void Start()
    {
        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        Prefs = RuntimePrefs.Prefs;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;

        StartCoroutine(GenerateTerrain(TerrainFilePath));

    }

    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.enabled = true;
        Prefs.TerrainFilePath = TerrainPath;
        Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;

        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;

        Prefs.heightmapResolution = 513;
        Prefs.textureMode = TextureMode.WithTexture;

        //Select GPX as Vector Type
        Prefs.vectorType = VectorType.GPX;

        //Enable Road Generation
        Prefs.EnableRoadGeneration = OptionEnabDisab.Enable;
        var GPXPathPrefab = (GISTerrainLoaderSO_Road)Resources.Load("Prefabs/Environment/Roads/GPX_GeoLine_Model", typeof(GISTerrainLoaderSO_Road));
        Prefs.PathPrefab = GPXPathPrefab;

        //Enable WayPoints Generation
        Prefs.EnableGeoPointGeneration = OptionEnabDisab.Enable;
        var GeoPointPrefab = (GameObject)Resources.Load("Prefabs/Environment/GeoPoints/PointPrefabs/GeoLocation", typeof(GameObject));
        Prefs.GeoPointPrefab = GeoPointPrefab;



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
}
