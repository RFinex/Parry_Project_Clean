using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        this.board.animator.PlayJump();

        JumpCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid JumpCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                owner.Move();

                if (board.movement.verticalVelocity <= 0f)
                {
                    TransitionState(typeof(PlayerFallState));
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
