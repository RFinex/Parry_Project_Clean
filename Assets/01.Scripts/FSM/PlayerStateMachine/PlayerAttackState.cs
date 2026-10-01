using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.InputSystem.XR;

public class PlayerAttackState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Attack State 시작");

        owner.Attack();

        EnterToken(owner.DestroyToken);

        AttackAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        Utils.Log<PlayerJumpState>("Attack State 종료");

        ExitToken();
    }

    private async UniTaskVoid AttackAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(owner.Combat.AttackDuration), cancellationToken: ctk);

            if (owner.MoveInput.x != 0f)
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
