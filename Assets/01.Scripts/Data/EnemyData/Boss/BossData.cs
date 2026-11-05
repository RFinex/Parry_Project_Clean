using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Enemy/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("Phase HP Ratio")]
    [SerializeField, Range(0f, 1f)] private float phase2HpRatio = 0.5f;

    [Header("Intro Setting")]
    [SerializeField] private float introJumpSpeed = 5f;
    [SerializeField] private float introJumpForce = 6f;

    public float Phase2HpRatio => phase2HpRatio;

    public float IntroJumpSpeed => introJumpSpeed;
    public float IntroJumpForce => introJumpForce;
}
