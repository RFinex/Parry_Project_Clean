using UnityEngine;

public class FlyingEnemyController : NormalEnemyController
{
    [SerializeField, Min(0f)] private float traceStopDistance = 0.5f;

    public override bool Move(Vector2 targetPos)
    {
        if (rb == null || data == null)
            return false;

        Vector2 dir = targetPos - (Vector2)transform.position;
        float arrivalDistance = Mathf.Max(data.ArrivalDistance, data.MoveSpeed * Time.fixedDeltaTime);

        if (dir.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            StopMove();
            return true;
        }

        if(Mathf.Abs(dir.x) > data.ArrivalDistance)
            SetFrontDir(dir.x);

        rb.linearVelocity = dir.normalized * data.MoveSpeed;
        return false;
    }

    public override bool MoveToTarget(Vector2 targetPos)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;

        // 거의 같은 위치일 경우 바라보는 방향 기준
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right * FrontDir;

        // 추적 지점 거리 보정
        Vector2 moveTarget = targetPos - dir.normalized * traceStopDistance;

        return Move(moveTarget);
    }

    public override void StopMove()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
    }
}
