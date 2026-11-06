using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public enum BossPhase
{
    Phase1,
    Phase2
}

public abstract class BossController : EliteEnemyController
{
    [SerializeField] private BossData bossData;
    protected BossData BossData => bossData;

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    private bool isPhase2Transition;
    protected virtual bool HasPhase2Intro => false;

    protected override INode BuildCombatBT()
    {
        SelectorNode root = new();

        SequenceNode phase2 = new();
        phase2.Add(new ActionNode(CheckPhase2));
        phase2.Add(BuildPhase2BT());

        SequenceNode phase1 = new();
        phase1.Add(new ActionNode(CheckPhase1));
        phase1.Add(BuildPhase1BT());

        root.Add(new ActionNode(CheckPhase2Transition));
        root.Add(phase2);
        root.Add(phase1);

        return root;
    }

    protected abstract INode BuildPhase1BT();
    protected abstract INode BuildPhase2BT();

    // 1페이즈 체크
    private UniTask<INode.State> CheckPhase1(CancellationToken ctk)
    {
        return UniTask.FromResult(CurrentPhase == BossPhase.Phase1 ? INode.State.Success : INode.State.Failure);
    }

    // 2페이즈 체크
    private UniTask<INode.State> CheckPhase2(CancellationToken ctk)
    {
        return UniTask.FromResult(CurrentPhase == BossPhase.Phase2 ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> CheckPhase2Transition(CancellationToken ctk)
    {
        return UniTask.FromResult(isPhase2Transition ? INode.State.Running : INode.State.Failure);
    }

    protected virtual UniTask PlayPhase2IntroAsync(CancellationToken ctk)
    {
        return UniTask.CompletedTask;
    }

    protected void CompletePhase2Transition()
    {
        isPhase2Transition = false;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (Stats.IsDead || CurrentPhase == BossPhase.Phase2)
            return;

        // BT 패턴 실행 도중 페이즈 변경 시 강제로 BT 재시작(패턴 취소)
        if (Stats.NowHp <= Stats.MaxHp * bossData.Phase2HpRatio)
        {
            CurrentPhase = BossPhase.Phase2;

            if (HasPhase2Intro)
            {
                isPhase2Transition = true;

                if (IsEnemyState(typeof(BTState)))
                    EnemyChangeState(typeof(BTState), true);

                PlayPhase2IntroAsync(DestroyToken).Forget();
            }
            else if (IsEnemyState(typeof(BTState)))
            {
                EnemyChangeState(typeof(BTState), true);
            }
        }
    }

    public override void InitPool()
    {
        CurrentPhase = BossPhase.Phase1;
        isPhase2Transition = false;
        base.InitPool();
    }

    public virtual void SetRoomCenter(Transform center)
    {

    }

    public abstract UniTask PlayIntroAsync(PlayerController player, Transform jumpPoint, Transform jumpPoint2, CancellationToken ctk);
}
