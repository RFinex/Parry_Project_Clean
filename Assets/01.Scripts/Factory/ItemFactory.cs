using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory
{
    private readonly Dictionary<int, Func<Item>> creatorsDic = new();

    public void Initialized()
    {
        creatorsDic.Clear();

        Register(1000001, () => new Artifact());
        Register(1000002, () => new Artifact());
        Register(1000003, () => new Artifact());
        Register(1000004, () => new Artifact());
        Register(1000005, () => new Artifact());
        Register(1000006, () => new EmblemOfCounter());
    }

    private void Register(int itemId, Func<Item> creator)
    {
        if (creator == null)
        {
            Utils.Log<ItemFactory>($"\uc0dd\uc131\uc790 \ud568\uc218\uac00 null\uc785\ub2c8\ub2e4: {itemId}");
            return;
        }

        if (!creatorsDic.TryAdd(itemId, creator))
            Utils.LogWarning<ItemFactory>($"\uc0dd\uc131\uc790 ID \uc911\ubcf5 : {itemId}");
    }

    public Item Create(int itemId)
    {
        if (!creatorsDic.TryGetValue(itemId, out Func<Item> creator))
        {
            Utils.LogWarning<ItemFactory>($"\ub4f1\ub85d\ub418\uc9c0 \uc54a\uc740 \uc544\uc774\ud15c : {itemId}");
            return null;
        }

        return creator();
    }
}
