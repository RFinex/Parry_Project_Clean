using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public abstract class EliteEnemyController : EnemyController
{
    private GameManager gameManager;
    private PlayerController player;

    private BehaviorTreeRunner runner;
    private INode rootNode;

    private bool isCombatStart;
    private bool isIdle;

    protected bool isIntroRunning;

    protected override bool UseStateMachine => false;

    protected override void Initialized()
    {
        base.Initialized();

        gameManager = GameManager.Instance;
        runner = new BehaviorTreeRunner();
        BuildBT();
    }

    protected override void AddStates()
    {
        base.AddStates();

        EnemyAddState<BTState>();
        EnemyAddState<EliteExecuteHurtState>();
    }

    protected void BuildBT()
    {
        SequenceNode combatSequence = new();
        combatSequence.Add(new ActionNode(CheckCombatStarted));
        combatSequence.Add(BuildCombatBT());

        SelectorNode root = new();
        root.Add(combatSequence);
        root.Add(new ActionNode(Idle));

        rootNode = root;
    }

    protected abstract INode BuildCombatBT();

    internal void StartBT(CancellationToken ctk)
    {
        if (runner == null || rootNode == null)
            return;

        runner.Run(rootNode, ctk).Forget();
    }

    internal void StopBT()
    {
        if (blackBoard == null)
            return;

        foreach (EnemySkill skill in blackBoard.skills)
            skill?.Reset();
    }

    protected virtual void Start()
    {
        if (!IsEnemyState(typeof(BTState)))
            EnemyChangeState(typeof(BTState));
    }

    protected PlayerController GetPlayer()
    {
        if (player == null && gameManager != null)
            player = gameManager.Player;

        if (player == null || player.Stats.IsDead)
            return null;

        return player;
    }

    public void FacePlayer(PlayerController player)
    {
        if (player == null)
            return;

        this.player = player;
        target = player.transform;
        LookAtTarget();
    }

    public void StartCombat(PlayerController player)
    {
        if (player == null)
            return;

        this.player = player;
        target = player.transform;
        isCombatStart = true;
        isIdle = false;
    }

    private UniTask<INode.State> CheckCombatStarted(CancellationToken ctk)
    {
        if (!isCombatStart || GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        target = player.transform;
        isIdle = false;

        return UniTask.FromResult(INode.State.Success);
    }

    protected UniTask<INode.State> CheckPlayerAlive(CancellationToken ctk)
    {
        if (GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        target = player.transform;
        return UniTask.FromResult(INode.State.Success);
    }

    protected UniTask<INode.State> TracePlayer(CancellationToken ctk)
    {
        if (GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        target = player.transform;
        isIdle = false;

        LookAtTarget();
        blackBoard.animator.Play(EnemyAnimationType.Move);
        Move(player.transform.position);

        return UniTask.FromResult(INode.State.Running);
    }

    protected UniTask<INode.State> Idle(CancellationToken ctk)
    {
        if (isIntroRunning)
            return UniTask.FromResult(INode.State.Running);

        StopMove();

        if (!isIdle)
        {
            blackBoard.animator.Play(EnemyAnimationType.Idle);
            isIdle = true;
        }

        return UniTask.FromResult(INode.State.Running);
    }

    public override void Execute()
    {
        if (!CanExecute)
            return;

        TakeDamage(Stats.MaxHp * data.ExecuteDamageRatio);

        if (Stats.IsDead)
            return;

        Stats.RestoreBalance();
        EnemyChangeState(typeof(EliteExecuteHurtState));
    }

    public override void InitPool()
    {
        base.InitPool();

        isCombatStart = false;
        isIdle = false;
        player = null;

        EnemyChangeState(typeof(BTState), true);
    }

    /// <summary>
    /// 기절 상태 종료 시 실행
    /// </summary>
    public override void OnStaggerFinished()
    {
        EnemyChangeState(typeof(BTState));
    }
}
