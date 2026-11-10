using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossBTSequenceData", menuName = "Enemy/Boss BT Sequence")]
public class BossBTSequenceData : ScriptableObject
{
    [Header("Phase 1 Skill Priority")]
    [SerializeField] private List<EnemySkillData> phase1Priority = new();

    [Header("Phase 2 Skill Priority")]
    [SerializeField] private List<EnemySkillData> phase2Priority = new();

    public IReadOnlyList<EnemySkillData> Phase1Priority => phase1Priority;
    public IReadOnlyList<EnemySkillData> Phase2Priority => phase2Priority;
}
