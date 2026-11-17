using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        enemyBoard.animator.Play(EnemyAnimationType.Move);

        UpdateAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        owner.StopMove();

        ExitToken();
    }

    private async UniTaskVoid UpdateAsync(EnemyController owner, CancellationToken ctk)
    {
        if (owner == null)
            return;

        try
        {
            Vector2 targetPos = owner.GetRandomPatrolPos();
            float arrivalDistanceSqr = owner.Data.ArrivalDistance * owner.Data.ArrivalDistance;

            while (!ctk.IsCancellationRequested)
            {
                if (owner.TryDetectTarget())
                {
                    owner.StopMove();
                    TransitionState(typeof(EnemyTraceState));
                    return;
                }

                Vector2 ownerPos = owner.transform.position;
                float distanceX = targetPos.x - ownerPos.x;

                if (distanceX * distanceX > arrivalDistanceSqr)
                {
                    float direction = Mathf.Sign(distanceX);

                    bool wallAhead = owner.IsWallAhead(direction);
                    bool noGroundAhead = !owner.IsGroundAhead(direction);

                    // 만약 현재 진행 방향에 땅이 없거나 벽이 있을 경우 방향으로 정찰 위치 재설정
                    if (wallAhead || noGroundAhead)
                    {
                        targetPos = owner.GetRandomPatrolPos(-direction);
                        distanceX = targetPos.x - ownerPos.x;
                    }
                }

                //float direction = Mathf.Sign(targetPos.x - owner.transform.position.x);


                // 만약 현재 진행 방향에 땅이 감지되지 않을 경우 반대 방향으로 정찰 위치 재설정
                //if (Mathf.Abs(targetPos.x - owner.transform.position.x) > owner.Data.ArrivalDistance)
                //{
                //    bool wallAhead = owner.IsWallAhead(direction);
                //    bool noGroundAhead = !owner.IsGroundAhead(direction);

                //    if (wallAhead || noGroundAhead)
                //        targetPos = owner.GetRandomPatrolPos(-direction);
                //}

                if (owner.Move(targetPos))
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
