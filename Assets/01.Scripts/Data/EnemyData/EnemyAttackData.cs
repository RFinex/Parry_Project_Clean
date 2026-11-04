using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAttackData : ScriptableObject
{
    [SerializeField] private bool canInterrupted = true;

    public bool CanInterrupted => canInterrupted;

    [Header("Animator")]
    [SerializeField] private EnemyAnimationType animType;

    [Header("Animation Timing")]
    [SerializeField] private float warningDelay = (15f / 40f);
    [SerializeField] private float warningDuration = (20f / 40f);
    [SerializeField] private float attackRecovery = (10f / 40f);

    [Header("Attack")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private Vector2 hitboxOffset;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Guard / Parry")]
    [SerializeField] private bool canGuard = true;
    [SerializeField] private bool canParry = true;

    [Header("Attack Range")]
    [FormerlySerializedAs("minThrowDistance")]
    [SerializeField, Min(0f)] private float minAttackRange;

    [FormerlySerializedAs("normalAttackRange")]
    [SerializeField, Min(0f)] private float maxAttackRange = 1.5f;

    public EnemyAnimationType AnimType => animType;

    public float WarningDelay => warningDelay;
    public float WarningDuration => warningDuration;
    public float AttackRecovery => attackRecovery;

    public float Damage => damage;
    public Vector2 HitboxSize => hitboxSize;
    public Vector2 HitboxOffset => hitboxOffset;
    public float AttackCooldown => attackCooldown;

    public bool CanGuard => canGuard;
    public bool CanParry => canParry;

    public float MinAttackRange => minAttackRange;
    public float MaxAttackRange => maxAttackRange;
    
}
