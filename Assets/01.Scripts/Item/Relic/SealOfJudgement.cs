using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class SealOfJudgement : Artifact
{
    private CancellationTokenSource token;
    private bool isApply;
    private float applyTimer = 3f;
    private StatPair applyPair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 140f
    };

    public SealOfJudgement()
    {
        // 일반 공격 기본 피해 -25% / 패리 성공 시 3초 이내 다음 일반 공격 피해 +100%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnParrySucceed -= FirstDamageUp;
        blackBoard.combat.OnAttackEnded -= FirstDamageDown;
        board.combat.OnParrySucceed += FirstDamageUp;
        blackBoard.combat.OnAttackEnded += FirstDamageDown;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        token = token.Cancel_DisposeToken();

        blackBoard.combat.OnParrySucceed -= FirstDamageUp;
        blackBoard.combat.OnAttackEnded -= FirstDamageDown;

        FirstDamageDown();

        base.OnUnequip(board);
    }

    private async UniTaskVoid DamageUpDurationAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(applyTimer), cancellationToken: ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (token != null && token.Token == ctk)
                FirstDamageDown();
        }
    }

    private void FirstDamageUp()
    {
        if (isApply)
            return;

        isApply = true;
        blackBoard.stats.AddStats(applyPair);

        token = token.LinkToken(blackBoard.owenrToken.Token);
        DamageUpDurationAsync(token.Token).Forget();
    }

    private void FirstDamageDown()
    {
        if (!isApply)
            return;

        isApply = false;

        CancellationTokenSource previousToken = token;
        token = null;

        blackBoard.stats.RemoveStats(applyPair);

        previousToken.Cancel_DisposeToken();
    }
}
