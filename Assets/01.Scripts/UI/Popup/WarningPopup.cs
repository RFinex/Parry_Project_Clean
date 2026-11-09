using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningPopup : PopupBase
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;

    private Action action;

    private void Awake()
    {
        yesBtn.onClick.AddListener(ShowWarning);
        noBtn.onClick.AddListener(CancelPopup);
    }

    public void SetWarningUI(string message, Action action)
    {
        text.text = message;
        this.action = action;
    }

    private void ShowWarning()
    {
        CancelPopup();
        action?.Invoke();
    }
}
