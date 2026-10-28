using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CounterAttackInsignia : Artifact
{
    private CancellationTokenSource token;

    public CounterAttackInsignia()
    {

    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        base.OnEquip(board);

        board.combat.OnParrySucceed -= FirstDamageUp;
        board.combat.OnParrySucceed += FirstDamageUp;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        base.OnUnequip(board);
    }

    private async UniTaskVoid DamageUpDurationAsync(PlayerBlackBoard board, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ctk);

            board.stats.RemoveStats(new StatPair
            {
                id = StatId.AttackDamage,
                modifierType = StatModifierType.Percent,
                value = 80f
            });

            board.combat.OnParrySucceed -= FirstDamageUp;
            board.combat.OnParrySucceed += FirstDamageUp;
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            board.combat.OnAttackEnded -= FirstDamageDown;
        }
    }

    private void FirstDamageUp(PlayerBlackBoard board)
    {
        token = token.ResetToken();
        DamageUpDurationAsync(board, token.Token).Forget();

        board.stats.AddStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 80f
        });

        board.combat.OnParrySucceed -= FirstDamageUp;
        board.combat.OnAttackEnded += FirstDamageDown;
    }

    private void FirstDamageDown(PlayerBlackBoard board)
    {
        token = token.Cancel_DisposeToken();

        board.stats.RemoveStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 80f
        });

        board.combat.OnParrySucceed -= FirstDamageUp;
        board.combat.OnParrySucceed += FirstDamageUp;
        board.combat.OnAttackEnded -= FirstDamageDown;
    }
}
