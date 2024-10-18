/*     Unity GIS Tech 2020-2023      */

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderVectorParser 
    {
        /// <summary>
        /// Load Vector Data and Convert them to a list of GISTerrainLoaderGeoVectorData
        /// </summary>
        /// <param name="Prefs"></param>
        /// <param name="GeneratedContainer"></param>
        /// <returns></returns>
        public static List<GISTerrainLoaderGeoVectorData> LoadVectorFiles(GISTerrainLoaderPrefs Prefs,  GISTerrainContainer GeneratedContainer = null)
        {
            List<GISTerrainLoaderGeoVectorData> LoadedGeoData = new List<GISTerrainLoaderGeoVectorData>();
            
            var AllVectorData = GISTerrainLoaderVectorExtensions.GetAllVectorFiles(Prefs.TerrainFilePath, Prefs.vectorType);

            if(AllVectorData.Count>0)
            {
                foreach (var vectorfile in AllVectorData)
                {
                    string filePath = vectorfile.Key;

                    VectorType fileType = vectorfile.Value;

                    GISTerrainLoaderGeoVectorData GeoData = new GISTerrainLoaderGeoVectorData();

                    switch (fileType)
                    {
                        case VectorType.OpenStreetMap:

                            GISTerrainLoaderOSMFileLoader osmloader = new GISTerrainLoaderOSMFileLoader(filePath);

                            GeoData = osmloader.GetGeoFiltredData(filePath, GeneratedContainer);

                            if (!GeoData.IsEmptyGeoData())
                                LoadedGeoData.Add(GeoData);

                            break;

                        case VectorType.ShapeFile:

                            GISTerrainLoaderShpFileHeader shape = GISTerrainLoaderShapeReader.LoadShape(filePath);
                            GISTerrainLoaderShapeFileLoader shapeloader = new GISTerrainLoaderShapeFileLoader(shape);

                            GeoData = shapeloader.GetGeoFiltredData(shape.FilePath, GeneratedContainer);

                            if (!GeoData.IsEmptyGeoData())
                                LoadedGeoData.Add(GeoData);
 
                            break;

                        case VectorType.GPX:

                            GISTerrainLoaderGPXLoader LoadGPXFile = new GISTerrainLoaderGPXLoader(filePath);
                            GeoData = LoadGPXFile.GetGeoFiltredData("", GeneratedContainer);

                            if (!GeoData.IsEmptyGeoData())
                                LoadedGeoData.Add(GeoData);

                            break;

                        case VectorType.KML:

                            GISTerrainLoaderKMLLoader KMLFile = new GISTerrainLoaderKMLLoader(filePath);
                            GeoData = KMLFile.GetGeoFiltredData(filePath, GeneratedContainer);

                            if (!GeoData.IsEmptyGeoData())
                                LoadedGeoData.Add(GeoData);

                            break;

                        case VectorType.Geojson:
#if GISTerrainLoaderGeoJson
                            GISTerrainLoaderGEOJSONLoader GeojsonFile = new GISTerrainLoaderGEOJSONLoader(filePath);
                            GeoData = GeojsonFile.GetGeoFiltredData(filePath, GeneratedContainer);
                           
                            if (!GeoData.IsEmptyGeoData())
                                LoadedGeoData.Add(GeoData);
                            break;
#else
                            Debug.LogError("GeoJson Lib not installed ...");
                            break;
#endif


                    }
                }
            }

            return LoadedGeoData;
        }
    }
}