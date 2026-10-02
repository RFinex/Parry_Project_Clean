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

        int index = owner.Combat.AttackStart(owner.transform.position, this.board.movement.frontDir);

        this.board.animator.PlayAttack(index);

        AttackAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        owner.Combat.AttackEnd();

        ExitToken();
    }

    private async UniTaskVoid AttackAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                int index = owner.Combat.UpdateAttack(UnityEngine.Time.deltaTime, owner.transform.position, board.movement.frontDir);

                if (index > 0)
                {
                    board.animator.PlayAttack(index);
                }

                if (owner.Combat.IsAttackFinished())
                {
                    Utils.Log<PlayerAttackState>("공격 상태 종료");

                    if (!board.movement.isGround)
                    {
                        TransitionState(typeof(PlayerFallState));
                        return;
                    }

                    if (board.movement.moveInput.x != 0f)
                    {
                        TransitionState(typeof(PlayerMoveState));
                        return;
                    }

                    TransitionState(typeof(PlayerIdleState));
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }            
        }
        catch (OperationCanceledException)
        {

        }
    }
}
