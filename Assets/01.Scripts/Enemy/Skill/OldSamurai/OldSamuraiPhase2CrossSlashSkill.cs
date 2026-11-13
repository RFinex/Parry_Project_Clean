using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OldSamuraiPhase2CrossSlashSkill : EnemySkill
{
    private readonly OldSamuraiPhase2CrossSlashData crossSlashData;
    private readonly List<Vector2> attackPoints = new();

    public OldSamuraiPhase2CrossSlashSkill(OldSamuraiPhase2CrossSlashData data) : base(data)
    {
        crossSlashData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null || crossSlashData == null || Target == null)
            return;

        owner.StopMove();

        try
        {
            owner.LookAtTarget();
            await WaitStartup(ctk);

            Vector2 targetPosition = Target.transform.position;
            attackPoints.Clear();

            for (int i = 0; i < crossSlashData.Strikes.Count; i++)
            {
                OldSamuraiPhase2CrossSlashStrike strike = crossSlashData.Strikes[i];

                PlayerController target = Target;

                if (target == null)
                    break;

                owner.LookAtTarget();

                Vector2 attackPoint = (Vector2)target.transform.position + data.HitboxOffset + strike.TargetOffset;
                Vector2 scale = strike.AttackPointScale;

                // 타격 지점 표시
                if (crossSlashData.AttackPointEffectId != 0 && ObjectPoolManager.Instance != null)
                {
                    EffectBase warning = ObjectPoolManager.Instance.Get<EffectBase>(crossSlashData.AttackPointEffectId, attackPoint);

                    if (warning != null)
                    {
                        warning.transform.localScale = new Vector3(scale.x, scale.y, 1f);
                    }
                }

                // 타격 지점 표시 후 가드/패리 경고까지 대기
                if (strike.WarningDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.WarningDelay), cancellationToken: ctk);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
                }

                owner.ShowAttackWarning(strike.CanGuard, strike.CanParry);

                // 경고를 보여준 뒤 애니메이션 시작까지 대기
                if (strike.PreAnimationDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.PreAnimationDelay), cancellationToken: ctk);
                }

                // 공격 애니메이션 시작
                if (!board.animator.PlayFromStart(strike.AnimationType))
                    return;

                // 애니메이션의 타격 프레임까지 대기
                if (strike.HitDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.HitDelay), cancellationToken: ctk);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
                }

                bool isHorizontal = strike.HitboxSize.x > strike.HitboxSize.y;
                EffectBase effect = ObjectPoolManager.Instance.Get<EffectBase>(crossSlashData.SlashEffectId, attackPoint);

                if (isHorizontal)
                {
                    effect.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    effect.transform.localScale = new(scale.y, scale.x, 1f);
                }
                else
                {
                    effect.transform.rotation = Quaternion.identity;
                    effect.transform.localScale = new(scale.x, scale.y, 1f);
                }

                if (effect is OldSamuraiEffect slash)
                    slash.PlayEffect();

                Collider2D hit = Physics2D.OverlapBox(attackPoint, strike.HitboxSize, 0f, board.playerLayer);

                if (hit != null)
                {                    
                    board.combat.Attack(hit.transform, owner, data.Damage, data.StaminaDamage, strike.CanGuard, strike.CanParry);
                }

                if (strike.AnimEndDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(strike.AnimEndDelay), cancellationToken: ctk);
                }
                else
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
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
