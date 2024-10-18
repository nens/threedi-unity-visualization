using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GISTech.GISTerrainLoader;

/// <summary>
/// Use this Script to Snap a terrain into a specific position
/// Sub Terrain: is a small part of the main terrain
/// Create/Spawn new small terrain and set its position to right placed according to the real world coordinates 
/// To test the script ( Generate a terrain from 'Example_SRTM30/Desert.tif' and SubTerrain from 'Example_SubDesert/SubTerrain.tif')
/// Files exist in StreamingAssets folder.
/// </summary>
public class SetTerrainPosition : MonoBehaviour
{
    public GISTerrainContainer MainTerrain;
    public GISTerrainContainer SubTerrain;


    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;
    private RuntimeTerrainGenerator RuntimeGenerator;

    private string MainTerrainFilePath;
    private string SubTerrainFilePath;
    // Start is called before the first frame update
    void Start()
    {
        //Path to the DEM File
        MainTerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_SRTM30/Desert.tif";
        //Path to the DEM File
        SubTerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_SubDesert/SubTerrain.tif";


        //Ref to RuntimeTerrainGenerator.cs Script
        RuntimeGenerator = RuntimeTerrainGenerator.Get;
        //Ref to GISTerrainLoaderRuntimePrefs.cs Script
        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        //Set GISTerrainLoaderPrefs to GISTerrainLoaderRuntimePrefs script
        Prefs = RuntimePrefs.Prefs;


        StartCoroutine(GenerateTerrains());

    }

    private IEnumerator GenerateTerrains()
    {
        //generate main terrain
        InitializingRuntimePrefs(Prefs, MainTerrainFilePath,513);
        yield return StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
        MainTerrain = RuntimeGenerator.GeneratedContainer;

        //generate sub terrain
        InitializingRuntimePrefs(Prefs, SubTerrainFilePath, 1025);
        yield return StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
        SubTerrain = RuntimeGenerator.GeneratedContainer;


        //Snap sub terrain to the right position
        //Get a List of the main terrain bounds  
        var SubContainer_LB = SubTerrain.GetBoundsCoordinatesAsDVector();
        //SubContainer_LB [3] is the origin of the terrain 
        DVector2 SetPotsition_LB = SubContainer_LB[3];
        //Set the sub terrain container position to the right place according to its origin and the main terrain container
        SubTerrain.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(MainTerrain, SetPotsition_LB);


    }
    private void InitializingRuntimePrefs(GISTerrainLoaderPrefs Prefs, string DEMpath, int heightmapResolution)
    {
        //Set the path of the DEM file to GISTerrainLoaderPrefs 
        Prefs.TerrainFilePath = DEMpath;
        //Enable it to Remove any GIS terrains already generated 
        Prefs.RemovePrvTerrain = OptionEnabDisab.Disable;
        //We are loading a GeoTiff DEM File and we need a to read a real Terrain elevation values
        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        //We are loading a GeoTiff DEM File and we need a to read the real world terrain dimensions
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        //Set the scale to default Vector3(1, 1, 1)
        Prefs.terrainScale = new Vector3(1, 1, 1);
        //The DEM file Projected in Geographic WGS84 EPSG= 4326 so we can set the projection mode to geographic or AutoDetection
        Prefs.projectionMode = ProjectionMode.Geographic;
        //Terrain Tiles Heightmap resolution
        Prefs.heightmapResolution = heightmapResolution;

        //Enable this to add textures to terrain
        Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
 
        Prefs.textureMode = TextureMode.WithTexture;
    }

}
