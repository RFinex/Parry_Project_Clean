using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Guard Release")]
public class PlayerGuardReleaseRule : PlayerTransitionRule
{
    public override Type GetNextState(PlayerBlackBoard board)
    {
        if (!board.movement.isGround)
            return typeof(PlayerFallState);

        return board.movement.moveInput.x != 0f ? typeof(PlayerMoveState) : typeof(PlayerIdleState);
    }
}
