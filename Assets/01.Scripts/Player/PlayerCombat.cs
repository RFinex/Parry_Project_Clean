using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat
{
    private PlayerBlackBoard playerBoard;
    private PlayerBaseData data;
    private PlayerStats stats;

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
    private int guardBlockCount;
    public bool CanGuard => guardBlockCount == 0 && guardTimer <= 0f;

    private int parryBlockCount;
    public bool CanParry => parryBlockCount == 0;

    public float GuardStaminaCost => stats.GetStats(StatId.GuardStaminaCost);

    #endregion

    public event Action OnParrySucceed;
    public event Action OnParryEnded;
    public event Action OnAttackEnded;
    public event Action OnExecuteSucceed;
    public event Action<Vector2, IExecuteTarget> OnExecuteArea;
    public event Action<int> OnAttackComboIndex;
    public event Action OnAttackHit;
    public event Action<IReadOnlyList<EnemyController>, float> OnAttackHitTargets;

    private readonly List<EnemyController> hitEnemyList = new();

    private List<Collider2D> hitEnemies = new();
    private ContactFilter2D enemyFilter;
    private bool isFilterInit = false;

    private bool isDamageConversion = false;

    public PlayerCombat()
    {
        
    }

    public void Initialized(PlayerBlackBoard board)
    {
        playerBoard = board;
        data = board.data;
        stats = board.stats;

        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(data.EnemyLayer);
        enemyFilter.useLayerMask = true;
        isFilterInit = true;
    }

    public void Attack(Vector2 pos, float frontDir)
    {
        Vector2 attackPos = GetAttackPos(pos, frontDir);

        OnAttackComboIndex?.Invoke(attackIndex);

        float attackRange = stats.GetStats(StatId.AttackRange);
        float attackDamage = stats.GetStats(StatId.AttackDamage);

        attackDamage = stats.CalculateAllHealthDamage(attackDamage);

        // 최적화

        hitEnemies.Clear();

        if (!isFilterInit)
        {
            enemyFilter.SetLayerMask(data.EnemyLayer);
            enemyFilter.useLayerMask = true;
        }

        int hitCount = Physics2D.OverlapCircle(attackPos, attackRange, enemyFilter, hitEnemies);

        hitEnemyList.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitEnemies[i];

            if (!hit.TryGetComponent<EnemyController>(out var enemy))
                enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy == null)
                continue;

            if (enemy.Stats.IsDead || enemy.IsInvincible)
                continue;

            if (hitEnemyList.Contains(enemy))
                continue;

            if (isDamageConversion)
                enemy.TakeBalanceDamage(attackDamage * 0.5f);
            else
                enemy.TakeDamage(attackDamage);

            hitEnemyList.Add(enemy);
        }

        // 최적화 전

        //hitEnemyList.Clear();

        //Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, data.EnemyLayer);

        //foreach (Collider2D hit in hits)
        //{
        //    EnemyController enemy = hit.GetComponentInParent<EnemyController>();

        //    if (enemy == null)
        //        continue;

        //    if (enemy.Stats.IsDead || enemy.IsInvincible)
        //        continue;

        //    enemy.TakeDamage(attackDamage);

        //    hitEnemyList.Add(enemy);
        //}

        if (hitEnemyList.Count > 0)
        {
            OnAttackHit?.Invoke();
            OnAttackHitTargets?.Invoke(hitEnemyList, attackDamage);
            Utils.Log<PlayerController>($"가한 대미지 : {attackDamage}");
        }

        OnAttackEnded?.Invoke();
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

            if (enemy == null | !enemy.CanExecute)
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

        if (enemy == null || !enemy.CanExecute)
            return;

        if (enemy is Component enemyComponent)
        {
            Vector2 executePos = enemyComponent.transform.position;
            OnExecuteArea?.Invoke(executePos, enemy);
        }

        Utils.Log<PlayerCombat>("특수 공격!");
        enemy.Execute(stats.GetStats(StatId.DamageDealtMultiplier));

        OnExecuteSucceed?.Invoke();
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

        return isParry && CanParry? GuardResult.Parry : GuardResult.Guard;
    }

    public bool TryGuard(EnemyAttackInfo info)
    {
        GuardResult guardResult = CheckGuard();

        if (guardResult == GuardResult.None)
            return false;

        if (guardResult == GuardResult.Parry && info.canParry)
        {
            if (info.damageAttackerBalanceOnParry && info.attacker != null)
            {
                float parryDamage = stats.GetStats(StatId.ParryDamage);
                info.attacker.TakeBalanceDamage(parryDamage);
                OnParryEnded?.Invoke();
            }

            OnParrySucceed?.Invoke();
            Utils.Log<PlayerController>("패리 성공");
            return true;
        }

        if (info.canGuard)
        {
            Utils.Log<PlayerController>("가드 성공");
            return true;
        }

        return false;
    }

    public void AddGuardBlock()
    {
        guardBlockCount++;
    }

    public void AddParryBlock()
    {
        parryBlockCount++;
    }

    public void RemoveGuardBlock()
    {
        if (guardBlockCount > 0)
            guardBlockCount--;
    }

    public void RemoveParryBlock()
    {
        if (parryBlockCount > 0)
            parryBlockCount--;
    }

    #endregion

    public void SetNormalAttackBalanceConversion(bool isConversion)
    {
        isDamageConversion = isConversion;
    }
}
