using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class BaseState<T>
{
    protected CancellationTokenSource token;

    public abstract void Enter(T owner);

    public abstract void Exit(T owner);

    //public virtual async UniTask UpdateAsync(T owner, CancellationToken ctk)
    //{
    //    await UniTask.CompletedTask;
    //}

    protected void EnterToken(CancellationToken ownerToken)
    {
        token?.Cancel();
        token?.Dispose();
        token = CancellationTokenSource.CreateLinkedTokenSource(ownerToken);
    }

    protected void ExitToken()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }
}
