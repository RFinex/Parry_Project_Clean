using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BlazefurySword : Artifact
{
    private float attackTime = 2f;
    private float shockWaveDamageRate = 0.2f;

    private float effectRadius = 3f;
    private bool isAttackReady;

    private List<Collider2D> hits = new();
    private readonly HashSet<EnemyController> hitEnemies = new();
    private ContactFilter2D enemyFilter;

    private CancellationTokenSource token;

    public BlazefurySword()
    {
        // 대시 후 2초 이내 첫 일반 공격 적중 시, 적중한 적의 위치에서 일반 공격 피해의 20%만큼 피해를 입히는 충격파 발생.
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


        board.skill.OnDashEnded -= OnDashEnded;
        board.combat.OnAttackHitTargets -= ApplyShockwave;
        board.skill.OnDashEnded += OnDashEnded;
        board.combat.OnAttackHitTargets += ApplyShockwave;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.skill.OnDashEnded -= OnDashEnded;
        board.combat.OnAttackHitTargets -= ApplyShockwave;

        EndEffect();

        hits.Clear();
        hitEnemies.Clear();

        base.OnUnequip(board);
    }

    private void OnDashEnded()
    {
        CancellationTokenSource previousToken = token;

        token = token.LinkTokenOnly(blackBoard.owenrToken.Token);
        previousToken.Cancel_DisposeToken();

        isAttackReady = true;
        AttackTimeAsync(token.Token).Forget();
    }

    private async UniTaskVoid AttackTimeAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(attackTime), cancellationToken: ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (token != null && token.Token == ctk)
                EndEffect();
        }
    }

    private void ApplyShockwave(IReadOnlyList<EnemyController> targetEnemies, float attackDamage)
    {
        if (!isAttackReady)
            return;

        EndEffect();

        float damage = attackDamage * shockWaveDamageRate;

        foreach (EnemyController targetEnemy in targetEnemies)
        {
            if (targetEnemy == null)
                continue;

            hits.Clear();
            hitEnemies.Clear();

            int hitCount = Physics2D.OverlapCircle(targetEnemy.transform.position, effectRadius, enemyFilter, hits);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hits[i];

                if (!hit.TryGetComponent<EnemyController>(out EnemyController enemy))
                    enemy = hit.GetComponentInParent<EnemyController>();

                if ((enemy == null || enemy.Stats.IsDead))
                    continue;

                if (!hitEnemies.Add(enemy))
                    continue;

                enemy.TakeDamage(damage);
            }
        }
    }

    private void EndEffect()
    {
        isAttackReady = false;

        CancellationTokenSource previousToken = token;
        token = null;

        previousToken?.Cancel_DisposeToken();
    }
}
