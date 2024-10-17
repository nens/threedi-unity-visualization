using UnityEngine;
using Mapbox.Utils; // For Vector2d
using Mapbox.Unity.Map; // For AbstractMap
using System.Collections; // For Coroutine

public class MapController : MonoBehaviour
{
    private float latitude;
    private float longitude;
    public GameObject mapPrefab; // Reference to the map prefab
    private GameObject currentMapInstance; // Current map instance

    public void SetCoordinates(float lat, float lon)
    {
        latitude = lat;
        longitude = lon;
        Debug.Log("Map coordinates updated - Latitude: " + latitude + ", Longitude: " + longitude);

        // Start the coroutine to update the map
        StartCoroutine(UpdateMapAfterCoordinatesSet());
    }

    private IEnumerator UpdateMapAfterCoordinatesSet()
    {
        // Wait for a frame
        yield return null;

        UpdateMap(); // Update the map
    }

    private void UpdateMap()
    {
        Debug.Log("UpdateMap called.");

        // Check if the map instance already exists
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            Debug.Log("Previous map instance destroyed.");
        }

        // Instantiate a new map at the specified coordinates
        currentMapInstance = Instantiate(mapPrefab);

        // Set the new map's center coordinates
        if (currentMapInstance.TryGetComponent<AbstractMap>(out var abstractMap))
        {
            Vector2d newCoordinates = new Vector2d(latitude, longitude);
            abstractMap.SetCenterLatitudeLongitude(newCoordinates);
            Debug.Log($"Setting new coordinates - Latitude: {newCoordinates.x}, Longitude: {newCoordinates.y}");

            // Update the map (optional, but you can do it without zoom check)
            abstractMap.UpdateMap(newCoordinates, abstractMap.Zoom);
            Debug.Log($"New map instance created at Latitude: {latitude}, Longitude: {longitude}");
        }
        else
        {
            Debug.LogWarning("The instantiated map prefab does not have an AbstractMap component!");
        }
    }
}
