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

    [Header("Stagger")]
    [SerializeField] private float staggerDuration = 3f;

    [Header("Idle")]
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;

    [Header("Execute")]
    [SerializeField] private float executeBackOffset = -0.8f;


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

    public float StaggerDuration => staggerDuration;

    public float MinIdleTime => minIdleTime;
    public float MaxIdleTime => maxIdleTime;

    public float ExecuteBakcOffset => executeBackOffset;
}
