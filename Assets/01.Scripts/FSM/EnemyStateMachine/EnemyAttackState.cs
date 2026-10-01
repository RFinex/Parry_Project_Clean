using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        owner.StopMove();

        owner.LookAtTarget();

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
                    owner.ClearTarget();
                    owner.ChangeState<EnemyIdleState>();
                    return;
                }

                if (!owner.IsTargetInAttackRange())
                {
                    owner.ChangeState<EnemyTraceState>();
                    return;
                }

                owner.Attack();

                await UniTask.Delay(TimeSpan.FromSeconds(owner.Data.AttackCool), cancellationToken: ctk);

                if (owner.Target == null)
                {
                    owner.ClearTarget();
                    owner.ChangeState<EnemyIdleState>();
                    return;
                }

                if (!owner.IsTargetInAttackRange())
                {
                    owner.ChangeState<EnemyTraceState>();
                    return;
                }
            }
            
        }
        catch (OperationCanceledException)
        {

        }
    }
}
