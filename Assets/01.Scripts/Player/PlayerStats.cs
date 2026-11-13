using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerStats
{
    private PlayerBlackBoard playerBoard;

    private readonly Dictionary<StatId, float> baseStats = new();
    private readonly Dictionary<StatId, float> currentStats = new();
    private readonly Dictionary<StatId, float> percentStats = new();

    private float nowHp;
    private float nowStamina;

    private float invincibleTimer;
    private bool isDamageInvincible;
    private bool isDashInvincible;
    private bool isStaminaBroken;

    public float NowHp => nowHp;
    public float MaxHp => GetStats(StatId.MaxHp);
    public float NowStamina => nowStamina;
    public float MaxStamina => GetStats(StatId.MaxStamina);

    public bool IsDead => nowHp <= 0f;
    public bool IsInvincible => isDamageInvincible || isDashInvincible;
    public float InvincibleTime => GetStats(StatId.InvincibleTime);
    public bool IsStaminaBroken => isStaminaBroken;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<PlayerBlackBoard> OnHealthHalf;
    public event Action OnDamaged;
    public event Action OnStaminaBroken;
    public event Action OnStaminaRestore;

    private CancellationTokenSource staminaToken;
    
    public PlayerStats(PlayerBaseData data)
    {
        SetBaseStats(StatId.AttackDamage, data.AttackDamage);
        SetBaseStats(StatId.MaxHp, data.MaxHp);
        SetBaseStats(StatId.MaxStamina, data.MaxStamina);
        SetBaseStats(StatId.MoveSpeed, data.MoveSpeed);
        SetBaseStats(StatId.JumpForce, data.JumpForce);
        SetBaseStats(StatId.DashTime, data.DashTime);
        SetBaseStats(StatId.DashSpeed, data.DashSpeed);
        SetBaseStats(StatId.DashCooldown, data.DashCooldown);
        SetBaseStats(StatId.AttackRange, data.AttackRange);
        SetBaseStats(StatId.ParryTime, data.ParryTime);
        SetBaseStats(StatId.ParryDamage, data.ParryDamage);
        SetBaseStats(StatId.GuardStaminaCost, data.GuardStaminaCost);
        SetBaseStats(StatId.InvincibleTime, data.InvincibleTime);
        SetBaseStats(StatId.DamageTakenMultiplier, data.DamageTakenMultiplier);
        SetBaseStats(StatId.DamageDealtMultiplier, data.DamageDealtMultiplier);
        SetBaseStats(StatId.DashStaminaCost, data.DashStaminaCost);
        SetBaseStats(StatId.StaminaRestorePerSec, data.StaminaRestorePerSec);
        SetBaseStats(StatId.StaminaRestorePerSecAllUse, data.StaminaRestorePerSecAllUse);
        SetBaseStats(StatId.StaminaRestoreDelay, data.StaminaRestoreDelay);

        SetCurrentStats();

        nowHp = MaxHp;
        nowStamina = MaxStamina;
    }

    public void Initialized(PlayerBlackBoard board)
    {
        playerBoard = board;
    }

    public float GetStats(StatId id)
    {
        if (!currentStats.TryGetValue(id, out float value))
            return 0f;

        // 배율 없으면 1배율
        float multiplier = percentStats.TryGetValue(id, out float percent) ? percent : 1f;

        float result = value * multiplier;

        result = (float)Math.Round(result, 2);

        float min = (id == StatId.MaxHp) ? 1f : 0f;

        return Mathf.Max(min, result);
    }

    private void SetBaseStats(StatId id, float value)
    {
        baseStats[id] = value;
    }

    private void SetCurrentStats()
    {
        foreach (var pair in baseStats)
            currentStats[pair.Key] = pair.Value;
    }

    public void AddStats(StatPair pair)
    {
        float previousMaxHp = MaxHp;
        float previousMaxStamina = MaxStamina;

        ApplyChangeStats(pair);
        RefreshHp_Stamina(previousMaxHp, previousMaxStamina);
    }

    public void RemoveStats(StatPair pair)
    {
        float previousMaxHp = MaxHp;
        float previousMaxStamina = MaxStamina;

        RemoveChangeStats(pair);
        RefreshHp_Stamina(previousMaxHp, previousMaxStamina);
    }       

    private void ApplyChangeStats(StatPair pair)
    {
        if (!currentStats.TryGetValue(pair.id, out float value))
            return;

        if (pair.modifierType == StatModifierType.Flat)
        {
            currentStats[pair.id] = value + pair.value;
            return;
        }

        if (pair.modifierType == StatModifierType.Percent)
        {
            float itemMultiplier = 1f + pair.value / 100f;

            if (itemMultiplier <= 0f)
            {
                Utils.LogWarning<PlayerStats>($"{pair.id} 퍼센트 효과는 -100%보다 커야 함");
                return;
            }

            float multiplier = percentStats.TryGetValue(pair.id, out float percent) ? percent : 1f;

            // 기존 배율 x 아이템 배율(곱연산)
            percentStats[pair.id] = multiplier * itemMultiplier;
        }
    }

    private void RemoveChangeStats(StatPair pair)
    {
        if (!currentStats.TryGetValue(pair.id, out float value))
            return;

        if (pair.modifierType == StatModifierType.Flat)
        {
            currentStats[pair.id] = value - pair.value;
            return;
        }

        if (pair.modifierType == StatModifierType.Percent)
        {
            float itemMultiplier = 1f + pair.value / 100f;

            if (itemMultiplier <= 0f)
                return;

            if (!percentStats.TryGetValue(pair.id, out float percent))
                return;

            percentStats[pair.id] = percent / itemMultiplier;
        }
    }

    private void RefreshHp_Stamina(float previousMaxHp, float previousMaxStamina)
    {
        float previousHp = nowHp;
        float previousStamina = nowStamina;

        nowHp = Mathf.Clamp(nowHp, 0f, MaxHp);
        nowStamina = Mathf.Clamp(nowStamina, 0f, MaxStamina);

        if (previousMaxHp != MaxHp || previousHp != nowHp)
            OnHealthChanged?.Invoke(nowHp, MaxHp);

        if (previousMaxStamina != MaxStamina || previousStamina != nowStamina)
            OnStaminaChanged?.Invoke(nowStamina, MaxStamina);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        damage *= GetStats(StatId.DamageTakenMultiplier);

        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0f;

        Utils.Log<PlayerStats>($"남은 체력 : {nowHp}/{MaxHp}");
        OnHealthChanged?.Invoke(nowHp, MaxHp);
        OnDamaged?.Invoke();
    }

    public bool TakeStaminaDamage(float damage)
    {
        if (IsDead)
            return false;

        nowStamina -= damage;

        // 이번 대미지에 스테미너가 파괴되었는지 확인
        bool staminaBrokenThisHit = nowStamina <= 0f;

        if (staminaBrokenThisHit)
        {
            nowStamina = 0f;
            isStaminaBroken = true;
            OnStaminaBroken?.Invoke();
        }

        OnStaminaChanged?.Invoke(nowStamina, MaxStamina);

        // 스테미너가 파괴된 상태일 경우 더 느린 회복량으로 회복됨
        float perSec = isStaminaBroken ? GetStats(StatId.StaminaRestorePerSecAllUse) : GetStats(StatId.StaminaRestorePerSec);

        RestartStaminaRestore(perSec);

        return staminaBrokenThisHit;
    }

    public bool UseStamina(float stamina)
    {
        stamina = stamina < 0f ? 0f : stamina;

        if (nowStamina < stamina)
        {
            Utils.Log<PlayerStats>("스테미너 부족");
            // 스테미너 게이지 흔들림
            return false;
        }

        nowStamina -= stamina;
        OnStaminaChanged?.Invoke(nowStamina, MaxStamina);

        // 스테미너 사용은 피격을 받는게 아니므로 회복량 그대로 사용
        RestartStaminaRestore(GetStats(StatId.StaminaRestorePerSec));

        return true;
    }

    public void RestoreHp(float value)
    {
        if (IsDead)
            return;

        nowHp += value;

        if (nowHp >= MaxHp)
            nowHp = MaxHp;

        Utils.Log<PlayerController>($"현재 체력 : {nowHp}/{MaxHp}");
        OnHealthChanged?.Invoke(nowHp, MaxHp);
    }

    public void RestoreStamina(float value)
    {
        nowStamina += value;

        if(nowStamina >= MaxStamina)
            nowStamina = MaxStamina;

        OnStaminaChanged?.Invoke(nowStamina, MaxStamina);
    }

    public void RestartStaminaRestore(float perSec)
    {
        staminaToken = staminaToken.LinkToken(playerBoard.owenrToken.Token);
        RestoreStaminaAsync(perSec, staminaToken.Token).Forget();
    }

    private async UniTaskVoid RestoreStaminaAsync(float restorePerSec, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(GetStats(StatId.StaminaRestoreDelay)), cancellationToken: ctk); 

            while (!ctk.IsCancellationRequested && nowStamina < MaxStamina)
            {
                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);

                RestoreStamina(restorePerSec * Time.deltaTime);
            }

            if (!ctk.IsCancellationRequested && isStaminaBroken && nowStamina >= MaxStamina)
            {
                isStaminaBroken = false;
                OnStaminaRestore?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    public void SetDashInvincible(bool invincible)
    {
        isDashInvincible = invincible;
    }

    public void StartInvincible()
    {
        invincibleTimer = InvincibleTime;
        isDamageInvincible = InvincibleTime > 0f;
    }

    /// <summary>
    /// 피격 후 무적 타이머
    /// </summary>
    /// <param name="deltaTime"> Time.deltaTime </param>
    public void UpdateInvincible(float deltaTime)
    {
        if (!isDamageInvincible)
            return;

        invincibleTimer -= deltaTime;

        if (invincibleTimer <= 0f)
            EndInvincible();
    }

    public void EndInvincible()
    {
        isDamageInvincible = false;
        invincibleTimer = 0f;
    }

    // 모든 체력 피해 증가(특수 공격 포함)
    public float CalculateAllHealthDamage(float damage)
    {
        return damage * GetStats(StatId.DamageDealtMultiplier);
    }
}
