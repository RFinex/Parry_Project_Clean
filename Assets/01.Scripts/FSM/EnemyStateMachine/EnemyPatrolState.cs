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

                float direction = Mathf.Sign(targetPos.x - owner.transform.position.x);

                // 만약 현재 진행 방향에 땅이 감지되지 않을 경우 반대 방향으로 정찰 위치 재설정
                if (Mathf.Abs(targetPos.x - owner.transform.position.x) > owner.Data.ArrivalDistance &&
                    !owner.HasGroundAhead(direction))
                {
                    targetPos = owner.GetRandomPatrolPos(-direction);
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
