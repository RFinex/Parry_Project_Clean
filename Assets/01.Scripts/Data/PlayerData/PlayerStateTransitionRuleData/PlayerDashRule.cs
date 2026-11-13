using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Dash")]
public class PlayerDashRule : PlayerTransitionRule
{
    public override bool CanApply(PlayerBlackBoard board)
    {
        if (board.stats == null)
            return false;

        float staminaCost = board.stats.GetStats(StatId.DashStaminaCost);

        return board.skill.dashCoolTimer <= 0f && !board.stats.IsStaminaBroken && board.stats.NowStamina >= staminaCost;
    }

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return typeof(PlayerDashState);
    }
}
