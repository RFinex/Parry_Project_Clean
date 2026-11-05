using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class OldSamurai : EliteEnemyController
{
    [Header("Skill Data")]
    [SerializeField] private ParryComboSkillData parryComboData;

    private OldSamuraiParryComboSkill parryComboSkill;
    

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        parryComboSkill = new(parryComboData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(parryComboSkill);
    }

    protected override INode BuildCombatBT()
    {
        SequenceNode parryComboSequence = new();
        parryComboSequence.Add(new ActionNode(CheckParryComboRange));
        parryComboSequence.Add(new ActionNode(CheckParryComboCooldown));
        parryComboSequence.Add(new ActionNode(UseParryComboAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode combatSelector = new();
        combatSelector.Add(parryComboSequence);
        combatSelector.Add(traceSequence);
        combatSelector.Add(new ActionNode(Idle));

        return combatSelector;
    }

    #region BT Method
    
    private UniTask<INode.State> CheckParryComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(parryComboSkill);
    }

    private UniTask<INode.State> CheckSkillRange(EnemySkill skill)
    {
        bool inRange = GetPlayer() != null && skill != null && skill.IsInRange(this);
        return UniTask.FromResult(inRange ? INode.State.Success : INode.State.Failure);
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

    private UniTask<INode.State> UseParryComboAttack(CancellationToken ctk)
    {
        return parryComboSkill.TryUse(this, blackBoard, ctk);
    }
    #endregion
}
