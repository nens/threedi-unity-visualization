/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GISTech.GISTerrainLoader;

public class GeoRef : MonoBehaviour
{
    public Dropdown Projections;

    public Dropdown DisplayFormat_DropDown;

    public Text LatLonText;

    public Text ElevationText;

    // Edit Mode to Get Geo-Ref Data From Terrain already generated in Edit mode
    public TerrainSourceMode terrainSourceMode = TerrainSourceMode.FromPlayMode;

    private GISTerrainLoaderPrefs Prefs;
    private GISTerrainLoaderRuntimePrefs RuntimePrefs;

    private RuntimeTerrainGenerator runtimeGenerator;

    public LayerMask TerrainLayer;

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

    private RaycastHit hitInfo;

    public int From_EPSG = 0;

    public int To_EPSG = 0;


    void Start()
    {
        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;
        Prefs = RuntimePrefs.Prefs;

        runtimeGenerator = RuntimeTerrainGenerator.Get;
        RuntimeTerrainGenerator.OnFinish += OnTerrainGenerated;

        if (Projections)
            Projections.onValueChanged.AddListener(OnProjectionChanged);

        if (DisplayFormat_DropDown)
            DisplayFormat_DropDown.onValueChanged.AddListener(OnDisplayFormatChanged);

        if (terrainSourceMode == TerrainSourceMode.FromEditMode)
        {
            var terrain = GameObject.FindObjectOfType<GISTerrainContainer>();

            if (terrain)
            {
                terrain.GetStoredHeightmap();
                RuntimeTerrainGenerator.Get.SetGeneratedTerrain(terrain);
            }
            else
            {
                terrainSourceMode = TerrainSourceMode.FromPlayMode;
                Debug.LogError("Terrain not found in the Current scene, generate a terrain in the editor mode or set the'Origin Projection Mode' to PlayMode ");
            }


        }

    }


    void Update()
    {
        RayCastMousePosition();
    }

    private void RayCastMousePosition()
    {
        hitInfo = new RaycastHit();

        if (Camera.main)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, TerrainLayer))
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

                var ReadWorldCoor = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(hitInfo.point, runtimeGenerator.GeneratedContainer, true, RealWorldElevation.Elevation);

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

    private DVector2 ConvertCoordinatesTo(DVector2 coor, int To_epsg)
    {
        if (From_EPSG != To_epsg)
            return GISTerrainLoaderGeoConversion.ConvertCoordinatesFromTo(coor, From_EPSG, To_epsg);
        else
            return coor;
    }
    private void OnProjectionChanged(int value)
    {
        To_EPSG = Prefs.ProjectionsData_SO.GetEPSG(Projections.options[value].text);
    }
    private void OnDisplayFormatChanged(int value)
    {
        Prefs.CoordinatesDisplayFormat = (DisplayFormat)value;
    }

    private void OnTerrainGenerated(GISTerrainContainer m_container)
    {
        //Load Runtime Projection Scriptable object 
        //Add The Inject the Terrain Projection into the Runtime Projection Scriptable object  
        Projections.ClearOptions();
        //Add Projections From Runtime Projection Scriptble Object to the Dropdown Options
        Projections.AddOptions(Prefs.ProjectionsData_SO.GetOptions());


        From_EPSG = runtimeGenerator.GeneratedContainer.data.EPSG;
        if (From_EPSG == 0) From_EPSG = 4326;

        To_EPSG = From_EPSG;
    }

    void OnDisable()
    {
        RuntimeTerrainGenerator.OnFinish -= OnTerrainGenerated;
    }

}