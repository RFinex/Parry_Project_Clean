using System.Collections.Generic;
using UnityEngine;

public class ChainOfExecutioner : Artifact
{
    private readonly HashSet<EnemyController> hitEnemies = new();
    private float effectRadius = 3f;

    public ChainOfExecutioner()
    {
        // 일반 공격 피해 -20% / 처형 성공 시 주변 적에게 기본 균형 피해의 100 % 만큼 균형 피해
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnExecuteArea -= ApplyBlalnceDamage;
        board.combat.OnExecuteArea += ApplyBlalnceDamage;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnExecuteArea -= ApplyBlalnceDamage;
        hitEnemies.Clear();

        base.OnUnequip(board);
    }

    public void ApplyBlalnceDamage(Vector2 position, IExecuteTarget target)
    {
        hitEnemies.Clear();

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, effectRadius, blackBoard.data.EnemyLayer);

        float damage = blackBoard.data.ParryDamage;

        if (target is EnemyController executedEnemy)
        {
            foreach (Collider2D hit in hits)
            {
                EnemyController enemy = hit.GetComponentInParent<EnemyController>();

                if (enemy == null || enemy.Stats.IsDead)
                    continue;

                if (enemy == executedEnemy)
                    continue;

                if (!hitEnemies.Add(enemy))
                    continue;

                enemy.TakeBalanceDamage(damage);
            }
        }        
    }
}
