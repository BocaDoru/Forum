using System.Runtime.CompilerServices;

namespace database;

public class AsyncLazy<T>(Func<Task<T>> factory)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Func<Task<T>> _factory = factory;
    private T? _value;

    public async Task<T> Get()
    {
        if (_value is not null)
            return _value;

        return await Generate();
    }

    public TaskAwaiter<T> GetAwaiter() => Get().GetAwaiter();

    private async Task<T> Generate()
    {
        await _semaphore.WaitAsync();
        if(_value is not null)
        {
            _semaphore.Release();
            return _value;
        }

        try
        {
            _value = await _factory();
        }
        finally { 
            _semaphore.Release(); 
        }

        return _value;
    }

}
