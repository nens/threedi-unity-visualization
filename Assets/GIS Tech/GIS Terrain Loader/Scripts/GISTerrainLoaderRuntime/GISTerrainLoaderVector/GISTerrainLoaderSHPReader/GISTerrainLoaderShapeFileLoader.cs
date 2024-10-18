/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeFileLoader : GISTerrainLoaderGeoDataHolder
    {
        public List<GISTerrainLoaderShapeFileData> TotalShapes;

        public static int DBFColCount = 0;
        public static int DBFRowCount = 0;
        public static DbfFile DBFBase;

        private GISTerrainLoaderVectorParameters_SO VectorParameters;
        public GISTerrainLoaderShapeFileLoader(GISTerrainLoaderShpFileHeader ShapeFile)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            TotalShapes = new List<GISTerrainLoaderShapeFileData>();

            if (ShapeFile != null)
            {
                string dbfpath = Path.ChangeExtension(ShapeFile.FilePath, ".dbf");
                DBFBase = LoadDBFBase(dbfpath, ShapeFile);
            }

        }

        public GISTerrainLoaderShapeFileLoader(GISTerrainLoaderShpFileHeader ShapeFile, byte[] DBFData, byte[] ProjData)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            TotalShapes = new List<GISTerrainLoaderShapeFileData>();

            if (ShapeFile != null)
            {
                DBFBase = LoadDBFBase(DBFData, ShapeFile);
            }

        }
        public DbfFile LoadDBFBase(string dbfPath, GISTerrainLoaderShpFileHeader shapeFile)
        {
            if (File.Exists(dbfPath))
            {

                try
                {
                    DBFBase = new DbfFile(System.Text.Encoding.ASCII);
                    DBFBase.Open(dbfPath, FileMode.Open);

                    DBFColCount = (int)DBFBase.Header.ColumnCount;
                    DBFRowCount = (int)DBFBase.Header.RecordCount;

                    for (int r = 0; r < DBFRowCount; r++)
                    {
                        var database = new List<GISTerrainLoaderShpDataBase>();

                        for (var c = 0; c < DBFColCount; c++)
                        {
                            string tag = "";
                            DBFBase.ReadValue(r, c, out tag);
                            database.Add(new GISTerrainLoaderShpDataBase(DBFBase.Header._fields[c].Name, tag));
                            if (r < shapeFile.RecordSet.Count)
                                shapeFile.RecordSet[r].DataBase = database;

                        }

                    }

                    for (int i = 0; i < shapeFile.RecordSet.Count; i++)
                    {
                        var shape = shapeFile.RecordSet[i];

                        GISTerrainLoaderShapeFileData shapeData = new GISTerrainLoaderShapeFileData(shapeFile.ShpType, shape, VectorParameters);

                        TotalShapes.Add(shapeData);
                    }

                    DBFBase.Close();

                    return DBFBase;

                }
                catch (Exception e)
                {
                    Debug.LogError("Could not read DataBase " + e);
                    DBFBase.Close();
                    return null;

                }
            }
            else
            {
                Debug.LogError("DBF Database not exist");
                return null;
            }


        }
        public DbfFile LoadDBFBase(byte[] dbfdata, GISTerrainLoaderShpFileHeader shapeFile)
        {
            if (dbfdata.Length > 0)
            {
                try
                {
                    DBFBase = new DbfFile(System.Text.Encoding.ASCII);

                    DBFBase.Open(dbfdata);

                    DBFColCount = (int)DBFBase.Header.ColumnCount;
                    DBFRowCount = (int)DBFBase.Header.RecordCount;

                    for (int r = 0; r < DBFRowCount; r++)
                    {
                        var database = new List<GISTerrainLoaderShpDataBase>();

                        for (var c = 0; c < DBFColCount; c++)
                        {
                            string tag = "";
                            DBFBase.ReadValue(r, c, out tag);
                            database.Add(new GISTerrainLoaderShpDataBase(DBFBase.Header._fields[c].Name, tag));
                            shapeFile.RecordSet[r].DataBase = database;

                        }

                    }

                    for (int i = 0; i < shapeFile.RecordSet.Count; i++)
                    {
                        var shape = shapeFile.RecordSet[i];

                        GISTerrainLoaderShapeFileData shapeData = new GISTerrainLoaderShapeFileData(shapeFile.ShpType, shape, VectorParameters);

                        TotalShapes.Add(shapeData);
                    }

                    DBFBase.Close();

                    return DBFBase;
                }
                catch (Exception e)
                {
                    Debug.Log("Could not read DataBase .. " + e);
                    DBFBase.Close();
                    return null;

                }
            }
            else
            {
                Debug.LogError("DBF Database not exist");
                return null;
            }


        }



        public override GISTerrainLoaderGeoVectorData GetGeoFiltredData(string name = "", GISTerrainContainer container = null)
        {
            GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData(name);

            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                var ID = shape.Id;

                if (string.IsNullOrEmpty(ID))
                {
                    ID = i.ToString();
                    shape.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, ID));
                }

                if (shape.ShapeType == ShapeType.Point || shape.ShapeType == ShapeType.PointZ || shape.ShapeType == ShapeType.PointM)
                {
                    GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                    PointGeoData.ID = ID;
                    PointGeoData.Name = shape.LAYER;
                    PointGeoData.DataBase = shape.DataBase;

                    var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                    if (points.Count > 0)
                    {
                        PointGeoData.GeoPoint = new DVector2(points[0].X, points[0].Y);

                        if (shape.ShapeType == ShapeType.PointZ)
                        {
                            var Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                            PointGeoData.Elevation = (float)Elevations[0];
                        }

                    }

                    if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                    {
                        if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                            fileData.GeoPoints.Add(PointGeoData);
                    }
                    else
                        fileData.GeoPoints.Add(PointGeoData);

                }

                if (shape.ShapeType == ShapeType.PolyLine || shape.ShapeType == ShapeType.PolyLineZ || shape.ShapeType == ShapeType.PolyLineM)
                {
                    GISTerrainLoaderLineGeoData LineGeoData = new GISTerrainLoaderLineGeoData();
                    LineGeoData.ID = ID;
                    LineGeoData.Name = shape.LAYER;
                    LineGeoData.DataBase = shape.DataBase;


                    var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                    for (int p = 0; p < points.Count; p++)
                    {
                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                        PointGeoData.GeoPoint = new DVector2(points[p].X, points[p].Y);

                        if (shape.ShapeType == ShapeType.PolyLineZ)
                        {
                            var Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                            PointGeoData.Elevation = (float)Elevations[p];
                        }

                        if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                        {
                            if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                                LineGeoData.GeoPoints.Add(PointGeoData);
                        }
                        else
                            LineGeoData.GeoPoints.Add(PointGeoData);

                    }

                    fileData.GeoLines.Add(LineGeoData);
                }

                if (shape.ShapeType == ShapeType.Polygon || shape.ShapeType == ShapeType.PolygonZ || shape.ShapeType == ShapeType.PolygonM)
                {
                    var PolyGeoData = new GISTerrainLoaderPolygonGeoData();
                    PolyGeoData.ID = ID;
                    PolyGeoData.Name = shape.LAYER;
                    PolyGeoData.DataBase = shape.DataBase;

                    int Partscount = 0;
                    List<Point> ShpPoints = new List<Point>();

                    switch (shape.ShapeType)
                    {
                        case ShapeType.Polygon:

                            var Polygon = (Polygon)shape.ShapeRecord.Contents as Polygon;

                            ShpPoints = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                            Partscount = Polygon.NumParts;

                            for (int r = 0; r < Partscount; r++)
                            {
                                List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();

                                int from = Polygon.Parts[r];
                                int To = ShpPoints.Count;

                                if ((r + 1) < Partscount) To = Polygon.Parts[r + 1];

                                for (int p = from; p < To; p++)
                                {
                                    GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                                    PointGeoData.GeoPoint = new DVector2(ShpPoints[p].X, ShpPoints[p].Y);

                                    if (shape.ShapeType == ShapeType.PolygonZ)
                                    {
                                        var Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                                        PointGeoData.Elevation = (float)Elevations[p];
                                    }

                                    if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                                    {
                                        if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                                            SubGeoPoints.Add(PointGeoData);
                                    }
                                    else
                                        SubGeoPoints.Add(PointGeoData);

                                }

                                bool ClockWise = PolyGeoData.IsClockwise(SubGeoPoints);

                                if (ClockWise)
                                    PolyGeoData.Roles.Add(Role.Outer);
                                else
                                    PolyGeoData.Roles.Add(Role.Inner);

                                PolyGeoData.GeoPoints.Add(SubGeoPoints);
                            }
                            break;
                            case ShapeType.PolygonZ:

                            var PolygonZ = (PolygonZ)shape.ShapeRecord.Contents as PolygonZ;

                            ShpPoints = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                            Partscount = PolygonZ.NumParts;

                            for (int r = 0; r < Partscount; r++)
                            {
                                List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();

                                int from = PolygonZ.Parts[r];
                                int To = ShpPoints.Count;

                                if ((r + 1) < Partscount) To = PolygonZ.Parts[r + 1];

                                for (int p = from; p < To; p++)
                                {
                                    GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

                                    PointGeoData.GeoPoint = new DVector2(ShpPoints[p].X, ShpPoints[p].Y);

                                    if (shape.ShapeType == ShapeType.PolygonZ)
                                    {
                                        var Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                                        PointGeoData.Elevation = (float)Elevations[p];
                                    }

                                    if (!VectorParameters.LoadDataOutOfContainerBounds && container)
                                    {
                                        if (container.IncludeRealWorldPoint(PointGeoData.GeoPoint))
                                            SubGeoPoints.Add(PointGeoData);
                                    }
                                    else
                                        SubGeoPoints.Add(PointGeoData);

                                }

                                bool ClockWise = PolyGeoData.IsClockwise(SubGeoPoints);

                                if (ClockWise)
                                    PolyGeoData.Roles.Add(Role.Outer);
                                else
                                    PolyGeoData.Roles.Add(Role.Inner);

                                PolyGeoData.GeoPoints.Add(SubGeoPoints);
                            }



                            break;
                        case ShapeType.PolygonM:
                            break;
                    }
                    PolyGeoData.SortPolyRole();
                    fileData.GeoPolygons.Add(PolyGeoData);

                }

            }
            return fileData;
        }


    }
}

