using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Player Data")]
    [SerializeField] private PlayerBaseData data;

    [Header("Player Info")]
    [SerializeField] private PlayerStats stats;
    private PlayerCombat combat;

    public PlayerCombat Combat => combat;

    [Header("Move")]
    [SerializeField] private float frontDir = 1f;
    public float FrontDir => frontDir;
    private float moveSpeed;

    [Header("Jump")]
    private float jumpForce;

    [Header("Ground CHeck")]
    private Vector2 checkSize;
    private float checkDistance;
    private LayerMask groundLayer;
    private Vector3 checkOffset;

    [Header("Dash")]
    private float dashTime;
    private float dashSpeed;
    private float dashCool;
    [SerializeField] private float dashCheckTimer = 0f;
    [SerializeField] private float dashCoolTimer = 0f;

    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    [Header("State Machine")]
    private StateMachine<PlayerController> stateMachine;

    [Header("Bool Check")]
    [SerializeField] private bool isGround;
    [SerializeField] private bool isDash;
    public bool IsGround => isGround;

    public float VerticalVelocity => rb.linearVelocity.y;

    [Header("Current State")]
    [SerializeField] private string currentStateName;


    private CancellationTokenSource token;
    public CancellationToken DestroyToken => token.Token;

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

        combat = new PlayerCombat(data);

        stateMachine = new StateMachine<PlayerController>(this);

        stateMachine.AddState<PlayerAttackState>();
        stateMachine.AddState<PlayerDashState>();
        stateMachine.AddState<PlayerGuardState>();
        stateMachine.AddState<PlayerMoveState>();
        stateMachine.AddState<PlayerIdleState>();
        stateMachine.AddState<PlayerJumpState>();

        stateMachine.ChangeState(typeof(PlayerIdleState));

        stats.Initialized(data);
    }

    private void Update()
    {
        GroundCheck();
    }
   

    public void Move()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        // 플레이어 보는 방향 판별
        if (moveInput.x != 0f)
        {
            frontDir = Mathf.Sign(moveInput.x);
        }
    }

    public void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void StopJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
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
        stateMachine.ChangeState(typeof(PlayerDashState));

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
            if(moveInput.x != 0f)
                stateMachine.ChangeState(typeof(PlayerMoveState));
            else
                stateMachine.ChangeState(typeof(PlayerIdleState));
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

    public void Attack()
    {
        combat.Attack(transform.position, frontDir);
    }

    public void GuardStart()
    {
        combat.GuardStart();
    }

    public void GuardEnd()
    {
        combat.GuardEnd();
    }

    #region InputAction Method
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        //if (moveInput.x != 0f && stateMachine.IsState(typeof(PlayerIdleState)))
        //{
        //    Utils.Log<PlayerController>($"{moveInput.x}");
        //    stateMachine.ChangeState(typeof(PlayerMoveState));
        //}
        //else if (moveInput.x == 0f && stateMachine.IsState(typeof(PlayerMoveState)))
        //{
        //    Utils.Log<PlayerController>($"{moveInput.x}");
        //    stateMachine.ChangeState(typeof(PlayerIdleState));
        //}
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isGround)
                return;

            stateMachine.ChangeState(typeof(PlayerJumpState));

        }
        
        if (context.canceled)
        {
            StopJump();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (isDash || dashCoolTimer > 0f)
        {
            Utils.Log<PlayerController>("대시중이거나 쿨타임 중");
            return;
        }

        if (stateMachine.IsState(typeof(PlayerIdleState)) ||
           stateMachine.IsState(typeof(PlayerMoveState)) ||
           stateMachine.IsState(typeof(PlayerJumpState)))
        {
            stateMachine.ChangeState(typeof(PlayerDashState));
        }

        Dash();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (stateMachine.IsState(typeof(PlayerIdleState)) ||
           stateMachine.IsState(typeof(PlayerMoveState)) ||
           stateMachine.IsState(typeof(PlayerJumpState)))
        {
            stateMachine.ChangeState(typeof(PlayerAttackState));
        }        
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        Utils.Log<PlayerCombat>($"Guard - started:{context.started}, performed:{context.performed}, canceled:{context.canceled}");

        if (context.started)
        {
            if (stateMachine.IsState(typeof(PlayerIdleState)) ||
                stateMachine.IsState(typeof(PlayerMoveState)) ||
                stateMachine.IsState(typeof(PlayerJumpState)))
            {
                stateMachine.ChangeState(typeof(PlayerGuardState));
            }
        }
        else if (context.canceled)
        {
            stateMachine.ChangeState(typeof(PlayerIdleState));
        }
    }
    #endregion
}
