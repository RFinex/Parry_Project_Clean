using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIPoolDataDataBase", menuName = "Pool/UIPoolDataBase")]
public class UIPoolDataBase : PoolDataBase
{
    [SerializeField] private List<UIPoolInfo> uiList;

    public IReadOnlyList<UIPoolInfo> UIList => uiList;

    public override void Register(ObjectPoolManager manager)
    {
        UIFactory factory = new UIFactory();

        foreach (UIPoolInfo info in uiList)
        {
            manager.AddPool(info.ID, info.Prefab, factory, manager.UIPoolParent);
        }
    }
}
