using UnityEditor;
using UnityEngine;

public class DebugTool : EditorWindow
{
    [SerializeField] private RelicData testRelic;

    [MenuItem("Tools/Debug Tool")]
    private static void Open()
    {
        GetWindow<DebugTool>("Debug Tool");
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서 사용 가능", MessageType.Info);
            return;
        }

        testRelic = (RelicData)EditorGUILayout.ObjectField("테스트 유물", testRelic, typeof(RelicData), false);

        using (new EditorGUI.DisabledScope(testRelic == null))
        {
            if (GUILayout.Button("테스트 유물 추가"))
            {
                if (GameManager.Instance.Inventory.TryAdd(testRelic))
                    Utils.Log<DebugTool>("테스트 유물 추가 완료");
                else
                    Utils.Log<DebugTool>("테스트 유물 추가 실패");
            }
        }

        ServiceLocator.Inject(out RoomSystem roomSystem);

        if (roomSystem == null)
        {
            EditorGUILayout.HelpBox("RoomSystem을 찾지 못했습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("현재 방 적 수", $"{roomSystem.ActiveEnemyCount}");

        using (new EditorGUI.DisabledScope(roomSystem.ActiveEnemyCount == 0))
        {
            if (GUILayout.Button("현재 방의 모든 적 처치"))
            {
                roomSystem.DebugKillAllEnemies();
            }
        }

        Repaint();
    }
}
