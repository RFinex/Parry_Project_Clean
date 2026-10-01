using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class EnemyIdleState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        EnterToken();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    public async UniTaskVoid UpdateAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {

        }
        catch (OperationCanceledException)
        {

        }
    }
}
