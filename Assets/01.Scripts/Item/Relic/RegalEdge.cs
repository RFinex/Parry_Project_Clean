using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class RegalEdge : Artifact
{
    private float coolTime = 3f;
    private bool isReady = true;
    private int currentComboIndex;

    private CancellationTokenSource token;

    public RegalEdge()
    {
        // 가드·패리 불가 / 일반 공격 피해 + 100 % / 받는 피해 + 30 % / 콤보 마지막 공격 적중 시 대시 쿨타임 초기화.발동 간격 3초
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.combat.AddGuardBlock();
        board.combat.AddParryBlock();

        board.combat.OnAttackComboIndex -= SetComboIndex;
        board.combat.OnAttackHit -= ResetDashCool;
        board.combat.OnAttackComboIndex += SetComboIndex;
        board.combat.OnAttackHit += ResetDashCool;
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.combat.RemoveGuardBlock();
        board.combat.RemoveParryBlock();

        board.combat.OnAttackComboIndex -= SetComboIndex;
        board.combat.OnAttackHit -= ResetDashCool;

        base.OnUnequip(board);
    }

    private async UniTaskVoid CooltimeAsync(CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(coolTime), cancellationToken: ctk);
            isReady = true;

        }
        catch (OperationCanceledException)
        {

        }
    }

    private void SetComboIndex(int index)
    {
        currentComboIndex = index;
    }

    private void ResetDashCool()
    {
        if (!isReady || currentComboIndex != blackBoard.data.MaxCombo)
            return;

        isReady = false;

        token = token.LinkTokenOnly(blackBoard.owenrToken.Token);
        blackBoard.skill.ResetDashCooldown();
        CooltimeAsync(token.Token).Forget();
    }
}
