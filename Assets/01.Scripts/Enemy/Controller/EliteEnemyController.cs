using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class EliteEnemyController : EnemyController
{
    [Header("Skill Data")]
    [SerializeField] private BanditMeleeSkillData meleeData;
    [SerializeField] private BanditChargeSkillData chargeData;
    [SerializeField] private BanditJumpAttackSkillData jumpAttackData;

    private BanditMeleeSkill meleeSkill;
    private BanditChargeSkill chargeSkill;
    private BanditJumpAttackSkill jumpAttackSkill;

    private GameManager gameManager;
    private PlayerController player;

    private BehaviorTreeRunner runner;
    private INode rootNode;
    private CancellationTokenSource token;

    private bool isCombatStart;
    private bool isIdle;

    protected override bool UseStateMachine => false;

    protected override void Initialized()
    {
        base.Initialized();

        gameManager = GameManager.Instance;
        runner = new BehaviorTreeRunner();

        BuildBT();
    }

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

    protected override void OnEnable()
    {
        base.OnEnable();

        if (runner != null && !IsStaggerState() && !Stats.IsDead)
            StartBT();           
    }

    protected override void OnDisable()
    {
        StopBT();
        base.OnDisable();
    }

    private void BuildBT()
    {
        // 점프 공격 시퀀스 : 사거리 -> 쿨타임 -> 공격 시도
        SequenceNode jumpSequnce = new SequenceNode();
        jumpSequnce.Add(new ActionNode(CheckJumpRange));
        jumpSequnce.Add(new ActionNode(CheckJumpCooldown));
        jumpSequnce.Add(new ActionNode(UseJumpAttack));

        // 돌진 공격 시퀀스
        SequenceNode chargeSequence = new SequenceNode();
        chargeSequence.Add(new ActionNode(CheckChargeRange));
        chargeSequence.Add(new ActionNode(CheckChargeCooldown));
        chargeSequence.Add(new ActionNode(UseChargeAttack));

        // 근접 공격 시퀀스
        SequenceNode meleeSequence = new SequenceNode();
        meleeSequence.Add(new ActionNode(CheckMeleeRange));
        meleeSequence.Add(new ActionNode(CheckMeleeCooldown));
        meleeSequence.Add(new ActionNode(UseMeleeAttack));

        // 추적 시퀀스 : 타겟 생존 체크 -> 추적
        SequenceNode traceSequence = new SequenceNode();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        // 공격 루트 선택 : 점프 - 돌진 - 근접 - 추적 - 대기
        SelectorNode combatSelector = new SelectorNode();
        combatSelector.Add(jumpSequnce);
        combatSelector.Add(chargeSequence);
        combatSelector.Add(meleeSequence);
        combatSelector.Add(traceSequence);
        combatSelector.Add(new ActionNode(Idle));

        // 전투 시퀀스 : 탐지 사거리 체크 -> 공격 시퀀스
        SequenceNode combatSequence = new SequenceNode();
        combatSequence.Add(new ActionNode(CheckCombatStarted));
        combatSequence.Add(combatSelector);

        // 루트 선택 : 
        SelectorNode selector = new SelectorNode();
        selector.Add(combatSequence);
        selector.Add(new ActionNode(Idle));

        rootNode = selector;
    }

    private void StartBT()
    {
        StopBT();

        token = CancellationTokenSource.CreateLinkedTokenSource(DestroyToken);
        runner.Run(rootNode, token.Token).Forget();
    }

    private void StopBT()
    {
        token = token.Cancel_DisposeToken();

        meleeSkill?.Reset();
        chargeSkill?.Reset();
        jumpAttackSkill?.Reset();
    }

    #region BT Method

    private PlayerController GetPlayer()
    {
        if (player == null && gameManager != null)
            player = gameManager.Player;

        if (player == null || player.Stats.IsDead)
            return null;

        return player;
    }

    /// <summary>
    /// 일정 거리 내 플레이어 진입 시 전투 시작
    /// </summary>
    /// <param name="ctk"></param>
    /// <returns></returns>
    private UniTask<INode.State> CheckCombatStarted(CancellationToken ctk)
    {
        if (GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        if (!isCombatStart)
        {
            float distance = Mathf.Abs(player.transform.position.x - transform.position.x);

            if (distance > data.DetectRange)
            {
                return UniTask.FromResult(INode.State.Failure);
            }

            isCombatStart = true;
        }

        target = player.transform;
        isIdle = false;

        return UniTask.FromResult(INode.State.Success);
    }

    private UniTask<INode.State> CheckPlayerAlive(CancellationToken ctk)
    {
        if (GetPlayer() == null)
            return UniTask.FromResult(INode.State.Failure);

        target = player.transform;
        return UniTask.FromResult(INode.State.Success);
    }

    private UniTask<INode.State> CheckSkillRange(EnemySkill skill)
    {
        bool inRange = GetPlayer() != null && skill.IsInRange(this);

        return UniTask.FromResult(inRange ? INode.State.Success : INode.State.Failure);
    }

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

    private UniTask<INode.State> CheckSkillReady(EnemySkill skill)
    {
        return UniTask.FromResult(skill.IsReady ? INode.State.Success : INode.State.Failure);
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

    private UniTask<INode.State> TracePlayer(CancellationToken ctk)
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

    private UniTask<INode.State> Idle(CancellationToken ctk)
    {
        StopMove();

        if (!isIdle)
        {
            blackBoard.animator.Play(EnemyAnimationType.Idle);
            isIdle = true;
        }

        return UniTask.FromResult(INode.State.Running);
    }

    #endregion

    public override void Execute()
    {
        if (!CanExecute)
            return;

        TakeDamage(Stats.MaxHp * 0.25f);

        // 처형 당한 후 생존 시 밸런스 즉시 회복
        if (!Stats.IsDead)
            RecoverFromStagger();
    }

    public override void InitPool()
    {
        StopBT();
        base.InitPool();

        isCombatStart = false;
        isIdle = false;
        player = null;

        StartBT();
    }

    protected override void OnDeathStarted()
    {
        StopBT();
    }

    protected override void OnStaggerStarted()
    {
        StopBT();
    }

    public override void OnStaggerFinished()
    {
        StartBT();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (blackBoard == null || blackBoard.currentSkill == null)
            return;

        Vector2 center = (Vector2)transform.position + Vector2.right * frontDir * blackBoard.currentSkill.HitboxXOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, blackBoard.currentSkill.HitboxSize);
    }
#endif
}
