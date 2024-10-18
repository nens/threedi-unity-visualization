using System.Collections;
using UnityEngine;
using GISTech.GISTerrainLoader;
using UnityEngine.UI;

/// <summary>
/// This Tutorial Show How to Load a Vector data without using RuntimeTerrainGenerator 
/// use it if you want to generate road,building ... from vector data 
/// </summary>
public class LoadVectorData : MonoBehaviour
{
    public VectorType vectorType;

    public KeyCode UpdateKey;

    public bool LoadTexture = false;

    public GISTerrainContainer container;
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            StartCoroutine(UpdateVecotrData());
    }
 
    void Update()
    {
        if (Input.GetKeyDown(UpdateKey))
            StartCoroutine(UpdateVecotrData());
    }
    public IEnumerator UpdateVecotrData()
    {
        GISTerrainLoaderPrefs Prefs = new GISTerrainLoaderPrefs();
        Prefs.LoadSettings();

        Prefs.TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_VectorData/Desert.tif";
        Prefs.textureMode = TextureMode.WithTexture;
        ////Set VectorType to OSM 
        Prefs.vectorType = vectorType;
        //Enable Road Generator
        Prefs.EnableRoadGeneration = OptionEnabDisab.Enable;
        Prefs.EnableTreeGeneration = OptionEnabDisab.Enable;
        Prefs.EnableBuildingGeneration = OptionEnabDisab.Enable;

        //Call GenerateTextures to Start generating Raster Data
        if (LoadTexture)
        {
            Prefs.textureMode = TextureMode.WithTexture;
            yield return StartCoroutine(container.GenerateTextures(Prefs, true));
        }

        yield return new WaitForSeconds(2f);
        //Call GenerateVectorData to Start generating Vector Data
        yield return StartCoroutine(container.GenerateVectorData(Prefs));
    }

    private void OnDisable()
    {
        TreeInstance[] originalTree = new TreeInstance[0];
        foreach (var terrain in container.terrains)
        {
            terrain.terrainData.treeInstances = originalTree;
        }
    }
}
