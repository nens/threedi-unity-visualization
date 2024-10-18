/*     Unity GIS Tech 2020-2023      */

using BitMiracle.LibTiff.Classic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
 
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTiffExporter
    {
        private GISTerrainContainer container;
        private string path;
        private TiffElevation tiffElevation;
        private Vector2 minMaxElevation;
        private static int EPSGCode = 4326;
        public GISTerrainLoaderTiffExporter(string m_path, GISTerrainContainer m_container, TiffElevation m_tiffElevation, Vector2 m_minMaxElevation)
        {
            path = m_path;
            container = m_container;
            tiffElevation = m_tiffElevation;
            minMaxElevation = m_minMaxElevation;
            EPSGCode = m_container.data.EPSG;
            if(EPSGCode==0) EPSGCode = 4326;
         }
        public GISTerrainLoaderTiffExporter(string m_path, GISTerrainContainer m_container)
        {
            path = m_path;
            container = m_container;
            tiffElevation = TiffElevation.Auto;
            EPSGCode = m_container.data.EPSG;
            if (EPSGCode == 0) EPSGCode = 4326;
        }
        public void ExportToTiff()
        {
            int heightmapResolution = -1;

            int cx = container != null ? container.TerrainCount.x : 1;
            int cy = container != null ? container.TerrainCount.y : 1;

            foreach (var terrain in container.terrains)
            {
                if (heightmapResolution == -1) heightmapResolution = terrain.terrainData.heightmapResolution;
                else if (heightmapResolution != terrain.terrainData.heightmapResolution)
                {
                    Debug.LogError("Error Terrains have different heightmap resolution.");
                    return;
                }
            }

            float RWMinElevation = 99999;
            float RWMaxElevation = -99999;


            if (tiffElevation == TiffElevation.Custom)
            {
                container.data.MinMaxElevation = minMaxElevation;
            }

            float RW_Range = container.data.MinMaxElevation.y - container.data.MinMaxElevation.x;

            float[,] RWElevations = new float[heightmapResolution* container.TerrainCount.x, heightmapResolution * container.TerrainCount.y];
    

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    var tdata = container.terrains[x, y].terrainData;

                    float[,] rawHeights = tdata.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {

                            var H_elevation = rawHeights[dx,dy];

                            var RWE = ((H_elevation * RW_Range)) + container.data.MinMaxElevation.x;

                            if (RWE < RWMinElevation)
                                RWMinElevation = RWE;
                            if (RWE > RWMaxElevation)
                                RWMaxElevation = RWE;

                            int col = dx + (heightmapResolution * y);
                            int row = dy + (heightmapResolution * x);


                            RWElevations[RWElevations.GetLength(0) - col - 1,  row] = RWE;
                        }

                    }

                }
            }
            container.data.floatheightData = RWElevations;
            container.data.MinMaxElevation = new Vector2(RWMinElevation, RWMaxElevation);
            WriteTiff(path, container.data);

        }


        //private const TiffTag ProjLinearUnitsGeoKey = (TiffTag)ExtraTiffTag.GeoKeyDirectoryTag;
 
        private const TiffTag GeoKeyDirectoryTag = (TiffTag)ExtraTiffTag.GeoKeyDirectoryTag;
        private const TiffTag GeoDoubleParamsTag = (TiffTag)34736;
        private const TiffTag GeoAsciiParamsTag = (TiffTag)34737;
        private const TiffTag GDAL_METADATA = (TiffTag)42112;
        private const TiffTag GDAL_NODATA = (TiffTag)42113;

        private Tiff.TiffExtendProc m_parentExtender;
        private void TagExtender(Tiff tiff)
        {
 
            TiffFieldInfo[] tiffFieldInfo =
            {
        new TiffFieldInfo((TiffTag)ExtraTiffTag.ProjLinearUnitsGeoKey, 1, 1, TiffType.SHORT, FieldBit.Custom, false, true, "LinearUnits"),
        new TiffFieldInfo(TiffTag.GEOTIFF_MODELTIEPOINTTAG, 6, 6, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELTILEPOINTTAG"),
        new TiffFieldInfo(TiffTag.GEOTIFF_MODELPIXELSCALETAG, 3, 3, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELPIXELSCALETAG"),
        new TiffFieldInfo(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG, 16, 16, TiffType.DOUBLE, FieldBit.Custom, true, false, "GEOTIFF_MODELTRANSFORMATIONTAG"),
        new TiffFieldInfo(GeoKeyDirectoryTag, (short)GeoTags.Length, (short)GeoTags.Length, TiffType.SHORT, FieldBit.Custom, true, false, "GeoKeyDirectoryTag")
        };
            tiff.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);
            if (m_parentExtender != null)
                m_parentExtender(tiff);
        }

        public void WriteTiff(string fileName, GISTerrainLoaderFileData item)
        {
            float[,] data = item.floatheightData;
            int Col = data.GetLength(1);
            int Row = data.GetLength(0);

            double PixelScaleX = Math.Abs((item.DROriginal_Coor.x - item.TLOriginal_Coor.x)) / Col;
            double PixelScaleY = Math.Abs((item.TLOriginal_Coor.y - item.DROriginal_Coor.y)) / Row;

            Tiff.TiffExtendProc extender = TagExtender;
            m_parentExtender = Tiff.SetTagExtender(extender);

            using (Tiff tiff = Tiff.Open(fileName, "w"))
            {
                if (tiff == null)
                    return;

                tiff.SetField(TiffTag.IMAGEWIDTH, Col);
                tiff.SetField(TiffTag.IMAGELENGTH, Row);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 32);
                tiff.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.IEEEFP);

                tiff.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                tiff.SetField(TiffTag.ROWSPERSTRIP, Col);
                tiff.SetField(TiffTag.XRESOLUTION, 1.0);
                tiff.SetField(TiffTag.YRESOLUTION, 1.0);
                tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                tiff.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tiff.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                tiff.SetField(TiffTag.MAXSAMPLEVALUE, item.MinMaxElevation.y);
                tiff.SetField(TiffTag.MINSAMPLEVALUE, 0);
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);
                tiff.SetField(TiffTag.COPYRIGHT, "Created By GIS Terrain Downloader Pro From Unity GISTech");

                tiff.SetField((TiffTag)ExtraTiffTag.ProjLinearUnitsGeoKey, 1, (int)container.data.Unite);

                double[] geotiff_modeltiepointtag = new double[] { 0, 0, 0, item.TLOriginal_Coor.x, item.TLOriginal_Coor.y, 0 };
                tiff.SetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG, 6, (object)geotiff_modeltiepointtag);

                double[] modelpixelscaletag = new double[] { PixelScaleX, PixelScaleY, 0.5 };
                
                tiff.SetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG, 3, modelpixelscaletag);
               
                tiff.SetField(GeoKeyDirectoryTag, GeoTags);

                float[] source = new float[Col];

                for (int i = 0; i < Row; i++)
                {
                    for (int j = 0; j < Col; j++)
                        source[j] = data[i, j];

                    byte[] dest = new byte[source.Length * sizeof(float)];
                    Buffer.BlockCopy(source, 0, dest, 0, dest.Length);
                    tiff.WriteScanline(dest, i);
                }

                tiff.Dispose();
            }

        }





        public enum ModelTypeEnum
        {
            ModelTypeProjected = 1,
            ModelTypeGeographic = 2
        }
        public enum RasterTypeEnum
        {
            RasterPixelIsArea = 1,
            RasterPixelIsPoint = 2
        }
        private static RasterTypeEnum RasterType { get; set; } = RasterTypeEnum.RasterPixelIsPoint;
        private static ModelTypeEnum ModelType { get; set; } = ModelTypeEnum.ModelTypeProjected;
        private static UInt16[] GeoTags
        {
            get
            {
                var index = 0;
                UInt16 count = 2;

                count += (EPSGCode > 0) ? (UInt16)1 : (UInt16)0;
                count += (EPSGCode > 0) ? (UInt16)1 : (UInt16)0;

                var geotags = new UInt16[(count + 1) * 4];

                geotags.SetValue((UInt16)1, index++);
                geotags.SetValue((UInt16)1, index++);
                geotags.SetValue((UInt16)1, index++);
                geotags.SetValue((UInt16)count, index++);

                geotags.SetValue((UInt16)1024, index++);
                geotags.SetValue((UInt16)0, index++);
                geotags.SetValue((UInt16)1, index++);
                geotags.SetValue((UInt16)ModelType, index++);

                geotags.SetValue((UInt16)1025, index++);
                geotags.SetValue((UInt16)0, index++);
                geotags.SetValue((UInt16)1, index++);
                geotags.SetValue((UInt16)RasterType, index++);

                if (EPSGCode > 0)
                {
                    geotags.SetValue((UInt16)2048, index++);
                    geotags.SetValue((UInt16)0, index++);
                    geotags.SetValue((UInt16)1, index++);
                    geotags.SetValue((UInt16)EPSGCode, index++);
                }
                //if (EPSGCode > 0)
                //{
                //    geotags.SetValue((UInt16)4096, index++);
                //    geotags.SetValue((UInt16)0, index++);
                //    geotags.SetValue((UInt16)1, index++);
                //    geotags.SetValue((UInt16)EPSGCode, index++);
                //}
                return geotags;
            }
        }
    }

}