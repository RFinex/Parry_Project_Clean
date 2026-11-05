using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Upper_Skill_Data", menuName = "Enemy/Skills/Old Samurai Upper")]
public class OldSamuraiUpperSkillData : EnemySkillData
{
    [Header("Timing")]
    [SerializeField] private float warningDelay = (10f / 60f);
    [SerializeField] private float hitDelay = (35f / 60f);
    [SerializeField] private float animEndDelay = (40f / 60f);

    public float WarningDelay => warningDelay;
    public float HitDelay => hitDelay;
    public float AnimEndDelay => animEndDelay;
}
