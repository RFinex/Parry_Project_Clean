using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Bandit : EliteEnemyController
{
    [Header("Bandit Skill Data")]
    [SerializeField] private BanditMeleeSkillData meleeData;
    [SerializeField] private BanditChargeSkillData chargeData;
    [SerializeField] private BanditJumpAttackSkillData jumpAttackData;

    private BanditMeleeSkill meleeSkill;
    private BanditChargeSkill chargeSkill;
    private BanditJumpAttackSkill jumpAttackSkill;

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        blackBoard.playerLayer = playerLayer;

        meleeSkill = new BanditMeleeSkill(meleeData);
        chargeSkill = new BanditChargeSkill(chargeData);
        jumpAttackSkill = new BanditJumpAttackSkill(jumpAttackData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(meleeSkill);
        blackBoard.skills.Add(chargeSkill);
        blackBoard.skills.Add(jumpAttackSkill);
    }

    protected override INode BuildCombatBT()
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

        SelectorNode combatSelector = new();
        combatSelector.Add(jumpSequence);
        combatSelector.Add(chargeSequence);
        combatSelector.Add(meleeSequence);
        combatSelector.Add(traceSequence);
        combatSelector.Add(new ActionNode(Idle));

        return combatSelector;
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
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (blackBoard == null || blackBoard.currentSkill == null)
            return;

        Vector2 center = (Vector2)transform.position + (frontDir * blackBoard.currentSkill.HitboxXOffset) * Vector2.right;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, blackBoard.currentSkill.HitboxSize);
    }
#endif
}
