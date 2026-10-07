using UnityEngine;

public class PlayerCombat
{
    private PlayerBaseData data;

    private bool isGuard;
    private bool isParry;
    private float guardTimer;

    #region Attack Stats

    private int attackIndex;
    private float attackTimer;
    private bool isComboRequest;

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

    public float GuardStaminaCost => data.GuardStaminaCost;

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

            enemy.TakeDamage(data.AttackDamage);
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

            if (enemy == null)
                continue;

            if (!enemy.CanExecute)
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

        if (enemy == null)
            return;

        if (!enemy.CanExecute)
            return;

        Utils.Log<PlayerCombat>("특수 공격!");

        enemy.Execute();
    }

    public void RequestCombo()
    {
        if (!isAttacking)
            return;

        isComboRequest = true;
    }

    public int UpdateAttack(float deltaTime, Vector2 pos, float frontDir)
    {
        if (!isAttacking)
            return 0;

        attackTimer += deltaTime;

        if (!isComboRequest)
            return 0;

        if (attackTimer < comboInputTime)
            return 0;

        if (attackIndex >= maxCombo)
            return 0;

        isComboRequest = false;
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
        isComboRequest = false;

        Attack(pos, frontDir);

        return attackIndex;
    }

    public void AttackEnd()
    {
        isAttacking = false;

        attackIndex = 0;
        attackTimer = 0f;
        isComboRequest = false;
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
        if (!isParry)
            return;

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
        if (guardTimer > 0f)
        {
            guardTimer -= deltaTime;

            if (guardTimer <= 0f)
                guardTimer = 0f;
        }
    }

    public GuardResult CheckGuard()
    {
        if (!isGuard)
            return GuardResult.None;

        if (isParry)
            return GuardResult.Parry;

        return GuardResult.Guard;
    }

    #endregion
}
