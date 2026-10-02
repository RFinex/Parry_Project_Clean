using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    #region Black Board
    private PlayerBlackBoard blackBoard;
    private PlayerMovement movement;
    private PlayerSkill skill;
    #endregion

    #region Initialized Value
    [Header("Player Data")]
    [SerializeField] private PlayerBaseData data;

    [Header("Player Info")]
    [SerializeField] private PlayerStats stats;
    private PlayerCombat combat;

    public PlayerCombat Combat => combat;

    [Header("Move")]
    [SerializeField] private float frontDir = 1f;
    private Vector2 moveInput;
    private float moveSpeed;

    [Header("Jump")]
    private float jumpForce;

    [Header("Dash")]
    private float dashTime;
    private float dashSpeed;
    private float dashCool;

    [Header("Ground Check")]
    private Vector2 checkSize;
    private float checkDistance;
    private LayerMask groundLayer;
    private Vector3 checkOffset;

    [Header("State Machine")]
    private PlayerStateMachine stateMachine;

    [Header("Bool Check")]
    [SerializeField] private bool isGround;

    [Header("Current State")]
    [SerializeField] private string currentStateName;
    #endregion

    private CancellationTokenSource token;
    public CancellationToken DestroyToken => token.Token;

    private void Awake()
    {
        Initialized();
    }

    private void OnEnable()
    {
        if (token == null)
        {
            token?.Cancel();
            token?.Dispose();
            token = new CancellationTokenSource();
        }

        UpdateAsync(token.Token).Forget();
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }

    private void Initialized()
    {
        rb = GetComponent<Rigidbody2D>();

        moveSpeed = data.MoveSpeed;
        jumpForce = data.JumpForce;

        checkSize = data.CheckSize;
        checkOffset = data.CheckOffset;
        checkDistance = data.CheckDistance;
        groundLayer = data.GroundLayer;

        dashTime = data.DashTime;
        dashSpeed = data.DashSpeed;
        dashCool = data.DashCool;

        movement = new PlayerMovement()
        {
            moveInput = moveInput,
            frontDir = frontDir,
            isGround = isGround,
            verticalVelocity = rb.linearVelocity.y,
            moveSpeed = moveSpeed,
            jumpForce = jumpForce
        };
        skill = new PlayerSkill()
        {
            dashTime = dashTime,
            dashSpeed = dashSpeed,
            dashCool = dashCool
        };
        blackBoard = new PlayerBlackBoard()
        {
            rb = rb,
            movement = movement,
            skill = skill
        };

        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        combat = new PlayerCombat(data);

        stateMachine = new PlayerStateMachine(this, blackBoard);

        stateMachine.PlayerAddState<PlayerAttackState>();
        stateMachine.PlayerAddState<PlayerDashState>();
        stateMachine.PlayerAddState<PlayerGuardState>();
        stateMachine.PlayerAddState<PlayerMoveState>();
        stateMachine.PlayerAddState<PlayerIdleState>();
        stateMachine.PlayerAddState<PlayerJumpState>();

        stateMachine.ChangeState(typeof(PlayerIdleState));

        stats.Initialized(data);
    }

    private async UniTaskVoid UpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                GroundCheck();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (stateMachine != null && stateMachine.CurrentState != null)
                {
                    currentStateName = stateMachine.CurrentState.GetType().Name;
                }
#endif

                PlayerStatsUpdate();

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    #region Player Movement/Jump Stop Method
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

    public void StopJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
    }
    #endregion

    private void GroundCheck()
    {
        Vector2 origin = transform.position + checkOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, checkSize, 0f, Vector2.down, checkDistance, groundLayer);

        isGround = hit.collider != null;
    }    

    private void PlayerStatsUpdate()
    {
        movement.moveInput = moveInput;
        movement.frontDir = frontDir;
        movement.isGround = isGround;
        movement.verticalVelocity = rb.linearVelocity.y;
        movement.moveSpeed = moveSpeed;
        movement.jumpForce = jumpForce;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 origin = transform.position + checkOffset;

        Gizmos.DrawWireCube(origin + Vector3.down * checkDistance, checkSize);
    }
#endif

    #region InputAction Method
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isGround)
                return;

            stateMachine.ChangeState(typeof(PlayerJumpState));

            PlayerJumpState jump = stateMachine.GetState<PlayerJumpState>();

            jump.StartJump();
        }
        
        if (context.canceled)
        {
            if (stateMachine.IsState(typeof(PlayerJumpState)))
            {
                PlayerJumpState jump = stateMachine.GetState<PlayerJumpState>();

                jump.JumpCanceled();
            }            
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (stateMachine.IsState(typeof(PlayerDashState)) || skill.dashCoolTimer > 0f)
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
            if (stateMachine.IsState(typeof(PlayerGuardState)))
            {
                PlayerGuardState guard = stateMachine.GetState<PlayerGuardState>();

                guard.GuardCanceled();
            }
        }
    }
    #endregion
}
