using UnityEngine;

public class PlayerGuardState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Guard State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Guard State 종료");
    }
}
