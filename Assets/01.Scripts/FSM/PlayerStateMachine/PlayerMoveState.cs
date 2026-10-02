using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class PlayerMoveState : PlayerBaseState
{
    private PlayerBlackBoard board;
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }
        
        FixedUpdateAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        owner.StopMove();

        ExitToken();
    }

    public async UniTaskVoid FixedUpdateAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                owner.Move();

                if (board.movement.moveInput.x == 0f)
                {
                    TransitionState(typeof(PlayerIdleState));
                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
