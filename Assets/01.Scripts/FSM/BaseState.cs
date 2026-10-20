using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class BaseState<T>
{
    protected CancellationTokenSource token;

    public event Action<Type, bool> OnTransition;

    public BlackBoard blackBoard;

    public virtual void Initialized(BlackBoard blackBoard)
    {
        this.blackBoard = blackBoard;
    }

    public abstract void Enter(T owner);

    public abstract void Exit(T owner);

    protected void EnterToken(CancellationToken ownerToken)
    {
        token = token.LinkToken(ownerToken);        
    }

    protected void ExitToken()
    {
        token = token.Cancel_DisposeToken();
    }

    protected void TransitionState(Type type, bool isForce = false)
    {
        OnTransition?.Invoke(type, isForce);
    }
}
