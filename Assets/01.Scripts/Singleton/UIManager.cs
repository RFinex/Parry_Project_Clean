using UnityEngine;

public enum UIName
{
    None,
    Popup_Map = 101,
    Popup_Map_Node_Btn = 102,
    Popup_Map_Connection = 103,
    Enemy_Status_Bar = 104
}

public class UIManager : Singleton<UIManager>
{
    public T GetUI<T>(UIName name, Vector3 pos, Transform parent) where T : UIBase
    {
        if (name == UIName.None)
        {
            Utils.Log<UIManager>("None UIName은 생성할 수 없음");
            return null;
        }

        return ObjectPoolManager.Instance.Get<T>((int)name, pos, parent);
    }
}
