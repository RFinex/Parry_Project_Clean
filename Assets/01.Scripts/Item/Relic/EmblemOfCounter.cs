using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EmblemOfCounter : Artifact
{
    private CancellationTokenSource token;

    public EmblemOfCounter()
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
        token = token.Cancel_DisposeToken();

        base.OnUnequip(board);
    }

    private async UniTaskVoid DamageUpDurationAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ctk);

            blackBoard.stats.RemoveStats(new StatPair
            {
                id = StatId.AttackDamage,
                modifierType = StatModifierType.Percent,
                value = 80f
            });

            blackBoard.combat.OnParrySucceed -= FirstDamageUp;
            blackBoard.combat.OnParrySucceed += FirstDamageUp;
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            blackBoard.combat.OnAttackEnded -= FirstDamageDown;
        }
    }

    private void FirstDamageUp()
    {
        token = token.ResetToken();
        DamageUpDurationAsync(token.Token).Forget();

        blackBoard.stats.AddStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 80f
        });

        blackBoard.combat.OnParrySucceed -= FirstDamageUp;
        blackBoard.combat.OnAttackEnded += FirstDamageDown;
    }

    private void FirstDamageDown()
    {
        token = token.Cancel_DisposeToken();

        blackBoard.stats.RemoveStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 80f
        });

        blackBoard.combat.OnParrySucceed -= FirstDamageUp;
        blackBoard.combat.OnParrySucceed += FirstDamageUp;
        blackBoard.combat.OnAttackEnded -= FirstDamageDown;
    }
}
