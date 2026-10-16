using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class PlayerMoveState : PlayerBaseState
{
    //private PlayerBlackBoard board;
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}
        
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

                if (playerBoard.movement.moveInput.x == 0f)
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

    public override void HandleInput(PlayerInput input)
    {
        switch (input)
        {
            case PlayerInput.JumpPressed:
                TryJump();
                break;

            case PlayerInput.DashPressed:
                TryDash();
                break;

            case PlayerInput.AttackPressed:
                TryAttackOrExecute();
                break;

            case PlayerInput.GuardPressed:
                TryGuard();
                break;

            default:
                break;
        }
    }
}
