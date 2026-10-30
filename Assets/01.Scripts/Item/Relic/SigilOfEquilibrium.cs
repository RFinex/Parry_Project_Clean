using System.Collections.Generic;
using UnityEngine;

public class SigilOfEquilibrium : Artifact
{
    private float healRate = 0.1f;
    private float shockwaveDamageRate = 0.5f;
    private float shockwaveRadius = 3f;

    private List<Collider2D> hits = new();
    private HashSet<EnemyController> hitEnemies = new();

    private ContactFilter2D enemyFilter;

    public SigilOfEquilibrium()
    {
        // 일반 공격의 HP 피해가 0이 되고, 원래 피해의 50%를 균형 피해로 전환
        // 패리 성공 시 패리 균형 피해 50%의 충격파 발생
        // 처형 성공 시 최대 HP의 10 % 회복
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(board.data.EnemyLayer);
        enemyFilter.useLayerMask = true;

        board.combat.SetNormalAttackBalanceConversion(true);

        board.combat.OnParrySucceed -= ApplyShockwave;
        board.combat.OnExecuteSucceed -= Heal;
        board.combat.OnParrySucceed += ApplyShockwave;
        board.combat.OnExecuteSucceed += Heal;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnParrySucceed -= ApplyShockwave;
        board.combat.OnExecuteSucceed -= Heal;

        board.combat.SetNormalAttackBalanceConversion(false);

        base.OnUnequip(board);
    }

    public void ApplyShockwave()
    {
        Vector2 center = blackBoard.playerTransform.position;
        float damage = blackBoard.stats.GetStats(StatId.ParryDamage) * shockwaveDamageRate;

        hits.Clear();
        hitEnemies.Clear();

        int hitCount = Physics2D.OverlapCircle(center, shockwaveRadius, enemyFilter, hits);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hits[i];

            if (!hit.TryGetComponent<EnemyController>(out EnemyController enemy))
                enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy == null || enemy.Stats.IsDead || !hitEnemies.Add(enemy))
                continue;

            enemy.TakeBalanceDamage(damage);
        }
    }

    public void Heal()
    {
        float value = blackBoard.stats.MaxHp * healRate;
        blackBoard.stats.RestoreHp(value);
    }
}
