using Cysharp.Threading.Tasks;
using System;
using System.Threading;

// 토큰 링크 해둘 것
public class SilentDagger : Artifact
{
    private CancellationTokenSource token;
    private float upTimer = 5f;
    private bool isApply;
    private StatPair applyPair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 25
    };

    public SilentDagger()
    {
        // 피격 없이 5초 경과 시 일반 공격 피해 +25%. 피격 시 해제
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        token = token.LinkToken(board.owenrToken.Token);

        board.stats.OnDamaged -= HandleDamaged;
        board.stats.OnDamaged += HandleDamaged;

        DamageUpTimerAsync(token.Token).Forget();
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        token = token.Cancel_DisposeToken();

        board.stats.OnDamaged -= HandleDamaged;

        DamageDown();

        base.OnUnequip(board);
    }

    private async UniTaskVoid DamageUpTimerAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(upTimer), cancellationToken: ctk);

            DamageUp();
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void DamageUp()
    {
        if (isApply)
            return;

        isApply = true;
        blackBoard.stats.AddStats(applyPair);
    }

    private void DamageDown()
    {
        if (!isApply)
            return;

        isApply = false;
        blackBoard.stats.RemoveStats(applyPair);
    }

    private void HandleDamaged()
    {
        DamageDown();

        ResetTimer();
    }

    private void ResetTimer()
    {
        token = token.LinkToken(blackBoard.owenrToken.Token);
        DamageUpTimerAsync(token.Token).Forget();
    }
}
