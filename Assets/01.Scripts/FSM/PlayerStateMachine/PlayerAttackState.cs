using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerAttackState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Attack State 시작");
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Attack State 종료");
    }
}
