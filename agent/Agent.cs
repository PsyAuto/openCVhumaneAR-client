using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

namespace AgentAPI
{
    public class UserAPI : MonoBehaviour
    {
        public string currentStage;
        public TextMeshProUGUI textComponent;
        public TMP_InputField urlInputField;
        public string BASE_URL = "http://192.168.2.3:3000/";
        private string DEFAULT_URL = "http://192.168.2.3:3000/";
        private const float RECONNECT_DELAY = 2f;
        private SocketIOUnity socket;

        private void Start()
        {
            // Make the UserAPI object persist between scenes
            DontDestroyOnLoad(gameObject);

            // Set the BASE_URL variable using the input field value
            if (urlInputField != null && !string.IsNullOrEmpty(urlInputField.text))
            {
                BASE_URL = urlInputField.text;
            }

            // Add an event listener to the input field to detect changes
            if (urlInputField != null)
            {
                urlInputField.onValueChanged.AddListener(OnUrlInputValueChanged);
            }
        }

        // Event listener for the input field value changed event
        private void OnUrlInputValueChanged(string value)
        {
            BASE_URL = string.IsNullOrEmpty(urlInputField.text) ? DEFAULT_URL : urlInputField.text;
        }

        public void OnConnectButtonClicked()
        {
            StartCoroutine(ConnectToServer());
            
            // Find the ShowARMarkerButton GameObject and make it interactable
            GameObject showARMarkerButtonGO = GameObject.Find("ShowARMarkerButton");
            if (showARMarkerButtonGO != null)
            {
                Button showARMarkerButton = showARMarkerButtonGO.GetComponent<Button>();
                if (showARMarkerButton != null)
                {
                    showARMarkerButton.interactable = true;
                }
            }
        }

        public IEnumerator ConnectToServer() {
            var uri = new Uri (BASE_URL+"socketio/");
            socket = new SocketIOUnity(uri);

            socket = new SocketIOUnity(uri, new SocketIOOptions
                {
                    Query = new Dictionary<string, string>
                        {
                            {"token", "UNITY" }
                        }
                    ,
                    EIO = 4
                    ,
                    Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
                });
            socket.JsonSerializer = new NewtonsoftJsonSerializer();


            ///// reserved socketio events
            socket.OnConnected += (sender, e) =>
            {
                
            };
            socket.OnDisconnected += (sender, e) =>
            {
                DebugText.LogToText("disconnect: " + e, textComponent);
            };
            socket.OnReconnectAttempt += (sender, e) =>
            {
                //DebugText.LogToText($"Reconnecting: attempt = {e}", textComponent);
            };
            socket.OnError += (sender, e) =>
            {
                DebugText.LogToText("error: " + e, textComponent);
            };
            ////
            socket.Connect();
            //await socket.ConnectAsync();
            // wait for socket connection to be established
            yield return new WaitUntil(() => socket.Connected);
            DebugText.LogToText("connected: "+socket.Connected, textComponent);

            // create local empty user
            Dictionary<string, object> user = CreateUser();

            // create local empty users
            Dictionary<string, object> users = CreateUsers();

            GetAllUsers();
    
            // call user create event
            socket.Emit("createSocketID", user);
        }

        private IEnumerator EventHandler(){
            // wait for socket connection to be established
            yield return new WaitUntil(() => socket.Connected);
            DebugText.LogToText("Socket.IO connected", textComponent);

            // create local user based on his unique socket id
            Dictionary<string, object> user = CreateUser();

            // call user create event
            socket.Emit("createSocketID", user);
        }

        private Dictionary<string, object> CreateUser()
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            userData.Add("userID", "");
            userData.Add("socketID", "");
            userData.Add("SelectedMarkerIndex", -1);
            userData.Add("NeighborKeywords", new List<string>());
            userData.Add("MyKeywords", new List<string>());
            userData.Add("NewKeywords", new List<string>());
            userData.Add("CurrentStage", 0);
            userData.Add("MyArticle", "");
            userData.Add("NeighborArticles", new List<string>());
            return userData;
        }

        private Dictionary<string, object> CreateUsers()
        {
            Dictionary<string, object> usersData = new Dictionary<string, object>();
            usersData.Add("users", new List<Dictionary<string, object>>());
            return usersData;
        }

        public void GetCurrentStage(Action<string> callback)
        {
            socket.Emit("getCurrentStage");
            socket.On("currentStage", (response) =>
            {
                string currentStage = response.GetValue<string>();
                callback(currentStage);
            });
        }

        public void GetRadius(Action<string> callback)
        {
            socket.Emit("getRadius");
            socket.On("radius", (response) =>
            {
                string radius = response.GetValue<string>();
                callback(radius);
            });
        }

        public void GetAllUsers() {
            socket.Emit("getUsers");
            socket.On("users", (data) => {
                string jsonArray = data.ToString();
                Debug.Log(jsonArray);

                UsersData usersData = data.GetValue<UsersData>();
                //UsersData usersData = JsonConvert.DeserializeObject<UsersData>(jsonArray);
                Debug.Log("usersData: "+usersData);
            });
        }

        // get a list of the userID of all users
        public void GetAllUserIDs(Action<string> callback)
        {
            socket.Emit("getUserIDs");
            socket.On("userIDs", (response) =>
            {
                string jsonArray = response.ToString();
                callback(jsonArray);
            });
        }

        public void GetUserByID(string userID, Action<UserData> callback)
        {
            socket.Emit("getUserByID", userID);
            socket.On("userByID", (response) =>
            {
                string jsonArray = response.ToString();
                Debug.Log(jsonArray);

                UserData userData = response.GetValue<UserData>();
                //UserData userData = JsonConvert.DeserializeObject<UserData>(jsonArray);
                Debug.Log("userData: "+userData);
                callback(userData);
            });
        }

        [System.Serializable]
        public class UserData
        {
            public string _id;
            public string userID;
            public string socketID;
            public int SelectedMarkerIndex;
            public List<string> NeighborKeywords;
            public List<string> MyKeywords;
            public List<string> NewKeywords;
            public int CurrentStage;
            public string MyArticle;
            public List<string> NeighborArticles;
            public int __v;
        }

        [System.Serializable]
        public class UsersData
        {
            public List<List<UserData>> users;

            public override string ToString()
            {
                return JsonUtility.ToJson(this);
            }

            public void Log()
            {
                Debug.Log("UsersData: " + JsonUtility.ToJson(this));
            }
        }
    }
}