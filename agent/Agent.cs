using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Agent
{
    public class Agent : MonoBehaviour
    {
        private bool isConnected = false;

        public string url = "http://localhost:3000";
        public bool debug = true;

        public int CurrentStage { get; private set; }
        public string MyKeywords { get; private set; }
        public string NeighborKeywords { get; private set; }
        public int SelectedMarkerIndex { get; private set; }
        public int PlayerID { get; private set; }

        public DebugText debugText;

        private void Start()
        {
            // Make the Agent object persist across all scenes
            DontDestroyOnLoad(gameObject);
            debugText = GameObject.Find("DebugText").GetComponent<DebugText>();
            Connect();
        }

        private void OnDestroy()
        {
            Disconnect();
            debugText.Clear();
        }

        private void Connect()
        {
            if (!isConnected)
            {
                StartCoroutine(ConnectCoroutine());
            }
        }

        private IEnumerator ConnectCoroutine()
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                isConnected = true;
                debugText.Log("Agent connected to " + url);
                Debug.Log("debugText: " + debugText);
                StartCoroutine(ReceiveCoroutine());
            }
            else
            {
                debugText.Log("Agent failed to connect to " + url + ": " + www.error);
            }
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                StopAllCoroutines();
                isConnected = false;
                debugText.Log("Agent disconnected from " + url);
            }
        }

        private IEnumerator ReceiveCoroutine()
        {
            while (isConnected)
            {
                UnityWebRequest www = UnityWebRequest.Get(url + "/update");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string response = www.downloadHandler.text;
                    Dictionary<string, string> data = JsonUtility.FromJson<Dictionary<string, string>>(response);
                    if (data.ContainsKey("CurrentStage"))
                    {
                        CurrentStage = int.Parse(data["CurrentStage"]);
                    }
                    if (data.ContainsKey("MyKeywords"))
                    {
                        MyKeywords = data["MyKeywords"];
                    }
                    if (data.ContainsKey("NeighborKeywords"))
                    {
                        NeighborKeywords = data["NeighborKeywords"];
                    }
                    if (data.ContainsKey("SelectedMarkerIndex"))
                    {
                        SelectedMarkerIndex = int.Parse(data["SelectedMarkerIndex"]);
                    }
                    if (data.ContainsKey("PlayerID"))
                    {
                        PlayerID = int.Parse(data["PlayerID"]);
                    }
                }
                else
                {
                    debugText.Log("Agent failed to receive update from " + url + ": " + www.error);
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        public void SendUpdate(int currentStage, string myKeywords, string neighborKeywords, int selectedMarkerIndex, int playerID)
        {
            if (isConnected)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["CurrentStage"] = currentStage.ToString();
                data["MyKeywords"] = myKeywords;
                data["NeighborKeywords"] = neighborKeywords;
                data["SelectedMarkerIndex"] = selectedMarkerIndex.ToString();
                data["PlayerID"] = playerID.ToString();
                string json = JsonUtility.ToJson(data);
                StartCoroutine(SendCoroutine(json));
            }
        }

        private IEnumerator SendCoroutine(string json)
        {
            UnityWebRequest www = UnityWebRequest.PostWwwForm(url + "/update", json);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                debugText.Log("Agent sent update to " + url);
            }
            else
            {
                debugText.Log("Agent failed to send update to " + url + ": " + www.error);
            }
        }
    }
}