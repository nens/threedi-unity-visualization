/*     Unity GIS Tech 2020-2023      */


#if GISTerrainLoaderGeoJson
using GeoJSON.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGEOJSONLoader : GISTerrainLoaderGeoDataHolder
    {
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;
 

        public static GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData("");

        public GISTerrainLoaderGEOJSONLoader(string file, byte[] bytes = null)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();
 
            string Data = "";

            if (bytes == null)
                Data = File.ReadAllText(file, Encoding.UTF8);
            else
                Data = Encoding.Default.GetString(bytes);

            
            LoadGEOJSONFile(Data);
 

        }

        public static void LoadGEOJSONFile(string Data)
        {
            fileData = new GISTerrainLoaderGeoVectorData("");

            GeoJSON.Net.Feature.FeatureCollection collection = new GeoJSON.Net.Feature.FeatureCollection();

            collection = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(Data);

            int ShpCount = collection.Features.Count;
  
            for (int i = 0; i < ShpCount; i++)
            {
                var feature = collection.Features[i];

                switch (feature.Geometry.Type)
                {

                    case GeoJSONObjectType.Point:
 
                        var point = feature.Geometry as GeoJSON.Net.Geometry.Point;
  
                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                        PointGeoData.ID = i.ToString();
                        if (feature.Properties.ContainsKey("name"))
                            PointGeoData.Name = feature.Properties["name"].ToString();
                        if (string.IsNullOrEmpty(PointGeoData.Name))
                            PointGeoData.Name = VectorParameters.GEOJSON_GeoPoint_Value;

                        PointGeoData.Tag = VectorParameters.GEOJSON_GeoPoint_Value;
                        PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoPoint_Attribute, VectorParameters.GEOJSON_GeoPoint_Value));
                        PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PointGeoData.ID));
                        PointGeoData.GeoPoint = new DVector2(point.Coordinates.Longitude, point.Coordinates.Latitude);

                        fileData.GeoPoints.Add(PointGeoData);

                        break;
                    case GeoJSONObjectType.MultiPoint:

                        var points = feature.Geometry as GeoJSON.Net.Geometry.MultiPoint;

                        for(int p =0;p<points.Coordinates.Count;p++)
                        {
                            var  Cpoint = points.Coordinates[p];

                            PointGeoData = new GISTerrainLoaderPointGeoData();

                            PointGeoData.ID = i.ToString()+p.ToString();
                            if (feature.Properties.ContainsKey("name"))
                                PointGeoData.Name = feature.Properties["name"].ToString();
                            if (string.IsNullOrEmpty(PointGeoData.Name))
                                PointGeoData.Name = VectorParameters.GEOJSON_GeoPoint_Value;

                            PointGeoData.Tag = VectorParameters.GEOJSON_GeoPoint_Value;
                            PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoPoint_Attribute, VectorParameters.GEOJSON_GeoPoint_Value));
                            PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PointGeoData.ID));
                            PointGeoData.GeoPoint = new DVector2(Cpoint.Coordinates.Longitude, Cpoint.Coordinates.Latitude);

                            fileData.GeoPoints.Add(PointGeoData);
                        }
 
                        break;
                    case GeoJSONObjectType.LineString:

                        var line = feature.Geometry as GeoJSON.Net.Geometry.LineString;
                        GISTerrainLoaderLineGeoData LineGeoData = new GISTerrainLoaderLineGeoData();

                        LineGeoData.ID = i.ToString();

                        if (feature.Properties.ContainsKey("name"))
                            LineGeoData.Name = feature.Properties["name"].ToString();

                        if (string.IsNullOrEmpty(LineGeoData.Name))
                            LineGeoData.Name = VectorParameters.GEOJSON_GeoLine_Value;


                        LineGeoData.Tag = VectorParameters.GEOJSON_GeoLine_Value;
                        LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoLine_Attribute, VectorParameters.GEOJSON_GeoLine_Value));
                        LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, LineGeoData.ID));

                        foreach (var pro in feature.Properties)
                            LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(pro.Key, pro.Value.ToString()));

                        for (int p = 0; p < line.Coordinates.Count; p++)
                        {
                            var Cpoint = line.Coordinates[p];
 
                            PointGeoData = new GISTerrainLoaderPointGeoData();
                            PointGeoData.ID = i.ToString() + p.ToString();
                            if (feature.Properties.ContainsKey("name"))
                                PointGeoData.Name = feature.Properties["name"].ToString();
                            PointGeoData.GeoPoint = new DVector2(Cpoint.Longitude, Cpoint.Latitude);

                            LineGeoData.GeoPoints.Add(PointGeoData);
                        }
                        fileData.GeoLines.Add(LineGeoData);
                        break;
                    case GeoJSONObjectType.MultiLineString:

                        var Lines = feature.Geometry as GeoJSON.Net.Geometry.MultiLineString;

                        for (int p = 0; p < Lines.Coordinates.Count; p++)
                        {
                            var CLine = Lines.Coordinates[p];


                            LineGeoData = new GISTerrainLoaderLineGeoData();

                            LineGeoData.ID = i.ToString();
                            
                            if (feature.Properties.ContainsKey("name"))
                                LineGeoData.Name = feature.Properties["name"].ToString();
                           
                            if (string.IsNullOrEmpty(LineGeoData.Name))
                                LineGeoData.Name = VectorParameters.GEOJSON_GeoLine_Value;


                            LineGeoData.Tag = VectorParameters.GEOJSON_GeoLine_Value;
                            LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoLine_Attribute, VectorParameters.GEOJSON_GeoLine_Value));
                            LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, LineGeoData.ID));
                           
                            foreach (var pro in feature.Properties)
                                LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(pro.Key, pro.Value.ToString()));

                            for (int pc = 0; pc < CLine.Coordinates.Count; pc++)
                            {
                                var Cpoint = CLine.Coordinates[pc];

                                PointGeoData = new GISTerrainLoaderPointGeoData();

                                PointGeoData.ID = i.ToString() + pc.ToString();
                                if (feature.Properties.ContainsKey("name"))
                                    PointGeoData.Name = feature.Properties["name"].ToString();
                                PointGeoData.GeoPoint = new DVector2(Cpoint.Longitude, Cpoint.Latitude);

                                LineGeoData.GeoPoints.Add(PointGeoData);
                            }

                            fileData.GeoLines.Add(LineGeoData);
                        }
                        break;
                    case GeoJSONObjectType.Polygon:
 
                        var polygon = feature.Geometry as GeoJSON.Net.Geometry.Polygon;

                        GISTerrainLoaderPolygonGeoData PolygonGeoData = new GISTerrainLoaderPolygonGeoData();

                        PolygonGeoData.ID = i.ToString();
                        if(feature.Properties.ContainsKey("name"))
                        PolygonGeoData.Name = feature.Properties["name"].ToString();

                        if (string.IsNullOrEmpty(PolygonGeoData.Name))
                            PolygonGeoData.Name = VectorParameters.GEOJSON_GeoPolygon_Value;
 
                        PolygonGeoData.Tag = VectorParameters.GEOJSON_GeoPolygon_Value;
                        PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoPolygon_Attribute, VectorParameters.GEOJSON_GeoPolygon_Value));
                        PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PolygonGeoData.ID));

                        foreach (var pro in feature.Properties)
                            PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(pro.Key, pro.Value.ToString()));

                        foreach (var pro in feature.Properties)
                        {
                            PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(pro.Key, pro.Value.ToString()));
                        }

                        for (int p = 0; p < polygon.Coordinates.Count; p++)
                        {
                            var Cline = polygon.Coordinates[p];

                            List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();
 
                            for (int pc = 0; pc < Cline.Coordinates.Count; pc++)
                            {
                                var Cpoint = Cline.Coordinates[pc];

                                PointGeoData = new GISTerrainLoaderPointGeoData();

                                PointGeoData.ID = i.ToString() + pc.ToString();
                                if (feature.Properties.ContainsKey("name"))
                                    PointGeoData.Name = feature.Properties["name"].ToString();
                                PointGeoData.GeoPoint = new DVector2(Cpoint.Longitude, Cpoint.Latitude);

                                SubGeoPoints.Add(PointGeoData);

                            }

                            bool ClockWise = PolygonGeoData.IsClockwise(SubGeoPoints);

                            if (ClockWise)
                                PolygonGeoData.Roles.Add(Role.Outer);
                            else
                                PolygonGeoData.Roles.Add(Role.Inner);

                            PolygonGeoData.GeoPoints.Add(SubGeoPoints);
 
                        }

                        PolygonGeoData.SortPolyRole();

                        fileData.GeoPolygons.Add(PolygonGeoData);

                        break;
                    case GeoJSONObjectType.MultiPolygon:

                        var Multipolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;
 
                        for (int pl = 0; pl < Multipolygon.Coordinates.Count; pl++)
                        {
                            var m_polygon = Multipolygon.Coordinates[pl];

                            GISTerrainLoaderPolygonGeoData MultiPolygonGeoData = new GISTerrainLoaderPolygonGeoData();

                            MultiPolygonGeoData.ID = i.ToString();
                            if (feature.Properties.ContainsKey("name"))
                                MultiPolygonGeoData.Name = feature.Properties["name"].ToString();

                            if (string.IsNullOrEmpty(MultiPolygonGeoData.Name))
                                MultiPolygonGeoData.Name = VectorParameters.GEOJSON_GeoPolygon_Value;

                            MultiPolygonGeoData.Tag = VectorParameters.GEOJSON_GeoPolygon_Value;
                            MultiPolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.GEOJSON_GeoPolygon_Attribute, VectorParameters.GEOJSON_GeoPolygon_Value));
                            MultiPolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, MultiPolygonGeoData.ID));
                            
                            foreach (var pro in feature.Properties)
                                MultiPolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(pro.Key, pro.Value.ToString()));

                            for (int p = 0; p < m_polygon.Coordinates.Count; p++)
                            {
                                var Cline = m_polygon.Coordinates[p];

                                List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();

                                for (int pc = 0; pc < Cline.Coordinates.Count; pc++)
                                {
                                    var Cpoint = Cline.Coordinates[pc];

                                    PointGeoData = new GISTerrainLoaderPointGeoData();

                                    PointGeoData.ID = i.ToString() + pc.ToString();
                                    if (feature.Properties.ContainsKey("name"))
                                        PointGeoData.Name = feature.Properties["name"].ToString();
                                    PointGeoData.GeoPoint = new DVector2(Cpoint.Longitude, Cpoint.Latitude);

                                    SubGeoPoints.Add(PointGeoData);
                                }

                                bool ClockWise = MultiPolygonGeoData.IsClockwise(SubGeoPoints);

                                if (ClockWise)
                                    MultiPolygonGeoData.Roles.Add(Role.Outer);
                                else
                                    MultiPolygonGeoData.Roles.Add(Role.Inner);

                                MultiPolygonGeoData.GeoPoints.Add(SubGeoPoints);

                            }

                            MultiPolygonGeoData.SortPolyRole();

                            fileData.GeoPolygons.Add(MultiPolygonGeoData);


                        }

                        break;
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
 

}
#endif