using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Patterns.Decorator;

/// <summary>
/// Патерн Decorator — динамічне розширення функціоналу компонента (ПЗ 12).
/// Декоруємо IDiscountStrategy додатковою поведінкою.
/// </summary>

/// <summary>Базовий декоратор — делегує до обгорнутої стратегії.</summary>
public abstract class DiscountStrategyDecorator : IDiscountStrategy
{
    protected readonly IDiscountStrategy _inner;
    protected DiscountStrategyDecorator(IDiscountStrategy inner) => _inner = inner;

    public virtual string Name => _inner.Name;
    public virtual decimal Apply(decimal amount) => _inner.Apply(amount);
}

/// <summary>Декоратор: логування застосування знижки.</summary>
public class LoggingDiscountDecorator : DiscountStrategyDecorator
{
    private readonly ILogger _logger;

    public LoggingDiscountDecorator(IDiscountStrategy inner, ILogger logger)
        : base(inner) => _logger = logger;

    public override string Name => $"[Логований] {_inner.Name}";

    public override decimal Apply(decimal amount)
    {
        var result = _inner.Apply(amount);
        _logger.Log($"Знижка '{_inner.Name}': {amount:C2} → {result:C2} " +
                    $"(зекономлено {amount - result:C2})");
        return result;
    }
}

/// <summary>Декоратор: мінімальна сума після знижки.</summary>
public class MinimumAmountDecorator : DiscountStrategyDecorator
{
    private readonly decimal _minimum;

    public MinimumAmountDecorator(IDiscountStrategy inner, decimal minimum)
        : base(inner) => _minimum = minimum;

    public override string Name => $"{_inner.Name} (мін. {_minimum:C2})";

    public override decimal Apply(decimal amount)
        => Math.Max(_minimum, _inner.Apply(amount));
}

/// <summary>Декоратор: накопичувальна статистика знижок.</summary>
public class StatisticsDiscountDecorator : DiscountStrategyDecorator
{
    public int TimesApplied { get; private set; }
    public decimal TotalSaved { get; private set; }

    public StatisticsDiscountDecorator(IDiscountStrategy inner) : base(inner) { }

    public override string Name => $"[Статистика] {_inner.Name}";

    public override decimal Apply(decimal amount)
    {
        var result = _inner.Apply(amount);
        TimesApplied++;
        TotalSaved += amount - result;
        return result;
    }

    public string GetStats() =>
        $"Знижку застосовано {TimesApplied} разів, зекономлено разом: {TotalSaved:C2}";
}
