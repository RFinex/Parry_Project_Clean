using UnityEngine;

public class EmberHeart : Artifact
{
    private bool isApplyHealth;

    private StatPair attackApplyPair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 30
    };

    private StatPair speedApplyPair = new()
    {
        id = StatId.MoveSpeed,
        modifierType = StatModifierType.Percent,
        value = 15
    };

    public EmberHeart()
    {
        // HP 35% 이하일 때 일반 공격 피해 +30%, 이동 속도 +15%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        CheckHealth(board.stats.NowHp, board.stats.MaxHp);

        board.stats.OnHealthChanged -= CheckHealth;
        board.stats.OnHealthChanged += CheckHealth;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        CheckHealth(board.stats.NowHp, board.stats.MaxHp);

        board.stats.OnHealthChanged -= CheckHealth;

        base.OnUnequip(board);
    }

    private void CheckHealth(float nowHp, float maxHp)
    {
        bool isApply = (nowHp / maxHp) <= 0.35f;

        if (isApplyHealth == isApply)
            return;

        isApplyHealth = isApply;

        if (isApply)
            DamageUp();
        else
            DamageDown();
    }

    private void DamageUp()
    {
        blackBoard.stats.AddStats(attackApplyPair);
        blackBoard.stats.AddStats(speedApplyPair);
    }

    private void DamageDown()
    {
        blackBoard.stats.RemoveStats(attackApplyPair);
        blackBoard.stats.RemoveStats(speedApplyPair);
    }
}
