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

    private GameObject dim;

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
                Utils.LogWarning<UIManager>($"중복 팝업 프리팹: {popupPrefab.UiName}");
        }
    }

    #region Map Popup

    /// <summary>
    /// 맵 데이터 생성 후 지도 생성
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleStageMapCreated(StageMapCreatedEvent eventData)
    {
        MapPanel map = GetOrCreatePopup<MapPanel>(UIName.Popup_Map);

        if (map != null)
            map.SetMap(eventData.nodes);
    }

    /// <summary>
    /// 방 클리어 후 다음 노드 활성화 및 OpenPopup 호출
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleStageMapShown(StageMapShownEvent eventData)
    {
        MapPanel map = GetOrCreatePopup<MapPanel>(UIName.Popup_Map);

        if (map == null)
            return;

        map.ShowNextNodes(eventData.selectableNodeIds);
        OpenPopup(UIName.Popup_Map);
    }

    /// <summary>
    /// 노드 선택 후 ClosePopup 호출
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleStageMapClosed(StageMapClosedEvent eventData)
    {
        ClosePopup(UIName.Popup_Map);
    }

    #endregion

    // 현재 씬의 UI 루트가 Canvas를 등록
    public void RegisterSceneCanvas(Canvas canvas, GameObject dim)
    {
        uiCanvas = canvas;
        this.dim = dim;
        popupSceneHandle = canvas.gameObject.scene.handle;

        if (dim != null)
            dim.SetActive(false);
    }

    public void OpenPopup(UIName name)
    {
        if (uiCanvas == null)
        {
            Utils.LogWarning<UIManager>("현재 씬의 Canvas가 등록되지 않았습니다.");
            return;
        }

        if (!runtimePopupDic.TryGetValue(name, out PopupBase popup) || popup == null)
        {
            if (!popupPrefabDic.TryGetValue(name, out PopupBase prefab))
            {
                Utils.LogWarning<UIManager>($"등록되지 않은 팝업: {name}");
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
        popup.transform.SetAsLastSibling();

        RefreshPopupDim();
    }

    public void ClosePopup(UIName name)
    {
        if (!runtimePopupDic.TryGetValue(name, out PopupBase popup) || popup == null)
        {
            Utils.LogWarning<UIManager>($"생성되지 않은 팝업: {name}");
            return;
        }

        popup.ClosePopup();
        RefreshPopupDim();
    }

    private void RefreshPopupDim()
    {
        if (dim == null)
            return;

        int popupIndex = int.MaxValue;

        foreach (PopupBase popup in runtimePopupDic.Values)
        {
            // 꺼져있거나 dim이 필요없다면 넘기기
            if (popup == null || !popup.gameObject.activeSelf || !popup.UseDim)
                continue;

            int index = popup.transform.GetSiblingIndex();

            if (index < popupIndex)
                popupIndex = index;
        }

        if (popupIndex == int.MaxValue)
        {
            dim.SetActive(false);
            return;
        }

        dim.SetActive(true);

        int currentDimIndex = dim.transform.GetSiblingIndex();

        if (currentDimIndex < popupIndex)
            popupIndex--;

        dim.transform.SetSiblingIndex(popupIndex);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.handle != popupSceneHandle)
            return;

        // 씬 오브젝트는 씬과 함께 파괴되므로 보관 중인 참조를 비움
        runtimePopupDic.Clear();
        uiCanvas = null;
        popupSceneHandle = -1;
        dim = null;
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
            Utils.LogWarning<UIManager>($"{name} 팝업의 타입이 {typeof(T).Name}과 일치하지 않습니다.");
            return null;
        }

        popup.gameObject.SetActive(false);
        runtimePopupDic[name] = popup;

        return popup;
    }
}
