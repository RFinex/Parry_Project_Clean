using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        owner.StopMove();
    }

    public override void Exit(PlayerController owner)
    {

    }
}
