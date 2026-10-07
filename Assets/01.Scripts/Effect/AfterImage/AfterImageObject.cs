using UnityEngine;

public class AfterImageObject : EffectBase
{
    [SerializeField] private SpriteRenderer sr;
    public SpriteRenderer Sr => sr;

    private void Awake()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }
    }

    public override void InitPoolReturn()
    {
        sr.sprite = null;
        sr.color = Color.white;
        sr.flipX = false;
        sr.flipY = false;
    }
}
