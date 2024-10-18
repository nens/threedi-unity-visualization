/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeReader
    {
        public static List<GISTerrainLoaderShpFileHeader> LoadShapes(string[] shapes)
        {
            List<GISTerrainLoaderShpFileHeader> shapesFile = new List<GISTerrainLoaderShpFileHeader>();

            if (shapes.Length > 0)
            {
                foreach (var shp in shapes)
                {
                    var shpfile = LoadFile(shp) as GISTerrainLoaderShpFileHeader;
                    shapesFile.Add(shpfile);
                }
                return shapesFile;
            }
            else return null;
        }
        public static GISTerrainLoaderShpFileHeader LoadShape(string shape)
        {
            GISTerrainLoaderShpFileHeader shapeFile = LoadFile(shape) as GISTerrainLoaderShpFileHeader;

            return shapeFile;
        }
        public static GISTerrainLoaderIFile LoadFile(string path)
        {
            try
            {
                GISTerrainLoaderIFile file;
                string fileExt = Path.GetExtension(path);
                file = GISTerrainLoaderFileFactory.CreateInstance(path);
                file.Load();
                return file;
            }
            catch (Exception e)
            {
                if (path.Length == 0)
                {
                    Debug.Log("Path is empty.");
                    return null;
                }
                Debug.Log("Could Not Read Shape File " + e);
                return null;
            }
        }
        public static GISTerrainLoaderIFile LoadFile(byte[] data, string path)
        {
            try
            {
                GISTerrainLoaderIFile file;
                string fileExt = Path.GetExtension(path);
                file = GISTerrainLoaderFileFactory.CreateInstancePlatformes(data, path);
                file.Load();
                return file;
            }
            catch (Exception e)
            {
                if (path.Length == 0)
                {
                    Debug.Log("Path is empty.");
                    return null;
                }
                Debug.Log(e);
                return null;
            }
        }


    }
    public class GISTerrainLoaderShapeFileData
    {
        public ShapeType ShapeType;
        public List<GISTerrainLoaderGeoDataBase> DataBase
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }
        public string LAYER
        {
            get;
            set;
        }

        public GISTerrainLoaderShpRecord ShapeRecord;

        public GISTerrainLoaderShapeFileData(ShapeType m_ShapeType, GISTerrainLoaderShpRecord m_shapeRecord, GISTerrainLoaderVectorParameters_SO VectorParameters)
        {
            ShapeType = m_ShapeType;
            ShapeRecord = m_shapeRecord;

            DataBase = new List<GISTerrainLoaderGeoDataBase>();

            foreach (var data in m_shapeRecord.DataBase)
            {
                try
                {
                    var tagKeyEnum = data.Col_Name;

                    if (data.Col_Name.Trim() == VectorParameters.ID_Tag.Trim())
                        Id = data.Row_Value;

                    if (data.Col_Name.Trim() == VectorParameters.Layer_Tag.Trim())
                        LAYER = data.Row_Value;
 
                    if (tagKeyEnum != VectorParameters.Null_Tag && !string.IsNullOrEmpty(data.Row_Value.Trim()))
                    {
                        var element = DataBase.Find(x => x.Key == tagKeyEnum);
                        if (element==null)
                            DataBase.Add(new GISTerrainLoaderGeoDataBase( tagKeyEnum, data.Row_Value.Trim()));
                    }

                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

    }
    #region ShapeFile
    public class GISTerrainLoaderShpFileHeader : GISTerrainLoaderIShpFile
    {
        private bool disposed;
        private FileStream fs;
        private BinaryReader br;
        private Stream stream;
        public string FilePath { get; set; }
        public int FileCode { get; set; }
        public int FileLength { get; set; }
        public int FileVersion { get; set; }
        public ShapeType ShpType { get; set; }
        public GISTerrainLoaderRangeXY TotalXYRange { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public int ContentLength { get; set; }
        public List<GISTerrainLoaderShpRecord> RecordSet { get; set; }
        public GISTerrainLoaderShpFileHeader()
        {
            TotalXYRange = new GISTerrainLoaderRangeXY();
            ZRange = new GISTerrainLoaderRange();
            MRange = new GISTerrainLoaderRange();
        }
        public GISTerrainLoaderShpFileHeader(string path)
        {

            FilePath = path;
            fs = File.OpenRead(path);
            br = new BinaryReader(fs);
         }
        public GISTerrainLoaderShpFileHeader(byte[] ShpData, string path)
        {

            FilePath = path;
            stream = new MemoryStream(ShpData);
            br = new BinaryReader(stream);

        }
        ~GISTerrainLoaderShpFileHeader()
        {
            Dispose(false);
        }

        public void Load()
        {
            FileCode = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32());
            br.BaseStream.Seek(20, SeekOrigin.Current);
            FileLength = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32())*2 ;

            FileVersion = br.ReadInt32();
            ShpType = (ShapeType)br.ReadInt32();
            TotalXYRange = new GISTerrainLoaderRangeXY();
            ZRange = new GISTerrainLoaderRange();
            MRange = new GISTerrainLoaderRange();
            TotalXYRange.Load(ref br);
            ZRange.Load(ref br);
            MRange.Load(ref br);

            ContentLength = FileLength - 100;
            long curPoint = 0;

            RecordSet = new List<GISTerrainLoaderShpRecord>();

            while (curPoint < ContentLength)
            {
                GISTerrainLoaderShpRecord record = new GISTerrainLoaderShpRecord(ShpType);
                record.Load(ref br);
                RecordSet.Add(record);
                curPoint += record.GetLength();
            }

            br.Close();
        }

        public GISTerrainLoaderIRecord GetData(int index)
        {
            return RecordSet.ElementAt(index);
        }

        public GISTerrainLoaderIRecord GetData(ShapeType type, int offset, int length)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            GISTerrainLoaderIRecord record = new GISTerrainLoaderShpRecord(type);
            record.Load(ref br);
            return record;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                br.Dispose();
                fs.Dispose();
                stream.Dispose();
            }
            disposed = true;
        }

        public void WriteTotalFileData(BinaryWriter writer)
        {
            writer.Flush();
            byte[] lbtFileCode = BitConverter.GetBytes(FileCode);
            byte[] bbtFileCode = GISTerrainLoaderExtensions.FromLittleToBig(lbtFileCode);
            writer.Write(bbtFileCode);
            int Unused = 0;
            byte[] lbtUnused = BitConverter.GetBytes(Unused);
            byte[] bbtUnused = GISTerrainLoaderExtensions.FromLittleToBig(lbtUnused);
            for (int i = 0; i < 5; i++)
            {
                writer.Write(bbtUnused);
            }
            byte[] lbtFileLength = BitConverter.GetBytes(FileLength);
            byte[] bbtFileLength = GISTerrainLoaderExtensions.FromLittleToBig(lbtFileLength);
            writer.Write(bbtFileLength);
            writer.Write(FileVersion);
            int tyeptemp = (int)ShpType;
            writer.Write(tyeptemp);
            writer.Write(TotalXYRange.MinX);
            writer.Write(TotalXYRange.MinY);
            writer.Write(TotalXYRange.MaxX);
            writer.Write(TotalXYRange.MaxY);
            writer.Write(ZRange.Min);
            writer.Write(ZRange.Max);
            writer.Write(MRange.Min);
            writer.Write(MRange.Max);
        }
    }
    public interface GISTerrainLoaderIShpFile : GISTerrainLoaderIFile
    {
        int FileCode { get; set; }
        int FileLength { get; set; }
        int FileVersion { get; set; }
        ShapeType ShpType { get; set; }
        GISTerrainLoaderRangeXY TotalXYRange { get; set; }
        GISTerrainLoaderRange ZRange { get; set; }
        GISTerrainLoaderRange MRange { get; set; }
        int ContentLength { get; set; }

    }
    public class GISTerrainLoaderShapeFactory
    {
        public static readonly IDictionary<ShapeType, Func<GISTerrainLoaderIElement>> Creators =
            new Dictionary<ShapeType, Func<GISTerrainLoaderIElement>>()
            {
                { ShapeType.Point, () => new Point() },
                { ShapeType.PolyLine, () => new PolyLine() },
                { ShapeType.Polygon, () => new Polygon() },
                { ShapeType.MultiPoint, () => new MultiPoint() },
                { ShapeType.PointM, () => new PointM() },
                { ShapeType.PolyLineM, () => new PolyLineM() },
                { ShapeType.PolygonM, () => new PolygonM() },
                { ShapeType.MultiPointM, () => new MultiPointM() },
                { ShapeType.PointZ, () => new PointZ() },
                { ShapeType.PolyLineZ, () => new PolyLineZ() },
                { ShapeType.PolygonZ, () => new PolygonZ() },
                { ShapeType.MultiPointZ, () => new MultiPointZ() },
                { ShapeType.MultiPatch, () => new MultiPatch() }
            };


        public static Point[] GetTypePoint(ShapeType type, GISTerrainLoaderIElement Contents)
        {
            Point[] points = null;

            switch (type)
            {
                case ShapeType.Point:
                    var p = (Point)Contents;
                    var pp = new Point(); pp.X = p.X; pp.Y = p.Y;
                    points = new Point[1] { pp };
                    break;
                case ShapeType.PointZ:
                    var pz = (PointZ)Contents as PointZ;
                    pp = new Point(); pp.X = pz.X; pp.Y = pz.Y;
                    points = new Point[1] { pp };
                    break;
                case ShapeType.PolyLine:
                    var PolyLine = (PolyLine)Contents;
                    points = PolyLine.Points;
                    break;
                case ShapeType.Polygon:
                    var Polygon = (Polygon)Contents as Polygon;
                    points = Polygon.Points;
                    break;
                case ShapeType.MultiPoint:
                    var MultiPoint = (MultiPoint)Contents as MultiPoint;
                    points = MultiPoint.Points;
                    break;
                case ShapeType.PolyLineZ:
                    var PolyLineZ = (PolyLineZ)Contents as PolyLineZ;
                    points = PolyLineZ.Points;
                    break;
                case ShapeType.PolygonZ:
                    var PolygonZ = (PolygonZ)Contents as PolygonZ;
                    points = PolygonZ.Points;
                    break;
            }

            return points;
        }
        public static double[] GetElevation(ShapeType type, GISTerrainLoaderIElement Contents)
        {
            double[] Elevation = new double[1];
            switch (type)
            {
                case ShapeType.Point:
                    var p = (Point)Contents;
                    var pp = new Point(); pp.X = p.X; pp.Y = p.Y;
                    break;
                case ShapeType.PointZ:
                    var pz = (PointZ)Contents;         
                    Elevation[0] = pz.Z;
                    break;
                case ShapeType.PolyLine:
                    var PolyLine = (PolyLine)Contents;
                    break;
                case ShapeType.Polygon:
                    var Polygon = (Polygon)Contents as Polygon;
                    break;
                case ShapeType.MultiPoint:
                    var MultiPoint = (MultiPoint)Contents as MultiPoint;
                    break;
                case ShapeType.PolyLineZ:
                    var PolyLineZ = (PolyLineZ)Contents as PolyLineZ;
                    Elevation = PolyLineZ.ZValues;
                    break;
                case ShapeType.PolygonZ:
                    var PolygonZ = (PolygonZ)Contents as PolygonZ;
                    Elevation = PolygonZ.ZValues;
                    break;
            }
            return Elevation;
        }


        public static GISTerrainLoaderIElement CreateInstance(ShapeType shapeType)
        {
            return Creators[shapeType]();
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1), Serializable]
    public class GISTerrainLoaderShpRecord : GISTerrainLoaderIRecord
    {
        public GISTerrainLoaderShpRecordHeader Header { get; set; }
        public GISTerrainLoaderIElement Contents { get; set; }
        public string Tag { get; set; }
        public List<GISTerrainLoaderShpDataBase> DataBase { get; set; }
        public Dictionary<Enum, string> Tags
        {
            get;
            set;
        }
        public GISTerrainLoaderShpRecord(ShapeType type)
        {
            Header = new GISTerrainLoaderShpRecordHeader();
            Contents = GISTerrainLoaderShapeFactory.CreateInstance(type);
            Tags = new Dictionary<Enum, string>();
        }

        public void Load(ref BinaryReader br)
        {
            Header.Load(ref br);
            Contents.Load(ref br);
        }

        public long GetLength()
        {
            return Header.GetLength() + Contents.GetLength();

        }

    }
    public class GISTerrainLoaderShpDataBase
    {
        public string Col_Name;
        public string Row_Value;
        public bool Generated = false;
        public GISTerrainLoaderShpDataBase(string m_Col_Name, string m_Row_Value)
        {
            Col_Name = m_Col_Name;
            Row_Value = m_Row_Value;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1), Serializable]
    public class GISTerrainLoaderShpRecordHeader : GISTerrainLoaderIRecord
    {
        public int RecordNumber { get; set; }
        public int ContentLength { get; set; }
        public ShapeType Type { get; set; }

        public void Load(ref BinaryReader br)
        {
            RecordNumber = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32());
            ContentLength = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32()) * 2;
            Type = (ShapeType)br.ReadInt32();
        }

        public long GetLength()
        {
            return Marshal.SizeOf(this);
        }
    }
    #endregion
    public interface GISTerrainLoaderIFile
    {
        void Load();
        GISTerrainLoaderIRecord GetData(int index);
    }
    public class GISTerrainLoaderRangeXY : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }

        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public double Width { get { return MaxX - MinX; } }
        public double Height { get { return MaxY - MinY; } }
 
        public void Load(ref BinaryReader br)
        {
            MinX = br.ReadDouble();
            MaxX = br.ReadDouble();
            MinY = br.ReadDouble();
            MaxY = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 4;
        }
    }
    public class GISTerrainLoaderRange : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get ; set; }

        public double Min { get; set; }
        public double Max { get; set; }

        public void Load(ref BinaryReader br)
        {
            Min = br.ReadDouble();
            Max = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 2;
        }
    }
    public class GISTerrainLoaderFileFactory
    {
        public static readonly IDictionary<string, Func<string, GISTerrainLoaderIFile>> Creators =
            new Dictionary<string, Func<string, GISTerrainLoaderIFile>>()
            {

                {
                    ".shp", (path) => new GISTerrainLoaderShpFileHeader(path)
                }

            };

        public static readonly IDictionary<string, Func<byte[], string, GISTerrainLoaderIFile>> CreatorsAndroid =
    new Dictionary<string, Func<byte[], string, GISTerrainLoaderIFile>>()
    {

                {
                    ".shp",(data, path)  => new GISTerrainLoaderShpFileHeader(data,path)
                }

    };
        public static GISTerrainLoaderIFile CreateInstancePlatformes(byte[] data, string path)
        {
            GISTerrainLoaderIFile Ifile = null;
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    Ifile = CreatorsAndroid[Path.GetExtension(path)](data, path);
                    break;
                case RuntimePlatform.Android:
                case RuntimePlatform.WebGLPlayer:
                case RuntimePlatform.IPhonePlayer:
                    Ifile = CreatorsAndroid[Path.GetExtension(path)](data, path);
                    break;
            }
            return Ifile;

        }
        public static GISTerrainLoaderIFile CreateInstance(string path)
        {
            return Creators[Path.GetExtension(path)](path);

        }
    }
    public interface GISTerrainLoaderIRecord
    {
        void Load(ref BinaryReader br);
        long GetLength();
    }
    public interface GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
  
        virtual void ReadFromGeoData(GISTerrainLoaderPointGeoData point)
        {
        }
        virtual void ReadFromGeoData(GISTerrainLoaderLineGeoData point)
        {
        }
        virtual void ReadFromGeoData(GISTerrainLoaderPolygonGeoData point)
        {
        }
        void Load(ref BinaryReader br);
        long GetLength();
    }

    #region Types
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Point : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Point()
        {
        }
        public Point(double _x, double _y)
        {
            X = _x;
            Y = _y;
        }
 
        public void ReadFromGeoData(GISTerrainLoaderPointGeoData point)
        {
            X = point.GeoPoint.x; Y = point.GeoPoint.y;
            GeoType = ShapeType.Point;
            DataLength = 10;
        }
        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
        }
        public long GetLength()
        {
            return sizeof(double) * 2;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPoint : GISTerrainLoaderIElement
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public Point[] Points { get; set; }
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += (NumPoints * sizeof(double) * 2);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLine : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }
        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }
        }
        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            return size;
        }
        public void ReadFromGeoData(GISTerrainLoaderLineGeoData line)
        {
            XYRange = new GISTerrainLoaderRangeXY();

            Points = new Point[line.GeoPoints.Count];
            Parts = new int[line.GeoPoints.Count];

            for (int i = 0; i < line.GeoPoints.Count; i++)
            {
                var point = line.GeoPoints[i];

                Point eVPoint = new Point();
                eVPoint.X = point.GeoPoint.x;
                eVPoint.Y = point.GeoPoint.y;

                Points[i] = eVPoint;

            }

            GeoType = ShapeType.PolyLine;

            if (Parts.Length > 0)
            {
                Parts[0] = 0; 
                NumParts = 1;
                NumPoints = Points.Length;
                double[] border = GISTerrainLoaderVectorExtensions.GetBorder(Points);

                
                XYRange.MinX = border[0];
                XYRange.MinY = border[1];
                XYRange.MaxY = border[2];
                XYRange.MaxX = border[3];

                DataLength = 24 + 8 * NumPoints;
            }
            else
            {
                Parts[0] = 0; 
                NumParts = 0;
                NumPoints = 0;
                XYRange.MinX = 0.0;
                XYRange.MinY = 0.0;
                XYRange.MaxY = 0.0;
                XYRange.MaxX = 0.0;

                DataLength = 24 + 8 * NumPoints;
            }

        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Polygon : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);

            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();
            Parts = new int[NumParts];
 
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            return size;
        }

        public void ReadFromGeoData(GISTerrainLoaderPolygonGeoData Poly)
        {
            NumParts = Poly.GeoPoints.Count;

            int num = 0;
           
            for (int i = 0; i < NumParts; i++)
            {
                var subPart = Poly.GeoPoints[i];
                
                for (int j = 0; j < subPart.Count; j++)
                {
                    num++;
                }
            }

            NumPoints = num ;

            XYRange = new GISTerrainLoaderRangeXY();

            Points = new Point[NumPoints];

            Parts = new int[NumParts];

            List<Point> temp = new List<Point>();

            Parts[0] = 0;

            int PCount = 0;
            for (int i = 0; i < NumParts; i++)
            {
                var sublist = Poly.GeoPoints[i];
 
                if (i > 0)
                {
                    PCount += Poly.GeoPoints[i - 1].Count;
                    Parts[i] = PCount;
                    bool ClockWises = Poly.IsClockwise(sublist);
                    if (ClockWises)
                        sublist = Poly.CounterOrderToClockwise(sublist);
                }
                for (int j = 0; j < sublist.Count; j++)
                {
                    Point eVPoint = new Point();
                    eVPoint.X = sublist[j].GeoPoint.x;
                    eVPoint.Y = sublist[j].GeoPoint.y;

                    temp.Add(eVPoint);
                }
 
            }


            Points = temp.ToArray();

            GeoType = ShapeType.Polygon;

            if (Parts.Length > 0)
            {
                double[] border = GISTerrainLoaderVectorExtensions.GetBorder(Poly.GeoPoints);
                
                XYRange.MinX = border[0];
                XYRange.MinY = border[1];
                XYRange.MaxY = border[2];
                XYRange.MaxX = border[3];
 
                DataLength = 2 + (Convert.ToInt32(GetLength()) / 2);
            }
            else
            {
                Parts[0] = 0;
                NumParts = 0;
                NumPoints = 0;
                XYRange.MinX = 0.0;
                XYRange.MinY = 0.0;
                XYRange.MaxY = 0.0;
                XYRange.MaxX = 0.0;

                DataLength = 24 + 8 * NumPoints;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PointM : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double M { get; set; }

        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
            M = br.ReadDouble();
        }

        public long GetLength()
        {
            long size = 0;
            size += sizeof(double) * 3;
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPointM : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += (NumPoints * sizeof(double) * 2);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLineM : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolygonM : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();
            Parts = new int[NumParts];
            Points = new Point[NumPoints];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i].Load(ref br);
            }
            MRange.Load(ref br);
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PointZ : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double M { get; set; }

        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
            Z = br.ReadDouble();
            M = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 4;
        }


        public PointZ()
        {
        }
        public PointZ(DVector3 vcoor)
        {
            X = vcoor.x;
            Y = vcoor.y;
            Z = vcoor.z;
        }
        public void ReadFromGeoData(GISTerrainLoaderPointGeoData point)
        {
            X = point.GeoPoint.x; Y = point.GeoPoint.y; Z = point.Elevation;
            GeoType = ShapeType.PointZ;
            DataLength = 14;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPointZ : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += (NumPoints * sizeof(double) * 2);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLineZ : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();

            XYRange.Load(ref br);

            NumParts = br.ReadInt32();

            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];

            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();

            ZRange.Load(ref br);

            ZValues = new double[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }



        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            return size;
        }
        public void ReadFromGeoData(GISTerrainLoaderLineGeoData line)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            ZRange = new GISTerrainLoaderRange();

            Points = new Point[line.GeoPoints.Count];
            Parts = new int[line.GeoPoints.Count];

            ZValues = new double[line.GeoPoints.Count];

            for (int i = 0; i < line.GeoPoints.Count; i++)
            {
                var point = line.GeoPoints[i];

                Point eVPoint = new Point();
                eVPoint.X = point.GeoPoint.x;
                eVPoint.Y = point.GeoPoint.y;

                ZValues[i] = point.Elevation;

                Points[i] = eVPoint;

            }

            GeoType = ShapeType.PolyLineZ;

            if (Parts.Length > 0)
            {
                Parts[0] = 0;
                NumParts = 1;
                NumPoints = Points.Length;

                double[] border = GISTerrainLoaderVectorExtensions.GetBorder(Points, ZValues);

                XYRange.MinX = border[0];
                XYRange.MinY = border[1];
                XYRange.MaxY = border[2];
                XYRange.MaxX = border[3];

                ZRange.Min = border[4];
                ZRange.Max = border[5];

                DataLength = 32 + 8 * NumPoints + 4 * NumPoints;
 
            }
            else
            {
                Parts[0] = 0;
                NumParts = 0;
                NumPoints = 0;
                XYRange.MinX = 0.0;
                XYRange.MinY = 0.0;
                XYRange.MaxY = 0.0;
                XYRange.MaxX = 0.0;
                ZRange.Min  = 0.0;
                ZRange.Max = 0.0;

                DataLength = 32 + 8 * NumPoints + 4 * NumPoints;

            }

        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolygonZ : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();

            XYRange.Load(ref br);

            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();
            Parts = new int[NumParts];
 

            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();

            }

        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += (NumPoints * sizeof(double) * 2);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            return size;
        }
        public void ReadFromGeoData(GISTerrainLoaderPolygonGeoData Poly)
        {
            NumParts = Poly.GeoPoints.Count;

            int num = 0;

            for (int i = 0; i < NumParts; i++)
            {
                var subPart = Poly.GeoPoints[i];

                for (int j = 0; j < subPart.Count; j++)
                {
                    num++;
                }
            }

            NumPoints = num;

            XYRange = new GISTerrainLoaderRangeXY();
            ZRange = new GISTerrainLoaderRange();
            ZValues = new double[NumPoints];

            Points = new Point[NumPoints];

            Parts = new int[NumParts];

            List<Point> temp = new List<Point>();
            List<double> Zvals = new List<double>();
            Parts[0] = 0;
            int PCount = 0;
            for (int i = 0; i < NumParts; i++)
            {
                var sublist = Poly.GeoPoints[i];

                if (i > 0)
                {
                    PCount += Poly.GeoPoints[i - 1].Count;
                    Parts[i] = PCount;

                    bool ClockWises = Poly.IsClockwise(sublist);

                    if (!ClockWises)
                        sublist = Poly.CounterOrderToClockwise(sublist);
                }
                for (int j = 0; j < sublist.Count; j++)
                {
                    Point eVPoint = new Point();
                    eVPoint.X = sublist[j].GeoPoint.x;
                    eVPoint.Y = sublist[j].GeoPoint.y;
                    Zvals.Add(sublist[j].Elevation);
                    temp.Add(eVPoint);
                }

            }

            Points = temp.ToArray();

            GeoType = ShapeType.PolygonZ;
            
            ZValues = Zvals.ToArray();

            if (Parts.Length > 0)
            {
                double[] border = GISTerrainLoaderVectorExtensions.GetBorder(Poly.GeoPoints,ZValues);

                XYRange.MinX = border[0];
                XYRange.MinY = border[1];
                XYRange.MaxY = border[2];
                XYRange.MaxX = border[3];

                ZRange.Min = border[4];
                ZRange.Max = border[5];

                DataLength = 2 +(Convert.ToInt32(GetLength()) / 2) ;

            }
            else
            {
                Parts[0] = 0;
                NumParts = 0;
                NumPoints = 0;
                XYRange.MinX = 0.0;
                XYRange.MinY = 0.0;
                XYRange.MaxY = 0.0;
                XYRange.MaxX = 0.0;

                ZRange.Min = 0.0;
                ZRange.Max = 0.0;

                DataLength = 2 + (Convert.ToInt32(GetLength()) / 2);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPatch : GISTerrainLoaderIElement
    {
        public string RecordNum { get; set; }
        public int DataLength { get; set; }
        public ShapeType GeoType { get; set; }
        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; }
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public int[] PartsTypes { get; set; }
        public Point[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            PartsTypes = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                PartsTypes[i] = br.ReadInt32();
            }

            Points = new Point[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new Point();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(PartsTypes);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }

    }

    public enum ShapeType : int
    {
        Null = 0,
        Point = 1,
        PolyLine = 3,
        Polygon = 5,
        MultiPoint = 8,
        PointZ = 11,
        PolyLineZ = 13,
        PolygonZ = 15,
        MultiPointZ = 18,
        PointM = 21,
        PolyLineM = 23,
        PolygonM = 25,
        MultiPointM = 28,
        MultiPatch = 31
    }
    #endregion

}
