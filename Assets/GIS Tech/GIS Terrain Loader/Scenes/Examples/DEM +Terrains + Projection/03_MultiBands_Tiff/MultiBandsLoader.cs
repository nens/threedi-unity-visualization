using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class MultiBandsLoader : MonoBehaviour
{
    public Text data;
    private string MultibandTiffPath = Application.streamingAssetsPath + @"/GIS Terrains/Example_Tiff_MultiBands/MultiBands_Tiff.tif";
    private DVector2 LatLonPosition = new DVector2(-119.9581587375, 38.9406188940);

    private GISTerrainLoaderPrefs Prefs = new GISTerrainLoaderPrefs();
    void Start()
    {
        StartCoroutine(LoadTiffBands());
    }

    private IEnumerator LoadTiffBands()
    {
        GISTerrainLoaderTiffMultiBands TiffBandsData = null;
 
        InitializingRuntimePrefs(Prefs);

        if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var FileData = new byte[0];

            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(Prefs.TerrainFilePath, (Fdata) =>
            {
                FileData = Fdata;

            }));

            TiffBandsData = GISTerrainLoaderTIFFLoader.LoadTiffBands(Prefs, FileData);

            ParseData(TiffBandsData);
        }
        else
        {
            TiffBandsData = GISTerrainLoaderTIFFLoader.LoadTiffBands(Prefs);
            ParseData(TiffBandsData);
        }
    }
    private void InitializingRuntimePrefs(GISTerrainLoaderPrefs Prefs)
    {
        //Load GTL Settings
        Prefs.LoadSettings();
        //Set File Path To Prefs Class
        Prefs.TerrainFilePath = MultibandTiffPath;
        //File Projected in WGS84 EPSG : 4326, we don't DotSpatialLib 
        Prefs.projectionMode = ProjectionMode.Geographic;

    }
    private void ParseData(GISTerrainLoaderTiffMultiBands RasterBands)
    {
        //Load all data in the different bands 
        var Values = RasterBands.GetValues(RasterBands.BandsNumber, LatLonPosition);
        for (int band = 0; band < Values.Length; band++)
        {
            var value = Values[band];
            var m_text = "Value For Band N : " + band + "  " + value;
            data.text += '\n' + m_text;
            Debug.Log(m_text);
        }

        //Read data from one bands By Lat/Lon Position
        int BandNumber = 7;
        var BValue = RasterBands.GetValue(BandNumber, LatLonPosition);

        var n_text = "Value For Band N : " + BandNumber + "  " + BValue;
        data.text += '\n' + n_text;
        Debug.Log(n_text);

        //Read Data By Row and Col Number and Band Number
        var Row = 194; var Col = 305;
        var Value = RasterBands.GetValue(BandNumber, Row, Col);

        n_text = "Band : " + BandNumber + "  " + Value;
        data.text += '\n' + n_text;
        Debug.Log(n_text);
    }

}
