using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Phase2_Sweep_Data", menuName = "Enemy/Skills/Old Samurai Phase 2 Sweep")]
public class OldSamuraiPhase2SweepData : EnemySkillData
{
    [SerializeField] private int attackPointEffectId;
    [SerializeField] private float attackPointDuration = 0.25f;
    [SerializeField] private List<OldSamuraiPhase2SweepStrike> strikes = new();

    public int AttackPointEffectId => attackPointEffectId;
    public float AttackPointDuration => attackPointDuration;
    public List<OldSamuraiPhase2SweepStrike> Strikes => strikes;
}

[Serializable]
public struct OldSamuraiPhase2SweepStrike
{
    [SerializeField] private EnemyAnimationType animationType;
    [SerializeField] private Vector2 hitboxOffset;
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private float warningDelay;
    [SerializeField] private float hitDelay;
    [SerializeField] private float animEndDelay;
    [SerializeField] private bool canGuard;
    [SerializeField] private bool canParry;
    [SerializeField] private Vector2 attackPointScale;

    public EnemyAnimationType AnimationType => animationType;
    public Vector2 HitboxOffset => hitboxOffset;
    public Vector2 HitboxSize => hitboxSize;
    public float WarningDelay => warningDelay;
    public float HitDelay => hitDelay;
    public float AnimEndDelay => animEndDelay;
    public bool CanGuard => canGuard;
    public bool CanParry => canParry;
    public Vector2 AttackPointScale => attackPointScale;
}
