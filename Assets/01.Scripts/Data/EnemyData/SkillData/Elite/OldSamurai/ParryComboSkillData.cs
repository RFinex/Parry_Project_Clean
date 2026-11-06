using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Parry_Combo_Skill_Data", menuName = "Enemy/Skills/Old Samurai Parry Combo")]
public class ParryComboSkillData : EnemySkillData
{
    [Header("Combo Attack Strikes")]
    [SerializeField] private List<ParryComboStrikeData> strikes = new();

    [Header("Charge Setting")]
    [SerializeField] private float chargeDistance = 3f;
    [SerializeField] private float chargeDuration = 0.05f;

    public List<ParryComboStrikeData> Strikes => strikes;

    public float ChargeDistance => chargeDistance;
    public float ChargeDuration => chargeDuration;
}

[Serializable]
public struct ParryComboStrikeData
{
    // 애니메이션 타입
    [Header("Animation Type")]
    public EnemyAnimationType animType;

    // 애니메이션 타이밍
    [Header("Timing")]
    public float warningDelay;
    public float hitDelay;
    public float animationDuration;
}