using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyProjectileBase : PoolableGameObject
{
    [SerializeField] private EnemyProjectileData data;

    private Rigidbody2D rb;

    private EnemyAttackInfo info;

    private float lifeTimer;
    private bool hasHit;

    private CancellationTokenSource token;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        token = token.ResetToken();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public void Shoot(Vector2 dir, EnemyAttackInfo info)
    {
        this.info = info;
        lifeTimer = 0f;
        hasHit = false;

        UpdateAsync(token.Token).Forget();

        Vector2 direction = dir.normalized;
        rb.linearVelocity = direction * data.Speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private async UniTaskVoid UpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {   
                if (lifeTimer >= data.LifeTime)
                {
                    ReturnPool();
                    return;
                }

                lifeTimer += Time.deltaTime;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || collision == null)
            return;

        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        PlayerController player = collision.GetComponent<PlayerController>();

        if (player == null)
            return;

        hasHit = true;
        player.TakeDamage(info);
        ReturnPool();
    }

    public override void InitPool()
    {
        hasHit = false;
        lifeTimer = 0f;

        rb.linearVelocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
    }

    public override void InitPoolReturn()
    {
        rb.linearVelocity = Vector2.zero;

        info = default;
        hasHit = false;
        lifeTimer = 0f;
    }
}
