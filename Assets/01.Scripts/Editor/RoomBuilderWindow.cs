using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomBuilderWindow : EditorWindow
{
    private const string RoomFolder = "Assets/02.Prefab/Room";
    private static readonly Vector3 PreviewOffset = Vector3.right * 1000f;
    private static readonly System.Random SeedRandom = new();

    [SerializeField] private int stage = 2;
    [SerializeField] private RoomType roomType;
    [SerializeField] private GameObject template;
    [SerializeField] private string roomName = "Room_Stage2_Combat06";
    [SerializeField] private bool randomizeSeed = true;
    [SerializeField] private int seed = 1;

    private Scene previewScene;
    private GameObject preview;
    private string previewFolder;
    private RoomType previewType;
    private int previewStage;
    private string variationSummary;
    private string generationIssue;

    [MenuItem("Tools/Room Builder")]
    public static void ShowWindow()
    {
        GetWindow<RoomBuilderWindow>("Room Builder");
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
        ClearPreview();
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;
    }

    private void HandlePlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingEditMode)
            ClearPreview();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        stage = EditorGUILayout.IntSlider("Stage", stage, 1, 2);
        roomType = (RoomType)EditorGUILayout.EnumPopup("Room Type", roomType);
        if (EditorGUI.EndChangeCheck())
        {
            template = null;
            string category = roomType == RoomType.Normal ? "Combat" : roomType.ToString();
            roomName = $"Room_Stage{stage}_{category}_New";
            ClearPreview();
        }

        template = (GameObject)EditorGUILayout.ObjectField("Template (Optional)", template, typeof(GameObject), false);
        roomName = EditorGUILayout.TextField("Prefab Name", roomName);
        // [수정] 자동 생성에 사용한 시드를 표시하고 수동 재현도 지원합니다.
        randomizeSeed = EditorGUILayout.Toggle("Randomize Seed", randomizeSeed);
        using (new EditorGUI.DisabledScope(randomizeSeed))
            seed = EditorGUILayout.IntField("Seed", seed);

        if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            EditorGUILayout.HelpBox("Use Room Builder in Edit Mode, outside Prefab Mode.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Generate Preview"))
            GeneratePreview();

        if (!string.IsNullOrEmpty(generationIssue))
            EditorGUILayout.HelpBox(generationIssue, MessageType.Warning);

        if (preview == null)
            return;

        EditorGUILayout.HelpBox(variationSummary, MessageType.Info);
        if (GUILayout.Button("Select / Frame Preview"))
            FramePreview();

        if (GUILayout.Button("Validate / Save Prefab"))
            SavePreview();

        if (GUILayout.Button("Discard Preview"))
            ClearPreview();
    }

    private void GeneratePreview()
    {
        generationIssue = null;
        string folder = GetRoomFolder(roomType);
        string[] candidates = AssetDatabase.FindAssets("t:Prefab", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => System.IO.Path.GetFileNameWithoutExtension(path)
                .StartsWith($"Room_Stage{stage}_", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        int generationSeed = seed;
        if (randomizeSeed)
        {
            generationSeed = SeedRandom.Next();
            if (generationSeed == seed)
                generationSeed = generationSeed == int.MaxValue - 1 ? 0 : generationSeed + 1;
        }

        var random = new System.Random(generationSeed);
        string templatePath = template != null ? AssetDatabase.GetAssetPath(template) : null;
        if (template == null && candidates.Length > 0)
            templatePath = candidates[random.Next(candidates.Length)];

        if (!candidates.Contains(templatePath))
        {
            EditorUtility.DisplayDialog("Room Builder", "Choose an existing room prefab for this stage and room type.", "OK");
            return;
        }

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
        string issue = ValidateRoom(source, roomType, stage);
        if (issue != null)
        {
            EditorUtility.DisplayDialog("Invalid Template", issue, "OK");
            return;
        }

        ClearPreview();
        Scene previousScene = SceneManager.GetActiveScene();
        try
        {
            previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            preview = (GameObject)PrefabUtility.InstantiatePrefab(source, previewScene);
            PrefabUtility.UnpackPrefabInstance(preview, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            preview.hideFlags = HideFlags.DontSaveInEditor;

            string layoutSummary = "Type-specific template layout";
            if (roomType == RoomType.Normal)
            {
                // [수정] 타일 좌표와 Grid 연결은 별도의 임시 씬에서 인스턴스화한 뒤 읽습니다.
                Scene referenceScene = EditorSceneManager.NewPreviewScene();
                bool generated;
                try
                {
                    GameObject[] references = candidates.Select(path =>
                        (GameObject)PrefabUtility.InstantiatePrefab(
                            AssetDatabase.LoadAssetAtPath<GameObject>(path), referenceScene)).ToArray();
                    generated = RoomTileLayout.TryGenerate(preview, references, random, out layoutSummary);
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(referenceScene);
                }

                if (!generated)
                {
                    generationIssue = $"{source.name} | Seed: {generationSeed} | {layoutSummary}";
                    seed = generationSeed;
                    ClearPreview();
                    return;
                }
            }

            // [수정] 현재 씬의 지형과 겹치지 않는 위치에서 미리 봅니다.
            preview.transform.position += PreviewOffset;
            preview.name = "Room Builder Preview";
            previewFolder = folder;
            previewType = roomType;
            previewStage = stage;
            seed = generationSeed;
            variationSummary = $"{source.name} | Seed: {generationSeed} | {layoutSummary}";
            FramePreview();
        }
        catch (Exception exception)
        {
            ClearPreview();
            Debug.LogException(exception);
        }
        finally
        {
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }
    }

    private static string ValidateRoom(GameObject room, RoomType type, int stageNumber)
    {
        if (room == null)
            return "Room prefab could not be loaded.";

        var issues = new List<string>();
        if (room.GetComponentsInChildren<PlayerSpawnPoint>(true).Length != 1)
            issues.Add("The room needs exactly one PlayerSpawnPoint.");

        PolygonCollider2D bounds = room.GetComponentInChildren<PolygonCollider2D>(true);
        if (bounds == null || bounds.name != "CameraBounds")
            issues.Add("The first PolygonCollider2D must be CameraBounds because RoomSystem uses it.");

        if ((type == RoomType.Normal || type == RoomType.Elite || type == RoomType.Boss) &&
            room.GetComponentInChildren<EnemySpawnPoint>(true) == null)
            issues.Add("Missing EnemySpawnPoint.");

        if (type != RoomType.Boss && room.GetComponentInChildren<RoomExitObject>(true) == null)
            issues.Add("Missing room exit.");

        if (type == RoomType.Elite)
            ValidateReferences(room.GetComponentInChildren<EliteRoomEncounter>(true), issues, "introTrigger");
        if (type == RoomType.Boss)
        {
            BossRoomEncounter boss = room.GetComponentInChildren<BossRoomEncounter>(true);
            ValidateReferences(boss, issues, "introTrigger", "bossJumpPoint_1", "bossJumpPoint_2");
            if (boss != null && stageNumber == 2)
                ValidateReferences(boss, issues, "roomCenter");
        }
        if (type == RoomType.Treasure)
            ValidateReferences(room.GetComponentInChildren<TreasureRoomSystem>(true), issues, "chestSpawnPoint");

        return issues.Count == 0 ? null : string.Join("\n", issues);
    }

    private static void ValidateReferences<T>(T component, List<string> issues, params string[] properties)
        where T : Component
    {
        if (component == null)
        {
            issues.Add($"Missing {typeof(T).Name}.");
            return;
        }

        var serialized = new SerializedObject(component);
        foreach (string property in properties)
        {
            if (serialized.FindProperty(property)?.objectReferenceValue == null)
                issues.Add($"{typeof(T).Name}: missing {property}.");
        }
    }

    private void SavePreview()
    {
        string issue = ValidateRoom(preview, previewType, previewStage);
        if (issue != null)
        {
            EditorUtility.DisplayDialog("Invalid Room", issue, "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Room Prefab", roomName, "prefab", "Choose a new prefab file.", previewFolder);
        if (string.IsNullOrEmpty(path))
            return;

        if (System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog("Room Builder", "Choose a new filename to preserve the existing prefab.", "OK");
            return;
        }

        preview.name = System.IO.Path.GetFileNameWithoutExtension(path);
        preview.hideFlags = HideFlags.None;
        preview.transform.position -= PreviewOffset;
        GameObject saved;
        try
        {
            saved = PrefabUtility.SaveAsPrefabAsset(preview, path);
        }
        finally
        {
            preview.transform.position += PreviewOffset;
            preview.hideFlags = HideFlags.DontSaveInEditor;
            preview.name = "Room Builder Preview";
        }

        if (saved == null)
            return;

        ClearPreview();
        Selection.activeObject = saved;
        EditorGUIUtility.PingObject(saved);
        AssetDatabase.OpenAsset(saved);
    }

    private void FramePreview()
    {
        Selection.activeGameObject = preview;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    private void ClearPreview()
    {
        if (previewScene.IsValid() && previewScene.isLoaded)
            EditorSceneManager.CloseScene(previewScene, true);

        previewScene = default;
        preview = null;
    }

    private static string GetRoomFolder(RoomType type)
    {
        return $"{RoomFolder}/{(type == RoomType.Normal ? "Battle" : type.ToString())}";
    }
}
