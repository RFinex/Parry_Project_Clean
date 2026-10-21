using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Bandit_Boss : BossController
{
    [Header("Phase 1 Skill Data")]
    [SerializeField] private BanditMeleeSkillData meleeData;
    [SerializeField] private BanditChargeSkillData chargeData;
    [SerializeField] private BanditJumpAttackSkillData jumpAttackData;

    [Header("Phase 2 Skill Data")]
    [SerializeField] private BanditMeleeSkillData phase2MeleeData;
    [SerializeField] private BanditChargeSkillData phase2ChargeData;
    [SerializeField] private BanditJumpAttackSkillData phase2JumpAttackData;

    private BanditMeleeSkill meleeSkill;
    private BanditChargeSkill chargeSkill;
    private BanditJumpAttackSkill jumpAttackSkill;

    private BanditMeleeSkill phase2MeleeSkill;
    private BanditChargeSkill phase2ChargeSkill;
    private BanditJumpAttackSkill phase2JumpAttackSkill;

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        blackBoard.playerLayer = playerLayer;

        meleeSkill = new BanditMeleeSkill(meleeData);
        chargeSkill = new BanditChargeSkill(chargeData);
        jumpAttackSkill = new BanditJumpAttackSkill(jumpAttackData);

        phase2MeleeSkill = new BanditMeleeSkill(phase2MeleeData);
        phase2ChargeSkill = new BanditChargeSkill(phase2ChargeData);
        phase2JumpAttackSkill = new BanditJumpAttackSkill(phase2JumpAttackData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(meleeSkill);
        blackBoard.skills.Add(chargeSkill);
        blackBoard.skills.Add(jumpAttackSkill);
        blackBoard.skills.Add(phase2MeleeSkill);
        blackBoard.skills.Add(phase2ChargeSkill);
        blackBoard.skills.Add(phase2JumpAttackSkill);
    }

    protected override INode BuildPhase1BT()
    {
        SequenceNode jumpSequence = new();
        jumpSequence.Add(new ActionNode(CheckJumpRange));
        jumpSequence.Add(new ActionNode(CheckJumpCooldown));
        jumpSequence.Add(new ActionNode(UseJumpAttack));

        SequenceNode chargeSequence = new();
        chargeSequence.Add(new ActionNode(CheckChargeRange));
        chargeSequence.Add(new ActionNode(CheckChargeCooldown));
        chargeSequence.Add(new ActionNode(UseChargeAttack));

        SequenceNode meleeSequence = new();
        meleeSequence.Add(new ActionNode(CheckMeleeRange));
        meleeSequence.Add(new ActionNode(CheckMeleeCooldown));
        meleeSequence.Add(new ActionNode(UseMeleeAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode phase1Selector = new();
        phase1Selector.Add(jumpSequence);
        phase1Selector.Add(chargeSequence);
        phase1Selector.Add(meleeSequence);
        phase1Selector.Add(traceSequence);
        phase1Selector.Add(new ActionNode(Idle));

        return phase1Selector;
    }

    protected override INode BuildPhase2BT()
    {
        SequenceNode jumpSequence = new();
        jumpSequence.Add(new ActionNode(CheckPhase2JumpRange));
        jumpSequence.Add(new ActionNode(CheckPhase2JumpCooldown));
        jumpSequence.Add(new ActionNode(UsePhase2JumpAttack));

        SequenceNode chargeSequence = new();
        chargeSequence.Add(new ActionNode(CheckPhase2ChargeRange));
        chargeSequence.Add(new ActionNode(CheckPhase2ChargeCooldown));
        chargeSequence.Add(new ActionNode(UsePhase2ChargeAttack));

        SequenceNode meleeSequence = new();
        meleeSequence.Add(new ActionNode(CheckPhase2MeleeRange));
        meleeSequence.Add(new ActionNode(CheckPhase2MeleeCooldown));
        meleeSequence.Add(new ActionNode(UsePhase2MeleeAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode phase2Selector = new();
        phase2Selector.Add(jumpSequence);
        phase2Selector.Add(chargeSequence);
        phase2Selector.Add(meleeSequence);
        phase2Selector.Add(traceSequence);
        phase2Selector.Add(new ActionNode(Idle));

        return phase2Selector;
    }

    #region BT Method
    private UniTask<INode.State> CheckJumpRange(CancellationToken ctk)
    {
        return CheckSkillRange(jumpAttackSkill);
    }

    private UniTask<INode.State> CheckChargeRange(CancellationToken ctk)
    {
        return CheckSkillRange(chargeSkill);
    }

    private UniTask<INode.State> CheckMeleeRange(CancellationToken ctk)
    {
        return CheckSkillRange(meleeSkill);
    }

    private UniTask<INode.State> CheckSkillRange(EnemySkill skill)
    {
        bool inRange = GetPlayer() != null && skill != null && skill.IsInRange(this);
        return UniTask.FromResult(inRange ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> CheckJumpCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(jumpAttackSkill);
    }

    private UniTask<INode.State> CheckChargeCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(chargeSkill);
    }

    private UniTask<INode.State> CheckMeleeCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(meleeSkill);
    }

    private UniTask<INode.State> CheckSkillReady(EnemySkill skill)
    {
        bool isReady = skill != null && skill.IsReady;
        return UniTask.FromResult(isReady ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> UseJumpAttack(CancellationToken ctk)
    {
        return jumpAttackSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseChargeAttack(CancellationToken ctk)
    {
        return chargeSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseMeleeAttack(CancellationToken ctk)
    {
        return meleeSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> CheckPhase2JumpRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2JumpAttackSkill);
    }

    private UniTask<INode.State> CheckPhase2ChargeRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2ChargeSkill);
    }

    private UniTask<INode.State> CheckPhase2MeleeRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2MeleeSkill);
    }

    private UniTask<INode.State> CheckPhase2JumpCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2JumpAttackSkill);
    }

    private UniTask<INode.State> CheckPhase2ChargeCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2ChargeSkill);
    }

    private UniTask<INode.State> CheckPhase2MeleeCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2MeleeSkill);
    }

    private UniTask<INode.State> UsePhase2JumpAttack(CancellationToken ctk)
    {
        return phase2JumpAttackSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UsePhase2ChargeAttack(CancellationToken ctk)
    {
        return phase2ChargeSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UsePhase2MeleeAttack(CancellationToken ctk)
    {
        return phase2MeleeSkill.TryUse(this, blackBoard, ctk);
    }
    #endregion
}
