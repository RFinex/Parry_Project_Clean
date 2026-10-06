
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

    private float staggerDuration;
    public float StaggerDuration => staggerDuration;

    public void Initialized(EnemyBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxBalance = data.MaxBalance;
        nowBalance = maxBalance;

        staggerDuration = data.StaggerDuration;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0;

        Utils.Log<EnemyStats>($"남은 체력 : {nowHp}/{maxHp}");
    }

    public void TakeBalanceDamage(float damage)
    {
        if (IsDead || IsBalanceBroken)
            return;

        nowBalance -= damage;

        if(nowBalance <= 0)
            nowBalance = 0;

        Utils.Log<EnemyStats>($"남은 밸런스 : {nowBalance}/{maxBalance}");
    }

    public void RestoreBalance(float value)
    {
        nowBalance += value;

        if (nowBalance >= maxBalance)
            nowBalance = maxBalance;
    }

    public void RestoreBalance()
    {
        nowBalance = maxBalance;

        Utils.Log<EnemyStats>($"밸런스 회복 : {nowBalance}/{maxBalance}");
    }

    public float RestoreBalancePerSec()
    {
        return maxBalance / staggerDuration;
    }
}
