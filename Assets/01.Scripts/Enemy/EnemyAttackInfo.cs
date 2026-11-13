using UnityEngine;

public struct EnemyAttackInfo
{
    public float attackDamage;
    public float staminaDamage;
    public EnemyController attacker;
    public bool canGuard;
    public bool canParry;
    public bool damageAttackerBalanceOnParry;

    public EnemyAttackInfo(float attackDamage, float staminaDamage, EnemyController attacker, bool canGuard, bool canParry) : this(attackDamage, staminaDamage, attacker, canGuard, canParry, true)
    {

    }

    public EnemyAttackInfo(float attackDamage, float staminaDamage, EnemyController attacker, bool canGuard, bool canParry, bool damageAttackerBalanceOnParry)
    {
        this.attackDamage = attackDamage;
        this.staminaDamage = staminaDamage;
        this.attacker = attacker;
        this.canGuard = canGuard;
        this.canParry = canParry;
        this.damageAttackerBalanceOnParry = damageAttackerBalanceOnParry;
    }
}
