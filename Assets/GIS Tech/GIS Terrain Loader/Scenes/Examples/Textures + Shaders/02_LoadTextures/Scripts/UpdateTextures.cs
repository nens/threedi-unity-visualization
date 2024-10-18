using System.Collections;
using UnityEngine;
using GISTech.GISTerrainLoader;

/// <summary>
/// This Tutorial Show How to texture a terrain at runtime without using the RuntimeTerrainGenerator 
/// use it  if you want to generate splatmaps or shaded relief and come back to real world texture ... verso
/// </summary>
public class UpdateTextures : MonoBehaviour
{
    public TextureMode texturemode;

    public KeyCode UpdateKey;

    public GISTerrainContainer container;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(UpdateKey))
            StartCoroutine(UpdateRasterData());
    }
    public IEnumerator UpdateRasterData()
    {

        GISTerrainLoaderPrefs Prefs = new GISTerrainLoaderPrefs();
        //Load GTL Settings
        Prefs.LoadSettings();

        Prefs.TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/UTM-NAD83/Tiff.tif";
        ////Set TextureMode to With Texture
        Prefs.textureMode = texturemode;
        Prefs.TerrainShaderType = ShaderType.ColorRamp;
        Prefs.UnderWaterShader = OptionEnabDisab.Disable;

        //Call GenerateTextures to Start generating Raster Data
        yield return StartCoroutine(container.GenerateTextures(Prefs, true));


    }

}
