using UnityEngine;

public enum SFXType
{
    Swrod_Hit,
    Special_Attack
}

public enum BGMType
{

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

    protected override void AwakeSingleton()
    {
        SetBGMVolume(PlayerPrefs.GetFloat(ConstString.BGMVolume, defaultVolume));
        SetSFXVolume(PlayerPrefs.GetFloat(ConstString.SFXVolume, defaultVolume));
        MuteBGM(PlayerPrefs.GetInt(ConstString.BGMMute, 0));
        MuteSFX(PlayerPrefs.GetInt(ConstString.SFXMute, 0));
    }

    public void SetBGMVolume(float vol)
    {
        bgmSource.volume = vol;
        PlayerPrefs.SetFloat(ConstString.BGMVolume, vol);
    }

    public void SetSFXVolume(float vol)
    {
        sfxSource.volume = vol;
        PlayerPrefs.SetFloat(ConstString.SFXVolume, vol);
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
        return bgmSource.volume;
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


        bgmSource.clip = bgmClip[(int)type];
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
