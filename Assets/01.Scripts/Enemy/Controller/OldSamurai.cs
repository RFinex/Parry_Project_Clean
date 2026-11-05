using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class OldSamurai : EliteEnemyController
{
    [Header("Skill Data")]
    [SerializeField] private ParryComboSkillData parryComboData;
    [SerializeField] private OldSamuraiThrowSkillData throwData;
    [SerializeField] private OldSamuraiUpperSkillData upperData;
    [SerializeField] private OldSamuraiLargeComboAttackData largeComboData;

    private OldSamuraiParryComboSkill parryComboSkill;
    private OldSamuraiThrowSkill throwSkill;
    private OldSamuraiUpperSkill upperSkill;
    private OldSamuraiLargeComboAttackSkill largeComboSkill;

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        parryComboSkill = new(parryComboData);
        throwSkill = new(throwData);
        upperSkill = new(upperData);
        largeComboSkill = new(largeComboData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(parryComboSkill);
        blackBoard.skills.Add(throwSkill);
        blackBoard.skills.Add(upperSkill);
        blackBoard.skills.Add(largeComboSkill);
    }

    protected override INode BuildCombatBT()
    {
        SequenceNode largeComboSequence = new();
        largeComboSequence.Add(new ActionNode(CheckLargeComboRange));
        largeComboSequence.Add(new ActionNode(CheckLargeComboCooldown));
        largeComboSequence.Add(new ActionNode(UseLargeComboAttack));

        SequenceNode throwSequence = new();
        throwSequence.Add(new ActionNode(CheckThrowRange));
        throwSequence.Add(new ActionNode(CheckThrowCooldown));
        throwSequence.Add(new ActionNode(UseThrowAttack));

        SequenceNode upperSequence = new();
        upperSequence.Add(new ActionNode(CheckUpperRange));
        upperSequence.Add(new ActionNode(CheckUpperCooldown));
        upperSequence.Add(new ActionNode(UseUpperAttack));

        SequenceNode parryComboSequence = new();
        parryComboSequence.Add(new ActionNode(CheckParryComboRange));
        parryComboSequence.Add(new ActionNode(CheckParryComboCooldown));
        parryComboSequence.Add(new ActionNode(UseParryComboAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode combatSelector = new();
        combatSelector.Add(largeComboSequence);
        combatSelector.Add(throwSequence);
        combatSelector.Add(upperSequence);
        combatSelector.Add(parryComboSequence);
        combatSelector.Add(traceSequence);
        combatSelector.Add(new ActionNode(Idle));

        return combatSelector;
    }

    #region BT Method
    
    private UniTask<INode.State> CheckLargeComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(largeComboSkill);
    }
    private UniTask<INode.State> CheckThrowRange(CancellationToken ctk)
    {
        return CheckSkillRange(throwSkill);
    }

    private UniTask<INode.State> CheckUpperRange(CancellationToken ctk)
    {
        return CheckSkillRange(upperSkill);
    }

    private UniTask<INode.State> CheckParryComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(parryComboSkill);
    }

    private UniTask<INode.State> CheckSkillRange(EnemySkill skill)
    {
        bool inRange = GetPlayer() != null && skill != null && skill.IsInRange(this);
        return UniTask.FromResult(inRange ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> CheckLargeComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(largeComboSkill);
    }

    private UniTask<INode.State> CheckThrowCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(throwSkill);
    }
    private UniTask<INode.State> CheckUpperCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(upperSkill);
    }
    private UniTask<INode.State> CheckParryComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(parryComboSkill);
    }

    private UniTask<INode.State> CheckSkillReady(EnemySkill skill)
    {
        bool isReady = skill != null && skill.IsReady;
        return UniTask.FromResult(isReady ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> UseLargeComboAttack(CancellationToken ctk)
    {
        return largeComboSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseThrowAttack(CancellationToken ctk)
    {
        return throwSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseUpperAttack(CancellationToken ctk)
    {
        return upperSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseParryComboAttack(CancellationToken ctk)
    {
        return parryComboSkill.TryUse(this, blackBoard, ctk);
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        DrawSkillHitbox(parryComboData, Color.red);

        DrawSkillHitbox(upperData, Color.yellow);
    }

    private void DrawSkillHitbox(EnemySkillData skillData, Color color)
    {
        if (skillData == null)
            return;

        Vector2 offset = skillData.HitboxOffset;
        offset.x *= frontDir;

        Vector2 center = (Vector2)transform.position + offset;

        Gizmos.color = color;
        Gizmos.DrawWireCube(center, skillData.HitboxSize);
    }
#endif
}
