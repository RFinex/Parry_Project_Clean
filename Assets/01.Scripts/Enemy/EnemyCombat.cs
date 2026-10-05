using UnityEngine;

public class EnemyCombat
{
    private EnemyBaseData data;

    private float attackDamage => data.AttackDamage;

    public void Initialized(EnemyBaseData data)
    {
        this.data = data;
    }

    public virtual void Attack(Transform target)
    {
        if (target == null)
            return;

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        player.TakeDamage(attackDamage);

        Utils.Log<EnemyCombat>($"Àû °ø°Ý : {target.name}");
    }
}
