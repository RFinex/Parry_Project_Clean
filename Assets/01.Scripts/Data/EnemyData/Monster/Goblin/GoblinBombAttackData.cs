using UnityEngine;

[CreateAssetMenu(fileName = "Attack_Data_Bomb_Goblin", menuName = "Enemy/Goblin Bomb Attack")]
public class GoblinBombAttackData : EnemyAttackData
{
    [SerializeField] private Vector2 bombSpawnOffset;
    [SerializeField] private int bombPoolId;
    [SerializeField] private Vector2 targetOffset;
    [SerializeField] private float minThrowDistance = 3f;

    public Vector2 BombSpawnOffset => bombSpawnOffset;
    public int BombPoolId => bombPoolId;
    public Vector2 TargetOffset => targetOffset;
    public float MinThrowDistance => minThrowDistance;
}
