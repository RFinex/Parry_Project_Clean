using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Attack Or Execute")]
public class PlayerAttackOrExecuteRule : PlayerTransitionRule
{
    private readonly PlayerAttackPrepareAction action = new();

    public override IPlayerInputAction Action => action;

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return board.executeTarget != null ? typeof(PlayerExecuteState) : typeof(PlayerAttackState);
    }
}
