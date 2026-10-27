using UnityEditor;
using UnityEngine;

public class DebugTool : EditorWindow
{
    [SerializeField] private ArtifactDataReader artifactReader;
    [SerializeField] private int selectedArtifactIndex;

    [MenuItem("Tools/Debug Tool")]
    private static void Open()
    {
        GetWindow<DebugTool>("Debug Tool");
    }

    private void OnGUI()
    {
        DrawArtifactSection();

        EditorGUILayout.Space();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("인벤토리 추가와 적 처치는 플레이 모드에서 가능합니다.", MessageType.Info);
            return;
        }

        DrawRoomSection();
    }

    // 플레이 중 적 수 표시 갱신
    private void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying)
            Repaint();
    }

    // 아이템 선택
    private void DrawArtifactSection()
    {
        EditorGUILayout.LabelField("Artifact", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        artifactReader = (ArtifactDataReader)EditorGUILayout.ObjectField("Artifact Reader", artifactReader, typeof(ArtifactDataReader), false);

        if (EditorGUI.EndChangeCheck())
            selectedArtifactIndex = 0;

        if (artifactReader == null)
        {
            EditorGUILayout.HelpBox("시트를 읽어온 ArtifactDataReader 에셋을 지정하세요.", MessageType.Info);
            return;
        }

        var artifacts = artifactReader.Artifacts;

        if (artifacts == null || artifacts.Count == 0)
        {
            EditorGUILayout.HelpBox("Artifact 목록이 비어 있습니다. Reader에서 먼저 임포트하세요.", MessageType.Warning);
            return;
        }

        string[] options = new string[artifacts.Count];

        for (int i = 0; i < artifacts.Count; i++)
        {
            ArtifactData artifact = artifacts[i];

            options[i] = artifact != null ? $"{artifact.Id} | {artifact.ItemName}" : $"[{i}] 비어 있는 참조";
        }

        selectedArtifactIndex = Mathf.Clamp(selectedArtifactIndex, 0, artifacts.Count - 1);

        selectedArtifactIndex = EditorGUILayout.Popup( "Artifact 선택", selectedArtifactIndex, options);

        ArtifactData selected = artifacts[selectedArtifactIndex];

        if (selected == null)
        {
            EditorGUILayout.HelpBox( "선택한 Artifact의 참조가 없습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("등급", selected.Rarity.ToString());

        EditorGUILayout.LabelField("설명", selected.Description, EditorStyles.wordWrappedLabel);

        GameManager manager = EditorApplication.isPlaying ? GameManager.Instance : null;

        bool canAdd = manager != null && manager.Inventory != null && manager.ItemFactory != null;

        using (new EditorGUI.DisabledScope(!canAdd))
        {
            if (GUILayout.Button("선택한 Artifact 추가"))
                AddArtifact(manager, selected);
        }
    }

    // 인벤토리에 특정 아이템 추가
    private void AddArtifact(GameManager manager, ArtifactData selected)
    {
        if (manager.TryGiveArtifact(selected.Id))
            Utils.Log<DebugTool>($"Artifact 추가 완료: {selected.ItemName}");
        else
            Utils.LogWarning<DebugTool>($"Artifact 추가 실패: {selected.ItemName}");
    }

    // 방 안의 모든 적 처치
    private void DrawRoomSection()
    {
        EditorGUILayout.LabelField("현재 방", EditorStyles.boldLabel);

        ServiceLocator.Inject(out RoomSystem roomSystem);

        if (roomSystem == null)
        {
            EditorGUILayout.HelpBox("RoomSystem을 찾지 못했습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("현재 적 수", roomSystem.ActiveEnemyCount.ToString());

        using (new EditorGUI.DisabledScope(roomSystem.ActiveEnemyCount == 0))
        {
            if (GUILayout.Button("현재 방의 모든 적 처치"))
                roomSystem.DebugKillAllEnemies();
        }
    }
}
