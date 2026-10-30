using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BerserkersSignet : Artifact
{
    private int damageStack;
    private int maxStack = 6;
    private float damagePerStack = 5f;
    private float damageTime = 4f;
    private bool isTakeDamageApply;    

    private CancellationTokenSource token;

    private StatPair damagePair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 0f
    };

    private StatPair takeDamagePair = new()
    {
        id = StatId.DamageTakenMultiplier,
        modifierType = StatModifierType.Percent,
        value = 15f
    };

    public BerserkersSignet()
    {
        // 일반 공격 적중 시 4초 동안 일반 공격 피해 +5%(적중 시 지속 시간 갱신, 최대 6중첩)
        // 최대 중첩 중 받는 피해 + 15 %
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnAttackHit -= StatsUpStack;
        board.combat.OnAttackHit += StatsUpStack;

        damageStack = 0;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnAttackHit -= StatsUpStack;

        StatsDown();

        base.OnUnequip(board);
    }

    private async UniTaskVoid StatsUpTimeAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(damageTime), cancellationToken: ctk);
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

    private void StatsUpStack()
    {
        if (blackBoard.owenrToken.IsCancellationRequested)
            return;

        CancellationTokenSource previousToken = token;
        token = token.LinkTokenOnly(blackBoard.owenrToken.Token);
        previousToken.Cancel_DisposeToken();

        StatsUpTimeAsync(token.Token).Forget();

        if (damageStack < maxStack)
        {
            if (damageStack > 0)
                blackBoard.stats.RemoveStats(damagePair);

            damageStack++;

            damagePair.value = damagePerStack * damageStack;
            blackBoard.stats.AddStats(damagePair);

            if (damageStack == maxStack && !isTakeDamageApply)
            {
                isTakeDamageApply = true;
                blackBoard.stats.AddStats(takeDamagePair);
            }
        }

        StatsUpTimeAsync(token.Token).Forget();
    }

    private void StatsDown()
    {
        CancellationTokenSource previouseToken = token;
        token = null;

        previouseToken.Cancel_DisposeToken();

        if (damageStack > 0)
            blackBoard.stats.RemoveStats(damagePair);

        if (isTakeDamageApply)
        {
            blackBoard.stats.RemoveStats(takeDamagePair);
            isTakeDamageApply = false;
        }

        damageStack = 0;
        damagePair.value = 0f;
    }
}
