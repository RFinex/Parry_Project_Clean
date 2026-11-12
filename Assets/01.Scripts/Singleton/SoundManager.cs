using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public enum SFXType
{
    Swrod_Hit,
    Special_Attack,
    Parry,
    Guard,
    Sword_NoHit,
    MouseOnButton,
    ClickButton,
    Heal,
    Chest
}

public enum BGMType
{
    Menu,
    Lobby,
    Stage1,
    Stage2,
    Elite,
    Boss
}

public class SoundManager : Singleton<SoundManager>
{
    public static SoundManager instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip[] bgmClip;
    [SerializeField] private AudioClip[] sfxClip;

    [Header("Default Volume")]
    [SerializeField] private float defaultVolume = 0.5f;

    private float currentBGMVolume = 0.5f;
    private float fadeMultiplier = 1f;
    [SerializeField] private float fadeDuration = 0.2f;

    private CancellationTokenSource token;

    private BGMType previousBGM;
    private BGMType currentBGM;

    protected override void AwakeSingleton()
    {
        SetBGMVolume(PlayerPrefs.GetFloat(ConstString.BGMVolume, defaultVolume));
        SetSFXVolume(PlayerPrefs.GetFloat(ConstString.SFXVolume, defaultVolume));
        MuteBGM(PlayerPrefs.GetInt(ConstString.BGMMute, 0));
        MuteSFX(PlayerPrefs.GetInt(ConstString.SFXMute, 0));
    }

    private void OnEnable()
    {
        token = token.ResetToken();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public void SetBGMVolume(float vol)
    {
        currentBGMVolume = vol;
        PlayerPrefs.SetFloat(ConstString.BGMVolume, vol);

        UpdateBGMVolume();
    }

    public void SetSFXVolume(float vol)
    {
        sfxSource.volume = vol;
        PlayerPrefs.SetFloat(ConstString.SFXVolume, vol);
    }

    private void UpdateBGMVolume()
    {
        bgmSource.volume = currentBGMVolume * fadeMultiplier;
    }

    public void MuteBGM(int mute)
    {
        if (mute == 1)
        {
            SetMuteBGM(true);
        }
        else
        {
            SetMuteBGM(false);
        }
    }

    public void MuteSFX(int mute)
    {
        if (mute == 1)
        {
            SetMuteSFX(true);
        }
        else
        {
            SetMuteSFX(false);
        }
    }

    // 음소거 설정 저장
    public void SetMuteBGM(bool isMute)
    {
        bgmSource.mute = isMute;
        PlayerPrefs.SetInt(ConstString.BGMMute, isMute ? 1 : 0);
    }

    public void SetMuteSFX(bool isMute)
    {
        sfxSource.mute = isMute;
        PlayerPrefs.SetInt(ConstString.SFXMute, isMute ? 1 : 0);
    }

    // 볼륨 정보 외부 제공용
    public float GetBGMVolume()
    {
        return currentBGMVolume;
    }

    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }

    public void PlaySFX(SFXType type)
    {
        if ((int)type >= sfxClip.Length || (int)type < 0)
            return;

        sfxSource.PlayOneShot(sfxClip[(int)type]);
    }

    public void PlayBGM(BGMType type)
    {
        if ((int)type >= bgmClip.Length || (int)type < 0)
            return;

        //switch (type)
        //{
        //    case BGMType.Menu:
        //    case BGMType.Game:
        //    case BGMType.Boss:
        //        bgmSource.loop = true;
        //        break;

        //    case BGMType.Die:
        //    case BGMType.Clear:
        //        bgmSource.loop = false;
        //        break;
        //}

        if (bgmSource.clip == bgmClip[(int)type] && bgmSource.isPlaying)
            return;

        previousBGM = currentBGM;
        currentBGM = type;

        fadeMultiplier = 1f;
        UpdateBGMVolume();

        bgmSource.clip = bgmClip[(int)type];
        bgmSource.Play();
    }

    public void PlayPreviousBGM()
    {
        PlayBGM(previousBGM);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public async UniTask StopBGMFadeOut(CancellationToken ctk)
    {
        try
        {
            await DOTween.To(() => fadeMultiplier,
                value =>
                {
                    fadeMultiplier = value;
                    UpdateBGMVolume();
                },
                0f,
                fadeDuration)
                .SetEase(Ease.Linear)
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

            StopBGM();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
