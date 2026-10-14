using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapPanel : PopupBase
{
    [Header("Tween")]
    [SerializeField] private RectTransform rect;

    [SerializeField] private float startY = 700f;
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float openDuration = 0.45f;
    [SerializeField] private float foldScaleX = 0.05f;

    [SerializeField] private Transform connectionLayer;
    [SerializeField] private Transform nodeLayer;

    private readonly Dictionary<int, MapNode> nodeDic = new Dictionary<int, MapNode>();

    private Sequence seq;

    protected override void Initialized()
    {
        if (rect == null)
            return;

        seq?.Kill();

        rect.anchoredPosition = new Vector2(0f, startY);
        rect.localScale = new Vector3(foldScaleX, 1f, 1f);
    }

    protected override void PlayOpenTween()
    {
        if (rect == null)
            return;

        seq?.Kill();

        seq = DOTween.Sequence();

        seq.Append(rect.DOAnchorPos(Vector2.zero, dropDuration).SetEase(Ease.OutCubic))
            .Append(rect.DOScaleX(1f, openDuration).SetEase(Ease.OutCubic));
    }

    private void OnDisable()
    {
        seq?.Kill();
        seq = null;
    }

    /// <summary>
    /// 스테이지 시작 후 노드 배치
    /// </summary>
    /// <param name="nodes"></param>
    public void SetMap(List<StageMapNodeData> nodes)
    {
        // 노드 생성
        foreach (StageMapNodeData data in nodes)
        {
            MapNode node = UIManager.Instance.GetUI<MapNode>(UIName.Popup_Map_Node_Btn, Vector3.zero, nodeLayer);

            if (node == null)
            {
                Utils.Log<MapPanel>($"MapNode 생성 실패 : {data.id}");
                continue;
            }

            node.Initialized(data, false);

            nodeDic.Add(data.id, node);
        }

        // 노드 연결선 생성
        foreach (StageMapNodeData data in nodes)
        {
            if (!nodeDic.TryGetValue(data.id, out MapNode startNode))
                continue;

            foreach (int nextNodeId in data.nextNodeIds)
            {
                if (!nodeDic.TryGetValue(nextNodeId, out MapNode endNode))
                {
                    Utils.Log<MapPanel>($"연결 대상 노드를 찾을 수 없음 : {nextNodeId}");
                    continue;
                }

                MapConnection line = UIManager.Instance.GetUI<MapConnection>(UIName.Popup_Map_Connection, Vector3.zero, connectionLayer);

                if (line == null)
                {
                    Utils.Log<MapPanel>("MapLine 생성 실패");
                    continue;
                }

                line.Initialized(startNode.MapPosition, endNode.MapPosition);
            }
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
    }
}
