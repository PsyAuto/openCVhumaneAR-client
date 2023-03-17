using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
//using Newtonsoft.Json;

public class APIClient : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:3000/users";

    public void CreateUser(string name, int age)
    {
        StartCoroutine(CreateUserRoutine(name, age));
    }

    private IEnumerator CreateUserRoutine(string name, int age)
    {
        var userData = new { name = name, age = age };
        var jsonData = name;//JsonConvert.SerializeObject(userData);
        var body = new System.Text.UTF8Encoding().GetBytes(jsonData);
        var request = new UnityWebRequest(BASE_URL, "POST");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("User created successfully: " + request.downloadHandler.text);
        }
    }
}
