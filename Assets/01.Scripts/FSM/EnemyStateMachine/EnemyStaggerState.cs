using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyStaggerState : EnemyBaseState
{
    private EnemyBlackBoard board;

    private float staggerTimer;
    private float balanceRestorePerSec;

    private Color originColor;

    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is EnemyBlackBoard board)
        {
            this.board = board;
        }

        staggerTimer = this.board.stats.StaggerDuration;

        balanceRestorePerSec = this.board.stats.RestoreBalancePerSec();

        originColor = this.board.sr.color;
        this.board.sr.color = Color.yellow;


        StaggerTimerAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        board.sr.color = originColor;

        ExitToken();
    }

    private async UniTaskVoid StaggerTimerAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                float deltaTime = Time.deltaTime;

                staggerTimer -= deltaTime;

                board.stats.RestoreBalance(balanceRestorePerSec * deltaTime);

                if (staggerTimer <= 0)
                {
                    staggerTimer = 0f;

                    board.stats.RestoreBalance();

                    owner.OnStaggerFinished();
                    TransitionState(typeof(EnemyIdleState));
                    return;
                }

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
