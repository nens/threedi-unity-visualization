/*     Unity GIS Tech 2020-2022      */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSettings_SO : ScriptableObject
    {
        [Header("Folder Parameters")]
        [Space(3)]
        public string TextureFolderName = "_Textures";
        public string VectorDataFolderName = "_VectorData";
        [Space(3)]
        [Header("Supported DEM")]
        [Space(3)]
        public List<string> SupportedDEMFiles = new List<string> { ".flt",".bin",".tif",".tiff", ".bil", ".hgt", ".asc", ".las", ".ter", ".png",".raw"};
        public List<string> SupportedTextures = new List<string> { ".png", ".jpg", ".tif" };
        public List<string> SupportedVectorData = new List<string> { ".shp", ".osm", ".kml",".gpx" };
        [Space(3)]
        [Header("GeoReferenced DEM Files")]
        [Space(3)]
        public List<string> GeoFile = new List<string> { ".flt", ".bin", ".tif", ".tiff", ".bil", ".hgt", ".asc", ".las" };

        [Space(3)]
        [Header("Tile Format")]
        [Space(3)]
        public List<TextureSourceFormat> TextureFormats = new List<TextureSourceFormat>();

        public bool IsGeoFile(string ext)
        {
            bool valid = false;

            if (GeoFile.Contains(ext))
            {
                valid = true;
            }
            return valid;
        }

        public bool IsValidTerrainFile(string filepath)
        {
            bool valid = false;

            if (!string.IsNullOrEmpty(filepath))
            {
                var ext = Path.GetExtension(filepath);
 
                if (SupportedDEMFiles.Contains(ext))
                {
                    valid = true;
                }

                if (!valid)
                {
                    Debug.LogError("DEM File not supprted, try another one ");
                }
            }else
            Debug.LogError("DEM File Path is not correct.... ");

            return valid;
        }
    }
    [Serializable]
    public class TextureSourceFormat
    {
        public string SoftwareName;
        public string Format;
        public Vector2Int StartTileNumber= new Vector2Int(0,0);
        public TextureOrder TilesOrder = TextureOrder.Col_Row;
        public TextureSourceFormat(string m_SoftwareName, string m_Format)
        {
            SoftwareName = m_SoftwareName;
            Format = m_Format;
        }
        public TextureSourceFormat()
        {
            SoftwareName = "";
            Format = "";
            StartTileNumber = new Vector2Int(0, 0);
        }
    }
    public enum TextureOrder
    {
        Col_Row = 1,
        Col_Row_Reversed,
        Row_Col,
        Row_Col_Reversed,
    }

}