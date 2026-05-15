using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Patterns.Strategy;

/// <summary>
/// Патерн Strategy: алгоритм знижки підміняється під час виконання (ПЗ 11).
/// Замість switch/if-else — окрема стратегія на кожен тип знижки.
/// </summary>

public class NoDiscountStrategy : IDiscountStrategy
{
    public string Name => "Без знижки";
    public decimal Apply(decimal amount) => amount;
}

public class PercentDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _percent;

    public PercentDiscountStrategy(decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Відсоток від 0 до 100.");
        _percent = percent;
    }

    public string Name => $"Знижка {_percent}%";
    public decimal Apply(decimal amount) => amount * (1 - _percent / 100);
}

public class FixedDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _amount;

    public FixedDiscountStrategy(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        _amount = amount;
    }

    public string Name => $"Фіксована знижка {_amount:C2}";
    public decimal Apply(decimal amount) => Math.Max(0, amount - _amount);
}

public class HappyHourDiscountStrategy : IDiscountStrategy
{
    private readonly TimeSpan _from;
    private readonly TimeSpan _to;
    private readonly decimal _percent;

    public HappyHourDiscountStrategy(TimeSpan from, TimeSpan to, decimal percent = 20)
    {
        _from = from; _to = to; _percent = percent;
    }

    public string Name => $"Happy Hour ({_from:hh\\:mm}–{_to:hh\\:mm}) {_percent}%";

    public decimal Apply(decimal amount)
    {
        var now = DateTime.Now.TimeOfDay;
        return (now >= _from && now <= _to)
            ? amount * (1 - _percent / 100)
            : amount;
    }
}

/// <summary>Контекст Strategy — зберігає поточну стратегію знижки.</summary>
public class DiscountContext
{
    private IDiscountStrategy _strategy;

    public DiscountContext(IDiscountStrategy? strategy = null)
        => _strategy = strategy ?? new NoDiscountStrategy();

    public void SetStrategy(IDiscountStrategy strategy)
        => _strategy = strategy;

    public decimal Calculate(decimal amount)
    {
        var result = _strategy.Apply(amount);
        return Math.Round(result, 2);
    }

    public string CurrentStrategyName => _strategy.Name;
}
