using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AgentAPI
{
    public class UserAPI : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:3000/users/";

    // Create a new user
    public static IEnumerator CreateUser(string json, System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(BASE_URL, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }

    // Get all users
    public static IEnumerator GetUsers(System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(BASE_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }

    // Get a user by id
    public static IEnumerator GetUser(string id, System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(BASE_URL + id))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }

    // Update a user
    public static IEnumerator UpdateUser(string id, string json, System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(BASE_URL + id, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }

    // Delete a user
    public static IEnumerator DeleteUser(string id, System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(BASE_URL + id))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
}
}