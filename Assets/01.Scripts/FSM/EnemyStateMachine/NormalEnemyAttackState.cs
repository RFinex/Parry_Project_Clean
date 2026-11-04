using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class NormalEnemyAttackState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not NormalEnemyController enemy || enemyBoard == null)
            return;

        // 공격 데이터 배열 
        EnemyAttackData[] attackDatas = enemyBoard.attackDatas;

        if (attackDatas == null || attackDatas.Length == 0)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        // 일반 공격 데이터만 탐색
        int count = 0;

        for (int i = 0; i < attackDatas.Length; i++)
        {
            if (attackDatas[i] != null && attackDatas[i] is not EnemyProjectileAttackData)
                count++;
        }

        if (count == 0)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, count);
        int currentCount = 0;
        EnemyAttackData attackData = null;

        for (int i = 0; i < attackDatas.Length; i++)
        {
            if (attackDatas[i] is EnemyProjectileAttackData)
                continue;

            if (currentCount == randomIndex)
            {
                attackData = attackDatas[randomIndex];
                break;
            }

            currentCount++;
        }

        if (attackData == null)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        enemyBoard.currentAttackData = attackData;

        EnterToken(owner.DestroyToken);
        AttackAsync(enemy, attackData, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(NormalEnemyController owner, EnemyAttackData attackData, CancellationToken ctk)
    {
        try
        {
            owner.StopMove();
            owner.LookAtTarget();

            if (!enemyBoard.animator.PlayFromStart(attackData.AnimType))
                return;

            if (attackData.WarningDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDelay), cancellationToken: ctk);
            }

            owner.ShowAttackWarning(attackData.CanGuard, attackData.CanParry);

            if (attackData.WarningDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDuration), cancellationToken: ctk);
            }

            Vector2 offset = attackData.HitboxOffset;
            offset.x *= owner.FrontDir;

            Vector2 hitboxPos = (Vector2)owner.transform.position + offset;

            owner.StartAttackCooldown(attackData);
            owner.ApplyAttackHit(attackData, hitboxPos);

            if (attackData.AttackRecovery > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.AttackRecovery), cancellationToken: ctk);
            }

            enemyBoard.animator.PlayFromStart(EnemyAnimationType.Idle);
            TransitionState(typeof(EnemyTraceState));
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (enemyBoard.currentAttackData == attackData)
                enemyBoard.currentAttackData = null;
        }
    }
}
