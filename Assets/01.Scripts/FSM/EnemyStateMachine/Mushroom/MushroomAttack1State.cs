using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class MushroomAttack1State : MushroomAttackState
{
    public override void Enter(EnemyController owner)
    {
        if (owner is not Mushroom mushroom || enemyBoard == null)
            return;

        EnterToken(owner.DestroyToken);
        AttackAsync(mushroom, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(Mushroom mushroom, CancellationToken ctk)
    {
        try
        {
            bool isCompleted = await mushroom.AttackAsync(mushroom.Attack1Data, ctk);
            
            if (isCompleted)
                TransitionState(typeof(EnemyTraceState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
