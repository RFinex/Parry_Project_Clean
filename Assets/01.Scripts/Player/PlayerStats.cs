using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerStats
{
    private float nowHp;
    private float maxHp;

    private float nowStamina;
    private float maxStamina;

    private float invincibleTime;
    private float invincibleTimer;

    public float NowHp => nowHp;
    public float MaxHp => maxHp;
    public float NowStamina => nowStamina;
    public float MaxStamina => maxStamina;

    public bool IsDead => nowHp <= 0f;
    private bool isInvincible;
    public bool IsInvincible => isInvincible;
    public float InvincibleTime => invincibleTime;
    
    public PlayerStats(PlayerBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxStamina = data.MaxStamina;
        nowStamina = maxStamina;

        invincibleTime = data.InvincibleTime;
        invincibleTimer = invincibleTime;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0f;

        Utils.Log<PlayerStats>($"남은 체력 : {nowHp}/{maxHp}");
    }

    public bool UseStamina(float stamina)
    {
        if (nowStamina < stamina)
        {
            Utils.Log<PlayerStats>("스테미너 부족");
            return false;
        }

        nowStamina -= stamina;

        return true;
    }

    public void RestoreHp(float value)
    {
        nowHp += value;

        if (nowHp >= maxHp)
            nowHp = maxHp;
    }

    public void RestoreStamina(float value)
    {
        nowStamina += value;

        if(nowStamina >= maxStamina)
            nowStamina = maxStamina;
    }

    public void StartInvincible()
    {
        isInvincible = true;
        invincibleTimer = invincibleTime;
    }

    /// <summary>
    /// 피격 후 무적 타이머
    /// </summary>
    /// <param name="deltaTime"> Time.deltaTime </param>
    public void UpdateInvincible(float deltaTime)
    {
        if (!isInvincible)
            return;

        invincibleTimer -= deltaTime;

        if (invincibleTimer <= 0f)
        {
            invincibleTimer = 0f;
            EndInvincible();
        }
    }

    public void EndInvincible()
    {
        isInvincible = false;
        invincibleTimer = 0f;
    }
}
