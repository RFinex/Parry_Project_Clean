using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Enemy/Boss Data")]
public class BossData : ScriptableObject
{
    [SerializeField, Range(0f, 1f)] private float phase2HpRatio = 0.5f;

    public float Phase2HpRatio => phase2HpRatio;
}
