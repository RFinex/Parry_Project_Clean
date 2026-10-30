using UnityEngine;

public class BloodveinCrystal : Artifact
{
    private float healRate = 0.15f;
    private bool isApply;

    private StatPair damagePair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 75f
    };

    public BloodveinCrystal()
    {
        // 최대 HP -25%
        // 처형 성공 시 최대 HP의 15% 회복, 다음 일반 공격 피해 +75%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= ApplyEffect;
        board.combat.OnAttackEnded -= RemoveEffect;
        board.combat.OnExecuteSucceed += ApplyEffect;
        board.combat.OnAttackEnded += RemoveEffect;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= ApplyEffect;
        board.combat.OnAttackEnded -= RemoveEffect;

        RemoveEffect();

        base.OnUnequip(board);
    }

    private void ApplyEffect()
    {
        Heal();

        if (isApply)
            return;

        isApply = true;

        blackBoard.stats.AddStats(damagePair);
    }

    private void RemoveEffect()
    {
        if (!isApply)
            return;

        isApply = false;

        blackBoard.stats.RemoveStats(damagePair);
    }

    private void Heal()
    {
        float value = blackBoard.stats.MaxHp * healRate;
        blackBoard.stats.RestoreHp(value);
    }
}
