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

    [Header("Guard")]
    [SerializeField] private float parryWindow = 0.15f;

    private bool isGuard;
    private bool isParry;

    private float parryTimer;

    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateParryTimer();
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
        if (isGuard)
            return;

        Utils.Log<PlayerCombat>("공격");

        Vector2 attackPos = (Vector2)transform.position + attackOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Utils.Log<PlayerCombat>($"공격 적중 : {hit.name}");

            //EnemyStats enemyStats = hit.GetComponent<EnemyStats>();
            //enemyStats.TakeDamage(attackDamage);
        }
    }

    private void GuardStart()
    {
        if (isGuard)
            return;

        isGuard = true;
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
        isGuard = false;
        isParry = false;
        parryTimer = 0f;

        Utils.Log<PlayerCombat>("가드 종료");
    }

    public bool IsParry()
    {
        return isParry;
    }
}
