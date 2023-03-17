using UnityEngine;

public class ApplicationLayer : MonoBehaviour
{
    public APIClient apiClient;
    public string name;
    public int age;

    private void Start()
    {
        apiClient.CreateUser(name, age);
    }
}
