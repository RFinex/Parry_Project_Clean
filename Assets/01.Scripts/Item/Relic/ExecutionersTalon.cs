using UnityEngine;

public class ExecutionersTalon : Artifact
{
    public ExecutionersTalon()
    {

    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        base.OnEquip(board);

        board.combat.OnExecuteSucceed -= Heal;
        board.combat.OnExecuteSucceed += Heal;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        board.combat.OnExecuteSucceed -= Heal;
    
        base.OnUnequip(board);
    }

    private void Heal()
    {
        float value = blackBoard.stats.MaxHp * 0.03f;
        blackBoard.stats.RestoreHp(value);
    }
}
