using UnityEngine;

public class EnemyCombat
{
    private EnemyBaseData data;

    public void Initialized(EnemyBaseData data)
    {
        this.data = data;
    }

    /// <summary>
    /// 일반 공격 함수
    /// </summary>
    /// <param name="target"> 목표 </param>
    /// <param name="attacker"> 공격자 정보 </param>
    public virtual void Attack(Transform target, EnemyController attacker)
    {
        if (target == null)
            return;

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        EnemyAttackInfo attackInfo = new(data.AttackDamage, data.StaminaDamage, attacker, data.CanGuard, data.CanParry);

        player.TakeDamage(attackInfo);

        Utils.Log<EnemyCombat>($"적 공격 : {target.name}");
    }

    /// <summary>
    /// 스킬 공격 함수
    /// </summary>
    /// <param name="target"> 목표 </param>
    /// <param name="attacker">공격자 정보 </param>
    /// <param name="damage"> 스킬 데미지 </param>
    /// <param name="canGuard"> 스킬 가드 가능 </param>
    /// <param name="canParry"> 스킬 패리 가능 </param>
    public virtual void Attack(Transform target, EnemyController attacker, float damage, float staminaDamage, bool canGuard, bool canParry)
    {
        if (target == null)
            return;

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        EnemyAttackInfo attackInfo = new(damage, staminaDamage, attacker, canGuard, canParry);

        player.TakeDamage(attackInfo);
    }
}
