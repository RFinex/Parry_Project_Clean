using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyHurtState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        enemyBoard.animator.PlayFromStart(EnemyAnimationType.Hurt);
        HitAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid HitAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);

            await UniTask.WaitUntil(() => enemyBoard.animator.Finish(EnemyAnimationType.Hurt), cancellationToken: ctk);

            if (owner.Target != null && owner.IsTargetInTraceRange())
                TransitionState(typeof(EnemyTraceState));
            else
                TransitionState(typeof(EnemyIdleState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
