using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        owner.StopMove();
        owner.LookAtTarget();

        for (int i = 0; i < comboData.Strikes.Count; i++)
        {
            ParryComboStrikeData strike = comboData.Strikes[i];

            if (strike == null || board.animator.PlayFromStart(strike.animType))
                return;


            if (strike.warningDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(strike.warningDelay), cancellationToken: ctk);
            }

            owner.ShowAttackWarning(comboData.CanGuard, comboData.CanParry);

            if (strike.hitDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(strike.hitDelay), cancellationToken: ctk);
            }

            ApplyAttack(owner, board);

            float remainTime = strike.animationDuration - strike.warningDelay - strike.hitDelay;

            if (remainTime > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainTime), cancellationToken: ctk);
            }

        }
    }
}
