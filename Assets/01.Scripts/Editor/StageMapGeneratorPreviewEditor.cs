using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageManager))]
public class StageMapGeneratorPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8f);

        if (GUILayout.Button("맵 프리뷰 창 열기"))
            StageMapGeneratorPreviewWindow.Open((StageManager)target);
    }
}

public class StageMapGeneratorPreviewWindow : EditorWindow
{
    private const float NodeRadius = 13f;
    private const float CanvasPadding = 36f;

    private static readonly FieldInfo MapGeneratorField = typeof(StageManager).GetField(
        "mapGenerator",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static GUIStyle nodeLabelStyle;

    [SerializeField] private StageManager stageManager;

    private List<StageMapNodeData> previewNodes;
    private string previewSettings;
    private int previewSeed;

    [MenuItem("Tools/Parry/Stage Map Preview")]
    private static void OpenFromMenu()
    {
        Open(Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<StageManager>()
            : null);
    }

    public static void Open(StageManager manager)
    {
        StageMapGeneratorPreviewWindow window = GetWindow<StageMapGeneratorPreviewWindow>("Stage Map Preview");
        window.stageManager = manager;
        window.previewSeed = unchecked((int)System.DateTime.UtcNow.Ticks) ^ (manager != null ? manager.GetInstanceID() : 0);
        window.previewSettings = null;
        window.minSize = new Vector2(500f, 420f);
        window.Show();
        window.Repaint();
    }

    private void OnEnable()
    {
        if (previewSeed == 0)
            previewSeed = unchecked((int)System.DateTime.UtcNow.Ticks);
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        stageManager = (StageManager)EditorGUILayout.ObjectField(
            "Stage Manager",
            stageManager,
            typeof(StageManager),
            true);

        if (EditorGUI.EndChangeCheck())
            previewSettings = null;

        using (new EditorGUI.DisabledScope(stageManager == null))
        {
            if (GUILayout.Button("지도 다시 생성"))
            {
                previewSeed++;
                previewSettings = null;
            }
        }

        if (stageManager == null)
        {
            EditorGUILayout.HelpBox(
                "StageManager 인스펙터의 '맵 프리뷰 창 열기'를 누르거나 위 슬롯에 StageManager를 지정하세요.",
                MessageType.Info);
            return;
        }

        StageMapGenerator generator = GetMapGenerator();

        if (generator == null)
        {
            EditorGUILayout.HelpBox("StageMapGenerator를 찾을 수 없습니다.", MessageType.Error);
            return;
        }

        string currentSettings = JsonUtility.ToJson(generator);

        if (previewNodes == null || currentSettings != previewSettings)
            GeneratePreview(currentSettings);

        if (previewNodes == null || previewNodes.Count == 0)
        {
            EditorGUILayout.HelpBox("표시할 맵 노드가 없습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"노드 {previewNodes.Count}개", EditorStyles.miniLabel);

        Rect previewRect = GUILayoutUtility.GetRect(
            100f,
            Mathf.Max(200f, position.height - 82f),
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        StageMapPreviewRenderer.Draw(previewRect, previewNodes, NodeRadius, CanvasPadding, GetNodeLabelStyle());
        EditorGUILayout.LabelField("N 일반   E 엘리트   T 보물   $ 상점   B 보스", EditorStyles.miniLabel);
    }

    private StageMapGenerator GetMapGenerator()
    {
        if (MapGeneratorField == null || stageManager == null)
            return null;

        return MapGeneratorField.GetValue(stageManager) as StageMapGenerator;
    }

    private void GeneratePreview(string settingsJson)
    {
        StageMapGenerator previewGenerator = JsonUtility.FromJson<StageMapGenerator>(settingsJson);

        if (previewGenerator == null)
        {
            previewNodes = null;
            return;
        }

        Random.State previousRandomState = Random.state;

        try
        {
            Random.InitState(previewSeed++);
            previewNodes = previewGenerator.Generate();
            previewSettings = settingsJson;
        }
        finally
        {
            Random.state = previousRandomState;
        }
    }

    private static GUIStyle GetNodeLabelStyle()
    {
        if (nodeLabelStyle == null)
        {
            nodeLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            nodeLabelStyle.normal.textColor = Color.white;
        }

        return nodeLabelStyle;
    }
}

internal static class StageMapPreviewRenderer
{
    public static void Draw(Rect rect, List<StageMapNodeData> nodes, float nodeRadius, float padding, GUIStyle labelStyle)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.13f, 0.15f));

        Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>(nodes.Count);
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (StageMapNodeData node in nodes)
        {
            minX = Mathf.Min(minX, node.mapPosition.x);
            maxX = Mathf.Max(maxX, node.mapPosition.x);
            minY = Mathf.Min(minY, node.mapPosition.y);
            maxY = Mathf.Max(maxY, node.mapPosition.y);
        }

        float contentWidth = Mathf.Max(1f, rect.width - padding * 2f);
        float contentHeight = Mathf.Max(1f, rect.height - padding * 2f);
        float xRange = Mathf.Max(1f, maxX - minX);
        float yRange = Mathf.Max(1f, maxY - minY);

        foreach (StageMapNodeData node in nodes)
        {
            float x = rect.x + padding + ((node.mapPosition.x - minX) / xRange) * contentWidth;
            float y = rect.yMax - padding - ((node.mapPosition.y - minY) / yRange) * contentHeight;
            positions[node.id] = new Vector2(x, y);
        }

        Handles.BeginGUI();

        Handles.color = new Color(0.63f, 0.61f, 0.53f, 0.8f);
        foreach (StageMapNodeData node in nodes)
        {
            if (!positions.TryGetValue(node.id, out Vector2 startPosition) || node.nextNodeIds == null)
                continue;

            foreach (int nextNodeId in node.nextNodeIds)
            {
                if (positions.TryGetValue(nextNodeId, out Vector2 endPosition))
                    Handles.DrawAAPolyLine(2f, startPosition, endPosition);
            }
        }

        foreach (StageMapNodeData node in nodes)
        {
            if (!positions.TryGetValue(node.id, out Vector2 position))
                continue;

            Handles.color = GetRoomColor(node.roomType);
            Handles.DrawSolidDisc(position, Vector3.forward, nodeRadius);

            Rect labelRect = new Rect(position.x - 18f, position.y - 8f, 36f, 16f);
            GUI.Label(labelRect, GetRoomLabel(node.roomType), labelStyle);
        }

        Handles.EndGUI();
    }

    private static Color GetRoomColor(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Elite => new Color(0.72f, 0.33f, 0.28f),
            RoomType.Treasure => new Color(0.91f, 0.72f, 0.25f),
            RoomType.Shop => new Color(0.29f, 0.66f, 0.48f),
            RoomType.Boss => new Color(0.67f, 0.26f, 0.33f),
            _ => new Color(0.57f, 0.60f, 0.64f)
        };
    }

    private static string GetRoomLabel(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Elite => "E",
            RoomType.Treasure => "T",
            RoomType.Shop => "$",
            RoomType.Boss => "B",
            _ => "N"
        };
    }
}
