using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SequenceNode : INode
{
    public UniTask<INode.State> Evaluate(CancellationToken ctk)
    {
        throw new System.NotImplementedException();
    }
}
