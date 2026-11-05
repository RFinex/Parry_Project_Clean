using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class OldSamurai : EliteEnemyController
{
    [Header("Skill Data")]
    [SerializeField] private ParryComboSkillData parryComboData;
    [SerializeField] private OldSamuraiThrowSkillData throwData;

    private OldSamuraiParryComboSkill parryComboSkill;
    private OldSamuraiThrowSkill throwSkill;
    

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        parryComboSkill = new(parryComboData);
        throwSkill = new(throwData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(parryComboSkill);
        blackBoard.skills.Add(throwSkill);
    }

    protected override INode BuildCombatBT()
    {
        SequenceNode throwSequence = new();
        throwSequence.Add(new ActionNode(CheckThrowRange));
        throwSequence.Add(new ActionNode(CheckThrowCooldown));
        throwSequence.Add(new ActionNode(UseThrowAttack));

        SequenceNode parryComboSequence = new();
        parryComboSequence.Add(new ActionNode(CheckParryComboRange));
        parryComboSequence.Add(new ActionNode(CheckParryComboCooldown));
        parryComboSequence.Add(new ActionNode(UseParryComboAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode combatSelector = new();
        combatSelector.Add(throwSequence);
        combatSelector.Add(parryComboSequence);
        combatSelector.Add(traceSequence);
        combatSelector.Add(new ActionNode(Idle));

        return combatSelector;
    }

    #region BT Method
    
    private UniTask<INode.State> CheckThrowRange(CancellationToken ctk)
    {
        return CheckSkillRange(throwSkill);
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

    private UniTask<INode.State> CheckThrowCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(throwSkill);
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

    private UniTask<INode.State> UseThrowAttack(CancellationToken ctk)
    {
        return throwSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseParryComboAttack(CancellationToken ctk)
    {
        return parryComboSkill.TryUse(this, blackBoard, ctk);
    }
    #endregion
}
