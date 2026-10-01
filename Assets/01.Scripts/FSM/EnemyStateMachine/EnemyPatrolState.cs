using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
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
            Vector2 targetPos = owner.GetRandomPatrolPos();

            while (!ctk.IsCancellationRequested)
            {
                if (owner.TryDetectTarget())
                {
                    owner.StopMove();
                    TransitionState(typeof(EnemyTraceState));
                    return;
                }

                bool goal = owner.Move(targetPos);

                if (goal)
                {
                    TransitionState(typeof(EnemyIdleState));
                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
