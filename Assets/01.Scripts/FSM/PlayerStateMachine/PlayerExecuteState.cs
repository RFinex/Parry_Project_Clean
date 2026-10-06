using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerExecuteState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private GameObject target;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        target = owner.ExecuteTarget;

        if (target == null)
        {
            TransitionState(typeof(PlayerIdleState));
            return;
        }

        owner.StopJump();

        ExecuteAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        board.afterImage.StopImage();

        owner.ClearExecuteTarget();

        owner.StopMove();

        ExitToken();
    }

    private async UniTaskVoid ExecuteAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            if (target == null)
            {
                TransitionState(typeof(PlayerIdleState));
                return;
            }

            IExecuteTarget enemy = target.GetComponent<IExecuteTarget>();

            if (enemy == null || !enemy.CanExecute)
            {
                TransitionState(typeof(PlayerIdleState));
                return;
            }

            board.afterImage.PlayImage(0.3f);

            Vector2 targetPos = enemy.GetExecutePos(owner.transform.position);
            
            float dir = target.transform.position.x - owner.transform.position.x;

            if (dir != 0f)
            {
                owner.SetFrontDirForExecute(dir);
            }

            while (!ctk.IsCancellationRequested)
            {
                if (target == null)
                {
                    TransitionState(typeof(PlayerIdleState));
                    return;
                }

                Vector2 currentPos = owner.transform.position;
                Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, board.movement.executeMoveSpeed * Time.deltaTime);

                board.rb.MovePosition(nextPos);

                float targetOffset = 0.05f * 0.05f;
                float distance = (targetPos - nextPos).sqrMagnitude;

                if (distance <= targetOffset)
                    break;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }

            if (ctk.IsCancellationRequested)
                return;

            board.afterImage.StopImage();

            board.animator.PlayExecuteAttack();

            owner.Combat.SpecialAttack(target);

            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ctk);

            TransitionState(typeof(PlayerIdleState));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
