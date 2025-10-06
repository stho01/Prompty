namespace Promty;

public abstract class Command<TArgs> where TArgs : new()
{
    public abstract Task<int> ExecuteAsync(TArgs args);
}
