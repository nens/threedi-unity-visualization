/*     Unity GIS Tech 2020-2023      */

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderDataFilter 
    {
 
        private static HashSet<string> PointsAlreadyAdded = new HashSet<string>();
        private static HashSet<string> LinesAlreadyAdded = new HashSet<string>();
        private static HashSet<string> PolygoneAlreadyAdded = new HashSet<string>();
 
        public static List<GISTerrainLoaderPointGeoData> GetGeoVectorPointsData(GISTerrainLoaderGeoVectorData GeoData, List<string> FiltredAttributes)
        {
            PointsAlreadyAdded = new HashSet<string>();

            List<GISTerrainLoaderPointGeoData> GeoVectorPointsData = new List<GISTerrainLoaderPointGeoData>();

            for (int i = 0; i < GeoData.GeoPoints.Count; i++)
            {
                var Geopoint = GeoData.GeoPoints[i];
    
                IEnumerable<string> PointsIntersection = null;
                var AttributesKeys = Geopoint.GetDataBaseKeys();
                PointsIntersection = FiltredAttributes.Intersect<string>(AttributesKeys);

                if (PointsIntersection != null)
                {
                    var ID = Geopoint.ID;

                    foreach (var attribute in PointsIntersection)
                    {
                        var Value = "";

                        if (Geopoint.TryGetValue(attribute, out Value))
                        {
                            if (!string.IsNullOrEmpty(Value) && !PointsAlreadyAdded.Contains(ID))
                            {
                                Geopoint.Tag = Value.Trim();
                                GeoVectorPointsData.Add(Geopoint);
                                PointsAlreadyAdded.Add(ID);

                            }

                        }
                    }
                }
            }
            return GeoVectorPointsData;
        }
        public static List<GISTerrainLoaderLineGeoData> GetGeoVectorLinesData(GISTerrainLoaderGeoVectorData GeoData, List<string> FiltredAttributes)
        {
            LinesAlreadyAdded = new HashSet<string>();

            List<GISTerrainLoaderLineGeoData> GeoVectorLinesData = new List<GISTerrainLoaderLineGeoData>();

            for (int i = 0; i < GeoData.GeoLines.Count; i++)
            {
                var GeoLine = GeoData.GeoLines[i];

                IEnumerable<string> LinesIntersection = null;
                var AttributesKeys = GeoLine.GetDataBaseKeys();
                LinesIntersection = FiltredAttributes.Intersect<string>(AttributesKeys);

                if (LinesIntersection != null)
                {
                    var ID = GeoLine.ID;
                 
                    foreach (var attribute in LinesIntersection)
                    {
                        var Value = "";

                        if (GeoLine.TryGetValue(attribute, out Value))
                        {
                            if (!string.IsNullOrEmpty(Value) && !LinesAlreadyAdded.Contains(ID))
                            {
                                GeoLine.Tag = Value.Trim();
                                GeoVectorLinesData.Add(GeoLine);
                                LinesAlreadyAdded.Add(ID);

                            }

                        }
                    }


                }
            }
            return GeoVectorLinesData;
        }
        public static List<GISTerrainLoaderPolygonGeoData> GetGeoVectorPolyData(GISTerrainLoaderGeoVectorData GeoData, List<string> FiltredAttributes)
        {
            PolygoneAlreadyAdded = new HashSet<string>();

            List<GISTerrainLoaderPolygonGeoData> GeoVectorPolygoneData = new List<GISTerrainLoaderPolygonGeoData>();

            for (int i = 0; i < GeoData.GeoPolygons.Count; i++)
            {
                var GeoPolygone = GeoData.GeoPolygons[i];

                IEnumerable<string> PolygonsIntersection = null;

                var AttributesKeys = GeoPolygone.GetDataBaseKeys();

                PolygonsIntersection = FiltredAttributes.Intersect<string>(AttributesKeys);

                if (PolygonsIntersection != null)
                {
                    var ID = GeoPolygone.ID;

                    foreach (var attribute in PolygonsIntersection)
                    {
                        var Value = "";

                        if (GeoPolygone.TryGetValue(attribute, out Value))
                        {
                            if (!string.IsNullOrEmpty(Value) && !PolygoneAlreadyAdded.Contains(ID))
                            {
                                GeoPolygone.Tag = Value.Trim();
                                GeoVectorPolygoneData.Add(GeoPolygone);
                                PolygoneAlreadyAdded.Add(ID);
                            }
                        }
                    }

                }
            }
            return GeoVectorPolygoneData;
        }
  
    }
}