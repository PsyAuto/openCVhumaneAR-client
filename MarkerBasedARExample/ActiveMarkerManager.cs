using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkerBasedARExample;
using OpenCVMarkerBasedAR;

public class ActiveMarkerManager : MonoBehaviour
{
    public static ActiveMarkerManager Instance;

    public MarkerSettings activeMarkerSettings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
