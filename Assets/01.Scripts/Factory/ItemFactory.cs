using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory
{
    private readonly Dictionary<int, Func<Item>> creatorsDic = new();

    public void Register(int itemId, Func<Item> creator)
    {
        if (creator == null)
        {
            Utils.Log<ItemFactory>($"아이템 생성 함수가 없습니다: {itemId}");
            return;
        }

        if (!creatorsDic.TryAdd(itemId, creator))
            Utils.LogWarning<ItemFactory>($"아이템 ID 중복 : {itemId}");
    }

    public Item Create(int itemId)
    {
        if (!creatorsDic.TryGetValue(itemId, out Func<Item> creator))
        {
            Utils.LogWarning<ItemFactory>($"등록되지 않은 아이템 : {itemId}");
            return null;
        }

        return creator();
    }
}
