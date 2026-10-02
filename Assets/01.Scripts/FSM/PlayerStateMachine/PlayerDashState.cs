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

        Utils.Log<PlayerDashState>("Dash 진입 성공");


        if (blackBoard is PlayerBlackBoard board)
        {
            Utils.Log<PlayerDashState>("BlackBoard 넘겨주기 성공");
            this.board = board;
        }

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
