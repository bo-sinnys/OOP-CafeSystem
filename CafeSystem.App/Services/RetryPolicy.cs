using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Services;

public class RetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _baseDelay;
    private readonly ILogger _logger;

    public RetryPolicy(ILogger logger, int maxAttempts = 3, TimeSpan? baseDelay = null)
    {
        _logger      = logger;
        _maxAttempts = maxAttempts;
        _baseDelay   = baseDelay ?? TimeSpan.FromMilliseconds(200);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName = "Operation")
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                _logger.Log($"[Retry] '{operationName}' — спроба {attempt}/{_maxAttempts}");
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning($"[Retry] '{operationName}' не вдалася (спроба {attempt}): {ex.Message}");

                if (attempt < _maxAttempts)
                {
                    var delay = TimeSpan.FromMilliseconds(
                        _baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    _logger.Log($"[Retry] Очікування {delay.TotalMilliseconds:F0} мс перед наступною спробою...");
                    await Task.Delay(delay);
                }
            }
        }

        _logger.LogError($"[Retry] '{operationName}' вичерпала {_maxAttempts} спроби.", lastException);
        throw new AggregateException($"'{operationName}' не вдалася після {_maxAttempts} спроб.", lastException!);
    }

    public T Execute<T>(Func<T> operation, string operationName = "Operation")
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                _logger.Log($"[Retry] '{operationName}' — спроба {attempt}/{_maxAttempts}");
                return operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning($"[Retry] '{operationName}' спроба {attempt} не вдалася: {ex.Message}");

                if (attempt < _maxAttempts)
                    Thread.Sleep((int)(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)));
            }
        }

        _logger.LogError($"[Retry] '{operationName}' вичерпала {_maxAttempts} спроби.", lastException);
        throw new AggregateException($"'{operationName}' не вдалася після {_maxAttempts} спроб.", lastException!);
    }
}