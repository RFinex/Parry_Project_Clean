using UnityEditor;
using UnityEngine;

public class DebugTool : EditorWindow
{
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
