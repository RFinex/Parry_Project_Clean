using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class NormalEnemyProjectileAttackState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not NormalEnemyController enemy || enemyBoard == null || enemyBoard.attackDatas == null || !enemy.IsProjectileAttackReady)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        EnemyAttackData[] attackDatas = enemyBoard.attackDatas;

        int count = 0;

        for (int i = 0; i < attackDatas.Length; i++)
        {
            if (attackDatas[i] is not EnemyProjectileAttackData projectileData)
                continue;

            if (enemy.IsTargetInRange(projectileData))
                count++;
        }

        if (count == 0)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, count);
        int currentIndex = 0;
        EnemyProjectileAttackData selectedData = null;

        for (int i = 0; i < attackDatas.Length; i++)
        {
            if (attackDatas[i] is not EnemyProjectileAttackData projectileData)
                continue;

            if (!enemy.IsTargetInRange(projectileData))
                continue;

            if (currentIndex == randomIndex)
            {
                selectedData = projectileData;
                break;
            }

            currentIndex++;
        }

        if (selectedData == null)
        {
            TransitionState(typeof(EnemyTraceState));
            return;
        }

        enemyBoard.currentAttackData = selectedData;

        EnterToken(owner.DestroyToken);
        AttackAsync(enemy, selectedData, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(NormalEnemyController owner, EnemyProjectileAttackData attackData, CancellationToken ctk)
    {
        enemyBoard.currentAttackData = attackData;

        try
        {
            owner.StopMove();
            owner.LookAtTarget();

            // 애니메이션 실행
            if (!enemyBoard.animator.PlayFromStart(attackData.AnimType))
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            if (attackData.WarningDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDelay), cancellationToken: ctk);
            }

            // 가드 경고 표시 실행
            owner.ShowAttackWarning(attackData.CanGuard, attackData.CanParry);

            if (attackData.WarningDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDuration), cancellationToken: ctk);
            }

            if (owner.Target == null || ObjectPoolManager.Instance == null)
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            // 투사체 생성 위치
            Vector2 spawnPos = (Vector2)owner.transform.position + new Vector2(attackData.ProjectileSpawnOffset.x * owner.FrontDir, attackData.ProjectileSpawnOffset.y);

            // 투사체 생성
            EnemyProjectileBase projectile = ObjectPoolManager.Instance.Get<EnemyProjectileBase>(attackData.ProjectilePoolId, spawnPos);

            if (projectile == null)
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            // 목표 지점
            Vector2 targetPos = (Vector2)owner.Target.position + attackData.TargetOffset;

            // 공격 정보
            EnemyAttackInfo info = new(attackData.Damage, attackData.StaminaDamage, owner, attackData.CanGuard, attackData.CanParry);

            // 목표 지점 발사
            projectile.ShootAt(targetPos, info);
            // 쿨타임 시작
            owner.StartProjectileAttackCooltime(attackData);

            // 후딜
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
