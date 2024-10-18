/*     Unity GIS Tech 2020-2023      */

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    [Serializable]
    public class GISTerrainLoaderFileData
    {
        public bool AlreadyLoaded =false;

        [SerializeField]
        public float[,] floatheightData;

        public int mapSize_row_y;
        public int mapSize_col_x;

        public Vector2 Tiles = new Vector2(0, 0);

        public DVector2 DLPoint_LatLon = new DVector2(0, 0);
        public DVector2 TRPoint_LatLon = new DVector2(0, 0);

        public DVector2 TLPoint_LatLon = new DVector2(0, 0);
        public DVector2 DRPoint_LatLon = new DVector2(0, 0);

        public DVector2 TLOriginal_Coor = new DVector2(0, 0);
        public DVector2 DROriginal_Coor = new DVector2(0, 0);

        public DVector2 TLPoint_Mercator = new DVector2(0, 0); 
        public DVector2 DRPoint_Mercator = new DVector2(0, 0);

        public DVector2 dim = new DVector2(0, 0);

        public double cellsize = 0;

        public DVector2 Dimensions = new DVector2(0, 0);

        public Vector2 MinMaxElevation = new Vector2(float.MaxValue, float.MinValue);

        public int EPSG = 0;
        public LinearUnit Unite;
        public string SerializedFileName = "";

        public GISTerrainLoaderFileData()
        {
            MinMaxElevation = new Vector2(float.MaxValue, float.MinValue);

            mapSize_row_y = 0;
            mapSize_col_x = 0;

            Tiles = new Vector2(0, 0);
 
            TRPoint_LatLon = new DVector2(0, 0);
            DLPoint_LatLon = new DVector2(0, 0);
        
            TLPoint_LatLon = new DVector2(0, 0);
            DRPoint_LatLon = new DVector2(0, 0);


            dim = new DVector2(0, 0);

            Dimensions = new DVector2(0, 0);

            floatheightData = new float[mapSize_col_x, mapSize_row_y];

            TLOriginal_Coor = new DVector2(0, 0);
            DROriginal_Coor = new DVector2(0, 0);

            TLPoint_Mercator = new DVector2(0, 0);
            DRPoint_Mercator = new DVector2(0, 0);

            EPSG = 0;
        }
        public float GetElevation(float fpx, float fpy)
        {
            int px = (int)fpx;
            int py = (int)fpy;

            float Rx = fpx - px;
            float Ry = fpy - py;

            return GetAverageElevation(Rx, Ry, px, py);
        }
        public float GetAverageElevation(float Rx, float Ry, int px, int py)
        {

            float C_25 = 0.25f;
            float C_12 = 12.0f;
            float C_36 = 36.0f;

            var Rsx_1 = Rx - 1;
            var Rsx_2 = Rx - 2;
            var RsxP_1 = Rx + 1;

            var Rsy_1 = Ry - 1;
            var Rsy_2 = Ry - 2;
            var RsyP_1 = Ry + 1;

            var PsxP_1 = px + 1;
            var PsyP_1 = py + 1;

            var PxyM = Rx * Ry;

            var Psx_1 = px - 1;
            var Psy_1 = py - 1;

            var PsxP_2 = px + 2;
            var PsyP_2 = py + 2;

            float el = Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * Rsy_2 * RsyP_1 * C_25 * ReadValue(px, py);

            el -= Rx * RsxP_1 * Rsx_2 * Rsy_1 * Rsy_2 * RsyP_1 * C_25 * ReadValue(PsxP_1, py);
            el -= Ry * Rsx_1 * Rsx_2 * RsxP_1 * RsyP_1 * Rsy_2 * C_25 * ReadValue(px, PsyP_1);
            el += PxyM * RsxP_1 * Rsx_2 * RsyP_1 * Rsy_2 * C_25 * ReadValue(PsxP_1, PsyP_1);
            el -= Rx * Rsx_1 * Rsx_2 * Rsy_1 * Rsy_2 * RsyP_1 / C_12 * ReadValue(Psx_1, py);
            el -= Ry * Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * Rsy_2 / C_12 * ReadValue(px, Psy_1);
            el += PxyM * Rsx_1 * Rsx_2 * RsyP_1 * Rsy_2 / C_12 * ReadValue(Psx_1, PsyP_1);
            el += PxyM * RsxP_1 * Rsx_2 * Rsy_1 * Rsy_2 / C_12 * ReadValue(PsxP_1, Psy_1);
            el += Rx * Rsx_1 * RsxP_1 * Rsy_1 * Rsy_2 * RsyP_1 / C_12 * ReadValue(PsxP_2, py);
            el += Ry * Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * RsyP_1 / C_12 * ReadValue(px, PsyP_2);
            el += PxyM * Rsx_1 * Rsx_2 * Rsy_1 * Rsy_2 / C_36 * ReadValue(Psx_1, Psy_1);
            el -= PxyM * Rsx_1 * RsxP_1 * RsyP_1 * Rsy_2 / C_12 * ReadValue(PsxP_2, PsyP_1);
            el -= PxyM * RsxP_1 * Rsx_2 * Rsy_1 * RsyP_1 / C_12 * ReadValue(PsxP_1, PsyP_2);
            el -= PxyM * Rsx_1 * RsxP_1 * Rsy_1 * Rsy_2 / C_36 * ReadValue(PsxP_2, Psy_1);
            el -= PxyM * Rsx_1 * Rsx_2 * Rsy_1 * RsyP_1 / C_36 * ReadValue(Psx_1, PsyP_2);
            el += PxyM * Rsx_1 * RsxP_1 * Rsy_1 * RsyP_1 / C_36 * ReadValue(PsxP_2, PsyP_2);

            return el;
        }
        public float ReadValue(int PX, int PY)
        {
            try
            {
                PX = Mathf.Clamp(PX, 0, mapSize_col_x - 1);
                PY = Mathf.Clamp(PY, 0, mapSize_row_y - 1);
                var el = floatheightData[PX, PY];
                return el;
            }
            catch (Exception e)
            {
                var es = e;
                return 0;
            }
        }
        public float[,] GetNormlizedHeightmap(int heightmapResolution)
        {
            var tdataHeightmap = new float[heightmapResolution, heightmapResolution];

            float elevationRange = MinMaxElevation.y - MinMaxElevation.x;

            float thx = heightmapResolution - 1;
            float thy = heightmapResolution - 1;

            int tw = heightmapResolution;
            int th = heightmapResolution;


            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {

                    float fpc = Mathf.Lerp(0, mapSize_col_x, x / thx);
                    float fpr = Mathf.Lerp(0, mapSize_row_y, y / thy);

                    int pr = Mathf.FloorToInt(fpr);
                    int pc = Mathf.FloorToInt(fpc);



                    if (pr > floatheightData.GetLength(0) - 1)
                        pr = floatheightData.GetLength(0) - 1;

                    if (pc > floatheightData.GetLength(1) - 1)
                        pc = floatheightData.GetLength(1) - 1;


                    var Rel = GetElevation(fpr, fpc);

                    tdataHeightmap[x, y] = (Rel - MinMaxElevation.x) / elevationRange;

                }

            }
            return tdataHeightmap;
        }
        public float[,] GetNormlizedHeightmap(int heightmapResolution, GISTerrainTile[,] terrains)
        {
            int x_count = terrains.GetLength(0);
            int y_count = terrains.GetLength(1);

            float[,] heightmap = new float[heightmapResolution* x_count, heightmapResolution* y_count];

            for(int x = 0; x < terrains.GetLength(0); x++)
            {
                for(int y = 0; y < terrains.GetLength(1); y++)
                {
                    var item = terrains[x, y];
                    var data = item.terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
                    for (int i = 0; i < data.GetLength(0); i++)
                    {
                        for (int j = 0; j < data.GetLength(1); j++)
                        {
                            heightmap[y * heightmapResolution + i, x * heightmapResolution + j] = data[i, j];
 
                        }
                    }
                }
            }

            return heightmap;
        }

        public void Store(string m_SerializePath)
        {
            SerializedFileName = m_SerializePath;

            GISTerrainLoaderHeightmapSerializer.Serialize(this, SerializedFileName);
        }

        public float GetElevation(DVector2 coor)
        {

            float value = 0;
 
            if (EPSG != 0)
            {
                var rang_x = Math.Abs(Math.Abs(DROriginal_Coor.x) - Math.Abs(TLOriginal_Coor.x));
                var rang_y = Math.Abs(Math.Abs(TLOriginal_Coor.y) - Math.Abs(DROriginal_Coor.y));

                var rang_px = Math.Abs(Math.Abs(coor.x) - Math.Abs(TLOriginal_Coor.x));
                var rang_py = Math.Abs(Math.Abs(TLOriginal_Coor.y) - Math.Abs(coor.y));

                int localLat = (int)(rang_px * mapSize_col_x / rang_x);
                int localLon = (int)(rang_py * mapSize_row_y / rang_y);

                try
                {
                    value = floatheightData[localLat, mapSize_row_y - localLon - 1];
                }
                catch
                {
                    value = 0;
                }
            }
            else
            {
                var rang_x = Math.Abs(Math.Abs(DRPoint_LatLon.x) - Math.Abs(TLPoint_LatLon.x));
                var rang_y = Math.Abs(Math.Abs(TLPoint_LatLon.y) - Math.Abs(DRPoint_LatLon.y));

                var rang_px = Math.Abs(Math.Abs(coor.x) - Math.Abs(TLPoint_LatLon.x));
                var rang_py = Math.Abs(Math.Abs(TLPoint_LatLon.y) - Math.Abs(coor.y));

                int localLat = (int)(rang_px * mapSize_col_x / rang_x);
                int localLon = (int)(rang_py * mapSize_row_y / rang_y);

                try
                {
                    value = floatheightData[localLat, mapSize_row_y - localLon - 1];

                }
                catch
                {
                    value = 0;
                }

            }

            return value;
        }

        public void SaveFileData(string FileDataName)
        {
#if UNITY_EDITOR
 

            var FileData = ""; 
            FileData += "MinMaxElevation_X :" + MinMaxElevation.x.ToString()+ "\n";
            FileData += "MinMaxElevation_Y :" + MinMaxElevation.y.ToString() + "\n";

            FileData += "mapSize_col_x :" + mapSize_col_x.ToString() + "\n";
            FileData += "mapSize_row_y :" + mapSize_row_y.ToString() + "\n";

            FileData += "TLPoint_LatLon_x :" + TLPoint_LatLon.x.ToString() + "\n";
            FileData += "TLPoint_LatLon_y :" + TLPoint_LatLon.y.ToString() + "\n";

            FileData += "DLPoint_LatLon_x :" + DLPoint_LatLon.x.ToString() + "\n";
            FileData += "DLPoint_LatLon_y :" + DLPoint_LatLon.y.ToString() + "\n";

            FileData += "TRPoint_LatLon_x :" + TRPoint_LatLon.x.ToString() + "\n";
            FileData += "TRPoint_LatLon_y :" + TRPoint_LatLon.y.ToString() + "\n";

            FileData += "DRPoint_LatLon_x :" + DRPoint_LatLon.x.ToString() + "\n";
            FileData += "DRPoint_LatLon_y :" + DRPoint_LatLon.y.ToString() + "\n";

            FileData += "Dimensions_x :" + Dimensions.x.ToString() + "\n";
            FileData += "Dimensions_y :" + Dimensions.y.ToString() + "\n";

            FileData += "TLOriginal_Coor_x :" + TLOriginal_Coor.x.ToString() + "\n";
            FileData += "TLOriginal_Coor_y :" + TLOriginal_Coor.y.ToString() + "\n";

            FileData += "DROriginal_Coor_x :" + DROriginal_Coor.x.ToString() + "\n";
            FileData += "DROriginal_Coor_y :" + DROriginal_Coor.y.ToString() + "\n";

            FileData += "TLPoint_Mercator_x :" + TLPoint_Mercator.x.ToString() + "\n";
            FileData += "TLPoint_Mercator_y :" + TLPoint_Mercator.y.ToString() + "\n";

            FileData += "DRPoint_Mercator_x :" + DRPoint_Mercator.x.ToString() + "\n";
            FileData += "DRPoint_Mercator_y :" + DRPoint_Mercator.y.ToString() + "\n";
            
            FileData += "EPSG :" + EPSG.ToString() + "\n";

            FileData += "SerializedFileName :" + SerializedFileName + "\n";
 

            var TabSavePath =FileDataName + "_Data.bytes";

            using (StreamWriter file = new StreamWriter(TabSavePath))
            {
                file.Write(FileData);
                file.Close();
                GC.Collect();
            }
#endif
        }

        public void LoadFileData(string FileData)
        {
            string[] fLines = Regex.Split(FileData, "\n|\r|\r\n");

            for (int i = 0; i < fLines.Length; i++)
            {
                string valueLine = fLines[i];
                valueLine.Replace(" ", "");

                string[] lineTemp = valueLine.Split('=');


                switch (lineTemp[0].Trim())
                {
                    case "MinMaxElevation_X":
                        MinMaxElevation.x = (float)GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "MinMaxElevation_Y":
                        MinMaxElevation.y = (float)GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "mapSize_col_x":
                        mapSize_col_x = (int)GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "mapSize_row_y":
                        mapSize_row_y = (int)GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "TLPoint_LatLon_x":
                        TLPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "TLPoint_LatLon_y":
                        TLPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "DLPoint_LatLon_x":
                        DLPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "DLPoint_LatLon_y":
                        DLPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "TRPoint_LatLon_x":
                        TRPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "TRPoint_LatLon_y":
                        TRPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "DRPoint_LatLon_x":
                        DRPoint_LatLon.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "DRPoint_LatLon_y":
                        DRPoint_LatLon.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "Dimensions_x":
                        Dimensions.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "Dimensions_y":
                        Dimensions.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "TLOriginal_Coor_x":
                        TLOriginal_Coor.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "TLOriginal_Coor_y":
                        TLOriginal_Coor.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "DROriginal_Coor_x":
                        DROriginal_Coor.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "DROriginal_Coor_y":
                        DROriginal_Coor.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "TLPoint_Mercator_x":
                        TLPoint_Mercator.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "TLPoint_Mercator_y":
                        TLPoint_Mercator.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "DRPoint_Mercator_x":
                        DRPoint_Mercator.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;
                    case "DRPoint_Mercator_y":
                        DRPoint_Mercator.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "EPSG":
                        EPSG = (int)GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                        break;

                    case "SerializedFileName":
                        SerializedFileName = lineTemp[1];
                        break;

                }
            }

        }


    }
}