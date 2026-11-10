using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Authoring-only BT graph editor. Graph assets are sidecar assets and are not
/// consumed by the runtime behavior trees yet.
/// </summary>
public sealed class BossBTSequenceEditor : EditorWindow
{
    internal const int MaxGraphNodes = 128;

    private sealed class MonsterEntry
    {
        public string prefabPath;
        public string hierarchyPath;
        public string label;
        public EliteEnemyController controller;
    }

    private readonly List<MonsterEntry> monsters = new();
    private MonsterEntry selectedMonster;
    private EnemyBTGraphAsset graphAsset;
    private EnemyBTGraphData activeGraph;
    private EnemyBTGraphView graphView;
    private VisualElement graphHost;
    private VisualElement inspectorHost;
    private VisualElement skillsHost;
    private string selectedNodeId;
    private int selectedGraphTab;
    private ToolbarMenu monsterMenu;
    private ToolbarToggle phase1Toggle;
    private ToolbarToggle phase2Toggle;
    private Label graphAssetLabel;

    [MenuItem("Tools/Enemy/BT Graph Editor")]
    private static void Open()
    {
        BossBTSequenceEditor window = GetWindow<BossBTSequenceEditor>("Enemy BT Graph");
        window.minSize = new Vector2(1100f, 620f);
    }

    private void CreateGUI()
    {
        rootVisualElement.Clear();
        BuildToolbar();

        VisualElement body = new();
        body.style.flexDirection = FlexDirection.Row;
        body.style.flexGrow = 1f;
        rootVisualElement.Add(body);

        skillsHost = CreatePanel(body, "Monster Skills", 245f);

        graphHost = new VisualElement();
        graphHost.style.flexGrow = 1f;
        graphHost.style.minWidth = 400f;
        body.Add(graphHost);

        inspectorHost = CreatePanel(body, "Selected Node", 310f);

        RefreshMonsterList();
    }

    private static VisualElement CreatePanel(VisualElement parent, string title, float width)
    {
        VisualElement panel = new();
        panel.style.width = width;
        panel.style.minWidth = width;
        panel.style.flexShrink = 0f;
        panel.style.paddingLeft = 8f;
        panel.style.paddingRight = 8f;
        panel.style.paddingTop = 6f;
        panel.style.borderRightWidth = 1f;
        panel.style.borderRightColor = new Color(0.24f, 0.24f, 0.24f);
        panel.Add(new Label(title));
        parent.Add(panel);
        return panel;
    }

    private void BuildToolbar()
    {
        Toolbar toolbar = new();
        monsterMenu = new ToolbarMenu { text = "Select Monster Prefab" };
        toolbar.Add(monsterMenu);

        graphAssetLabel = new Label("No graph selected");
        graphAssetLabel.style.flexGrow = 1f;
        graphAssetLabel.style.marginLeft = 8f;
        toolbar.Add(graphAssetLabel);

        phase1Toggle = new ToolbarToggle { text = "Phase 1" };
        phase2Toggle = new ToolbarToggle { text = "Phase 2" };
        phase1Toggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
                SelectGraphTab(1);
        });
        phase2Toggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
                SelectGraphTab(2);
        });
        toolbar.Add(phase1Toggle);
        toolbar.Add(phase2Toggle);

        Button refreshButton = new(RefreshMonsterList) { text = "Refresh Prefabs" };
        toolbar.Add(refreshButton);
        rootVisualElement.Add(toolbar);
    }

    private void RefreshMonsterList()
    {
        monsters.Clear();
        monsterMenu?.menu.ClearItems();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            EliteEnemyController[] controllers = prefab.GetComponentsInChildren<EliteEnemyController>(true);
            foreach (EliteEnemyController controller in controllers)
            {
                string hierarchyPath = GetHierarchyPath(controller.transform, prefab.transform);
                MonsterEntry entry = new()
                {
                    prefabPath = path,
                    hierarchyPath = hierarchyPath,
                    controller = controller,
                    label = $"{controller.GetType().Name} | {path} | {hierarchyPath}"
                };
                monsters.Add(entry);
            }
        }

        monsters.Sort((left, right) => string.CompareOrdinal(left.label, right.label));
        foreach (MonsterEntry entry in monsters)
            monsterMenu.menu.AppendAction(entry.label, _ => SelectMonster(entry));

        if (selectedMonster != null)
        {
            selectedMonster = monsters.FirstOrDefault(entry =>
                entry.prefabPath == selectedMonster.prefabPath &&
                entry.hierarchyPath == selectedMonster.hierarchyPath &&
                entry.controller.GetType() == selectedMonster.controller.GetType());
            if (selectedMonster == null)
            {
                graphAsset = null;
                activeGraph = null;
            }
        }

        RefreshToolbar();
        RefreshSelectionView();
    }

    private static string GetHierarchyPath(Transform target, Transform prefabRoot)
    {
        List<string> names = new();
        Transform current = target;
        while (current != null && current != prefabRoot)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return names.Count == 0 ? prefabRoot.name : $"{prefabRoot.name}/{string.Join("/", names)}";
    }

    private void SelectMonster(MonsterEntry entry)
    {
        selectedMonster = entry;
        selectedNodeId = null;
        selectedGraphTab = entry.controller is BossController ? 1 : 0;
        graphAsset = AssetDatabase.LoadAssetAtPath<EnemyBTGraphAsset>(GetGraphAssetPath(entry));
        RefreshToolbar();
        RefreshSelectionView();
    }

    private static string GetGraphAssetPath(MonsterEntry entry)
    {
        string basePath = entry.prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
            ? entry.prefabPath[..^".prefab".Length]
            : entry.prefabPath;
        string safeTypeName = SanitizePathSegment(entry.controller.GetType().Name);
        string safeHierarchy = SanitizePathSegment(entry.hierarchyPath);
        return $"{basePath}.{safeTypeName}_{safeHierarchy}.BTGraph.asset";
    }

    private static string SanitizePathSegment(string value)
    {
        char[] characters = value.Select(character =>
            char.IsLetterOrDigit(character) || character == '_' || character == '-'
                ? character
                : '_').ToArray();
        return new string(characters);
    }

    private void RefreshToolbar()
    {
        if (monsterMenu != null)
            monsterMenu.text = selectedMonster != null ? selectedMonster.controller.GetType().Name : "Select Monster Prefab";

        bool isBoss = selectedMonster != null && selectedMonster.controller is BossController;
        if (phase1Toggle != null)
        {
            phase1Toggle.style.display = isBoss ? DisplayStyle.Flex : DisplayStyle.None;
            phase2Toggle.style.display = isBoss ? DisplayStyle.Flex : DisplayStyle.None;
            phase1Toggle.SetValueWithoutNotify(isBoss && selectedGraphTab == 1);
            phase2Toggle.SetValueWithoutNotify(isBoss && selectedGraphTab == 2);
        }

        if (graphAssetLabel != null)
        {
            graphAssetLabel.text = graphAsset != null
                ? AssetDatabase.GetAssetPath(graphAsset)
                : selectedMonster == null ? "No graph selected" : "Graph asset not created";
        }
    }

    private void SelectGraphTab(int tab)
    {
        if (selectedMonster == null || selectedMonster.controller is not BossController)
            return;

        selectedGraphTab = tab;
        selectedNodeId = null;
        RefreshToolbar();
        RefreshSelectionView();
    }

    private void RefreshSelectionView()
    {
        if (skillsHost == null || graphHost == null || inspectorHost == null)
            return;

        skillsHost.Clear();
        skillsHost.Add(new Label("Drag a skill into the graph to create an Action node."));
        graphHost.Clear();
        inspectorHost.Clear();
        graphView = null;

        if (selectedMonster == null)
        {
            graphHost.Add(new HelpBox("Select an Elite or Boss prefab from the toolbar.", HelpBoxMessageType.Info));
            RefreshToolbar();
            return;
        }

        List<EnemySkillData> ownedSkills = GetOwnedSkills();
        DrawSkillList(ownedSkills);
        activeGraph = GetActiveGraph();

        if (graphAsset == null)
        {
            graphHost.Add(new HelpBox(
                "This prefab does not have an authoring graph yet. Create one to save nodes and links. Runtime BT is not connected to this graph.",
                HelpBoxMessageType.Info));
            graphHost.Add(new Button(CreateGraphAsset) { text = "Create Graph Asset" });
            RefreshToolbar();
            return;
        }

        if (activeGraph == null)
        {
            graphHost.Add(new HelpBox("The selected monster/phase graph data is unavailable.", HelpBoxMessageType.Warning));
            RefreshToolbar();
            return;
        }

        EnemyBTGraphAsset capturedAsset = graphAsset;
        EnemyBTGraphData capturedGraph = activeGraph;
        graphView = new EnemyBTGraphView(
            capturedGraph,
            ownedSkills,
            selectedNodeId,
            id =>
            {
                selectedNodeId = id;
                RefreshInspector();
            },
            view => SaveGraph(capturedAsset, capturedGraph, view));
        graphHost.Add(graphView);
        DrawInspector();
        RefreshToolbar();
    }

    private void CreateGraphAsset()
    {
        if (selectedMonster == null)
            return;

        string path = GetGraphAssetPath(selectedMonster);
        EnemyBTGraphAsset existingAsset = AssetDatabase.LoadAssetAtPath<EnemyBTGraphAsset>(path);
        if (existingAsset != null)
        {
            graphAsset = existingAsset;
            RefreshToolbar();
            RefreshSelectionView();
            return;
        }

        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
        {
            Debug.LogWarning($"Cannot create BT graph because another asset already uses this path: {path}");
            return;
        }

        graphAsset = CreateInstance<EnemyBTGraphAsset>();
        AssetDatabase.CreateAsset(graphAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshToolbar();
        RefreshSelectionView();
    }

    private EnemyBTGraphData GetActiveGraph()
    {
        if (graphAsset == null)
            return null;

        if (selectedMonster.controller is BossController)
        {
            return selectedGraphTab == 2 ? graphAsset.Phase2 : graphAsset.Phase1;
        }

        return graphAsset.Combat;
    }

    private List<EnemySkillData> GetOwnedSkills()
    {
        List<EnemySkillData> result = new();
        if (selectedMonster?.controller == null)
            return result;

        HashSet<EnemySkillData> seen = new();
        SerializedObject serializedController = new(selectedMonster.controller);
        SerializedProperty iterator = serializedController.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = true;
            if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                continue;

            if (iterator.objectReferenceValue is EnemySkillData skillData && seen.Add(skillData))
                result.Add(skillData);
        }

        result.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        return result;
    }

    private void DrawSkillList(List<EnemySkillData> skills)
    {
        if (skills.Count == 0)
        {
            skillsHost.Add(new HelpBox("No EnemySkillData references were found on this prefab component.", HelpBoxMessageType.Warning));
            return;
        }

        foreach (EnemySkillData skill in skills)
        {
            Label row = new(skill.name)
            {
                tooltip = $"{skill.GetType().Name}\n{AssetDatabase.GetAssetPath(skill)}"
            };
            row.style.height = 27f;
            row.style.paddingLeft = 6f;
            row.style.paddingTop = 5f;
            row.style.marginTop = 2f;
            row.style.backgroundColor = new Color(0.22f, 0.23f, 0.25f);
            row.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { skill };
                DragAndDrop.StartDrag(skill.name);
                evt.StopPropagation();
            });
            skillsHost.Add(row);
        }
    }

    private void DrawInspector()
    {
        inspectorHost.Clear();
        inspectorHost.Add(new Label("Selected Node"));
        EnemyBTGraphNodeData node = FindSelectedNode();
        if (node == null)
        {
            inspectorHost.Add(new HelpBox("Select a node to inspect it. Right-click the canvas to create composites/actions.", HelpBoxMessageType.Info));
            return;
        }

        inspectorHost.Add(new Label($"{node.type}  |  {node.id}"));
        if (node.type == EnemyBTGraphNodeType.Sequence || node.type == EnemyBTGraphNodeType.Selector)
        {
            Button setRoot = new(() =>
            {
                activeGraph.SetRoot(node.id);
                graphView?.RefreshRootMarker();
                SaveCurrentGraph();
                RefreshInspector();
            }) { text = activeGraph.RootNodeId == node.id ? "Root Node" : "Set as Root" };
            inspectorHost.Add(setRoot);
        }

        if (node.type == EnemyBTGraphNodeType.SkillAction)
        {
            ObjectField skillField = new("Skill Data")
            {
                objectType = typeof(EnemySkillData),
                allowSceneObjects = false,
                value = node.skillData
            };
            skillField.RegisterValueChangedCallback(evt =>
            {
                node.skillData = evt.newValue as EnemySkillData;
                SaveCurrentGraph();
                graphView?.RefreshNode(node.id);
            });
            inspectorHost.Add(skillField);

            if (node.skillData != null)
            {
                inspectorHost.Add(new HelpBox(
                    "The fields below edit the referenced EnemySkillData asset itself. Other uses of this shared asset will see the same changes.",
                    HelpBoxMessageType.Warning));
                IMGUIContainer dataInspector = new(() => DrawSkillData(node.skillData));
                dataInspector.style.marginTop = 8f;
                inspectorHost.Add(dataInspector);
            }
        }

        inspectorHost.Add(new Button(() =>
        {
            graphView?.DeleteSelectedNode(node.id);
            selectedNodeId = null;
            RefreshInspector();
        }) { text = "Delete Node" });
    }

    private static void DrawSkillData(EnemySkillData skillData)
    {
        if (skillData == null)
            return;

        SerializedObject serializedSkill = new(skillData);
        serializedSkill.UpdateIfRequiredOrScript();
        SerializedProperty property = serializedSkill.GetIterator();
        bool enterChildren = true;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        while (property.NextVisible(enterChildren))
        {
            enterChildren = true;
            if (property.propertyPath == "m_Script")
                continue;

            EditorGUILayout.PropertyField(property, true);
        }
        EditorGUILayout.EndVertical();

        if (serializedSkill.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(skillData);
            AssetDatabase.SaveAssets();
        }
    }

    private void RefreshInspector()
    {
        if (inspectorHost != null && activeGraph != null)
            DrawInspector();
    }

    private EnemyBTGraphNodeData FindSelectedNode()
    {
        if (activeGraph == null || string.IsNullOrEmpty(selectedNodeId))
            return null;

        return activeGraph.Nodes.FirstOrDefault(node => node != null && node.id == selectedNodeId);
    }

    private void SaveCurrentGraph()
    {
        if (graphAsset == null || graphView == null || activeGraph == null)
            return;

        SaveGraph(graphAsset, activeGraph, graphView);
    }

    private void SaveGraph(EnemyBTGraphAsset asset, EnemyBTGraphData graph, EnemyBTGraphView view)
    {
        if (asset == null || graph == null || view == null)
            return;

        view.WriteToModel(graph, MaxGraphNodes);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        if (asset == graphAsset && graph == activeGraph &&
            !string.IsNullOrEmpty(selectedNodeId) && FindSelectedNode() == null)
        {
            selectedNodeId = null;
            RefreshInspector();
        }
        RefreshToolbar();
    }
}

internal sealed class EnemyBTGraphView : GraphView
{
    private readonly EnemyBTGraphData graphData;
    private readonly List<EnemySkillData> ownedSkills;
    private readonly Action<string> onNodeSelected;
    private readonly Action<EnemyBTGraphView> onGraphChanged;
    private readonly Dictionary<string, EnemyBTGraphNodeView> nodeViews = new();

    public EnemyBTGraphView(
        EnemyBTGraphData graphData,
        List<EnemySkillData> ownedSkills,
        string selectedNodeId,
        Action<string> onNodeSelected,
        Action<EnemyBTGraphView> onGraphChanged)
    {
        this.graphData = graphData;
        this.ownedSkills = ownedSkills;
        this.onNodeSelected = onNodeSelected;
        this.onGraphChanged = onGraphChanged;

        style.flexGrow = 1f;
        Insert(0, new GridBackground());
        this.Q<GridBackground>().StretchToParentSize();
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        graphViewChanged = HandleGraphViewChanged;

        foreach (EnemyBTGraphNodeData nodeData in graphData.Nodes)
        {
            if (nodeData == null || string.IsNullOrEmpty(nodeData.id))
                continue;

            EnemyBTGraphNodeView nodeView = CreateNodeView(nodeData);
            AddElement(nodeView);
            if (selectedNodeId == nodeData.id)
                AddToSelection(nodeView);
            nodeViews[nodeData.id] = nodeView;
        }

        foreach (EnemyBTGraphEdgeData edgeData in graphData.Edges)
        {
            if (edgeData == null ||
                !nodeViews.TryGetValue(edgeData.parentId, out EnemyBTGraphNodeView parent) ||
                !nodeViews.TryGetValue(edgeData.childId, out EnemyBTGraphNodeView child) ||
                parent.Output == null || child.Input == null || child.Input.connections.Any())
                continue;

            Edge edge = parent.Output.ConnectTo(child.Input);
            AddElement(edge);
        }

        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
        RefreshRootMarker();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(port =>
            port != startPort &&
            port.node != startPort.node &&
            port.direction != startPort.direction &&
            port.direction == Direction.Input &&
            port.capacity == Port.Capacity.Single &&
            !port.connections.Any()).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        Vector2 position = contentViewContainer.WorldToLocal(evt.mousePosition);
        evt.menu.AppendAction("Create/Sequence", _ => AddNode(EnemyBTGraphNodeType.Sequence, position));
        evt.menu.AppendAction("Create/Selector", _ => AddNode(EnemyBTGraphNodeType.Selector, position));
        evt.menu.AppendAction("Create/Trace Action", _ => AddNode(EnemyBTGraphNodeType.TraceAction, position));
        evt.menu.AppendAction("Create/Idle Action", _ => AddNode(EnemyBTGraphNodeType.IdleAction, position));
        evt.menu.AppendAction("Set Selected Composite as Root", _ => SetSelectedAsRoot(),
            _ => selection.OfType<EnemyBTGraphNodeView>().Any(view => view.Output != null)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
    }

    public void RefreshRootMarker()
    {
        foreach (EnemyBTGraphNodeView node in nodeViews.Values)
            node.SetRoot(node.Data.id == graphData.RootNodeId);
    }

    public void RefreshNode(string id)
    {
        if (nodeViews.TryGetValue(id, out EnemyBTGraphNodeView view))
            view.RefreshTitle();
    }

    public void DeleteSelectedNode(string id)
    {
        if (!nodeViews.TryGetValue(id, out EnemyBTGraphNodeView view))
            return;

        DeleteElements(new GraphElement[] { view });
        nodeViews.Remove(id);
        if (graphData.RootNodeId == id)
            graphData.SetRoot(null);
        EditorApplication.delayCall += () => onGraphChanged?.Invoke(this);
        RefreshRootMarker();
    }

    public void WriteToModel(EnemyBTGraphData target, int maxNodes)
    {
        List<EnemyBTGraphNodeView> views = nodes.ToList().OfType<EnemyBTGraphNodeView>().Take(maxNodes).ToList();
        HashSet<string> ids = new(views.Select(view => view.Data.id));

        target.Nodes.Clear();
        foreach (EnemyBTGraphNodeView view in views)
        {
            Rect rect = view.GetPosition();
            view.Data.position = rect.position;
            target.Nodes.Add(view.Data);
        }

        target.Edges.Clear();
        List<(string parent, string child, float parentY, float childY)> connections = new();
        foreach (Edge edge in edges.ToList())
        {
            if (edge.output?.node is not EnemyBTGraphNodeView parent ||
                edge.input?.node is not EnemyBTGraphNodeView child ||
                !ids.Contains(parent.Data.id) || !ids.Contains(child.Data.id))
                continue;

            connections.Add((parent.Data.id, child.Data.id, parent.GetPosition().y, child.GetPosition().y));
        }

        // For authoring, vertical order is the displayed child order per composite.
        foreach (var connection in connections.OrderBy(item => item.parentY).ThenBy(item => item.childY))
        {
            target.Edges.Add(new EnemyBTGraphEdgeData
            {
                parentId = connection.parent,
                childId = connection.child
            });
        }

        EnemyBTGraphNodeView root = views.FirstOrDefault(view => view.Data.id == target.RootNodeId);
        if (root == null || root.Output == null)
            target.SetRoot(null);
    }

    private GraphViewChange HandleGraphViewChanged(GraphViewChange change)
    {
        if (change.elementsToRemove != null)
        {
            foreach (EnemyBTGraphNodeView node in change.elementsToRemove.OfType<EnemyBTGraphNodeView>())
            {
                nodeViews.Remove(node.Data.id);
                if (graphData.RootNodeId == node.Data.id)
                    graphData.SetRoot(null);
            }
        }

        if (change.edgesToCreate != null)
        {
            List<Edge> accepted = new();
            foreach (Edge candidate in change.edgesToCreate)
            {
                if (candidate.output?.node is not EnemyBTGraphNodeView parent ||
                    candidate.input?.node is not EnemyBTGraphNodeView child ||
                    WouldCreateCycle(parent.Data.id, child.Data.id, accepted))
                    continue;

                accepted.Add(candidate);
            }
            change.edgesToCreate = accepted;
        }

        if (change.elementsToRemove != null || change.edgesToCreate != null || change.movedElements != null)
        {
            EditorApplication.delayCall += () =>
            {
                RefreshRootMarker();
                onGraphChanged?.Invoke(this);
            };
        }

        return change;
    }

    private bool WouldCreateCycle(string parentId, string childId, List<Edge> pending)
    {
        if (parentId == childId)
            return true;

        Stack<string> stack = new();
        HashSet<string> visited = new();
        stack.Push(childId);

        while (stack.Count > 0)
        {
            string current = stack.Pop();
            if (!visited.Add(current))
                continue;
            if (current == parentId)
                return true;

            foreach (Edge edge in edges.ToList())
            {
                if (edge.output?.node is EnemyBTGraphNodeView from &&
                    edge.input?.node is EnemyBTGraphNodeView to &&
                    from.Data.id == current)
                    stack.Push(to.Data.id);
            }

            foreach (Edge edge in pending)
            {
                if (edge.output?.node is EnemyBTGraphNodeView from &&
                    edge.input?.node is EnemyBTGraphNodeView to &&
                    from.Data.id == current)
                    stack.Push(to.Data.id);
            }
        }

        return false;
    }

    private void AddNode(EnemyBTGraphNodeType type, Vector2 position, EnemySkillData skill = null)
    {
        if (nodeViews.Count >= BossBTSequenceEditor.MaxGraphNodes)
        {
            Debug.LogWarning($"BT graph node limit reached ({BossBTSequenceEditor.MaxGraphNodes}).");
            return;
        }

        EnemyBTGraphNodeData data = new()
        {
            id = Guid.NewGuid().ToString("N"),
            type = type,
            position = position,
            skillData = skill
        };
        EnemyBTGraphNodeView view = CreateNodeView(data);
        AddElement(view);
        nodeViews.Add(data.id, view);
        ClearSelection();
        AddToSelection(view);
        onGraphChanged?.Invoke(this);
        onNodeSelected?.Invoke(data.id);
    }

    private EnemyBTGraphNodeView CreateNodeView(EnemyBTGraphNodeData data)
    {
        EnemyBTGraphNodeView view = new(data, id => onNodeSelected?.Invoke(id));
        view.SetPosition(new Rect(data.position, new Vector2(220f, 105f)));
        return view;
    }

    private void SetSelectedAsRoot()
    {
        EnemyBTGraphNodeView selected = selection.OfType<EnemyBTGraphNodeView>().FirstOrDefault();
        if (selected == null || selected.Output == null)
            return;

        graphData.SetRoot(selected.Data.id);
        RefreshRootMarker();
        onGraphChanged?.Invoke(this);
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        DragAndDrop.visualMode = DragAndDrop.objectReferences.Any(item => item is EnemySkillData)
            ? DragAndDropVisualMode.Copy
            : DragAndDropVisualMode.Rejected;
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        EnemySkillData skill = DragAndDrop.objectReferences.OfType<EnemySkillData>().FirstOrDefault();
        if (skill == null || !ownedSkills.Contains(skill))
            return;

        DragAndDrop.AcceptDrag();
        AddNode(EnemyBTGraphNodeType.SkillAction, contentViewContainer.WorldToLocal(evt.mousePosition), skill);
    }

}

internal sealed class EnemyBTGraphNodeView : Node
{
    public EnemyBTGraphNodeData Data { get; }
    public Port Input { get; }
    public Port Output { get; }

    public EnemyBTGraphNodeView(EnemyBTGraphNodeData data, Action<string> onSelected)
    {
        Data = data;
        title = GetTitle();
        capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;

        Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        Input.portName = "Parent";
        inputContainer.Add(Input);

        if (data.type == EnemyBTGraphNodeType.Sequence || data.type == EnemyBTGraphNodeType.Selector)
        {
            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            Output.portName = "Children";
            outputContainer.Add(Output);
        }

        RefreshPorts();
        RefreshExpandedState();
        RegisterCallback<MouseDownEvent>(_ => onSelected?.Invoke(data.id));
    }

    public void SetRoot(bool isRoot)
    {
        title = isRoot ? $"ROOT  |  {GetTitle()}" : GetTitle();
    }

    public void RefreshTitle()
    {
        title = GetTitle();
    }

    private string GetTitle()
    {
        return Data.type switch
        {
            EnemyBTGraphNodeType.Sequence => "Sequence",
            EnemyBTGraphNodeType.Selector => "Selector",
            EnemyBTGraphNodeType.SkillAction => Data.skillData != null ? $"Action: {Data.skillData.name}" : "Action: (Skill Missing)",
            EnemyBTGraphNodeType.TraceAction => "Action: Trace Player",
            EnemyBTGraphNodeType.IdleAction => "Action: Idle",
            _ => Data.type.ToString()
        };
    }
}
