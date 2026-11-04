using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class SkeletonAttack1State : SkeletonAttackState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not Skeleton skeleton || enemyBoard == null)
            return;

        EnterToken(owner.DestroyToken);
        AttackAsync(skeleton, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(Skeleton skeleton, CancellationToken ctk)
    {
        try
        {
            //bool isCompleted = await skeleton.AttackAsync(skeleton.Attack1Data, ctk);

            //if (isCompleted)
            //    TransitionState(typeof(EnemyTraceState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
