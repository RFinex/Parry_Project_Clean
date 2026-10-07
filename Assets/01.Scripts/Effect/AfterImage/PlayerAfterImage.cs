using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    [Header("Player SpriteRenderer")]
    [SerializeField] private SpriteRenderer source;

    [Header("After Image Id")]
    [SerializeField] private int afterImageId;

    [Header("Image Instantiate")]
    [SerializeField] private float interval = 0.02f;
    [SerializeField] private float duration = 0.15f;

    private CancellationTokenSource token;

    private void Awake()
    {
        if (source == null)
        {
            source = GetComponent<SpriteRenderer>();
        }
    }

    public void PlayImage(float time)
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        PlayAsync(time, token.Token).Forget();
    }

    public void StopImage()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }

    private async UniTaskVoid PlayAsync(float time, CancellationToken ctk)
    {
        try
        {
            float timer = 0f;
            float intervalTimer = 0f;

            while (timer < time && !ctk.IsCancellationRequested)
            {
                float deltaTime = Time.deltaTime;
                timer += deltaTime;
                intervalTimer += deltaTime;

                if (intervalTimer >= interval)
                {
                    CreateAfterImage();
                    intervalTimer = 0f;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void CreateAfterImage()
    {
        if (source == null)
            return;

        AfterImageObject image = ObjectPoolManager.Instance.Get<AfterImageObject>(afterImageId, transform.position);

        image.Sr.sprite = source.sprite;
        image.Sr.color = source.color;
        image.Sr.flipX = source.flipX;
        image.Sr.flipY = source.flipY;

        image.transform.localScale = transform.lossyScale;

        FadeOutImageAsync(image).Forget();
    }

    private async UniTaskVoid FadeOutImageAsync(AfterImageObject image)
    {
        try
        {
            if (image == null)
                return;

            SpriteRenderer sr = image.Sr;

            Color color = sr.color;
            float timer = 0f;

            while (timer < duration)
            {
                if (image == null)
                    return;

                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(1f, 0f, timer / duration);

                color.a = alpha;
                sr.color = color;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            }

            image.ReturnPool();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
