using UnityEngine;

public class BerserkersShield : Artifact
{
    public BerserkersShield()
    {
        // 패리 불가 / 일반 공격 피해 +65% / 대시 쿨타임 - 20%
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.AddParryBlock();
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.RemoveParryBlock();

        base.OnUnequip(board);
    }
}
