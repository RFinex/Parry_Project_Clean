using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerSkill
{
    public float dashTime;
    public float dashSpeed;
    public float dashCool;

    public float dashCoolTimer;

    public async UniTaskVoid DashCoolTime(CancellationToken ctk)
    {
        try
        {
            dashCoolTimer = dashCool;

            while (dashCoolTimer >= 0)
            {
                dashCoolTimer -= Time.deltaTime;

                await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate, ctk);
            }

            if (dashCoolTimer <= 0)
                dashCoolTimer = 0f;

            Utils.Log<PlayerController>("쿨타임 종료");
        }
        catch (OperationCanceledException)
        {

        }
    }
}
