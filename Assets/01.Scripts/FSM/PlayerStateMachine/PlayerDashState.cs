using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    //private PlayerBlackBoard board;

    private float dashCheckTimer;
    private float dashCoolTimer;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}

        playerBoard.animator.PlayDash();

        DashAsync(token.Token).Forget();
        playerBoard.skill.DashCoolTime(owner.DestroyToken).Forget();
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

            while (dashCheckTimer <= playerBoard.skill.dashTime)
            {
                dashCheckTimer += Time.deltaTime;

                playerBoard.rb.linearVelocity = new Vector2(playerBoard.movement.frontDir * playerBoard.skill.dashSpeed, 0f);

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
            Utils.Log<PlayerController>("대시 종료");
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (!playerBoard.movement.isGround)
                TransitionState(typeof(PlayerFallState));
            else if (playerBoard.movement.moveInput.x != 0f)
                TransitionState(typeof(PlayerMoveState));
            else
                TransitionState(typeof(PlayerIdleState));
        }
    }
}
