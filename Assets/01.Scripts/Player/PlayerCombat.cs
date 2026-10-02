using UnityEngine;

public class PlayerCombat
{
    private PlayerBaseData data;

    //private readonly float attackDamage = 10f;
    //private readonly float attackRange = 0.8f;
    //private readonly Vector2 attackOffset = new Vector2(0.6f, 0f);
    //private readonly LayerMask enemyLayer;
    //private readonly float parryWindow = 0.15f;
    //// 테스트 공격 시간
    //private readonly float attackDuration = 0.3f;

    private bool isParry;

    public float AttackDuration => data.AttackDuration;
    public float ParryWindow => data.ParryWindow;

    private float parryTimer;

    private float attackTimer;

    public PlayerCombat(PlayerBaseData data)
    {
        this.data = data;
    }

    public void Attack(Vector2 pos, float frontDir)
    {
        Utils.Log<PlayerCombat>("공격 시작");

        Vector2 attackPos = GetAttackPos(pos, frontDir);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, data.AttackRange, data.EnemyLayer);

        foreach (Collider2D hit in hits)
        {
            Utils.Log<PlayerCombat>($"공격 적중 : {hit.name}");

            EnemyStats enemyStats = hit.GetComponent<EnemyStats>();
            enemyStats.TakeDamage(data.AttackDamage);
        }        
    }

    public void GuardStart()
    {
        // 패리 시작
        isParry = true;

        Utils.Log<PlayerCombat>($"가드 시작 / parryTimer = {parryTimer}");
    }

    public void ParryEnd()
    {
        if (!isParry)
            return;

        isParry = false;

        Utils.Log<PlayerCombat>("패링 종료");
    }

    public void GuardEnd()
    {
        isParry = false;
        parryTimer = 0f;

        Utils.Log<PlayerCombat>("가드 종료");
    }

    public bool IsParry()
    {
        return isParry;
    }

    private Vector2 GetAttackPos(Vector2 pos, float frontDir)
    {
        Vector2 offset = data.AttackOffset;

        offset.x *= frontDir;

        return pos + offset;
    }
}
