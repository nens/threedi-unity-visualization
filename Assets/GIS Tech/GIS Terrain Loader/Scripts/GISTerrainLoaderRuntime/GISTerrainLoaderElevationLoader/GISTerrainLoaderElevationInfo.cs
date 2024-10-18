/*     Unity GIS Tech 2020-2023      */


using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderElevationInfo
    {
 
        public GISTerrainLoaderFileData data;

        private TerrainData tdata;

        public float[,] tdataHeightmap;

        private int lastX;

        public GISTerrainLoaderElevationInfo()
        {

        }
       
        private float UnderWateroffest = 0;
        public async Task GenerateHeightMap(GISTerrainLoaderPrefs prefs, GISTerrainTile item, TerrainSide terrainSide = TerrainSide.Non)
        {
            if (item != null)
            {
                float MaxElevation = data.MinMaxElevation.y;
                float MinElevation = data.MinMaxElevation.x;
                float elevationRange = data.MinMaxElevation.y - data.MinMaxElevation.x;

                if (prefs.UnderWater == OptionEnabDisab.Enable)
                {
                    UnderWateroffest = Math.Abs(MinElevation);
                    MinElevation = data.MinMaxElevation.x + UnderWateroffest;
                    MaxElevation = data.MinMaxElevation.y + UnderWateroffest;
                    elevationRange = MaxElevation - MinElevation;

                }
                else
                {
                    if (data.MinMaxElevation.x < 0)
                    {
                        MinElevation = data.MinMaxElevation.x;
                        elevationRange = data.MinMaxElevation.y - data.MinMaxElevation.x;
                    }

                }

                tdata = item.terrain.terrainData;

                if (tdataHeightmap == null)
                    tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
                if (tdata == null)
                {
                    tdata = item.terrain.terrainData;
                    tdata.baseMapResolution = prefs.baseMapResolution;
                    tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
                    tdata.size = item.size;

                    if (tdataHeightmap == null)
                        tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
                }

                float thx = tdata.heightmapResolution - 1;
                float thy = tdata.heightmapResolution - 1;


                var y_Terrain_Col_num = (data.mapSize_row_y / prefs.terrainCount.y);
                var x_Terrain_row_num = (data.mapSize_col_x / prefs.terrainCount.x);

                int tw = tdata.heightmapResolution;
                int th = tdata.heightmapResolution;

                for (int x = lastX; x < tw; x++)
                {
                    for (int y = 0; y < th; y++)
                    {
                        var x_from = item.Number.x * x_Terrain_row_num;
                        var x_To = (item.Number.x * x_Terrain_row_num + x_Terrain_row_num);

                        var y_from = (item.Number.y * y_Terrain_Col_num);
                        var y_To = (item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num);

                        float fpx = Mathf.Lerp(x_from, x_To, x / thx);
                        float fpy = Mathf.Lerp(y_from, y_To, y / thy);

                        int px = Mathf.FloorToInt(fpx);
                        int py = Mathf.FloorToInt(fpy);

                        if (y == tdata.heightmapResolution - 1)
                        {
                            if (y_To >= data.mapSize_row_y - 1)
                            {
                                y_To = data.mapSize_row_y - 1;
                            }

                            py = y_To;
                        }

                        if (x == tdata.heightmapResolution - 1)
                        {
                            if (px >= data.mapSize_col_x - 1)
                            {
                                px = data.mapSize_col_x - 1;
                            }
                            px = x_To;
                        }

                        if (px > data.floatheightData.GetLength(0) - 1)
                            px = data.floatheightData.GetLength(0) - 1;
                        if (py > data.floatheightData.GetLength(1) - 1)
                            py = data.floatheightData.GetLength(1) - 1;

                        var Rel = data.GetElevation(fpx, fpy);

                        if (prefs.UnderWater == OptionEnabDisab.Disable && Rel < 0)
                            Rel = 0;

                        if (prefs.UnderWater == OptionEnabDisab.Enable)
                            Rel = Rel + UnderWateroffest;

                        var el = (((Rel - MinElevation)) / elevationRange);

                        var m_x = tdataHeightmap.GetLength(0);
                        var m_y = tdataHeightmap.GetLength(1);

                        switch (terrainSide)
                        {
                            case TerrainSide.Non:
                                tdataHeightmap[y, x] = el;
                                break;
                            case TerrainSide.Bottom:
                                tdataHeightmap[m_x - y -1, x] = el;
                                break;
                            case TerrainSide.Top:
                                tdataHeightmap[m_x - y - 1, x] = el;
                                break;
                            case TerrainSide.Right:
                                tdataHeightmap[y,m_y - x - 1 ] = el;
                                break;
                            case TerrainSide.Left:
                                tdataHeightmap[y,m_y - x - 1] = el;
                                break;

                            case TerrainSide.TopRight:
                                tdataHeightmap[m_x-y-1,m_y - x - 1 ] = el;
                                break;

                            case TerrainSide.TopLeft:
                                tdataHeightmap[m_x - y - 1, m_y - x - 1] = el;                                //tdataHeightmap[m_y - x - 1, m_x - y - 1] = el;
                                break;


                            case TerrainSide.BottomRight:
                                tdataHeightmap[m_x - y - 1, m_y - x - 1] = el;
                                break;

                            case TerrainSide.BottomLeft:
                                tdataHeightmap[m_x - y - 1, m_y - x - 1] = el;                                //tdataHeightmap[m_y - x - 1, m_x - y - 1] = el;
                                break;
                        }
                    }
                    lastX = x;
                }

                lastX = 0;
                tdata.SetHeights(0, 0, tdataHeightmap);

                tdata = null;

                await Task.Delay(TimeSpan.FromSeconds(0.01));
            }
        }

        public void RuntimeGenerateHeightMap(GISTerrainLoaderPrefs prefs, GISTerrainTile item)
        {

            float elevationRange = data.MinMaxElevation.y - data.MinMaxElevation.x;
            float MaxElevation = data.MinMaxElevation.y;
            float MinElevation = data.MinMaxElevation.x;

            if (prefs.UnderWater == OptionEnabDisab.Enable)
            {
                UnderWateroffest = Math.Abs(MinElevation);
                MinElevation = data.MinMaxElevation.x + UnderWateroffest;
                MaxElevation = data.MinMaxElevation.y + UnderWateroffest;
                elevationRange = MaxElevation - MinElevation;

            }
            else
            {
                if (data.MinMaxElevation.x < 0)
                {
                    MinElevation = data.MinMaxElevation.x;
                    elevationRange = data.MinMaxElevation.y - data.MinMaxElevation.x;
                }
            }

            tdata = item.terrain.terrainData;      

            if (tdataHeightmap == null)
                tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];

            if (tdata == null)
            {
                tdata = item.terrain.terrainData;
                tdata.baseMapResolution = prefs.baseMapResolution;
                tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
                tdata.size = item.size;

                if (tdataHeightmap == null)
                    tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
            }

           

            float thx = tdata.heightmapResolution - 1;
            float thy = tdata.heightmapResolution - 1;


            var y_Terrain_Col_num = (data.mapSize_row_y / prefs.terrainCount.y);
            var x_Terrain_row_num = (data.mapSize_col_x / prefs.terrainCount.x);

            int tw = tdata.heightmapResolution;
            int th = tdata.heightmapResolution;

            for (int x = lastX; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {

                    var x_from = item.Number.x * x_Terrain_row_num;
                    var x_To = (item.Number.x * x_Terrain_row_num + x_Terrain_row_num);

                    var y_from = (item.Number.y * y_Terrain_Col_num);
                    var y_To = (item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num);

                    float fpx = Mathf.Lerp(x_from, x_To, x / thx);
                    float fpy = Mathf.Lerp(y_from, y_To, y / thy);

                    int px = Mathf.FloorToInt(fpx);
                    int py = Mathf.FloorToInt(fpy);

                    if (y == tdata.heightmapResolution - 1)
                    {
                        if (y_To >= data.mapSize_row_y - 1)
                        {
                            y_To = data.mapSize_row_y - 1;
                        }

                        py = y_To;
                    }

                    if (x == tdata.heightmapResolution - 1)
                    {
                        if (px >= data.mapSize_col_x - 1)
                        {
                            px = data.mapSize_col_x - 1;
                        }
                        px = x_To;
                    }

                    if (px > data.floatheightData.GetLength(0) - 1)
                        px = data.floatheightData.GetLength(0) - 1;
                    if (py > data.floatheightData.GetLength(1) - 1)
                        py = data.floatheightData.GetLength(1) - 1;

                    var Rel = data.GetElevation(fpx, fpy);

                    if (prefs.UnderWater == OptionEnabDisab.Disable && Rel < 0)
                        Rel = 0;

                    if (prefs.UnderWater == OptionEnabDisab.Enable)
                        Rel = Rel + UnderWateroffest;

                    var el = (((Rel - MinElevation)) / elevationRange);

                    tdataHeightmap[y, x] = el;
                }
                lastX = x;
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);

            tdata = null;

            item.ElevationState = ElevationState.Loaded;


        }
        public void GetData(GISTerrainLoaderFileData m_data)
        {
            data = new GISTerrainLoaderFileData();

            data.AlreadyLoaded = m_data.AlreadyLoaded;

            data.MinMaxElevation = m_data.MinMaxElevation;


            data.mapSize_row_y = m_data.mapSize_row_y;
            data.mapSize_col_x = m_data.mapSize_col_x;

            data.Tiles = m_data.Tiles;


            data.DLPoint_LatLon = m_data.DLPoint_LatLon;
            data.TRPoint_LatLon = m_data.TRPoint_LatLon;

            data.TLPoint_LatLon = m_data.TLPoint_LatLon;
            data.DRPoint_LatLon = m_data.DRPoint_LatLon;

            data.DROriginal_Coor = m_data.DROriginal_Coor;
            data.TLOriginal_Coor = m_data.TLOriginal_Coor;

            data.Dimensions = m_data.Dimensions;

            data.dim = m_data.dim;
            data.cellsize = m_data.cellsize;

            data.floatheightData = m_data.floatheightData;

            data.EPSG = m_data.EPSG;
            data.Unite = m_data.Unite;


            data.TLPoint_Mercator = GISTerrainLoaderGeoConversion.LatLongToMercat(m_data.TLPoint_LatLon.x, m_data.TLPoint_LatLon.y);
            data.DRPoint_Mercator = GISTerrainLoaderGeoConversion.LatLongToMercat(m_data.DLPoint_LatLon.x, m_data.DLPoint_LatLon.y);


        }


    }
}