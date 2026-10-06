using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerHurtState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private float hurtTimer = (2f / 11f);

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        this.board.animator.PlayHurt();

        HurtTimeAsync(token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    public async UniTaskVoid HurtTimeAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(hurtTimer), cancellationToken: ctk);

            if (!board.movement.isGround)
            {
                TransitionState(typeof(PlayerFallState));
                return;
            }

            if (board.movement.moveInput.x != 0)
            {
                TransitionState(typeof(PlayerMoveState));
                return;
            }

            TransitionState(typeof(PlayerIdleState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
