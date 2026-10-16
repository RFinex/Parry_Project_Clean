using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    //private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}

        owner.Jump();

        playerBoard.animator.PlayJump();

        JumpCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid JumpCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                owner.Move();

                if (playerBoard.movement.verticalVelocity <= 0f)
                {
                    TransitionState(typeof(PlayerFallState));
                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    //public override void HandleInput(PlayerInput input)
    //{
    //    switch (input)
    //    {
    //        case PlayerInput.DashPressed:
    //            TryDash();
    //            break;

    //        case PlayerInput.AttackPressed:
    //            TryAttackOrExecute();
    //            break;

    //        case PlayerInput.GuardPressed:
    //            TryGuard();
    //            break;

    //        case PlayerInput.JumpReleased:
    //            if(playerBoard.rb.linearVelocity.y > 0f)
    //            {
    //                playerBoard.rb.linearVelocity = new Vector2(playerBoard.rb.linearVelocity.x, playerBoard.rb.linearVelocity.y * 0.4f);
    //            }
    //            break;

    //        default:
    //            break;
    //    }
    //}
}
