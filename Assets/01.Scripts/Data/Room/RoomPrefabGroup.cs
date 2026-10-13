using System;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Normal,
    Elite,
    Treasure,
    Shop,
    Boss
}

[Serializable]
public class RoomPrefabGroup
{
    public RoomType roomType;
    public Sprite mapIcon;
    public List<GameObject> prefabs = new List<GameObject>();
}
