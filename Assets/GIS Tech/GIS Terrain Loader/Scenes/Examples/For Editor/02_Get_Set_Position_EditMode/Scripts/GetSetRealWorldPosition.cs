using GISTech.GISTerrainLoader;

using UnityEngine;

[ExecuteInEditMode]
public class GetSetRealWorldPosition : MonoBehaviour
{
    public TerrainSourceMode terrainSourceMode = TerrainSourceMode.FromEditMode;
    // Load Geo-Data and Get Lat-Lon from Unity World Space
    public GameObject Player;
    //Get Elevation Mode (Check docs)
    public RealWorldElevation GetElevationMode = RealWorldElevation.Altitude;
 
    private GISTerrainContainer container;

    [Space(10)]
    // Set GameObject Position by converting Lat-Lon Position to Unity World Space Positon 
    public DVector2 Coordinates = new DVector2(2.72289517819244, 33.8634254169107);
    public SetElevationMode SetElevationMode = SetElevationMode.RelativeToSeaLevel;
    public float StartElevation = 20;
    private void Start()
    {
        //Get Terrain if it was generated in edit mode
        if (terrainSourceMode == TerrainSourceMode.FromEditMode)
        {
            GetTerrainData();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            var RWPosition = GetObjectLatLonElevation();
            Debug.Log("Lat/Lon: " + RWPosition.x + " " + RWPosition.y + "  Elevation :" + RWPosition.z);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Player.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, Coordinates, StartElevation, SetElevationMode);
        }
    }
    // Start is called before the first frame update
    public void GetTerrainData()
    {
        container = GameObject.FindObjectOfType<GISTerrainContainer>();

        if (container)
        {
            //Click on that button if the terrain do not dispose any Geo-Ref data
            container.GetStoredHeightmap();
        }
    }

    public void GetPosition()
    {
        var RWPosition = GetObjectLatLonElevation();
        Debug.Log("Lat/Lon: " + RWPosition.x + " " + RWPosition.y + "  Elevation :" + RWPosition.z);
    }
    public void SetPosition()
    {
        container = GameObject.FindObjectOfType<GISTerrainContainer>();

        if (container)
            Player.transform.position = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, Coordinates, StartElevation, SetElevationMode);
 
    }
    /// <summary>
    /// Get the real world Elevation of an object
    /// </summary>
    /// <returns></returns>
    public DVector3 GetObjectLatLonElevation()
    {
        container = GameObject.FindObjectOfType<GISTerrainContainer>();
        return GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(Player.transform.position, container, true, GetElevationMode);
    }

   

}
