using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;
using TinyJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetImporter : EditorWindow
{
    private const string GoogleSheetIdKey = "GoogleSheetIdKey";
    private const string CsvSaveFolderKey = "CsvSaveFolderKey";

    private DefaultAsset csvFolder;
    private string csvFolderPath = string.Empty;

    private string googleSheetId = string.Empty;
    private Vector2 scrollPos;

    [MenuItem("Tool/Google Sheet Importer")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetImporter>();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        googleSheetId = EditorGUILayout.TextField("Google Sheet ID", googleSheetId);

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(GoogleSheetIdKey, googleSheetId);
        }

        EditorGUILayout.Space();

        DrawFolderField("CSV 파일을 저장할 폴더", ref csvFolder, ref csvFolderPath, CsvSaveFolderKey);

        // 해당 매개변수들이 비어있으면 이 코드 이후 그려지는 GUI 컨트롤의 상호 작용 여부를 켜거나 끔
        GUI.enabled = IsValid(googleSheetId, csvFolderPath);

        if (GUILayout.Button("Import Google Sheet"))
        {
            if (!string.IsNullOrEmpty(googleSheetId))
            {
                GetGoogleSheet().Forget();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private async UniTaskVoid GetGoogleSheet()
    {
        string accessToken = SpreadsheetManager.Config.gdr.access_token;

        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{googleSheetId}";

        string metadataJson = await DownloadTextAsync(url, accessToken);

        SheetsRootObject root = JSON.Load(metadataJson).Make<SheetsRootObject>();

        foreach (Sheet sheet in root.sheets)
        {
            int gid = sheet.properties.sheetId;

            string csvUrl = $"https://docs.google.com/spreadsheets/d/{googleSheetId}/export?format=csv&gid={gid}";

            string csv = await DownloadTextAsync(csvUrl, accessToken);

            //파일 경로 지정
            string filePath = $"{csvFolderPath}/{sheet.properties.title}Table.txt";
            File.WriteAllText(filePath, csv, Encoding.UTF8);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    ///  구글 시트의 정보를 받아오는 함수
    /// </summary>
    /// <param name="url"> 받아올 구글 시트 주소 </param>
    /// <returns></returns>
    private async UniTask<string> DownloadTextAsync(string url, string accessToken = null)
    {
        try
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            await request.SendWebRequest();

            return request.result == UnityWebRequest.Result.Success
                ? request.downloadHandler.text
                : string.Empty;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"구글 시트 데이터를 읽어오지 못했습니다: {e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 폴더를 드래그할 오브젝트 필드를 만들고 에디터 내부에 저장하는 메서드
    /// </summary>
    /// <param name="label"> 라벨 </param>
    /// <param name="folderAsset"> 선택된 폴더 </param>
    /// <param name="path"> 폴더 경로 </param>
    /// <param name="key"> EditorPrefs에 저장할 키 </param>
    private void DrawFolderField(string label, ref DefaultAsset folderAsset, ref string path, string key)
    {
        folderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
            label,
            folderAsset,
            typeof(DefaultAsset),
            false
            );

        if (folderAsset == null)
        {
            path = null;
            EditorPrefs.DeleteKey(key);
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(folderAsset);

        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            EditorGUILayout.HelpBox("폴더만 지정할 수 있습니다.", MessageType.Error);
            EditorPrefs.DeleteKey(key);
            return;
        }

        path = assetPath;
        EditorPrefs.SetString(key, assetPath);
    }

    /// <summary>
    /// 폴더 변수에 경로에 있는 폴더 연결
    /// </summary>
    /// <param name="asset"> 값을 넣어줄 폴더 변수 </param>
    /// <param name="path"> 폴더 경로 </param>
    private void RestoreFolderAsset(ref DefaultAsset asset, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
        }
    }

    /// <summary>
    /// 모든 경로에 값이 들어있는지 확인
    /// </summary>
    /// <returns> 모든 경로에 값이 들어있으면 true, 아니면 false </returns>
    private bool IsValid(params string[] path)
    {
        bool isValid = true;

        foreach (var p in path)
        {
            if (string.IsNullOrEmpty(p))
                isValid = false;
        }

        return isValid;
    }

    private void OnEnable()
    {
        minSize = new Vector2(300, 120);

        googleSheetId = EditorPrefs.GetString(GoogleSheetIdKey, string.Empty);
        csvFolderPath = EditorPrefs.GetString(CsvSaveFolderKey);

        RestoreFolderAsset(ref csvFolder, csvFolderPath);
    }
}