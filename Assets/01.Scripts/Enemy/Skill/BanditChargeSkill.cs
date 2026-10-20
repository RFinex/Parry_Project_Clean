using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BanditChargeSkill : EnemySkill
{
    private readonly BanditChargeSkillData chargeData;

    public BanditChargeSkill(BanditChargeSkillData data) : base(data)
    {
        chargeData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        owner.StopMove();
        owner.LookAtTarget();

        board.animator.Play(EnemyAnimationType.Idle);
        await WaitStartup(ctk);

        if (!board.animator.Play(data.AnimationType))
            return;

        board.rb.linearVelocity = new Vector2(owner.FrontDir * chargeData.ChargeSpeed, board.rb.linearVelocity.y);

        try
        {
            await UniTask.WhenAll(
                CheckHitCharge(owner, board, ctk),
                UniTask.Delay(TimeSpan.FromSeconds(chargeData.ChargeDuration),
                cancellationToken: ctk));
        }
        finally
        {
            owner.StopMove();
        }

        await UniTask.WaitUntil(() => board.animator.Finish(data.AnimationType), cancellationToken: ctk);

        await WaitRecovery(ctk);
    }

    private async UniTask CheckHitCharge(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        float time = 0f;

        while (time < chargeData.ChargeDuration && !ctk.IsCancellationRequested)
        {
            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);

            if (ApplyAttack(owner, board))
                return;

            time += Time.fixedDeltaTime;
        }
    }
}
