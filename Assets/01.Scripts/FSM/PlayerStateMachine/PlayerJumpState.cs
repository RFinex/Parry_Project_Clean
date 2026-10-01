using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        owner.Jump();

        LandingCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    public async UniTaskVoid LandingCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                if (owner.IsGround && owner.VerticalVelocity <= 0f)
                {
                    if (owner.MoveInput.x != 0f)
                        TransitionState(typeof(PlayerMoveState));
                    else
                        TransitionState(typeof(PlayerIdleState));

                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
