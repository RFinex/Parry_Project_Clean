using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Dash")]
public class PlayerDashRule : PlayerTransitionRule
{
    public override bool CanApply(PlayerBlackBoard board)
    {
        return board.skill.dashCoolTimer <= 0f;
    }
    public override Type GetNextState(PlayerBlackBoard board)
    {
        return typeof(PlayerDashState);
    }
}
