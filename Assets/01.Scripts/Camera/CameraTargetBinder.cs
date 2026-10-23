using UnityEngine;
using Cinemachine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    [SerializeField] private Transform cameraTargetPivot;
    [SerializeField] private Vector3 targetOffset = new(0f, 0.3f, 0f);

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner2D confiner;

    private RoomSystem roomSystem;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();

        if (virtualCamera != null && cameraTargetPivot != null)
            virtualCamera.Follow = cameraTargetPivot;
    }

    private void OnEnable()
    {
        TryRegisterRoomSystem();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCameraBinder(this);
    }

    private void OnDisable()
    {
        if(roomSystem != null)
            roomSystem.UnRegisterCameraBinder(this);

        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterCameraBinder(this);

        roomSystem = null;
    }

    private void TryRegisterRoomSystem()
    {
        if (roomSystem != null)
            return;

        ServiceLocator.Inject(out roomSystem);

        if (roomSystem != null)
            roomSystem.RegisterCameraBinder(this);
    }

    private void Start()
    {
        TryRegisterRoomSystem();

        if (GameManager.Instance != null)
            CatchTarget(GameManager.Instance.Player.transform);
    }

    public bool SetBounds(PolygonCollider2D bounds)
    {
        if (confiner == null || bounds == null)
            return false;

        confiner.m_BoundingShape2D = bounds;
        confiner.InvalidateCache();

        return true;
    }

    public void CatchTarget(Transform target)
    {
        if (cameraTargetPivot == null || target == null)
            return;

        cameraTargetPivot.DOKill();
        cameraTargetPivot.SetParent(target, false);
        cameraTargetPivot.localPosition = targetOffset;
    }

    public async UniTask MoveToTargetAsync(Transform target, float duration, CancellationToken ctk)
    {
        if (cameraTargetPivot == null || target == null)
            return;

        cameraTargetPivot.DOKill();

        cameraTargetPivot.SetParent(null, true);

        if (duration <= 0f)
        {
            cameraTargetPivot.position = target.position;
            cameraTargetPivot.SetParent(target, true);
            cameraTargetPivot.localPosition = targetOffset;
            return;
        }

        Vector3 targetPos = target.position + targetOffset;

        await cameraTargetPivot.DOMove(targetPos, duration)
            .SetEase(Ease.InOutSine)
            .ToUniTask(TweenCancelBehaviour.CompleteAndCancelAwait, ctk);

        cameraTargetPivot.SetParent(target, true);
        cameraTargetPivot.localPosition = targetOffset;
    }

    public void SetFollowTarget(Transform target)
    {
        CatchTarget(target);
    }
}
