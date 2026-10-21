using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class EnemyIdleState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (!owner.UseStateMachineBehavior)
        {
            owner.StopMove();
            return;
        }

        owner.StopMove();

        EnterToken(owner.DestroyToken);

        UpdateAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid UpdateAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {
            float detectInterval = 0.1f;

            if (owner.TryDetectTarget())
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            float remainingIdleTime = UnityEngine.Random.Range(owner.Data.MinIdleTime, owner.Data.MaxIdleTime);

            while (remainingIdleTime > 0f)
            {
                float waitTime = detectInterval < remainingIdleTime ? detectInterval : remainingIdleTime;
                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ctk);
                remainingIdleTime -= waitTime;

                // 타겟이 감지됐는지 0.1초마다 검사
                if (owner.TryDetectTarget())
                {
                    TransitionState(typeof(EnemyTraceState));
                    return;
                }
            }

            TransitionState(typeof(EnemyPatrolState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
