using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Guard")]
public class PlayerGuardRule : PlayerTransitionRule
{
    public override bool CanApply(PlayerBlackBoard board)
    {
        return board.combat.CanGuard;
    }

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return typeof(PlayerGuardState);
    }
}
