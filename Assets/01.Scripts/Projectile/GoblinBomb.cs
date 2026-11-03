using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class GoblinBomb : EnemyProjectileBase
{
    [Header("Flight")]
    [SerializeField, Min(0.01f)] private float flightTime = 0.8f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string bombAnimationName = "Bomb";
    [SerializeField] private GameObject warningBase;
    [SerializeField] private Transform warningFill;
    [SerializeField] private float warningDuration = 1.5f;

    [Header("Explode")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    private int idleAnimHash;
    private int bombAnimHash;

    private Vector3 warningFillFullScale;
    private Tween warningTween;

    private Vector2 targetPos;
    private bool isGround;
    private bool hasExploded;

    private CancellationTokenSource fixedToken;

    protected override void Awake()
    {
        base.Awake();

        idleAnimHash = Animator.StringToHash(idleAnimationName);
        bombAnimHash = Animator.StringToHash(bombAnimationName);

        if (warningFill != null)
            warningFillFullScale = warningFill.localScale;

        if (warningBase != null)
            warningBase.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        fixedToken = fixedToken.ResetToken();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        fixedToken = fixedToken.Cancel_DisposeToken();
    }

    public void ShootAt(Vector2 targetPos, EnemyAttackInfo info)
    {
        this.targetPos = targetPos;
        base.Shoot(Vector2.zero, info);
    }

    protected override void Launch(Vector2 dir)
    {
        rb.gravityScale = gravityScale;

        Vector2 startPos = transform.position;
        Vector2 distance = targetPos - startPos;
        float gravity = Physics2D.gravity.y * gravityScale;

        float velocityX = distance.x / flightTime;
        float velocityY = (distance.y - 0.5f * gravity * flightTime * flightTime) / flightTime;

        rb.linearVelocity = new(velocityX, velocityY);

        FixedUpdateAsync(fixedToken.Token).Forget();
    }

    protected override void StartLifeTime()
    {
        
    }

    private async UniTaskVoid FixedUpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);

                if (isGround || groundCheckPoint == null || rb.linearVelocity.y >= 0f)
                    continue;

                RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);

                if (hit.collider == null)
                    continue;

                isGround = true;

                rb.linearVelocity = Vector2.zero;

                base.StartLifeTime();

                if (animator != null)
                    animator.Play(bombAnimHash, 0, 0f);

                if (warningBase != null)
                    warningBase.SetActive(true);

                if (warningFill != null)
                {
                    warningFill.localScale = Vector3.zero;

                    warningTween = warningFill.DOScale(warningFillFullScale, warningDuration)
                        .SetEase(Ease.Linear)
                        .OnComplete(Explode);

                    await warningTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ctk);
                }

                return;
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        warningBase.SetActive(false);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);

        if (hit == null)
            return;

        PlayerController player = hit.GetComponentInParent<PlayerController>();

        if (player != null)
            player.TakeDamage(Info);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public override void InitPool()
    {
        base.InitPool();

        warningTween?.Kill();
        warningTween = null;

        isGround = false;
        hasExploded = false;

        if (warningBase != null)
            warningBase.SetActive(false);

        if (warningFill != null)
            warningFill.localScale = warningFillFullScale;

        if (animator != null)
            animator.Play(idleAnimHash, 0, 0f);
    }

    public override void InitPoolReturn()
    {
        warningTween?.Kill();
        warningTween = null;

        if (warningBase != null)
            warningBase.SetActive(false);

        base.InitPoolReturn();
    }
}
