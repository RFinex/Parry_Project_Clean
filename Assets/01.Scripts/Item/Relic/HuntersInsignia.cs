using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class HuntersInsignia : Artifact
{
    private bool isApply;
    private float statUpTimer = 5f;
    private CancellationTokenSource token;

    private StatPair speedPair = new()
    {
        id = StatId.MoveSpeed,
        modifierType = StatModifierType.Percent,
        value = 20
    };

    private StatPair parryDamagePair = new()
    {
        id = StatId.ParryDamage,
        modifierType = StatModifierType.Percent,
        value = 30
    };

    public HuntersInsignia()
    {
        // 처형 성공 후 5초 동안 이동 속도 +20%, 패리 균형 피해 +30%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= StatsUp;
        board.combat.OnExecuteSucceed += StatsUp;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= StatsUp;

        StatsDown();

        base.OnUnequip(board);
    }

    private async UniTaskVoid StatsUpTimeAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(statUpTimer), cancellationToken: ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (token != null && token.Token == ctk)
                StatsDown();
        }
    }

    private void StatsUp()
    {
        if (blackBoard.owenrToken.IsCancellationRequested)
            return;

        CancellationTokenSource previousToken = token;

        token = token.LinkTokenOnly(blackBoard.owenrToken.Token);
        previousToken.Cancel_DisposeToken();
        
        if (!isApply)
        {
            isApply = true;

            blackBoard.stats.AddStats(speedPair);
            blackBoard.stats.AddStats(parryDamagePair);
        }

        StatsUpTimeAsync(token.Token).Forget();
    }

    private void StatsDown()
    {
        CancellationTokenSource previousToken = token;
        token = null;

        previousToken.Cancel_DisposeToken();

        if (!isApply)
            return;

        isApply = false;

        blackBoard.stats.RemoveStats(speedPair);
        blackBoard.stats.RemoveStats(parryDamagePair);
    }
}
