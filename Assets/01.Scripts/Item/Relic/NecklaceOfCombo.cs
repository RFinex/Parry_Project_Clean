using UnityEngine;

public class NecklaceOfCombo : Artifact
{
    private StatPair applyPair;
    private bool isApply;

    public NecklaceOfCombo()
    {
        // 콤보 마지막 공격 피해 +60%, 첫 공격 피해 -15%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnAttackComboIndex -= ApplyDamage;
        board.combat.OnAttackEnded -= RemoveDamage;
        board.combat.OnAttackComboIndex += ApplyDamage;
        board.combat.OnAttackEnded += RemoveDamage;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnAttackComboIndex -= ApplyDamage;
        board.combat.OnAttackEnded -= RemoveDamage;

        RemoveDamage();

        base.OnUnequip(board);
    }
    
    public void ApplyDamage(int index)
    {
        RemoveDamage();

        float percent;

        if (index == 1)
            percent = -15f;
        else if (index == blackBoard.data.MaxCombo)
            percent = 60f;
        else
            return;

            applyPair = new StatPair
            {
                id = StatId.AttackDamage,
                modifierType = StatModifierType.Percent,
                value = percent
            };

        isApply = true;
        blackBoard.stats.AddStats(applyPair);
    }

    public void RemoveDamage()
    {
        // 효과가 적용되지 않았다면 제거 안함
        if (!isApply)
            return;

        isApply = false;
        blackBoard.stats.RemoveStats(applyPair);
    }
}
