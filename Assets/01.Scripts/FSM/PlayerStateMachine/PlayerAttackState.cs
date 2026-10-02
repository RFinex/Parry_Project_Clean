using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerAttackState : PlayerBaseState
{
    private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        owner.Combat.Attack(owner.transform.position, this.board.movement.frontDir);

        AttackAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid AttackAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(owner.Combat.AttackDuration), cancellationToken: ctk);

            if (!board.movement.isGround)
            {
                TransitionState(typeof(PlayerJumpState));
                return;
            }

            if (board.movement.moveInput.x != 0f)
            {
                TransitionState(typeof(PlayerMoveState));
            }
            else
            {
                TransitionState(typeof(PlayerIdleState));
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
