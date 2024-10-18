/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GISTech.GISTerrainLoader
{

    public class GISVirtualTextureTerrainGenerator : MonoBehaviour
    {
        public KeyCode GenerateKey;
        public WayPoints GeoWaypoints;
        public AirplaneDemo AirPlane;
        public Text UIText;
        //Enable it to place empty gameobjects 
        public bool InstantiateGameObjects;

        private string TerrainFilePath;

        private RuntimeTerrainGenerator RuntimeGenerator;

        private GISTerrainLoaderPrefs Prefs;
        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        [HideInInspector]
        public bool TerrainGenerated;

        void Start()
        {
            TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/GISVirtualTexture/Tererro.tif";

            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
            Prefs = RuntimePrefs.Prefs;

            RuntimeGenerator = RuntimeTerrainGenerator.Get;

            RuntimeTerrainGenerator.OnFinish += OnTerrainGeneratingCompleted;

            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                StartCoroutine(GenerateTerrain(TerrainFilePath));


        }
        void Update()
        {
            if (Input.GetKeyDown(GenerateKey))
                StartCoroutine(GenerateTerrain(TerrainFilePath));

            if (TerrainGenerated)
                UIText.text = "Latitude: " + AirPlane.GetAirPlaneLatLonElevation().y + " \n" + "Longitude: " + AirPlane.GetAirPlaneLatLonElevation().x + " \n" + "Elevation:" + AirPlane.GetAirPlaneLatLonElevation().z + " m";
        }
        private IEnumerator GenerateTerrain(string TerrainPath)
        {
            yield return new WaitForSeconds(2f);

            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {

                if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
                {
                    InitializingRuntimePrefs(TerrainPath);

                    StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
                }
                else
                {
                    Debug.LogError("Terrain file null or not supported.. Try againe");
                    yield return null;
                }
            }
            else
    if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                InitializingRuntimePrefs(TerrainPath);
                StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
            }
         }
        private void InitializingRuntimePrefs(string TerrainPath)
        {
            RuntimeGenerator.enabled = true;
            Prefs.TerrainFilePath = TerrainPath;
            Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;

            //Load Real Terrain elevation values
            Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;

            Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;

            Prefs.heightmapResolution = 4097;

            Prefs.textureMode = TextureMode.WithoutTexture;


#if GISVirtualTexture
            RuntimePrefs.terrainMaterialMode = TerrainMaterialMode.GISVirtualTexture;

#else
            Debug.Log("GIS Virtual Texture Not instaled on your project");

#endif



        }

        private void OnTerrainGeneratingCompleted(GISTerrainContainer m_container)
        {
            //Convert Geo Lat/Lon Way Points to Unity World Space

            GeoWaypoints.ConvertLatLonToSpacePosition(RuntimeGenerator.GeneratedContainer, InstantiateGameObjects);

            AirPlane.OnTerrainGeneratingCompleted();

            TerrainGenerated = true;

#if GISVirtualTexture
            var RuntimeGTVPrefs = RuntimeGenerator.GeneratedContainer.GetComponent<GISVirtualTexture.GISVirtualTextureRuntimePrefs>();
            RuntimeGTVPrefs.GISFolder = @"C:\Users\GISTech\Desktop\Terrains\Huge_Terrain\GVT_21";
#else
            Debug.Log("GIS Virtual Texture Not instaled on your project");

#endif

        }
    }
}
 