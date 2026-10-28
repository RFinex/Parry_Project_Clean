using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactDataReader", menuName = "Data/Import/Artifact Data Reader")]
public class ArtifactDataReader : DataReaderBase
{
    [Header("Item Info")]
    [SerializeField]
    private SheetReadSettings itemInfo = new()
    {
        sheetName = "Item Info",
        startRow = 2,
        endRow = 3
    };

    [Header("New Artifact Save Folder")]
    [SerializeField] private string saveFolder = "Assets/01.Scripts/Data/Item/Artifact";

    [Header("Icon References")]
    [SerializeField] private List<ArtifactIconEntry> icons = new();

    [Header("Imported Artifacts")]
    [SerializeField] private List<ArtifactData> artifacts = new();

    public SheetReadSettings ItemInfo => itemInfo;
    public string SaveFolder => saveFolder;

    public IReadOnlyList<ArtifactIconEntry> Icons => icons;
    public List<ArtifactData> Artifacts => artifacts;

    public bool TryGetData(int id, out ArtifactData data)
    {
        data = artifacts.Find((a) => a.Id == id);

        return data != null;
    }
}

[Serializable]
public class ArtifactIconEntry
{
    public string iconId;
    public Sprite sprite;
}