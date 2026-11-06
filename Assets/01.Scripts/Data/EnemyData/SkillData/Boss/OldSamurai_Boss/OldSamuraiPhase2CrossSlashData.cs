using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Phase2_Cross_Slash_Data", menuName = "Enemy/Skills/Old Samurai Phase 2 Cross Slash")]
public class OldSamuraiPhase2CrossSlashData : EnemySkillData
{
    [SerializeField] private int attackPointEffectId = 7;
    [SerializeField] private List<OldSamuraiPhase2CrossSlashStrike> strikes = new();

    public int AttackPointEffectId => attackPointEffectId;
    public List<OldSamuraiPhase2CrossSlashStrike> Strikes => strikes;
}

[Serializable]
public struct OldSamuraiPhase2CrossSlashStrike
{
    [SerializeField] private EnemyAnimationType animationType;
    [SerializeField] private Vector2 targetOffset;
    [SerializeField] private Vector2 hitboxSize;
    [SerializeField] private Vector2 attackPointScale;
    [SerializeField] private float warningDelay;
    [SerializeField] private float hitDelay;
    [SerializeField] private float animEndDelay;
    [SerializeField] private bool canGuard;
    [SerializeField] private bool canParry;
    [SerializeField] private float preAnimationDelay;

    public EnemyAnimationType AnimationType => animationType;
    public Vector2 TargetOffset => targetOffset;
    public Vector2 HitboxSize => hitboxSize;
    public Vector2 AttackPointScale => attackPointScale;
    public float WarningDelay => warningDelay;
    public float HitDelay => hitDelay;
    public float AnimEndDelay => animEndDelay;
    public bool CanGuard => canGuard;
    public bool CanParry => canParry;
    public float PreAnimationDelay => preAnimationDelay;
}