using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    [SerializeField] private SpriteRenderer source;
    [SerializeField] private SpriteRenderer afterImagePrefab;

    [SerializeField] private float interval = 0.01f;
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

            while (timer < time && !ctk.IsCancellationRequested)
            {
                CreateAfterImage();

                timer += interval;

                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void CreateAfterImage()
    {
        if (source == null || afterImagePrefab == null)
            return;

        SpriteRenderer image = Instantiate(afterImagePrefab, transform.position, transform.rotation);

        image.sprite = source.sprite;
        image.color = source.color;
        image.flipX = source.flipX;
        image.flipY = source.flipY;

        image.transform.localScale = transform.lossyScale;

        FadeOutImageAsync(image).Forget();
    }

    private async UniTaskVoid FadeOutImageAsync(SpriteRenderer image)
    {
        try
        {
            if (image == null)
                return;

            Color color = image.color;
            float timer = 0f;

            while (timer < duration)
            {
                if (image == null)
                    return;

                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(1f, 0f, timer / duration);

                color.a = alpha;
                image.color = color;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            }

            if (image != null)
                Destroy(image.gameObject);
        }
        catch (OperationCanceledException)
        {

        }
    }
}
