using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EliteExecuteHurtState : EnemyBaseState
{
    private EnemyBlackBoard board;

    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is EnemyBlackBoard board)
        {
            this.board = board;
        }

        owner.StopMove();

        if (!this.board.animator.Play(EnemyAnimationType.Hurt))
        {
            TransitionState(typeof(BTState));
            return;
        }

        HurtAnimationAsync(token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    public async UniTaskVoid HurtAnimationAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate, ctk);

            await UniTask.WaitUntil(() => board.animator.Finish(EnemyAnimationType.Hurt), cancellationToken: ctk);

            TransitionState(typeof(BTState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
