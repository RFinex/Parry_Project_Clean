using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTransitionData", menuName = "Player/PlayerTransitionData")]
public class PlayerTransitionData : ScriptableObject
{
    [SerializeField] private List<PlayerTransitionEntry> entries = new();

    public IReadOnlyList<PlayerTransitionEntry> Entries => entries;
}
