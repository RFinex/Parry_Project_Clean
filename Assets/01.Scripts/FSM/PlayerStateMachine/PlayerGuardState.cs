using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerGuardState : PlayerBaseState
{
    //private PlayerBlackBoard board;
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}

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
            await UniTask.Delay(TimeSpan.FromSeconds(playerBoard.combat.ParryWindow), cancellationToken: ctk);

            playerBoard.combat.ParryEnd();
        }
        catch (OperationCanceledException)
        {

        }
    }

    //public override void HandleInput(PlayerInput input)
    //{
    //    switch (input)
    //    {
    //        case PlayerInput.AttackPressed:
    //            TryAttackOrExecute();
    //            break;

    //        case PlayerInput.GuardReleased:
    //            ReleaseGuard();
    //            break;

    //        default:
    //            break;
    //    }
    //}
}
