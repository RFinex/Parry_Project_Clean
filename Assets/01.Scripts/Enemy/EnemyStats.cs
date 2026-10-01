using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private float nowHp;
    [SerializeField] private float maxHp;

    [SerializeField] private float nowBalance;
    [SerializeField] private float maxBalance;

    public void Initialized(EnemyBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxBalance = data.MaxBalance;
        nowBalance = maxBalance;
    }

    public void TakeDamage(float damage)
    {
        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0;
    }

    public void TakeBalanceDamage(float damage)
    {
        nowBalance -= damage;

        if(nowBalance <= 0)
            nowBalance = 0;
    }
}
