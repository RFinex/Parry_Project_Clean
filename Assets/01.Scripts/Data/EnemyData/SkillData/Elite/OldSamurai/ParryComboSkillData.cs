using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Parry_Combo_Skill_Data", menuName = "Enemy/Skills/Old Samurai Parry Combo")]
public class ParryComboSkillData : EnemySkillData
{
    [SerializeField] private List<ParryComboStrikeData> strikes = new();    
    public List<ParryComboStrikeData> Strikes => strikes;
}

[Serializable]
public class ParryComboStrikeData
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