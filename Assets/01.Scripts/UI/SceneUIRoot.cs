using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SceneUIRoot : MonoBehaviour
{
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private GameObject dim;

    private void Awake()
    {
        UIManager.Instance.RegisterSceneCanvas(popupCanvas, dim);
    }
}
