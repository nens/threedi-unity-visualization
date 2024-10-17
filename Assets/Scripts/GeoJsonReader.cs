using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GeoJsonReader : MonoBehaviour
{
    [SerializeField] private TextAsset geoJsonFile; // Add the GeoJSON file in the Unity Editor

    public GameObject weirPrefab;
    public GameObject pumpPrefab;
    public Material pipeMaterial;
    public Material manholeMaterial;
    public Material leveeMaterial;
    public Material culvertMaterial;
    public Material nodeMaterial;
    public Material channelMaterial;
    public Material orificeMaterial;

    // Dictionary for 3D model types (for pumps and weirs)
    private Dictionary<string, GameObject> modelTypePrefabs;

    // Dictionary for line materials
    private Dictionary<string, Material> lineMaterials;

    private CameraFollow cameraFollow; // Reference to the CameraFollow script

    void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>(); // Get the CameraFollow component

        // Initialize the dictionary for 3D objects (only for Pumps and Weirs)
        modelTypePrefabs = new Dictionary<string, GameObject>
        {
            { "Weirs", weirPrefab },
            { "Pumps", pumpPrefab }
        };

        // Initialize the dictionary for line materials
        lineMaterials = new Dictionary<string, Material>
        {
            { "Pipes", pipeMaterial },
            { "Manholes", manholeMaterial },
            { "Levees", leveeMaterial },
            { "Culverts", culvertMaterial },
            { "Nodes", nodeMaterial },
            { "Channels", channelMaterial },
            { "Orifices", orificeMaterial }
        };

        // Parse the GeoJSON data
        JObject geoJson = JObject.Parse(geoJsonFile.text);

        Transform firstObjectTransform = null; // Track the first instantiated object

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

                // Handle weirs as 3D objects instead of lines
                if (modelType == "Weirs")
                {
                    // Instantiate the weir model at the midpoint of the coordinates
                    Vector3 midpoint = (positions[0] + positions[positions.Count - 1]) / 2;
                    firstObjectTransform = Create3DObject(midpoint, modelType);
                }
                else
                {
                    // Create a line for all non-3D model types
                    CreateLine(positions.ToArray(), modelType);
                }
            }
            else if (geometryType == "Point")
            {
                // Get the coordinates for Point
                var coordinates = feature["geometry"]["coordinates"];
                float longitude = (float)coordinates[0];
                float latitude = (float)coordinates[1];
                Vector3 worldPos = ConvertGeoCoordsToUnity(longitude, latitude);

                // Only instantiate 3D objects for Pumps if type is Point
                if (modelType == "Pumps")
                {
                    firstObjectTransform = Create3DObject(worldPos, modelType);
                }
            }
            else
            {
                Debug.LogWarning("Unhandled geometry type: " + geometryType);
            }
        }

        // Set the camera target to the first instantiated object
        if (firstObjectTransform != null && cameraFollow != null)
        {
            cameraFollow.target = firstObjectTransform; // Set the camera target to the first object
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

    // Method to create a 3D object (Pump or Weir)
    private Transform Create3DObject(Vector3 position, string modelType)
    {
        // Check if the model type exists in the dictionary
        if (modelTypePrefabs.ContainsKey(modelType))
        {
            GameObject prefab = modelTypePrefabs[modelType];

            // Check if the prefab is assigned
            if (prefab != null)
            {
                // Instantiate the corresponding prefab at the given position
                GameObject instance = Instantiate(prefab, position, Quaternion.identity);
                return instance.transform; // Return the transform of the instantiated object
            }
            else
            {
                Debug.LogError($"Prefab for {modelType} is not assigned in the Inspector!");
            }
        }
        else
        {
            Debug.LogWarning("Model type not recognized: " + modelType);
        }

        return null; // Return null if instantiation fails
    }

    // Method to create a line between points for LineString geometries
    private void CreateLine(Vector3[] positions, string modelType)
    {
        // Only create lines for non-3D objects
        if (lineMaterials.ContainsKey(modelType))
        {
            GameObject lineObj = new GameObject(modelType + "_Line");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            // Assign the material for the line based on model type
            lineRenderer.material = lineMaterials[modelType];

            // Set the positions for the line renderer
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);

            // Adjust the line width if needed
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
        else
        {
            // Log a warning for model types that don't have materials assigned
            Debug.LogWarning("No line material found for model type: " + modelType);
        }
    }
}
