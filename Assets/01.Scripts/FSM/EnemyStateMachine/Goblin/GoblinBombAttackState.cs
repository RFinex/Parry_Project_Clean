using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GoblinBombAttackState : EnemyBaseState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not Goblin goblin || enemyBoard == null)
            return;

        EnterToken(owner.DestroyToken);
        AttackAsync(goblin, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(Goblin goblin, CancellationToken ctk)
    {
        try
        {
            bool isCompleted = await goblin.AttackAsync(goblin.BombAttackData, ctk);

            if (isCompleted)
                TransitionState(typeof(EnemyTraceState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
