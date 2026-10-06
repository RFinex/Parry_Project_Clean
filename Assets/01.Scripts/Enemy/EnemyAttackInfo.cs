using UnityEngine;

public struct EnemyAttackInfo
{
    public float attackDamage;
    public EnemyController attacker;

    public EnemyAttackInfo(float attackDamage, EnemyController attacker)
    {
        this.attackDamage = attackDamage;
        this.attacker = attacker;
    }
}
