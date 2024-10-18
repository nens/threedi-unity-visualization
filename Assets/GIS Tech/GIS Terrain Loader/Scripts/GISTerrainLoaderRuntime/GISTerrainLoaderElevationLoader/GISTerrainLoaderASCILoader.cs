/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderASCILoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private float cellsize;
        private float nodata_value;
        private string line;
        private int counter;
        int c = 0;

        private GISTerrainLoaderProjectionSystem CoordinateReferenceSystem;
        private List<float> FixedList;
        private static int EPSG = 0;
        private GISTerrainLoaderPrefs Prefs;
        public GISTerrainLoaderASCILoader(GISTerrainLoaderPrefs prefs)
        {
            EPSG = 0;
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
            Prefs = prefs;
        }

        public void LoadFile(string filepath)
        {
            if (Prefs.readingMode == ReadingMode.Full)
                ParseFile(filepath);
            else
                ParseSubFile(filepath, Prefs.SubRegionUpperLeftCoordiante, Prefs.SubRegionDownRightCoordiante);
        }
        private void ParseFile(string filepath)
        {
            try
            {
                CoordinateReferenceSystem = new GISTerrainLoaderProjectionSystem();
                
                if (Prefs.EPSGCode != 0 && Prefs.projectionMode == ProjectionMode.Custom_EPSG)
                {
                    EPSG = Prefs.EPSGCode;

                }

                LoadComplet = false;

                ReadASCIHead(filepath);

                data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                ReadASCIData(filepath, Prefs.TerrainFixOption);

                if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {
                    double startLat = data.DLPoint_LatLon.y + (cellsize / 2.0);
                    double startLon = data.DLPoint_LatLon.x + (cellsize / 2.0);

                    double currentLat = startLat;
                    double currentLon = startLon;

                    if (EPSG == 0)
                    {
                        EPSG = 4326;

                        data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.DLPoint_LatLon.y + (cellsize * data.mapSize_row_y));
                        data.DRPoint_LatLon = new DVector2(data.DLPoint_LatLon.x + (cellsize * data.mapSize_col_x), data.DLPoint_LatLon.y);
                        data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y + (cellsize * data.mapSize_row_y));
                        data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);

                    }
                    else
                    {
                        data.EPSG = EPSG;

                        data.TLOriginal_Coor = new DVector2(data.DLPoint_LatLon.x, data.DLPoint_LatLon.y + (cellsize * data.mapSize_row_y));
                        data.DROriginal_Coor = new DVector2(data.DLPoint_LatLon.x + (cellsize * data.mapSize_col_x), data.DLPoint_LatLon.y);

                        data.TLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.TLOriginal_Coor, EPSG);
                        data.DRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.DROriginal_Coor, EPSG);

                        data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);
                        data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);
                    }


                    data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                    data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');

                }

                LoadComplet = true;

            }
            catch (Exception e)
            {
                if (OnReadError != null)
                    OnReadError("Error occured while reading ASC file! " + e.ToString());

                return;
            }
        }
        public void ParseSubFile(string filepath, DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    ReadASCIHead(filepath);

                    if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                    {

                        data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + (cellsize * data.mapSize_col_x);
                        data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + (cellsize * data.mapSize_row_y);

                        data.DLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DLPoint_LatLon);
                        data.TRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TRPoint_LatLon);

                        data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.TRPoint_LatLon.y);
                        data.DRPoint_LatLon = new DVector2(data.TRPoint_LatLon.x, data.DLPoint_LatLon.y);
                    }

                    data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                    float el = 0;
                    StreamReader file = new StreamReader(filepath);

                    while ((line = file.ReadLine()) != null)
                    {
                        if (c < 6)
                        {
                            c++;
                        }
                        else
                        if (c >= 6)
                        {
                            var replacedLine = line.Replace('.', ',');

                            var floatLineList = replacedLine.Split(' ');

                            if (floatLineList.Length >= data.mapSize_row_y - 1)
                            {
                                for (int i = 0; i < floatLineList.Length; i++)
                                {
                                    if (!string.IsNullOrEmpty(floatLineList[i]))
                                        el = float.Parse(floatLineList[i]);

                                    if (el == -99999 || el == -9999)
                                        el = 0;

                                    if (Prefs.TerrainFixOption == FixOption.ManualFix)
                                    {
                                        if (el < data.MinMaxElevation.x)
                                            el = data.MinMaxElevation.x;

                                        if (el > data.MinMaxElevation.y)
                                            el = data.MinMaxElevation.y;

                                    }
                                    else
                                    {
                                        if (el < data.MinMaxElevation.x)
                                            data.MinMaxElevation.x = el;
                                        if (el > data.MinMaxElevation.y)
                                            data.MinMaxElevation.y = el;
                                    }

                                    if (i < data.mapSize_col_x)
                                    {
                                        data.floatheightData[i, data.mapSize_row_y - (c - 6) - 1] = el;
                                        FixedList.Add(el);
                                    }
                                        
                                }
                            }
                            c++;
                        }

                    }

                    file.Close();


                    if (Prefs.TerrainFixOption == FixOption.AutoFix)
                        FixTerrainData();

                        if (GISTerrainLoaderExtensions.IsSubRegionIncluded(data.TLPoint_LatLon, data.DRPoint_LatLon, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante))
                    {
                        var points = SubZone(data, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante);

                        data.floatheightData = points;

                        if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                        {
                            data.TLPoint_LatLon = SubRegionUpperLeftCoordiante;
                            data.DRPoint_LatLon = SubRegionDownRightCoordiante;
                            data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);
                            data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);


                            data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                            data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');

 
                        }

                    }
                    else
                    {
                        if (OnReadError != null)
                            OnReadError("");
                    }

                    LoadComplet = true;

                }
                catch (Exception e)
                {

                    if (OnReadError != null)
                        OnReadError("Error occured while reading ASC file! " + e.ToString());
                    return;
                }
            }
        }
        private void ReadASCIHead(string filepath)
        {
            StreamReader file = new StreamReader(filepath);

            while ((line = file.ReadLine()) != null)
            {
                if (counter < 6)
                {
                    string[] lineTemp = line.Split(' ');

                    switch (lineTemp[0])
                    {
                        case "ncols":
                            data.mapSize_col_x = int.Parse(lineTemp[lineTemp.Length - 1]);
                            break;
                        case "nrows":
                            data.mapSize_row_y = int.Parse(lineTemp[lineTemp.Length - 1]);
                            break;
                        case "xllcorner":
                            data.DLPoint_LatLon.x = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                        case "yllcorner":
                            data.DLPoint_LatLon.y = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                        case "cellsize":
                            cellsize = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                    }


                    counter++;
                }

            }

            file.Close();

            ReadProjection(filepath);

        }
        private void ReadASCIData(string filepath, FixOption fixOption)
        {
            float el = 200;

            StreamReader file = new StreamReader(filepath);

            while ((line = file.ReadLine()) != null)
            {
                if (c < 6)
                {
                    c++;
                }
                else
                if (c >= 6)
                {
                    var replacedLine = line.Replace('.', ',');

                    var floatLineList = replacedLine.Split(' ');

                    if (floatLineList.Length >= data.mapSize_row_y - 1)
                    {
                        for (int i = 0; i < floatLineList.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(floatLineList[i]))
                                el = float.Parse(floatLineList[i]);

                            if (el == -99999 || el == -9999)
                                el = 0;

                            if (fixOption == FixOption.ManualFix)
                            {
                                if (el < data.MinMaxElevation.x)
                                    el = (ushort)data.MinMaxElevation.x;

                                if (el > data.MinMaxElevation.y)
                                    el = (ushort)data.MinMaxElevation.y;

                            }
                            else
                            {
                                if (el < data.MinMaxElevation.x)
                                    data.MinMaxElevation.x = el;
                                if (el > data.MinMaxElevation.y)
                                    data.MinMaxElevation.y = el;
                            }

                            if (i < data.mapSize_col_x)
                            {
                                data.floatheightData[i, data.mapSize_row_y - (c - 6) - 1] = el;
                                FixedList.Add(el);
                            }
                               

                           

                            if (OnProgress != null)
                                OnProgress("Loading File ", i * c * 100 / (data.mapSize_row_y * data.mapSize_col_x));


                        }
                    }
                    c++;
                }

            }

            file.Close();


            if (fixOption == FixOption.AutoFix)
                FixTerrainData();
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

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x - 1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y - 1; y++)
                {
                    int Step_X = x - 1 - ((int)StartLocation.x - 1);
                    int Step_Y = y - 1 - ((int)StartLocation.y - 1);

                    var el = data.floatheightData[x, data.mapSize_row_y - (y) - 1];

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

                    SubZone[Step_X, submapSize_row_y - Step_Y - 1] = el;

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