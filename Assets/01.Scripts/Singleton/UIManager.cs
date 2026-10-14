using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum UIName
{
    None,
    Popup_Map = 101,
    Popup_Map_Node_Btn = 102,
    Popup_Map_Connection = 103,
    Enemy_Status_Bar = 104
}

public class UIManager : Singleton<UIManager>
{
    [Header("Popup Prefab")]
    [SerializeField] private List<PopupBase> popupPrefabs;

    [Header("Current Scene UI")]
    [SerializeField] private Canvas uiCanvas;

    private readonly Dictionary<UIName, PopupBase> popupPrefabDic = new();
    private readonly Dictionary<UIName, PopupBase> runtimePopupDic = new();

    private int popupSceneHandle = -1;

    protected override void Awake()
    {
        base.Awake();

        InitializePopupPrefabs();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<StageMapCreatedEvent>(HandleStageMapCreated);
        EventBus.Subscribe<StageMapShownEvent>(HandleStageMapShown);
        EventBus.Subscribe<StageMapClosedEvent>(HandleStageMapClosed);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<StageMapCreatedEvent>(HandleStageMapCreated);
        EventBus.UnSubscribe<StageMapShownEvent>(HandleStageMapShown);
        EventBus.UnSubscribe<StageMapClosedEvent>(HandleStageMapClosed);
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        base.OnDestroy();
    }

    private void InitializePopupPrefabs()
    {
        popupPrefabDic.Clear();

        foreach (PopupBase popupPrefab in popupPrefabs)
        {
            if (popupPrefab == null)
                continue;

            if (!popupPrefabDic.TryAdd(popupPrefab.UiName, popupPrefab))
                Debug.LogWarning($"중복 팝업 프리팹: {popupPrefab.UiName}");
        }
    }

    private void HandleStageMapCreated(StageMapCreatedEvent eventData)
    {
        MapPanel map = GetOrCreatePopup<MapPanel>(UIName.Popup_Map);

        if (map != null)
            map.SetMap(eventData.Nodes);
    }

    private void HandleStageMapShown(StageMapShownEvent eventData)
    {
        MapPanel map = GetOrCreatePopup<MapPanel>(UIName.Popup_Map);

        if (map == null)
            return;

        map.ShowNextNodes(eventData.SelectableNodeIds);
        OpenPopup(UIName.Popup_Map);
    }

    private void HandleStageMapClosed(StageMapClosedEvent eventData)
    {
        ClosePopup(UIName.Popup_Map);
    }

    // 현재 씬의 UI 루트가 Canvas를 등록
    public void RegisterSceneCanvas(Canvas canvas)
    {
        uiCanvas = canvas;
        popupSceneHandle = canvas.gameObject.scene.handle;
    }

    public void OpenPopup(UIName name)
    {
        if (uiCanvas == null)
        {
            Debug.LogWarning("현재 씬의 Canvas가 등록되지 않았습니다.");
            return;
        }

        if (!runtimePopupDic.TryGetValue(name, out PopupBase popup) || popup == null)
        {
            if (!popupPrefabDic.TryGetValue(name, out PopupBase prefab))
            {
                Debug.LogWarning($"등록되지 않은 팝업: {name}");
                return;
            }

            popup = Instantiate(prefab, uiCanvas.transform);
            popup.gameObject.SetActive(false);
            runtimePopupDic[name] = popup;
        }

        if (popup.gameObject.activeSelf)
            return;

        popup.gameObject.SetActive(true);
        popup.OpenPopup();
    }

    public void ClosePopup(UIName name)
    {
        if (!runtimePopupDic.TryGetValue(name, out PopupBase popup) || popup == null)
        {
            Debug.LogWarning($"생성되지 않은 팝업: {name}");
            return;
        }

        popup.ClosePopup();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.handle != popupSceneHandle)
            return;

        // 씬 오브젝트는 씬과 함께 파괴되므로 보관 중인 참조를 비움
        runtimePopupDic.Clear();
        uiCanvas = null;
        popupSceneHandle = -1;
    }

    public T GetUI<T>(UIName name, Vector3 pos, Transform parent) where T : UIBase
    {
        if (name == UIName.None)
        {
            Utils.Log<UIManager>("None UIName은 생성할 수 없음");
            return null;
        }

        return ObjectPoolManager.Instance.Get<T>((int)name, pos, parent);
    }

    private T GetOrCreatePopup<T>(UIName name) where T : PopupBase
    {
        if (uiCanvas == null)
        {
            Utils.LogWarning<UIManager>("현재 씬의 Canvas가 등록되지 않았습니다.");
            return null;
        }

        if (runtimePopupDic.TryGetValue(name, out PopupBase cached) && cached != null)
            return cached as T;

        if (!popupPrefabDic.TryGetValue(name, out PopupBase prefab))
        {
            Utils.LogWarning<UIManager>($"등록되지 않은 팝업: {name}");
            return null;
        }

        T popup = Instantiate(prefab, uiCanvas.transform) as T;

        if (popup == null)
        {
            Debug.LogWarning($"{name} 팝업의 타입이 {typeof(T).Name}과 일치하지 않습니다.");
            return null;
        }

        popup.gameObject.SetActive(false);
        runtimePopupDic[name] = popup;

        return popup;
    }
}
