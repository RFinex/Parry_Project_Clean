using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerIdleState : PlayerBaseState
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

        InputCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid InputCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                if (playerBoard.movement.moveInput.x != 0f)
                {
                    TransitionState(typeof(PlayerMoveState));
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
    //        case PlayerInput.JumpPressed:
    //            TryJump();
    //            break;

    //        case PlayerInput.DashPressed:
    //            TryDash();
    //            break;

    //        case PlayerInput.AttackPressed:
    //            TryAttackOrExecute();
    //            break;

    //        case PlayerInput.GuardPressed:
    //            TryGuard();
    //            break;

    //        default:
    //            break;
    //    }
    //}
}
