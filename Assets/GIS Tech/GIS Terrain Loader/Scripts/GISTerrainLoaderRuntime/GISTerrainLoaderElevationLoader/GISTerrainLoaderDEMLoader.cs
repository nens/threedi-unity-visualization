/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderDEMLoader
    {
        public static event ReaderEvents OnReadError;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GISTerrainLoaderProjectionSystem CoordinateReferenceSystem;

        private List<float> FixedList;
 
        private GISTerrainLoaderPrefs Prefs;

        public GISTerrainLoaderDEMLoader(GISTerrainLoaderPrefs prefs)
        {
            data = new GISTerrainLoaderFileData();
            Prefs = prefs;
            FixedList = new List<float>();
 
        }
        public void LoadFile(string filepath)
        {
            LoadComplet = false;   

            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a DEM(.dem) file.");

                if (OnReadError != null)
                {
                    OnReadError("Could not open DEM file");
                }
                return;
            }
 
            if (File.Exists(filepath))
            {
                BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open));
 
                double TL_X = reader.ReadDouble();
                double DR_X = reader.ReadDouble();

                double TL_Y = reader.ReadDouble();
                double DR_Y = reader.ReadDouble();

                double MinEle = reader.ReadDouble();
                double MaxEle = reader.ReadDouble();

                data.TLPoint_LatLon = new DVector2(TL_X, TL_Y);
                data.DRPoint_LatLon = new DVector2(DR_X, DR_Y);

                data.DLPoint_LatLon = new DVector2(TL_X, DR_Y);
                data.TRPoint_LatLon = new DVector2(DR_X, TL_Y);

                data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');

                data.mapSize_row_y = 65;
                data.mapSize_col_x = 65;

                data.floatheightData = new float[65, 65];

                for (int i = 0; i < 65; i++)
                {
                    for (int j = 0; j < 65; j++)
                    {
                        var el = reader.ReadSingle();
                        GetElevationData(el, i, j, Prefs.TerrainFixOption, data);
                    }
                }

                reader.Close();
                LoadComplet = true;      
            }
        }
        private void GetElevationData(float elevation, int x, int y, FixOption m_fixOption, GISTerrainLoaderFileData m_data)
        {

            if (m_fixOption == FixOption.ManualFix)
            {
                if (elevation < m_data.MinMaxElevation.x)
                    elevation = m_data.MinMaxElevation.x;

                if (elevation > m_data.MinMaxElevation.y)
                    elevation = m_data.MinMaxElevation.y;

            }
            else
            {
                if (elevation < m_data.MinMaxElevation.x)
                    m_data.MinMaxElevation.x = elevation;
                if (elevation > m_data.MinMaxElevation.y)
                    m_data.MinMaxElevation.y = elevation;
            }

            m_data.floatheightData[x, y] = elevation;

            FixedList.Add(elevation);
        }

    }

}


