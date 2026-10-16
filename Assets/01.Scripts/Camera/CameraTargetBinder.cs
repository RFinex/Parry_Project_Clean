using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner2D confiner;

    private RoomSystem roomSystem;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    private void OnEnable()
    {
        TryRegisterRoomSystem();
    }

    private void OnDisable()
    {
        if(roomSystem != null)
            roomSystem.UnRegisterCameraBinder(this);

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

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        }
    }

    public bool SetBounds(PolygonCollider2D bounds)
    {
        if (confiner == null || bounds == null)
            return false;

        confiner.m_BoundingShape2D = bounds;
        confiner.InvalidateCache();

        return true;
    }
}
