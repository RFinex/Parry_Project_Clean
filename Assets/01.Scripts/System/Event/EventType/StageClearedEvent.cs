public struct StageClearedEvent
{
    public readonly bool isFinalStage;

    public StageClearedEvent(bool isFinalStage)
    {
        this.isFinalStage = isFinalStage;
    }
}
