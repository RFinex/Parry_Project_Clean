using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ItemPickup : InteractableBase
{
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private ItemPickupPool poolObject;
    [SerializeField] private LayerMask groundLayer;

    [Header("Drop Motion")]
    [SerializeField] private float popHeight = 0.8f;
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float raycastDistance = 30f;
    [SerializeField] private float landOffset = 0.15f;
    [SerializeField] private float fallDuration = 0.5f;

    [Header("Floating")]
    [SerializeField] private float floatHeight = 0.06f;
    [SerializeField] private float floatDuration = 0.8f;

    private Item item;
    private CancellationTokenSource dropToken;
    private Tween dropTween;
    private Tween floatTween;

    private bool isDropping;
    private Vector3 baseRendererLocalPos;

    public override bool CanInteract => !isDropping && item is Artifact artifact && artifact.ArtifactItemData != null;

    protected override void Awake()
    {
        base.Awake();

        if(itemRenderer != null)
            baseRendererLocalPos = itemRenderer.transform.localPosition;
    }

    /// <summary>
    /// 아이템 스프라이트를 데이터의 스프라이트로 교체
    /// </summary>
    /// <param name="item"> 스프라이트 교체할 아이템 </param>
    public void SetItem(Item item)
    {
        dropToken = dropToken.Cancel_DisposeToken();

        this.item = item;

        if (itemRenderer != null)
        {
            itemRenderer.sprite = item != null && item.Data != null ? item.Data.Icon : null;
            itemRenderer.transform.localPosition = baseRendererLocalPos;
        }

        if (CanInteract)
            StartDropMotion();
        else
            RefreshIcon();
    }

    public override void Interact(PlayerController interactor)
    {
        if (interactor == null || !CanInteract || poolObject == null)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null || manager.Inventory == null)
            return;

        Artifact artifact = item as Artifact;

        if (!manager.Inventory.TryAdd(artifact))
        {
            Utils.Log<ItemPickup>("인벤토리 공간 부족");
            return;
        }

        poolObject.ReturnPool();
    }

    public void ClearItem()
    {
        dropToken = dropToken.Cancel_DisposeToken();

        item = null;

        isDropping = false;
        dropTween?.Kill();
        dropTween = null;
        floatTween?.Kill();
        floatTween = null;

        if (itemRenderer != null)
        {
            itemRenderer.sprite = null;
            itemRenderer.transform.localPosition = baseRendererLocalPos;
        }

        SetTrigger(false);
        RefreshIcon();
    }

    private void StartDropMotion()
    {
        dropToken = dropToken.ResetToken();

        isDropping = true;
        RefreshIcon();

        PoppingItemAsync(dropToken.Token).Forget();
    }

    private async UniTaskVoid PoppingItemAsync(CancellationToken ctk)
    {
        Vector3 startPos = transform.position;
        Vector3 popPos = startPos + Vector3.up * popHeight;

        try
        {
            // 위로 튀어오르기
            dropTween = transform.DOMove(popPos, popDuration)
                .SetEase(Ease.OutQuad);

            await dropTween.ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

            dropTween = null;

            // 착지할 바닥 감지
            RaycastHit2D hit = Physics2D.Raycast(popPos, Vector2.down, raycastDistance, groundLayer);

            // 바닥을 감지하지 못하면 현재 위치 그대로 
            if (hit.collider == null)
            {
                transform.position = startPos;
                isDropping = false;
                RefreshIcon();
                return;
            }

            Vector3 landPos = new(popPos.x, hit.point.y + landOffset, startPos.z);

            dropTween = transform.DOMove(landPos, fallDuration)
                .SetEase(Ease.InQuad);

            // 바닥까지 내려가기
            await dropTween.ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

            dropTween = null;
            isDropping = false;
            RefreshIcon();

            if (itemRenderer == null)
                return;

            floatTween = itemRenderer.transform.DOLocalMoveY(baseRendererLocalPos.y + floatHeight, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            await floatTween.ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void OnDestroy()
    {
        dropToken = dropToken.Cancel_DisposeToken();
    }
}
