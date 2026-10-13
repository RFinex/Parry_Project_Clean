using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private Transform cameraTransform;
    private Vector3 previousCameraPos;

    [SerializeField] private float parallaxFactor = 0.2f;

    private CancellationTokenSource token;

    private void OnEnable()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Utils.LogError<ParallaxBackground>("Main Camera 등록 실패");
            enabled = false;
            return;
        }

        cameraTransform = mainCamera.transform;

        previousCameraPos = cameraTransform.position;

        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        LateUpdateAsync(token.Token).Forget();
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;
    }

    private async UniTaskVoid LateUpdateAsync(CancellationToken ctk)
    {
        try
        {
            while (!ctk.IsCancellationRequested)
            {
                Vector3 cameraDelta = cameraTransform.position - previousCameraPos;

                transform.position += new Vector3(cameraDelta.x * parallaxFactor, 0f, 0f);

                previousCameraPos = cameraTransform.position;

                await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate, ctk);
            }            
        }
        catch (OperationCanceledException)
        {

        }
    }
}
