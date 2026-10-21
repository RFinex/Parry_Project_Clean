using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerAttackState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        UnityEngine.Vector2 velocity = playerBoard.rb.linearVelocity;
        bool isAir = !playerBoard.movement.isGround || velocity.y > 0f;

        if (isAir)
        {            
            velocity.x = 0f;

            if (velocity.y > 0f)
                velocity.y = 0f;

            playerBoard.rb.linearVelocity = velocity;
            playerBoard.movement.verticalVelocity = velocity.y;
        }

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
}
