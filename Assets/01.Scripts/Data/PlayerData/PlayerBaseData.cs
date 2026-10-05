using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseData", menuName = "Player/PlayerBaseData")]
public class PlayerBaseData : ScriptableObject
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Vector2 checkSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Vector3 checkOffset = new Vector2(0f, -0.4f);
    [SerializeField] private float checkDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashTime = 0.1f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashCool = 0.5f;

    [Header("Stats")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float maxStamina = 100f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.6f, 0f);
    [SerializeField] private int maxCombo = 3;
    [SerializeField] private float comboInputTime = 0.25f;
    [SerializeField] private float attackEndTime = 0.5f;

    [Header("Guard")]
    [SerializeField] private float parryWindow = 0.15f;
    [SerializeField] private float parryDamage = 10f;

    [Header("Hurt")]
    [SerializeField] private float invincibleTime = 1f;
    
    [Header("Layer Mask")]
    [SerializeField] private LayerMask enemyLayer;

    public float MoveSpeed => moveSpeed;
    
    public float JumpForce => jumpForce;

    public Vector2 CheckSize => checkSize;
    public Vector3 CheckOffset => checkOffset;
    public float CheckDistance => checkDistance;
    public LayerMask GroundLayer => groundLayer;

    public float DashTime => dashTime;
    public float DashSpeed => dashSpeed;
    public float DashCool => dashCool;

    public float MaxHp => maxHp;
    public float MaxStamina => maxStamina;

    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public Vector2 AttackOffset => attackOffset;
    public int MaxCombo => maxCombo;
    public float ComboInputTime => comboInputTime;
    public float AttackEndTime => attackEndTime;

    public float ParryWindow => parryWindow;
    public float ParryDamage => parryDamage;

    public float InvincibleTime => invincibleTime;

    public LayerMask EnemyLayer => enemyLayer;
}
