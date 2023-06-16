using UnityEngine;

public class ApplicationLayer : MonoBehaviour
{
    public APIClient apiClient;
    public new string name;
    public int age;

    private void Start()
    {
        apiClient.CreateUser(name, age);
    }
}
