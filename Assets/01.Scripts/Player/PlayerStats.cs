using UnityEngine;

public class PlayerStats
{
    private float nowHp;
    private float maxHp;

    private float nowStamina;
    private float maxStamina;

    private float invincibleTime;

    public float NowHp => nowHp;
    public float MaxHp => maxHp;
    public float NowStamina => nowStamina;
    public float MaxStamina => maxStamina;
    public float InvincibleTime => invincibleTime;


    public bool IsDead => nowHp <= 0f;
    
    public PlayerStats(PlayerBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxStamina = data.MaxStamina;
        nowStamina = maxStamina;

        invincibleTime = data.InvincibleTime;
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
}
