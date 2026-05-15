using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Repositories;

public class OrderRepository : InMemoryRepository<Order>, IOrderRepository
{
    protected override int GetId(Order entity) => entity.Id;

    public IEnumerable<Order> GetByStatus(OrderStatus status) =>
        _store.Values.Where(o => o.Status == status);

    public IEnumerable<Order> GetByTable(int tableNumber) =>
        _store.Values.Where(o => o.TableNumber == tableNumber);

    public IEnumerable<Order> GetByDateRange(DateTime from, DateTime to) =>
        _store.Values.Where(o => o.CreatedAt >= from && o.CreatedAt <= to);
}

public class MenuRepository : InMemoryRepository<MenuItem>, IMenuRepository
{
    protected override int GetId(MenuItem entity) => entity.Id;

    public IEnumerable<MenuItem> GetByCategory(string category) =>
        _store.Values.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<MenuItem> GetAvailable() =>
        _store.Values.Where(i => i.IsAvailable);
}

public class StaffRepository : InMemoryRepository<StaffMember>
{
    protected override int GetId(StaffMember entity) => entity.Id;

    public IEnumerable<StaffMember> GetOnShift() =>
        _store.Values.Where(s => s.IsOnShift);
}
