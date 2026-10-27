using UnityEngine;

public class PlayerCombat
{
    private readonly PlayerBaseData data;
    private readonly PlayerStats stats;

    private bool isGuard;
    private bool isParry;
    private float guardTimer;

    #region Attack Stats

    private int attackIndex;
    private float attackTimer;
    private bool isComboRequest;
    private bool isHit;

    public float ParryTime => stats.GetStats(StatId.ParryTime);

    public int AttackIndex => attackIndex;
    public bool isAttacking;

    #endregion

    // PlayerStats로 옮기기
    #region Guard Stats

    private const float guardTime = 0.3f;

    public bool CanGuard => guardTimer <= 0f;

    public float GuardStaminaCost => stats.GetStats(StatId.GuardStaminaCost);

    #endregion


    public PlayerCombat(PlayerBaseData data, PlayerStats stats)
    {
        this.data = data;
        this.stats = stats;
    }

    public void Attack(Vector2 pos, float frontDir)
    {
        Vector2 attackPos = GetAttackPos(pos, frontDir);

        float attackRange = stats.GetStats(StatId.AttackRange);
        float attackDamage = stats.GetStats(StatId.AttackDamage);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, data.EnemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(attackDamage);
        }        
    }

    /// <summary>
    /// 가장 가까운 처형 가능 적 탐색 함수
    /// </summary>
    /// <param name="pos"> 기준 위치(transform.position) </param>
    /// <returns></returns>
    public GameObject FindExecuteTarget(Vector2 pos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, data.ExecuteSearchRange, data.EnemyLayer);

        GameObject target = null;

        float closeDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IExecuteTarget enemy = hit.GetComponentInParent<IExecuteTarget>();

            if (enemy == null || !enemy.CanExecute)
                continue;

            Vector2 dir = (Vector2)hit.transform.position - pos;
            float distance = dir.magnitude;

            if (distance >= closeDistance)
                continue;

            closeDistance = distance;

            target = hit.gameObject;
        }

        return target;
    }

    public void SpecialAttack(GameObject target)
    {
        if (target == null)
            return;

        IExecuteTarget enemy = target.GetComponentInParent<IExecuteTarget>();

        if (enemy == null | !enemy.CanExecute)
            return;

        Utils.Log<PlayerCombat>("특수 공격!");
        enemy.Execute();
    }

    public void RequestCombo()
    {
        if (isAttacking)
            isComboRequest = true;
    }

    public int UpdateAttack(float deltaTime, Vector2 pos, float frontDir)
    {
        if (!isAttacking)
            return 0;

        attackTimer += deltaTime;

        if (isComboRequest && attackTimer >= data.ComboInputTime && attackIndex < data.MaxCombo)
        {
            isComboRequest = false;
            attackIndex++;
            attackTimer = 0f;
            isHit = false;

            return attackIndex;
        }

        if (!isHit && attackTimer >= GetCurrentAttackHitDelay())
        {
            Attack(pos, frontDir);
            isHit = true;
        }

        return 0;
    }

    // 애니메이션 딜레이
    private float GetCurrentAttackHitDelay()
    {
        if (data.AttackHitDelays == null || attackIndex <= 0 || attackIndex > data.AttackHitDelays.Count)
            return 0f;

        return data.AttackHitDelays[attackIndex - 1];
    }

    public bool IsAttackFinished()
    {
        return !isAttacking || attackTimer >= data.AttackEndTime;
    }

    public int AttackStart(Vector2 pos, float frontDir)
    {
        isAttacking = true;

        attackIndex = 1;
        attackTimer = 0;
        isComboRequest = false;
        isHit = false;

        return attackIndex;
    }

    public void AttackEnd()
    {
        isAttacking = false;

        attackIndex = 0;
        attackTimer = 0f;
        isComboRequest = false;
        isHit = false;
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

        isGuard = true;
        // 패리 시작
        isParry = true;

        guardTimer = guardTime;
    }

    public void ParryEnd()
    {
        isParry = false;

        Utils.Log<PlayerCombat>("패링 종료");
    }

    public void GuardEnd()
    {
        isGuard = false;
        isParry = false;
    }

    public bool IsParry()
    {
        return isParry;
    }

    public void GuardTimeUpdate(float deltaTime)
    {
        if (guardTimer <= 0f)
            return;

        guardTimer -= deltaTime;

        if (guardTimer <= 0f)
            guardTimer = 0f;
    }

    public GuardResult CheckGuard()
    {
        if (!isGuard)
            return GuardResult.None;

        return isParry? GuardResult.Parry : GuardResult.Guard;
    }

    #endregion
}
