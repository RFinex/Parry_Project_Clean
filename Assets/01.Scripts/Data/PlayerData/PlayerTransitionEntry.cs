using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerTransitionEntry
{
    [SerializeField] private List<PlayerStateId> fromStates = new();

    [SerializeField] private PlayerInput input;

    [SerializeField] private PlayerTransitionRule rule;

    public IReadOnlyList<PlayerStateId> FromStates => fromStates;
    public PlayerInput Input => input;
    public PlayerTransitionRule Rule => rule;
}