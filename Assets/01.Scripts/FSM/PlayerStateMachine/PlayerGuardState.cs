using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerGuardState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private bool isParryEnd;
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        isParryEnd = false;

        owner.StopMove();

        this.board.animator.PlayGuard();

        owner.Combat.GuardStart();

        ParryTimerAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        board.animator.StopGuard();

        owner.Combat.GuardEnd();

        ExitToken();
    }

    private async UniTaskVoid ParryTimerAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(owner.Combat.ParryWindow), cancellationToken: ctk);
                        
            isParryEnd = true;

            owner.Combat.ParryEnd();
        }
        catch (OperationCanceledException)
        {

        }
    }
}
