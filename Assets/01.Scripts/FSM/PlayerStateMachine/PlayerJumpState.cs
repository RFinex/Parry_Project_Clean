using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerJumpState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private bool isJumpCanceled;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        isJumpCanceled = false;


        LandingCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        owner.StopMove();

        ExitToken();
    }

    public void StartJump()
    {
        this.board.rb.linearVelocity = new UnityEngine.Vector2(this.board.rb.linearVelocity.x, this.board.movement.jumpForce);
    }

    public void JumpCanceled()
    {
        if (isJumpCanceled)
        {
            Utils.Log<PlayerJumpState>("점프 취소 불가능");
            return;
        }

        isJumpCanceled= true;

        if (board.movement.verticalVelocity > 0f)
        {
            Utils.Log<PlayerJumpState>("점프 취소 성공");
            board.rb.linearVelocity = new UnityEngine.Vector2(board.rb.linearVelocity.x, board.rb.linearVelocity.y * 0.4f);
        }
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
