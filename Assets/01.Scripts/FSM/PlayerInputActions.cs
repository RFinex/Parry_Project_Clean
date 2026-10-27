using UnityEngine;

public class PlayerInputActions : MonoBehaviour
{
    
}

public class PlayerJumpAction : IPlayerInputAction
{
    public void PlayerInput(PlayerBlackBoard board)
    {
        float jumpForce = board.stats.GetStats(StatId.JumpForce);

        board.rb.linearVelocity = new Vector2(board.rb.linearVelocity.x, jumpForce);
        board.movement.verticalVelocity = board.rb.linearVelocity.y;
    }
}

public class PlayerAttackPrepareAction : IPlayerInputAction
{
    public void PlayerInput(PlayerBlackBoard board)
    {
        board.executeTarget = board.combat.FindExecuteTarget(board.playerTransform.position);
    }
}

public class PlayerRequestComboAction : IPlayerInputAction
{
    public void PlayerInput(PlayerBlackBoard board)
    {
        board.combat.RequestCombo();
    }
}

public class PlayerJumpReleaseAction : IPlayerInputAction
{
    public void PlayerInput(PlayerBlackBoard board)
    {
        Vector2 velocity = board.rb.linearVelocity;

        if (velocity.y <= 0f)
            return;

        velocity.y *= 0.4f;

        board.rb.linearVelocity = velocity;
        board.movement.verticalVelocity = velocity.y;
    }
}