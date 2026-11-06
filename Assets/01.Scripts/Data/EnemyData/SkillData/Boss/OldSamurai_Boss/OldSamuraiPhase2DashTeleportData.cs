using UnityEngine;

[CreateAssetMenu(fileName = "OldSamurai_Phase2_DashTeleport_Data", menuName = "Enemy/Skills/Old Samurai Phase 2 Dash Teleport")]
public class OldSamuraiPhase2DashTeleportData : EnemySkillData
{
    [SerializeField] private float rearDistance = 1.5f;
    [SerializeField] private float dashStartDelay = 0.2f;
    [SerializeField] private float animEndDelay = 0.2f;

    public float RearDistance => rearDistance;
    public float DashStartDelay => dashStartDelay;
    public float AnimEndDelay => animEndDelay;
}
