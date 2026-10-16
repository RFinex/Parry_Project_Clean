using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTransitioner
{
    private readonly PlayerBlackBoard board;

    private readonly Dictionary<Type, Dictionary<PlayerInput, List<PlayerTransitionRule>>> rulesDic = new();

    public PlayerTransitioner(PlayerBlackBoard board, PlayerTransitionData data)
    {
        this.board = board;

        if (data == null)
            return;

        InitRules(data);
    }

    private void InitRules(PlayerTransitionData data)
    {
        var stateTypeDic = new Dictionary<PlayerStateId, Type>()
        {
            { PlayerStateId.Idle, typeof(PlayerIdleState) },
            { PlayerStateId.Move, typeof(PlayerMoveState) },
            { PlayerStateId.Jump, typeof(PlayerJumpState) },
            { PlayerStateId.Fall, typeof(PlayerFallState) },
            { PlayerStateId.Attack, typeof(PlayerAttackState) },
            { PlayerStateId.Guard, typeof(PlayerGuardState) },
            { PlayerStateId.Dash, typeof(PlayerDashState) },
            { PlayerStateId.Hurt, typeof(PlayerHurtState) },
            { PlayerStateId.Execute, typeof(PlayerExecuteState) },
            { PlayerStateId.Dead, typeof(PlayerDeadState) }
        };

        foreach (PlayerTransitionEntry entry in data.Entries)
        {
            if (entry == null || entry.Rule == null)
            {
                Utils.LogError<PlayerTransitioner>("entry 또는 Rule이 비어있습니다.");
                continue;
            }

            foreach (PlayerStateId stateId in entry.FromStates)
            {
                if (!stateTypeDic.TryGetValue(stateId, out Type stateType))
                {
                    Utils.LogError<PlayerTransitioner>($"등록되지 않은 상태 : {stateId}");
                    continue;
                }

                AddRule(stateType, entry.Input, entry.Rule);
            }

        }
    }

    private void AddRule(Type stateType, PlayerInput input, PlayerTransitionRule rule)
    {
        if (!rulesDic.TryGetValue(stateType, out var stateRules))
        {
            stateRules = new Dictionary<PlayerInput, List<PlayerTransitionRule>>();

            rulesDic.Add(stateType, stateRules);
        }

        if (!stateRules.TryGetValue(input, out var inputRules))
        {
            inputRules = new List<PlayerTransitionRule>();
            stateRules.Add(input, inputRules);
        }

        if (!inputRules.Contains(rule))
        {
            inputRules.Add(rule);
        }
    }

    public bool TryResolve(Type currentState, PlayerInput input, out Type nextState)
    {
        nextState = null;

        if (currentState == null)
            return false;

        if (!rulesDic.TryGetValue(currentState, out var stateRules))
            return false;

        if (!stateRules.TryGetValue(input, out var inputRules))
            return false;

        for (int i = 0; i < inputRules.Count; i++)
        {
            PlayerTransitionRule rule = inputRules[i];

            if (!rule.CanApply(board))
                continue;

            rule.Action?.PlayerInput(board);
            nextState = rule.GetNextState(board);

            return true;
        }

        return false;
    }
}
