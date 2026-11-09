using UnityEngine;
using UnityEngine.UI;

public class CreditPopup : PopupBase
{
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        if (exitBtn != null)
            exitBtn.onClick.AddListener(CancelPopup);
    }

    private void OnDestroy()
    {
        if (exitBtn != null)
            exitBtn.onClick.RemoveListener(CancelPopup);
    }
}
