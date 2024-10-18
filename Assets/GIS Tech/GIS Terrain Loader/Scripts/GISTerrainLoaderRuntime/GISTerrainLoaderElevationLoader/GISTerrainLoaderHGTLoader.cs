/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{

    public class GISTerrainLoaderHGTLoader 
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;
        private List<float> FixedList;
        private GISTerrainLoaderPrefs Prefs;

        public GISTerrainLoaderHGTLoader(GISTerrainLoaderPrefs prefs)
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
            Prefs = prefs;
        }
        public void LoadFile(string filepath)
        {
            LoadComplet = false;

            if (Prefs.readingMode == ReadingMode.Full)
                ParseFile(filepath);
            else
                ParseSubFile(filepath, Prefs.SubRegionUpperLeftCoordiante, Prefs.SubRegionDownRightCoordiante);
 
        }
        public void ParseFile(string filepath)
        {
            LoadComplet = false;
            string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
            string[] fileCoordinate = filename.Split(new[] { 'e', 'w' });
            if (fileCoordinate.Length != 2)
                throw new ArgumentException("Invalid filename.", filepath);

            fileCoordinate[0] = fileCoordinate[0].TrimStart(new[] { 'n', 's' });
            var Latitude = int.Parse(fileCoordinate[0]);
            data.DLPoint_LatLon.y = Latitude;
            if (filename.Contains("s"))
                data.DLPoint_LatLon.y *= -1;

            var Longitude = int.Parse(fileCoordinate[1]);
            data.DLPoint_LatLon.x = Longitude;
            if (filename.Contains("w"))
                data.DLPoint_LatLon.x *= -1;

            var HgtData = File.ReadAllBytes(filepath);

            switch (HgtData.Length)
            {
                case 1201 * 1201 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 1201;
                    break;
                case 3601 * 3601 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 3601;
                    break;
                default:
                    throw new ArgumentException("Invalid file size.", filepath);
            }

            data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + 1;
            data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + 1;

            data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.TRPoint_LatLon.y);
            data.DRPoint_LatLon = new DVector2(data.TRPoint_LatLon.x, data.DLPoint_LatLon.y);

            data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
            data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');

 
            data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];
            short[,] heightMap = new short[data.mapSize_col_x + 1, data.mapSize_row_y + 1];

            FileStream fs = File.OpenRead(filepath);

            const int size = 1000000;

            int c = 0;

            do
            {
                byte[] buffer = new byte[size];
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    var buf = buffer[i] * 256 + buffer[i + 1];

                    short value = (short)(buf);

                    heightMap[c % data.mapSize_col_x, c / data.mapSize_row_y] = value;

                    float el = value;

                    var x = c % data.mapSize_col_x;
                    var y = c / data.mapSize_row_y;

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

                    data.floatheightData[x, data.mapSize_row_y - y - 1] = el;
                    FixedList.Add(el);
                    c++;
                }

            }
            while (fs.Position != fs.Length);

            fs.Close();

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData();


            GC.Collect();

            LoadComplet = true;
 

        }
        public void ParseSubFile(string filepath, DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante)
        {
            LoadComplet = false;
            string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
            string[] fileCoordinate = filename.Split(new[] { 'e', 'w' });
            if (fileCoordinate.Length != 2)
                throw new ArgumentException("Invalid filename.", filepath);

            fileCoordinate[0] = fileCoordinate[0].TrimStart(new[] { 'n', 's' });
            var Latitude = int.Parse(fileCoordinate[0]);
            data.DLPoint_LatLon.y = Latitude;
            if (filename.Contains("s"))
                data.DLPoint_LatLon.y *= -1;

            var Longitude = int.Parse(fileCoordinate[1]);
            data.DLPoint_LatLon.x = Longitude;
            if (filename.Contains("w"))
                data.DLPoint_LatLon.x *= -1;

            var HgtData = File.ReadAllBytes(filepath);

            switch (HgtData.Length)
            {
                case 1201 * 1201 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 1201;
                    break;
                case 3601 * 3601 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 3601;
                    break;
                default:
                    throw new ArgumentException("Invalid file size.", filepath);
            }

            data.TRPoint_LatLon.x = data.DLPoint_LatLon.x + 1;
            data.TRPoint_LatLon.y = data.DLPoint_LatLon.y + 1;

            data.TLPoint_LatLon = new DVector2(data.DLPoint_LatLon.x, data.TRPoint_LatLon.y);
            data.DRPoint_LatLon = new DVector2(data.TRPoint_LatLon.x, data.DLPoint_LatLon.y);
 
            data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];
            short[,] heightMap = new short[data.mapSize_col_x + 1, data.mapSize_row_y + 1];

            FileStream fs = File.OpenRead(filepath);

            const int size = 1000000;

            int c = 0;

            do
            {
                byte[] buffer = new byte[size];
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    var buf = buffer[i] * 256 + buffer[i + 1];

                    short value = (short)(buf);

                    heightMap[c % data.mapSize_col_x, c / data.mapSize_row_y] = value;

                    float el = value;

                    var x = c % data.mapSize_col_x;
                    var y = c / data.mapSize_row_y;

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

                    data.floatheightData[x, data.mapSize_row_y - y - 1] = el;
                    FixedList.Add(el);

                    c++;
                }

            }
            while (fs.Position != fs.Length);

            fs.Close();
            GC.Collect();

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

            if (!File.Exists(filepath))
            {
                if (OnReadError != null)
                {
                    OnReadError("File Not Found .. ");
                }

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

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x - 1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y -1; y++)
                {
                    int Step_X = x - 1 - ((int)StartLocation.x - 1);
                    int Step_Y = y - 1 - ((int)StartLocation.y - 1);
 
                  var el = data.floatheightData[x, data.mapSize_row_y - y - 1];

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
            Debug.Log(data.MinMaxElevation.y + "  " + data.MinMaxElevation.x);
            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

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