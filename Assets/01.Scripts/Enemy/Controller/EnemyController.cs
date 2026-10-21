using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public interface IExecuteTarget
{
    bool CanExecute {  get; }

    Vector2 GetExecutePos(Vector2 attackerPos);

    void Execute();
}

public class EnemyController : MonoBehaviour, IExecuteTarget, IPoolable
{
    [Header("Data")]
    [SerializeField] protected EnemyBaseData data;
    public EnemyBaseData Data => data;

    protected EnemyBlackBoard blackBoard;

    [Header("Info")]
    protected EnemyStats stats;
    protected EnemyCombat combat;

    public EnemyStats Stats => stats;
    public EnemyCombat Combat => combat;

    [Header("Layer Mask")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float wallCheckHeight = 0.3f;

    [Header("Ground Check")]
    [SerializeField] protected float groundCheckForwardOffset = 0.1f;
    [SerializeField] protected float groundCheckDistance = 0.2f;
    [SerializeField] protected Transform groundCheckPoint;

    [Header("Death")]
    protected float deathDuration;
    public float DeathDuration => deathDuration;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator animator;

    protected Transform target;
    public Transform Target => target;

    protected float frontDir = 1f;
    public float FrontDir => frontDir;

    [Header("State Machine")]
    private EnemyStateMachine stateMachine;

    [Header("Left Facses")]
    [SerializeField] private bool isLeftFaces;

    [Header("Debug Check")]
    [SerializeField] private string currentStateName;

    [Header("Animation Info")]
    [SerializeField] private EnemyAnimationInfo[] animations;

    private CancellationTokenSource token;
    public CancellationToken DestroyToken => token.Token;

    protected virtual bool UseStateMachine => true;
    public bool UseStateMachineBehavior => UseStateMachine;

    public bool CanExecute
    {
        get
        {
            return stateMachine.IsState(typeof(EnemyStaggerState));
        }
    }

    private IPool pool;
    private int objectId;

    private EnemyStatusBar statusBar;

    protected virtual void Awake()
    {
        Initialized();
    }

    protected virtual void OnEnable()
    {
        if (token == null || token.IsCancellationRequested)
        {
            token?.Dispose();
            token = new CancellationTokenSource();
        }

        UpdateAsync(token.Token).Forget();
    }

    protected virtual void OnDisable()
    {
        token?.Cancel();
    }

    protected async UniTaskVoid UpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                SpriteCheck();

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    protected virtual void Initialized()
    {
        token = token.ResetToken();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        deathDuration = data.DeathDuration;

        InitComponents();
        InitBlackBoard();
        InitStateMachine();
        InitData();

        if (UseStateMachine)
        {
            stateMachine.ChangeState(typeof(EnemyIdleState));
        } 
    }

    protected virtual void InitComponents()
    {
        stats = new EnemyStats();
        combat = new EnemyCombat();
    }

    protected virtual void InitBlackBoard()
    {
        EnemyAnimator enemyAnimator = new(animator, animations);

        blackBoard = new EnemyBlackBoard()
        {
            rb = rb,
            stats = stats,
            combat = combat,
            sr = sr,
            animator = enemyAnimator
        };
    }

    protected virtual void InitStateMachine()
    {
        stateMachine = new EnemyStateMachine(this, blackBoard);

        AddStates();
    }

    protected virtual void AddStates()
    {
        stateMachine.EnemyAddState<EnemyAttackState>();
        stateMachine.EnemyAddState<EnemyAttackState>();
        stateMachine.EnemyAddState<EnemyDeadState>();
        stateMachine.EnemyAddState<EnemyIdleState>();
        stateMachine.EnemyAddState<EnemyPatrolState>();
        stateMachine.EnemyAddState<EnemyStaggerState>();
        stateMachine.EnemyAddState<EnemyTraceState>();
    }

    #region State Machine

    protected void EnemyAddState<TState>() where TState : EnemyBaseState, new()
    {
        stateMachine.EnemyAddState<TState>();
    }

    protected void EnemyChangeState(Type stateType, bool isForce = false)
    {
        stateMachine.ChangeState(stateType, isForce);
    }

    protected bool IsEnemyState(Type stateType)
    {
        return stateMachine.IsState(stateType);
    }

    #endregion

    protected virtual void InitData()
    {
        if (data == null)
            return;

        stats.Initialized(data);
        combat.Initialized(data);
    }

    public void SetStatusBar(EnemyStatusBar statusBar)
    {
        this.statusBar = statusBar;
    }

    public bool TryDetectTarget()
    {
        Vector2 origin = (Vector2)transform.position + data.DetectOffset;

        Vector2 dir = Vector2.right * frontDir;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, data.DetectRange, playerLayer | obstacleLayer);

        if (hit.collider == null)
        {
            target = null;
            return false;
        }

        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            target = null;
            return false;
        }

        if (((1 << hit.collider.gameObject.layer) & playerLayer) == 0)
        {
            target = null;
            return false;
        }

        target = hit.collider.transform;

        return true;
    }

    public void ClearTarget()
    {
        target = null;
    }

    public Vector2 GetRandomPatrolPos()
    {
        float dir = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        float distance = UnityEngine.Random.Range(data.MinPatrolDistance, data.MaxPatrolDistance);

        return (Vector2)transform.position + Vector2.right * dir * distance;
    }
    public Vector2 GetRandomPatrolPos(float dir)
    {
        float distance = UnityEngine.Random.Range(data.MinPatrolDistance, data.MaxPatrolDistance);

        return (Vector2)transform.position + Vector2.right * Mathf.Sign(dir) * distance;
    }

    public bool IsGroundAhead(float dir)
    {
        if (groundCheckPoint == null)
            return true;

        Vector2 origin = (Vector2)groundCheckPoint.position + Vector2.right * (Mathf.Sign(dir) * groundCheckForwardOffset);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    public bool IsWallAhead(float dir)
    {
        if (dir == 0f)
            return false;

        Vector2 origin = (Vector2)transform.position + Vector2.up * wallCheckHeight;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, wallCheckDistance, obstacleLayer);

        return hit.collider != null;
    }

    /// <summary> 타겟이 공격 범위에 있는지 체크 </summary>
    public bool IsTargetInAttackRange()
    {
        if (target == null)
            return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        return distance <= data.AttackRange;
    }

    /// <summary> 타겟이 탐지 범위에 있는지 체크 </summary>
    public bool IsTargetInDetectRange()
    {
        if (target == null)
            return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        return distance <= data.DetectRange;
    }

    /// <summary> 타겟이 추격 범위에 있는지 체크 </summary>
    public bool IsTargetInTraceRange()
    {
        if (target == null)
            return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        return distance <= data.TraceRange;
    }

    public void SpriteCheck()
    {
        if (sr == null || frontDir == 0f)
            return;

        sr.flipX = frontDir > 0f == isLeftFaces;
    }

    public bool Move(Vector2 targetPos)
    {
        float direction = targetPos.x - transform.position.x;

        SetFrontDir(direction);

        rb.linearVelocity = new Vector2(frontDir * data.MoveSpeed, rb.linearVelocity.y);

        // 도착 지점과의 거리가 ArrivalDistance 보다 가까운지 체크
        return Mathf.Abs(targetPos.x - transform.position.x) <= data.ArrivalDistance;
    }

    public void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public virtual async UniTask AttackAsync(CancellationToken ctk)
    {
        try
        {
            LookAtTarget();
            blackBoard.animator.PlayFromStart(EnemyAnimationType.Attack);

            if (data.AttackDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(data.AttackDelay), cancellationToken: ctk);
            }

            // 공격 선딜 도중 목표가 벗어나면 타격X
            if (target == null || !IsTargetInAttackRange())
                return;

            // 실제 공격 판정
            ApplyAttackHit();

            // 선딜을 뺀 쿨타임
            float cool = data.AttackCool - data.AttackDelay;

            if (cool > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(cool), cancellationToken: ctk);
            }
        }
        catch(OperationCanceledException)
        {

        }
    }

    protected virtual void ApplyAttackHit()
    {
        combat.Attack(target, this);
    }
    
    public virtual void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);

        if (stats.IsDead)
        {
            OnDeathStarted();
            stateMachine.ChangeState(typeof(EnemyDeadState));
            return;
        }
    }

    public void TakeBalanceDamage(float damage)
    {
        stats.TakeBalanceDamage(damage);

        if (stats.IsBalanceBroken)
        {
            OnStaggerStarted();
            stateMachine.ChangeState(typeof(EnemyStaggerState));
            return;
        }
    }

    protected void SetFrontDir(float direction)
    {
        if (direction == 0f)
            return;

        frontDir = Mathf.Sign(direction);
    }

    public void LookAtTarget()
    {
        if (target == null)
            return;

        float dir = target.position.x - transform.position.x;

        SetFrontDir(dir);
    }

    public bool IsStaggerState()
    {
        return stateMachine.IsState(typeof(EnemyStaggerState));
    }

    /// <summary>
    /// 처형 대상(본인)의 위치를 알려줌
    /// </summary>
    /// <param name="attackerPos"> 공격자(플레이어)의 위치 </param>
    /// <returns></returns>
    public Vector2 GetExecutePos(Vector2 attackerPos)
    {       
        float dir = Mathf.Sign(transform.position.x - attackerPos.x);

        return new Vector2(transform.position.x - dir * data.ExecuteBakcOffset, transform.position.y);
    }


    /// <sumamry> 처형 함수 </sumamry>
    public virtual void Execute()
    {
        if (!CanExecute)
            return;

        stats.TakeDamage(stats.NowHp);

        if (stats.IsDead)
        {
            stateMachine.ChangeState(typeof(EnemyDeadState));
        }
    }

    /// <summary> 처형당한 후 밸런스 회복 </summary>
    public void RecoverFromStagger()
    {
        if (!IsStaggerState())
            return;

        stats.RestoreBalance();
        OnStaggerFinished();
    }

    public event Action<EnemyController> OnDeathFinished;

    public void FinishDeath()
    {
        ReturnPool();
        OnDeathFinished?.Invoke(this);
    }

    #region Object Pool

    public void SetPool(IPool pool, int objectId)
    {
        this.pool = pool;
        this.objectId = objectId;
    }

    public virtual void InitPool()
    {
        ResetPoolState();

        if (sr != null)
        {
            Color color = sr.color;
            color.a = 1f;
            sr.color = color;
        }

        if (UseStateMachine)
        {
            stateMachine.ChangeState(typeof(EnemyIdleState), true);
        }
    }

    public virtual void InitPoolReturn()
    {
        ClearTarget();

        StopMove();

        if(rb != null)
            rb.linearVelocity = Vector2.zero;

        if (statusBar != null)
        {
            statusBar.ReturnPool();
            statusBar = null;
        }
    }

    public void ReturnPool()
    {
        if (pool == null)
            return;

        pool?.Return(this, objectId);
    }

    protected virtual void ResetPoolState()
    {
        if (data == null)
            return;

        stats.Initialized(data);

        target = null;
        frontDir = 1f;

        rb.linearVelocity = Vector2.zero;
    }
    #endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (stateMachine.CurrentState != null)
        {
            currentStateName = stateMachine.CurrentState.GetType().Name;
        }
    }

    private void OnDrawGizmos()
    {
        if (data == null)
            return;

        Vector2 origin = (Vector2)transform.position + data.DetectOffset;
        Vector2 dir = Vector2.right * FrontDir;

        Gizmos.DrawRay(origin, dir * Data.DetectRange);

        Vector2 origin2 = (Vector2)transform.position + Vector2.up * wallCheckHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin2, dir * wallCheckDistance);
    }    
#endif

    public bool IsGround()
    {
        if (groundCheckPoint == null)
            return false;

        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    protected virtual void OnDeathStarted()
    {

    }

    protected virtual void OnStaggerStarted()
    {

    }

    public virtual void OnStaggerFinished()
    {
        EnemyChangeState(typeof(EnemyIdleState));
    }
}
