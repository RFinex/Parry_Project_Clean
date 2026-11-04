using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class Skeleton : NormalEnemyController
{
    [Header("Attack Data")]
    [SerializeField] private SkeletonAttackData attack1Data;
    [SerializeField] private SkeletonAttackData attack2Data;
    [SerializeField] private int maxAttackIndex = 2;

    public SkeletonAttackData Attack1Data => attack1Data;
    public SkeletonAttackData Attack2Data => attack2Data;

    private int attackIndex;

    protected override void AddStates()
    {
        base.AddStates();
        EnemyAddState<SkeletonAttack1State>();
        EnemyAddState<SkeletonAttack2State>();
    }

    public override async UniTask<bool> AttackAsync(EnemyAttackData attackData, CancellationToken ctk)
    {
        try
        {
            return await base.AttackAsync(attackData, ctk);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            if (attackIndex < maxAttackIndex)
                attackIndex++;
            else
                attackIndex = 0;            
        }
    }

    public override Type GetAttackStateType()
    {
        return attackIndex == maxAttackIndex ? typeof(MushroomAttack2State) : typeof(MushroomAttack1State);
    }

    protected override void ResetPoolState()
    {
        base.ResetPoolState();
        attackIndex = 0;
    }
}
