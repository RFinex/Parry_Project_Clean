using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class OldSamuraiThrowSkill : EnemySkill
{
    private readonly OldSamuraiThrowSkillData throwData;

    public OldSamuraiThrowSkill(OldSamuraiThrowSkillData data) : base(data)
    {
        throwData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        board.animator.Play(EnemyAnimationType.Idle);

        owner.StopMove();

        await WaitStartup(ctk);

        if (owner == null || board == null)
            return;

        for (int i = 0; i < throwData.AttackCount; i++)
        {
            owner.LookAtTarget();

            if (!board.animator.PlayFromStart(data.AnimationType))
                return;

            if(throwData.ThrowDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(throwData.ThrowDelay), cancellationToken: ctk);

            PlayerController player = Target;

            if (player == null)
                break;

            Vector2 targetPos = (Vector2)player.transform.position + Vector2.up * throwData.PlayerBodyOffset;
            float dirX = targetPos.x - owner.transform.position.x;
            float frontDir = dirX >= 0f ? 1f : -1f;

            Vector2 spawnPos = (Vector2)owner.transform.position + new Vector2(throwData.SpawnOffset.x * frontDir, throwData.SpawnOffset.y);
            Vector2 dir = targetPos - spawnPos;

            Dagger projectile = ObjectPoolManager.Instance.Get<Dagger>(throwData.ObjectId, spawnPos);

            if (projectile == null)
                continue;

            EnemyAttackInfo info = new(data.Damage, data.StaminaDamage, owner, data.CanGuard, data.CanParry, false);

            projectile.Shoot(dir, info);

            if (throwData.AnimEndDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(throwData.AnimEndDelay), cancellationToken: ctk);
        }

        board.animator.Play(EnemyAnimationType.Idle);

        await WaitRecovery(ctk);
    }
}
