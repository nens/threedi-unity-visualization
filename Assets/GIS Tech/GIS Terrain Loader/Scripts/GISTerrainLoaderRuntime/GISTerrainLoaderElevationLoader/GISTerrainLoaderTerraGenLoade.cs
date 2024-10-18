/*     Unity GIS Tech 2020-2023      */

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTerraGenLoade
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GISTerrainLoaderPrefs Prefs;

        public GISTerrainLoaderTerraGenLoade(GISTerrainLoaderPrefs prefs)
        {
            data = new GISTerrainLoaderFileData();
            Prefs = prefs;
         }
        public void LoadFile(string filePath)
        {
            try
            {
                LoadComplet = false;

                data = new GISTerrainLoaderFileData();

            FileInfo file = new FileInfo(filePath);

            FileStream s = file.Open(FileMode.Open, FileAccess.Read);

                LoadStream(s);

            s.Close();
            }
            catch (Exception ex)
            {
                if (OnReadError != null)
                {
                    OnReadError("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);
                }
            };



        }


        public void LoadStream(Stream s)
        {
            data.Dimensions = new DVector2(30, 30);

            int size = (int)Math.Sqrt(s.Length);

            BinaryReader bs = new BinaryReader(s);

            bool eof = false;
            if (Encoding.ASCII.GetString(bs.ReadBytes(16)) == "TERRAGENTERRAIN ")
            {
                while (eof == false)
                {
                    string tmp = Encoding.ASCII.GetString(bs.ReadBytes(4));

                    switch (tmp)
                    {

                        case "SIZE":
                            int sztmp = bs.ReadInt16() + 1;

                            data.mapSize_row_y = sztmp;
                            data.mapSize_col_x = sztmp;

                            bs.ReadInt16();
                            break;
                        case "XPTS":
                            data.mapSize_row_y = bs.ReadInt16();
                            bs.ReadInt16();
                            break;
                        case "YPTS":
                            data.mapSize_col_x = bs.ReadInt16();
                            bs.ReadInt16();
                            break;
                        case "ALTW":
                            data.floatheightData = new float[data.mapSize_row_y, data.mapSize_col_x];
                            eof = true;
                            double heightScale = (double)bs.ReadInt16() / 65536.0;
                            double baseHeight = (double)bs.ReadInt16();
                            for (int i = 0; i < data.mapSize_col_x; i++)
                            {
                                for (int j = 0; j < data.mapSize_row_y; j++)
                                {
                                   
                                    var el = baseHeight + (double)bs.ReadInt16() * heightScale / 65536;

                                    data.floatheightData[i, j] = (float)el;

                                    if ((float)el > -9900)
                                    {

                                        if ((float)el < data.MinMaxElevation.x)
                                            data.MinMaxElevation.x = (float)el;
                                        if ((float)el > data.MinMaxElevation.y)
                                            data.MinMaxElevation.y = (float)el;

                                        if(OnProgress!=null)
                                        OnProgress("Loading File ", i * j * 100 / (data.mapSize_row_y * data.mapSize_col_x));
                                    }
                                   
                                }
                            }

                            break;
                        default:
                            bs.ReadInt32();
                            break;
                    }
                }
            }

            bs.Close();
        
            LoadComplet = true;

        }

    }


}





