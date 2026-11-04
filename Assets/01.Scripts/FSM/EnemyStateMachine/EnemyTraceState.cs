using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyTraceState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        enemyBoard.animator.PlayFromStart(EnemyAnimationType.Move);

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
        bool isWaiting = false;

        try
        {
            while (!ctk.IsCancellationRequested)
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);

                if (owner.Target == null)
                {
                    owner.ClearTarget();
                    TransitionState(typeof(EnemyPatrolState));
                    return;
                }

                Type attackStateType = owner.GetAttackStateType();

                if (attackStateType != null)
                {
                    owner.StopMove();

                    TransitionState(attackStateType);
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

                bool targetX = owner.MoveToTarget(owner.Target.position);

                if (targetX)
                {
                    if (!isWaiting)
                    {
                        enemyBoard.animator.PlayFromStart(EnemyAnimationType.Idle);
                        isWaiting = true;
                    }
                }
                else if (isWaiting)
                {
                    enemyBoard.animator.PlayFromStart(EnemyAnimationType.Move);
                    isWaiting = false;
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
