using UnityEngine;

public abstract class PlayerBaseState : BaseState<PlayerController>
{
    protected PlayerBlackBoard playerBoard;

    public virtual void HandleInput(PlayerInput input)
    {

    }

    public override void Initialized(BlackBoard blackBoard)
    {
        base.Initialized(blackBoard);

        if (blackBoard is PlayerBlackBoard board)
        {
            playerBoard = board;
        }
    }

    protected void TryJump()
    {
        if (!playerBoard.movement.isGround)
            return;

        playerBoard.rb.linearVelocity = new Vector2(playerBoard.rb.linearVelocity.x, playerBoard.movement.jumpForce);

        playerBoard.movement.verticalVelocity = playerBoard.rb.linearVelocity.y;
        TransitionState(typeof(PlayerJumpState));
    }

    protected void TryDash()
    {
        if (playerBoard.skill.dashCoolTimer > 0f)
            return;

        TransitionState(typeof(PlayerDashState));
    }

    protected void TryAttackOrExecute()
    {
        GameObject target = playerBoard.combat.FindExecuteTarget(playerBoard.playerTransform.position);

        if (target != null)
        {
            playerBoard.executeTarget = target;
            TransitionState(typeof(PlayerExecuteState));
            return;
        }

        TransitionState(typeof(PlayerAttackState));
    }

    protected void TryGuard()
    {
        if (playerBoard.combat.CanGuard)
            TransitionState(typeof(PlayerGuardState));
    }

    protected void ReleaseGuard()
    {
        if (!playerBoard.movement.isGround)
        {
            TransitionState(typeof(PlayerFallState));
            return;
        }

        if (playerBoard.movement.moveInput.x != 0f)
        {
            TransitionState(typeof(PlayerMoveState));
            return;
        }

        TransitionState(typeof(PlayerIdleState));
    }
}
