using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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

public abstract class CompositeNode : INode
{
    protected List<INode> children = new();
    public void Add(INode node)
    {
        children.Add(node);
    }
    public abstract UniTask<INode.State> Evaluate(CancellationToken ctk);
}

// Failure 노드 찾기
public class SequenceNode : CompositeNode
{
    public override async UniTask<INode.State> Evaluate(CancellationToken ctk)
    {
        foreach (var node in children)
        {
            var state = await node.Evaluate(ctk);
            if (state != INode.State.Success)
                return state;
        }

        return INode.State.Success;
    }
}

// Success 노드 찾기
public class SelectorNode : CompositeNode
{
    public override async UniTask<INode.State> Evaluate(CancellationToken ctk)
    {
        foreach (var node in children)
        {
            var state = await node.Evaluate(ctk);
            if (state != INode.State.Failure)
                return state;
        }

        return INode.State.Failure;
    }
}
