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
    }
}