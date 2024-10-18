/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public static class GISTerrainLoaderVectorExtensions
    {
        public static string[] GetFiles(string terrainPath, HashSet<string> hash)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrainPath), TerrainFilename + Settings_SO.VectorDataFolderName);

            string[] tiles = null;

            if (Directory.Exists(VectorFolder))
            {
                tiles = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => hash.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();

            }
            else
                Debug.LogError("VectorData directory not exist");

            return tiles;
        }
        /// <summary>
        /// Used For Standard Platforms,Return a Dic of Vector Files Where Key= file Path, Value = FileType  
        /// </summary>
        /// <param name="terrainPath"></param>
        /// <param name="PrefVectorType"></param>
        /// <returns></returns>
        public static Dictionary<string,VectorType> GetAllVectorFiles(string terrainPath, VectorType PrefVectorType)
        {
            Dictionary<string, VectorType> VectorFiles = new Dictionary<string, VectorType>();

            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));
 
            var hash = Settings_SO.SupportedVectorData;
 
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrainPath), TerrainFilename + Settings_SO.VectorDataFolderName);

            string[] Files = new string[0];

            if (Directory.Exists(VectorFolder))
            {
                Files = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => hash.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();

                if (Files.Length > 0)
                {
                    foreach (var file in Files)
                    {
                        var Ext = Path.GetExtension(file);

                        switch (Ext)
                        {
                            case ".shp":
                                VectorFiles.Add(file, VectorType.ShapeFile);
                                break;
                            case ".osm":
                                VectorFiles.Add(file, VectorType.OpenStreetMap);
                                break;
                            case ".kml":
                                VectorFiles.Add(file, VectorType.KML);
                                break;
                            case ".gpx":
                                VectorFiles.Add(file, VectorType.GPX);
                                break;
                            case ".geojson":
                                VectorFiles.Add(file, VectorType.Geojson);
                                break;
                        }

                    }
                }
            }
            else
                Debug.LogError("VectorData directory not exist");


            Dictionary<string, VectorType> m_VectorFiles = new Dictionary<string, VectorType>();

            if (PrefVectorType != VectorType.AllVectorFiles)
            {
                foreach (var vectorfile in VectorFiles)
                {
                    if (vectorfile.Value == PrefVectorType)
                        m_VectorFiles.Add(vectorfile.Key, vectorfile.Value);
                }
            }
            else
                m_VectorFiles = VectorFiles;

            return m_VectorFiles;
        }
       
        /// <summary>
        /// Used For WebBase Platforms,Return a Dic of Vector Files Where Key= file Path, Value = FileType  
        /// </summary>
        /// <param name="terrainPath"></param>
        /// <param name="PrefVectorType"></param>
        /// <returns></returns>
        public static Dictionary<string, VectorType> GetAllVectorFilesFromWebData(GISTerrainLoaderWebData WebData, VectorType PrefVectorType)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            Dictionary<string, VectorType> VectorFiles = new Dictionary<string, VectorType>();

            if (WebData != null)
            {
                if (WebData.VectorFolderExist == 1)
                {
                    if (WebData.vectors != null)
                    {
                        foreach (var vectorfile in WebData.vectors)
                        {
                            var file = GISTerrainLoaderPlatformHelper.GetGISFilePath(Application.platform, WebData.MainPath, Settings_SO.VectorDataFolderName, vectorfile);

                            if (file.Contains(".osm"))
                                VectorFiles.Add(file, VectorType.OpenStreetMap);
                            if (file.Contains(".shp"))
                                VectorFiles.Add(file, VectorType.ShapeFile);
                            if (file.Contains(".kml"))
                                VectorFiles.Add(file, VectorType.KML);
                            if (file.Contains(".gpx"))
                                VectorFiles.Add(file, VectorType.GPX);
                            if (file.Contains(".geojson"))
                                VectorFiles.Add(file, VectorType.Geojson);

                        }
                    }
                }
        
            }

            Dictionary<string, VectorType> m_VectorFiles = new Dictionary<string, VectorType>();

            if (PrefVectorType != VectorType.AllVectorFiles)
            {
                foreach (var vectorfile in VectorFiles)
                {
                    if (vectorfile.Value == PrefVectorType)
                        m_VectorFiles.Add(vectorfile.Key, vectorfile.Value);
                }
            }
            else
                m_VectorFiles = VectorFiles;

            return m_VectorFiles;
        }


        /// <summary>
        /// Check if Point Unity Space World Position intersect with a Polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsPointInPolygon(Vector3[] poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        /// <summary>
        /// Check if Point Unity Space World Position intersect with a Polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<Vector3> poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        /// <summary>
        /// Check if Point in Real World Position intersect with a Polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<DVector2> poly, double x, double y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].y <= y && y < poly[j].y ||
                     poly[j].y <= y && y < poly[i].y) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        /// <summary>
        /// Check if GISTerrainLoaderPointGeoData in Real World Position intersect with a Polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<GISTerrainLoaderPointGeoData> polygon, DVector2 testPoint)
        {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].GeoPoint.y < testPoint.y && polygon[j].GeoPoint.y >= testPoint.y || polygon[j].GeoPoint.y < testPoint.y && polygon[i].GeoPoint.y >= testPoint.y)
                {
                    if (polygon[i].GeoPoint.x + (testPoint.y - polygon[i].GeoPoint.y) / (polygon[j].GeoPoint.y - polygon[i].GeoPoint.y) * (polygon[j].GeoPoint.x - polygon[i].GeoPoint.x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static Rect GetRectFromPoints(Vector3[] points)
        {
            return new Rect
            {
                x = points.Min(p => p.x),
                y = points.Min(p => p.z),
                xMax = points.Max(p => p.x),
                yMax = points.Max(p => p.z)
            };
        }
        public static Rect GetRectFromPoints(List<DVector3> points)
        {
            return new Rect
            {
                x = points.Min(p => (float)p.x),
                y = points.Min(p => (float)p.z),
                xMax = points.Max(p => (float)p.x),
                yMax = points.Max(p => (float)p.z)
            };
        }
        public static double[] GetBorder(Point[] points, double[] Zvalue = null)
        {
            double[] border = new double[4];

            if (Zvalue != null)
            {
                border = new double[6];
            }

            double Xmin = ((Point)points[0]).X;
            double Ymin = ((Point)points[0]).Y;
            double Xmax = ((Point)points[0]).X;
            double Ymax = ((Point)points[0]).Y;

            double zXmin = 99999;
            double zXmax = -99999;



            for (int i = 0; i < points.Length; i++)
            {
                var Point = points[i];

                if (Xmin > Point.X)
                {
                    Xmin = Point.X;
                }
                if (Xmax < Point.X)
                {
                    Xmax = Point.X;
                }
                if (Ymin > Point.Y)
                {
                    Ymin = Point.Y;
                }
                if (Ymax < Point.Y)
                {
                    Ymax = Point.Y;
                }

                if (Zvalue != null)
                {
                    if (zXmin > Zvalue[i])
                    {
                        zXmin = Zvalue[i];
                    }
                    if (zXmax < Zvalue[i])
                    {
                        zXmax = Zvalue[i];
                    }
                }


            }

            border[0] = Xmin;
            border[1] = Ymin;
            border[2] = Xmax;
            border[3] = Ymax;

            if (Zvalue != null)
            {
                border[4] = zXmin;
                border[5] = zXmax;
            }

            return border;
        }
        public static double[] GetBorder(List<List<GISTerrainLoaderPointGeoData>> GeoPoints, double[] Zvalue = null)
        {
            double[] border = new double[4];

            if (Zvalue != null)
            {
                border = new double[6];
            }

            for(int i = 0; i < GeoPoints.Count; i++)
            {
                var SubPoint = GeoPoints[i];

                for(int j =0;j < SubPoint.Count; j++)
                {
                    double Xmin = SubPoint[0].GeoPoint.x;
                    double Ymin = SubPoint[0].GeoPoint.y;
                    double Xmax = SubPoint[0].GeoPoint.x;
                    double Ymax = SubPoint[0].GeoPoint.y;

                    double zXmin = 99999;
                    double zXmax = -99999;



                    for (int s = 0; s < 4; s++)
                    {
                        var Point = SubPoint[s].GeoPoint;

                        if (Xmin > Point.x)
                        {
                            Xmin = Point.x;
                        }
                        if (Xmax < Point.x)
                        {
                            Xmax = Point.x;
                        }
                        if (Ymin > Point.y)
                        {
                            Ymin = Point.y;
                        }
                        if (Ymax < Point.y)
                        {
                            Ymax = Point.y;
                        }

                        if (Zvalue != null)
                        {
                            if (zXmin > Zvalue[s])
                            {
                                zXmin = Zvalue[s];
                            }
                            if (zXmax < Zvalue[s])
                            {
                                zXmax = Zvalue[s];
                            }
                        }


                    }

                    border[0] = Xmin;
                    border[1] = Ymin;
                    border[2] = Xmax;
                    border[3] = Ymax;

                    if (Zvalue != null)
                    {
                        border[4] = zXmin;
                        border[5] = zXmax;
                    }
                }
            }
           return border;
        }

        /// <summary>
        /// Get a Random Point Position Inside Polygon In Unity World Space
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="groundHight"></param>
        /// <returns></returns>
        public static List<Vector3> GenerateRandomPointInsidePolygon(List<Vector3> polygon, float threshold, float groundHight = 0)
        {
            List<Vector3> Newpoints = new List<Vector3>();

            Vector3 MinVec = MinPointOnThePolygon(polygon);
            Vector3 MaxVec = MaxPointOnThePolygon(polygon);
 
            float sizeX = MaxVec.x - MinVec.x;
            float sizeZ = MaxVec.z - MinVec.z;

            float PointsX = sizeX / threshold;
            float PointsZ = sizeZ / threshold;

            for (int i = 0; i < PointsX; i++)
            {
                for (int j = 0; j < PointsZ; j++)
                {
                    float x = UnityEngine.Random.Range(MinVec.x, MaxVec.x);
                    float z = UnityEngine.Random.Range(MinVec.z, MaxVec.z);
                    Vector3 point = new Vector3(x, groundHight, z);

                    if (IsPointInPolygon(polygon, x, z))
                    {
                        Newpoints.Add(point);
                    }

                }
            }
 
            return Newpoints;

        }
        private static Vector3 MinPointOnThePolygon(List<Vector3> polygon, float groundHight = 0)
        {
            float minX = polygon[0].x;
            float minZ = polygon[0].z;
            for (int i = 1; i < polygon.Count; i++)
            {
                if (minX > polygon[i].x)
                {
                    minX = polygon[i].x;
                }
                if (minZ > polygon[i].z)
                {
                    minZ = polygon[i].z;
                }
            }
            return new Vector3(minX, groundHight, minZ);
        }
        private static Vector3 MaxPointOnThePolygon(List<Vector3> polygon, float groundHight = 0)
        {
            float maxX = polygon[0].x;
            float maxZ = polygon[0].z;
            for (int i = 1; i < polygon.Count; i++)
            {
                if (maxX < polygon[i].x)
                {
                    maxX = polygon[i].x;
                }
                if (maxZ < polygon[i].z)
                {
                    maxZ = polygon[i].z;
                }
            }
            return new Vector3(maxX, groundHight, maxZ);
        }


        /// <summary>
        /// Return a regular in Unity World Space Points Inside of a Polygon without elevation data 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="groundHight"></param>
        /// <returns></returns>
        public static List<Vector3> GenerateRegular2DPointsInsidePolygon(List<Vector3> polygon, float threshold, float groundHight = 0)
        {
            if (threshold == 0)
                threshold = 0.01f;

            Vector3 MinVec = MinPointOnThePolygon(polygon);
            Vector3 MaxVec = MaxPointOnThePolygon(polygon);
 
            List<Vector3> Newpoints = new List<Vector3>();

            for (float x = MinVec.x; x <= MaxVec.x; x += threshold)
            {
                for (float z = MinVec.z; z <= MaxVec.z; z += threshold)
                {

                    if (IsPointInPolygon(polygon, x, z))
                    {
                        Vector3 p = new Vector3(x, groundHight, z);
                        Newpoints.Add(p);

                    }
                }
            }

            return Newpoints;
        }

        /// <summary>
        /// Return a regular in Unity World Space Points Inside of a Polygon with elevation data 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="groundHight"></param>
        /// <returns></returns>
        public static List<Vector3> GenerateRegular3DPointsInsidePolygon(GISTerrainContainer container, List<Vector3> polygon, float threshold)
        {
            if (threshold == 0)
                threshold = 0.01f;

            List<Vector3> Newpoints = new List<Vector3>();

            Newpoints.AddRange(polygon);

            return Newpoints;
        }

        public static List<GISTerrainLoaderPointGeoData> ToGeoPointList(this List<GISTerrainLoader.DVector2> items)
        {
            List<GISTerrainLoaderPointGeoData> Newitems = new List<GISTerrainLoaderPointGeoData>();
 
            for (int i = 0; i < items.Count; i++)
            {
                GISTerrainLoaderPointGeoData item = new GISTerrainLoaderPointGeoData(items[i]);
                Newitems.Add(item);
            }
            return Newitems;
        }
        public static List<GISTerrainLoader.DVector2> ToDVector2List(this List<GISTerrainLoaderPointGeoData> items)
        {
            List<GISTech.GISTerrainLoader.DVector2> Newitems = new List<GISTech.GISTerrainLoader.DVector2>();

            for (int i = 0; i < items.Count; i++)
            {
                GISTech.GISTerrainLoader.DVector2 item = items[i].GeoPoint as GISTech.GISTerrainLoader.DVector2;
                Newitems.Add(item);
            }
            return Newitems;
        }
        public static List<List<GISTerrainLoaderPointGeoData>> ToMultiGeoPointList(this List<List<GISTerrainLoader.DVector2>> items)
        {
            List<List<GISTerrainLoaderPointGeoData>> Newitems = new List<List<GISTerrainLoaderPointGeoData>>();

            for (int i = 0; i < items.Count; i++)
            {
                List<GISTerrainLoaderPointGeoData> subList = new List<GISTerrainLoaderPointGeoData>();
               
                for (int j = 0; j < items[i].Count; j++)
                {
                    GISTerrainLoader.DVector2 Point = items[i][j];
                    GISTerrainLoaderPointGeoData item = new GISTerrainLoaderPointGeoData(Point);
                    subList.Add(item);
                }
                Newitems.Add(subList);
            }
            return Newitems;
        }
 
    }
}