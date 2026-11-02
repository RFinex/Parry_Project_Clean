using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyTraceState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (blackBoard is EnemyBlackBoard board)
        {
            enemyBoard = board;
            enemyBoard.animator.Play(EnemyAnimationType.Move);
        }

        EnterToken(owner.DestroyToken);

        UpdateAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        owner.StopMove();

        ExitToken();
    }

    private async UniTaskVoid UpdateAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);

            while (!ctk.IsCancellationRequested)
            {
                if (owner.Target == null)
                {
                    owner.ClearTarget();
                    TransitionState(typeof(EnemyPatrolState));
                    return;
                }

                if (owner.IsTargetInAttackRange())
                {
                    owner.StopMove();
                    TransitionState(owner.GetAttackStateType());
                    return;
                }

                if (!owner.IsTargetInTraceRange())
                {
                    owner.ClearTarget();
                    owner.StopMove();
                    TransitionState(typeof(EnemyPatrolState));
                    return;
                }

                owner.LookAtTarget();

                owner.Move(owner.Target.position);

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
