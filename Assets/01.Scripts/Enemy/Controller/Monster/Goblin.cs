using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Goblin : NormalEnemyController
{
    [SerializeField] private GoblinAttackData normalAttackData;
    [SerializeField] private GoblinBombAttackData bombAttackData;

    public GoblinAttackData NormalAttackData => normalAttackData;
    public GoblinBombAttackData BombAttackData => bombAttackData;

    private bool isBombAttackReady = true;

    public bool IsBombAttackReady => isBombAttackReady;

    private CancellationTokenSource coolToken;

    public bool IsTargetFarEnoughBomb()
    {
        if (Target == null || bombAttackData == null)
            return false;

        float minDistance = bombAttackData.MinThrowDistance;
        float distance = ((Vector2)Target.position - (Vector2)transform.position).sqrMagnitude;

        return distance >= minDistance * minDistance;
    }

    protected override void AddStates()
    {
        base.AddStates();
        EnemyAddState<GoblinNormalAttackState>();
        EnemyAddState<GoblinBombAttackState>();

    }

    public override Type GetAttackStateType()
    {
        if (Target == null || normalAttackData == null || bombAttackData == null)
            return null;

        float distanceSqr = ((Vector2)Target.position - (Vector2)transform.position).sqrMagnitude;

        float normalRangeSqr = normalAttackData.NormalAttackRange * normalAttackData.NormalAttackRange;

        float minBombDistanceSqr = bombAttackData.MinThrowDistance * bombAttackData.MinThrowDistance;

        if (distanceSqr <= normalRangeSqr)
            return typeof(GoblinNormalAttackState);

        if(distanceSqr >= minBombDistanceSqr && isBombAttackReady)
            return typeof(GoblinBombAttackState);

        return null;
    }

    protected override void ApplyAttackHit(EnemyAttackData attackData, Vector2 hitboxPos)
    {
        if (attackData is not GoblinBombAttackData bombData)
        {
            base.ApplyAttackHit(attackData, hitboxPos);
            return;
        }

        if (Target == null || ObjectPoolManager.Instance == null)
            return;

        Vector2 spawnPos = (Vector2)transform.position + new Vector2(bombData.BombSpawnOffset.x * FrontDir, bombData.BombSpawnOffset.y);

        GoblinBomb bomb = ObjectPoolManager.Instance.Get<GoblinBomb>(bombData.BombPoolId, spawnPos);

        if (bomb == null)
            return;

        Vector2 targetPos = (Vector2)Target.position + bombData.TargetOffset;
        EnemyAttackInfo info = new(bombData.Damage, this, bombData.CanGuard, bombData.CanParry);

        bomb.ShootAt(targetPos, info);
        BombAttackCooldownAsync(DestroyToken).Forget();
    }

    private async UniTaskVoid BombAttackCooldownAsync(CancellationToken ctk)
    {
        try
        {
            isBombAttackReady = false;

            await UniTask.Delay(TimeSpan.FromSeconds(bombAttackData.AttackCooldown), cancellationToken: ctk);

            if (!ctk.IsCancellationRequested)
                isBombAttackReady = true;
        }
        catch (OperationCanceledException)
        {

        }
    }

    protected override void StartAttackCooldown(EnemyAttackData attackData)
    {
        if (attackData is GoblinBombAttackData)
            return;

        base.StartAttackCooldown(attackData);
    }

    protected override void ResetPoolState()
    {
        base.ResetPoolState();
        isBombAttackReady = true;
    }
}
