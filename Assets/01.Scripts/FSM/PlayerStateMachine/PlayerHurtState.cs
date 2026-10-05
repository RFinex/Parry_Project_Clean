using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerHurtState : PlayerBaseState
{
    private PlayerBlackBoard board;

    private float hurtTimer;

    public override void Enter(PlayerController owner)
    {
        EnterToken(owner.DestroyToken);

        if (blackBoard is PlayerBlackBoard board)
        {
            this.board = board;
        }

        Utils.Log<PlayerHurtState>("Hurt 진입 성공");

        hurtTimer = this.board.stats.InvincibleTime;

        this.board.animator.PlayHurt();

        InvincibleTimeAsync(owner, token.Token).Forget();
    }

    public override void Exit(PlayerController owner)
    {
        ExitToken();
    }

    public async UniTaskVoid InvincibleTimeAsync(PlayerController owner, CancellationToken ctk)
    {
        try
        {
            Utils.Log<PlayerHurtState>("무적 타이머 시작");
            while (!ctk.IsCancellationRequested)
            {
                hurtTimer -= Time.deltaTime;

                owner.Move();

                if (hurtTimer <= 0)
                {
                    hurtTimer = 0;
                    TransitionState(typeof(PlayerIdleState));
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
