using System;
using System.Threading;
using Cysharp.Threading.Tasks;
public class PlayerMoveState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Move State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Move State 종료");
    }
}
