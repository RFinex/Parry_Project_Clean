using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageMapNodeData
{
    public int id;
    public RoomType roomType;
    public Vector2 mapPosition;
    public Sprite icon;
    public List<int> nextNodeIds = new List<int>();
    public int floorIndex;

    [NonSerialized] public GameObject roomPrefab;
}
