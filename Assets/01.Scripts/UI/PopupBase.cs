using UnityEngine;

public class PopupBase : UIBase
{
    public UIName UiName => uiName;

    [SerializeField] private bool useDim;
    public bool UseDim => useDim;

    public virtual void OpenPopup()
    {
        Initialized();
        PlayOpenTween();
    }

    public virtual void ClosePopup()
    {
        PlayCloseTween();
    }

    protected virtual void Initialized()
    {

    }

    protected virtual void PlayOpenTween()
    {

    }
    protected virtual void PlayCloseTween()
    {

        gameObject.SetActive(false);
    }
}
