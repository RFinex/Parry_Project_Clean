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
    [SerializeField] private float followDuration = 0.5f;

    [Header("Balance")]
    [SerializeField] private Image balanceFillLeft;
    [SerializeField] private Image balanceFillRight;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new(0f, 1f, 0f);

    private EnemyController target;
    private EnemyStats stats;

    private Tween delayTween;

    private CancellationTokenSource token;
    private CancellationTokenSource barToken;

    public void Bind(EnemyController enemy)
    {
        UnBind();

        Utils.EnterToken(ref token, enemy.DestroyToken);

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
        Utils.ExitToken(ref token);

        Utils.ExitToken(ref barToken);

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
                {
                    return;
                }

                transform.position = target.transform.position + offset;

                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate, ctk);
            }
        }
        catch(OperationCanceledException)
        {

        }
    }

    private async UniTaskVoid SetHealthAsync(float currentHealth, float maxHealth, CancellationToken ctk)
    {
        try
        {
            float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

            healthFill.fillAmount = ratio;

            await delayFill.DOFillAmount(ratio, followDuration)
                .SetEase(Ease.OutQuad)
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

        }
        catch (OperationCanceledException)
        {

        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        Utils.EnterToken(ref barToken);

        SetHealthAsync(currentHealth, maxHealth, barToken.Token).Forget();
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

    private void OnDestroy()
    {
        UnBind();
    }
}
