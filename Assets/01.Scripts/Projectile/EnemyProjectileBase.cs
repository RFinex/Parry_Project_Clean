using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class EnemyProjectileBase : PoolableGameObject
{
    [SerializeField] private EnemyProjectileData data;

    protected Rigidbody2D rb;

    private EnemyAttackInfo info;

    private float lifeTimer;
    private bool hasHit;

    private CancellationTokenSource token;

    protected float ProjectileSpeed => data.Speed;

    protected EnemyAttackInfo Info => info;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        token = token.ResetToken();
    }

    protected virtual void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public void Shoot(Vector2 dir, EnemyAttackInfo info)
    {
        this.info = info;
        hasHit = false;

        StartLifeTime();

        Launch(dir.normalized);
    }

    public virtual void ShootAt(Vector2 targetPos, EnemyAttackInfo info)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;
        Shoot(dir, info);
    }

    protected virtual void Launch(Vector2 dir)
    {
        rb.linearVelocity = dir * ProjectileSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected virtual void StartLifeTime()
    {
        lifeTimer = 0f;
        UpdateAsync(token.Token).Forget();
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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
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
