using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Idle,
    Attack,
    Guard,
    Dash,
    Jump,
    Hit,
    Dead
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Player Data")]
    [SerializeField] private PlayerBaseData data;

    [Header("Player Info")]
    [SerializeField] private PlayerStats stats;

    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float frontDir = 1f;

    [Header("Jump")]
    [SerializeField] private float jumpForce;

    [Header("Ground CHeck")]
    [SerializeField] private Vector2 checkSize;
    [SerializeField] private float checkDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 checkOffset;

    [Header("Dash")]
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCool;
    [SerializeField] private float dashCheckTimer = 0f;
    [SerializeField] private float dashCoolTimer = 0f;

    private Vector2 moveInput;

    [Header("Bool Check")]
    [SerializeField] private bool isGround;
    [SerializeField] private bool isDash;

    private CancellationTokenSource token;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Initialized();
    }

    private void OnEnable()
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }

    private void Initialized()
    {
        moveSpeed = data.MoveSpeed;

        jumpForce = data.JumpForce;

        checkSize = data.CheckSize;
        checkOffset = data.CheckOffset;
        checkDistance = data.CheckDistance;
        groundLayer = data.GroundLayer;

        dashTime = data.DashTime;
        dashSpeed = data.DashSpeed;
        dashCool = data.DashCool;

        stats.Initialized(data);
    }

    private void Update()
    {
        GroundCheck();
    }

    private void FixedUpdate()
    {
        if (isDash)
            return;

        Move();
    }

    private void Move()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        // 플레이어 보는 방향 판별
        if (moveInput.x != 0f)
        {
            frontDir = Mathf.Sign(moveInput.x);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void GroundCheck()
    {
        Vector2 origin = transform.position + checkOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, checkSize, 0f, Vector2.down, checkDistance, groundLayer);

        isGround = hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 origin = transform.position + checkOffset;

        Gizmos.DrawWireCube(origin + Vector3.down * checkDistance, checkSize);
    }

    private void Dash()
    {
        Utils.Log<PlayerController>("대시 시작");
        isDash = true;

        DashAsync(token.Token).Forget();
        DashCoolTime(token.Token).Forget();
    }

    private async UniTaskVoid DashAsync(CancellationToken ctk)
    {
        try
        {
            dashCheckTimer = 0f;

            while (dashCheckTimer <= dashTime)
            {
                dashCheckTimer += Time.deltaTime;

                rb.linearVelocity = new Vector2(frontDir * dashSpeed, 0f);

                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            }
            Utils.Log<PlayerController>("대시 종료");
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            isDash = false;
        }
    }

    private async UniTaskVoid DashCoolTime(CancellationToken ctk)
    {
        try
        {
            dashCoolTimer = dashCool;

            while (dashCoolTimer >= 0)
            {
                dashCoolTimer -= Time.deltaTime;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }

            if (dashCoolTimer <= 0)
                dashCoolTimer = 0f;

            Utils.Log<PlayerController>("쿨타임 종료");
        }
        catch (OperationCanceledException)
        {

        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGround)
            Jump();

        if (context.canceled && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            Utils.Log<PlayerController>("context.performed");
            return;
        }

        if (isDash || dashCoolTimer > 0f)
        {
            Utils.Log<PlayerController>("대시중이거나 쿨타임 중");
            return;
        }

        Dash();
    }
}
