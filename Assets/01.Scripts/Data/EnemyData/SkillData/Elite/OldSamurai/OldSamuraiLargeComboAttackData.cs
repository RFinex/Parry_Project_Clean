using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Large_Combo_Data", menuName = "Enemy/Skills/Old Samurai Large Combo")]
public class OldSamuraiLargeComboAttackData : EnemySkillData
{
    [Header("Warning Prefab")]
    [SerializeField] private int previewEffectId;
    [SerializeField] private float previewInterval = 0.24f;

    [Header("Strike Number")]
    [SerializeField] private List<OldSamuraiLargeAttackData> strikes = new();

    public int PreviewEffectId => previewEffectId;
    public float PreviewInterval => previewInterval;

    public List<OldSamuraiLargeAttackData> Strikes => strikes;
}

[Serializable]
public class OldSamuraiLargeAttackData
{
    [Header("Target Offset")]
    [SerializeField] private Vector2 targetOffset;

    [Header("Attack")]
    [SerializeField] private EnemyAnimationType animationType;
    [SerializeField] private Vector2 hitboxSize = new(2f, 1.5f);
    [SerializeField] private float warningDelay;
    [SerializeField] private float warningDuration = 0.4f;
    [SerializeField] private float hitDelay = 0.3f;
    [SerializeField] private float animDuration = 0.8f;

    [Header("Guard / Parry")]
    [SerializeField] private bool canGuard = true;
    [SerializeField] private bool canParry = true;

    public Vector2 TargetOffset => targetOffset;

    public EnemyAnimationType AnimationType => animationType;
    public Vector2 HitboxSize => hitboxSize;
    public float WarningDelay => warningDelay;
    public float WarningDuration => warningDuration;
    public float HitDelay => hitDelay;
    public float AnimDuration => animDuration;

    public bool CanGuard => canGuard;
    public bool CanParry => canParry;
}