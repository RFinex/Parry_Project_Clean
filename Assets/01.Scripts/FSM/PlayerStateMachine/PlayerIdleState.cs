using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Idle State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Idle State 종료");
    }
}
