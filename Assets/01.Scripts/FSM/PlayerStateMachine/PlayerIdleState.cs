using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        UpdateAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }

    public override async UniTaskVoid UpdateAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            while (true)
            {
                if (owner.MoveInput.x != 0f)
                {
                    owner.ChangeState<PlayerMoveState>();
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
