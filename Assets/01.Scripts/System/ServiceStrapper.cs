using UnityEngine;
using UnityEngine.SceneManagement;

public class ServiceStrapper : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneUnloaded += ServiceLocator.Remove;
    }
}
