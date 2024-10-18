/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGPXLoader : GISTerrainLoaderGeoDataHolder
    {
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;

        public static GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData("");

        public GISTerrainLoaderGPXLoader(string gpxfile, byte[] bytes = null)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            if (bytes == null)
                LoadGPXFile(gpxfile);
            else
                LoadGPXFile(bytes);

        }
        public static void LoadGPXFile(byte[] bytes)
        {
            fileData = new GISTerrainLoaderGeoVectorData("");

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Stream myStream = stream;
                ReadData(stream);
            }


        }
        public static void LoadGPXFile(string gpxfile)
        {
            fileData = new GISTerrainLoaderGeoVectorData("");

            using (FileStream stream = File.Open(gpxfile, FileMode.Open))
            {
                ReadData(stream);
            }
        }

        private static void ReadData(Stream stream)
        {
            using (GISTerrainLoaderGPXFileLoader reader = new GISTerrainLoaderGPXFileLoader(stream))
            {

                while (reader.Read())
                {
                    try
                    {
                        switch (reader.ObjectType)
                        {
                            case GpxObjectType.WayPoint:

                                GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                                if (VectorParameters.AddRandom_ID_Vector_GPX)
                                    PointGeoData.ID = UnityEngine.Random.Range(0, 100000).ToString();
                                else
                                    PointGeoData.ID = reader.WayPoint.DgpsId.ToString();

                                PointGeoData.Name = reader.WayPoint.Name;
                                PointGeoData.Tag = VectorParameters.GPX_GeoPoint_Value;
                                PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GPX_GeoPoint_Attribute, VectorParameters.GPX_GeoPoint_Value));
                                PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PointGeoData.ID));
                                PointGeoData.GeoPoint = new DVector2(reader.WayPoint.Longitude, reader.WayPoint.Latitude);

                                fileData.GeoPoints.Add(PointGeoData);
                                break;

                            case GpxObjectType.Track:

                                GISTerrainLoaderLineGeoData LineGeoData = new GISTerrainLoaderLineGeoData();

                                if (VectorParameters.AddRandom_ID_Vector_GPX)
                                    LineGeoData.ID = UnityEngine.Random.Range(0, 100000).ToString();
                                else
                                    LineGeoData.ID = reader.WayPoint.DgpsId.ToString();

                                if (string.IsNullOrEmpty(reader.Track.Name))
                                    LineGeoData.Name = reader.Track.Name;
                                else
                                    LineGeoData.Name = VectorParameters.GPX_GeoPoint_Value;

                                LineGeoData.Tag = VectorParameters.GPX_GeoPoint_Value;
                                LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GPX_GeoLine_Attribute, VectorParameters.GPX_GeoLine_Value));
                                LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, LineGeoData.ID));

                                foreach (var p in reader.Track.Segments)
                                {
                                    foreach (var s in p.TrackPoints)
                                    {
                                        PointGeoData = new GISTerrainLoaderPointGeoData();

                                        var Point = new DVector2(s.Longitude, s.Latitude);
                                        PointGeoData.GeoPoint = Point;
                                        LineGeoData.GeoPoints.Add(PointGeoData);
                                    }
                                }

                                fileData.GeoLines.Add(LineGeoData);


                                break;
                        }
                    }
                    catch
                    {
                        Debug.LogError("Couldn't read .gpx file ");
                    }
                }
            }

        }
        public override GISTerrainLoaderGeoVectorData GetGeoFiltredData(string name, GISTerrainContainer container = null)
        {
            if (!VectorParameters.LoadDataOutOfContainerBounds && container)
            {
                return fileData.GetDataInContainerBounds(container);
            }
            else
                return fileData;
        }
    }

    public class GISTerrainLoaderGPXFileData
    {
        public GpxObjectType Type;
        public List<GISTerrainLoaderGPXWayPoint> WayPoints = new List<GISTerrainLoaderGPXWayPoint>();
        public List<GISTerrainLoaderGPXPath> Paths = new List<GISTerrainLoaderGPXPath>();
    }
    public class GISTerrainLoaderGPXWayPoint
    {
        public string Name;
        public double Latitude;
        public double Longitude;
    }
    public class GISTerrainLoaderGPXPath
    {
        public string Name;
        public List<DVector2> WayPoints = new List<DVector2>();
    }

}