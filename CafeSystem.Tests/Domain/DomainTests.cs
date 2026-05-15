using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Exceptions;
using Xunit;

namespace CafeSystem.Tests.Domain;

/// <summary>
/// Тести доменних сутностей. Структура AAA (Arrange-Act-Assert). ПЗ 14.
/// </summary>
public class MenuItemTests
{
    [Fact]
    public void Constructor_ValidData_CreatesItem()
    {
        // Arrange & Act
        var item = new MenuItem(1, "Еспресо", 45m, "Напої", 3);

        // Assert
        Assert.Equal("Еспресо", item.Name);
        Assert.Equal(45m, item.Price);
        Assert.Equal("Напої", item.Category);
        Assert.True(item.IsAvailable);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Constructor_EmptyName_Throws(string name)
    {
        Assert.Throws<InvalidMenuItemException>(() =>
            new MenuItem(1, name, 45m, "Напої"));
    }

    [Fact]
    public void Constructor_NegativePrice_Throws()
    {
        Assert.Throws<InvalidMenuItemException>(() =>
            new MenuItem(1, "Кава", -1m, "Напої"));
    }

    [Fact]
    public void CopyConstructor_ProducesEquivalentItem()
    {
        var original = new MenuItem(1, "Борщ", 120m, "Перші страви", 15);
        var copy = new MenuItem(original);

        Assert.Equal(original.Name, copy.Name);
        Assert.Equal(original.Price, copy.Price);
        Assert.Equal(original.Category, copy.Category);
    }

    [Fact]
    public void OperatorPlus_AppliesDiscount()
    {
        var item = new MenuItem(1, "Стейк", 280m, "Другі страви");
        var discounted = item + 30m;

        Assert.Equal(250m, discounted.Price);
        Assert.Equal(280m, item.Price); // незмінність оригіналу
    }

    [Fact]
    public void EqualityOperator_SameId_ReturnsTrue()
    {
        var a = new MenuItem(5, "A", 10, "Cat");
        var b = new MenuItem(5, "B", 20, "Cat2");
        Assert.True(a == b);
    }

    [Fact]
    public void EqualityOperator_DifferentId_ReturnsFalse()
    {
        var a = new MenuItem(1, "A", 10, "Cat");
        var b = new MenuItem(2, "A", 10, "Cat");
        Assert.True(a != b);
    }
}

public class OrderTests
{
    private static MenuItem BuildItem(int id = 1) =>
        new MenuItem(id, $"Страва{id}", 100m, "Категорія", 10);

    [Fact]
    public void CreateOrder_ValidTable_SetsStatusNew()
    {
        var order = new Order(3);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(3, order.TableNumber);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void CreateOrder_InvalidTable_Throws()
    {
        Assert.Throws<CafeDomainException>(() => new Order(0));
        Assert.Throws<CafeDomainException>(() => new Order(-1));
    }

    [Fact]
    public void AddItem_NewOrder_AddsItem()
    {
        var order = new Order(1);
        order.AddItem(BuildItem(), 2);
        Assert.Single(order.Items);
        Assert.Equal(2, order.Items[0].Quantity);
    }

    [Fact]
    public void AddItem_SameItem_AccumulatesQuantity()
    {
        var order = new Order(1);
        var item = BuildItem();
        order.AddItem(item, 2);
        order.AddItem(item, 3);
        Assert.Single(order.Items);
        Assert.Equal(5, order.Items[0].Quantity);
    }

    [Fact]
    public void AddItem_UnavailableItem_Throws()
    {
        var order = new Order(1);
        var item = BuildItem();
        item.SetAvailability(false);
        Assert.Throws<InvalidMenuItemException>(() => order.AddItem(item));
    }

    [Fact]
    public void TotalAmount_MultipleItems_CalculatesCorrectly()
    {
        var order = new Order(1);
        order.AddItem(new MenuItem(1, "A", 100m, "Cat"), 2);
        order.AddItem(new MenuItem(2, "B", 50m, "Cat"), 3);
        Assert.Equal(350m, order.TotalAmount); // 200 + 150
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Accepted)]
    [InlineData(OrderStatus.Accepted, OrderStatus.InProgress)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Ready)]
    [InlineData(OrderStatus.Ready, OrderStatus.Delivered)]
    public void TransitionTo_ValidTransitions_Succeeds(OrderStatus from, OrderStatus to)
    {
        var order = new Order(1);
        // Просуваємо до потрібного стану
        AdvanceTo(order, from);
        order.TransitionTo(to);
        Assert.Equal(to, order.Status);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_Throws()
    {
        var order = new Order(1);
        Assert.Throws<InvalidOrderTransitionException>(() =>
            order.TransitionTo(OrderStatus.Delivered));
    }

    [Fact]
    public void CopyConstructor_CopiesItems()
    {
        var original = new Order(2);
        original.AddItem(BuildItem(), 1);
        var copy = new Order(original);
        Assert.Single(copy.Items);
        Assert.NotEqual(original.Id, copy.Id); // Новий ID
    }

    [Fact]
    public void OperatorPlus_SameTable_MergesItems()
    {
        var a = new Order(1);
        a.AddItem(new MenuItem(1, "A", 100m, "Cat"), 1);
        var b = new Order(1);
        b.AddItem(new MenuItem(2, "B", 50m, "Cat"), 2);

        var merged = a + b;
        Assert.Equal(2, merged.Items.Count);
        Assert.Equal(200m, merged.TotalAmount);
    }

    [Fact]
    public void OperatorPlus_DifferentTables_Throws()
    {
        var a = new Order(1);
        var b = new Order(2);
        Assert.Throws<CafeDomainException>(() => { var _ = a + b; });
    }

    // Допоміжний метод: просувати замовлення до потрібного стану
    private static void AdvanceTo(Order order, OrderStatus target)
    {
        var sequence = new[]
        {
            OrderStatus.New,
            OrderStatus.Accepted,
            OrderStatus.InProgress,
            OrderStatus.Ready,
            OrderStatus.Delivered
        };
        int idx = Array.IndexOf(sequence, target);
        for (int i = 0; i < idx; i++)
            order.TransitionTo(sequence[i + 1]);
    }
}

public class StaffTests
{
    [Fact]
    public void Waiter_CalculateBonus_IncludesPerOrderBonus()
    {
        var waiter = new Waiter(1, "Тест", 18_000m);
        var bonus = waiter.CalculateBonus(10);
        // 18000 * 0.05 + 10 * 5 = 900 + 50 = 950
        Assert.Equal(950m, bonus);
    }

    [Fact]
    public void Chef_CalculateBonus_IncludesPerOrderBonus()
    {
        var chef = new Chef(1, "Шеф", 32_000m);
        var bonus = chef.CalculateBonus(5);
        // 32000 * 0.05 + 5 * 10 = 1600 + 50 = 1650
        Assert.Equal(1650m, bonus);
    }

    [Fact]
    public void Waiter_EndShift_ResetsTablesAssigned()
    {
        var waiter = new Waiter(1, "Тест", 18_000m);
        waiter.StartShift();
        waiter.AssignTable();
        waiter.AssignTable();
        waiter.EndShift();
        Assert.Equal(0, waiter.TablesAssigned);
        Assert.False(waiter.IsOnShift);
    }

    [Fact]
    public void StaffMember_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Waiter(1, "", 18_000m));
    }

    [Fact]
    public void PerformDuty_Polymorphic_ReturnsRoleSpecificMessage()
    {
        // Демонструє поліморфізм (ПЗ 3)
        StaffMember waiter  = new Waiter(1, "Олена", 18_000m);
        StaffMember chef    = new Chef(2, "Іван", 32_000m);
        StaffMember cashier = new Cashier(3, "Марія", 15_000m);

        Assert.Contains("обслуговує", waiter.PerformDuty());
        Assert.Contains("готує", chef.PerformDuty());
        Assert.Contains("оплату", cashier.PerformDuty());
    }
}
