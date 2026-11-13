using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [SerializeField] private Transform spriteLightTransform;

    [SerializeField] private float maxScale = 2.6f;
    [SerializeField] private float minScale = 2.4f;
    [SerializeField] private float duration = 2f;

    [SerializeField] private int steps = 10;

    private CancellationTokenSource token;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        token = token.ResetToken();
        LightShake(token.Token).Forget();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    private async UniTaskVoid LightShake(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                while (!ctk.IsCancellationRequested)
                {
                    // minScale -> maxScale
                    await DOTween.To(() => minScale, x => {
                        // ´Ü°è¸¦ ³ª´©¾î ¶Ò¶Ò ²÷±âµµ·Ï °ª º¯È¯
                        float snapped = Mathf.Round(x * steps) / steps;
                        spriteLightTransform.localScale = new Vector3(snapped, snapped, 1f);
                    }, maxScale, duration * 0.5f)
                    .SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: ctk);

                    // maxScale -> minScale
                    await DOTween.To(() => maxScale, x => {
                        float snapped = Mathf.Round(x * steps) / steps;
                        spriteLightTransform.localScale = new Vector3(snapped, snapped, 1f);
                    }, minScale, duration * 0.5f)
                    .SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: ctk);
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
