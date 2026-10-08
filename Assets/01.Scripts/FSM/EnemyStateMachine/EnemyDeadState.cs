using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;

public class EnemyDeadState : EnemyBaseState
{
    private EnemyBlackBoard board;

    public override void Enter(EnemyController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is EnemyBlackBoard board)
        {
            this.board = board;
        }

        owner.StopMove();

        DeadAsync(owner, token.Token).Forget();
    }

    public override void Exit(EnemyController owner)
    {
        ExitToken();
    }

    private async UniTask DeadAsync(EnemyController owner, CancellationToken ctk)
    {
        try
        {
            if (board.animator.Play(EnemyAnimationType.Death))
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);

                await UniTask.WaitUntil(() => board.animator.Finish(EnemyAnimationType.Death), cancellationToken: ctk);
            }

            if (board.sr != null)
            {
                await board.sr.DOFade(0f, owner.DeathDuration)
                    .SetLink(owner.gameObject, LinkBehaviour.KillOnDisable)
                    .OnComplete(owner.FinishDeath)
                    .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);
            }
            else
            {
                owner.FinishDeath();
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
