using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }
    void SetTrigger(bool isTrigger);
    void Interact(PlayerController interactor);
}

public class InteractScanner : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float scanTime = 0.05f;

    private readonly Dictionary<GameObject, IInteractable> interactDic = new();
    private readonly List<GameObject> interactCache = new();

    private GameObject nearestObject;
    private IInteractable nearestTarget;

    private PlayerController player;

    private CancellationTokenSource token;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        Utils.EnterToken(ref token);

        UpdateAsync(token.Token).Forget();
    }

    private void OnDisable()
    {
        Utils.ExitToken(ref token);

        interactCache.Clear();

        foreach (var pair in interactDic)
        {
            Component targetComponent = pair.Value as Component;

            if (targetComponent != null)
                pair.Value.SetTrigger(false);

            interactCache.Add(pair.Key);
        }

        for (int i = 0; i < interactCache.Count; i++)
        {
            interactDic.Remove(interactCache[i]);
        }

        interactCache.Clear();
        nearestObject = null;
        nearestTarget = null;
    }

    private async UniTaskVoid UpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(scanTime), cancellationToken: ctk);

                RefreshNearestTarget();
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((targetLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        IInteractable target = collision.gameObject.GetComponentInParent<IInteractable>();
        Component targetComponent = target as Component;

        if (targetComponent == null)
            return;

        GameObject targetObject = targetComponent.gameObject;

        if (!interactDic.TryAdd(targetObject, target))
            return;

        RefreshNearestTarget();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable target = collision.GetComponentInParent<IInteractable>();
        Component targetComponent = target as Component;

        if (targetComponent == null)
            return;

        RemoveTarget(targetComponent.gameObject);
        RefreshNearestTarget();
    }

    public void TryInteract()
    {
        RefreshNearestTarget();

        if (nearestTarget == null || !nearestTarget.CanInteract)
            return;

        nearestTarget.Interact(player);

        RefreshNearestTarget();
    }

    private void RefreshNearestTarget()
    {
        GameObject previousObject = nearestObject;
        IInteractable previousTarget = nearestTarget;

        nearestObject = null;
        nearestTarget = null;

        float nearestDistance = float.MaxValue;
        Vector2 scannerPos = transform.position;

        interactCache.Clear();

        foreach (var pair in interactDic)
        {
            GameObject targetObject = pair.Key;
            IInteractable target = pair.Value;
            Component targetComponent = target as Component;

            if (targetObject == null || targetComponent == null || !targetObject.activeInHierarchy)
            {
                interactCache.Add(targetObject);
                continue;
            }

            if (!target.CanInteract)
                continue;

            float distance = ((Vector2)targetObject.transform.position - scannerPos).sqrMagnitude;
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestObject = targetObject;
            nearestTarget = target;
        }

        for (int i = 0; i < interactCache.Count; i++)
            RemoveTarget(interactCache[i]);

        if (previousObject == nearestObject)
            return;

        Component previousComponent = previousTarget as Component;

        if (previousComponent != null)
            previousTarget.SetTrigger(false);

        if (nearestTarget != null)
            nearestTarget.SetTrigger(true);
    }

    private void RemoveTarget(GameObject targetObject)
    {
        if (ReferenceEquals(targetObject, null) || !interactDic.TryGetValue(targetObject, out IInteractable target))
            return;

        Component targetComponent = target as Component;

        if (targetComponent != null)
            target.SetTrigger(false);

        interactDic.Remove(targetObject);

        if (nearestObject == targetObject)
        {
            nearestObject = null;
            nearestTarget = null;
        }
    }
}
