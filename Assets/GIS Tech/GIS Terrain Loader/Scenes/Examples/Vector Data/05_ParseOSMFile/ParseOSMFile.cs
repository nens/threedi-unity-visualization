using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GISTech.GISTerrainLoader;
using UnityEngine.UI;
using System.Linq;

public class ParseOSMFile : MonoBehaviour
{
    public Text TextData;
    private string OSMFilePath = Application.streamingAssetsPath + @"/GIS Terrains/Example_VectorData/Desert_VectorData/OSM_Cuenca.osm";
    private GISTerrainLoaderPrefs Prefs = new GISTerrainLoaderPrefs();

    void Start()
    {
        StartCoroutine(LoadOSMFile());
    }
    private IEnumerator LoadOSMFile()
    {
        InitializingRuntimePrefs(Prefs);

        GISTerrainLoaderOSMFileLoader osmloader;

        if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var FileData = new byte[0];

            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(Prefs.TerrainFilePath, (Fdata) =>
            {
                FileData = Fdata;

            }));

            osmloader = new GISTerrainLoaderOSMFileLoader(FileData);
        }
        osmloader = new GISTerrainLoaderOSMFileLoader(Prefs.TerrainFilePath);


        if (osmloader!= null)
        ParseData(osmloader);
    }

    private void InitializingRuntimePrefs(GISTerrainLoaderPrefs m_Prefs)
    {
        //Load GTL Settings
        m_Prefs.LoadSettings();
        m_Prefs.TerrainFilePath = OSMFilePath;
    }
    private void ParseData(GISTerrainLoaderOSMFileLoader osmloader)
    {
        GISTerrainLoaderGeoVectorData GeoData = osmloader.GetGeoFiltredData("ParseOSMDemo");
        TextData.text += '\n' + " OSM File Loaded Correctly : ";
        int PointsCount = GeoData.GeoPoints.Count;
        int LinesCount = GeoData.GeoLines.Count;
        int PolygonsCount = GeoData.GeoPolygons.Count;
        TextData.text += '\n' + " Number Of Points: "+ PointsCount  + ", Lines : " + LinesCount + ", Polygons : " + PolygonsCount ;

        // Ex: Get All Building With Key ="buildings" and value = "residential"
        //GeoVectorType is used to avoid filtring points and lines because building is a polygon
        GISTerrainLoaderGeoVectorData ResidentialBuilding = GeoData.GetVectorDataByKeyValue("building", "residential",GeoVectorType.Polygon);
        TextData.text += '\n' + "Number of Buildings with 'residential' Tag : " + ResidentialBuilding.GeoPolygons.Count;

        for(int i = 0; i < ResidentialBuilding.GeoPolygons.Count; i++)
        {
            var building = ResidentialBuilding.GeoPolygons[i];

            TextData.text += '\n' + " building id : " + building .ID;
        }

 
    }
}
