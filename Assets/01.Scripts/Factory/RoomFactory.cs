using System.Collections.Generic;
using UnityEngine;

public class RoomFactory
{
    private readonly Dictionary<RoomType, List<GameObject>> roomPrefabDic = new Dictionary<RoomType, List<GameObject>>();

    public void Register(RoomPrefabGroup group)
    {
        if (group == null || group.prefabs == null)
        {
            Utils.Log<RoomFactory>("비어있거나 잘못된 RoomPrefabGroup");
            return;
        }

        if (!roomPrefabDic.TryGetValue(group.roomType, out List<GameObject> prefabs))
        {
            prefabs = new List<GameObject>();
            roomPrefabDic.Add(group.roomType, prefabs);
        }

        foreach (GameObject prefab in group.prefabs)
        {
            if (prefab == null || prefabs.Contains(prefab))
                continue;

            prefabs.Add(prefab);
        }
    }

    public GameObject Create(RoomType roomType)
    {
        if (!roomPrefabDic.TryGetValue(roomType, out List<GameObject> prefabs) || prefabs.Count == 0)
        {
            Utils.LogError<RoomFactory>($"{roomType} 방 프리팹 목록이 비어있음");
            return null;
        }

        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
        return Object.Instantiate(prefab);
    }
}
