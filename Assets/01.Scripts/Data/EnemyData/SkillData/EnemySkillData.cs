using UnityEngine;

public class EnemySkillData : ScriptableObject
{
    [Header("Common Set")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [SerializeField] private float cooldown;
    [SerializeField] private float damage;
    [SerializeField] private float attackDelay;
    [SerializeField] private EnemyAnimationType animationType;
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private float hitboxXOffset;

    public float MinRange => minRange;
    public float MaxRange => maxRange;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float AttackDelay => attackDelay;
    public EnemyAnimationType AnimationType => animationType;
    public Vector2 HitboxSize => hitboxSize;
    public float HitboxXOffset => hitboxXOffset;
}
