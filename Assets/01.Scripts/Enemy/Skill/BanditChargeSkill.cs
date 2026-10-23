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
        ChargeWarning warning = null;



        //await WaitStartup(ctk);

        //if (!board.animator.Play(data.AnimationType))
        //    return;

        //board.rb.linearVelocity = new Vector2(owner.FrontDir * chargeData.ChargeSpeed, board.rb.linearVelocity.y);

        try
        {
            owner.StopMove();
            owner.LookAtTarget();
            board.animator.Play(EnemyAnimationType.Idle);


            float chargeDistance = chargeData.ChargeSpeed * chargeData.ChargeDuration;

            float warningWidth = chargeDistance + data.HitboxSize.x;
            float warningOffset = data.HitboxXOffset + chargeDistance * 0.5f;

            Vector2 warningCenter = (Vector2)owner.transform.position + Vector2.right * owner.FrontDir * warningOffset;
            warningCenter.y += chargeData.Offset;

            warning = ObjectPoolManager.Instance.Get<ChargeWarning>(chargeData.WarningId, warningCenter);

            if (warning != null)
            {
                warning.transform.localScale = new Vector3(warningWidth, data.HitboxSize.y, 1f);

                await warning.FillAsync(data.StartupDelay, owner.FrontDir, ctk);

                warning.ReturnPool();
                warning = null;
            }
            else
            {
                // 풀에 경고 표시를 안넣어도 선딜은 대기
                await WaitStartup(ctk);
            }

            if (!board.animator.Play(data.AnimationType))
                return;

            board.rb.linearVelocity = new Vector2(owner.FrontDir * chargeData.ChargeSpeed, board.rb.linearVelocity.y);

            await UniTask.WhenAll(
                CheckHitCharge(owner, board, ctk),
                UniTask.Delay(TimeSpan.FromSeconds(chargeData.ChargeDuration),
                cancellationToken: ctk));

            owner.StopMove();

            await UniTask.WaitUntil(() => board.animator.Finish(data.AnimationType), cancellationToken: ctk);

            await WaitRecovery(ctk);
        }
        finally
        {
            owner.StopMove();

            if (warning != null)
                warning.ReturnPool();
        }
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
