using CafeSystem.App.Factories;
using CafeSystem.App.Patterns.Composite;
using CafeSystem.App.Patterns.Decorator;
using CafeSystem.App.Patterns.Strategy;
using CafeSystem.App.Repositories;
using CafeSystem.App.Serialization;
using CafeSystem.App.Services;
using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Interfaces;
using Moq;
using Xunit;

namespace CafeSystem.Tests.Services;

// ─── Strategy ────────────────────────────────────────────────────────────────

public class DiscountStrategyTests
{
    [Fact]
    public void NoDiscount_ReturnsOriginalAmount()
    {
        var strategy = new NoDiscountStrategy();
        Assert.Equal(100m, strategy.Apply(100m));
    }

    [Theory]
    [InlineData(10, 100, 90)]
    [InlineData(25, 200, 150)]
    [InlineData(100, 500, 0)]
    public void PercentDiscount_CalculatesCorrectly(decimal pct, decimal amount, decimal expected)
    {
        var strategy = new PercentDiscountStrategy(pct);
        Assert.Equal(expected, strategy.Apply(amount));
    }

    [Fact]
    public void PercentDiscount_InvalidPercent_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PercentDiscountStrategy(101));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PercentDiscountStrategy(-1));
    }

    [Fact]
    public void FixedDiscount_DoesNotGoNegative()
    {
        var strategy = new FixedDiscountStrategy(200m);
        Assert.Equal(0m, strategy.Apply(100m));
    }

    [Fact]
    public void DiscountContext_SwitchesStrategyAtRuntime()
    {
        // Демонструє підміну алгоритму під час виконання (патерн Strategy)
        var ctx = new DiscountContext(new NoDiscountStrategy());
        Assert.Equal(100m, ctx.Calculate(100m));

        ctx.SetStrategy(new PercentDiscountStrategy(20));
        Assert.Equal(80m, ctx.Calculate(100m));
    }

    [Fact]
    public void DiscountStrategyFactory_CreatesFromConfig()
    {
        var strategy = DiscountStrategyFactory.Create("percent:15");
        Assert.IsType<PercentDiscountStrategy>(strategy);
        Assert.Equal(85m, strategy.Apply(100m));
    }
}

// ─── Decorator ────────────────────────────────────────────────────────────────

public class DiscountDecoratorTests
{
    [Fact]
    public void LoggingDecorator_LogsAndReturnsCorrectValue()
    {
        // Arrange — Moq для ізоляції (СР 14)
        var mockLogger = new Mock<ILogger>();
        var inner = new PercentDiscountStrategy(10);
        var decorated = new LoggingDiscountDecorator(inner, mockLogger.Object);

        // Act
        var result = decorated.Apply(200m);

        // Assert
        Assert.Equal(180m, result);
        mockLogger.Verify(l => l.Log(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void StatisticsDecorator_TracksUsageAndSavings()
    {
        var inner = new FixedDiscountStrategy(50m);
        var stats = new StatisticsDiscountDecorator(inner);

        stats.Apply(200m); // saved 50
        stats.Apply(300m); // saved 50

        Assert.Equal(2, stats.TimesApplied);
        Assert.Equal(100m, stats.TotalSaved);
    }

    [Fact]
    public void MinimumAmountDecorator_EnforcesFloor()
    {
        var inner = new FixedDiscountStrategy(1000m); // агресивна знижка
        var decorated = new MinimumAmountDecorator(inner, minimum: 50m);
        Assert.Equal(50m, decorated.Apply(100m));
    }
}

// ─── Composite ───────────────────────────────────────────────────────────────

public class MenuCompositeTests
{
    [Fact]
    public void MenuGroup_GetTotalPrice_SumsChildren()
    {
        var group = new MenuGroup("Напої");
        group.Add(new MenuLeaf("Еспресо", 45m));
        group.Add(new MenuLeaf("Сік", 55m));
        Assert.Equal(100m, group.GetTotalPrice());
    }

    [Fact]
    public void ComboMeal_AppliesDiscount()
    {
        var combo = new ComboMeal("Ланч", discountPercent: 10);
        combo.AddItem(new MenuLeaf("Борщ", 120m));
        combo.AddItem(new MenuLeaf("Чай", 30m));
        // (120+30) * 0.9 = 135
        Assert.Equal(135m, combo.GetTotalPrice());
    }

    [Fact]
    public void NestedGroups_GetTotalPrice_IsRecursive()
    {
        var root = new MenuGroup("Меню");
        var sub = new MenuGroup("Підменю");
        sub.Add(new MenuLeaf("A", 100m));
        sub.Add(new MenuLeaf("B", 200m));
        root.Add(sub);
        root.Add(new MenuLeaf("C", 50m));
        Assert.Equal(350m, root.GetTotalPrice());
    }
}

// ─── Repository (Generics) ───────────────────────────────────────────────────

public class RepositoryTests
{
    private static MenuItem MakeItem(int id) =>
        new MenuItem(id, $"Item{id}", 10m * id, "Cat");

    [Fact]
    public void MenuRepository_AddAndGetById_Works()
    {
        var repo = new MenuRepository();
        var item = MakeItem(1);
        repo.Add(item);
        var found = repo.GetById(1);
        Assert.Equal(item, found);
    }

    [Fact]
    public void MenuRepository_GetAvailable_FiltersUnavailable()
    {
        var repo = new MenuRepository();
        var available = MakeItem(1);
        var unavailable = MakeItem(2);
        unavailable.SetAvailability(false);

        repo.Add(available);
        repo.Add(unavailable);

        Assert.Single(repo.GetAvailable());
    }

    [Fact]
    public void MenuRepository_GetByCategory_FiltersCorrectly()
    {
        var repo = new MenuRepository();
        repo.Add(new MenuItem(1, "Еспресо", 45, "Напої"));
        repo.Add(new MenuItem(2, "Борщ", 120, "Перші страви"));
        repo.Add(new MenuItem(3, "Капучино", 65, "Напої"));

        var drinks = repo.GetByCategory("Напої").ToList();
        Assert.Equal(2, drinks.Count);
    }

    [Fact]
    public void Repository_DuplicateAdd_Throws()
    {
        var repo = new MenuRepository();
        var item = MakeItem(1);
        repo.Add(item);
        Assert.Throws<InvalidOperationException>(() => repo.Add(item));
    }
}

// ─── Serialization ───────────────────────────────────────────────────────────

public class SerializationTests
{
    [Fact]
    public void JsonSerializer_RoundTrip_PreservesData()
    {
        var serializer = new JsonStateSerializer<OrderDto>();
        var dto = new OrderDto
        {
            Id          = 1,
            TableNumber = 3,
            Status      = "Delivered",
            CreatedAt   = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350m,
            Items       = new List<OrderItemDto>
            {
                new() { MenuItemName = "Борщ", Quantity = 1, UnitPrice = 120m, LineTotal = 120m }
            }
        };

        var json = serializer.Serialize(dto);
        var restored = serializer.Deserialize(json);

        Assert.NotNull(restored);
        Assert.Equal(dto.Id, restored!.Id);
        Assert.Equal(dto.TotalAmount, restored.TotalAmount);
        Assert.Single(restored.Items);
    }

    [Fact]
    public void OrderDto_From_MapsCorrectly()
    {
        var order = new Order(3);
        var item = new MenuItem(1, "Тест", 100m, "Cat");
        order.AddItem(item, 2);

        var dto = OrderDto.From(order);

        Assert.Equal(3, dto.TableNumber);
        Assert.Equal(200m, dto.TotalAmount);
        Assert.Single(dto.Items);
        Assert.Equal("Тест", dto.Items[0].MenuItemName);
    }
}

// ─── Collection algorithms (СР 5) ────────────────────────────────────────────

public class CollectionAlgorithmsTests
{
    [Fact]
    public void Map_TransformsElements()
    {
        var input = new[] { 1, 2, 3 };
        var result = CollectionAlgorithms.Map(input, x => x * 2).ToList();
        Assert.Equal(new[] { 2, 4, 6 }, result);
    }

    [Fact]
    public void Reduce_SumsElements()
    {
        var input = new[] { 1, 2, 3, 4, 5 };
        var sum = CollectionAlgorithms.Reduce(input, 0, (acc, x) => acc + x);
        Assert.Equal(15, sum);
    }

    [Fact]
    public void Filter_RemovesNonMatching()
    {
        var input = new[] { 1, 2, 3, 4, 5, 6 };
        var evens = CollectionAlgorithms.Filter(input, x => x % 2 == 0).ToList();
        Assert.Equal(new[] { 2, 4, 6 }, evens);
    }
}

// ─── Mock-тести з Moq (СР 14) ────────────────────────────────────────────────

public class LoggerMockTests
{
    [Fact]
    public void RetryPolicy_LogsEachAttempt()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retry = new RetryPolicy(mockLogger.Object, maxAttempts: 2,
                                    TimeSpan.FromMilliseconds(1));
        int call = 0;

        // Act
        var result = retry.Execute(() =>
        {
            call++;
            if (call < 2) throw new IOException("fail");
            return "ok";
        }, "TestOp");

        // Assert
        Assert.Equal("ok", result);
        // Очікуємо 2 виклики Log (по одному на спробу)
        mockLogger.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeast(2));
    }

    [Fact]
    public void RetryPolicy_AllAttemptsFail_ThrowsAggregateException()
    {
        var mockLogger = new Mock<ILogger>();
        var retry = new RetryPolicy(mockLogger.Object, maxAttempts: 2,
                                    TimeSpan.FromMilliseconds(1));

        Assert.Throws<AggregateException>(() =>
            retry.Execute<int>(() => throw new IOException("fail"), "AlwaysFails"));

        mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()),
                          Times.Once);
    }
}
