using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Phase2_LineBarrage_Data", menuName = "Enemy/Skills/Old Samurai Phase 2 Line Barrage")]
public class OldSamuraiPhase2LineBarrageData : EnemySkillData
{
    [SerializeField] private int lineEffectId;
    [SerializeField] private float lineInterval = 0.12f;

    [Header("Line Setting")]
    [SerializeField] private int attackCount = 5;
    [SerializeField] private Vector2 angleRange = new(-75f, 75f);
    [SerializeField] private Vector2 lineHitboxSize;
    [SerializeField] private Vector2 randomPositionRange = new(4f, 2f);
    [SerializeField] private float warningStartDelay = 1f;

    public int LineEffectId => lineEffectId;
    public float LineInterval => lineInterval;

    public int AttackCount => attackCount;
    public Vector2 AngleRange => angleRange;
    public Vector2 LineHitboxSize => lineHitboxSize;
    public Vector2 RandomPositionRange => randomPositionRange;
    public float WarningStartDelay => warningStartDelay;
}