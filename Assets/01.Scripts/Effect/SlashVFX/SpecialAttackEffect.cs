using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class SpecialAttackEffect : EffectBase
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
        particle.Play();

        ReturnTimer(token.Token).Forget();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
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
