using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class OldSamuraiUpperSkill : EnemySkill
{
    private readonly OldSamuraiUpperSkillData upperData;

    public OldSamuraiUpperSkill(OldSamuraiUpperSkillData data) : base(data)
    {
        upperData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        board.animator.Play(EnemyAnimationType.Idle);

        owner.StopMove();

        await WaitStartup(ctk);

        owner.LookAtTarget();

        if (!board.animator.Play(data.AnimationType))
            return;

        if (upperData.WarningDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(upperData.WarningDelay), cancellationToken: ctk);

        owner.ShowAttackWarning(upperData.CanGuard, upperData.CanParry);

        if(upperData.HitDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(upperData.HitDelay), cancellationToken: ctk);

        ApplyAttack(owner, board);

        if(upperData.AnimEndDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(upperData.AnimEndDelay), cancellationToken: ctk);

        board.animator.Play(EnemyAnimationType.Idle);

        await WaitRecovery(ctk);
    }
}
