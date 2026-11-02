using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;

public class Mushroom : EnemyController
{
    [Header("Attack Data")]
    [SerializeField] private MushroomAttackData attack1Data;
    [SerializeField] private MushroomAttackData attack2Data;

    [Header("Warning Effect")]
    [SerializeField] private Transform warningPos;
    [SerializeField] private int guardOnlyWarningId;
    [SerializeField] private int parryOnlyWarningId;
    [SerializeField] private int neitherWarningId;

    public MushroomAttackData Attack1Data => attack1Data;
    public MushroomAttackData Attack2Data => attack2Data;

#if UNITY_EDITOR
    [SerializeField, Min(0f)] private float attackGizmoDuration = 0.2f;

    private Vector2 attackGizmoCenter;
    private Vector2 attackGizmoSize;
    private float attackGizmoEndTime;
#endif

    protected override void AddStates()
    {
        base.AddStates();

        EnemyAddState<MushroomAttack1State>();
        EnemyAddState<MushroomAttack2State>();
    }

    public async UniTask AttackAsync(MushroomAttackData attackData, CancellationToken ctk)
    {
        EffectBase warning = null;

        try
        {
            StopMove();
            LookAtTarget();

            if (!blackBoard.animator.PlayFromStart(attackData.AnimType))
                return;

            if (attackData.WarningDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDelay), cancellationToken: ctk);
            }

            int warningId = 0;

            if (!attackData.CanGuard && !attackData.CanParry)
                warningId = neitherWarningId;
            else if (attackData.CanGuard && !attackData.CanParry)
                warningId = guardOnlyWarningId;
            else if (!attackData.CanGuard && attackData.CanParry)
                warningId = parryOnlyWarningId;

            if (warningId != 0 && warningPos != null)
            {
                warning = ObjectPoolManager.Instance.Get<EffectBase>(warningId, warningPos.position);
            }

            if (attackData.WarningDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackData.WarningDuration), cancellationToken: ctk);
            }

            warning = null;

            Vector2 offset = attackData.HitboxOffset;
            offset.x *= FrontDir;

            Vector2 hitboxPos = (Vector2)transform.position + offset;

#if UNITY_EDITOR
            attackGizmoCenter = hitboxPos;
            attackGizmoSize = attackData.HitboxSize;
            attackGizmoEndTime = Time.time + attackGizmoDuration;
#endif


            Collider2D hit = Physics2D.OverlapBox(hitboxPos, attackData.HitboxSize, 0f, blackBoard.playerLayer);

            if (hit != null)
            {
                blackBoard.combat.Attack(hit.transform, this, attackData.Damage, attackData.CanGuard, attackData.CanParry);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(attackData.AttackCooldown), cancellationToken: ctk);
        }
        catch (OperationCanceledException)
        {

        }
    }

    public override Type GetAttackStateType()
    {
        return typeof(MushroomAttack1State);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || attackGizmoEndTime <= Time.time)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackGizmoCenter, attackGizmoSize);
    }
#endif
}
