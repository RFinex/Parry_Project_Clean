using UnityEngine;

public class SceneUIRoot : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;

    private void Awake()
    {
        UIManager.Instance.RegisterSceneCanvas(uiCanvas);
    }
}
