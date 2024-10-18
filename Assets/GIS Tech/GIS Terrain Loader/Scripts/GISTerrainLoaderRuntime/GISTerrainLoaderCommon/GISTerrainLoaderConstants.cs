/*     Unity GIS Tech 2020-2023      */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderConstants  
    {

        /// <summary> Main GIS Folder. </summary>
        public const string MainGISFolder = "/GIS Tech/GIS Terrain Loader";

        /// <summary> Water Folder. </summary>
        public const string WaterPrefabFolder = "Prefabs/Environment/Water/";
        /// <summary> Grass Folder. </summary>
        public const string GrassPrefabFolder = "Prefabs/Environment/Grass/";
        /// <summary> Trees Folder. </summary>
        public const string TreesPrefabFolder = "Prefabs/Environment/Trees/";
        /// <summary> Roads Folder. </summary>
        public const string RoadsPrefabFolder = "Prefabs/Environment/Roads/";
        /// <summary> GeoPoints Folder. </summary>
        public const string GeoPointsPrefabFolder = "Prefabs/Environment/GeoPoints/";
        /// <summary> UV Building Textures Folder. </summary>
        public const string BuildingPrefabFolder = "Prefabs/Environment/Buildings/";
        /// <summary> LandParcel Folder. </summary>
        public const string LandParcelPrefabFolder = "Prefabs/Environment/LandParcel/";

        /// <summary> UV Building Textures Folder. </summary>
        public const string UVBuildingTextureFolder = "/Resources/Prefabs/Environment/Buildings/Textures";
        /// <summary> UV Default Building Material. </summary>
        public const string DefaultWallMaterial = "Environment/Buildings/Default/Materials/DefaultWallMaterial";
        public const string DefaultRoofMaterial = "Environment/Buildings/Default/Materials/DefaultRoofMaterial";
        public const string DefaultBasementMaterial = "Environment/Buildings/Default/Materials/DefaultBasementMaterial";

        public const string DefaultWallTexture = "Environment/Buildings/Default/Textures/UV_DefaultBuildingWall";
        public const string DefaultRoofTexture = "Environment/Buildings/Default/Textures/UV_DefaultBuildingRoof";
        public const string DefaultBasementTexture = "Environment/Buildings/Default/Textures/UV_DefaultBuildingBasement";

        /// <summary> Default Water Material. </summary>
        public const string DefaultWaterMaterial = "Environment/Water/Default/Materials/DefaultWaterMaterial";
        public const string DefaultLandParcelMaterial = "Environment/LandParcel/Default/Materials/DefaultLandParcel";

        public const string DefaultBuildingSO = "Prefabs/Environment/Buildings/DefaultBuilding";
        public const string DefaultWaterSO = "Prefabs/Environment/Water/DefaultWater";
        public const string DefaultLandParcelSO = "Prefabs/Environment/LandParcel/DefaultLandParcel";

        public static readonly Vector3 Vector3Right = new Vector3(1, 0, 0);
        public static readonly Vector3 Vector3Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 Vector3Up = new Vector3(0, 1, 0);
        public static readonly Vector3 Vector3Down = new Vector3(0, -1, 0);
        public static readonly Vector3 Vector3One = new Vector3(1, 1, 1);
        public static readonly Vector3 Vector3Forward = new Vector3(0, 0, 1);
        public static readonly Vector3 Vector3Unused = new Vector3(float.MinValue, float.MinValue, float.MinValue);


        #region ShadersData
        public static readonly float LineWidth = 0.017f;
        public static readonly float Brightness = 0.518f;
        public static readonly float Contrast = 0.982f;

        #endregion
    }
}