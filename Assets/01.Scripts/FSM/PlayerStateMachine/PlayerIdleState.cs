using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        owner.StopMove();

        InputCheckAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    private async UniTaskVoid InputCheckAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                if (owner.MoveInput.x != 0f)
                {
                    TransitionState(typeof(PlayerMoveState));
                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
