using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerGuardState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        owner.StopMove();

        owner.GuardStart();

        ParryTimerAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        owner.GuardEnd();

        ExitToken();
    }

    private async UniTaskVoid ParryTimerAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(owner.Combat.ParryWindow), cancellationToken: ctk);

            owner.Combat.ParryEnd();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
