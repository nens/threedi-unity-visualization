/*     Unity GIS Tech 2020-2023      */

using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderVectorParameters_SO : ScriptableObject
    {
        [Header("Filter Vector Data (Attributes)")]
        public List<string> Attributes_Points = new List<string>();
        public List<string> Attributes_Grass = new List<string>();
        public List<string> Attributes_Trees = new List<string>();
        public List<string> Attributes_Buildings = new List<string>();
        public List<string> Attributes_Roads = new List<string>();
        public List<string> Attributes_Water = new List<string>();
        public List<string> Attributes_LandParcel = new List<string>();
        [Space(10)]
        [Header("Customized Attributes")]
        public string ID_Tag = "id";
        public string Layer_Tag = "LAYER";
        public string Name_Tag = "name";
        public string Null_Tag = "None";

        [Space(10)]
        [Header("Buildings Tags")]
        public string building_Levels_Tga = "building:levels";
        public string building_MinLevel_Tga = "building:min_level";
        public string building_Height_Tga = "building:height";
        public string building_MinHeight_Tga = "building:min_height";
        public string building_MaxHeight_Tga = "building:max_height";
        [Space(10)]
        [Header("Default Building Values")]
        public float DefaultHeightScale = 5f;
        //public static float DefaultWallWidth = 0.0001f;
        //public static int firstIndexRandomRoof = 0;
        //public static int endIndexRandomRoof = 4;
        //public static int firstIndexRandomShop = 0;
        //public static int endIndexRandomShop = 0;
        //public static int firstIndexRandomBuilding = 0;
        //public static int endIndexRandomBuilding = 3;


        [Space(10)]
        [Header("Vector Options")]
        [Tooltip("Enable This Option to add  VectorDataBase Component to gameobjects generated from vector data, this component will include all geo data (Tag, Layer, Coordinates, Database ... etc)")]
        public bool AddVectorDataBaseToGameObjects = false;
        [Tooltip("Enable This Option to Unload All Vector Data with coordinates out of the container bounds")]
        public bool LoadDataOutOfContainerBounds = false;
        [Space(5)]
        [Header("GPX")]
        [Tooltip("Enable This Option to add a Random ID for each GPX Vector Object")]
        public bool AddRandom_ID_Vector_GPX = true;
        public string GPX_GeoPoint_Attribute = "GPX_Point";
        public string GPX_GeoPoint_Value = "GPX_GeoPoint_Model";
        public string GPX_GeoLine_Attribute = "GPX_Line";
        public string GPX_GeoLine_Value = "GPX_GeoLine_Model";
        [Space(5)]
        [Header("KML")]
        [Tooltip("Enable This Option to add a Random ID for each KML Vector Object")]
        public bool AddRandom_ID_Vector_KML = true;
        public string KML_GeoPoint_Attribute = "KML_Point";
        public string KML_GeoPoint_Value = "KML_GeoPoint_Model";
        public string KML_GeoLine_Attribute = "KML_Line";
        public string KML_GeoLine_Value = "KML_GeoLine_Model";
        [Space(5)]
        [Header("GEOJSON")]
        public string GEOJSON_GeoPoint_Attribute = "GEOJSON_Point";
        public string GEOJSON_GeoPoint_Value = "GEOJSON_GeoPoint_Model";
        public string GEOJSON_GeoLine_Attribute = "GEOJSON_Line";
        public string GEOJSON_GeoLine_Value = "GEOJSON_GeoLine_Model";
        public string GEOJSON_GeoPolygon_Attribute = "GEOJSON_Polygon";
        public string GEOJSON_GeoPolygon_Value = "GEOJSON_GeoPolygon_Model";
        //[Space(5)]
        //[Header("SHP")]
        //[Tooltip("Enable This Option to add a Random ID for each SHP Vector Object")]
 



        public static GISTerrainLoaderVectorParameters_SO LoadParameters()
        {
            GISTerrainLoaderVectorParameters_SO VectorParameters = Resources.Load("Settings/GISTerrainLoaderVectorParameters") as GISTerrainLoaderVectorParameters_SO;

            if (!VectorParameters)
            {
                Debug.LogError("Vector Parameters ScriptableObject File Not found ...");
                return null;
            }
            return VectorParameters;
        }
    }
}
