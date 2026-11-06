using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEngine;

public class OldSamuraiLargeComboAttackSkill : EnemySkill
{
    private readonly OldSamuraiLargeComboAttackData largeComboData;
    private readonly List<Vector2> attackPoints;

    public OldSamuraiLargeComboAttackSkill(OldSamuraiLargeComboAttackData data) : base(data)
    {
        largeComboData = data;
        attackPoints = new List<Vector2>();
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null || largeComboData == null)
            return;

        owner.StopMove();

        try
        {
            owner.LookAtTarget();

            await WaitStartup(ctk);

            if (Target == null || largeComboData.Strikes == null || largeComboData.Strikes.Count == 0)
                return;
                        
            Vector2 hitboxOffset = data.HitboxOffset;
            hitboxOffset.x *= owner.FrontDir;

            attackPoints.Clear();

            for (int i = 0; i < largeComboData.Strikes.Count; i++)
            {
                Vector2 targetPos = Target.transform.position;

                OldSamuraiLargeAttackData strike = largeComboData.Strikes[i];

                Vector2 strikeOffset = strike.TargetOffset;
                strikeOffset.x *= owner.FrontDir;

                // 공격 예정 좌표 저장
                attackPoints.Add(targetPos + hitboxOffset + strikeOffset);

                if (largeComboData.PreviewEffectId != 0 && ObjectPoolManager.Instance != null)
                {
                    ObjectPoolManager.Instance.Get<EffectBase>(largeComboData.PreviewEffectId, attackPoints[i]);
                }

                if (largeComboData.PreviewInterval > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(largeComboData.PreviewInterval), cancellationToken: ctk);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: ctk);
                }
            }

            // 공격 지점에 원형 미리보기 표시
            //for (int i = 0; i < attackPoints.Count; i++)
            //{
            //    if (largeComboData.PreviewEffectId != 0 && ObjectPoolManager.Instance != null)
            //    {
            //        ObjectPoolManager.Instance.Get<EffectBase>(largeComboData.PreviewEffectId, attackPoints[i]);
            //    }

            //    if (largeComboData.PreviewInterval > 0f)
            //    {
            //        await UniTask.Delay(TimeSpan.FromSeconds(largeComboData.PreviewInterval), cancellationToken: ctk);
            //    }
            //    else
            //    {
            //        await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: ctk);
            //    }
            //}

            for (int i = 0; i < largeComboData.Strikes.Count; i++)
            {
                OldSamuraiLargeAttackData strike = largeComboData.Strikes[i];

                owner.ShowAttackWarning(strike.CanGuard, strike.CanParry);

                if (largeComboData.PreviewInterval > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(largeComboData.PreviewInterval), cancellationToken: ctk);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: ctk);
                }
            }

            // 애니메이션, 적 위치의 가드 경고, 지정 프레임 공격
            for (int i = 0; i < largeComboData.Strikes.Count; i++)
            {
                OldSamuraiLargeAttackData strike = largeComboData.Strikes[i];                

                if (!board.animator.PlayFromStart(strike.AnimationType))
                    return;

                float warningDelay = Mathf.Min(strike.WarningDelay, strike.HitDelay);

                if (warningDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(warningDelay), cancellationToken: ctk);
                }

                float delayHit = strike.HitDelay - warningDelay;

                if (delayHit > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delayHit), cancellationToken: ctk);
                }

                Collider2D hit = Physics2D.OverlapBox(attackPoints[i], strike.HitboxSize, 0f, board.playerLayer);

                if (hit != null)
                {
                    board.combat.Attack(hit.transform, owner, data.Damage, strike.CanGuard, strike.CanParry);
                }

                float remainTime = strike.AnimDuration - strike.HitDelay;

                if (remainTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(remainTime), cancellationToken: ctk);
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
            if (owner != null)
                owner.StopMove();
        }
    }
}
