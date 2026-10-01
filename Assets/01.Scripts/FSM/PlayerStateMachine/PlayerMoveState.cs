using System;
using System.Threading;
using Cysharp.Threading.Tasks;
public class PlayerMoveState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

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

                if (owner.MoveInput.x == 0f)
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
