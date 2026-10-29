using UnityEngine;

public class TalonOfExecutioner : Artifact
{
    public TalonOfExecutioner()
    {
        // 처형 성공 시 최대 HP의 3% 회복
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= Heal;
        board.combat.OnExecuteSucceed += Heal;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.OnExecuteSucceed -= Heal;
    
        base.OnUnequip(board);
    }

    private void Heal()
    {
        float value = blackBoard.stats.MaxHp * 0.03f;
        blackBoard.stats.RestoreHp(value);
    }
}
