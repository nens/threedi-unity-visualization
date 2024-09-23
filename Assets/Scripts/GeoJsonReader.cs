using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GeoJsonReader : MonoBehaviour
{
    [SerializeField] private TextAsset geoJsonFile; // Voeg het GeoJSON-bestand toe in de Unity-editor
    public Material lineMaterial; // Voeg een materiaal toe voor de lijnen

    void Start()
    {
        // Parse the GeoJSON data
        JObject geoJson = JObject.Parse(geoJsonFile.text);

        // Loop through the features
        foreach (var feature in geoJson["features"])
        {
            // Get the coordinates array
            var coordinates = feature["geometry"]["coordinates"];

            List<Vector3> linePoints = new List<Vector3>();

            // Convert GeoJSON coordinates to Unity world coordinates
            foreach (var coordinate in coordinates)
            {
                float longitude = (float)coordinate[0];
                float latitude = (float)coordinate[1];

                // Convert geographical coordinates to Unity world position
                Vector3 worldPos = ConvertGeoCoordsToUnity(longitude, latitude);
                linePoints.Add(worldPos);
            }

            // Create a new GameObject with a LineRenderer for each feature
            CreateLine(linePoints.ToArray());
        }
    }

    private Vector3 ConvertGeoCoordsToUnity(float longitude, float latitude)
    {
        // Dit is een vereenvoudigde conversie. Voor preciezere conversies kun je een
        // bibliotheek voor conversie tussen geografische coördinaten en kaartprojecties gebruiken.
        float x = longitude * 1000f; // Schalen of aanpassen voor jouw project
        float z = latitude * 1000f;  // Schalen of aanpassen voor jouw project
        float y = 0f; // Dit is vlak op de grond. Voeg eventueel hoogte toe.

        return new Vector3(x, y, z);
    }

    private void CreateLine(Vector3[] points)
    {
        GameObject lineObj = new GameObject("GeoLine");
        lineObj.transform.parent = GameObject.Find("LineContainer").transform; // Maak het kind van LineContainer
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

}
