using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusBar : UIBase
{
    [Header("Hp")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image delayFill;
    [SerializeField] private float followDelay = 0.25f;
    [SerializeField] private float followDuration = 0.4f;

    [Header("Balance")]
    [SerializeField] private Image balanceFillLeft;
    [SerializeField] private Image balanceFillRight;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new(0f, 1f, 0f);

    private EnemyController target;
    private EnemyStats stats;

    private Tween delayTween;

    private CancellationTokenSource token;

    public void Bind(EnemyController enemy)
    {
        UnBind();

        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        target = enemy;
        stats = enemy.Stats;

        stats.OnHealthChanged += SetHealth;
        stats.OnBalanceChanged += SetBalance;

        SetHealth(stats.NowHp, stats.MaxHp);
        SetBalance(stats.NowBalance, stats.MaxBalance);

        LateUpdateAsync(token.Token).Forget();
    }

    public void UnBind()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        if (stats != null)
        {
            stats.OnHealthChanged -= SetHealth;
            stats.OnBalanceChanged -= SetBalance;
            stats = null;
        }

        target = null;

        delayTween?.Kill();
        delayTween = null;
    }

    private async UniTaskVoid LateUpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                if (target == null)
                    return;

                transform.position = target.transform.position + offset;

                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate, ctk);
            }
        }
        catch(OperationCanceledException)
        {

        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        healthFill.fillAmount = ratio;

        if (ratio >= delayFill.fillAmount)
        {
            delayTween?.Kill();
            delayFill.fillAmount = ratio;
            return;
        }

        delayTween?.Kill();
        delayTween = delayFill.DOFillAmount(ratio, followDuration)
            .SetDelay(followDelay)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    public void SetBalance(float currentBalance, float maxBalance)
    {
        float ratio = maxBalance > 0f ? Mathf.Clamp01(currentBalance / maxBalance) : 0f;

        balanceFillLeft.fillAmount = ratio;
        balanceFillRight.fillAmount = ratio;
    }

    public override void InitPoolReturn()
    {
        UnBind();
    }
}
