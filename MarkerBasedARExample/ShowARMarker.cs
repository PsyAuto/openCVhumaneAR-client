using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using OpenCVMarkerBasedAR;
using System.Collections.Generic;

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
        /// The index.
        /// </summary>
        int index = 0;

        [System.Serializable]
        public class ARMarkerData
        {
            public int selectedMarkerIndex;
        }

        // Use this for initialization
        void Start()
        {
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
        }
        
        //TODO: store the selected marker index in file for access by other scripts, run in android

    }
}
