using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Enemy_Projectile_Attack_Data", menuName = "Enemy/Projectile Attack Data")]
public class EnemyProjectileAttackData : EnemyAttackData
{
    [FormerlySerializedAs("bombSpawnOffset")]
    [SerializeField] private Vector2 projectileSpawnOffset;

    [FormerlySerializedAs("bombPoolId")]
    [SerializeField] private int projectilePoolId;

    [SerializeField] private Vector2 targetOffset;

    public Vector2 ProjectileSpawnOffset => projectileSpawnOffset;
    public int ProjectilePoolId => projectilePoolId;
    public Vector2 TargetOffset => targetOffset;
}
