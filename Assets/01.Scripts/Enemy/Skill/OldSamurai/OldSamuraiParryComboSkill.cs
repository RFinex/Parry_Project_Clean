using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class OldSamuraiParryComboSkill : EnemySkill
{
    private readonly ParryComboSkillData comboData;

    public OldSamuraiParryComboSkill(ParryComboSkillData data) : base(data)
    {
        comboData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        board.animator.Play(EnemyAnimationType.Idle);

        owner.StopMove();

        await WaitStartup(ctk);

        try
        {
            for (int i = 0; i < comboData.Strikes.Count; i++)
            {
                ParryComboStrikeData strike = comboData.Strikes[i];

                if (strike == null || !board.animator.PlayFromStart(strike.animType))
                    return;

                owner.LookAtTarget();

                if (strike.warningDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.warningDelay), cancellationToken: ctk);
                }

                owner.ShowAttackWarning(comboData.CanGuard, comboData.CanParry);

                if (strike.hitDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.hitDelay), cancellationToken: ctk);
                }

                float chargePower = comboData.ChargeDistance / comboData.ChargeDuration;
                float timer = 0f;

                board.rb.linearVelocity = new Vector2(owner.FrontDir * chargePower, board.rb.linearVelocity.y);

                // 돌진 시간동안 히트박스 유지
                while (timer < comboData.ChargeDuration)
                {
                    ApplyAttack(owner, board);

                    timer += Time.deltaTime;
                    await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: ctk);
                }

                owner.StopMove();

                float remainTime = strike.animationDuration - strike.warningDelay - strike.hitDelay - comboData.ChargeDuration;

                if (remainTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(remainTime), cancellationToken: ctk);
                }
            }

            board.animator.Play(EnemyAnimationType.Idle);

            await WaitRecovery(ctk);
        }
        finally
        {
            owner.StopMove();
        }       
    }
}
