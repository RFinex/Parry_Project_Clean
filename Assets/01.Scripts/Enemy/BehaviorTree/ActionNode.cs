using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public interface INode
{
    public enum State
    {
        Failure,
        Success,
        Running
    }

    UniTask<State> Evaluate(CancellationToken ctk);
}

public class ActionNode : INode
{
    private readonly Func<CancellationToken, UniTask<INode.State>> action;

    public ActionNode(Func<CancellationToken, UniTask<INode.State>> action)
    {
        this.action = action;
    }

    public async UniTask<INode.State> Evaluate(CancellationToken ctk)
    {
        if(action == null)
            return INode.State.Failure;

        return await action.Invoke(ctk);
    }
}
