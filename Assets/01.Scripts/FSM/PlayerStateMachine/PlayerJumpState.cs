using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Jump State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Jump State 종료");
    }
}
