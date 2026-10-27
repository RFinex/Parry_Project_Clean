using GoogleSheetsToUnity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ArtifactDataReader))]
public class ArtifactDataReaderTool : Editor
{
    private bool isReading;

    private sealed class ArtifactRow
    {
        public int itemId;
        public string itemName;
        public string description;
        public ItemRarity rarity;
        public string iconId;
        public Sprite icon;

        public readonly List<StatPair> stats = new();
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(isReading))
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("아티팩트 불러오기"))
                ReadSheets();
        }

        if (isReading)
        {
            EditorGUILayout.HelpBox("Gool Sheets를 읽는 중입니다.", MessageType.Info);
        }
    }

    private void ReadSheets()
    {
        ArtifactDataReader reader = (ArtifactDataReader)target;

        try
        {
            ValidateSettings(reader);

            isReading = true;

            // 첫 번째 표를 받은 후 두 번째 표를 읽는다.
            SpreadsheetManager.Read(new GSTU_Search(reader.SheetId.Trim(), reader.ItemInfo.sheetName), infoSheet =>
                {
                    try
                    {
                        if (reader == null)
                        {
                            FinishReading();
                            return;
                        }

                        Dictionary<int, ArtifactRow> rows = ReadItemInfo(reader, infoSheet);

                        SpreadsheetManager.Read(new GSTU_Search(reader.SheetId.Trim(), reader.ItemStats.sheetName), statsSheet =>
                            {
                                try
                                {
                                    if (reader == null)
                                        return;

                                    if (!ReadItemStats(reader, statsSheet, rows))
                                        return;

                                    // 두 표의 파싱이 성공한 뒤 에셋 반영
                                    SaveArtifacts(reader, rows);

                                    Debug.Log($"Artifact 임포트 완료: {rows.Count}개", reader);
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogException(exception);
                                }
                                finally
                                {
                                    FinishReading();
                                }
                            },
                            false);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                        FinishReading();
                    }
                },
                false);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            FinishReading();
        }
    }

    private void FinishReading()
    {
        isReading = false;

        if (this != null)
            Repaint();
    }

    private static void ValidateSettings(ArtifactDataReader reader)
    {
        if (string.IsNullOrWhiteSpace(reader.SheetId))
            throw new Exception("Google Sheet 문서 ID를 입력하세요.");

        ValidateRange(reader.ItemInfo);
        ValidateRange(reader.ItemStats);

        string folder = reader.SaveFolder;

        if (string.IsNullOrWhiteSpace(folder) || !folder.StartsWith("Assets/", StringComparison.Ordinal) || !AssetDatabase.IsValidFolder(folder))
        {
            throw new Exception("저장 폴더는 Assets 아래에 존재하는 폴더로 지정하세요.");
        }
    }

    private static void ValidateRange(SheetReadSettings settings)
    {
        if (settings == null || string.IsNullOrWhiteSpace(settings.sheetName))
        {
            throw new Exception("읽을 탭 이름을 입력하세요.");
        }

        if (settings.startRow < 2 || settings.endRow < settings.startRow)
        {
            throw new Exception($"{settings.sheetName}: 읽기 범위를 확인하세요. 1행은 헤더이며 데이터는 2행부터입니다.");
        }
    }

    private static Dictionary<int, ArtifactRow> ReadItemInfo(ArtifactDataReader reader, GstuSpreadSheet sheet)
    {
        if (sheet == null)
            throw new Exception("Item Info 시트 응답이 없습니다.");

        Dictionary<string, Sprite> icons = BuildIconMap(reader);
        Dictionary<int, ArtifactRow> result = new();

        SheetReadSettings settings = reader.ItemInfo;

        for (int row = settings.startRow; row <= settings.endRow; row++)
        {
            Dictionary<string, string> cells = ReadCells(sheet.rows[row]);

            if (IsEmptyRow(cells))
                continue;

            string context = $"{settings.sheetName} {row}행";

            int itemId = ReadItemId(cells, context);

            if (result.ContainsKey(itemId))
                throw new Exception($"{context}: ItemId {itemId} 중복");

            string category = Required(cells, "Category", context).Trim();

            if (category != "Artifact")
            {
                throw new Exception($"{context}: 이 도구는 Artifact만 처리합니다. 현재 Category: {category}");
            }

            string rarityText = Required(cells, "Rarity", context).Trim();

            if (!Enum.TryParse(rarityText, out ItemRarity rarity) || rarity.ToString() != rarityText)
            {
                throw new Exception($"{context}: 잘못된 Rarity '{rarityText}'");
            }

            string iconId = Required(cells, "IconId", context).Trim();

            if (!icons.TryGetValue(iconId, out Sprite icon))
            {
                throw new Exception($"{context}: Icon References에 '{iconId}'를 등록하세요.");
            }

            ArtifactRow item = new()
            {
                itemId = itemId,
                itemName = Required(cells, "Name", context),
                description = Required(cells, "Description", context),
                rarity = rarity,
                iconId = iconId,
                icon = icon
            };

            result.Add(itemId, item);
        }

        if (result.Count == 0)
            throw new Exception("읽은 Artifact가 없습니다. 행 범위를 확인하세요.");

        return result;
    }

    private static bool ReadItemStats(ArtifactDataReader reader, GstuSpreadSheet sheet, Dictionary<int, ArtifactRow> items)
    {
        if (sheet == null)
        {
            Debug.LogWarning("Item Stats Value 시트 응답이 없습니다.");
            return false;
        }

        SheetReadSettings settings = reader.ItemStats;

        for (int row = settings.startRow; row <= settings.endRow; row++)
        {
            Dictionary<string, string> cells = ReadCells(sheet.rows[row]);

            if (IsEmptyRow(cells))
                continue;

            string context = $"{settings.sheetName} {row}행";

            if (!cells.TryGetValue("ItemId", out string itemIdText) || !int.TryParse(itemIdText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId) || itemId <= 0)
            {
                Debug.LogWarning($"{context}: ItemId를 확인하세요.");
                return false;
            }

            if (!items.TryGetValue(itemId, out ArtifactRow item))
            {
                Debug.LogWarning($"{context}: Item Info에 ItemId {itemId}가 없습니다.");
                return false;
            }

            if (!cells.TryGetValue("StatId", out string statText))
            {
                Debug.LogWarning($"{context}: StatId 열이 없습니다.");
                return false;
            }

            statText = statText.Trim();

            if (!Enum.TryParse(statText, out StatId statId) || statId.ToString() != statText)
            {
                Debug.LogWarning($"{context}: 잘못된 StatId '{statText}'");
                return false;
            }

            // [추가] Flat / Percent 읽기
            if (!cells.TryGetValue("ModifierType", out string modifierText))
            {
                Debug.LogWarning($"{context}: ModifierType 열이 없습니다.");
                return false;
            }

            modifierText = modifierText.Trim();

            if (!Enum.TryParse(modifierText, out StatModifierType modifierType) || modifierType.ToString() != modifierText)
            {
                Debug.LogWarning($"{context}: ModifierType은 Flat 또는 Percent로 입력하세요.");
                return false;
            }

            if (!cells.TryGetValue("Value", out string valueText) || !float.TryParse(valueText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) || float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogWarning($"{context}: Value는 % 기호 없이 숫자로 입력하세요.");
                return false;
            }

            item.stats.Add(new StatPair
            {
                id = statId,
                modifierType = modifierType, // [추가]
                value = value
            });
        }

        return true;
    }

    private static Dictionary<string, Sprite> BuildIconMap(ArtifactDataReader reader)
    {
        Dictionary<string, Sprite> result = new();

        foreach (ArtifactIconEntry entry in reader.Icons)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.iconId) || entry.sprite == null)
            {
                throw new Exception("Icon References의 IconId와 Sprite를 모두 지정하세요.");
            }

            string key = entry.iconId.Trim();

            if (result.ContainsKey(key))
                throw new Exception($"중복된 IconId: {key}");

            result.Add(key, entry.sprite);
        }

        return result;
    }

    private static Dictionary<string, string> ReadCells(List<GSTU_Cell> cells)
    {
        if (cells == null)
            throw new Exception("읽기 범위에 존재하지 않는 행이 있습니다.");

        Dictionary<string, string> result = new();

        foreach (GSTU_Cell cell in cells)
        {
            string key = cell.columnId;

            if (string.IsNullOrWhiteSpace(key))
                continue;

            key = key.Trim();

            if (result.ContainsKey(key))
                throw new Exception($"중복된 열 이름: {key}");

            result.Add(key, cell.value ?? string.Empty);
        }

        return result;
    }

    private static bool IsEmptyRow(Dictionary<string, string> cells)
    {
        foreach (string value in cells.Values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return false;
        }

        return true;
    }

    private static string Required(Dictionary<string, string> cells, string column, string context)
    {
        if (!cells.TryGetValue(column, out string value) || string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"{context}: '{column}' 값이 없습니다.");
        }

        return value;
    }

    private static int ReadItemId(Dictionary<string, string> cells, string context)
    {
        string text = Required(cells, "ItemId", context).Trim();

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId) || itemId <= 0)
        {
            throw new Exception($"{context}: 잘못된 ItemId '{text}'");
        }

        return itemId;
    }

    private static Dictionary<int, ArtifactData> FindExistingArtifacts()
    {
        Dictionary<int, ArtifactData> result = new();

        string[] guids = AssetDatabase.FindAssets("t:ArtifactData", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            ArtifactData artifact = AssetDatabase.LoadAssetAtPath<ArtifactData>(path);

            if (artifact == null)
                continue;

            if (result.ContainsKey(artifact.Id))
            {
                throw new Exception($"기존 ArtifactData의 ID가 중복됩니다: {artifact.Id}");
            }

            result.Add(artifact.Id, artifact);
        }

        return result;
    }

    private static void SaveArtifacts(ArtifactDataReader reader, Dictionary<int, ArtifactRow> rows)
    {
        Dictionary<int, ArtifactData> existing = FindExistingArtifacts();

        // 에셋을 변경하기 전에 새 저장 경로 충돌 확인
        foreach (ArtifactRow row in rows.Values)
        {
            if (existing.ContainsKey(row.itemId))
                continue;

            string path = $"{reader.SaveFolder}/Artifact_{row.itemId}.asset";

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                throw new Exception($"저장 경로에 다른 에셋이 있습니다: {path}");
        }

        List<ArtifactData> imported = new();

        foreach (ArtifactRow row in rows.Values)
        {
            if (!existing.TryGetValue(row.itemId, out ArtifactData artifact))
            {
                artifact = ScriptableObject.CreateInstance<ArtifactData>();

                string path = $"{reader.SaveFolder}/Artifact_{row.itemId}.asset";

                AssetDatabase.CreateAsset(artifact, path);
            }

            WriteArtifact(artifact, row);
            imported.Add(artifact);
        }

        // 설정 에셋에는 이번에 읽은 결과 참조를 저장
        SerializedObject readerObject = new(reader);
        SerializedProperty list = readerObject.FindProperty("artifacts");

        list.arraySize = imported.Count;

        for (int i = 0; i < imported.Count; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = imported[i];

        readerObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(reader);
        AssetDatabase.SaveAssets();
    }

    private static void WriteArtifact(ArtifactData artifact, ArtifactRow row)
    {
        Undo.RecordObject(artifact, "Import Artifact");

        SerializedObject serialized = new(artifact);

        serialized.FindProperty("id").intValue = row.itemId;
        serialized.FindProperty("itemName").stringValue = row.itemName;
        serialized.FindProperty("description").stringValue = row.description;
        serialized.FindProperty("rarity").intValue = (int)row.rarity;
        serialized.FindProperty("iconId").stringValue = row.iconId;
        serialized.FindProperty("icon").objectReferenceValue = row.icon;

        SerializedProperty stats = serialized.FindProperty("statPairs");

        // 기존 내용을 교체하므로 재임포트해도 스탯이 중복되지 않는다.
        stats.arraySize = row.stats.Count;

        for (int i = 0; i < row.stats.Count; i++)
        {
            SerializedProperty pair = stats.GetArrayElementAtIndex(i);
            StatPair source = row.stats[i];

            pair.FindPropertyRelative("id").intValue = (int)source.id;
            pair.FindPropertyRelative("modifierType").intValue = (int)source.modifierType;
            pair.FindPropertyRelative("value").floatValue = source.value;
        }

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(artifact);
    }
}
