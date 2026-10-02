using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private float dashCheckTimer;
    private float dashCoolTimer;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        this.board.animator.PlayDash();

        DashAsync(token.Token).Forget();
        this.board.skill.DashCoolTime(owner.DestroyToken).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid DashAsync(CancellationToken ctk)
    {
        try
        {
            dashCheckTimer = 0f;

            while (dashCheckTimer <= board.skill.dashTime)
            {
                dashCheckTimer += Time.deltaTime;

                board.rb.linearVelocity = new Vector2(board.movement.frontDir * board.skill.dashSpeed, 0f);

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
            Utils.Log<PlayerController>("대시 종료");
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (!board.movement.isGround)
                TransitionState(typeof(PlayerFallState));
            else if (board.movement.moveInput.x != 0f)
                TransitionState(typeof(PlayerMoveState));
            else
                TransitionState(typeof(PlayerIdleState));
        }
    }
}
