using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class BaseState<T>
{
    protected CancellationTokenSource token;

    public abstract void Enter(T owner);

    public abstract void Exit(T owner);

    public virtual async UniTaskVoid UpdateAsync(T owner, CancellationToken ctk)
    {

    }

    protected void EnterToken()
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();
    }

    protected void ExitToken()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }
}
