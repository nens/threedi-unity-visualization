/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderOSMFileLoader : GISTerrainLoaderGeoDataHolder
    {
        public GISTerrainLoaderOSMData osmData;
        private GISTerrainLoaderVectorParameters_SO VectorParameters;
        public GISTerrainLoaderOSMFileLoader(string FilePath)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            var parser = new GISTerrainLoaderOSMParser();
            osmData = parser.ParseFromFile(FilePath);
            osmData.FillNodes();
        }
        public GISTerrainLoaderOSMFileLoader(byte[] Data)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            var parser = new GISTerrainLoaderOSMParser();
            osmData = parser.ParseFromFile(Data);
            osmData.FillNodes();
        }
        
        public override GISTerrainLoaderGeoVectorData GetGeoFiltredData(string name="", GISTerrainContainer container = null)
        {
          GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData(name);
            
            if (osmData.Nodes.Count != 0)
            {
                foreach (var node in osmData.Nodes)
                {
                    long ID = node.Key;

                    GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                    PointGeoData.ID = ID.ToString();

                    PointGeoData.DataBase = node.Value.DataBase;
                    if (container)
                        PointGeoData.GeoPoint = ConvertCoordinatesTo(new DVector2(node.Value.Lon, node.Value.Lat), 4326, container.data.EPSG);
                    else
                        PointGeoData.GeoPoint = new DVector2(node.Value.Lon, node.Value.Lat);
 
                    if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                    {
                        if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                            fileData.GeoPoints.Add(PointGeoData);
                    }else
                        fileData.GeoPoints.Add(PointGeoData);

                }
            }

            if (osmData.Ways.Count != 0)
            {
                foreach (var way in osmData.Ways)
                {
                    string ID = way.Id;

                    GISTerrainLoaderLineGeoData LineGeoData = new GISTerrainLoaderLineGeoData();
                    LineGeoData.ID = ID;
                    LineGeoData.DataBase = way.DataBase;


                    string Areatype = "";
                    bool IsPoly = way.TryGetValue("area", out Areatype);
                    //Areatype.Trim() !="yes"


                    for (int i = 0; i < way.Nodes.Count; i++)
                    {
                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                        PointGeoData.ID = way.Nodes[i].Id;
 
                        if (container)
                            PointGeoData.GeoPoint = ConvertCoordinatesTo(new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat),4326, container.data.EPSG);
                        else PointGeoData.GeoPoint = new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat);

                        if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                        {
                            if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                            {
                                fileData.GeoPoints.Add(PointGeoData);
                                LineGeoData.GeoPoints.Add(PointGeoData);
                            }
                        }
                        else
                        {
                            fileData.GeoPoints.Add(PointGeoData);
                            LineGeoData.GeoPoints.Add(PointGeoData);
                        }
                    }
 
                    fileData.GeoLines.Add(LineGeoData);

                    //if (!IsPoly && Areatype.Trim() !="yes")

                    GISTerrainLoaderPolygonGeoData PolygoneGeoData = new GISTerrainLoaderPolygonGeoData();
                    PolygoneGeoData.ID = ID;
                    PolygoneGeoData.DataBase = way.DataBase;

                    List<GISTerrainLoaderPointGeoData> points = new List<GISTerrainLoaderPointGeoData>();
                    
                    for (int i = 0; i < way.Nodes.Count; i++)
                    {
                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                        PointGeoData.ID = way.Nodes[i].Id;
 
                        if (container)
                            PointGeoData.GeoPoint = ConvertCoordinatesTo(new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat), 4326, container.data.EPSG);
                        else
                            PointGeoData.GeoPoint = new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat);

                        if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                        {
                            if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                                points.Add(PointGeoData);
                        }
                        else
                            points.Add(PointGeoData);


                    }

                    PolygoneGeoData.Roles.Add(Role.Outer);

                    PolygoneGeoData.GeoPoints.Add(points);
                    fileData.GeoPolygons.Add(PolygoneGeoData);
 

                }
            }

            if (osmData.Relations.Count != 0)
            {
                foreach (var relation in osmData.Relations)
                {
                    string ID = relation.Id;

                    string relationtype = "";

                    bool IsPoly = relation.TryGetValue("type", out relationtype);

                    if (IsPoly)
                    {
                        GISTerrainLoaderPolygonGeoData PolygoneGeoData = new GISTerrainLoaderPolygonGeoData();
                        PolygoneGeoData.ID = ID;
                        PolygoneGeoData.DataBase = relation.DataBase;

                        foreach (var memeber in relation.Members)
                        {
                            var way = osmData.Ways.Find(x => x.Id.Trim() == memeber.Ref.Trim());

                            if (way != null)
                            {
                                int SubCounter = way.Nodes.Count;

                                if (SubCounter > 0)
                                {
                                    List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();
 
                                    for (int i = 0; i < SubCounter; i++)
                                    {
                         
                                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                                        PointGeoData.ID = way.Nodes[i].Id;
                                        if (container)
                                            PointGeoData.GeoPoint = ConvertCoordinatesTo(new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat), 4326, container.data.EPSG);
                                        else
                                            PointGeoData.GeoPoint = new DVector2(way.Nodes[i].Lon, way.Nodes[i].Lat);
                                        if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                                        {
                                            if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                                                SubGeoPoints.Add(PointGeoData);
                                        }
                                        else
                                            SubGeoPoints.Add(PointGeoData);
                                    }

                                     if (memeber.role == "outer")
                                        PolygoneGeoData.Roles.Add(Role.Outer);
                                    else
                                        PolygoneGeoData.Roles.Add(Role.Inner);

                                    PolygoneGeoData.GeoPoints.Add(SubGeoPoints);

                                }
                            }

                        }

                        PolygoneGeoData.SortPolyRole();

                        fileData.GeoPolygons.Add(PolygoneGeoData);
                      
                    }else
                    {
                        //
                    }
 
                }
            }
  
            return fileData;
        }
        private DVector2 ConvertCoordinatesTo(DVector2 coor, int From_EPSG, int To_epsg)
        {
            if (From_EPSG != To_epsg)
                return GISTerrainLoaderGeoConversion.ConvertCoordinatesFromTo(coor, From_EPSG, To_epsg);
            else
                return coor;
        }
    }
}

