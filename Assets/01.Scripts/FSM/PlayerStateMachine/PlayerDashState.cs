using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float dashCheckTimer;
    private float dashCoolTimer;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        playerBoard.animator.PlayDash();

        float dashTime = playerBoard.stats.GetStats(StatId.DashTime);
        float dashSpeed = playerBoard.stats.GetStats(StatId.DashSpeed);
        float dashCooldown = playerBoard.stats.GetStats(StatId.DashCooldown);

        playerBoard.skill.DashCoolTime(dashCooldown, owner.DestroyToken).Forget();
        playerBoard.stats.SetDashInvincible(true);

        playerBoard.stats.UseStamina(playerBoard.stats.GetStats(StatId.DashStaminaCost));
        DashAsync(dashTime, dashSpeed, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        playerBoard.stats.SetDashInvincible(false);
        ExitToken();
    }

    private async UniTaskVoid DashAsync(float dashTime, float dashSpeed, CancellationToken ctk)
    {
        try
        {
            dashCheckTimer = 0f;

            while (dashCheckTimer <= dashTime)
            {
                playerBoard.rb.linearVelocity = new Vector2(playerBoard.movement.frontDir * dashSpeed, 0f);

                dashCheckTimer += Time.fixedDeltaTime;

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }

            playerBoard.skill.DashEndedInvoke();
            Utils.Log<PlayerController>("대시 종료");
        }
        catch (OperationCanceledException)
        {
            return;
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
