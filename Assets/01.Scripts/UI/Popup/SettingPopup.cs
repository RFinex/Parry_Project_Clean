using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : PopupBase
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;
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
