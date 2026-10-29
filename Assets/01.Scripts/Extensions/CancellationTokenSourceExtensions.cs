using System.Threading;

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

    public static System.Threading.CancellationTokenSource LinkToken(this System.Threading.CancellationTokenSource token, CancellationToken ownerToken)
    {
        token?.Cancel();
        token?.Dispose();
        return CancellationTokenSource.CreateLinkedTokenSource(ownerToken);
    }

    public static System.Threading.CancellationTokenSource LinkTokenOnly(this System.Threading.CancellationTokenSource token, CancellationToken ownerToken)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(ownerToken);
    }
}
