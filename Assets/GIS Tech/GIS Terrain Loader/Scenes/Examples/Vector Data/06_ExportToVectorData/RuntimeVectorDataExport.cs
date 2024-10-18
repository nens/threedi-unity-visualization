using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class RuntimeVectorDataExport : MonoBehaviour
{
    public GISTerrainContainer Container;
    public string ExportFolder;
    public bool  StoreElevationValue = false;
    public RealWorldElevation ElevationMode = RealWorldElevation.Elevation;
    public ExportVectorType VectorType = ExportVectorType.Shapfile;
    public Button export_btn;
    // Start is called before the first frame update
    void Start()
    {
        export_btn.onClick.AddListener(ExportVectorData);
    }
    private void ExportVectorData()
    {
        string FolderName = Random.Range(1, 100).ToString();

        var FolderPath = Path.Combine(ExportFolder, FolderName);
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);

        var VectorFilePath = Path.Combine(FolderPath, "MyShapeFile.shp");

        //Use this API to Get Stored terrain metadata + Elevation as it was generated in Edit mode
        Container.GetStoredHeightmap();

        GISTerrainLoaderGeoVectorData m_GeoData = GISTerrainLoaderGeoVectorData.GetGeoDataFromScene(Container, CoordinatesSource.FromTerrain, StoreElevationValue, ElevationMode);

        Container.ExportVectorData(VectorType, VectorFilePath, m_GeoData, StoreElevationValue);

        Debug.Log("VectorData Exported to : " + VectorFilePath);
    }
}
