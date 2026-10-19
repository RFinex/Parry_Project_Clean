public static class CancellationTokenSourceExtensions
{
    public static System.Threading.CancellationTokenSource ResetToken(this System.Threading.CancellationTokenSource token)
    {
        token?.Cancel();
        token?.Dispose();
        return new System.Threading.CancellationTokenSource();
    }

    public static System.Threading.CancellationTokenSource Cancel_DisposeToken(this System.Threading.CancellationTokenSource token)
    {
        token?.Cancel();
        token?.Dispose();
        return null;
    }
}
