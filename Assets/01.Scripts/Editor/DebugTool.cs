using UnityEditor;
using UnityEngine;

public class DebugTool : EditorWindow
{
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
            EditorGUILayout.HelpBox("ÀÎº¥Åä¸® Ãß°¡¿Í Àû Ã³Ä¡´Â ÇÃ·¹ÀÌ ¸ğµå¿¡¼­ °¡´ÉÇÕ´Ï´Ù.", MessageType.Info);
            return;
        }

        DrawRoomSection();
    }

    // ÇÃ·¹ÀÌ Áß Àû ¼ö Ç¥½Ã °»½Å
    private void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying)
            Repaint();
    }

    // ¾ÆÀÌÅÛ ¼±ÅÃ
    private void DrawArtifactSection()
    {
        EditorGUILayout.LabelField("Artifact", EditorStyles.boldLabel);

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("í”Œë ˆì´ ëª¨ë“œì—ì„œ GameManagerê°€ ì´ˆê¸°í™”í•œ ìœ ë¬¼ ë°ì´í„°ë¥¼ ì‚¬ìš©í•  ìˆ˜ ìˆìŠµë‹ˆë‹¤.", MessageType.Info);
            return;
        }

        ServiceLocator.Inject(out ArtifactSystem artifactSystem);

        if (artifactSystem == null || artifactSystem.Artifacts == null)
        {
            EditorGUILayout.HelpBox("ArtifactSystem ë˜ëŠ” ìœ ë¬¼ ë°ì´í„°ê°€ ì´ˆê¸°í™”ë˜ì§€ ì•Šì•˜ìŠµë‹ˆë‹¤.", MessageType.Warning);
            return;
        }

        var artifacts = artifactSystem.Artifacts;

        if (artifacts == null || artifacts.Count == 0)
        {
            EditorGUILayout.HelpBox("Artifact ¸ñ·ÏÀÌ ºñ¾î ÀÖ½À´Ï´Ù. Reader¿¡¼­ ¸ÕÀú ÀÓÆ÷Æ®ÇÏ¼¼¿ä.", MessageType.Warning);
            return;
        }

        string[] options = new string[artifacts.Count];

        for (int i = 0; i < artifacts.Count; i++)
        {
            ArtifactData artifact = artifacts[i];

            options[i] = artifact != null ? $"{artifact.Id} | {artifact.ItemName}" : $"[{i}] ºñ¾î ÀÖ´Â ÂüÁ¶";
        }

        selectedArtifactIndex = Mathf.Clamp(selectedArtifactIndex, 0, artifacts.Count - 1);

        selectedArtifactIndex = EditorGUILayout.Popup( "Artifact ¼±ÅÃ", selectedArtifactIndex, options);

        ArtifactData selected = artifacts[selectedArtifactIndex];

        if (selected == null)
        {
            EditorGUILayout.HelpBox( "¼±ÅÃÇÑ ArtifactÀÇ ÂüÁ¶°¡ ¾ø½À´Ï´Ù.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("µî±Ş", selected.Rarity.ToString());

        EditorGUILayout.LabelField("¼³¸í", selected.Description, EditorStyles.wordWrappedLabel);

        GameManager manager = GameManager.Instance;

        bool canAdd = manager != null && manager.Inventory != null && manager.ItemFactory != null;

        using (new EditorGUI.DisabledScope(!canAdd))
        {
            if (GUILayout.Button("¼±ÅÃÇÑ Artifact Ãß°¡"))
                AddArtifact(manager, selected);
        }
    }

    // ÀÎº¥Åä¸®¿¡ Æ¯Á¤ ¾ÆÀÌÅÛ Ãß°¡
    private void AddArtifact(GameManager manager, ArtifactData selected)
    {
        if (manager.TryGiveArtifact(selected.Id))
            Utils.Log<DebugTool>($"Artifact Ãß°¡ ¿Ï·á: {selected.ItemName}");
        else
            Utils.LogWarning<DebugTool>($"Artifact Ãß°¡ ½ÇÆĞ: {selected.ItemName}");
    }

    // ¹æ ¾ÈÀÇ ¸ğµç Àû Ã³Ä¡
    private void DrawRoomSection()
    {
        EditorGUILayout.LabelField("ÇöÀç ¹æ", EditorStyles.boldLabel);

        ServiceLocator.Inject(out RoomSystem roomSystem);

        if (roomSystem == null)
        {
            EditorGUILayout.HelpBox("RoomSystemÀ» Ã£Áö ¸øÇß½À´Ï´Ù.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("ÇöÀç Àû ¼ö", roomSystem.ActiveEnemyCount.ToString());

        using (new EditorGUI.DisabledScope(roomSystem.ActiveEnemyCount == 0))
        {
            if (GUILayout.Button("ÇöÀç ¹æÀÇ ¸ğµç Àû Ã³Ä¡"))
                roomSystem.DebugKillAllEnemies();
        }
    }
}
