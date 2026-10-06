using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public interface IExecuteTarget
{
    bool CanExecute {  get; }

    Vector2 GetExecutePos(Vector2 attackerPos);

    void Execute();
}

public class EnemyController : MonoBehaviour, IExecuteTarget
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

    protected Rigidbody2D rb;

    protected Transform target;
    public Transform Target => target;

    protected float frontDir = 1f;
    public float FrontDir => frontDir;

    [Header("State Machine")]
    private EnemyStateMachine stateMachine;

    [Header("Debug Check")]
    [SerializeField] private string currentStateName;

    private CancellationToken destroyToken;
    public CancellationToken DestroyToken => destroyToken;

    public bool CanExecute
    {
        get
        {
            return stateMachine.IsState(typeof(EnemyStaggerState));
        }
    }

    protected virtual void Awake()
    {
        Initialized();
    }

    protected virtual void Initialized()
    {
        destroyToken = this.GetCancellationTokenOnDestroy();

        rb = GetComponent<Rigidbody2D>();

        InitComponents();
        InitBlackBoard();
        InitStateMachine();
        InitData();

        stateMachine.ChangeState(typeof(EnemyIdleState));       
    }

    protected virtual void InitComponents()
    {
        stats = new EnemyStats();
        combat = new EnemyCombat();
    }

    protected virtual void InitBlackBoard()
    {
        blackBoard = new EnemyBlackBoard()
        {
            rb = rb,
            stats = stats,
            combat = combat
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

    protected virtual void InitData()
    {
        if (data == null)
            return;

        stats.Initialized(data);
        combat.Initialized(data);
    }

    public bool TryDetectTarget()
    {
        Vector2 origin = transform.position;

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
        float dir = Random.value < 0.5f ? -1f : 1f;

        float distance = Random.Range(data.MinPatrolDistance, data.MaxPatrolDistance);

        return (Vector2)transform.position + Vector2.right * dir * distance;
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

    public void Attack()
    {
        if (combat == null)
            return;

        combat.Attack(target, this);
    }
    
    public void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);

        if (stats.IsDead)
        {
            stateMachine.ChangeState(typeof(EnemyDeadState));
            return;
        }
    }

    public void TakeBalanceDamage(float damage)
    {
        stats.TakeBalanceDamage(damage);

        if (stats.IsBalanceBroken)
        {
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        currentStateName = stateMachine.CurrentState.GetType().Name;
    }

    private void OnDrawGizmos()
    {
        if (data == null)
            return;

        Vector2 origin = transform.position;
        Vector2 dir = Vector2.right * FrontDir;

        Gizmos.DrawRay(origin, dir * Data.DetectRange);
    }
#endif
}
