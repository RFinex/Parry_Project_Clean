using UnityEngine;

public struct BossFightStartedEvent
{
    public BossController Boss { get; }

    public BossFightStartedEvent(BossController boss)
    {
        Boss = boss;
    }
}
