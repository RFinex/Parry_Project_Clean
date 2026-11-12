using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private BGMType bgmType;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(bgmType);
    }
}
