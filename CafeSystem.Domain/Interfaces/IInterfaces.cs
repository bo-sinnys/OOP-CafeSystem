using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;

namespace CafeSystem.Domain.Interfaces;

// ─── Репозиторії ────────────────────────────────────────────────────────────

/// <summary>Контракт базового CRUD-сховища (Generics, ПЗ 5).</summary>
public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

public interface IOrderRepository : IRepository<Order>
{
    IEnumerable<Order> GetByStatus(OrderStatus status);
    IEnumerable<Order> GetByTable(int tableNumber);
    IEnumerable<Order> GetByDateRange(DateTime from, DateTime to);
}

public interface IMenuRepository : IRepository<MenuItem>
{
    IEnumerable<MenuItem> GetByCategory(string category);
    IEnumerable<MenuItem> GetAvailable();
}

// ─── Сервіси ────────────────────────────────────────────────────────────────

/// <summary>Контракт сервісу обробки замовлень.</summary>
public interface IOrderService
{
    Order CreateOrder(int tableNumber, string? notes = null);
    void AddItemToOrder(int orderId, int menuItemId, int quantity);
    void AdvanceOrderStatus(int orderId);
    void CancelOrder(int orderId);
    IEnumerable<Order> GetActiveOrders();
    decimal CalculateBill(int orderId);
}

/// <summary>Контракт сервісу персоналу.</summary>
public interface IStaffService
{
    void StartShift(int staffId);
    void EndShift(int staffId);
    IEnumerable<StaffMember> GetOnShiftStaff();
    decimal CalculateBonus(int staffId, int ordersServed);
}

/// <summary>Контракт логера — зовнішня залежність (DIP, СР 9).</summary>
public interface ILogger
{
    void Log(string message);
    void LogError(string message, Exception? ex = null);
    void LogWarning(string message);
}

/// <summary>Контракт серіалізатора стану (ПЗ 13).</summary>
public interface IStateSerializer<T>
{
    string Serialize(T obj);
    T? Deserialize(string data);
}

/// <summary>Контракт стратегії знижок (Strategy, ПЗ 11).</summary>
public interface IDiscountStrategy
{
    string Name { get; }
    decimal Apply(decimal originalAmount);
}

/// <summary>Контракт сповіщення (Observer, ПЗ 11).</summary>
public interface IOrderObserver
{
    void OnOrderStatusChanged(Order order, OrderStatus previousStatus);
}

/// <summary>Контракт компонента (Composite, СР 12).</summary>
public interface IMenuComponent
{
    string Name { get; }
    decimal GetTotalPrice();
    void Display(int indent = 0);
}
