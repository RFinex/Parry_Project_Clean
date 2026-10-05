using UnityEngine;

public class PlayerCombat
{
    private PlayerBaseData data;

    private bool isParry;
    private float guardTimer;

    #region Attack Stats

    private int attackIndex;
    private float attackTimer;
    private bool iscomboRequest;

    private const int maxCombo = 3;
    private const float comboInputTime = 0.25f;
    private const float attackEndTime = 0.5f;

    public float ParryWindow => data.ParryWindow;
    public int AttackIndex => attackIndex;
    public bool isAttacking;

    #endregion

    #region Guard Stats

    private const float guardTime = 0.3f;

    public bool CanGuard => guardTimer <= 0f;

    #endregion

    public PlayerCombat(PlayerBaseData data)
    {
        this.data = data;
    }

    public void Attack(Vector2 pos, float frontDir)
    {
        Utils.Log<PlayerCombat>($"공격 시작 : {attackIndex}");

        Vector2 attackPos = GetAttackPos(pos, frontDir);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, data.AttackRange, data.EnemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy == null)
                continue;
            
            Utils.Log<PlayerCombat>($"공격 적중 : {hit.name}");

            enemy.Stats.TakeDamage(data.AttackDamage);
        }        
    }

    public void RequestCombo()
    {
        if (!isAttacking)
            return;

        iscomboRequest = true;
    }

    public int UpdateAttack(float deltaTime, Vector2 pos, float frontDir)
    {
        if (!isAttacking)
            return 0;

        attackTimer += deltaTime;

        if (!iscomboRequest)
            return 0;

        if (attackTimer < comboInputTime)
            return 0;

        if (attackIndex >= maxCombo)
            return 0;

        iscomboRequest = false;
        attackIndex++;
        attackTimer = 0f;

        Attack(pos, frontDir);

        return attackIndex;
    }

    public bool IsAttackFinished()
    {
        if (!isAttacking)
            return true;

        return attackTimer >= attackEndTime;
    }

    public int AttackStart(Vector2 pos, float frontDir)
    {
        isAttacking = true;

        attackIndex = 1;
        attackTimer = 0;
        iscomboRequest = false;

        Attack(pos, frontDir);

        return attackIndex;
    }

    public void AttackEnd()
    {
        isAttacking = false;

        attackIndex = 0;
        attackTimer = 0f;
        iscomboRequest = false;
    }

    private Vector2 GetAttackPos(Vector2 pos, float frontDir)
    {
        Vector2 offset = data.AttackOffset;

        offset.x *= frontDir;

        return pos + offset;
    }

    #region Guard

    public void GuardStart()
    {
        if (!CanGuard)
            return;

        // 패리 시작
        isParry = true;

        guardTimer = guardTime;
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

        Utils.Log<PlayerCombat>("가드 종료");
    }

    public bool IsParry()
    {
        return isParry;
    }

    public void GuardTimeUpdate(float deltaTime)
    {
        if (guardTimer > 0f)
        {
            guardTimer -= deltaTime;

            if (guardTimer <= 0f)
                guardTimer = 0f;
        }
    }

    #endregion
}
