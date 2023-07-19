using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Caller : MonoBehaviour
{
    void Start()
    {
        //StartCoroutine(CreateUser());
    }

    IEnumerator CreateUser()
    {
        // Define the user data to send in the request body
        Dictionary<string, string> userData = new Dictionary<string, string>();
        userData.Add("userID", "444");
        userData.Add("SelectedMarkerIndex", "0");
        userData.Add("NeighborKeywords", "keyword1,keyword2");
        userData.Add("MyKeywords", "keyword1,keyword2");
        userData.Add("NewKeywords", "");
        userData.Add("CurrentStage", "1");
        userData.Add("MyArticle", "");
        userData.Add("NeighborArticles", "");

        // Convert the user data to JSON format
        string jsonData = JsonUtility.ToJson(userData);

        // Create a UnityWebRequest object to send a POST request to the router
        UnityWebRequest request = UnityWebRequest.PostWwwForm("http://localhost:3000/users/userID/555", jsonData);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        // Check if there was an error with the request
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Get the response body as a string
            string responseBody = request.downloadHandler.text;
            Debug.Log("User created with");
        }
    }
}