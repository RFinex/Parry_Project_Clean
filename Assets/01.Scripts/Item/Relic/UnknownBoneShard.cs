using UnityEngine;

public class UnknownBoneShard : Artifact
{
    private bool isHealthHalf;

    private StatPair applyPair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 15
    };

    public UnknownBoneShard()
    {
        // HP 50% 이하일 때 일반 공격 피해 +15%
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

        blackBoard.stats.OnHealthChanged -= CheckHealth;

        base.OnUnequip(board);
    }

    private void CheckHealth(float nowHp, float maxHp)
    {
        bool isApply = (nowHp / maxHp) <= 0.5f;

        if (isHealthHalf == isApply)
            return;

        isHealthHalf = isApply;

        if (isApply)
            DamageUp();
        else
            DamageDown();
    }

    private void DamageUp()
    {
        blackBoard.stats.AddStats(applyPair);
    }

    private void DamageDown()
    {
        blackBoard.stats.RemoveStats(applyPair);
    }
}
