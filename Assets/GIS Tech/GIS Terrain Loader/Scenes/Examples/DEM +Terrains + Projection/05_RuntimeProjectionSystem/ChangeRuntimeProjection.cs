using System.Collections;
using UnityEngine;
using GISTech.GISTerrainLoader;
using UnityEngine.UI;
using System;

/// <summary>
/// GIS Terrain Loader Pro
///  This Script Show how to Change the coordinates system + Format for WGS84 EPSG = 4326 at runtime
/// </summary>
public class ChangeRuntimeProjection : MonoBehaviour
{
    private string TerrainFilePath;

    private RuntimeTerrainGenerator RuntimeGenerator;

    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;


    public Dropdown Projections;
    public Dropdown DisplayFormat_DropDown;
    public Text LatLonText;
    public Text ElevationText;
    public int From_EPSG = 0;
    public int To_EPSG = 0;
    private RaycastHit hitInfo;
    public LayerMask terrainLayer;

    private Terrain m_terrain;
    public Terrain terrain
    {
        get { return m_terrain; }
        set
        {
            if (m_terrain != value)
            {
                m_terrain = value;

            }
        }
    }

    void Start()
    {
        //DEM Path
        TerrainFilePath = Application.streamingAssetsPath + "/GIS Terrains/Example_SHP/Cuenca.tif";

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        Prefs = RuntimePrefs.Prefs;

        RuntimeGenerator = RuntimeTerrainGenerator.Get;

        RuntimeTerrainGenerator.OnFinish += OnTerrainGenerated;

        if (Projections)
            Projections.onValueChanged.AddListener(OnProjectionChanged);

        if (DisplayFormat_DropDown)
            DisplayFormat_DropDown.onValueChanged.AddListener(OnDisplayFormatChanged);

        //Generate Terrain
        StartCoroutine(GenerateTerrain(TerrainFilePath));


    }
    void Update()
    {
        //Get Mouse Position 
        RayCastMousePosition();
    }
    private void RayCastMousePosition()
    {
        hitInfo = new RaycastHit();

        if (Camera.main)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, terrainLayer))
            {
                if (terrain == null)
                {
                    terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                    ElevationText.text = "0";
                }


                if (terrain != null)
                {
                    if (!string.Equals(hitInfo.collider.transform.name, terrain.name))
                    {
                        terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                    }
                }

                var ReadWorldCoor = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(hitInfo.point, RuntimeGenerator.GeneratedContainer, true, RealWorldElevation.Elevation);

                if (LatLonText)
                {
                    var ConvertedCoor = ConvertCoordinatesTo(ReadWorldCoor.ToDVector2(), To_EPSG);

                    if (To_EPSG == 4326)
                        LatLonText.text = "GEO WGS84 " + GISTerrainLoaderGeoConversion.ToDisplayFormat(ConvertedCoor, Prefs.CoordinatesDisplayFormat);

                    if (To_EPSG != 4326)
                        LatLonText.text = Projections.captionText.text + " { " + ConvertedCoor.x + " / " + ConvertedCoor.y + "}";

                }

                if (ElevationText)
                    ElevationText.text = Math.Round(ReadWorldCoor.z, 2) + " m ";

            }
        }

    }
    private IEnumerator GenerateTerrain(string TerrainPath)
    {
        yield return new WaitForSeconds(2f);
 
            if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {

            InitializingRuntimePrefs(TerrainPath);
            StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
        }else
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

    }
    private void InitializingRuntimePrefs(string TerrainPath)
    {
        RuntimeGenerator.enabled = true;
        Prefs.TerrainFilePath = TerrainPath;
        Prefs.RemovePrvTerrain =  OptionEnabDisab.Enable;

        //Load Real Terrain elevation values
        Prefs.TerrainElevation = TerrainElevation.RealWorldElevation;
        Prefs.terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        Prefs.heightmapResolution = 256;
        Prefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
        Prefs.terrainMaterialMode = TerrainMaterialMode.Standard;
    }

    private void OnProjectionChanged(int value)
    {
        To_EPSG = Prefs.ProjectionsData_SO.GetEPSG(Projections.options[value].text);
    }
    private void OnDisplayFormatChanged(int value)
    {
        Prefs.CoordinatesDisplayFormat = (DisplayFormat)value;
    }
    private DVector2 ConvertCoordinatesTo(DVector2 coor, int To_epsg)
    {
        if (From_EPSG != To_epsg)
            return GISTerrainLoaderGeoConversion.ConvertCoordinatesFromTo(coor, From_EPSG, To_epsg);
        else
            return coor;
    }

    private void OnTerrainGenerated(GISTerrainContainer m_container)
    {
        //Load Runtime Projection Scriptable object 
        Projections.ClearOptions();
        //Add Projections From Runtime Projection Scriptble Object to the Dropdown Options
        Projections.AddOptions(Prefs.ProjectionsData_SO.GetOptions());

        From_EPSG = RuntimeGenerator.GeneratedContainer.data.EPSG;
        if (From_EPSG == 0) From_EPSG = 4326;

        To_EPSG = From_EPSG;
    }

    void OnDisable()
    {
        RuntimeTerrainGenerator.OnFinish -= OnTerrainGenerated;
    }
}
