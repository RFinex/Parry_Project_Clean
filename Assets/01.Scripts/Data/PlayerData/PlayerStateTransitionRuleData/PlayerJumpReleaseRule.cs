using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Jump Release")]
public class PlayerJumpReleaseRule : PlayerTransitionRule
{
    private readonly PlayerJumpReleaseAction action = new();

    public override IPlayerInputAction Action => action;

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return null;
    }
}
