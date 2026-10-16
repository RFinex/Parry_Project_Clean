using System.Collections.Generic;
using UnityEngine;
using System;

public readonly struct TransitionResult
{
    public bool IsHandled { get; }
    public Type NextState { get; }

    public TransitionResult(bool handled, Type nextState)
    {
        IsHandled = handled;
        NextState = nextState;
    }
}

public class PlayerTransitioner
{
    private readonly PlayerBlackBoard board;

    private readonly Dictionary<Type, Dictionary<PlayerInput, Func<TransitionResult>>> rules = new();

    public PlayerTransitioner(PlayerBlackBoard board)
    {
        this.board = board;

        RegisterRules();
    }

    private void RegisterRules()
    {
        // 공통 규칙
        RegisterCommonRules<PlayerIdleState>();
        RegisterCommonRules<PlayerMoveState>();
        RegisterCommonRules<PlayerFallState>();
        RegisterCommonRules<PlayerJumpState>();

        // 대기 규칙
        AddRule<PlayerIdleState>(PlayerInput.JumpPressed, ResolveJump);

        // 움직임 규칙
        AddRule<PlayerMoveState>(PlayerInput.JumpPressed, ResolveJump);

        // 점프 규칙
        AddRule<PlayerJumpState>(PlayerInput.JumpReleased, ReleaseJump);

        // 공격 규칙
        AddRule<PlayerAttackState>(PlayerInput.AttackPressed, RequestCombo);
        AddRule<PlayerAttackState>(PlayerInput.DashPressed, ResolveDash);
        AddRule<PlayerAttackState>(PlayerInput.GuardPressed, ResolveGuard);

        // 가드 규칙
        AddRule<PlayerGuardState>(PlayerInput.AttackPressed, ResolveAttackOrExecute);
        AddRule<PlayerGuardState>(PlayerInput.GuardReleased, ResolveGuardRelease);

        // Dash / Hurt / Execute / Dead에는 입력 규칙을 등록하지 않음
    }

    /// <summary>
    /// 공통 규칙
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    private void RegisterCommonRules<TState>() where TState : PlayerBaseState
    {
        AddRule<TState>(PlayerInput.DashPressed, ResolveDash);

        AddRule<TState>(PlayerInput.AttackPressed, ResolveAttackOrExecute);

        AddRule<TState>(PlayerInput.GuardPressed, ResolveGuard);
    }

    /// <summary>
    /// 추가 규칙
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="input"></param>
    /// <param name="resolve"></param>
    private void AddRule<TState>(PlayerInput input, Func<TransitionResult> resolve) where TState : PlayerBaseState
    {
        Type stateType = typeof(TState);

        if (!rules.TryGetValue(stateType, out var stateRules))
        {
            stateRules = new Dictionary<PlayerInput, Func<TransitionResult>>();

            rules.Add(stateType, stateRules);
        }

        stateRules.Add(input, resolve);
    }

    public bool TryResolve(Type currentState, PlayerInput input, out Type nextState)
    {
        nextState = null;

        if (!rules.TryGetValue(currentState, out var stateRules))
            return false;

        if (!stateRules.TryGetValue(input, out var resolve))
            return false;

        TransitionResult result = resolve();

        if (!result.IsHandled)
            return false;

        nextState = result.NextState;
        return true;
    }

    private TransitionResult ResolveJump()
    {
        if (!board.movement.isGround)
            return default;

        // 실제 점프 속도 설정은 JumpState.Enter에서 실행
        return new TransitionResult(true, typeof(PlayerJumpState));
    }

    private TransitionResult ResolveDash()
    {
        if (board.skill.dashCoolTimer > 0f)
            return default;

        return new TransitionResult(true, typeof(PlayerDashState));
    }

    private TransitionResult ResolveAttackOrExecute()
    {
        // 검색은 한 번만 하고 결과를 다음 상태에 전달
        board.executeTarget = board.combat.FindExecuteTarget(board.playerTransform.position);

        if (board.executeTarget != null)
        {
            return new TransitionResult(true, typeof(PlayerExecuteState));
        }

        return new TransitionResult(true, typeof(PlayerAttackState));
    }

    private TransitionResult ResolveGuard()
    {
        if (!board.combat.CanGuard)
            return default;

        return new TransitionResult(true, typeof(PlayerGuardState));
    }

    private TransitionResult ResolveGuardRelease()
    {
        if (!board.movement.isGround)
        {
            return new TransitionResult(true, typeof(PlayerFallState));
        }

        if (board.movement.moveInput.x != 0f)
        {
            return new TransitionResult(
                true, typeof(PlayerMoveState));
        }

        return new TransitionResult(true, typeof(PlayerIdleState));
    }

    private TransitionResult RequestCombo()
    {
        board.combat.RequestCombo();

        // AttackState를 다시 Enter하지 않음
        return new TransitionResult(true, null);
    }

    private TransitionResult ReleaseJump()
    {
        Vector2 velocity = board.rb.linearVelocity;

        if (velocity.y > 0f)
        {
            velocity.y *= 0.4f;

            board.rb.linearVelocity = velocity;
            board.movement.verticalVelocity = velocity.y;
        }

        // JumpState 유지
        return new TransitionResult(true, null);
    }
}