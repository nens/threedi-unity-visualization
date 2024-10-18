using UnityEngine;
using System.IO;
using GISTech.GISTerrainLoader;
using System.Collections;
using UnityEngine.UI;
public class LoadShapeFileData : MonoBehaviour
{
    public Text data;
    string ShapeFilePath = Application.streamingAssetsPath + @"/GIS Terrains/Example_SHP/ParseSHP/Roads.shp";

    void Start()
    {
        StartCoroutine(LoadShapeFile(ShapeFilePath));
    }
    private IEnumerator LoadShapeFile(string TerrainPath)
    {
        yield return new WaitForSeconds(2f);

        GISTerrainLoaderGeoVectorData GeoData = null;
        GISTerrainLoaderShpFileHeader shpfile = null;
        GISTerrainLoaderShapeFileLoader shapeloader = null;

        if(Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var ShpData = new byte[0];
            var ProjData = new byte[0];
            var DBFData = new byte[0];

            string dbfpath = Path.ChangeExtension(ShapeFilePath, ".dbf");
            string ProjPath = Path.ChangeExtension(ShapeFilePath, ".prj");

            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(ShapeFilePath, (data) =>
            {
                ShpData = data;

            }));

            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(dbfpath, (data) =>
            {
                DBFData = data;
            }));

            yield return StartCoroutine(GISTerrainLoaderPlatformHelper.LoadFileBytes(ProjPath, (data) =>
            {
                ProjData = data;
            }));

            shpfile = GISTerrainLoaderShapeReader.LoadFile(ShpData, ShapeFilePath) as GISTerrainLoaderShpFileHeader;

            shapeloader = new GISTerrainLoaderShapeFileLoader(shpfile, DBFData, ProjData);

            GeoData = shapeloader.GetGeoFiltredData(ShapeFilePath);
        }
        else
        {
            shpfile = GISTerrainLoaderShapeReader.LoadFile(ShapeFilePath) as GISTerrainLoaderShpFileHeader;
            shapeloader = new GISTerrainLoaderShapeFileLoader(shpfile);
            GeoData = shapeloader.GetGeoFiltredData("");
        }

        Debug.Log("Polygons Counts : " + GeoData.GeoPolygons.Count + " Lines Counts : " + GeoData.GeoLines.Count+  " Points Counts : " + GeoData.GeoPoints.Count);

        DebugShapeFileData(GeoData);
    }
    private void DebugShapeFileData(GISTerrainLoaderGeoVectorData GeoData)
    {
        if (GeoData.GeoPoints.Count > 0)
        {
            foreach (var point in GeoData.GeoPoints)
            {
                Debug.Log("GeoPoint ID: " + point.ID + " Lat-Lon: " + point.GeoPoint);
            }
        }

        //We Can Also Debug Shape Elevation Geo-Poly(Line - gone ) Z 

        if (GeoData.GeoLines.Count > 0)
        {
            foreach (var Poly in GeoData.GeoLines)
            {
                var text = "PolyLine ID: " + Poly.ID + " Points Count : " + Poly.GeoPoints.Count;
                data.text += '\n' + text;
                Debug.Log(text);

                //Debug Lat-Lon Points
                foreach (var point in Poly.GeoPoints)
                {
                    text = "PolyLine ID: " + Poly.ID + " Point N " + Poly.GeoPoints.IndexOf(point) + " Lat-Lon : " + point.GeoPoint.x + " - " + point.GeoPoint.y + " Elevation " + point.Elevation;
                    data.text += '\n' + text;
                    Debug.Log(text);
                }

                //Debug PolyLine DataBase
                foreach (var Bdata in Poly.DataBase)
                {
                    text = "PolyLine ID: " + Poly.ID + " Attribute: " + Bdata.Key + " Value: " + Bdata.Value;
                    data.text += '\n' + text;
                    Debug.Log(text);
                }
            }
        }

        //Debug Geo-Polygons Geo-Points Data

        if (GeoData.GeoPolygons.Count > 0)
        {
            foreach (var Poly in GeoData.GeoPolygons)
            {
                var text = "Polygon ID: " + Poly.ID + " Points Count : " + Poly.GeoPoints.Count;
                data.text += '\n' + text;
                Debug.Log(text);
 

                //Debug Lat-Lon Points
                foreach (var point in Poly.GeoPoints)
                {
                    text = "Polygon ID: " + Poly.ID + " Point N " + Poly.GeoPoints.IndexOf(point) + " Lat-Lon : " + point;
                    data.text += '\n' + text;
                    Debug.Log(text);
 
                }

                //Debug Polygon DataBase
                foreach (var Bdata in Poly.DataBase)
                {
                    text = "Polygon ID: " + Poly.ID + " Attribute: " + Bdata.Key + " Value: " + Bdata.Value;
                    data.text += '\n' + text;
                    Debug.Log(text);
 
                }
            }
        }
    }

}

