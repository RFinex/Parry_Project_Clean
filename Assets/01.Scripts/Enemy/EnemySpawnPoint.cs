using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private int enemyId;

    public int EnemyId => enemyId;
}
