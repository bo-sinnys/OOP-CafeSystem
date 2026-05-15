namespace CafeSystem.App.Services;

/// <summary>
/// Узагальнені алгоритми на базі делегатів (СР 5):
/// ForEach, Map, Reduce, Filter — без дублювання коду.
/// </summary>
public static class CollectionAlgorithms
{
    /// <summary>Виконати дію для кожного елемента (Action&lt;T&gt;).</summary>
    public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    /// <summary>Перетворення кожного елемента (Func&lt;T, TResult&gt;).</summary>
    public static IEnumerable<TResult> Map<T, TResult>(
        IEnumerable<T> source, Func<T, TResult> selector)
        => source.Select(selector);

    /// <summary>Агрегація (Func&lt;TAcc, T, TAcc&gt;).</summary>
    public static TAcc Reduce<T, TAcc>(
        IEnumerable<T> source, TAcc seed, Func<TAcc, T, TAcc> accumulator)
        => source.Aggregate(seed, accumulator);

    /// <summary>Фільтрація (Predicate&lt;T&gt;).</summary>
    public static IEnumerable<T> Filter<T>(IEnumerable<T> source, Predicate<T> predicate)
        => source.Where(x => predicate(x));

    /// <summary>Сортування з компаратором.</summary>
    public static IEnumerable<T> SortBy<T, TKey>(
        IEnumerable<T> source, Func<T, TKey> keySelector)
        => source.OrderBy(keySelector);
}

/// <summary>
/// Extension Methods для часто вживаної логіки вибірки замовлень (СР 7).
/// </summary>
public static class OrderQueryExtensions
{
    public static IEnumerable<Domain.Entities.Order> WithStatus(
        this IEnumerable<Domain.Entities.Order> orders,
        Domain.Enums.OrderStatus status)
        => orders.Where(o => o.Status == status);

    public static IEnumerable<Domain.Entities.Order> ForTable(
        this IEnumerable<Domain.Entities.Order> orders, int table)
        => orders.Where(o => o.TableNumber == table);

    public static decimal TotalRevenue(this IEnumerable<Domain.Entities.Order> orders)
        => orders.Sum(o => o.TotalAmount);

    public static IEnumerable<Domain.Entities.Order> CreatedToday(
        this IEnumerable<Domain.Entities.Order> orders)
        => orders.Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date);
}
