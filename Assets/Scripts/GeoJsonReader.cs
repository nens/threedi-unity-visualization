using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GeoJsonReader : MonoBehaviour
{
    [SerializeField] private TextAsset geoJsonFile; // Add the GeoJSON file in the Unity Editor

    // Prefabs for each model type
    public GameObject pipePrefab;
    public GameObject weirPrefab;
    public GameObject pumpPrefab;
    public GameObject manholePrefab;
    public GameObject leveePrefab;
    public GameObject culvertPrefab;  
    public GameObject nodePrefab;      
    public GameObject channelPrefab;   
    public GameObject orificePrefab;   

    // Dictionary to map model types to prefabs
    private Dictionary<string, GameObject> modelTypePrefabs;

    void Start()
    {
        // Initialize the dictionary that maps model types to prefabs
        modelTypePrefabs = new Dictionary<string, GameObject>
        {
            { "Pipes", pipePrefab },
            { "Weirs", weirPrefab },
            { "Pumps", pumpPrefab },
            { "Manholes", manholePrefab },
            { "Levees", leveePrefab },
            { "Culverts", culvertPrefab },  
            { "Nodes", nodePrefab },          
            { "Channels", channelPrefab },     
            { "Orifices", orificePrefab }      
        };

        // Parse the GeoJSON data
        JObject geoJson = JObject.Parse(geoJsonFile.text);

        // Loop through the features
        foreach (var feature in geoJson["features"])
        {
            // Get the geometry type
            string geometryType = feature["geometry"]["type"].ToString();

            // Get the model type from the properties
            string modelType = feature["properties"]["model_type"].ToString();

            // Handle different geometry types
            if (geometryType == "LineString")
            {
                // Get the coordinates array for LineString
                var coordinates = feature["geometry"]["coordinates"];
                List<Vector3> positions = new List<Vector3>();

                // Convert each coordinate to Unity position
                foreach (var coordinate in coordinates)
                {
                    float longitude = (float)coordinate[0];
                    float latitude = (float)coordinate[1];
                    Vector3 worldPos = ConvertGeoCoordsToUnity(longitude, latitude);
                    positions.Add(worldPos);
                }

                // Instantiate a line segment for Pipes, Culverts, Channels, Orifices, or other line-based models
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    CreateLineSegment(positions[i], positions[i + 1], modelType);
                }
            }
            else if (geometryType == "Point")
            {
                // Get the coordinates for Point
                var coordinates = feature["geometry"]["coordinates"];
                float longitude = (float)coordinates[0];
                float latitude = (float)coordinates[1];
                Vector3 worldPos = ConvertGeoCoordsToUnity(longitude, latitude);
                
                // Instantiate the icon at the point
                CreateIcon(worldPos, modelType);
            }
            else
            {
                Debug.LogWarning("Unhandled geometry type: " + geometryType);
            }
        }
    }

    // Method to convert geographical coordinates to Unity world coordinates
    private Vector3 ConvertGeoCoordsToUnity(float longitude, float latitude)
    {
        // A simplified conversion. Adjust scaling for your project.
        float x = longitude * 1000f; // Scale factor
        float z = latitude * 1000f;   // Scale factor
        float y = 0f; // This is flat on the ground. Add height if necessary.

        return new Vector3(x, y, z);
    }

    // Method to create an icon based on the model type
    private void CreateIcon(Vector3 position, string modelType)
    {
        // Check if the model type exists in the dictionary
        if (modelTypePrefabs.ContainsKey(modelType))
        {
            // Instantiate the corresponding prefab at the given position
            Instantiate(modelTypePrefabs[modelType], position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Model type not recognized: " + modelType);
        }
    }

    // Method to create a line segment between two points for LineString geometries
    private void CreateLineSegment(Vector3 start, Vector3 end, string modelType)
    {
        // Instantiate a prefab only at the start position
        CreateIcon(start, modelType);
    }
}
