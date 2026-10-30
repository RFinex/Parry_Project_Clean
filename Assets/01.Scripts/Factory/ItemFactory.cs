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
        Register(1000006, () => new SignetOfRetaliation());
        Register(1000007, () => new Artifact());
        Register(1000008, () => new Artifact());
        Register(1000009, () => new Artifact());
        Register(1000010, () => new UnknownBoneShard());
        Register(1000011, () => new TalonOfExecutioner());
        Register(1000012, () => new Artifact());
        Register(1000013, () => new NecklaceOfCombo());
        Register(1000014, () => new Artifact());
        Register(1000015, () => new SilentDagger());
        Register(1000016, () => new Artifact());
        Register(1000017, () => new RingOfChaser());
        Register(1000018, () => new NecklaceOfAfterWind());
        Register(1000019, () => new EmberHeart());
        Register(1000020, () => new HuntersInsignia());
        Register(1000021, () => new BerserkersShield());
        Register(1000022, () => new Artifact());
        Register(1000023, () => new Artifact());
        Register(1000024, () => new GemOfSprint());
        Register(1000025, () => new EverburningEmber());
        Register(1000026, () => new ChainOfExecutioner());
        Register(1000027, () => new BerserkersSignet());
        Register(1000028, () => new SealOfJudgement());
        Register(1000029, () => new BlazefurySword());
        Register(1000030, () => new BloodveinCrystal());
        Register(1000031, () => new RegalEdge());
        Register(1000032, () => new SigilOfEquilibrium());
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
