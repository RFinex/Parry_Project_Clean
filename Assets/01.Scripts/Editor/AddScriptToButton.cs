using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class AddScriptToButton : MonoBehaviour
{
    [MenuItem("Tools/선택한 프리팹에 버튼 스크립트 일괄 추가")]
    private static void AddScriptToSelectedPrefabButtons()
    {
        GameObject selected = Selection.activeObject as GameObject;

        if (selected == null || !PrefabUtility.IsPartOfPrefabAsset(selected))
        {
            Debug.LogWarning("Project 창에서 프리팹 에셋을 선택하세요.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

        try
        {
            Button[] buttons = prefabRoot.GetComponentsInChildren<Button>(true);
            int addedCount = 0;

            foreach (Button button in buttons)
            {
                if (button.GetComponent<ButtonEffector>() != null)
                    continue;

                Undo.AddComponent<ButtonEffector>(button.gameObject);
                addedCount++;
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Debug.Log($"{buttons.Length}개 버튼 중 {addedCount}개에 ButtonEffector를 추가했습니다: {path}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
