using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerSkill
{
    public float dashCoolTimer;

    public event Action OnDashEnded;

    public async UniTaskVoid DashCoolTime(float dashCooldown, CancellationToken ctk)
    {
        try
        {
            dashCoolTimer = dashCooldown;

            while (dashCoolTimer >= 0)
            {
                dashCoolTimer -= Time.deltaTime;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }

            dashCoolTimer = 0f;

            Utils.Log<PlayerController>("쿨타임 종료");
        }
        catch (OperationCanceledException)
        {

        }
    }

    public void DashEndedInvoke()
    {
        OnDashEnded?.Invoke();
    }

    public void ReduceDashCooldown(float seconds)
    {
        if (seconds <= 0f)
            return;

        dashCoolTimer -= seconds;

        if (dashCoolTimer <= 0f)
            dashCoolTimer = 0f;
    }
}
