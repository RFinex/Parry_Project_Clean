using UnityEngine;

public class EnemySkillData : ScriptableObject
{
    [Header("Common Set")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float cooldown;
    [SerializeField] private float damage;

    [Header("Attack Delay(Animation")]
    [SerializeField] private float attackDelay;

    [Header("Attack Animation")]
    [SerializeField] private EnemyAnimationType animationType;

    [Header("Hitbox")]
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private float hitboxXOffset;

    [Header("Start Up")]
    [SerializeField] private float startupDelay;
    [SerializeField] private float recoveryDelay;

    [Header("Guard/Parry")]
    [SerializeField] private bool canGuard = true;
    [SerializeField] private bool canParry = true;

    public float MinRange => minRange;
    public float MaxRange => maxRange;
    public float Cooldown => cooldown;
    public float Damage => damage;

    public float AttackDelay => attackDelay;

    public EnemyAnimationType AnimationType => animationType;

    public Vector2 HitboxSize => hitboxSize;
    public float HitboxXOffset => hitboxXOffset;

    public float StartupDelay => startupDelay;
    public float RecoveryDelay => recoveryDelay;

    public bool CanGuard => canGuard;
    public bool CanParry => canParry;
}
