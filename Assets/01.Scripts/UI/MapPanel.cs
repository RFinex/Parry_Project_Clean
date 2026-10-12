using System;
using System.Collections.Generic;
using UnityEngine;

public class MapPanel : UIBase
{
    [SerializeField] private Transform nodeLayer;

    private readonly Dictionary<int, MapNode> nodeDic = new Dictionary<int, MapNode>();

    /// <summary>
    /// 스테이지 시작 후 노드 배치
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="onSelected"></param>
    public void SetMap(List<StageMapNodeData> nodes, Action<int> onSelected)
    {
        foreach (StageMapNodeData data in nodes)
        {
            MapNode node = UIManager.Instance.GetUI<MapNode>(UIName.Popup_Map_Node_Btn, Vector3.zero, nodeLayer);

            if (node == null)
            {
                Utils.Log<MapPanel>($"MapNode 생성 실패 : {data.id}");
                continue;
            }

            node.Initialized(data, false, onSelected);

            nodeDic.Add(data.id, node);
        }        
    }

    /// <summary>
    /// 현재 노드와 연결된 다음 노드만 클릭
    /// </summary>
    /// <param name="nextNodeIds"></param>
    public void ShowNextNodes(List<int> nextNodeIds)
    {
        foreach (var pair in nodeDic)
        {
            pair.Value.SetInteractable(nextNodeIds.Contains(pair.Key));
        }

        gameObject.SetActive(true);
    }

    public void HideMap()
    {
        gameObject.SetActive(false);
    }
}
