using System;
using UnityEngine;

public abstract class DataReaderBase : ScriptableObject
{
    [Header("Google Spreadsheet ID")]
    [Tooltip("URL의 /d/ 와 /edit 사이 값")]
    [SerializeField] private string sheetId;
    public string SheetId => sheetId;
}

[Serializable]
public class SheetReadSettings
{
    public string sheetName;

    [Min(2)]
    public int startRow = 2;

    [Min(2)]
    public int endRow = 2;
}