using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AgentAPI
{
    public class DebugText : MonoBehaviour
    {
        private TextMeshProUGUI textComponent;

        private void Start()
        {
            GameObject rootObject = GameObject.Find("DebugCanvas");
            transform.parent = rootObject.transform;
            DontDestroyOnLoad(rootObject);
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void Log(string message)
        {
            textComponent.text += message + "\n";
        }

        public void Clear()
        {
            textComponent.text = "";
        }

        public static void LogToText(string message, TextMeshProUGUI textComponent)
        {
            textComponent.text += message + "\n";
        }

        public static void ClearText(TextMeshProUGUI textComponent)
        {
            textComponent.text = "";
        }
    }
}