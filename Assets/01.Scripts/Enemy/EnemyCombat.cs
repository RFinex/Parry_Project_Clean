using UnityEngine;

public class EnemyCombat
{
    private EnemyBaseData data;

    private float AttackDamage => data.AttackDamage;

    public void Initialized(EnemyBaseData data)
    {
        this.data = data;
    }

    public virtual void Attack(Transform target)
    {
        if (target == null)
            return;

        Utils.Log<EnemyCombat>($"Рћ АјАн : {target.name}");
    }
}
