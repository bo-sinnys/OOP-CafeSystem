using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Exceptions;
using CafeSystem.Domain.Interfaces;
using CafeSystem.App.Repositories;
using CafeSystem.App.Patterns.Observer;
using CafeSystem.App.Patterns.Strategy;
using CafeSystem.App.Serialization;

namespace CafeSystem.App.Patterns.Facade;

/// <summary>
/// Патерн Facade: єдина точка входу до складних підсистем кафе (ПЗ 12).
/// Клієнтський код взаємодіє лише з цим класом — не знає деталей репозиторіїв, 
/// спостерігачів, черги, серіалізації.
/// </summary>
public class CafeSystemFacade
{
    private readonly OrderRepository _orders;
    private readonly MenuRepository _menu;
    private readonly StaffRepository _staff;
    private readonly OrderEventPublisher _publisher;
    private readonly DiscountContext _discountContext;
    private readonly ILogger _logger;
    private readonly JsonStateSerializer<List<OrderDto>> _serializer;

    public CafeSystemFacade(ILogger logger)
    {
        _logger = logger;
        _orders = new OrderRepository();
        _menu   = new MenuRepository();
        _staff  = new StaffRepository();
        _publisher = new OrderEventPublisher();
        _discountContext = new DiscountContext();
        _serializer = new JsonStateSerializer<List<OrderDto>>();

        // Підключаємо спостерігачів
        _publisher.Subscribe(new KitchenDisplay(logger));
        _publisher.Subscribe(new WaiterNotifier(logger));
        _publisher.Subscribe(new CashierNotifier(logger));
    }

    // ─── Меню ───────────────────────────────────────────────────────────────

    public void AddMenuItem(MenuItem item)
    {
        _menu.Add(item);
        _logger.Log($"Додано позицію меню: {item}");
    }

    public IEnumerable<MenuItem> GetAvailableMenu() => _menu.GetAvailable();
    public MenuItem GetMenuItem(int id) => _menu.GetById(id)
        ?? throw new InvalidMenuItemException($"Позиція {id} не знайдена.");

    // ─── Замовлення ──────────────────────────────────────────────────────────

    public Order CreateOrder(int tableNumber, string? notes = null)
    {
        var order = new Order(tableNumber, notes);
        _orders.Add(order);
        _logger.Log($"Створено {order}");
        return order;
    }

    public void AddItemToOrder(int orderId, int menuItemId, int qty = 1)
    {
        var order = GetOrder(orderId);
        var item  = GetMenuItem(menuItemId);
        order.AddItem(item, qty);
        _logger.Log($"До замовлення #{orderId} додано: {item.Name} x{qty}");
    }

    public void AdvanceOrder(int orderId)
    {
        var order = GetOrder(orderId);
        var prev  = order.Status;
        var next  = order.Status switch
        {
            OrderStatus.New        => OrderStatus.Accepted,
            OrderStatus.Accepted   => OrderStatus.InProgress,
            OrderStatus.InProgress => OrderStatus.Ready,
            OrderStatus.Ready      => OrderStatus.Delivered,
            _ => throw new InvalidOrderTransitionException(order.Status.ToString(), "Next")
        };
        order.TransitionTo(next);
        _publisher.Publish(order, prev);
    }

    public void CancelOrder(int orderId)
    {
        var order = GetOrder(orderId);
        var prev  = order.Status;
        order.TransitionTo(OrderStatus.Cancelled);
        _publisher.Publish(order, prev);
        _logger.LogWarning($"Замовлення #{orderId} скасовано.");
    }

    public decimal GetBill(int orderId)
    {
        var order = GetOrder(orderId);
        var total = _discountContext.Calculate(order.TotalAmount);
        _logger.Log($"Рахунок #{orderId}: {order.TotalAmount:C2} → {total:C2} ({_discountContext.CurrentStrategyName})");
        return total;
    }

    public void SetDiscountStrategy(IDiscountStrategy strategy) =>
        _discountContext.SetStrategy(strategy);

    public IEnumerable<Order> GetActiveOrders() =>
        _orders.GetAll().Where(o => o.Status != OrderStatus.Delivered
                                 && o.Status != OrderStatus.Cancelled);

    // ─── Персонал ────────────────────────────────────────────────────────────

    public void AddStaff(StaffMember member) => _staff.Add(member);

    public void StartShift(int staffId)
    {
        var member = _staff.GetById(staffId)
            ?? throw new CafeDomainException($"Персонал ID={staffId} не знайдено.");
        member.StartShift();
        _logger.Log($"Зміна розпочата: {member}");
    }

    // ─── Статистика (LINQ, ПЗ 7) ─────────────────────────────────────────────

    public Dictionary<string, decimal> GetRevenueByCategory()
    {
        return _orders.GetAll()
            .Where(o => o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItem.Category)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.LineTotal));
    }

    public IEnumerable<(string Name, int Count)> GetTopItems(int top = 5)
    {
        return _orders.GetAll()
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItem.Name)
            .Select(g => (Name: g.Key, Count: g.Sum(i => i.Quantity)))
            .OrderByDescending(x => x.Count)
            .Take(top);
    }

    // ─── Серіалізація (ПЗ 13) ────────────────────────────────────────────────

    public string ExportOrders()
    {
        var dtos = _orders.GetAll().Select(OrderDto.From).ToList();
        return _serializer.Serialize(dtos);
    }

    // ─── Приватні ────────────────────────────────────────────────────────────

    private Order GetOrder(int orderId) =>
        _orders.GetById(orderId) ?? throw new OrderNotFoundException(orderId);
}
