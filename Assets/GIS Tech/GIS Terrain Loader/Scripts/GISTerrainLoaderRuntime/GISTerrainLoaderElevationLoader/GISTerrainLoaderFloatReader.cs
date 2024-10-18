/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderFloatReader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GISTerrainLoaderProjectionSystem CoordinateReferenceSystem;

        private static GISTerrainLoaderPrefs Prefs;

        private List<float> FixedList;
        public GISTerrainLoaderFloatReader(GISTerrainLoaderPrefs prefs)
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();

            Prefs = prefs;

            if (Prefs.TerrainFixOption == FixOption.ManualFix)
                data.MinMaxElevation = Prefs.TerrainMaxMinElevation;

        }
        public void LoadFile(string FilePath)
        {
            LoadComplet = false;
 
            if (!File.Exists(FilePath))
            {
                if (OnReadError != null)
                    OnReadError("Please select a FloatGrid (.flt) file.");

                return;
            }

            if (Prefs.readingMode == ReadingMode.Full)
                ParseFile(Prefs.TerrainFilePath);
            else
                ParseSubFile(Prefs.TerrainFilePath,Prefs.SubRegionUpperLeftCoordiante, Prefs.SubRegionDownRightCoordiante);
         }
        private void ParseFile(string FilePath)
        {
            var hdrpath = Path.ChangeExtension(FilePath, ".hdr");

            if (File.Exists(hdrpath))
            {
                CoordinateReferenceSystem = new GISTerrainLoaderProjectionSystem("GCS_WGS_1984");

                StreamReader hdrReader = new StreamReader(hdrpath);

                string hdrTemp = null;

                hdrTemp = hdrReader.ReadLine();

                while (hdrTemp != null)
                {
                    int spaceStart = hdrTemp.IndexOf(" ");
                    int spaceEnd = hdrTemp.LastIndexOf(" ");

                    hdrTemp = hdrTemp.Remove(spaceStart, spaceEnd - spaceStart);

                    string[] lineTemp = hdrTemp.Split(" "[0]);

                    switch (lineTemp[0])
                    {
                        case "nrows":
                            data.mapSize_row_y = Int32.Parse(lineTemp[1]);
                            break;
                        case "ncols":
                            data.mapSize_col_x = Int32.Parse(lineTemp[1]);
                            break;
                        case "xllcorner":
                            data.DLPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "yllcorner":
                            data.DLPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "cellsize":
                            data.cellsize = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "xdim":
                            data.dim.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "ydim":
                            data.dim.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                    }
                    hdrTemp = hdrReader.ReadLine();
                }

                if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {
                    ReadProjection(FilePath);

                    if (data.cellsize == 0)
                    {
                        data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + (data.dim.x * data.mapSize_col_x);
                        data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + (data.dim.y * data.mapSize_row_y);
                    }
                    else
                    {
                        data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + (data.cellsize * data.mapSize_col_x);
                        data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + (data.cellsize * data.mapSize_row_y);
                    }

                    data.TRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TRPoint_LatLon);
                    data.DLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DLPoint_LatLon);

                    data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.TRPoint_LatLon.y);
                    data.DRPoint_LatLon = new DVector2(data.TRPoint_LatLon.x, data.DLPoint_LatLon.y);


                    data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                    data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');
                }
                else
                {
                    if (OnReadError != null)
                        OnReadError("The header (HDR) file is missing.");
                    return;
                }
            }


            if (File.Exists(FilePath))
            {
                var bytes = File.ReadAllBytes(FilePath);

                data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                for (int i = 0; i < data.mapSize_row_y; i++)
                {
                    for (int j = 0; j < data.mapSize_col_x; j++)
                    {
                        var el = BitConverter.ToSingle(bytes, i * data.mapSize_col_x * 4 + j * 4);

                        if (Prefs.TerrainFixOption == FixOption.ManualFix)
                        {
                            if (el < data.MinMaxElevation.x)
                                el = (short)data.MinMaxElevation.x;

                            if (el > data.MinMaxElevation.y)
                                el = (short)data.MinMaxElevation.y;

                        }
                        else
                        {
                            if (el < data.MinMaxElevation.x)
                                data.MinMaxElevation.x = el;
                            if (el > data.MinMaxElevation.y)
                                data.MinMaxElevation.y = el;
                        }

                        data.floatheightData[j, data.mapSize_row_y - i - 1] = el;

                        FixedList.Add(el);

                        if (OnProgress != null)
                            OnProgress("Loading File ", i * j * 100 / (data.mapSize_row_y * data.mapSize_col_x));



                    }
                }

                if (Prefs.TerrainFixOption == FixOption.AutoFix)
                    FixTerrainData();

                LoadComplet = true;
            }
            else
            {
                Debug.Log("File not found!");
                return;
            }
        }
        public void ParseSubFile(string FilePath,DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante)
        {
            LoadComplet = false;

            var hdrpath = Path.ChangeExtension(FilePath, ".hdr");

            if (!File.Exists(FilePath))
            {
 
                if (OnReadError != null)
                    OnReadError("Please select a FloatGrid (.flt) file.");

                return;
            }

            if (File.Exists(hdrpath))
            {
 
                StreamReader hdrReader = new StreamReader(hdrpath);

                string hdrTemp = null;

                hdrTemp = hdrReader.ReadLine();

                while (hdrTemp != null)
                {
                    int spaceStart = hdrTemp.IndexOf(" ");
                    int spaceEnd = hdrTemp.LastIndexOf(" ");

                    hdrTemp = hdrTemp.Remove(spaceStart, spaceEnd - spaceStart);

                    string[] lineTemp = hdrTemp.Split(" "[0]);

                    switch (lineTemp[0])
                    {
                        case "nrows":
                            data.mapSize_row_y = Int32.Parse(lineTemp[1]);
                            break;
                        case "ncols":
                            data.mapSize_col_x = Int32.Parse(lineTemp[1]);
                            break;
                        case "xllcorner":
                            data.DLPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "yllcorner":
                            data.DLPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "cellsize":
                            data.cellsize = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "xdim":
                            data.dim.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "ydim":
                            data.dim.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                    }

                    hdrTemp = hdrReader.ReadLine();
                }
                if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {
                    ReadProjection(FilePath);

                    if (data.cellsize == 0)
                    {
                        data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + (data.dim.x * data.mapSize_col_x);
                        data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + (data.dim.y * data.mapSize_row_y);
                    }
                    else
                    {
                        data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + (data.cellsize * data.mapSize_col_x);
                        data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + (data.cellsize * data.mapSize_row_y);
                    }

                    data.TRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TRPoint_LatLon);
                    data.DLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DLPoint_LatLon);

                    data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.TRPoint_LatLon.y);
                    data.DRPoint_LatLon = new DVector2(data.TRPoint_LatLon.x, data.DLPoint_LatLon.y);

                }

            }
            else
            {
                if (OnReadError != null)
                    OnReadError("The header (HDR) file is missing.");
                return;
            }

            if (File.Exists(FilePath))
            {
                var bytes = File.ReadAllBytes(FilePath);
                data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                for (int i = 0; i < data.mapSize_row_y; i++)
                {
                    for (int j = 0; j < data.mapSize_col_x; j++)
                    {
                        var el = BitConverter.ToSingle(bytes, i * data.mapSize_col_x * 4 + j * 4);

                        if (Prefs.TerrainFixOption == FixOption.ManualFix)
                        {
                            if (el < data.MinMaxElevation.x)
                                el = (short)data.MinMaxElevation.x;

                            if (el > data.MinMaxElevation.y)
                                el = (short)data.MinMaxElevation.y;

                        }
                        else
                        {
                            if (el < data.MinMaxElevation.x)
                                data.MinMaxElevation.x = el;
                            if (el > data.MinMaxElevation.y)
                                data.MinMaxElevation.y = el;
                        }

                        data.floatheightData[j, i] = el;
                        FixedList.Add(el);

                        if (OnProgress != null)
                            OnProgress("Loading File ", i * j * 100 / (data.mapSize_row_y * data.mapSize_col_x));


                    }
                }

                if (Prefs.TerrainFixOption == FixOption.AutoFix)
                    FixTerrainData();

                if (GISTerrainLoaderExtensions.IsSubRegionIncluded(data.TLPoint_LatLon, data.DRPoint_LatLon, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante))
                {
                    var points = SubZone(data, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante);
                    data.floatheightData = points;


                    data.TLPoint_LatLon = SubRegionUpperLeftCoordiante;
                    data.DRPoint_LatLon = SubRegionDownRightCoordiante;
                    data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);
                    data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);

                    data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                    data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');
 
                }
                else
                {
                    if (OnReadError != null)
                    {
                        OnReadError("");
                    }
                }

                LoadComplet = true;
            }
            else
            {
                Debug.Log("File not found!");
                return;
            }

        }
        private float[,] SubZone(GISTerrainLoaderFileData data, DVector2 SubTopLeft, DVector2 SubDownRight)
        {
            var rang_x = Math.Abs(Math.Abs(data.DRPoint_LatLon.x) - Math.Abs(data.TLPoint_LatLon.x));
            var rang_y = Math.Abs(Math.Abs(data.TLPoint_LatLon.y) - Math.Abs(data.DRPoint_LatLon.y));

            var Subrang_x = Math.Abs(Math.Abs(SubDownRight.x) - Math.Abs(SubTopLeft.x));
            var Subrang_y = Math.Abs(Math.Abs(SubTopLeft.y) - Math.Abs(SubDownRight.y));

            int submapSize_col_x = (int)(Subrang_x * data.mapSize_col_x / rang_x);
            int submapSize_row_y = (int)(Subrang_y * data.mapSize_row_y / rang_y);

            var StartLocation = GISTerrainLoaderExtensions.GetLocalLocation(data, SubTopLeft);
            var EndLocation = GISTerrainLoaderExtensions.GetLocalLocation(data, SubDownRight);

            float[,] SubZone = new float[submapSize_col_x, submapSize_row_y];

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x-1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y-1; y++)
                {
                    int Step_X = x-1 - ((int)StartLocation.x - 1);
                    int Step_Y = y-1 - ((int)StartLocation.y - 1);

                    var el = data.floatheightData[x, y];

                    if (el > -9900)
                    {
                        if (el < data.MinMaxElevation.x)
                            data.MinMaxElevation.x = el;
                        if (el > data.MinMaxElevation.y)
                            data.MinMaxElevation.y = el;
                    }

                    if (OnProgress != null)
                    {
                        OnProgress("Loading File ", Step_X * Step_Y * 100 / (submapSize_col_x * submapSize_row_y));
                    }

                    SubZone[ Step_X,submapSize_row_y - Step_Y - 1 ] = el;

                }

            }
            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

        }
        public void ReadProjection(string path)
        {
            string prjFile = path.Replace(Path.GetExtension(path), ".prj");

            if (File.Exists(prjFile))
                CoordinateReferenceSystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
        }
        private void FixTerrainData()
        {
            var orderdDown = FixedList.OrderBy(x => x).ToList();
            for (int i = 0; i < orderdDown.Count; i++)
            {
                var el = orderdDown[i];
                if (el > -9999)
                {
                    data.MinMaxElevation.x = el;
                    break;
                }
            }

            for (int i = 0; i < data.floatheightData.GetLength(0); i++)
            {
                for (int j = 0; j < data.floatheightData.GetLength(1); j++)
                {
                    var el = data.floatheightData[i, j];

                    if (el == -9999)
                    {
                        data.floatheightData[i, j] = (data.MinMaxElevation.x + ((data.MinMaxElevation.y - data.MinMaxElevation.x) / 2));

                    }

                }
            }
        }
    }

}