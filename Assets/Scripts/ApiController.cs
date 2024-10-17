using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro; // Add this for TextMeshPro components



public class ThreediModel
{
    public ExtentTwoD extent_two_d { get; set; }
}

public class ExtentTwoD
{
    public string type { get; set; }
    public List<List<double>> coordinates { get; set; }
}

public class ApiController : MonoBehaviour
{
    private string apiKey = "nF2sPztE.t8q48ibGKWZQ4XhErnTEYbAbjJkoU7EW";
    private const string baseUrl = "https://api.3di.live/v3/threedimodels/"; // Base URL without the model ID

    public MapController mapController; // Reference to your MapController
    public TMP_InputField codeInputField; // Reference to the TMP input field for user code
    public Button submitButton; // Reference to the button for submission

    void Start()
    {
        // Check if the mapController is assigned
        if (mapController == null)
        {
            Debug.LogError("MapController reference is not set in ApiController!");
            return;
        }

        // Set up button listener
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    void OnSubmitButtonClicked()
    {
        // Get the user input code
        string userCode = codeInputField.text.Trim(); // Remove any leading/trailing whitespace

        // Ensure userCode is not empty
        if (string.IsNullOrEmpty(userCode))
        {
            Debug.LogWarning("Input code is empty!");
            return;
        }

        // Create the full URL using the user code
        string url = baseUrl + userCode + "/";

        // Start the coroutine to fetch data
        StartCoroutine(GetRequest(url));

        // Clear the input field after submission
        codeInputField.text = "";
    }

    IEnumerator GetRequest(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Create a Basic Authentication header
        string username = "__key__";
        string password = apiKey;
        string auth = username + ":" + password;
        string authHeader = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(auth));

        request.SetRequestHeader("Authorization", authHeader);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            var model = JsonConvert.DeserializeObject<ThreediModel>(responseText);

            // Check if the model is valid
            if (model?.extent_two_d?.coordinates != null && model.extent_two_d.coordinates.Count > 0)
            {
                // Calculate the center point of the coordinates
                Vector2 centerPoint = CalculateCenterPoint(model.extent_two_d.coordinates);
                Debug.Log("Center Point - Longitude: " + centerPoint.x + ", Latitude: " + centerPoint.y);

                // Call the MapController to set coordinates
                mapController.SetCoordinates(centerPoint.y, centerPoint.x);
            }
            else
            {
                Debug.LogWarning("Invalid model data received!");
            }
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }
    }

    private Vector2 CalculateCenterPoint(List<List<double>> coordinates)
    {
        double sumLongitude = 0;
        double sumLatitude = 0;
        int count = coordinates.Count;

        for (int i = 0; i < count; i++)
        {
            sumLongitude += coordinates[i][0]; // Longitude
            sumLatitude += coordinates[i][1];   // Latitude
        }

        double centerLongitude = sumLongitude / count;
        double centerLatitude = sumLatitude / count;

        return new Vector2((float)centerLongitude, (float)centerLatitude);
    }
}
