using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using MarkerBasedARExample;

public class ChangeStage : MonoBehaviour
{
    public int currentValue = 1;
    public GyroSensorMarkerBasedARExample gyroSensorMarkerBasedARExample;

    public void SwitchValue()
    {
        if (currentValue == 1)
            currentValue = 2;
        else
            currentValue = 1;
        
        // send value to other script
        gyroSensorMarkerBasedARExample.currentStage = currentValue;

        GameObject myButtonObject = GameObject.Find("ChangeStage");
        Button myButton = myButtonObject.GetComponent<Button>();
        myButton.GetComponentInChildren<Text>().text = "Change Stage: " + currentValue.ToString() + " ";
    }
}