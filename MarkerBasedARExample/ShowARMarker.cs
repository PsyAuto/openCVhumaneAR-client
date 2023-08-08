using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using OpenCVMarkerBasedAR;
using System.Collections.Generic;
using AgentAPI;
using System.Linq;

namespace MarkerBasedARExample
{
    /// <summary>
    /// Show ARMarker.
    /// </summary>
    public class ShowARMarker : MonoBehaviour
    {
        //public Button myButton;
        /// <summary>
        /// Show ARMarker
        /// </summary>
        public Texture2D[] markerTexture;

        /// <summary>
        /// The marker settings.
        /// </summary>
        public MarkerSettings[] markerSettings;

        /// <summary>
        /// The index.
        /// </summary>
        int index = 0;

        private UserAPI userAPI;

        [System.Serializable]
        public class ARMarkerData
        {
            public int selectedMarkerIndex;
        }

        // Use this for initialization
        void Start()
        {
            // Find the GameObject with the UserAPI component
            GameObject userObject = GameObject.Find("Agent");

            // Get the UserAPI component from the GameObject
            userAPI = userObject.GetComponent<UserAPI>();

            float width = gameObject.transform.localScale.x;
            float height = gameObject.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            gameObject.GetComponent<Renderer>().material.mainTexture = markerTexture[index];

            // create a list of all marker ids
            for (int i = 0; i < markerSettings.Length; i++)
            {
                Objects.allMarkerIds.Add(markerSettings[i].getMarkerId());
            }
            string AllMarkerIds = string.Join(",", Objects.allMarkerIds.Select(i => i.ToString()).ToArray());
            PlayerPrefs.SetString("AllMarkerIds", AllMarkerIds);
            PlayerPrefs.Save();
        }


        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable()
        {

        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerBasedARExample");
        }

        /// <summary>
        /// Raises the change marker button event.
        /// </summary>
        public void OnChangeMarkerButtonClick()
        {
            index = (index + 1) % markerTexture.Length;
            gameObject.GetComponent<Renderer>().material.mainTexture = markerTexture[index];

            // Store the selected marker index in PlayerPrefs
            PlayerPrefs.SetInt("SelectedMarkerIndex", index);
            PlayerPrefs.Save();
            int savedIndex = PlayerPrefs.GetInt("SelectedMarkerIndex");
            GameObject myButtonObject = GameObject.Find("ChangeMarkerButton");
            Button myButton = myButtonObject.GetComponent<Button>();
            myButton.GetComponentInChildren<Text>().text = "Selected Marker: " + index.ToString() + " PlayerPrefs:" + savedIndex.ToString();
            Debug.Log("marker id" + markerSettings[savedIndex].getMarkerId());
            
            Objects.userData.SelectedMarkerIndex = savedIndex;
            userAPI.UpdateUserByID(Objects.userData.userID, Objects.userData);
        }
    }
}
