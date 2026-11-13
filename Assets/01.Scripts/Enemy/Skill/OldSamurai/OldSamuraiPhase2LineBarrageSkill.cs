using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OldSamuraiPhase2LineBarrageSkill : EnemySkill
{
    private readonly OldSamuraiPhase2LineBarrageData lineData;
    private readonly List<Vector2> attackPoints = new();
    private readonly List<AttackLinePreviewEffect> linePreviews = new();
    private readonly List<float> attackAngles = new();
    private Transform lineAttackCenter;

    public OldSamuraiPhase2LineBarrageSkill(OldSamuraiPhase2LineBarrageData data, Transform lineAttackCenter) : base(data)
    {
        lineData = data;
        this.lineAttackCenter = lineAttackCenter;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null || lineData == null || lineAttackCenter == null)
            return;

        owner.StopMove();
        attackPoints.Clear();
        attackAngles.Clear();
        linePreviews.Clear();

        try
        {
            owner.LookAtTarget();
            await WaitStartup(ctk);

            // 선을 하나씩 표시
            for (int i = 0; i < lineData.AttackCount; i++)
            {
                Vector2 center = lineAttackCenter.position;
                Vector2 range = lineData.RandomPositionRange;

                Vector2 attackPoint = center + new Vector2(UnityEngine.Random.Range(-range.x, range.x), UnityEngine.Random.Range(-range.y, range.y));

                float angle = UnityEngine.Random.Range(lineData.AngleRange.x, lineData.AngleRange.y);
                
                attackPoints.Add(attackPoint);
                attackAngles.Add(angle);

                AttackLinePreviewEffect preview = null;

                if (lineData.LinePreviewId != 0 && ObjectPoolManager.Instance != null)
                {
                    preview = ObjectPoolManager.Instance.Get<AttackLinePreviewEffect>(lineData.LinePreviewId, attackPoint);
                }

                if (preview != null)
                {
                    preview.transform.localScale = new Vector3(lineData.LinePreviewScale.x, lineData.LinePreviewScale.y, 1f);

                    preview.transform.rotation = Quaternion.Euler(0f, 0f, angle);

                    linePreviews.Add(preview);
                }

                if (i < lineData.AttackCount - 1)
                {
                    if (lineData.LineInterval > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(lineData.LineInterval), cancellationToken: ctk);
                    }
                    else
                    {
                        await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
                    }
                }
            }

            if (lineData.WarningStartDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(lineData.WarningStartDelay), cancellationToken: ctk);
            }
            else
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
            }

            owner.ShowAttackWarning(data.CanGuard, data.CanParry);

            if (!board.animator.PlayFromStart(data.AnimationType))
                return;

            if (data.AttackDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(data.AttackDelay), cancellationToken: ctk);
            }
            else
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
            }

            for (int i = 0; i < linePreviews.Count; i++)
            {
                if (linePreviews[i] != null)
                    linePreviews[i].ReturnPool();
            }

            linePreviews.Clear();

            if (lineData.LineEffectId != 0 && ObjectPoolManager.Instance != null)
            {
                for (int i = 0; i < attackPoints.Count; i++)
                {
                    EffectBase effect = ObjectPoolManager.Instance.Get<EffectBase>(lineData.LineEffectId, attackPoints[i]);

                    if (effect == null)
                        return;

                    effect.transform.localScale = new Vector3(lineData.LineEffectScale.x, lineData.LineEffectScale.y, 1f);
                    effect.transform.rotation = Quaternion.Euler(0f, 0f, attackAngles[i]);

                    if (effect is OldSamuraiEffect slash)
                        slash.PlayEffect();
                }
            }            

            // 모든 선의 판정을 같은 타이밍에 확인
            // 겹친 선에 플레이어가 여러 번 맞는 것을 방지
            for (int i = 0; i < attackPoints.Count; i++)
            {
                Collider2D hit = Physics2D.OverlapBox(attackPoints[i], lineData.LineHitboxSize, attackAngles[i], board.playerLayer);

                if (hit == null)
                    continue;

                board.combat.Attack(hit.transform, owner, data.Damage, data.StaminaDamage, data.CanGuard, data.CanParry);

                break;
            }            

            board.animator.Play(EnemyAnimationType.Idle);
            await WaitRecovery(ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            for (int i = 0; i < linePreviews.Count; i++)
            {
                if (linePreviews[i] != null)
                    linePreviews[i].ReturnPool();
            }

            attackPoints.Clear();
            attackAngles.Clear();
            linePreviews.Clear();

            owner.StopMove();
        }
    }

    public void SetRoomCenter(Transform center)
    {
        lineAttackCenter = center;
    }
}
