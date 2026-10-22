using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProjectileData", menuName = "Projectile/Enemy Projectile Data")]
public class EnemyProjectileData : ScriptableObject
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;

    public float Speed => speed;
    public float LifeTime => lifeTime;
}
