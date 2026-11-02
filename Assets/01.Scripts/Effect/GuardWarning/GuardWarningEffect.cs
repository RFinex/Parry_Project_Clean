using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GuardWarningEffect : EffectBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animName = "Warning_Guard";
    [SerializeField] private float animDuration = (5f / 12f);

    private int animHash;

    private CancellationTokenSource token;

    private void Awake()
    {
        animHash = Animator.StringToHash(animName);
    }

    private void OnEnable()
    {
        token = token.ResetToken();

        ReturnPoolAsync(token.Token).Forget();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public override void InitPool()
    {
        base.InitPool();

        if (animator == null)
            return;

        animator.Play(animHash, 0, 0f);
    }

    private async UniTaskVoid ReturnPoolAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(animDuration), cancellationToken: ctk);

            ReturnPool();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
