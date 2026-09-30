using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseData", menuName = "Player/PlayerBaseData")]
public class PlayerBaseData : ScriptableObject
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground CHeck")]
    [SerializeField] private Vector2 checkSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Vector3 checkOffset = new Vector2(0f, -0.4f);
    [SerializeField] private float checkDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashTime = 0.1f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashCool = 0.5f;

    [Header("Stats")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private float maxStamina = 100f;

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
}
