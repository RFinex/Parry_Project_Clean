using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class BaseState<T>
{
    protected CancellationTokenSource token;

    public abstract void Enter(T obj);

    public abstract void Exit(T obj);

    public virtual async UniTaskVoid Update()
    {

    }
}
