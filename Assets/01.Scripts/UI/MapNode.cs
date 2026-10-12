using System;
using UnityEngine;
using UnityEngine.UI;

public class MapNode : UIBase
{
    [SerializeField] private Button btn;
    [SerializeField] private Image icon;

    private RectTransform rectTransform;
    private int nodeId;
    private Action<int> OnSelected;

    private void Awake()
    {
        btn = GetComponent<Button>();
        icon = GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();
        btn.onClick.AddListener(HandleClick);
    }

    public void Initialized(StageMapNodeData data, bool interact, Action<int> selected)
    {
        nodeId = data.id;
        icon.sprite = data.icon;
        rectTransform.anchoredPosition = data.mapPosition;
        btn.interactable = interact;
        OnSelected = selected;
    }

    public void SetInteractable(bool interactable)
    {
        btn.interactable = interactable;
    }

    private void HandleClick()
    {
        OnSelected?.Invoke(nodeId);
    }

    public override void InitPoolReturn()
    {
        nodeId = 0;
        icon.sprite = null;
        btn.interactable = false;
        OnSelected = null;
    }
}
