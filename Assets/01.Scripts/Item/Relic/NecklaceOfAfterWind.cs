using UnityEngine;

public class NecklaceOfAfterWind : Artifact
{
    private float cooldownValue = 0.15f;

    public NecklaceOfAfterWind()
    {
        // 일반 공격 명중 시 대시의 남은 쿨타임 0.15초 감소
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.OnAttackHit -= ReduceCooldown;
        board.combat.OnAttackHit += ReduceCooldown;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        board.combat.OnAttackHit -= ReduceCooldown;

        base.OnUnequip(board);
    }

    private void ReduceCooldown()
    {
        blackBoard.skill.ReduceDashCooldown(cooldownValue);
    }
}
