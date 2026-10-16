using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerAttackState : PlayerBaseState
{
    //private PlayerBlackBoard board;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        //if (blackBoard is PlayerBlackBoard board)
        //{
        //    this.board = board;
        //}

        int index = playerBoard.combat.AttackStart(owner.transform.position, playerBoard.movement.frontDir);

        playerBoard.animator.PlayAttack(index);

        AttackAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        playerBoard.combat.AttackEnd();

        ExitToken();
    }

    private async UniTaskVoid AttackAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                int index = playerBoard.combat.UpdateAttack(UnityEngine.Time.deltaTime, owner.transform.position, playerBoard.movement.frontDir);

                if (index > 0)
                {
                    playerBoard.animator.PlayAttack(index);
                }

                if (playerBoard.combat.IsAttackFinished())
                {
                    Utils.Log<PlayerAttackState>("공격 상태 종료");

                    if (!playerBoard.movement.isGround)
                    {
                        TransitionState(typeof(PlayerFallState));
                        return;
                    }

                    if (playerBoard.movement.moveInput.x != 0f)
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

    public override void HandleInput(PlayerInput input)
    {
        switch (input)
        {
            case PlayerInput.DashPressed:
                TryDash();
                break;

            case PlayerInput.AttackPressed:
                playerBoard.combat.RequestCombo();
                break;

            case PlayerInput.GuardPressed:
                TryGuard();
                break;

            default:
                break;
        }
    }
}
