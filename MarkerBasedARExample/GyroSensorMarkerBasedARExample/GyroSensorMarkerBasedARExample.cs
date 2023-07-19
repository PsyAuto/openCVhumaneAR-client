using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVMarkerBasedAR;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using AgentAPI;
using Newtonsoft.Json.Linq;

namespace MarkerBasedARExample
{
    /// <summary>
    /// Gyro Sensor Marker Based AR Example
    /// An example of augmented reality display method with gyro sensor.
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class GyroSensorMarkerBasedARExample : MonoBehaviour
    {
        // Add a public property to store the current stage
        public int CurrentStage { get; set; } = 1;

        private int selectedMarkerIndex;
        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// The marker settings.
        /// </summary>
        public MarkerSettings[] markerSettings;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

        /// <summary>
        /// The marker detector.
        /// </summary>
        MarkerDetector markerDetector;

        /// <summary>
        /// The matrix that inverts the Y axis.
        /// </summary>
        Matrix4x4 invertYM;

        /// <summary>
        /// The matrix that inverts the Z axis.
        /// </summary>
        Matrix4x4 invertZM;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// position of selected marker
        /// </summary>
        Vector3 selectedMarkerPosition =  Vector3.zero;

        private Button changeCameraButton;
        private Button createKeywordsButton;

        public Canvas keywordsInputCanvas;
        public TMP_InputField keywordsInputField;
        public Button submitButton;
        private UserAPI userAPI;
        private float myRadius;
        private List<string> globalUserIds = new List<string>();

#if UNITY_EDITOR
        Vector3 rot;
#endif

        // Use this for initialization
        void Start()
        {
            // Find the GameObject with the UserAPI component
            GameObject userObject = GameObject.Find("Agent");

            // Get the UserAPI component from the GameObject
            userAPI = userObject.GetComponent<UserAPI>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

            webCamTextureToMatHelper.Initialize();

#if UNITY_EDITOR
            rot = ARCamera.transform.rotation.eulerAngles;
#else
            Input.gyro.enabled = true;
#endif

            // Find the "ChangeCameraButton" object and get its Button component
            GameObject myButtonObject = GameObject.Find("ChangeCameraButton");
            changeCameraButton = myButtonObject.GetComponent<Button>();

            // Find the "createKeywordsButton" object and get its Button component
            GameObject createKeywordsObject = GameObject.Find("createKeywordsButton");
            createKeywordsButton = createKeywordsObject.GetComponent<Button>();

            // Set the initial visibility of the button based on the current stage
            createKeywordsButton.gameObject.SetActive(CurrentStage == 1);

            // Add an OnClick event to the "createKeywordsButton" that shows the text input canvas
            createKeywordsButton.onClick.AddListener(() => {
                keywordsInputCanvas.gameObject.SetActive(true);
            });

            // disable the text input canvas at the start
            keywordsInputCanvas.gameObject.SetActive(false);

            // Add an OnClick event to the "submitButton" that submits the text input and hides the canvas
            submitButton.onClick.AddListener(() => {
                string inputText = keywordsInputField.text;
                Debug.Log("Input text: " + inputText);
                keywordsInputCanvas.gameObject.SetActive(false);
            });

            InvokeRepeating("GetCurrentStage", 0f, 1f);
            InvokeRepeating("GetRadius", 0f, 1f);
            InvokeRepeating("getUserIDs", 0f, 1f);
            // print the globalUserIds list to the console
            InvokeRepeating("PrintGlobalUserIds", 0f, 1f);
        }

//create PrintGlobalUserIds
        void PrintGlobalUserIds()
        {
            Debug.Log("Global User IDs: " + string.Join(", ", globalUserIds.ToArray()));
            // print the first element of the globalUserIds list to the console
            Debug.Log("First Global User ID: " + globalUserIds[0]);
        }
        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            //Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;


            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);

            //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }


            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            //Debug.Log("camMatrix " + camMatrix.dump());

            distCoeffs = new MatOfDouble(0, 0, 0, 0);
            //Debug.Log("distCoeffs " + distCoeffs.dump());

            //calibration camera
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];

            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

            // To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));

            // Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale)
            {
                ARCamera.fieldOfView = (float)(fovx[0] * fovXScale);
            }
            else
            {
                ARCamera.fieldOfView = (float)(fovy[0] * fovYScale);
            }

            MarkerDesign[] markerDesigns = new MarkerDesign[markerSettings.Length];
            for (int i = 0; i < markerDesigns.Length; i++)
            {
                markerDesigns[i] = markerSettings[i].markerDesign;
            }

            markerDetector = new MarkerDetector(camMatrix, distCoeffs, markerDesigns);


            invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            //Debug.Log("invertYM " + invertYM.ToString());

            invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            //Debug.Log("invertZM " + invertZM.ToString());


            //if WebCamera is frontFaceing,flip Mat.
            webCamTextureToMatHelper.flipHorizontal = webCamTextureToMatHelper.GetWebCamDevice().isFrontFacing;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // private float lastUpdateTime = 0f;
        // private float updateInterval = 0.05f;

        // Update is called once per frame
        void Update()
        {
            if (!webCamTextureToMatHelper.IsPlaying() || !webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                return;
            }

            UpdateARCameraTransform();

            Mat rgbaMat = webCamTextureToMatHelper.GetMat();
            int selectedMarkerIndex = PlayerPrefs.GetInt("SelectedMarkerIndex", -1);

            ProcessMarkers(rgbaMat, selectedMarkerIndex);

            Utils.fastMatToTexture2D(rgbaMat, texture);
        }

        void GetCurrentStage()
        {
            userAPI.GetCurrentStage((currentStage) =>
            {
                CurrentStage = int.Parse(currentStage);
                Debug.Log("CurrentStage: " + CurrentStage);
            });
        }

        void GetRadius()
        {
            userAPI.GetRadius((radius) =>
            {
                myRadius = float.Parse(radius);
                Debug.Log("Radius: " + myRadius);
            });
        }

        public void getUserIDs()
        {
            userAPI.GetAllUserIDs((userIDs) =>
            {
                JArray jsonArrayObj = JArray.Parse(userIDs);
                JArray userIdsArray = (JArray)jsonArrayObj[0];
                List<string> userIds = new List<string>();
                foreach (JToken userId in userIdsArray)
                {
                    userIds.Add(userId.ToString());
                }
                globalUserIds = userIds;
            });
        }

        void UpdateUI(string buttonText)
        {
            // Update the text of the button using the stored reference
            changeCameraButton.GetComponentInChildren<Text>().text = buttonText;

            // Update the visibility of the "createKeywordsButton" based on the current stage
            createKeywordsButton.gameObject.SetActive(CurrentStage == 1);
        }

        void ProcessMarkers(Mat rgbaMat, int selectedMarkerIndex)
        {
            markerDetector.processFrame(rgbaMat, 1);

            int selectedMarkerId = markerSettings[selectedMarkerIndex].getMarkerId();

            // findMarkers are the markers that are found in the current frame
            List<Marker> findMarkers = markerDetector.getFindMarkers();
            if (CurrentStage == 1)
            {
                //debug
                string buttonText = $"Marker: {selectedMarkerIndex} {markerSettings[selectedMarkerIndex].getMarkerId()} Stage: {CurrentStage}";
                UpdateUI(buttonText);

                HideDetectionRange();

                foreach (Marker marker in findMarkers)
                {
                    MarkerSettings settings = markerSettings.FirstOrDefault(s => s.getMarkerId() == marker.id);

                    if (settings != null)
                    {
                        bool isSelectedMarker = (marker.id == selectedMarkerId);
                        UpdateARGameObject(marker, settings, isSelectedMarker);
                    }
                }
            }
            else if (CurrentStage == 2)
            {
                //debug
                string message = "Neighbours: ";
                
                foreach (Marker marker in findMarkers)
                {
                    MarkerSettings settings = markerSettings.FirstOrDefault(s => s.getMarkerId() == marker.id);

                    if (settings != null)
                    {
                        UpdateARGameObject(marker, settings, true);

                        SpawnMarkerRadius(markerSettings[selectedMarkerIndex], myRadius);
                        // selectedMarkerPosition is the position of the selected marker
                        selectedMarkerPosition =  markerSettings[selectedMarkerIndex].getARGameObject().transform.position;
                        // if the marker is not the selected marker and is within the radius of the selected marker then it is a neighbour
                        if (settings.getMarkerId() != selectedMarkerId && Vector3.Distance(selectedMarkerPosition, settings.getARGameObject().transform.position) <= 3f)
                        {
                            message += marker.id + ", ";
                        }

                    }
                }
                //debug
                string buttonText = $"Marker: {markerSettings[selectedMarkerIndex].getMarkerId()} Stage: {CurrentStage} {message}";
                UpdateUI(buttonText);
                ShowDetectionRange();
            }
        }

        void UpdateARGameObject(Marker marker, MarkerSettings settings, bool activateObject)
        {
            Matrix4x4 transformationM = marker.transformation;
            Matrix4x4 adjustedTransformationMatrix = ModifyTransformationMatrix(transformationM);

            GameObject ARGameObject = settings.getARGameObject();

            settings.debugText.text = settings.getMarkerId().ToString();

            if (ARGameObject != null)
            {
                ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref adjustedTransformationMatrix);

                if (activateObject)
                {
                    DelayableSetActive obj = ARGameObject.GetComponent<DelayableSetActive>();

                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                    else
                    {
                        ARGameObject.SetActive(true);
                    }
                }
                else
                {
                    DelayableSetActive obj = ARGameObject.GetComponent<DelayableSetActive>();

                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                    else
                    {
                        ARGameObject.SetActive(false);
                    }
                }
            }
        }

        public void UpdateARCameraTransform()
        {
#if UNITY_EDITOR
            float spd = Time.deltaTime * 100.0f;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rot.y -= spd;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rot.y += spd;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                rot.x -= spd;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                rot.x += spd;
            }
            ARCamera.transform.rotation = Quaternion.Euler(rot);
#else
            ARCamera.transform.rotation = Quaternion.AngleAxis (90.0f, Vector3.right) * Input.gyro.attitude * Quaternion.AngleAxis (180.0f, Vector3.forward);
#endif
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            webCamTextureToMatHelper.Dispose();

#if UNITY_EDITOR
#else
            Input.gyro.enabled = false;
#endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerBasedARExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        private Matrix4x4 ModifyTransformationMatrix(Matrix4x4 transformationMatrix) {

            Matrix4x4 ARM = invertYM * transformationMatrix * invertYM;

            ARM = ARM * invertYM * invertZM;

            ARM = ARCamera.transform.localToWorldMatrix * ARM;

            return ARM;
        }

        void SpawnMarkerRadius(MarkerSettings settings, float radius)
        {
            // Get the GameObject associated with the marker
            GameObject arGameObject = settings.getARGameObject();
            if (arGameObject != null)
            {
                // Get the DetectionRange GameObject
                GameObject detectionRange = GameObject.Find("DetectionRange");

                // Set the position of the DetectionRange to the position of the AR GameObject
                detectionRange.transform.position = arGameObject.transform.position;

                // Set the scale of the DetectionRange based on the radius
                float diameter = radius * 2f;
                detectionRange.transform.localScale = new Vector3(diameter, 0.1f, diameter);
            }
        }

        void ShowDetectionRange()
        {
            GameObject detectionRange = GameObject.Find("DetectionRange");
            if (detectionRange != null)
            {
                detectionRange.layer = LayerMask.NameToLayer("AR");
            }
        }

        void HideDetectionRange()
        {
            GameObject detectionRange = GameObject.Find("DetectionRange");
            if (detectionRange != null)
            {
                detectionRange.layer = LayerMask.NameToLayer("Default");
            }
        }

        private List<MarkerSettings> FindMarkerNeighbours(MarkerSettings[] markerSettings, int selectedMarkerId, float radius){
            //find the distance between the selected marker and all other markers
            //if the distance is less than the radius of the selected marker, add it to the list of neighbours
            //return the list of neighbours
            List<MarkerSettings> neighbours = new List<MarkerSettings>();
            foreach (MarkerSettings marker in markerSettings)
            {
                if (marker.getMarkerId() != selectedMarkerId)
                {
                    float distance = Vector3.Distance(markerSettings[selectedMarkerId].getARGameObject().transform.position, marker.getARGameObject().transform.position);
                    if (distance < radius)
                    {
                        neighbours.Add(marker);
                    }
                }
            }
            return neighbours;
        }
    }
}