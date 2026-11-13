using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OldSamuraiPhase2SweepSkill : EnemySkill
{
    private readonly OldSamuraiPhase2SweepData sweepData;
    private readonly List<Vector2> attackPoints = new();

    public OldSamuraiPhase2SweepSkill(OldSamuraiPhase2SweepData data) : base(data)
    {
        sweepData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null || sweepData == null || sweepData.Strikes.Count == 0)
            return;

        owner.StopMove();

        try
        {
            owner.LookAtTarget();
            await WaitStartup(ctk);

            attackPoints.Clear();

            // 공격 지점 표시
            for (int i = 0; i < sweepData.Strikes.Count; i++)
            {
                OldSamuraiPhase2SweepStrike strike = sweepData.Strikes[i];

                Vector2 offset = strike.HitboxOffset;
                offset.x *= owner.FrontDir;

                Vector2 attackPoint = (Vector2)owner.transform.position + offset;
                attackPoints.Add(attackPoint);

                if (sweepData.AttackPointEffectId != 0 && ObjectPoolManager.Instance != null)
                {
                    EffectBase effect = ObjectPoolManager.Instance.Get<EffectBase>(sweepData.AttackPointEffectId, attackPoint);

                    if (effect != null)
                    {
                        effect.transform.localScale = new Vector3(strike.AttackPointScale.x, strike.AttackPointScale.y, 1f);
                    }
                }

                if (i < sweepData.Strikes.Count - 1)
                {
                    if (sweepData.AttackPointDuration > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(sweepData.AttackPointDuration), cancellationToken: ctk);
                    else
                        await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
                }
            }

            // 가드 경고 표시
            for (int i = 0; i < sweepData.Strikes.Count; i++)
            {
                OldSamuraiPhase2SweepStrike strike = sweepData.Strikes[i];

                owner.ShowAttackWarning(strike.CanGuard, strike.CanParry);

                if (strike.WarningDelay > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.WarningDelay), cancellationToken: ctk);
                else
                    await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
            }

            // 애니메이션 타이밍에 맞춰 저장된 위치를 공격
            for (int i = 0; i < sweepData.Strikes.Count; i++)
            {
                OldSamuraiPhase2SweepStrike strike = sweepData.Strikes[i];

                if (!board.animator.PlayFromStart(strike.AnimationType))
                    return;

                if (strike.HitDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.HitDelay), cancellationToken: ctk);
                }

                EffectBase effect = ObjectPoolManager.Instance.Get<EffectBase>(sweepData.Strikes[i].StrikeEffectId, attackPoints[i]);

                effect.transform.localScale = sweepData.Strikes[i].StrikeEffectScale;

                if (effect is OldSamuraiEffect slash)
                    slash.PlayEffect();

                Collider2D hit = Physics2D.OverlapBox(attackPoints[i], strike.HitboxSize, 0f, board.playerLayer);

                if (hit != null)
                {
                    board.combat.Attack(hit.transform, owner, data.Damage, data.StaminaDamage, strike.CanGuard, strike.CanParry);
                }

                // 타격 후 현재 애니메이션이 끝날 때까지 대기
                if (strike.AnimEndDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.AnimEndDelay), cancellationToken: ctk);
                }
            }

            board.animator.Play(EnemyAnimationType.Idle);
            await WaitRecovery(ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            owner.StopMove();
        }
    }
}
