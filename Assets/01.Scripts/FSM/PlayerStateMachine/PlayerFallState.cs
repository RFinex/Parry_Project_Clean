using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerFallState : PlayerBaseState
{
    private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        LandingCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        owner.StopMove();

        ExitToken();
    }    

    public async UniTaskVoid LandingCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested && board.movement.isGround)
            {
                owner.Move();

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }

            while (!ctk.IsCancellationRequested)
            {
                owner.Move();

                if (board.movement.isGround && board.movement.verticalVelocity <= 0f)
                {
                    if (board.movement.moveInput.x != 0f)
                        TransitionState(typeof(PlayerMoveState));
                    else
                        TransitionState(typeof(PlayerIdleState));

                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
