using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Move")]
public class PlayerMoveRule : PlayerTransitionRule
{
    public override bool CanApply(PlayerBlackBoard board)
    {
        return board.movement.isGround;
    }

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return board.movement.moveInput.x != 0f ? typeof(PlayerMoveState) : typeof(PlayerIdleState);
    }
}
