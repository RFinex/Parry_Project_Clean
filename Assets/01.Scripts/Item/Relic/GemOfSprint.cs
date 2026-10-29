using UnityEngine;

public class GemOfSprint : Artifact
{
    private bool isReady;
    private float refundRatio = 0.5f;

    public GemOfSprint()
    {
        // 이동 속도 +20%, 최대 HP -20% / 대시 후 첫 일반 공격 적중 시 대시 쿨타임 50 % 반환
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.skill.OnDashEnded -= RefundReady;
        board.combat.OnAttackHit -= RefundCooldown;
        board.skill.OnDashEnded += RefundReady;
        board.combat.OnAttackHit += RefundCooldown;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.skill.OnDashEnded -= RefundReady;
        board.combat.OnAttackHit -= RefundCooldown;

        isReady = false;

        base.OnUnequip(board);
    }

    private void RefundReady()
    {
        isReady = true;
    }

    private void RefundCooldown()
    {
        if (!isReady)
            return;

        isReady = false;

        float refund = blackBoard.stats.GetStats(StatId.DashCooldown) * refundRatio;
        blackBoard.skill.ReduceDashCooldown(refund);
    }
}
