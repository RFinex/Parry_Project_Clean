using UnityEngine;

public class BerserkersShield : Artifact
{
    public BerserkersShield()
    {

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
