using UnityEngine;

public class EnemyTestStartTrigger : MonoBehaviour
{
    [SerializeField] private EnemyController targetEnemy;

    private bool hasStarted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStarted || targetEnemy == null)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (targetEnemy is BossController boss)
            boss.StartCombat(player);
        else if (targetEnemy is EliteEnemyController elite)
            elite.StartCombat(player);
        else
            return;

        hasStarted = true;
    }
}
