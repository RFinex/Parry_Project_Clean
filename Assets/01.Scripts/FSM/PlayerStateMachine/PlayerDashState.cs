using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Dash State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Dash State 종료");
    }
}
