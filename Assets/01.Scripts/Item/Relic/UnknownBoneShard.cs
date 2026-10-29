using UnityEngine;

public class UnknownBoneShard : Artifact
{
    private bool isHealthHalf;

    public UnknownBoneShard()
    {

    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        base.OnEquip(board);

        CheckHpStat(blackBoard.stats.NowHp, blackBoard.stats.MaxHp);

        blackBoard.stats.OnHealthChanged -= CheckHpStat;
        blackBoard.stats.OnHealthChanged += CheckHpStat;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        CheckHpStat(board.stats.NowHp, board.stats.MaxHp);

        base.OnUnequip(board);

        blackBoard.stats.OnHealthChanged -= CheckHpStat;
    }

    private void CheckHpStat(float nowHp, float maxHp)
    {
        bool isApply = nowHp / maxHp <= 0.5f;

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
        blackBoard.stats.AddStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 15
            
        });
    }

    private void DamageDown()
    {
        blackBoard.stats.RemoveStats(new StatPair
        {
            id = StatId.AttackDamage,
            modifierType = StatModifierType.Percent,
            value = 15

        });
    }
}
