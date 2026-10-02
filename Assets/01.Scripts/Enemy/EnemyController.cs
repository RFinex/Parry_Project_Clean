using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected EnemyBaseData data;
    public EnemyBaseData Data => data;

    protected EnemyBlackBoard blackBoard;

    [Header("Info")]
    [SerializeField] protected EnemyStats stats;
    [SerializeField] protected EnemyCombat combat;

    [Header("Layer Mask")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;

    private float moveSpeed;

    protected Rigidbody2D rb;

    protected Transform target;
    public Transform Target => target;

    public float FrontDir { get; private set; } = 1f;

    [Header("State Machine")]
    private StateMachine<EnemyController> stateMachine;
    private BaseState<EnemyController> currentState;
    public string currentStateName;

    private CancellationToken destroyToken;
    public CancellationToken DestroyToken => destroyToken;

    protected virtual void Awake()
    {
        Initialized();
    }

    private void Initialized()
    {
        destroyToken = this.GetCancellationTokenOnDestroy();

        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        combat = GetComponent<EnemyCombat>();

        blackBoard = new EnemyBlackBoard();

        stateMachine = new StateMachine<EnemyController>(this, blackBoard);

        stateMachine.AddState<EnemyAttackState>();
        stateMachine.AddState<EnemyDeadState>();
        stateMachine.AddState<EnemyIdleState>();
        stateMachine.AddState<EnemyPatrolState>();
        stateMachine.AddState<EnemyStaggerState>();
        stateMachine.AddState<EnemyTraceState>();

        stateMachine.ChangeState(typeof(EnemyIdleState));

        if(data != null)
            stats.Initialized(data);
    }

    public bool TryDetectTarget()
    {
        Vector2 origin = transform.position;

        Vector2 dir = Vector2.right * FrontDir;

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

    public bool IsTargetInAttackRange()
    {
        if (target == null)
            return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        return distance <= data.AttackRange;
    }

    public bool IsTargetInDetectRange()
    {
        if (target == null)
            return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        return distance <= data.DetectRange;
    }

    public bool IsTargetTraceRange()
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

        rb.linearVelocity = new Vector2(FrontDir * data.MoveSpeed, rb.linearVelocity.y);

        return Mathf.Abs(targetPos.x - transform.position.x) <= data.PatrolArrivalDistance;
    }

    public void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void Attack()
    {
        if (combat == null)
            return;

        combat.Attack(target);
    }
    
    protected void SetFrontDir(float direction)
    {
        if (direction == 0f)
            return;

        FrontDir = Mathf.Sign(direction);
    }

    public void LookAtTarget()
    {
        if (target == null)
            return;

        float direction = target.position.x - transform.position.x;

        SetFrontDir(direction);

    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
