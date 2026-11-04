using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// [수정] 일반 적의 공통 공격 진행 및 쿨타임
public class NormalEnemyController : EnemyController
{
    [SerializeField] protected EnemyAttackData[] attackDatas;
    private bool isAttackReady = true;

    public override bool IsAttackReady => isAttackReady;
    protected override bool CanEnterHurtState => base.CanEnterHurtState && (blackBoard == null || blackBoard.currentAttackData == null || blackBoard.currentAttackData.CanInterrupted);

    private bool isProjectileAttackReady = true;
    public bool IsProjectileAttackReady => isProjectileAttackReady;

#if UNITY_EDITOR
    [SerializeField, Min(0f)] private float attackGizmoDuration = 0.2f;

    private Vector2 attackGizmoCenter;
    private Vector2 attackGizmoSize;
    private float attackGizmoEndTime;
#endif

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();
        blackBoard.attackDatas = attackDatas;
    }

    protected override void AddStates()
    {
        base.AddStates();
        EnemyAddState<NormalEnemyAttackState>();
        EnemyAddState<NormalEnemyProjectileAttackState>();
    }

    /// <summary>
    /// 거리 계산 후 상태 전환 판단
    /// </summary>
    /// <returns></returns>
    public override Type GetAttackStateType()
    {
        if (!IsTargetInAttackRange() || attackDatas == null)
            return null;

        for (int i = 0; i < attackDatas.Length; i++)
        {
            if (attackDatas[i] != null)
                return typeof(NormalEnemyAttackState);
        }

        return null;
    }

    public virtual void ApplyAttackHit(EnemyAttackData attackData, Vector2 hitboxPos)
    {
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
    }

    public virtual void StartAttackCooldown(EnemyAttackData attackData)
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

    public void StartProjectileAttackCooltime(EnemyAttackData attackData)
    {
        if (!isProjectileAttackReady || attackData == null)
            return;

        isProjectileAttackReady = false;
        ProjectileAttackCooldownAsync(attackData.AttackCooldown, DestroyToken).Forget();
    }

    private async UniTaskVoid ProjectileAttackCooldownAsync(float cooldown, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(cooldown), cancellationToken: ctk);

            isProjectileAttackReady = true;
        }
        catch (OperationCanceledException)
        {

        }
    }

    /// <summary>
    /// 사거리 별 공격 가능 여부 판단
    /// </summary>
    /// <param name="data"> 판단 대상 공격 데이터 </param>
    /// <returns></returns>
    public bool IsTargetInRange(EnemyAttackData data)
    {
        if (Target == null || data == null)
            return false;

        float distanceSqr = (Target.position - transform.position).sqrMagnitude;

        float minRangeSqr = data.MinAttackRange * data.MinAttackRange;
        float maxRangeSqr = data.MaxAttackRange * data.MaxAttackRange;

        return distanceSqr >= minRangeSqr && distanceSqr <= maxRangeSqr;
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

        if (blackBoard != null)
            blackBoard.currentAttackData = null;

        isAttackReady = true;
        isProjectileAttackReady = true;
    }
}
