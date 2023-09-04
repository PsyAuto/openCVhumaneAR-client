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
        private string myUserId;

        private void Start()
        {
            // Make the UserAPI object persist between scenes
            DontDestroyOnLoad(gameObject);

            // get myUserId from playerprefs
            myUserId = PlayerPrefs.GetString("myUserId", "");

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

            // wait for socket connection to be established
            yield return new WaitUntil(() => socket.Connected);

            // If myUserId is not set, create a new user
            if (string.IsNullOrEmpty(myUserId))
            {
                StartCoroutine(CreateNewUser());
            }
            // else, get all users from the server and check if myUserId is in the list
            else
            {
                StartCoroutine(RestoreUser(myUserId));
            }
            DebugText.LogToText("connected: "+socket.Connected, textComponent);
        }

        private IEnumerator CreateNewUser()
        {
            // use the userData object to create a new user
            socket.Emit("createUser", Objects.userData);
            socket.On("userCreated", (response) =>
            {
                string jsonArray = response.ToString();
                Objects.UserData[] userDataArray = JsonConvert.DeserializeObject<Objects.UserData[]>(jsonArray);
                Objects.userData = userDataArray[0];

            });

            yield return new WaitUntil(() => Objects.userData.userID != null);
            myUserId = Objects.userData.userID;
            PlayerPrefs.SetString("myUserId", myUserId);
            PlayerPrefs.Save();
            DebugText.LogToText("Created user: " + myUserId, textComponent);
        }

        private IEnumerator RestoreUser(string myUserId)
        {
            StartCoroutine(getUserIDs());
            // wait until Objects.globalUserIds is set
            yield return new WaitUntil(() => Objects.globalUserIds != null);
            // Check if myUserId exists in Objects.globalUserIds
            if (Objects.globalUserIds.Contains(myUserId))
            {
                // If myUserId exists, get the user data from the server
                GetUserByID(myUserId, (userData) =>
                {
                    Objects.userData = userData;
                    myUserId = Objects.userData.userID;
                });
                // wait for Objects.userData to be set
                yield return new WaitUntil(() => Objects.userData.userID != null);
                DebugText.LogToText("Restored user: " + Objects.userData.userID, textComponent);
            }
            else
            {
                // If myUserId does not exist, create a new user
                StartCoroutine(CreateNewUser());
            }
        }

        // get the current stage
        public void GetCurrentStage(Action<string> callback)
        {
            socket.Emit("getCurrentStage");
            socket.On("currentStage", (response) =>
            {
                string currentStage = response.GetValue<string>();
                callback(currentStage);
            });
        }

        // get the radius of the current stage
        public void GetRadius(Action<string> callback)
        {
            socket.Emit("getRadius");
            socket.On("radius", (response) =>
            {
                string radius = response.GetValue<string>();
                callback(radius);
            });
        }

        // get all users
        public IEnumerator GetAllUsers() {
            socket.Emit("getUsers");
            socket.On("users", (response) => 
            {
                // reset allUsersData
                Objects.allUsersData = null;
                string jsonArray = response.ToString();
                var userDataArray = JsonConvert.DeserializeObject<List<List<Objects.UserData>>>(jsonArray);
                Objects.allUsersData = userDataArray[0];            
            });

            // yield until Objects.allUsersData is set
            yield return new WaitUntil(() => Objects.allUsersData != null);
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

        public IEnumerator getUserIDs()
        {
            Objects.globalUserIds = null;
            GetAllUserIDs((userIDs) =>
            {
                JArray jsonArrayObj = JArray.Parse(userIDs);
                JArray userIdsArray = (JArray)jsonArrayObj[0];
                List<string> userIds = new List<string>();
                foreach (JToken userId in userIdsArray)
                {
                    userIds.Add(userId.ToString());
                }
                Objects.globalUserIds = userIds;
            });

            yield return new WaitUntil(() => Objects.globalUserIds != null);
        }

        // get a specific user by their userID
        public void GetUserByID(string userID, Action<Objects.UserData> callback)
        {
            socket.Emit("getUserByID", userID);
            socket.On("userByID", (response) =>
            {
                string jsonArray = response.ToString();
                Objects.UserData[] userDataArray = JsonConvert.DeserializeObject<Objects.UserData[]>(jsonArray);
                if (userDataArray != null && userDataArray.Length > 0)
                {
                    callback(userDataArray[0]);
                }
                else
                {
                    callback(null);
                }
            });
        }

        // update a specific user by their userID
        public void UpdateUserByID(string userID, Objects.UserData user)
        {
            socket.Emit("updateUserByID", userID, user);
        }
    }
}