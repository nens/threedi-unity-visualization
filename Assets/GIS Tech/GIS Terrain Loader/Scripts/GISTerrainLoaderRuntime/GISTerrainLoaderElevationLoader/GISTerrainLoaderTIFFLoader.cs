/*     Unity GIS Tech 2020-2023      */

using System;
using UnityEngine;
using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
using System.Linq;
using System.IO;
 
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTIFFLoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GISTerrainLoaderProjectionSystem CoordinateReferenceSystem;

        private GISTerrainLoaderTIFFMetadataReader TiffMetadata;

        private List<float> FixedList;

        public Byte[] WebData = new byte[0];

        private static int EPSG = 0;
        private static LinearUnit Unite = 0;
 
        private static GISTerrainLoaderPrefs Prefs;
        public GISTerrainLoaderTIFFLoader(GISTerrainLoaderPrefs prefs)
        {
            EPSG = 0;
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
            CoordinateReferenceSystem = new GISTerrainLoaderProjectionSystem();
            Prefs = prefs;

            if (Prefs.terrainDimensionMode == TerrainDimensionsMode.Manual)
                data.Dimensions = Prefs.TerrainDimensions;

            if (Prefs.TerrainFixOption == FixOption.ManualFix)
                data.MinMaxElevation = Prefs.TerrainMaxMinElevation;

        }
        GISTerrainLoaderFileData LasData = null;
        public void LoadFile(GISTerrainLoaderFileData m_LasData = null)
        {

            try
            {

                LasData = m_LasData;

                if (Prefs.EPSGCode != 0 && Prefs.projectionMode == ProjectionMode.Custom_EPSG)
                    EPSG = Prefs.EPSGCode;

                LoadComplet = false;

                if (WebData.Length > 0)
                {
                    GISTerrainLoaderTiffStreamForBytes byteStream = new GISTerrainLoaderTiffStreamForBytes(WebData);

                    using (Tiff tiff = Tiff.ClientOpen("bytes", "r", null, byteStream))
                    {
                        if (tiff == null)
                        {
                            if (OnReadError != null)
                                OnReadError("Could not open DEM file");
                        }


                        if (Prefs.readingMode == ReadingMode.Full)
                            ParseTiff(tiff,Prefs);
                        else LoadSubTiff(Prefs.SubRegionUpperLeftCoordiante, Prefs.SubRegionDownRightCoordiante);

                        tiff.Close();

                        WebData = new byte[0];
                    }
                }
                else
                {
                    using (Tiff tiff = Tiff.Open(Prefs.TerrainFilePath, "r"))
                    {
                        if (Prefs.readingMode == ReadingMode.Full)
                            ParseTiff(tiff,Prefs);
                        else LoadSubTiff(Prefs.SubRegionUpperLeftCoordiante, Prefs.SubRegionDownRightCoordiante);

                        tiff.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnReadError != null)
                    OnReadError("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);
            };

        }
        private void ParseTiff(Tiff tiff, GISTerrainLoaderPrefs Prefs)
        {
            if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                TiffMetadata = new GISTerrainLoaderTIFFMetadataReader(tiff, Prefs.projectionMode);
                CoordinateReferenceSystem = TiffMetadata.CoordinateReferenceSystem;

                if (Prefs.projectionMode == ProjectionMode.AutoDetection)
                {
                    EPSG = TiffMetadata.EPSG_Code;
                    Unite = TiffMetadata.LinearUnit;

   
                    if (EPSG == 4326 || EPSG == 32767)
                        EPSG = 0;
                  
                }


#if DotSpatial

                if (TiffMetadata.ProjectedCoordinatesystem == ProjectedCoordinateSystem.UserDefined)
                {
                    try
                    {
                        DotSpatial.Projections.ProjectionInfo source = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(EPSG);

                    }
                    catch (Exception ex)
                    {
                        if (OnReadError != null)
                            OnReadError("Undefined File Projection, GTL is Unable to load the correct projection from your file please, Change the projection mode to Custom_EPSG " + ex);
                    }

                }
#endif
                if (Prefs.projectionMode == ProjectionMode.AutoDetection && TiffMetadata.ProjectionSystem == ProjectionSystem.Undefined && EPSG == 0)
                {
                    if (OnReadError != null)
                        OnReadError("Undefined File Projection, Solution : \n 01- Reproject your file to WGS84-Geographic-Lat/Lon. '\' 02- Install DotSpatial Lib and Set The Projection Mode to 'Custom' then Set Your EPSG Code. '\' 03- Set Dimention Mode to manual (Geo-Referencing System not working in this case)  ");

                    return;
                }



            }

            int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int lenght = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

            int samplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();

            data.mapSize_row_y = lenght;
            data.mapSize_col_x = width;

            data.floatheightData = new float[width, lenght];

            int counter = -1;

            int[] raster = new int[lenght * width];

            Compression compression = (Compression)tiff.GetField(TiffTag.COMPRESSION)[0].ToInt();

            switch (Prefs.tiffElevationSource)
            {
                case TiffElevationSource.GrayScale:
                    RASTER_GrayScale(tiff, width, lenght, raster, Prefs);
                    break;

                case TiffElevationSource.DEM:

                    switch (BITSPERSAMPLE)
                    {
                        case 8:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                    DEM_8BIT_Tiled(tiff, width, lenght, counter, Prefs);
                                }
                            }
                            else
                            {
                                DEM_8BIT_NotTiled(tiff, width, lenght, counter, Prefs);
                            }

                            break;
                        case 16:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                    if (compression == Compression.NONE)
                                        DEM_16BIT_Tiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                    else
                                        DEM_16BIT_Tiled_Compressed(tiff, width, lenght, counter, Prefs);
                                }
                            }
                            else
                            {
                                if (compression == Compression.NONE)
                                    DEM_16BIT_NotTiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                else
                                    DEM_16BIT_NotTiled_Compressed(tiff, width, lenght, counter, Prefs);
                            }

                            break;

                        case 32:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                   
                                    if (compression == Compression.NONE)
                                        DEM_32BIT_Tiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                    else
                                        DEM_32BIT_Tiled_Compressed(tiff, width, lenght, counter, Prefs);

                                }
                            }else
                            {
                                if (compression == Compression.NONE)
                                    DEM_32BIT_NotTiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                else
                                    DEM_32BIT_NotTiled_Compressed(tiff, width, lenght, counter, Prefs);
                            }
                            break;

                        case 64:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                    if (compression == Compression.NONE)
                                        DEM_64BIT_Tiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                    else
                                    {
                                        DEM_64BIT_Tiled_Compressed(tiff, width, lenght, counter, Prefs);
                                    }
                                }
                            }
                            else
                            {
                                if (compression == Compression.NONE)
                                    DEM_64BIT_NotTiled_NotCompressed(tiff, width, lenght, counter, Prefs);
                                else
                                    DEM_64BIT_NotTiled_Compressed(tiff, width, lenght, counter, Prefs);
                            }

                            break;

                    }
                    break;

                case TiffElevationSource.BandsData:

                    int ElevationBandIndex = Prefs.BandsIndex;

                    GISTerrainLoaderTiffMultiBands TiffData = null;

                    TiffData = new GISTerrainLoaderTiffMultiBands(samplesPerPixel, width, lenght);
                    TiffData.TiffMetadata = TiffMetadata;
                    TiffData.CoordinateReferenceSystem = CoordinateReferenceSystem;

                    switch (BITSPERSAMPLE)
                    {
                        case 8:

                            if (compression == Compression.LZW || compression == Compression.PACKBITS)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                                {
                                    if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                    {

                                        BANDS_8BIT_Tiled_Compressed(tiff, TiffData, width, lenght, samplesPerPixel);
                                    }
                                }
                              }else
                            {
                                BANDS_8BIT_NotTiled_NotCompressed(tiff, TiffData, width, lenght, samplesPerPixel);
                            }

                            break;
                        case 16:
                            break;
                        case 32:

                            BANDS_32BIT_NotTiled_NotCompressed(tiff, TiffData, width, lenght, samplesPerPixel);

                            break;

                    }

                    BANDS_GETELEVATION_DATA(TiffData, width, lenght, ElevationBandIndex, counter, Prefs);
   
                    break;

            }
 
            READ_GEO_DATA(tiff, LasData, width, lenght, Prefs);

            LoadComplet = true;

        }
        private void DEM_8BIT_Tiled(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            UnityEngine.Debug.LogError("GeoTiff Not Supported, Contact The developper");
        }
        private void DEM_8BIT_NotTiled(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            UnityEngine.Debug.LogError("GeoTiff Not Supported, Contact The developper");
 
        }
        private void DEM_16BIT_Tiled_Compressed(Tiff tiff, int width, int lenght,int counter, GISTerrainLoaderPrefs Prefs)
        {
            //Get the tile size
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();

            //Pixel depth
            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    //Read the value and store to the buffer
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            var el = BitConverter.ToInt16(buffer, startIndex);

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }


                    }
                }

                var prog = (y * 100 / lenght);


                if (counter != prog && prog <= 99)
                {

                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }


            }
            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_16BIT_Tiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            //Get the tile size
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();

            //Pixel depth
            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    //Read the value and store to the buffer
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            var el = BitConverter.ToInt16(buffer, startIndex);

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }


                    }
                }

                var prog = (y * 100 / lenght);


                if (counter != prog && prog <= 99)
                {

                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }


            }
            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_16BIT_NotTiled_Compressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            byte[] scanline16 = new byte[tiff.ScanlineSize()];

            for (int row = 0; row < lenght; row++)
            {
                tiff.ReadScanline(scanline16, row);

                for (int col = 0; col < width; col++)
                {
                    var el = (short)((scanline16[col * 2 + 1] << 8) + scanline16[col * 2]);

                    var el1 = Convert.ToSingle(el);

                    GetElevationData(el1, col, data.mapSize_row_y - row - 1, Prefs, data);
                }


                var prog = (row * 100 / lenght);


                if (counter != prog && prog <= 99)
                {

                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

                if (Prefs.TerrainFixOption == FixOption.AutoFix)
                    FixTerrainData(Prefs);
            }
        }
        private void DEM_16BIT_NotTiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            byte[] scanline16 = new byte[tiff.ScanlineSize()];

            for (int row = 0; row < lenght; row++)
            {
                tiff.ReadScanline(scanline16, row);

                for (int col = 0; col < width; col++)
                {
                    var el = (short)((scanline16[col * 2 + 1] << 8) + scanline16[col * 2]);

                    var el1 = Convert.ToSingle(el);

                    GetElevationData(el1, col, data.mapSize_row_y - row - 1, Prefs, data);
                }


                var prog = (row * 100 / lenght);


                if (counter != prog && prog <= 99)
                {

                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

                if (Prefs.TerrainFixOption == FixOption.AutoFix)
                    FixTerrainData(Prefs);
            }
        }
        private void DEM_32BIT_Tiled_Compressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {

            //Get the tile size
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();
            string simpleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString();

            //Pixel depth
            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            //(pixelX = Column && (pixelY) = Row)

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            float el = ConvertToFloat(simpleFormat, buffer, startIndex);

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }

                    }

                }

                var prog = (y * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }


            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);

        }
        private void DEM_32BIT_Tiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {

            //Get the tile size
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();
            string simpleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString();

            //Pixel depth
            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            //(pixelX = Column && (pixelY) = Row)

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            float el = ConvertToFloat(simpleFormat, buffer, startIndex);

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }

                    }

                }

                var prog = (y * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }


            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_32BIT_NotTiled_Compressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            byte[] scanline32 = new byte[tiff.ScanlineSize()];

            float[] scanline32Bit = new float[tiff.ScanlineSize() / 2];

            for (int i = 0; i < lenght; i++)
            {
                tiff.ReadScanline(scanline32, i);

                for (int j = 0; j < width; j++)
                {
                    Buffer.BlockCopy(scanline32, 0, scanline32Bit, 0, scanline32.Length);

                    var el = scanline32Bit[j];

                    GetElevationData(el, j, data.mapSize_row_y - i - 1, Prefs, data);
                }

                var prog = (i * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);

        }
        private void DEM_32BIT_NotTiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            var scanline32 = new byte[tiff.ScanlineSize()];

            float[] scanline32Bit = new float[tiff.ScanlineSize() / 2];

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tiff.ReadScanline(scanline32, 0, i, 0);

                    Buffer.BlockCopy(scanline32, 0, scanline32Bit, 0, scanline32.Length);

                    float el = scanline32Bit[j];

                    GetElevationData(el, j, data.mapSize_row_y - i - 1, Prefs, data);
                    

                }

                var prog = (i * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_64BIT_Tiled_Compressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();

            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            float el = (float)(BitConverter.ToDouble(buffer, startIndex));

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }

                    }

                }

                var prog = (y * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }


            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_64BIT_Tiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();

            int depth = tileSize / (tileWidth * tileHeight);

            byte[] buffer = new byte[tileSize];

            for (int y = 0; y < lenght; y += tileHeight)
            {
                for (int x = 0; x < width; x += tileWidth)
                {
                    tiff.ReadTile(buffer, 0, x, y, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= buffer.Length)
                                continue;

                            int pixelX = x + i;
                            int pixelY = y + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            float el = (float)(BitConverter.ToDouble(buffer, startIndex));

                            GetElevationData(el, pixelX, data.mapSize_row_y - pixelY - 1, Prefs, data);

                        }

                    }

                }

                var prog = (y * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }


            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_64BIT_NotTiled_Compressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            var scanline64 = new byte[tiff.ScanlineSize()];

            double[] scanline64Bit = new double[tiff.ScanlineSize()];

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tiff.ReadScanline(scanline64, 0, i, 0);

                    Buffer.BlockCopy(scanline64, 0, scanline64Bit, 0, scanline64.Length);

                    float el = (float)scanline64Bit[j];

                    GetElevationData(el, j, data.mapSize_row_y - i - 1, Prefs, data);

                }

                var prog = (i * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void DEM_64BIT_NotTiled_NotCompressed(Tiff tiff, int width, int lenght, int counter, GISTerrainLoaderPrefs Prefs)
        {
            var scanline64 = new byte[tiff.ScanlineSize()];

            double[] scanline64Bit = new double[tiff.ScanlineSize()];

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tiff.ReadScanline(scanline64, 0, i, 0);

                    Buffer.BlockCopy(scanline64, 0, scanline64Bit, 0, scanline64.Length);

                    float el = (float)scanline64Bit[j];

                    GetElevationData(el, j, data.mapSize_row_y - i - 1, Prefs, data);

                }

                var prog = (i * 100 / lenght);

                if (counter != prog && prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);

                    counter = prog;
                }

            }

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void BANDS_8BIT_NotTiled_NotCompressed(Tiff tiff, GISTerrainLoaderTiffMultiBands TiffData, int width, int lenght, int samplesPerPixel)
        {
            byte[] scanline8 = new byte[tiff.ScanlineSize()];

            var BandCounter = 0;
            var C_BandCounter = 0;
            var R_BandCounter = 0;

            for (int row = 0; row < lenght; row++)
            {
                tiff.ReadScanline(scanline8, row);

                for (int col = 0; col < width; col++)
                {
                    var el = (short)((scanline8[col]) + scanline8[col]);

                    var el1 = Convert.ToSingle(el) / 2;

                    if (BandCounter >= samplesPerPixel)
                    {
                        BandCounter = 0;
                        C_BandCounter++;

                        if (C_BandCounter > width - 1)
                        {
                            C_BandCounter = 0;
                            R_BandCounter++;
                        }

                    }

                    TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el1;

                    BandCounter++;
                }


            }
        }
        private void BANDS_8BIT_Tiled_Compressed(Tiff tiff, GISTerrainLoaderTiffMultiBands TiffData, int width, int lenght, int samplesPerPixel)
        {
            int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
            int tileSize = tiff.TileSize();

            int depth = tileSize / (tileWidth * tileHeight);

            byte[] Tilebuffer = new byte[tileSize];


            for (int Row = 0; Row < lenght; Row += tileHeight)
            {
                for (int Col = 0; Col < width; Col += tileWidth)
                {
                    //Read the value and store to the buffer
                    tiff.ReadTile(Tilebuffer, 0, Col, Row, 0, 0);

                    for (int i = 0; i < tileWidth; i++)
                    {
                        for (int j = 0; j < tileHeight; j++)
                        {
                            int startIndex = (i + tileWidth * j) * depth;
                            if (startIndex >= Tilebuffer.Length)
                                continue;

                            int pixelX = Col + i;
                            int pixelY = Row + j;
                            if (pixelX >= width || pixelY >= lenght)
                                continue;

                            var el = Convert.ToSingle((short)((Tilebuffer[startIndex]) + Tilebuffer[startIndex])) / 2;

                            TiffData.BandsData[0][pixelY, pixelX] = el;
                        }


                    }
                }
            }

        }
        private void BANDS_32BIT_NotTiled_NotCompressed(Tiff tiff, GISTerrainLoaderTiffMultiBands TiffData, int width, int lenght, int samplesPerPixel)
        {
            var scanlin = new byte[tiff.ScanlineSize()];
            var scanlinBit = new float[tiff.ScanlineSize()];
            var BandCounter = 0;
            var C_BandCounter = 0;
            var R_BandCounter = 0;

            for (int i = 0; i < lenght; i++)
            {
                tiff.ReadScanline(scanlin, 0, i, 0);

                for (int j = 0; j < width * samplesPerPixel; j++)
                {
                    Buffer.BlockCopy(scanlin, 0, scanlinBit, 0, scanlin.Length);

                    float el = Convert.ToSingle(scanlinBit[j]);

                    if (BandCounter >= samplesPerPixel)
                    {
                        BandCounter = 0;
                        C_BandCounter++;

                        if (C_BandCounter > width - 1)
                        {
                            C_BandCounter = 0;
                            R_BandCounter++;
                        }

                    }

                    TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el;

                    BandCounter++;
                }
            }
        }
        private void RASTER_GrayScale(Tiff tiff, int width, int lenght, int[] raster,GISTerrainLoaderPrefs Prefs)
        {
            if (!tiff.ReadRGBAImage(width, lenght, raster))
            {
                if (OnReadError != null)
                    OnReadError("Could not read Tiff image ...");

                return;
            }

            for (int row = 0; row < lenght; ++row)
                for (int col = 0; col < width; ++col)
                {
                    int offset = (lenght - row - 1) * width + col;
                    Color color = new Color();
                    color.r = Tiff.GetR(raster[offset]);
                    color.g = Tiff.GetG(raster[offset]);
                    color.b = Tiff.GetB(raster[offset]);

                    var el = color.grayscale;

                    GetElevationData(el, col, lenght - row - 1, Prefs, data);
                }

            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                FixTerrainData(Prefs);
        }
        private void BANDS_GETELEVATION_DATA(GISTerrainLoaderTiffMultiBands TiffData, int width, int lenght, int ElevationBandIndex,int counter, GISTerrainLoaderPrefs Prefs)
        {
            if (TiffData != null)
            {
                if (TiffData.BandsData != null)
                {
                    if (ElevationBandIndex < TiffData.BandsData.Count)
                    {
                        for (int i = 0; i < lenght; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                float el = TiffData.BandsData[ElevationBandIndex][i, j];

                                GetElevationData(el, j, data.mapSize_row_y - i - 1, Prefs, data);

                            }

                            var prog = (i * 100 / lenght);

                            if (counter != prog && prog <= 99)
                            {
                                if (OnProgress != null)
                                    OnProgress("Loading File ", prog);

                                counter = prog;
                            }

                        }

                        if (Prefs.TerrainFixOption == FixOption.AutoFix)
                            FixTerrainData(Prefs);
                    }

                }
            }
        }
        private void READ_GEO_DATA(Tiff tiff,GISTerrainLoaderFileData LasData, int width, int lenght, GISTerrainLoaderPrefs Prefs)
        {
            if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                if (LasData == null)
                {
                    FieldValue[] modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
                    FieldValue[] modelTiepointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
                    FieldValue[] modelTransformationTag = tiff.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);

                    double pixelSizeX = 0;
                    double pixelSizeY = 0;

                    double TL_X = 0;
                    double TL_Y = 0;

                    double DR_X = 0;
                    double DR_Y = 0;


                    if (modelPixelScaleTag != null)
                    {
                        byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();

                        pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
                        pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                        byte[] modelTransformation = modelTiepointTag[1].GetBytes();

                        TL_X = BitConverter.ToDouble(modelTransformation, 24) - (pixelSizeX / 2);
                        TL_Y = BitConverter.ToDouble(modelTransformation, 32) - (pixelSizeY / 2);

                        DR_X = TL_X + (pixelSizeX / 2.0) + (pixelSizeX * width) - (pixelSizeX / 2);
                        DR_Y = TL_Y + (pixelSizeY / 2.0) + (pixelSizeY * lenght) - (pixelSizeY / 2);


                    }
                    else if (modelTransformationTag != null)
                    {
                        byte[] modelTransformationBytes = modelTransformationTag[1].GetBytes();

                        pixelSizeX = BitConverter.ToDouble(modelTransformationBytes, 0 * sizeof(double));
                        pixelSizeY = BitConverter.ToDouble(modelTransformationBytes, (4 + 1) * sizeof(double));

                        TL_X = BitConverter.ToDouble(modelTransformationBytes, 3 * sizeof(double)) - (pixelSizeX / 2); ;
                        TL_Y = BitConverter.ToDouble(modelTransformationBytes, (4 + 3) * sizeof(double)) - (pixelSizeY / 2); ;

                        DR_X = (TL_X + (width * pixelSizeX))- (pixelSizeX / 2);  
                        DR_Y = (TL_Y + (lenght * pixelSizeY)) - (pixelSizeY / 2);  

                    }

                    if (EPSG == 0)
                    {
                        EPSG = 4326;

                        data.TLPoint_LatLon = new DVector2(TL_X, TL_Y);
                        data.DRPoint_LatLon = new DVector2(DR_X, DR_Y);

                        data.DLPoint_LatLon = new DVector2(TL_X, data.DRPoint_LatLon.y);
                        data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);

                        data.TLOriginal_Coor = data.TLPoint_LatLon;
                        data.DROriginal_Coor = data.DRPoint_LatLon;
                    }
                    else
                    {
                        data.EPSG = EPSG;
                        data.Unite = Unite;


                        data.TLOriginal_Coor = new DVector2(TL_X, TL_Y);
                        data.DROriginal_Coor = new DVector2(DR_X, DR_Y);
 
                        data.TLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.TLOriginal_Coor, EPSG);
                        data.DRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.DROriginal_Coor, EPSG);

                        data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);
                        data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);
                    }
  

                }
                else
            if (LasData.AlreadyLoaded)
                {
                    // Read Projection
                    if (LasData.EPSG == 0)
                    {
                        data.TLPoint_LatLon = LasData.TLPoint_LatLon;
                        data.DRPoint_LatLon = LasData.DRPoint_LatLon;

                        data.DLPoint_LatLon = LasData.DLPoint_LatLon;
                        data.TRPoint_LatLon = LasData.TRPoint_LatLon;
                    }
                    else
                    {
                        data.EPSG = EPSG;

                        data.TLOriginal_Coor = LasData.TLOriginal_Coor;
                        data.DROriginal_Coor = LasData.DROriginal_Coor;

                        data.TLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.TLOriginal_Coor, EPSG);
                        data.DRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(data.DROriginal_Coor, EPSG);

                        data.DLPoint_LatLon = new DVector2(data.TLPoint_LatLon.x, data.DRPoint_LatLon.y);
                        data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);
                    }
                }

                data.Dimensions.x = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.DRPoint_LatLon, 'X');
                data.Dimensions.y = GISTerrainLoaderGeoConversion.Getdistance(data.DLPoint_LatLon, data.TLPoint_LatLon, 'Y');
 
            }

        }
        public void LoadSubTiff(DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante,GISTerrainLoaderFileData LasData = null)
        {
            try
            {
                LoadComplet = false;

                using (Tiff tiff = Tiff.Open(Prefs.TerrainFilePath, "r"))
                {

                    int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();

                    int lenght = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                    int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

                    data.mapSize_row_y = lenght;
                    data.mapSize_col_x = width;

                    data.floatheightData = new float[width, lenght];

                    int s = -1;

                    switch (BITSPERSAMPLE)
                    {
                        case 16:

                            var scanline = new byte[tiff.ScanlineSize()];

                            for (int row = 0; row < lenght; row++)
                            {
                                tiff.ReadScanline(scanline, row);

                                for (int col = 0; col < width; col++)
                                {
                                    var el = (short)((scanline[col * 2 + 1] << 8) + scanline[col * 2]);

                                    var el1 = Convert.ToSingle(el);

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

                                    data.floatheightData[col, data.mapSize_row_y - row - 1] = el1;
                                    FixedList.Add(el1);
                                }

                                var prog = (row * 100 / lenght);

                                if (s != prog && prog <= 99)
                                {
                                    if (OnProgress != null)
                                        OnProgress("Loading File ", prog);

                                    s = prog;
                                }

                            }

                            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                                FixTerrainData(Prefs);

                            break;

                        case 32:

                            scanline = new byte[tiff.ScanlineSize()];

                            float[] scanline32Bit = new float[tiff.ScanlineSize() / 2];

                            for (int i = 0; i < lenght; i++)
                            {
                                for (int j = 0; j < width; j++)
                                {
                                    tiff.ReadScanline(scanline, 0, i, 0);

                                    Buffer.BlockCopy(scanline, 0, scanline32Bit, 0, scanline.Length);

                                    float el = scanline32Bit[j];

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
                                    data.floatheightData[j, data.mapSize_row_y - i - 1] = el;
                                    FixedList.Add(el);
                                }
                            }

                            if (Prefs.TerrainFixOption == FixOption.AutoFix)
                                FixTerrainData(Prefs);

                            break;
                    }

                    if (Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                    {
                        if (LasData == null)
                        {

                            FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
                            FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);

                            byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();
                            double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
                            double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                            byte[] modelTransformation = modelTiepointTag[1].GetBytes();
                            double DownRightLon = BitConverter.ToDouble(modelTransformation, 24);
                            double DownRightLat = BitConverter.ToDouble(modelTransformation, 32);


                            double startLat = DownRightLat + (pixelSizeY / 2.0);
                            double startLon = DownRightLon + (pixelSizeX / 2.0);

                            double currentLat = startLat;
                            double currentLon = startLon;

                            data.DLPoint_LatLon = new DVector2(DownRightLon, startLat + (pixelSizeY * lenght));
                            data.TLPoint_LatLon = new DVector2(DownRightLon, DownRightLat);
                            data.DRPoint_LatLon = new DVector2(startLon + (pixelSizeX * width), startLat + (pixelSizeY * lenght));
                            data.TRPoint_LatLon = new DVector2(data.DRPoint_LatLon.x, data.TLPoint_LatLon.y);

                            // Read Projection

                            data.DLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DLPoint_LatLon, EPSG);
                            data.TLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TLPoint_LatLon, EPSG);
                            data.DRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DRPoint_LatLon, EPSG);
                            data.TRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TRPoint_LatLon, EPSG);


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

                        }
                        else
                    if (LasData.AlreadyLoaded)
                        {
                            data.TLPoint_LatLon = LasData.TLPoint_LatLon;
                            data.DRPoint_LatLon = LasData.DRPoint_LatLon;

                            data.DLPoint_LatLon = LasData.DLPoint_LatLon;
                            data.TRPoint_LatLon = LasData.TRPoint_LatLon;

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
                                    OnReadError("");
                            }
                        }
                    }
                    LoadComplet = true;

                }
            }
            catch (Exception ex)
            {
                if (OnReadError != null)
                    OnReadError("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

            };



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

                    SubZone[Step_X, Step_Y] = el;

                }

            }

            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

        }
        public bool ReadProjectionFile(string path, GISTerrainLoaderProjectionSystem CoordinateReferencesystem)
        {
            string prjFile = path.Replace(Path.GetExtension(path), ".prj");

            if (File.Exists(prjFile))
            {
                CoordinateReferencesystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
                return true;
            }
            else
                return false;
        }
        private void FixTerrainData(GISTerrainLoaderPrefs Prefs)
        {
  
            var orderdDown = FixedList.OrderBy(x => x).ToList();

            Vector2 MinMaElv = new Vector2(0, 0);

            //Min
            for (int i = 0; i < orderdDown.Count; i++)
            {
                var el = orderdDown[i];

                if (el > -9999)
                {
                    MinMaElv.x = el;
                    break;
                }
            }
            //Max
            for (int i = orderdDown.Count-1; i >0 ; i--)
            {
                var el = orderdDown[i];

                if (el < 32767)
                {
                    MinMaElv.y = el;
                    break;
                }
            }
 
            for (int i = 0; i < data.floatheightData.GetLength(0); i++)
            {
                for (int j = 0; j < data.floatheightData.GetLength(1); j++)
                {
                    var el = data.floatheightData[i, j];

                    float nullvalue = 0;

                    if (Prefs.ElevationForNullPoints == EmptyPoints.Average)
                        nullvalue = (MinMaElv.x + ((MinMaElv.y - MinMaElv.x) / 2));
                    else
                        nullvalue = Prefs.ElevationValueForNullPoints;

                    if (el == -9999 || el == -32767 || el == 32767)
                    {
                       data.floatheightData[i, j] = nullvalue;

                    }

                }
            }
            data.MinMaxElevation = MinMaElv;
        }
        public static GISTerrainLoaderTiffMultiBands LoadTiffBands(GISTerrainLoaderPrefs m_Prefs, Byte[] WebData =null)
        {
            GISTerrainLoaderTiffMultiBands TiffBandsData = new GISTerrainLoaderTiffMultiBands(0,0,0);

            try
            {
                if (WebData != null)
                {
                    GISTerrainLoaderTiffStreamForBytes byteStream = new GISTerrainLoaderTiffStreamForBytes(WebData);

                    using (Tiff tiff = Tiff.ClientOpen("bytes", "r", null, byteStream))
                    {
                        if (tiff == null)
                        {
                            if (OnReadError != null)
                                OnReadError("Could not open Tiff file");
                        }

                        TiffBandsData = ParseMultiBands(tiff, m_Prefs);

                        tiff.Close();
                    }
                }
                else
                {
                    using (Tiff tiff = Tiff.Open(m_Prefs.TerrainFilePath, "r"))
                    {
                        TiffBandsData = ParseMultiBands(tiff, m_Prefs);
                        tiff.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnReadError != null)
                    OnReadError("Couldn't Load Tiff file: " + ex.Message + "  " + Environment.NewLine);
            };
 
            return TiffBandsData;
        }
        private static GISTerrainLoaderTiffMultiBands ParseMultiBands(Tiff tiff, GISTerrainLoaderPrefs m_Prefs)
        {
            GISTerrainLoaderTiffMultiBands TiffData = new GISTerrainLoaderTiffMultiBands(0,0,0);

            int Col = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int Row = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            var samplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
            int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

            TiffData = new GISTerrainLoaderTiffMultiBands(samplesPerPixel, Col, Row);
  
            TiffData.TiffMetadata = new GISTerrainLoaderTIFFMetadataReader(tiff, m_Prefs.projectionMode);
            TiffData.CoordinateReferenceSystem = TiffData.TiffMetadata.CoordinateReferenceSystem;

            if (TiffData.TiffMetadata.GeographicTypeGeoKey == GeographicCoordinateSystem.Undefined)
            {
                if (OnReadError != null)
                    OnReadError("Error While Loading File : Undefined Projection System, reproject your file to one of supported Projection or Set Dimention Mode to manual");
            }

            switch (BITSPERSAMPLE)
            {
                case 8:

                    byte[] scanline8 = new byte[tiff.ScanlineSize()];
                    float[] scanlinBit = new float[tiff.ScanlineSize()];

                    int BandCounter = 0;
                    int C_BandCounter = 0;
                    int R_BandCounter = 0;
                    for (int row = 0; row < Row; row++)
                    {
                        tiff.ReadScanline(scanline8, row);

                        for (int col = 0; col < Col; col++)
                        {
                            var el = (short)((scanline8[col]) + scanline8[col]);

                            var el1 = Convert.ToSingle(el) / 2;

                            if (BandCounter >= samplesPerPixel)
                            {
                                BandCounter = 0;
                                C_BandCounter++;

                                if (C_BandCounter > Col - 1)
                                {
                                    C_BandCounter = 0;
                                    R_BandCounter++;
                                }

                            }

                            TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el1;

                            BandCounter++;
                        }


                    }

                    break;
                case 16:
                    break;
                case 32:
                    var scanlin = new byte[tiff.ScanlineSize()];
                    scanlinBit = new float[tiff.ScanlineSize()];
                    BandCounter = 0;
                    C_BandCounter = 0;
                    R_BandCounter = 0;

                    for (int i = 0; i < Row; i++)
                    {
                        tiff.ReadScanline(scanlin, 0, i, 0);

                        for (int j = 0; j < Col * samplesPerPixel; j++)
                        {
                            Buffer.BlockCopy(scanlin, 0, scanlinBit, 0, scanlin.Length);

                            float el = Convert.ToSingle(scanlinBit[j]);

                            if (BandCounter >= samplesPerPixel)
                            {
                                BandCounter = 0;
                                C_BandCounter++;

                                if (C_BandCounter > Col - 1)
                                {
                                    C_BandCounter = 0;
                                    R_BandCounter++;
                                }

                            }

                            TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el;

                            BandCounter++;
                        }
                    }
                    break;

            }



            FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
            FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);

            byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();

            double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
            double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

            byte[] modelTransformation = modelTiepointTag[1].GetBytes();

            double originLon = BitConverter.ToDouble(modelTransformation, 24);
            double originLat = BitConverter.ToDouble(modelTransformation, 32);


            double startLat = originLat + (pixelSizeY / 2.0);
            double startLon = originLon + (pixelSizeX / 2.0);

            double currentLat = startLat;
            double currentLon = startLon;

            TiffData.data.DLPoint_LatLon = new DVector2(originLon, startLat + (pixelSizeY * Row));
            TiffData.data.TLPoint_LatLon = new DVector2(originLon, originLat);
            TiffData.data.DRPoint_LatLon = new DVector2(startLon + (pixelSizeX * Col), startLat + (pixelSizeY * Row));
            TiffData.data.TRPoint_LatLon = new DVector2(TiffData.data.DRPoint_LatLon.x, TiffData.data.TLPoint_LatLon.y);


            // Read Projection
            if (EPSG == 0) EPSG = TiffData.CoordinateReferenceSystem.EPSG_Code;
            TiffData.data.DLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.DLPoint_LatLon, EPSG);
            TiffData.data.TRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.TRPoint_LatLon, EPSG);
            TiffData.data.TLPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.TLPoint_LatLon, EPSG);
            TiffData.data.DRPoint_LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.DRPoint_LatLon, EPSG);

            return TiffData;
        }
        private static Color getSample(int x, int y, int[] raster, int width, int height)
        {
            int offset = (height - y - 1) * width + x;
            int red = Tiff.GetR(raster[offset]);
            int green = Tiff.GetG(raster[offset]);
            int blue = Tiff.GetB(raster[offset]);
            return new Color(red / 255.0f, green / 255.0f, blue / 255.0f, 255);
        }
        public static Texture2D TiffToTexture2D(string fileName)
        {
            Texture2D tex = new Texture2D(2, 2);

            using (Tiff tiff = Tiff.Open(fileName, "r"))
            {
                // Find the width and height of the image
                int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                // Read the image into the memory buffer
                int[] raster = new int[height * width];

                if (!tiff.ReadRGBAImage(width, height, raster))
                {
                    UnityEngine.Debug.LogError("Could not read Tiff image ");
                }

                tex = new Texture2D(width, height);

                for (int row = 0; row < height; row++)
                    for (int col = 0; col < width; col++)
                    {
                        Color color = getSample(col, row, raster, width, height);
                        tex.SetPixel(col, height - row-1, color);

                    }
                tex.Apply();
            }
            return tex;
        }
        public static Texture2D TiffToTexture2D(byte[] filedata)
        {
            GISTerrainLoaderTiffStreamForBytes byteStream = new GISTerrainLoaderTiffStreamForBytes(filedata);

            Texture2D tex = new Texture2D(2, 2);

            using (Tiff tiff = Tiff.ClientOpen("bytes", "r", null, byteStream))
            {
                // Find the width and height of the image

                int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                int[] raster = new int[height * width];

                if (tiff == null || !tiff.ReadRGBAImage(width, height, raster))
                {
                    if (OnReadError != null)
                        OnReadError("Could not read Tiff image ");
                }

                // Read the image into the memory buffer

                tex = new Texture2D(width, height);

                for (int row = 0; row < height; row++)
                    for (int col = 0; col < width; col++)
                    {
                        Color color = getSample(col, row, raster, width, height);
                        tex.SetPixel(col, row, color);

                    }
                tex.Apply();
            }
            return tex;
        }
        private void GetElevationData(float elevation, int x, int y, GISTerrainLoaderPrefs Prefs , GISTerrainLoaderFileData m_data)
        {

            if (Prefs.TerrainFixOption == FixOption.ManualFix)
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

        private float ConvertToFloat(string SimpleFormat, byte[] buffer, int startIndex)
        {
            float value = 0f;

            switch (SimpleFormat)
            {
                case "INT":
                    value = BitConverter.ToInt16(buffer, startIndex);
                    break;
                case "UINT":
                    value = BitConverter.ToUInt16(buffer, startIndex);
                    break;
                case "IEEEFP":
                    value = BitConverter.ToSingle(buffer, startIndex);
                    break;
                case "COMPLEXINT":
                    value = BitConverter.ToInt32(buffer, startIndex);
                    break;
                case "COMPLEXIEEEFP":
                    value = BitConverter.ToInt32(buffer, startIndex);
                    break;
                default:
                    value = BitConverter.ToSingle(buffer, startIndex);
                    break;
            }
            return value;
        }
    }

    public class GISTerrainLoaderTiffMultiBands
    {
        public GISTerrainLoaderFileData data;
        public int BandsNumber = 0;
        public List<float[,]> BandsData = null;
        public GISTerrainLoaderProjectionSystem CoordinateReferenceSystem;
        public GISTerrainLoaderTIFFMetadataReader TiffMetadata;
        public int samplesPerPixel = 0;


        public GISTerrainLoaderTiffMultiBands(int m_BandsNumber, int Col, int Row)
        {
            data = new GISTerrainLoaderFileData();

            BandsData = new List<float[,]>();

            BandsNumber = m_BandsNumber;

            for (int b = 0; b < BandsNumber; b++)
            {
                var list = new float[Row, Col];
                BandsData.Add(list);
            }
            data.mapSize_col_x = Col;
            data.mapSize_row_y = Row;

        }
        public float GetValue(int BandID, int row, int col)
        {
            return BandsData[BandID][row, col];
        }
        public float GetValue(int BandsNumber, DVector2 LatLon)
        {
            float value = 0;

            var rang_x = Math.Abs(Math.Abs(data.DRPoint_LatLon.x) - Math.Abs(data.TLPoint_LatLon.x));
            var rang_y = Math.Abs(Math.Abs(data.TLPoint_LatLon.y) - Math.Abs(data.DRPoint_LatLon.y));

            var rang_px = Math.Abs(Math.Abs(LatLon.x) - Math.Abs(data.TLPoint_LatLon.x));
            var rang_py = Math.Abs(Math.Abs(data.TLPoint_LatLon.y) - Math.Abs(LatLon.y));

            int localLat = (int)(rang_px * data.mapSize_col_x / rang_x);
            int localLon = (int)(rang_py * data.mapSize_row_y / rang_y);

            if (localLat > data.mapSize_col_x - 1) localLat = data.mapSize_col_x - 1;
            if (localLon > data.mapSize_row_y - 1) localLon = data.mapSize_row_y - 1;

            value = BandsData[BandsNumber][localLon, localLat];

            return value;
        }
        public float[] GetValues(int BandsNumber, DVector2 LatLon)
        {
            float[] data = new float[BandsNumber];

            for (int i = 0; i < BandsNumber; i++)
                data[i] = GetValue(i, LatLon);

            return data;
        }

    }
    public class GISTerrainLoaderTiffStreamForBytes : TiffStream
    {
        private byte[] m_bytes;
        private int m_position;

        public GISTerrainLoaderTiffStreamForBytes(byte[] bytes)
        {
            m_bytes = bytes;
            m_position = 0;
        }

        public override int Read(object clientData, byte[] buffer, int offset, int count)
        {
            if ((m_position + count) > m_bytes.Length)
                return -1;

            Buffer.BlockCopy(m_bytes, m_position, buffer, offset, count);
            m_position += count;
            return count;
        }

        public override void Write(object clientData, byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("This stream is read-only");
        }

        public override long Seek(object clientData, long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > m_bytes.Length)
                        return -1;

                    m_position = (int)offset;
                    return m_position;

                case SeekOrigin.Current:
                    if ((offset + m_position) > m_bytes.Length)
                        return -1;

                    m_position += (int)offset;
                    return m_position;

                case SeekOrigin.End:
                    if ((m_bytes.Length - offset) < 0)
                        return -1;

                    m_position = (int)(m_bytes.Length - offset);
                    return m_position;
            }

            return -1;
        }

        public override void Close(object clientData)
        {
            // nothing to do
        }

        public override long Size(object clientData)
        {
            return m_bytes.Length;
        }
    }
    public struct AreaBounds
    {
        public double west;
        public double east;
        public double north;
        public double south;

        public bool IsEmpty => west == east || north == south;

        public AreaBounds(double west, double east, double north, double south)
        {
            this.west = west;
            this.east = east;
            this.north = north;
            this.south = south;
        }

        public AreaBounds(AreaBounds other) : this(other.west, other.east, other.north, other.south) { }

    }

}
