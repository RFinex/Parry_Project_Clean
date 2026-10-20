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
    private bool isIntroRunning;

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

        EnemyAddState<EliteBTState>();
        EnemyAddState<EliteExecuteHurtState>();
    }

    private void BuildBT()
    {
        SequenceNode combatSequence = new();
        combatSequence.Add(new ActionNode(CheckCombatStarted));
        combatSequence.Add(BuildCombatTree());

        SelectorNode root = new();
        root.Add(combatSequence);
        root.Add(new ActionNode(Idle));

        rootNode = root;
    }

    protected abstract INode BuildCombatTree();

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
        if (!IsEnemyState(typeof(EliteBTState)))
            EnemyChangeState(typeof(EliteBTState));
    }

    protected PlayerController GetPlayer()
    {
        if (player == null && gameManager != null)
            player = gameManager.Player;

        if (player == null || player.Stats.IsDead)
            return null;

        return player;
    }

    private UniTask<INode.State> CheckCombatStarted(CancellationToken ctk)
    {
        if (GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        if (isCombatStart)
        {
            target = player.transform;
            isIdle = false;
            return UniTask.FromResult(INode.State.Success);
        }

        if (isIntroRunning)
            return UniTask.FromResult(INode.State.Running);

        Vector2 playerPos = player.transform.position;
        Vector2 enemyPos = transform.position;
        float distance = (playerPos - enemyPos).sqrMagnitude;
        float sqrDetectRange = data.DetectRange * data.DetectRange;

        if (distance > sqrDetectRange)
            return UniTask.FromResult(INode.State.Failure);

        isIntroRunning = true;
        target = player.transform;
        IntroAsync(player, ctk).Forget();

        return UniTask.FromResult(INode.State.Running);
    }

    private async UniTaskVoid IntroAsync(PlayerController introPlayer, CancellationToken ctk)
    {
        StopMove();
        gameManager.SetGameState(GameState.Cutscene);

        try
        {
            gameManager.SetCameraTarget(transform);
            await UniTask.Delay(TimeSpan.FromSeconds(data.IntroFocusDuration), cancellationToken: ctk);

            target = introPlayer.transform;
            LookAtTarget();
            await UniTask.Delay(TimeSpan.FromSeconds(data.IntroFaceDuration), cancellationToken: ctk);

            gameManager.SetCameraTarget(introPlayer.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(data.IntroReturnDuration), cancellationToken: ctk);

            isCombatStart = true;
            isIdle = false;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (gameManager != null && gameManager.Player != null)
                gameManager.SetCameraTarget(gameManager.Player.transform);

            if (gameManager != null && gameManager.CurrentState == GameState.Cutscene)
                gameManager.SetGameState(GameState.Play);

            isIntroRunning = false;
        }
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
        isIntroRunning = false;
        player = null;

        EnemyChangeState(typeof(EliteBTState), true);
    }

    /// <summary>
    /// 기절 상태 종료 시 실행
    /// </summary>
    public override void OnStaggerFinished()
    {
        EnemyChangeState(typeof(EliteBTState));
    }
}
