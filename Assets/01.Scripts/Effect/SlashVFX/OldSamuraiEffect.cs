using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class OldSamuraiEffect : EffectBase
{
    [SerializeField] private float returnTimer = 1f;
    private ParticleSystem particle;

    private CancellationTokenSource token;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        token = token.ResetToken();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public void PlayEffect()
    {
        if (particle == null)
            return;

        particle.Play();

        ReturnTimer(token.Token).Forget();
    }

    private async UniTaskVoid ReturnTimer(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(returnTimer), cancellationToken: ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            ReturnPool();
        }
    }
}
