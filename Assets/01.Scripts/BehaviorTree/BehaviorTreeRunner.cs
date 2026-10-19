using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BehaviorTreeRunner
{
    public async UniTask Run(INode root, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                await root.Evaluate(ctk);
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
            }
        }
        catch(OperationCanceledException)
        {

        } 
    }

}
