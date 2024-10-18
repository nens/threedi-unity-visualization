/*     Unity GIS Tech 2020-2023      */

using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeFileExporter
    {
        public string FilePath;
        public GISTerrainLoaderGeoVectorData GeoData;
        public bool StoreElevationData;

        public GISTerrainLoaderShapeFileExporter(string m_FilePath, GISTerrainLoaderGeoVectorData m_GeoData, bool m_StoreElevationData = false)
        {
            FilePath = m_FilePath;
            GeoData = m_GeoData;
            StoreElevationData = m_StoreElevationData;
        }
        public void Export()
        {
            string Proj = GISTerrainLoaderEPSG.GetProjEPSGName(GeoData.EPSG);
            
            if (!string.IsNullOrEmpty(Proj))
            {
                ShpFileWriter ShapeWriter = new ShpFileWriter();

                ShapeWriter.Creat(FilePath, FileMode.Create, StoreElevationData);

                ShapeWriter.WriteShp(GeoData);

                ShapeWriter.WriteShx();

                ShapeWriter.WritePrj(FilePath, GeoData.EPSG);

                ShapeWriter.WriteDbf(FilePath);

                ShapeWriter.Close();

                Debug.Log("Vector Data Exported successfully to " + FilePath);
            }
            else
            {
                Debug.LogError("Projection not supported. Unable to create Prj file");
            }



        }

    }
    public class ShpFileWriter
    {
        public bool StoreElevationData;

        protected GISTerrainLoaderShpFileHeader mHeader;

        protected ShpRecordData mShpRecord;

        protected bool mHeaderWritten = false;

        protected bool mFileCreat = false;

        protected Stream mShpFile = null;

        protected Stream mShxFile = null;

        protected BinaryWriter mShpFileWriter = null;

        protected BinaryWriter mShxFileWriter = null;

        protected string mFileName = "";

        protected bool mIsForwardOnly = false;

        public string FileName => mFileName;

        public bool IsForwardOnly => mIsForwardOnly;

        public GISTerrainLoaderShpFileHeader Header => mHeader;

        public ShpRecordData ShpRecord => mShpRecord;

        public ShxRecord ShxRecord;

        public ShpFileWriter()
        {
            mHeader = new GISTerrainLoaderShpFileHeader();
        }
        public void Creat(string sPath, FileMode mode,bool m_StoreElevationData)
        {
            mFileName = sPath;
            StoreElevationData = m_StoreElevationData;
            string path = mFileName.Remove(mFileName.Length - 1, 1) + "x";
            Creat(File.Open(sPath, mode), File.Open(path, mode));
        }
        public void Creat(Stream shpStream, Stream shxStream)
        {
            if (mShpFile != null)
            {
                Close();
            }

            mShpFile = shpStream;
            mShxFile = shxStream;
            mShpFileWriter = null;
            mShxFileWriter = null;

            if (mShpFile.CanWrite)
            {
                mShpFileWriter = new BinaryWriter(mShpFile);
            }

            if (mShxFile.CanWrite)
            {
                mShxFileWriter = new BinaryWriter(mShxFile);
            }

            mFileCreat = true;
            if (mShpFile != null)
            {
                mIsForwardOnly = !mShpFile.CanSeek;
            }

            mShpRecord = new ShpRecordData(mHeader);
            mShpRecord.StoreElevationData = StoreElevationData;
        }

        public void WriteShx()
        {
            if (mFileCreat)
            {
                ShxRecord.Write(mShxFileWriter);
            }
        }
        public void WriteShp(GISTerrainLoaderGeoVectorData GeoData)
        {
            if (mFileCreat)
            {
                mShpRecord.GetShapeInfo(GeoData);
                mHeader.WriteTotalFileData(mShpFileWriter);

                ShxRecord = new ShxRecord(mShpRecord.Header);
                ShxRecord.RecordDic = mShpRecord.WriteShpData(mShpFileWriter);
            }
        }
        public void WritePrj(string m_Shpfilepath,int EPSG)
        {
            GISTerrainLoaderEPSG.WritePrjFile(m_Shpfilepath.Replace("shp", "prj"), EPSG);
        }
        public void WriteDbf(string m_Shpfilepath)
        {
            DbfFile DbfFile = new DbfFile(Encoding.GetEncoding(936));

            DbfFile.Open(m_Shpfilepath.Replace("shp", "dbf"), FileMode.Create);

            List<DbfRecord> m_Records = new List<DbfRecord>();
            List<string> Item_Cols = new List<string>();

            foreach (var Item in mShpRecord.RecordedShapesDic)
            {
                foreach (var data in Item.Value.GeoShape.DataBase)
                {
                    if (!Item_Cols.Contains(data.Key))
                    {
                        Item_Cols.Add(data.Key);
                        var colName = data.Key;

                        if (colName.Length>11) colName = colName.Substring(0, 10);
                        DbfColumn New_Col = new DbfColumn(colName, DbfColumn.DbfColumnType.Character, 120, 0);
                        DbfFile.Header.AddColumn(New_Col);
                    }
                }
            }
            
            foreach (var Item in mShpRecord.RecordedShapesDic)
            {
                DbfRecord Item_Record = new DbfRecord(DbfFile.Header);

                foreach (var data in Item.Value.GeoShape.DataBase)
                {
                    if (Item_Cols.Contains(data.Key))
                    {
                        Item_Cols.Add(data.Key);
                        int index = DbfFile.Header.FindColumn(data.Key);
                        Item_Record[index] = data.Value;

                    }

                }
                m_Records.Add(Item_Record);
            }

            foreach (var re in m_Records)
                DbfFile.Write(re);

            DbfFile.Close();
        }

        public void Close()
        {
            mHeader = new GISTerrainLoaderShpFileHeader();
            mHeaderWritten = false;

            if (mShpFileWriter != null)
            {
                mShpFileWriter.Flush();
                mShpFileWriter.Close();
            }

            if (mShpFile != null)
            {
                mShpFile.Close();
            }

            if (mShxFileWriter != null)
            {
                mShxFileWriter.Flush();
                mShxFileWriter.Close();
            }

            if (mShxFile != null)
            {
                mShxFile.Close();
            }

            mShpFileWriter = null;
            mShpFile = null;

            mShxFileWriter = null;
            mShxFile = null;
            mFileName = "";
        }
    }
    public class ShpRecordData
    {
        double xmin = 0.0;
        double ymin = 0.0;
        double xmax = 0.0;
        double ymax = 0.0;

        double zmin = 0.0;
        double zmax = 0.0;

        public bool StoreElevationData;

        private GISTerrainLoaderShpFileHeader mHeader = null;

        public Dictionary<string, ShpData> RecordedShapesDic;
        public GISTerrainLoaderShpFileHeader Header
        {
            get
            {
                return mHeader;
            }
            set
            {
                mHeader = value;
            }
        }
        public ShpRecordData(GISTerrainLoaderShpFileHeader oHeader)
        {
            mHeader = oHeader;
        }
        public void GetShapeInfo(GISTerrainLoaderGeoVectorData GeoData)
        {
            int index = 0;

            if (RecordedShapesDic != null)
            {
                RecordedShapesDic.Clear();
            }
            else
            {
                RecordedShapesDic = new Dictionary<string, ShpData>();
            }

            if (GeoData != null)
            {
                for (int i = 0; i < GeoData.GeoPoints.Count; i++)
                {
                    index++;
                    var point = GeoData.GeoPoints[i];

                    ShpData shpData = new ShpData();
                    shpData.StoreElevationData = StoreElevationData;
                    shpData.LoadData(GeoData.GeoPoints[i]);

                    point.ID = (index).ToString();

                    shpData.GeoShape.RecordNum = point.ID;
                    shpData.GeoShape.DataBase = point.DataBase;
                    RecordedShapesDic.Add(point.ID, shpData);
                 
                }

                for (int i = 0; i < GeoData.GeoLines.Count; i++)
                {
                    index++;
                    var line = GeoData.GeoLines[i];
                    ShpData shpData = new ShpData();
                    shpData.StoreElevationData = StoreElevationData;
                    shpData.LoadData(line);
                    line.ID = (index).ToString();

                    shpData.GeoShape.RecordNum = line.ID;
                    shpData.GeoShape.DataBase = line.DataBase;
                    RecordedShapesDic.Add(line.ID, shpData);
                }
                for (int i = 0; i < GeoData.GeoPolygons.Count; i++)
                {
                    index++;
                    var polygon = GeoData.GeoPolygons[i];
                    ShpData shpData = new ShpData();
                    shpData.StoreElevationData = StoreElevationData;
                    shpData.LoadData(polygon);
                    polygon.ID = (index).ToString();
                    shpData.GeoShape.RecordNum = polygon.ID;
                    shpData.GeoShape.DataBase = polygon.DataBase;
                    RecordedShapesDic.Add(polygon.ID, shpData);

                }

            }

            SetShpHeaderInfo();
        }

        private void SetShpHeaderInfo()
        {
            if (mHeader == null)
            {
                mHeader = new GISTerrainLoaderShpFileHeader();
            }

            mHeader.FileCode = 9994;
            mHeader.FileLength = 50;
            this.mHeader.TotalXYRange.MinX = 0.0;
            this.mHeader.TotalXYRange.MinY = 0.0;
            this.mHeader.TotalXYRange.MaxX = 0.0;
            this.mHeader.TotalXYRange.MaxY = 0.0;

            this.mHeader.ZRange.Min = 0.0;
            this.mHeader.ZRange.Max = 0.0;
            this.mHeader.MRange.Min = 0.0;
            this.mHeader.MRange.Max = 0.0;

            foreach (var item in RecordedShapesDic)
            {
                mHeader.FileLength += item.Value.GeoShape.DataLength + 4;
                mHeader.FileVersion = 1000;
                mHeader.ShpType = item.Value.GeoShape.GeoType;
                GetTotalMaxMinBorder(item.Value, mHeader.ShpType);
            }


        }
        private void GetTotalMaxMinBorder(ShpData shp, ShapeType ShpType)
        {
            switch (ShpType)
            {
                case ShapeType.Point:
                    {
                        Point eVPoint = shp.GeoShape as Point;

                        if (eVPoint.X < xmin)
                        {
                            xmin = eVPoint.X;
                        }

                        if (eVPoint.X>xmax)
                        {
                            xmax = eVPoint.X;
                        }

                        if (eVPoint.Y< ymin )
                        {
                            ymin = eVPoint.Y;
                        }

                        if (eVPoint.Y > ymax)
                        {
                            ymax = eVPoint.Y;
                        }
 
                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;

                        break;

                    }
                case ShapeType.PointZ:
                    {


                        PointZ eVPoint = shp.GeoShape as PointZ;

                        if (eVPoint.X < xmin)
                        {
                            xmin = eVPoint.X;
                        }

                        if (eVPoint.X > xmax)
                        {
                            xmax = eVPoint.X;
                        }

                        if (eVPoint.Y < ymin)
                        {
                            ymin = eVPoint.Y;
                        }

                        if (eVPoint.Y > ymax)
                        {
                            ymax = eVPoint.Y;
                        }

                        if (eVPoint.Z < zmin)
                        {
                            zmin = eVPoint.Z;
                        }

                        if (eVPoint.Z > zmax)
                        {
                            zmax = eVPoint.Z;
                        }

                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;
                        this.mHeader.ZRange.Min = zmin;
                        this.mHeader.ZRange.Max = zmax;

                        break;
                    }
                case ShapeType.PolyLine:
                    {
                        PolyLine eVPolyLine2 = shp.GeoShape as PolyLine;
 
                        if (eVPolyLine2.XYRange.MinX < xmin)
                        {
                            xmin = eVPolyLine2.XYRange.MinX;
                        }

                        if (eVPolyLine2.XYRange.MaxX > xmax)
                        {
                            xmax = eVPolyLine2.XYRange.MaxX;
                        }

                        if (eVPolyLine2.XYRange.MinY < ymin)
                        {
                            ymin = eVPolyLine2.XYRange.MinY;
                        }

                        if (eVPolyLine2.XYRange.MaxY > ymax)
                        {
                            ymax = eVPolyLine2.XYRange.MaxY;
                        }

                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;


                        break;
                    }
                case ShapeType.PolyLineZ:
                    {
                        PolyLineZ eVPolyLine2 = shp.GeoShape as PolyLineZ;

                        if (eVPolyLine2.XYRange.MinX < xmin)
                        {
                            xmin = eVPolyLine2.XYRange.MinX;
                        }

                        if (eVPolyLine2.XYRange.MaxX > xmax)
                        {
                            xmax = eVPolyLine2.XYRange.MaxX;
                        }

                        if (eVPolyLine2.XYRange.MinY < ymin)
                        {
                            ymin = eVPolyLine2.XYRange.MinY;
                        }

                        if (eVPolyLine2.XYRange.MaxY > ymax)
                        {
                            ymax = eVPolyLine2.XYRange.MaxY;
                        }

                        if (eVPolyLine2.ZRange.Min < zmin)
                        {
                            zmin = eVPolyLine2.ZRange.Min;
                        }
                        if (eVPolyLine2.ZRange.Max > zmax)
                        {
                            zmax = eVPolyLine2.ZRange.Max;
                        }

                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;
                        this.mHeader.ZRange.Min = zmin;
                        this.mHeader.ZRange.Max = zmax;
                        break;
                    }
                case ShapeType.Polygon:
                    {                            
                        Polygon eVPolygon2 = shp.GeoShape as Polygon;

                        if (eVPolygon2.XYRange.MinX < xmin)
                        {
                            xmin = eVPolygon2.XYRange.MinX;
                        }

                        if (eVPolygon2.XYRange.MaxX > xmax)
                        {
                            xmax = eVPolygon2.XYRange.MaxX;
                        }

                        if (eVPolygon2.XYRange.MinY < ymin)
                        {
                            ymin = eVPolygon2.XYRange.MinY;
                        }

                        if (eVPolygon2.XYRange.MaxY > ymax)
                        {
                            ymax = eVPolygon2.XYRange.MaxY;
                        }
 

                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;
 

                        break;
                    }
                case ShapeType.PolygonZ:
                    {                            
                        PolygonZ eVPolygon2 = shp.GeoShape as PolygonZ;


                        if (eVPolygon2.XYRange.MinX < xmin)
                        {
                            xmin = eVPolygon2.XYRange.MinX;
                        }

                        if (eVPolygon2.XYRange.MaxX > xmax)
                        {
                            xmax = eVPolygon2.XYRange.MaxX;
                        }

                        if (eVPolygon2.XYRange.MinY < ymin)
                        {
                            ymin = eVPolygon2.XYRange.MinY;
                        }

                        if (eVPolygon2.XYRange.MaxY > ymax)
                        {
                            ymax = eVPolygon2.XYRange.MaxY;
                        }

                        if (eVPolygon2.ZRange.Min < zmin)
                        {
                            zmin = eVPolygon2.ZRange.Min;
                        }
                        if (eVPolygon2.ZRange.Max > zmax)
                        {
                            zmax = eVPolygon2.ZRange.Max;
                        }

                        this.mHeader.TotalXYRange.MinX = xmin;
                        this.mHeader.TotalXYRange.MinY = ymin;
                        this.mHeader.TotalXYRange.MaxX = xmax;
                        this.mHeader.TotalXYRange.MaxY = ymax;
                        this.mHeader.ZRange.Min = zmin;
                        this.mHeader.ZRange.Max = zmax;

                        break;
                    }

                case (ShapeType)4:
                    break;
            }
        }
        public Dictionary<string, ShxData> WriteShpData(BinaryWriter writer)
        {
            Dictionary<string, ShxData> ShxDatadictionary = new Dictionary<string, ShxData>();
            
            ShpData shpdata = null;
            ShxData shxData = new ShxData();
            byte[] bytes = null;
            byte[] buffer = null;

            if (writer.BaseStream.Position == 0)
            {
                mHeader.WriteTotalFileData(writer);
            }

            foreach (var shap in RecordedShapesDic)
            {
                int num = 50;

                var geometry = shap.Value.GeoShape.GeoType;

                shpdata = shap.Value;

                switch (geometry)
                {
                    case ShapeType.Point:
                        {
                            Point eVPoint = shpdata.GeoShape as Point;
                            shxData = new ShxData();
                            shxData.ContentLength = eVPoint.DataLength;
                            shxData.Offset = num;
                            num += shxData.ContentLength + 4;
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPoint.RecordNum)));
                            writer.Write(buffer);
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPoint.DataLength));
                            writer.Write(buffer);
                            writer.Write(1);
                            writer.Write(eVPoint.X);
                            writer.Write(eVPoint.Y);
                            break;
                        }
                    case ShapeType.PointZ:
                        {
                            PointZ eVPoint = shpdata.GeoShape as PointZ;
                            shxData = new ShxData();
                            shxData.ContentLength = eVPoint.DataLength;
                            shxData.Offset = num;
                            num += shxData.ContentLength + 8;
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPoint.RecordNum)));
                            writer.Write(buffer);
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPoint.DataLength));
                            writer.Write(buffer);
                            writer.Write(1);
                            writer.Write(eVPoint.X);
                            writer.Write(eVPoint.Y);
                            writer.Write(eVPoint.Z);
                        }
                        break;
                    case ShapeType.PolyLine:
                        {
                            PolyLine eVPolyLine = shpdata.GeoShape as PolyLine;
                            shxData = new ShxData();
                            shxData.ContentLength = eVPolyLine.DataLength;
                            shxData.Offset = num;
                            num += shxData.ContentLength + 4;
 
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPolyLine.RecordNum)));
                            writer.Write(buffer);
                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPolyLine.DataLength));
                            writer.Write(buffer);

                            writer.Write((int)eVPolyLine.GeoType);

                            writer.Write(eVPolyLine.XYRange.MinX);
                            writer.Write(eVPolyLine.XYRange.MinY);
                            writer.Write(eVPolyLine.XYRange.MaxX);
                            writer.Write(eVPolyLine.XYRange.MaxY);

                            writer.Write(eVPolyLine.NumParts);
                            writer.Write(eVPolyLine.NumPoints);

                            for (int j = 0; j < eVPolyLine.NumParts; j++)
                            {
                                writer.Write((int)eVPolyLine.Parts[j]);
                            }

                            for (int j = 0; j < eVPolyLine.NumPoints; j++)
                            {
                                Point eVPointz = eVPolyLine.Points[j];
                                writer.Write(eVPointz.X);
                                writer.Write(eVPointz.Y);
                            }
                            break;
                        }
                    case ShapeType.PolyLineZ:
                        {
                            PolyLineZ eVPolyLine = shpdata.GeoShape as PolyLineZ;

                            var shxDatax = new ShxData();
                            shxDatax.ContentLength = eVPolyLine.DataLength;
                            shxDatax.Offset = num;
                            num += shxData.ContentLength + 8;

                            var buffer1 = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPolyLine.RecordNum)));
                            writer.Write(buffer1);

                            var buffer2 = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPolyLine.DataLength));
                            writer.Write(buffer2);

                            writer.Write((int)eVPolyLine.GeoType);

                            writer.Write(eVPolyLine.XYRange.MinX);
                            writer.Write(eVPolyLine.XYRange.MinY);
                            writer.Write(eVPolyLine.XYRange.MaxX);
                            writer.Write(eVPolyLine.XYRange.MaxY);

                            writer.Write(eVPolyLine.NumParts);
                            writer.Write(eVPolyLine.NumPoints);

                            for (int j = 0; j < eVPolyLine.NumParts; j++)
                            {
                                writer.Write((int)eVPolyLine.Parts[j]);
                            }

                            for (int j = 0; j < eVPolyLine.NumPoints; j++)
                            {
                                Point eVPointz = eVPolyLine.Points[j] as Point;
                                writer.Write(eVPointz.X);
                                writer.Write(eVPointz.Y);

                            }
                            writer.Write(eVPolyLine.ZRange.Min);
                            writer.Write(eVPolyLine.ZRange.Max);

                            for (int j = 0; j < eVPolyLine.ZValues.Length; j++)
                            {
                                writer.Write(eVPolyLine.ZValues[j]);
                            }

                            break;
                        }

                    case ShapeType.Polygon:
                        {
                            Polygon eVPolygon = shpdata.GeoShape as Polygon;

                            shxData = new ShxData();
                            shxData.ContentLength = eVPolygon.DataLength;
 
                            shxData.Offset = num;
                            num += shxData.ContentLength + 4;

                            bytes = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPolygon.RecordNum)));
                            writer.Write(bytes);
                            byte[] buffer2 = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPolygon.DataLength));
                            writer.Write(buffer2);

                            writer.Write((int)eVPolygon.GeoType);

                            writer.Write(eVPolygon.XYRange.MinX);
                            writer.Write(eVPolygon.XYRange.MinY);
                            writer.Write(eVPolygon.XYRange.MaxX);
                            writer.Write(eVPolygon.XYRange.MaxY);

                            writer.Write(eVPolygon.NumParts);
                            writer.Write(eVPolygon.NumPoints);

                            for (int j = 0; j < eVPolygon.NumParts; j++)
                            {
                                writer.Write((int)eVPolygon.Parts[j]);
                            }

                            for (int j = 0; j < eVPolygon.NumPoints; j++)
                            {
                                var eVPointx = eVPolygon.Points[j] as Point;
                                writer.Write(eVPointx.X);
                                writer.Write(eVPointx.Y);
                            }


                            break;
                        }
                    case ShapeType.PolygonZ:
                        {
                            PolygonZ eVPolygon = shpdata.GeoShape as PolygonZ;

                            shxData = new ShxData();
                            shxData.ContentLength = eVPolygon.DataLength;
                            shxData.Offset = num;
                            num += shxData.ContentLength + 8;

                            buffer = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(ulong.Parse(eVPolygon.RecordNum)));
                            writer.Write(buffer);
                            byte[] buffer2 = GISTerrainLoaderExtensions.FromLittleToBig(BitConverter.GetBytes(eVPolygon.DataLength));
                            writer.Write(buffer2);

                            writer.Write((int)eVPolygon.GeoType);

                            writer.Write(eVPolygon.XYRange.MinX);
                            writer.Write(eVPolygon.XYRange.MinY);
                            writer.Write(eVPolygon.XYRange.MaxX);
                            writer.Write(eVPolygon.XYRange.MaxY);

                            writer.Write(eVPolygon.NumParts);
                            writer.Write(eVPolygon.NumPoints);

                            for (int j = 0; j < eVPolygon.NumParts; j++)
                            {
                                writer.Write((int)eVPolygon.Parts[j]);
                            }

                            for (int j = 0; j < eVPolygon.NumPoints; j++)
                            {
                                var eVPointx = eVPolygon.Points[j] as Point;
                                writer.Write(eVPointx.X);
                                writer.Write(eVPointx.Y);
                            }

                            writer.Write(eVPolygon.ZRange.Min);
                            writer.Write(eVPolygon.ZRange.Max);

                            for (int j = 0; j < eVPolygon.ZValues.Length; j++)
                            {
                                writer.Write(eVPolygon.ZValues[j]);
                            }
 

                            break;
                        }


                }
            }

            return ShxDatadictionary;
        }

        public int GetRecordCount()
        {
            return (RecordedShapesDic != null) ? RecordedShapesDic.Count : 0;
        }
    }
    public class ShpData
    {
        public bool StoreElevationData;

        private GISTerrainLoaderIElement mGeoShape;
 
        public GISTerrainLoaderIElement GeoShape
        {
            get
            {
                return mGeoShape;
            }
            set
            {
                mGeoShape = value;
            }
        }

        public void LoadData(GISTerrainLoaderPointGeoData data)
        {

            if (StoreElevationData == false)
            {
                mGeoShape = new Point();
                mGeoShape.ReadFromGeoData(data);
            }else
            {
                mGeoShape = new PointZ();
                mGeoShape.ReadFromGeoData(data);
            }


        }
        public void LoadData(GISTerrainLoaderLineGeoData data)
        {
            if (StoreElevationData == false)
            {
                mGeoShape = new PolyLine();
                mGeoShape.ReadFromGeoData(data);
            }
            else
            {
                mGeoShape = new PolyLineZ();
                mGeoShape.ReadFromGeoData(data);
            }
        }
        public void LoadData(GISTerrainLoaderPolygonGeoData data)
        {
            if (StoreElevationData == false)
            {
                mGeoShape = new Polygon();
                mGeoShape.ReadFromGeoData(data);
            }
            else
            {
                mGeoShape = new PolygonZ();
                mGeoShape.ReadFromGeoData(data);
            }
        }
    }
    public class ShxRecord
    {
        private GISTerrainLoaderShpFileHeader mHeader = null;

        private Dictionary<string, ShxData> mRecordDic = null;

        public GISTerrainLoaderShpFileHeader Header
        {
            get
            {
                return this.mHeader;
            }
            set
            {
                this.mHeader = value;
            }
        }

        public Dictionary<string, ShxData> RecordDic
        {
            get
            {
                return this.mRecordDic;
            }
            set
            {
                this.mRecordDic = value;
            }
        }

        public ShxRecord(GISTerrainLoaderShpFileHeader oHeader)
        {
            this.mHeader = oHeader;
        }
        public void Write(BinaryWriter writer)
        {
            if (this.mRecordDic != null && this.mHeader != null)
            {
                writer.Flush();
                byte[] lbtFileCode = System.BitConverter.GetBytes(this.mHeader.FileCode);
                byte[] bbtFileCode = GISTerrainLoaderExtensions.FromLittleToBig(lbtFileCode);
                writer.Write(bbtFileCode);
                int Unused = 0;
                byte[] lbtUnused = System.BitConverter.GetBytes(Unused);
                byte[] bbtUnused = GISTerrainLoaderExtensions.FromLittleToBig(lbtUnused);

                for (int i = 0; i < 5; i++)
                {
                    writer.Write(bbtUnused);
                }

                int mFileLength = 50 + this.mRecordDic.Count * 4;
                byte[] lbtFileLength = System.BitConverter.GetBytes(mFileLength);
                byte[] bbtFileLength = GISTerrainLoaderExtensions.FromLittleToBig(lbtFileLength);
                writer.Write(bbtFileLength);

                writer.Write(this.mHeader.FileVersion);
                int tyeptemp = (int)this.mHeader.ShpType;
                writer.Write(tyeptemp);
                writer.Write(this.mHeader.TotalXYRange.MinX);
                writer.Write(this.mHeader.TotalXYRange.MinY);
                writer.Write(this.mHeader.TotalXYRange.MaxX);
                writer.Write(this.mHeader.TotalXYRange.MaxY);

                writer.Write(this.mHeader.ZRange.Min);
                writer.Write(this.mHeader.ZRange.Max);

                writer.Write(this.mHeader.MRange.Min);
                writer.Write(this.mHeader.MRange.Max);

                foreach (var mRec in mRecordDic)
                {
                    var Key = mRec.Key;
                    byte[] lbtOffset = BitConverter.GetBytes(this.mRecordDic[Key].Offset);
                    byte[] bbtOffset = GISTerrainLoaderExtensions.FromLittleToBig(lbtOffset);
                    writer.Write(bbtOffset);
                    byte[] lbtContentLength = BitConverter.GetBytes(this.mRecordDic[Key].ContentLength);
                    byte[] bbtContentLength = GISTerrainLoaderExtensions.FromLittleToBig(lbtContentLength);
                    writer.Write(bbtContentLength);
                }
            }
        }
    }
    public class ShxData
    {
        public int Offset;

        public int ContentLength;
    }
}
