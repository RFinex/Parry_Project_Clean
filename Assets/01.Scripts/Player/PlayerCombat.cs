using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private Vector2 attackOffset = new Vector2(0.6f, 0f);
    [SerializeField] private LayerMask enemyLayer;

    // 테스트 공격 시간
    [SerializeField] private float attackDuration = 0.3f;

    [Header("Guard")]
    [SerializeField] private float parryWindow = 0.15f;

    [SerializeField] private bool isParry;

    [SerializeField] private float parryTimer;

    private float attackTimer;

    private PlayerController controller;

    private CancellationTokenSource token;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateParryTimer();
        UpdateAttackTimer();
    }

    private void UpdateParryTimer()
    {
        if (!isParry)
            return;

        parryTimer -= Time.deltaTime;

        if (parryTimer > 0)
            return;

        isParry = false;
        parryTimer = 0f;
        Utils.Log<PlayerCombat>("패링 판정 종료");
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Attack();
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        Utils.Log<PlayerCombat>($"Guard - started:{context.started}, performed:{context.performed}, canceled:{context.canceled}");

        if (context.started)
        {
            GuardStart();
        }
        else if (context.canceled)
        {
            GuardEnd();
        }
    }

    private void Attack()
    {
        if (controller.CurrentState is PlayerGuardState)
            return;

        if (controller.CurrentState is PlayerAttackState)
            return;

        Utils.Log<PlayerCombat>("공격 시작");

        controller.ChangeState<PlayerAttackState>();

        attackTimer = attackDuration;

        Vector2 attackPos = GetAttackPos();

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Utils.Log<PlayerCombat>($"공격 적중 : {hit.name}");

            //EnemyStats enemyStats = hit.GetComponent<EnemyStats>();
            //enemyStats.TakeDamage(attackDamage);
        }        
    }

    private void UpdateAttackTimer()
    {
        if (controller.CurrentState is not PlayerAttackState)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0)
            return;

        ReturnState();

        Utils.Log<PlayerCombat>("공격 종료");
    }

    private void GuardStart()
    {
        if (controller.CurrentState is PlayerGuardState)
            return;

        if (controller.CurrentState is PlayerAttackState)
            return;

        controller.ChangeState<PlayerGuardState>();

        // 패리 시작
        isParry = true;

        parryTimer = parryWindow;

        Utils.Log<PlayerCombat>($"가드 시작 / parryTimer = {parryTimer}");
    }

    //private async UniTaskVoid ParryTimer(CancellationToken ctk)
    //{
    //    try
    //    {
    //        float timer = 0f;

    //        while (timer < parryWindow)
    //        {
    //            timer += Time.deltaTime;

    //            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
    //        }

    //        isParry = false;
    //        Utils.Log<PlayerCombat>("패링 판정 종료");
    //    }
    //    catch (OperationCanceledException)
    //    {

    //    }
    //}

    private void GuardEnd()
    {
        if (controller.CurrentState is not PlayerGuardState)
            return;

        isParry = false;
        parryTimer = 0f;

        ReturnState();

        Utils.Log<PlayerCombat>("가드 종료");
    }

    public bool IsParry()
    {
        return isParry;
    }

    private void ReturnState()
    {
        if (controller.MoveInput.x != 0f)
        {
            controller.ChangeState<PlayerMoveState>();
        }
        else
        {
            controller.ChangeState<PlayerIdleState>();
        }
    }

    private Vector2 GetAttackPos()
    {
        Vector2 offset = attackOffset;

        offset.x *= controller.FrontDir;

        return (Vector2)transform.position + offset;
    }

    private void OnDrawGizmos()
    {
        if (controller == null)
            return;

        Vector2 attackPos = GetAttackPos();

        Gizmos.color = Color.red;

        Gizmos.DrawSphere(attackPos, attackRange);
    }
}
