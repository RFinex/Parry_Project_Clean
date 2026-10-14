using UnityEngine;

public class TestEnemyController : EnemyController
{
    [Header("Test Attack")]
    [SerializeField] private Vector2 attackBoxSize = new(1.6f, 1f);
    [SerializeField] private float attackBoxForwardOffset = 0.3f;

    public override void Attack()
    {
        blackBoard.animator.Play(EnemyAnimationType.Attack);
    }

    public void ApplyAttackHit()
    {
        if (Target == null || !IsTargetInAttackRange())
            return;

        Vector2 hitBoxCenter = (Vector2)transform.position + Vector2.right * FrontDir * attackBoxForwardOffset;

        Collider2D hit = Physics2D.OverlapBox(
            hitBoxCenter,
            attackBoxSize,
            0f,
            playerLayer);

        if (hit == null)
            return;

        Combat.Attack(hit.transform, this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 hitBoxCenter = (Vector2)transform.position + Vector2.right * FrontDir * attackBoxForwardOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hitBoxCenter, attackBoxSize);
    }
#endif
}
