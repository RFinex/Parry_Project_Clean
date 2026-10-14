using UnityEngine;

public class PopupBase : UIBase
{
    public UIName UiName => uiName;

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
