using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Exceptions;

namespace CafeSystem.Domain.Entities;

/// <summary>
/// Замовлення — центральний агрегат системи кафе.
/// Демонструє інкапсуляцію, конструктори, операції над колекцією (ПЗ 1–2).
/// Управління станом реалізується через IOrderState (патерн State, ПЗ 11).
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();
    private static int _nextId = 1;

    public int Id { get; }
    public int TableNumber { get; }
    public DateTime CreatedAt { get; }
    public DateTime? CompletedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.LineTotal);
    public int EstimatedTimeMinutes => _items.Max(i => (int?)i.MenuItem.PreparationTimeMinutes) ?? 0;

    // Основний конструктор
    public Order(int tableNumber, string? notes = null)
    {
        if (tableNumber <= 0)
            throw new CafeDomainException("Номер столу має бути позитивним числом.");

        Id = _nextId++;
        TableNumber = tableNumber;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.New;
        Notes = notes;
    }

    // Копіювальний конструктор
    public Order(Order other)
    {
        Id = _nextId++;
        TableNumber = other.TableNumber;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.New;
        Notes = other.Notes;
        foreach (var item in other._items)
            _items.Add(new OrderItem(item));
    }

    public void AddItem(MenuItem menuItem, int quantity = 1)
    {
        if (Status != OrderStatus.New && Status != OrderStatus.Accepted)
            throw new InvalidOrderTransitionException(Status.ToString(), "AddItem");

        if (!menuItem.IsAvailable)
            throw new InvalidMenuItemException($"'{menuItem.Name}' недоступна для замовлення.");

        var existing = _items.FirstOrDefault(i => i.MenuItem.Id == menuItem.Id);
        if (existing != null)
            existing.Quantity += quantity;
        else
            _items.Add(new OrderItem(menuItem, quantity));
    }

    public void RemoveItem(int menuItemId)
    {
        if (Status != OrderStatus.New && Status != OrderStatus.Accepted)
            throw new InvalidOrderTransitionException(Status.ToString(), "RemoveItem");
        _items.RemoveAll(i => i.MenuItem.Id == menuItemId);
    }

    public void TransitionTo(OrderStatus newStatus)
    {
        ValidateTransition(Status, newStatus);
        Status = newStatus;
        if (newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled)
            CompletedAt = DateTime.UtcNow;
    }

    private static void ValidateTransition(OrderStatus current, OrderStatus next)
    {
        var allowed = current switch
        {
            OrderStatus.New        => new[] { OrderStatus.Accepted, OrderStatus.Cancelled },
            OrderStatus.Accepted   => new[] { OrderStatus.InProgress, OrderStatus.Cancelled },
            OrderStatus.InProgress => new[] { OrderStatus.Ready },
            OrderStatus.Ready      => new[] { OrderStatus.Delivered },
            OrderStatus.Delivered  => Array.Empty<OrderStatus>(),
            OrderStatus.Cancelled  => Array.Empty<OrderStatus>(),
            _ => Array.Empty<OrderStatus>()
        };

        if (!allowed.Contains(next))
            throw new InvalidOrderTransitionException(current.ToString(), next.ToString());
    }

    public void AddNote(string note) => Notes = note;

    // Operator overloading: merge two orders into one (same table)
    public static Order operator +(Order a, Order b)
    {
        if (a.TableNumber != b.TableNumber)
            throw new CafeDomainException("Не можна об'єднати замовлення з різних столів.");
        var merged = new Order(a.TableNumber, $"Об'єднано #{a.Id}+#{b.Id}");
        foreach (var item in a._items.Concat(b._items))
            merged.AddItem(item.MenuItem, item.Quantity);
        return merged;
    }

    public override string ToString() =>
        $"Замовлення #{Id} | Стіл {TableNumber} | {Status} | {TotalAmount:C2} | {_items.Count} позицій";
}
