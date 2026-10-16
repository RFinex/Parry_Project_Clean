using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Jump")]
public class PlayerJumpRule : PlayerTransitionRule
{
    private readonly PlayerJumpAction action = new();

    public override IPlayerInputAction Action => action;

    public override bool CanApply(PlayerBlackBoard board)
    {
        return board.movement.isGround;
    }

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return typeof(PlayerJumpState);
    }
}
