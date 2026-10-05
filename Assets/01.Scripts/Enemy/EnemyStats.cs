using UnityEngine;

public class EnemyStats
{
    private float nowHp;
    private float maxHp;

    public float NowHp => nowHp;
    public float MaxHp => maxHp;

    private float nowBalance;
    private float maxBalance;

    public float NowBalance => nowBalance;
    public float MaxBalance => maxBalance;

    public bool IsDead => nowHp <= 0;
    public bool IsBalanceBroken => nowBalance <= 0;

    public void Initialized(EnemyBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxBalance = data.MaxBalance;
        nowBalance = maxBalance;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0;
    }

    public void TakeBalanceDamage(float damage)
    {
        if (IsDead || IsBalanceBroken)
            return;

        nowBalance -= damage;

        if(nowBalance <= 0)
            nowBalance = 0;
    }

    public void RestoreBalance()
    {
        nowBalance = maxBalance;
    }
}
