using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Interfaces;
using CafeSystem.App.Patterns.Strategy;

namespace CafeSystem.App.Factories;

/// <summary>
/// Factory Method: ізоляція логіки створення сутностей (ПЗ 10).
/// Замість new StaffMember(...) — фабричний метод, що повертає правильний підтип.
/// </summary>
public static class StaffFactory
{
    private static int _nextId = 1;

    /// <summary>
    /// Фабричний метод — повертає правильний підтип залежно від ролі.
    /// Клієнт не залежить від конкретних класів (DIP).
    /// </summary>
    public static StaffMember Create(StaffRole role, string name, decimal salary,
                                     string? specialization = null)
    {
        return role switch
        {
            StaffRole.Waiter   => new Waiter(_nextId++, name, salary),
            StaffRole.Chef     => new Chef(_nextId++, name, salary, specialization ?? "Загальна кухня"),
            StaffRole.Cashier  => new Cashier(_nextId++, name, salary),
            StaffRole.Manager  => new Manager(_nextId++, name, salary),
            _ => throw new ArgumentOutOfRangeException(nameof(role), $"Невідома роль: {role}")
        };
    }
}

/// <summary>Фабрика позицій меню з автоінкрементом ID.</summary>
public static class MenuItemFactory
{
    private static int _nextId = 1;

    public static MenuItem Create(string name, decimal price, string category,
                                  int prepTimeMins = 10)
        => new MenuItem(_nextId++, name, price, category, prepTimeMins);
}

/// <summary>
/// Фабрика стратегій знижок — динамічне створення за рядком конфігурації (СР 10).
/// Приклад: "percent:15", "fixed:50", "happyhour:12:00-14:00:20"
/// </summary>
public static class DiscountStrategyFactory
{
    public static IDiscountStrategy Create(string config)
    {
        if (string.IsNullOrWhiteSpace(config)) return new NoDiscountStrategy();

        var parts = config.Split(':');
        return parts[0].ToLower() switch
        {
            "percent"    => new PercentDiscountStrategy(decimal.Parse(parts[1])),
            "fixed"      => new FixedDiscountStrategy(decimal.Parse(parts[1])),
            "happyhour"  => ParseHappyHour(parts),
            "none"       => new NoDiscountStrategy(),
            _ => throw new ArgumentException($"Невідомий тип знижки: {parts[0]}")
        };
    }

    private static IDiscountStrategy ParseHappyHour(string[] parts)
    {
        // happyhour:12:00-14:00:20  →  parts = ["happyhour","12","00-14","00","20"]
        // Спрощений формат: "happyhour:12-14:20"
        var range = parts[1].Split('-');
        var from = TimeSpan.FromHours(int.Parse(range[0]));
        var to   = TimeSpan.FromHours(int.Parse(range[1]));
        var pct  = parts.Length > 2 ? decimal.Parse(parts[2]) : 20m;
        return new HappyHourDiscountStrategy(from, to, pct);
    }
}

/// <summary>
/// Singleton: єдина чергова система кафе (ПЗ 10).
/// Thread-safe реалізація через Lazy&lt;T&gt;.
/// </summary>
public sealed class CafeQueueManager
{
    private static readonly Lazy<CafeQueueManager> _instance =
        new(() => new CafeQueueManager(), isThreadSafe: true);

    public static CafeQueueManager Instance => _instance.Value;

    private readonly Queue<int> _orderQueue = new();
    private const int MaxCapacity = 20;

    // Приватний конструктор — Singleton
    private CafeQueueManager() { }

    public int QueueLength => _orderQueue.Count;

    public void Enqueue(int orderId)
    {
        if (_orderQueue.Count >= MaxCapacity)
            throw new Domain.Exceptions.KitchenQueueOverflowException(MaxCapacity);
        _orderQueue.Enqueue(orderId);
    }

    public int? Dequeue() =>
        _orderQueue.TryDequeue(out var id) ? id : null;

    public bool Contains(int orderId) => _orderQueue.Contains(orderId);

    public IReadOnlyCollection<int> Peek() => _orderQueue.ToArray();
}
