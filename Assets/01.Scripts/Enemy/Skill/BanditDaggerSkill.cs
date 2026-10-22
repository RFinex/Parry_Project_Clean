using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BanditDaggerSkill : EnemySkill
{
    // 발사 시각, 투사체 풀 ID, 생성 위치 오프셋을 담은 이 스킬 전용 SO 데이터.
    private readonly BanditDaggerSkillData daggerData;

    public BanditDaggerSkill(BanditDaggerSkillData data) : base(data)
    {
        daggerData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        // 선딜
        await WaitStartup(ctk);
        if (owner == null || board == null || daggerData.ShotTimes == null)
            return;

        if (!board.animator.Play(data.AnimationType))
            return;

        float timer = 0f;

        // ShotTimes를 순서대로 처리한다. SO의 값은 오름차순으로 설정해야 한다.
        for (int i = 0; i < daggerData.ShotTimes.Count; i++)
        {
            float shotTime = daggerData.ShotTimes[i];
            // 직전 발사 시각부터 이번 발사 시각까지의 간격만큼 기다린다.
            float waitTime = shotTime - timer;

            if (waitTime > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime), cancellationToken: ctk);
            }

            timer = shotTime;

            // 발사 순간의 플레이어 위치 조준
            PlayerController player = Target;

            if (player == null)
                break;

            Vector2 targetPos = (Vector2)player.transform.position + Vector2.up * daggerData.PlayerBodyOffset;
            float directionX = targetPos.x - owner.transform.position.x;
            float frontDirection = directionX >= 0f ? 1f : -1f;
                        
            Vector2 spawnPosition = (Vector2)owner.transform.position + new Vector2(frontDirection * daggerData.SpawnOffset.x, daggerData.SpawnOffset.y);

            Vector2 direction = targetPos - spawnPosition;

            Dagger projectile = ObjectPoolManager.Instance.Get<Dagger>(daggerData.ObjectId, spawnPosition);

            if (projectile == null)
                continue;

            EnemyAttackInfo attackInfo = new EnemyAttackInfo(data.Damage, owner, data.CanGuard, data.CanParry, false);

            projectile.Shoot(direction, attackInfo);
        }

        // 마지막 발사 시각 이후에도 애니메이션 시간이 남으면 종료 시각까지 대기
        float remainingAnimationTime = daggerData.AnimationDuration - timer;

        if (remainingAnimationTime > 0f)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(remainingAnimationTime), cancellationToken: ctk);
        }

        // 후딜
        await WaitRecovery(ctk);
    }
}