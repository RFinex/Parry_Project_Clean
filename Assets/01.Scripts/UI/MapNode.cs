using UnityEngine;
using UnityEngine.UI;

public class MapNode : UIBase
{
    [SerializeField] private Button btn;
    [SerializeField] private Image icon;

    private RectTransform rectTransform;
    private int nodeId;

    private void Awake()
    {
        btn = GetComponent<Button>();
        icon = GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();
        btn.onClick.AddListener(HandleClick);
    }

    public void Initialized(StageMapNodeData data, bool interact)
    {
        nodeId = data.id;
        icon.sprite = data.icon;
        rectTransform.anchoredPosition = data.mapPosition;
        btn.interactable = interact;
    }

    public void SetInteractable(bool interactable)
    {
        btn.interactable = interactable;
    }

    private void HandleClick()
    {
        EventBus.Publish<MapNodeSelectedEvent>(new MapNodeSelectedEvent(nodeId));
    }

    public override void InitPoolReturn()
    {
        nodeId = 0;
        icon.sprite = null;
        btn.interactable = false;
    }
}
