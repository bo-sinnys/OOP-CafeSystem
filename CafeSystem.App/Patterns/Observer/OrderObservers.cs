using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Patterns.Observer;

/// <summary>
/// Патерн Observer: компоненти реагують на зміну статусу замовлення (ПЗ 11).
/// Також демонструє event/EventHandler (СР 11) — без memory leak завдяки явному відписуванню.
/// </summary>

// ─── EventArgs ──────────────────────────────────────────────────────────────

public class OrderStatusChangedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderStatus PreviousStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEventArgs(Order order, OrderStatus previous, OrderStatus newStatus)
    {
        Order = order;
        PreviousStatus = previous;
        NewStatus = newStatus;
    }
}

// ─── Publisher ──────────────────────────────────────────────────────────────

public class OrderEventPublisher
{
    // Стандартний .NET event — підписники можуть відключатися без memory leak (СР 11)
    public event EventHandler<OrderStatusChangedEventArgs>? OrderStatusChanged;

    private readonly List<IOrderObserver> _observers = new();

    public void Subscribe(IOrderObserver observer) => _observers.Add(observer);

    public void Unsubscribe(IOrderObserver observer) => _observers.Remove(observer);

    public void Publish(Order order, OrderStatus previousStatus)
    {
        var args = new OrderStatusChangedEventArgs(order, previousStatus, order.Status);

        // Інтерфейсні спостерігачі
        foreach (var obs in _observers.ToList())
            obs.OnOrderStatusChanged(order, previousStatus);

        // .NET event
        OrderStatusChanged?.Invoke(this, args);
    }
}

// ─── Конкретні Observer-и ────────────────────────────────────────────────────

public class KitchenDisplay : IOrderObserver
{
    private readonly ILogger _logger;
    public KitchenDisplay(ILogger logger) => _logger = logger;

    public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
    {
        if (order.Status == OrderStatus.Accepted)
            _logger.Log($"[КУХНЯ] Нове замовлення #{order.Id} — стіл {order.TableNumber}. " +
                        $"Позицій: {order.Items.Count}");

        if (order.Status == OrderStatus.InProgress)
            _logger.Log($"[КУХНЯ] Готування #{order.Id} розпочато.");
    }
}

public class WaiterNotifier : IOrderObserver
{
    private readonly ILogger _logger;
    public WaiterNotifier(ILogger logger) => _logger = logger;

    public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
    {
        if (order.Status == OrderStatus.Ready)
            _logger.Log($"[ОФІЦІАНТ] Замовлення #{order.Id} готове! " +
                        $"Несіть на стіл {order.TableNumber}.");
    }
}

public class CashierNotifier : IOrderObserver
{
    private readonly ILogger _logger;
    public CashierNotifier(ILogger logger) => _logger = logger;

    public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
    {
        if (order.Status == OrderStatus.Delivered)
            _logger.Log($"[КАСА] Замовлення #{order.Id} доставлено. " +
                        $"До оплати: {order.TotalAmount:C2}");

        if (order.Status == OrderStatus.Cancelled)
            _logger.LogWarning($"[КАСА] Замовлення #{order.Id} скасовано.");
    }
}
