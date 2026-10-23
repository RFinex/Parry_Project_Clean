using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BossStatusBar : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image delayFill;
    [SerializeField] private float delayDuration;

    [Header("Balance")]
    [SerializeField] private Image balanceFillLeft;
    [SerializeField] private Image balanceFillRight;

    private BossController boss;
    private EnemyStats stats;

    private CancellationTokenSource token;

    private void Awake()
    {
        visual.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BossFightStartedEvent>(HandleBossFightStarted);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<BossFightStartedEvent>(HandleBossFightStarted);
        UnBind();
    }

    private void HandleBossFightStarted(BossFightStartedEvent eventData)
    {
        if (eventData.Boss == null)
            return;

        UnBind();

        boss = eventData.Boss;
        stats = boss.Stats;

        stats.OnHealthChanged += SetHealth;
        stats.OnBalanceChanged += SetBalance;
        boss.OnDeathFinished += HandleBossDeathFinished;

        float healthRatio = stats.MaxHp > 0f ? Mathf.Clamp01(stats.NowHp / stats.MaxHp) : 0f;

        SetHealth(stats.NowHp, stats.MaxHp);
        SetBalance(stats.NowBalance, stats.MaxBalance);

        visual.SetActive(true);
    }

    private async UniTaskVoid SetHealthAsync(float currentHealth, float maxHealth, CancellationToken ctk)
    {
        try
        {
            float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

            healthFill.fillAmount = ratio;

            await delayFill.DOFillAmount(ratio, delayDuration)
                .SetEase(Ease.OutQuad)
                .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);
        }
        catch (OperationCanceledException)
        {

        }        
    }

    private void SetHealth(float currentHelath, float maxHelath)
    {
        token = token.ResetToken();

        SetHealthAsync(currentHelath, maxHelath, token.Token).Forget();
    }

    private void SetBalance(float currentBalance, float maxBalance)
    {
        float ratio = maxBalance > 0f ? Mathf.Clamp01(currentBalance / maxBalance) : 0f;

        balanceFillLeft.fillAmount = ratio;
        balanceFillRight.fillAmount = ratio;
    }

    private void HandleBossDeathFinished(EnemyController enemy)
    {
        if (enemy != boss)
            return;

        UnBind();

        visual.SetActive(false);
    }

    private void UnBind()
    {
        token = token.Cancel_DisposeToken();

        if (stats != null)
        {
            stats.OnHealthChanged -= SetHealth;
            stats.OnBalanceChanged -= SetBalance;
            stats = null;
        }

        if (boss != null)
        {
            boss.OnDeathFinished -= HandleBossDeathFinished;
            boss = null;
        }
    }
}
