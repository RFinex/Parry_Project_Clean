using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class RoomFadeUI : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.3f;

    private void Awake()
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    public UniTask FadeOutAsync(CancellationToken ctk)
    {
        return FadeAsync(1f, ctk);
    }

    public UniTask FadeInAsync(CancellationToken ctk)
    {
        return FadeAsync(0f, ctk);
    }

    private async UniTask FadeAsync(float targetAlpha, CancellationToken ctk)
    {
        try
        {
            if (fadeImage == null)
                return;

            fadeImage.gameObject.SetActive(true);

            await fadeImage.DOFade(targetAlpha, fadeDuration)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

            if (targetAlpha <= 0f)
                fadeImage.gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {

        }
    }
}
