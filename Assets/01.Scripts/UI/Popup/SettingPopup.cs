using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : PopupBase
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        bgmSlider.onValueChanged.AddListener(BGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(SFXVolumeChanged);
        exitBtn.onClick.AddListener(CancelPopup);

    }

    private void OnDestroy()
    {
        bgmSlider.onValueChanged.RemoveListener(BGMVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(SFXVolumeChanged);
        exitBtn.onClick.RemoveListener(CancelPopup);
    }

    protected override void Initialized()
    {
        if (SoundManager.Instance == null)
            return;

        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.GetBGMVolume());
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.GetSFXVolume());
    }

    private void BGMVolumeChanged(float vol)
    {
        SoundManager.Instance.SetBGMVolume(vol);
    }

    private void SFXVolumeChanged(float vol)
    {
        SoundManager.Instance.SetSFXVolume(vol);
    }
}
