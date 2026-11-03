using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// [수정] 일반 적의 공통 공격 진행 및 쿨타임
public class NormalEnemyController : EnemyController
{
    private EnemyAttackData currentAttackData;
    private bool isAttackReady = true;

    public override bool IsAttackReady => isAttackReady;
    protected override bool CanEnterHurtState =>
        base.CanEnterHurtState && (currentAttackData == null || currentAttackData.CanInterrupted);

#if UNITY_EDITOR
    [SerializeField, Min(0f)] private float attackGizmoDuration = 0.2f;

    private Vector2 attackGizmoCenter;
    private Vector2 attackGizmoSize;
    private float attackGizmoEndTime;
#endif

    public virtual async UniTask<bool> AttackAsync(EnemyAttackData attackData, CancellationToken ctk)
    {
        if (attackData == null || ctk.IsCancellationRequested)
            return false;

        currentAttackData = attackData;

        try
        {
            StopMove();
            LookAtTarget();

            if (!blackBoard.animator.PlayFromStart(attackData.AnimType))
                return false;

            if (attackData.WarningDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDelay), cancellationToken: ctk);
            }

            ShowAttackWarning(attackData.CanGuard, attackData.CanParry);

            if (attackData.WarningDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDuration), cancellationToken: ctk);
            }


            Vector2 offset = attackData.HitboxOffset;
            offset.x *= FrontDir;

            Vector2 hitboxPos = (Vector2)transform.position + offset;

#if UNITY_EDITOR
            attackGizmoCenter = hitboxPos;
            attackGizmoSize = attackData.HitboxSize;
            attackGizmoEndTime = Time.time + attackGizmoDuration;
#endif

            StartAttackCooldown(attackData);

            ApplyAttackHit(attackData, hitboxPos);                      

            await UniTask.Delay(TimeSpan.FromSeconds(attackData.AttackRecovery), cancellationToken: ctk);

            blackBoard.animator.PlayFromStart(EnemyAnimationType.Idle);

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            if (currentAttackData == attackData)
                currentAttackData = null;
        }
    }

    protected virtual void ApplyAttackHit(EnemyAttackData attackData, Vector2 hitboxPos)
    {
        Collider2D hit = Physics2D.OverlapBox(hitboxPos, attackData.HitboxSize, 0f, blackBoard.playerLayer);

        if (hit != null)
        {
            blackBoard.combat.Attack(hit.transform, this, attackData.Damage, attackData.CanGuard, attackData.CanParry);
        }
    }

    protected virtual void StartAttackCooldown(EnemyAttackData attackData)
    {
        AttackCooldownAsync(attackData, DestroyToken).Forget();
    }

    private async UniTaskVoid AttackCooldownAsync(EnemyAttackData attackData, CancellationToken ctk)
    {
        try
        {
            isAttackReady = false;
            await UniTask.Delay(TimeSpan.FromSeconds(attackData.AttackCooldown), cancellationToken: ctk);

            if (!ctk.IsCancellationRequested)
                isAttackReady = true;

        }
        catch (OperationCanceledException)
        {

        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || attackGizmoEndTime <= Time.time)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackGizmoCenter, attackGizmoSize);
    }
#endif

    protected override void ResetPoolState()
    {
        base.ResetPoolState();
        currentAttackData = null;
        isAttackReady = true;
    }
}
