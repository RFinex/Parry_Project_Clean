using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class EnemyAttackState : EnemyBaseState
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
            while (!ctk.IsCancellationRequested)
            {
                if (owner.Target == null)
                {
                    TransitionState(typeof(EnemyIdleState));
                    return;
                }

                if (!owner.IsTargetInAttackRange())
                {
                    TransitionState(typeof(EnemyTraceState));
                    return;
                }

                owner.LookAtTarget();

                owner.Attack();

                await UniTask.Delay(TimeSpan.FromSeconds(owner.Data.AttackCool), cancellationToken: ctk);
            }
            
        }
        catch (OperationCanceledException)
        {

        }
    }
}
