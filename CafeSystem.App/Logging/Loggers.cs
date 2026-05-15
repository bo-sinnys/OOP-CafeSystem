using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Logging;

/// <summary>Консольний логер — конкретна реалізація ILogger (DIP).</summary>
public class ConsoleLogger : ILogger
{
    private readonly string _prefix;

    public ConsoleLogger(string prefix = "CAFE")
        => _prefix = prefix;

    public void Log(string message) =>
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][{_prefix}] {message}");

    public void LogError(string message, Exception? ex = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][ERROR] {message}");
        if (ex != null) Console.WriteLine($"  Exception: {ex.Message}");
        Console.ResetColor();
    }

    public void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][WARN] {message}");
        Console.ResetColor();
    }
}

/// <summary>Логер у файл — альтернативна реалізація (підміна компонента, ПЗ 4).</summary>
public class FileLogger : ILogger, IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public FileLogger(string path)
        => _writer = new StreamWriter(path, append: true);

    public void Log(string message) =>
        _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][INFO] {message}");

    public void LogError(string message, Exception? ex = null)
    {
        _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][ERROR] {message}");
        if (ex != null) _writer.WriteLine($"  Exception: {ex.Message}");
        _writer.Flush();
    }

    public void LogWarning(string message) =>
        _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][WARN] {message}");

    public void Dispose()
    {
        if (_disposed) return;
        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Складений логер — делегує декільком реалізаціям одночасно.
/// Демонструє Decorator-like композицію (ПЗ 12).
/// </summary>
public class CompositeLogger : ILogger
{
    private readonly IReadOnlyList<ILogger> _loggers;

    public CompositeLogger(params ILogger[] loggers)
        => _loggers = loggers;

    public void Log(string message)        => _loggers.ForEach(l => l.Log(message));
    public void LogError(string msg, Exception? ex) => _loggers.ForEach(l => l.LogError(msg, ex));
    public void LogWarning(string message) => _loggers.ForEach(l => l.LogWarning(message));
}

// Extension для ForEach на IReadOnlyList
file static class Extensions
{
    public static void ForEach<T>(this IReadOnlyList<T> list, Action<T> action)
    {
        foreach (var item in list) action(item);
    }
}
