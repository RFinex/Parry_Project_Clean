using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseData", menuName = "Enemy/EnemyBaseData")]
public class EnemyBaseData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float maxBalance = 100f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minPatrolDistance = 2f;
    [SerializeField] private float maxPatrolDistance = 4f;
    [SerializeField] private float arrivalDistance = 0.05f;

    [Header("Range")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float traceRange = 7f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackCool = 1f;
    [SerializeField] private float attackDelay = (5f / 14f);

    [Header("Stagger")]
    [SerializeField] private float staggerDuration = 3f;

    [Header("Idle")]
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;

    [Header("Execute")]
    [SerializeField] private float executeBackOffset = -0.8f;
    [SerializeField] private float executeDamageRatio = 1f;

    [Header("Death")]
    [SerializeField] private float deathDuration = 1f;

    [Header("Offset")]
    [SerializeField] private Vector2 detectOffset = new(0f, 0.3f);

    [Header("Intro Duration")]
    [SerializeField] private float introFocusDuration = 0.6f;
    [SerializeField] private float introFaceDuration = 0.5f;
    [SerializeField] private float introReturnDuration = 0.6f; 


    public float MaxHp => maxHp;
    public float MaxBalance => maxBalance;

    public float MoveSpeed => moveSpeed;
    public float MinPatrolDistance => minPatrolDistance;
    public float MaxPatrolDistance => maxPatrolDistance;
    public float ArrivalDistance => arrivalDistance;

    public float DetectRange => detectRange;
    public float AttackRange => attackRange;
    public float TraceRange => traceRange;

    public float AttackDamage => attackDamage;
    public float AttackCool => attackCool;
    public float AttackDelay => attackDelay;

    public float StaggerDuration => staggerDuration;

    public float MinIdleTime => minIdleTime;
    public float MaxIdleTime => maxIdleTime;

    public float ExecuteBakcOffset => executeBackOffset;
    public float ExecuteDamageRatio => executeDamageRatio;

    public float DeathDuration => deathDuration;

    public Vector2 DetectOffset => detectOffset;

    public float IntroFocusDuration => introFocusDuration;
    public float IntroFaceDuration => introFaceDuration;
    public float IntroReturnDuration => introReturnDuration;
}
