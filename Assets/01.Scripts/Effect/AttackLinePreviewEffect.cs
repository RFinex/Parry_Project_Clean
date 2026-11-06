using UnityEngine;

public class AttackLinePreviewEffect : EffectBase
{
    private Vector3 originScale;
    private Quaternion originRotation;

    private void Awake()
    {
        originScale = transform.localScale;
        originRotation = transform.localRotation;
    }

    public override void InitPoolReturn()
    {
        transform.localScale = originScale;
        transform.localRotation = originRotation;

        base.InitPoolReturn();
    }
}
