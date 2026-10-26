using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class SceneTransitionUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private GameObject loadingRoot;

    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        fade.alpha = 0f;
        fade.gameObject.SetActive(false);

        HideLoading();
    }

    public void ShowLoading()
    {
        loadingRoot.SetActive(true);
    }

    public void HideLoading()
    {
        loadingRoot.SetActive(false);
    }

    public async UniTask FadeOutAsync(CancellationToken ctk)
    {
        await FadeAsync(1f, ctk);
    }
    public async UniTask FadeInAsync(CancellationToken ctk)
    {
        await FadeAsync(0f, ctk);
    }

    public async UniTask FadeAsync(float targetAlpha, CancellationToken ctk)
    {
        try
        {
            fade.gameObject.SetActive(true);

            await fade.DOFade(targetAlpha, fadeDuration)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ctk);

            if(targetAlpha == 0f)
                fade.gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {

        }
    }
}
