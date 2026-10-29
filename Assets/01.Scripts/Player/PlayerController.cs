using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GuardResult
{
    None,
    Guard,
    Parry
}

public enum PlayerInput
{
    JumpPressed,
    JumpReleased,
    DashPressed,
    AttackPressed,
    GuardPressed,
    GuardReleased,
    MoveChanged
}

public enum PlayerStateId
{
    Idle = 0,
    Move = 1,
    Jump = 2,
    Fall = 3,
    Attack = 4,
    Guard = 5,
    Dash = 6,
    Hurt = 7,
    Execute = 8,
    Dead = 9
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private PlayerAfterImage afterImage;
    private InteractScanner scanner;

    #region Black Board

    private PlayerBlackBoard blackBoard;
    private PlayerMovement movement;
    private PlayerSkill skill;

    public PlayerBlackBoard PlayerBoard => blackBoard;

    #endregion

    #region Initialized Value

    [Header("Player Data")]
    [SerializeField] private PlayerBaseData data;
    [SerializeField] private PlayerTransitionData transitionData;

    [Header("Player Info")]
    private PlayerStats stats;
    private PlayerCombat combat;

    // 플레이어가 공격중인지 판단
    public bool IsAttacking => combat != null && combat.isAttacking;

    public PlayerStats Stats => stats;

    [Header("Move")]
    [SerializeField] private float frontDir = 1f;
    private Vector2 moveInput;
    private Vector2 cacheMoveInput;

    [Header("Ground Check")]
    private Vector2 checkSize;
    private float checkDistance;
    private LayerMask groundLayer;
    private Vector3 checkOffset;

    [Header("State Machine")]
    private PlayerStateMachine stateMachine;
    private PlayerTransitioner transitioner;

    [Header("Bool Check")]
    [SerializeField] private bool isGround;

    [Header("Current State Test")]
    [SerializeField] private string currentStateName;

    [Header("Invincible Visual")]
    private float invincibleAlpha;
    private float blinkInterval;
    private float blinkTimer;
    private bool isInvincibleVisual;

    #endregion

    private CancellationTokenSource token;
    public CancellationToken DestroyToken => token.Token;

    private CancellationTokenSource playerToken;

    private bool canInput = true;

    private void Awake()
    {
        Initialized();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(HandleGmaeStateChanged);

        if (token == null)  
            token = token.ResetToken();
        
        playerToken = playerToken.ResetToken();
        blackBoard.owenrToken = playerToken;

        UpdateAsync(token.Token).Forget();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
        playerToken = playerToken.Cancel_DisposeToken();

        EventBus.UnSubscribe<GameStateChangedEvent>(HandleGmaeStateChanged);

        if(GameManager.Instance != null)
            GameManager.Instance.UnregisterPlayer(this);
    }

    private void Start()
    {
        GameManager.Instance.RegisterPlayer(this);
        SetInputGameState(GameManager.Instance.CurrentState);
    }

    private void Initialized()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        afterImage = GetComponent<PlayerAfterImage>();
        scanner = GetComponentInChildren<InteractScanner>();

        InitBasePlayerInfo();
        InitData();
        InitBlackBoard();

        combat.Initialized(blackBoard);

        token = token.ResetToken();

        InitStateMachine();        

        stateMachine.ChangeState(typeof(PlayerIdleState));
    }

    private void InitBasePlayerInfo()
    {
        checkSize = data.CheckSize;
        checkOffset = data.CheckOffset;
        checkDistance = data.CheckDistance;
        groundLayer = data.GroundLayer;

        movement = new PlayerMovement()
        {
            moveInput = moveInput,
            frontDir = frontDir,
            isGround = isGround,
            verticalVelocity = rb.linearVelocity.y,
            executeMoveSpeed = data.ExecuteMoveSpeed
        };
        skill = new PlayerSkill();

        invincibleAlpha = data.InvincibleAlpha;
        blinkInterval = data.BlinkInterval;
        isInvincibleVisual = true;
    }

    private void InitBlackBoard()
    {
        PlayerAnimator playerAnimator = new(animator);

        blackBoard.rb = rb;
        blackBoard.movement = movement;
        blackBoard.stats = stats;
        blackBoard.playerTransform = transform;
        blackBoard.skill = skill;
        blackBoard.animator = playerAnimator;
        blackBoard.afterImage = afterImage;
        blackBoard.combat = combat;
        blackBoard.data = data;
    }

    private void InitData()
    {
        blackBoard = new PlayerBlackBoard();

        stats = new PlayerStats(data);
        combat = new PlayerCombat();
    }

    private void InitStateMachine()
    {
        stateMachine = new PlayerStateMachine(this, blackBoard);

        AddStates();

        transitioner = new PlayerTransitioner(blackBoard, transitionData);
    }

    private void AddStates()
    {
        stateMachine.PlayerAddState<PlayerAttackState>();
        stateMachine.PlayerAddState<PlayerDashState>();
        stateMachine.PlayerAddState<PlayerGuardState>();
        stateMachine.PlayerAddState<PlayerMoveState>();
        stateMachine.PlayerAddState<PlayerIdleState>();
        stateMachine.PlayerAddState<PlayerJumpState>();
        stateMachine.PlayerAddState<PlayerFallState>();
        stateMachine.PlayerAddState<PlayerHurtState>();
        stateMachine.PlayerAddState<PlayerDeadState>();
        stateMachine.PlayerAddState<PlayerExecuteState>();
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
        float moveSpeed = stats.GetStats(StatId.MoveSpeed);
        float targetSpeed = moveInput.x * moveSpeed;

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        // 플레이어 보는 방향 판별
        if (moveInput.x != 0f)
            frontDir = moveInput.x >= 0f ? 1f : -1f;
    }

    public void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void MoveToRoomStart(Vector3 pos)
    {
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;

        moveInput = Vector2.zero;
        movement.moveInput = Vector2.zero;

        stateMachine.ChangeState(typeof(PlayerIdleState), true);
    }

    public void StopJump()
    {
        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);
        }
    }

    #endregion

    public void TakeDamage(EnemyAttackInfo info)
    {
        if (stats.IsInvincible)
            return;

        if (combat.TryGuard(info))
            return;

        stats.TakeDamage(info.attackDamage);

        if (stats.IsDead)
        {
            stateMachine.ChangeState(typeof(PlayerDeadState));
            return;
        }        

        stats.StartInvincible();

        stateMachine.ChangeState(typeof(PlayerHurtState));
    }

    

    private void GroundCheck()
    {
        Vector2 origin = transform.position + checkOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, checkSize, 0f, Vector2.down, checkDistance, groundLayer);

        isGround = hit.collider != null;
    }    

    private void PlayerStatsUpdate()
    {
        combat.GuardTimeUpdate(Time.deltaTime);
        stats.UpdateInvincible(Time.deltaTime);

        UpdateInvincibleVisual();

        movement.moveInput = moveInput;
        movement.frontDir = frontDir;
        movement.isGround = isGround;
        movement.verticalVelocity = rb.linearVelocity.y;

        // Animator
        blackBoard.animator.SetGrounded(isGround);
        blackBoard.animator.SetAirSpeed(rb.linearVelocity.y);

        blackBoard.animator.SetAnimState(moveInput.x != 0f ? 1 : 0);

        if (frontDir > 0f)
            sr.flipX = false;
        else if (frontDir < 0f)
            sr.flipX = true;
    }

    public void Jump()
    {
        float jumpForce = stats.GetStats(StatId.JumpForce);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        blackBoard.movement.verticalVelocity = rb.linearVelocity.y;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 origin = transform.position + checkOffset;

        Gizmos.DrawWireCube(origin + Vector3.down * checkDistance, checkSize);
    }
#endif

    #region Callback Method

    public void InputToCurrentState(PlayerInput input)
    {
        if (token == null || token.IsCancellationRequested)
            return;

        if (stateMachine == null || transitioner == null)
            return;
        
        var currentState = stateMachine.CurrentState;

        if (currentState == null)
            return;

        if (!transitioner.TryResolve(currentState.GetType(), input, out Type nextState))
            return;

        if (nextState != null)
            stateMachine.ChangeState(nextState);

        //if (stateMachine.CurrentState is PlayerBaseState state)
        //    state.HandleInput(input);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsDeadState())
            return;

        // 조작이 막힌 상태에서 눌렸을 시에도 인풋 값을 저장
        cacheMoveInput = context.ReadValue<Vector2>();

        if (!canInput)
            return;

        // 실제 움직임
        moveInput = cacheMoveInput;
        movement.moveInput = moveInput;

        InputToCurrentState(PlayerInput.MoveChanged);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canInput || IsDeadState())
            return;

        if (context.performed)
        {
            InputToCurrentState(PlayerInput.JumpPressed);

            //if (!isGround)
            //    return;

            //Jump();
            //movement.verticalVelocity = rb.linearVelocity.y;

            //stateMachine.ChangeState(typeof(PlayerJumpState));

        }        
        else if (context.canceled)
        {
            InputToCurrentState(PlayerInput.JumpReleased);
            //if (stateMachine.IsState(typeof(PlayerJumpState)))
            //{
            //    StopJump();
            //}            
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canInput || IsDeadState())
            return;

        if (context.performed)
        {
            InputToCurrentState(PlayerInput.DashPressed);
        }

        //if (!context.performed)
        //    return;

        //if (stateMachine.IsState(typeof(PlayerDashState)) || skill.dashCoolTimer > 0f)
        //{
        //    Utils.Log<PlayerController>("대시중이거나 쿨타임 중");
        //    return;
        //}

        //if (stateMachine.IsState(typeof(PlayerIdleState)) ||
        //    stateMachine.IsState(typeof(PlayerMoveState)) ||
        //    stateMachine.IsState(typeof(PlayerFallState)) ||
        //    stateMachine.IsState(typeof(PlayerAttackState)) ||
        //    stateMachine.IsState(typeof(PlayerJumpState)))
        //{
        //    stateMachine.ChangeState(typeof(PlayerDashState));
        //}
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canInput || IsDeadState())
            return;

        if (context.performed)
        {
            InputToCurrentState(PlayerInput.AttackPressed);
        }

        //if (!context.performed)
        //    return;

        //if (stateMachine.IsState(typeof(PlayerAttackState)))
        //{
        //    combat.RequestCombo();
        //    return;
        //}

        //if (stateMachine.IsState(typeof(PlayerIdleState)) ||
        //    stateMachine.IsState(typeof(PlayerMoveState)) ||
        //    stateMachine.IsState(typeof(PlayerGuardState)) ||
        //    stateMachine.IsState(typeof(PlayerFallState)) ||
        //    stateMachine.IsState(typeof(PlayerJumpState)))
        //{
        //    GameObject target = combat.FindExecuteTarget(transform.position);

        //    if (target != null)
        //    {
        //        executeTarget = target;

        //        stateMachine.ChangeState(typeof(PlayerExecuteState));
        //        return;
        //    }

        //    stateMachine.ChangeState(typeof(PlayerAttackState));
        //}        
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        if (!canInput || IsDeadState())
            return;

        if (context.started)
        {
            InputToCurrentState(PlayerInput.GuardPressed);
        }
        else if (context.canceled)
        {
            InputToCurrentState(PlayerInput.GuardReleased);
        }

        //if (context.started)
        //{
        //    if (!combat.CanGuard)
        //        return;

        //    if (stateMachine.IsState(typeof(PlayerIdleState)) ||
        //        stateMachine.IsState(typeof(PlayerMoveState)) ||
        //        stateMachine.IsState(typeof(PlayerAttackState)) ||
        //        stateMachine.IsState(typeof(PlayerFallState)) ||
        //        stateMachine.IsState(typeof(PlayerJumpState)))
        //    {
        //        stateMachine.ChangeState(typeof(PlayerGuardState));
        //    }
        //}
        //else if (context.canceled)
        //{
        //    if (stateMachine.IsState(typeof(PlayerGuardState)))
        //    {
        //        if (!isGround)
        //        {
        //            stateMachine.ChangeState(typeof(PlayerFallState));
        //            return;
        //        }

        //        if (moveInput.x != 0f)
        //        {
        //            stateMachine.ChangeState(typeof(PlayerMoveState));
        //            return;
        //        }

        //        stateMachine.ChangeState(typeof(PlayerIdleState));
        //    }
        //}
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!canInput || !context.started || IsDeadState())
            return;

        scanner.TryInteract();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!canInput || !context.started || IsDeadState())
            return;

        EventBus.Publish(new InventoryToggleEvent());
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.started || IsDeadState())
            return;

        EventBus.Publish(new PopupCancelEvent());
    }

    #endregion

    #region Animation Event

    public void OnDeathAnimationFinished()
    {
        EventBus.Publish(new PlayerDiedEvent());
    }

    #endregion

    //public void ClearExecuteTarget()
    //{
    //    executeTarget = null;
    //}

    private bool IsDeadState()
    {
        return stateMachine.IsState(typeof(PlayerDeadState));
    }

    public void SetInvincibleVisual(bool value)
    {
        Color color = sr.color;
        color.a = value ? 1f : invincibleAlpha;
        sr.color = color;
    }

    public void UpdateInvincibleVisual()
    {
        if (!stats.IsInvincible)
        {
            if (!isInvincibleVisual)
            {
                isInvincibleVisual = true;
                SetInvincibleVisual(true);
            }

            blinkTimer = 0f;
            return;
        }

        blinkTimer -= Time.deltaTime;

        if (blinkTimer > 0f)
            return;

        blinkTimer = blinkInterval;

        isInvincibleVisual = !isInvincibleVisual;
        SetInvincibleVisual(isInvincibleVisual);
    }    

    public void SetFrontDirForExecute(float dir)
    {
        if (dir == 0f)
            return;

        frontDir = Mathf.Sign(dir);
    }

    private void HandleGmaeStateChanged(GameStateChangedEvent eventData)
    {
        SetInputGameState(eventData.state);
    }

    public void SetInputGameState(GameState state)
    {
        canInput = state == GameState.Play;

        if (!canInput)
        {
            moveInput = Vector2.zero;
            movement.moveInput = Vector2.zero;
            StopMove();
            return;
        }

        moveInput = cacheMoveInput;
        movement.moveInput = moveInput;

        InputToCurrentState(PlayerInput.MoveChanged);
    }
}
