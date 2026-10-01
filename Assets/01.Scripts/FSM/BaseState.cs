using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class BaseState<T>
{
    protected CancellationTokenSource token;

    protected StateMachine<T> stateMachine;

    public event Action<Type> OnTransition;

    public abstract void Enter(T owner);

    public abstract void Exit(T owner);

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

    protected void TransitionState(Type type)
    {
        OnTransition?.Invoke(type);
    }
}
