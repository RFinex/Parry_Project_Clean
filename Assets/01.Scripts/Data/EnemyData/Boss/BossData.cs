using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Enemy/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("Phase HP Ratio")]
    [SerializeField, Range(0f, 1f)] private float phase2HpRatio = 0.5f;

    [Header("Intro Setting")]
    [SerializeField] private float introJumpSpeed = 5f;
    [SerializeField] private float introJumpForce = 6f;
    [SerializeField] private float introUpJumpHeight = 5f;
    [SerializeField] private float introDownJumpHeight = 1f; 
    [SerializeField] private float introUpJumpDuration = 8f;
    [SerializeField] private float introDownJumpDuration = 4f;

    public float Phase2HpRatio => phase2HpRatio;

    public float IntroJumpSpeed => introJumpSpeed;
    public float IntroJumpForce => introJumpForce;
    public float IntroUpJumpHeight => introUpJumpHeight;
    public float IntroDownJumpHeight => introDownJumpHeight;
    public float IntroUpJumpDuration => introUpJumpDuration;
    public float IntroDownJumpDuration => introDownJumpDuration;
}
