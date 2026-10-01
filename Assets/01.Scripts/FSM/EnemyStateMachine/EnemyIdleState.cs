using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class EnemyIdleState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
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
            await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);

            if (owner.TryDetectTarget())
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            float waitTime = UnityEngine.Random.Range(owner.Data.MinIdleTime, owner.Data.MaxIdleTime);

            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ctk);

            if (owner.TryDetectTarget())
            {
                TransitionState(typeof(EnemyTraceState));
                return;
            }

            TransitionState(typeof(EnemyPatrolState));            
        }
        catch (OperationCanceledException)
        {

        }
    }
}
