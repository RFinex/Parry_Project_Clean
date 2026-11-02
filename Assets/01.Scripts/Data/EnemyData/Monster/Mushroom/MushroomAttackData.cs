using UnityEngine;

[CreateAssetMenu(fileName = "Attack_Data_Mushroom", menuName = "Enemy/Mushroom Attack")]
public class MushroomAttackData : ScriptableObject
{
    [Header("Animator")]
    [SerializeField] private EnemyAnimationType animType;

    [Header("Animation Timing")]
    [SerializeField] private float warningDelay = (15f / 40f);
    [SerializeField] private float warningDuration = (20f / 40f);
    [SerializeField] private float attackDuration = (5f / 40f);

    [Header("Attack")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private Vector2 hitboxOffset;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Guard / Parry")]
    [SerializeField] private bool canGuard = true;
    [SerializeField] private bool canParry = true;

    public EnemyAnimationType AnimType => animType;

    public float WarningDelay => warningDelay;
    public float WarningDuration => warningDuration;
    public float AttackDuration => attackDuration;

    public float Damage => damage;
    public Vector2 HitboxSize => hitboxSize;
    public Vector2 HitboxOffset => hitboxOffset;
    public float AttackCooldown => attackCooldown;

    public bool CanGuard => canGuard;
    public bool CanParry => canParry;
    
}
