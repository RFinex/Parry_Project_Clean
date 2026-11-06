using UnityEngine;

[CreateAssetMenu(fileName = "OldSamuraiBaseData", menuName = "Data/Old Samurai Base Data")]
public class OldSamuraiBaseData : EnemyBaseData
{
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float walkDuration = 5f;

    public float WalkSpeed => walkSpeed;
    public float WalkDuration => walkDuration;
}
