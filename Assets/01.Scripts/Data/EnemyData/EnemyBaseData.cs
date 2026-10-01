using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBaseData", menuName = "Enemy/EnemyBaseData")]
public class EnemyBaseData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float maxBalance = 100f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Range")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float attackRange = 2f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackCool = 1f;

    [Header("Stagger")]
    [SerializeField] private float staggerDuration = 3f;


    public float MaxHp => maxHp;
    public float MaxBalance => maxBalance;

    public float MoveSpeed => moveSpeed;

    public float DetectRange => detectRange;
    public float AttackRange => attackRange;

    public float AttackDamage => attackDamage;
    public float AttackCool => attackCool;

    public float StaggerDuration => staggerDuration;
}
