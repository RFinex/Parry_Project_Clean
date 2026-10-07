using UnityEngine;

public class UIPoolParent : MonoBehaviour
{
    private void Awake()
    {
        ObjectPoolManager.Instance.SetUIPoolParent(transform);
    }
}
