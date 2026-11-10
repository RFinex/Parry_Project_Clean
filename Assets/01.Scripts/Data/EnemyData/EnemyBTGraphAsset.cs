using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBTGraph", menuName = "Enemy/Behavior Tree Graph")]
public sealed class EnemyBTGraphAsset : ScriptableObject
{
    [SerializeField] private EnemyBTGraphData combat = new();
    [SerializeField] private EnemyBTGraphData phase1 = new();
    [SerializeField] private EnemyBTGraphData phase2 = new();

    public EnemyBTGraphData Combat => combat;
    public EnemyBTGraphData Phase1 => phase1;
    public EnemyBTGraphData Phase2 => phase2;
}

[Serializable]
public sealed class EnemyBTGraphData
{
    [SerializeField] private string rootNodeId;
    [SerializeField] private List<EnemyBTGraphNodeData> nodes = new();
    [SerializeField] private List<EnemyBTGraphEdgeData> edges = new();

    public string RootNodeId => rootNodeId;
    public List<EnemyBTGraphNodeData> Nodes => nodes;
    public List<EnemyBTGraphEdgeData> Edges => edges;

    public void SetRoot(string id)
    {
        rootNodeId = id;
    }
}

public enum EnemyBTGraphNodeType
{
    Sequence,
    Selector,
    SkillAction,
    TraceAction,
    IdleAction
}

[Serializable]
public sealed class EnemyBTGraphNodeData
{
    public string id;
    public EnemyBTGraphNodeType type;
    public Vector2 position;
    public EnemySkillData skillData;
}

[Serializable]
public sealed class EnemyBTGraphEdgeData
{
    public string parentId;
    public string childId;
}
