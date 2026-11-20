using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerGuardState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(playerBoard.owenrToken.Token);

        owner.StopMove();

        playerBoard.animator.PlayGuard();

        playerBoard.combat.GuardStart();

        ParryTimerAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        playerBoard.animator.StopGuard();

        playerBoard.combat.GuardEnd();

        ExitToken();
    }

    private async UniTaskVoid ParryTimerAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(playerBoard.combat.ParryTime), cancellationToken: ctk);

            playerBoard.combat.ParryEnd();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
